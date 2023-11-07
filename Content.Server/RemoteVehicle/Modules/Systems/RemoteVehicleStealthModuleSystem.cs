using Content.Server.RemoteVehicle.Modules.Components;
using Content.Server.Stealth;
using Content.Shared.PowerCell;
using Content.Shared.RemoteVehicle.Components;
using Content.Shared.RemoteVehicle.Systems;
using Content.Shared.Stealth.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.RemoteVehicle.Modules.Systems
{
    public sealed class RemoteVehicleStealthModuleSystem : EntitySystem
    {
        [Dependency] private readonly StealthSystem _stealth = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RemoteVehicleStealthModuleComponent, RemoteVehicleModuleInsertedEvent>(OnModuleInserted);
            SubscribeLocalEvent<RemoteVehicleStealthModuleComponent, RemoteVehicleModuleRemovedEvent>(OnModuleRemoved);

            SubscribeLocalEvent<RemoteVehicleStealthModuleComponent, RemoteVehicleModuleStateChangedEvent>(OnModuleStateChanged);
            SubscribeLocalEvent<RemoteVehicleStealthModuleComponent, RemoteVehicleModuleUseEvent>(OnModuleUse);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var modulesQuery = AllEntityQuery<RemoteVehicleStealthModuleComponent>();
            while (modulesQuery.MoveNext(out var uid, out var comp))
            {
                if (comp.VehicleUid != null)
                {
                    _stealth.ModifyVisibility(comp.VehicleUid.Value, comp.CurrentTransparentDelta * frameTime);
                }
            }
        }

        private void OnModuleInserted(EntityUid uid, RemoteVehicleStealthModuleComponent component, RemoteVehicleModuleInsertedEvent args)
        {
            EnsureComp<StealthComponent>(args.VehicleUid);
            component.VehicleUid = args.VehicleUid;

            SetVisible(uid, component, component.InvisibleEnabled);
        }

        private void OnModuleRemoved(EntityUid uid, RemoteVehicleStealthModuleComponent component, RemoteVehicleModuleRemovedEvent args)
        {
            SetVisible(uid, component, false);

            component.VehicleUid = null;
            RemComp<StealthComponent>(args.VehicleUid);
        }

        private void OnModuleStateChanged(EntityUid uid, RemoteVehicleStealthModuleComponent component, RemoteVehicleModuleStateChangedEvent args)
        {
            if (component.VehicleUid == null)
                return;

            if (!args.Enabled)
                SetVisible(uid, component, false);
        }

        private void OnModuleUse(EntityUid uid, RemoteVehicleStealthModuleComponent component, RemoteVehicleModuleUseEvent args)
        {
            SetVisible(uid, component, !component.InvisibleEnabled);
        }

        private void SetVisible(EntityUid uid, RemoteVehicleStealthModuleComponent component, bool enabled)
        {
            if (component.VehicleUid == null)
                return;

            component.InvisibleEnabled = enabled;
            component.CurrentTransparentDelta = component.TransparentDelta * (enabled ? -1f : 1f);

            if (TryComp<PowerCellDrawComponent>(component.VehicleUid, out var powerCellDraw))
            {
                if (!component.DrawRateApplied && enabled)
                {
                    powerCellDraw.DrawRate += component.DrawRateModifier;
                    component.DrawRateApplied = true;
                }
                if (component.DrawRateApplied && !enabled)
                {
                    powerCellDraw.DrawRate -= component.DrawRateModifier;
                    component.DrawRateApplied = false;
                }
            }

            var moduleComp = Comp<RemoteVehicleModuleComponent>(uid);
            moduleComp.ModuleStatus = Loc.GetString(enabled ? "remote-vehicle-module-stealth-enabled" : "remote-vehicle-module-stealth-disabled");
            Dirty(moduleComp);
        }
    }
}
