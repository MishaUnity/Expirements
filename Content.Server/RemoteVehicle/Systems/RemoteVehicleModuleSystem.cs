using Content.Shared.RemoteVehicle.Systems;
using Robust.Server.GameObjects;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Shared.RemoteVehicle.Components;
using Robust.Server.Containers;
using Content.Shared.Containers.ItemSlots;

namespace Content.Server.RemoteVehicle.Systems
{
    public sealed class RemoteVehicleModuleSystem : SharedRemoteVehicleModuleSystem
    {
        [Dependency] private readonly ContainerSystem _containerSystem = default!;
        [Dependency] private readonly AudioSystem _audioSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RemoteVehicleModuleContainerComponent, RemoteVehiclePryFinished>(OnPryFinished);
        }

        public bool TryUseModule(EntityUid moduleUid, EntityUid? user, EntityUid controllerUid, EntityUid vehicleUid)
        {
            if (!TryComp<RemoteVehicleModuleComponent>(moduleUid, out var moduleComp) || !moduleComp.UseAvalible || !moduleComp.Enabled)
                return false;

            RaiseLocalEvent(moduleUid, new RemoteVehicleModuleUseEvent(user, vehicleUid, controllerUid));
            return true;
        }

        public void SetModuleState(EntityUid moduleUid, EntityUid vehicleUid, bool enabled)
        {
            if (!TryComp<RemoteVehicleModuleComponent>(moduleUid, out var comp))
                return;

            comp.Enabled = enabled;
            Dirty(comp);

            RaiseLocalEvent(moduleUid, new RemoteVehicleModuleStateChangedEvent(vehicleUid, enabled));
        }

        public void SetVehicleModulesState(RemoteVehicleComponent component, EntityUid uid, bool enabled)
        {
            if (component.InsertedModules == null)
                return;

            foreach (var module in component.InsertedModules)
            {
                SetModuleState(module.Owner, uid, enabled);
            }
        }

        private void OnPryFinished(EntityUid uid, RemoteVehicleModuleContainerComponent component, RemoteVehiclePryFinished args)
        {
            if (args.Cancelled)
                return;

            EjectModules(component, uid);
            _audioSystem.PlayPvs(component.EjectSound, uid);
        }

        public void EjectModules(RemoteVehicleModuleContainerComponent component, EntityUid uid)
        {
            var container = _containerSystem.GetContainer(uid, component.ContainerId);
            var modules = container.ContainedEntities.ToArray();

            foreach (var module in modules)
            {
                container.Remove(module);
                RaiseLocalEvent(module, new RemoteVehicleModuleRemovedEvent(uid));
            }

            UpdateVehicleModules(component, uid);
        }
    }
}
