using Content.Server.Popups;
using Content.Server.RemoteVehicle.Modules.Components;
using Content.Server.Stealth;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.PowerCell;
using Content.Shared.RemoteVehicle.Components;
using Content.Shared.RemoteVehicle.Systems;
using Content.Shared.Stealth.Components;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.RemoteVehicle.Modules.Systems
{
    public sealed class RemoteVehicleContainerModuleSystem : EntitySystem
    {
        [Dependency] private readonly ContainerSystem _container = default!;
        [Dependency] private readonly PopupSystem _popup = default!;

        [Dependency] private readonly AudioSystem _audio = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RemoteVehicleContainerModuleComponent, RemoteVehicleModuleInsertedEvent>(OnModuleInserted);
            SubscribeLocalEvent<RemoteVehicleContainerModuleComponent, RemoteVehicleModuleUseEvent>(OnModuleUse);

            SubscribeLocalEvent<RemoteVehicleContainerModuleComponent, InteractUsingEvent>(OnInsertAttempt);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
        }

        private void OnInsertAttempt(EntityUid uid, RemoteVehicleContainerModuleComponent component, InteractUsingEvent args)
        {
            if (!EntityManager.TryGetComponent<ItemComponent>(args.Used, out var item))
                return;

            BaseContainer container = _container.GetContainer(uid, "items_container");

            if (container.ContainedEntities.Count >= component.ContainerCapacity)
            {
                _popup.PopupEntity(Loc.GetString("remote-vehicle-module-container-limit-popup"), uid);
                return;
            }

            _audio.PlayPvs(component.InsertSound, uid);
            _container.Insert(args.Used, container);
        }

        private void OnModuleInserted(EntityUid uid, RemoteVehicleContainerModuleComponent component, RemoteVehicleModuleInsertedEvent args)
        {
            component.VehicleUid = args.VehicleUid;

            UpdateModuleStatus(uid, component);
        }

        private void OnModuleUse(EntityUid uid, RemoteVehicleContainerModuleComponent component, RemoteVehicleModuleUseEvent args)
        {
            InjectItem(uid, component);
        }

        private void InjectItem(EntityUid uid, RemoteVehicleContainerModuleComponent component)
        {
            if (component.VehicleUid == null)
                return;

            var moduleComp = Comp<RemoteVehicleModuleComponent>(uid);
            if (!moduleComp.UseAvalible || !moduleComp.Enabled)
                return;

            _audio.PlayPvs(component.EjectSound, uid);

            BaseContainer container = _container.GetContainer(uid, "items_container");
            _container.RemoveEntity(uid, container.ContainedEntities.Last(), destination: Transform(component.VehicleUid.Value).Coordinates, force: true, reparent: true);

            UpdateModuleStatus(uid, component);
        }

        private void UpdateModuleStatus(EntityUid uid, RemoteVehicleContainerModuleComponent component)
        {
            BaseContainer container = _container.GetContainer(uid, "items_container");

            var moduleComp = Comp<RemoteVehicleModuleComponent>(uid);
            moduleComp.ModuleStatus = Loc.GetString("remote-vehicle-module-container-status",
                                      ("capacity", component.ContainerCapacity), ("current", container.ContainedEntities.Count));

            moduleComp.UseAvalible = container.ContainedEntities.Count != 0;

            Dirty(moduleComp);
        }
    }
}

