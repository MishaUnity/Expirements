using Content.Server.Physics.Controllers;
using Content.Shared.RemoteVehicle.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.DeviceLinking;
using Content.Shared.Movement.Systems;
using Robust.Shared.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Shared.Movement.Components;
using System.Numerics;
using Robust.Shared.Timing;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Content.Shared.RemoteVehicle.Systems;
using JetBrains.Annotations;
using Content.Server.PowerCell;
using Robust.Shared.Containers;
using Content.Shared.Light;
using Content.Shared.PowerCell;
using Content.Server.Pinpointer;
using Content.Shared.PowerCell.Components;
using Robust.Server.GameObjects;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Content.Shared.Interaction;
using Content.Server.Tools;
using Content.Shared.Containers.ItemSlots;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Content.Shared.Examine;
using Content.Server.Audio;

namespace Content.Server.RemoteVehicle.Systems
{
    [UsedImplicitly]
    public sealed class RemoteVehicleSystem : SharedRemoteVehicleSystem
    {
        [Dependency] private readonly MoverController _moverController = default!;
        [Dependency] private readonly PowerCellSystem _powerCell = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
        [Dependency] private readonly ToolSystem _toolSystem = default!;
        [Dependency] private readonly AppearanceSystem _appearance = default!;
        [Dependency] private readonly AmbientSoundSystem _ambientSound = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        [Dependency] private readonly RemoteVehicleControllerSystem _controllerSystem = default!;
        [Dependency] private readonly RemoteVehicleModuleSystem _moduleSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RemoteVehicleComponent, ComponentShutdown>(OnComponentShutdown);

            SubscribeLocalEvent<RemoteVehicleComponent, PowerCellSlotEmptyEvent>(OnPowerCellEmpty);
            SubscribeLocalEvent<RemoteVehicleComponent, PowerCellChangedEvent>(OnPowerCellChanged);

            SubscribeLocalEvent<RemoteVehicleComponent, RemoteVehicleModulesChangedEvent>(OnModulesChanged);

            SubscribeLocalEvent<RemoteVehicleComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<RemoteVehicleComponent, RemoteVehiclePryFinished>(OnPryFinished);
            SubscribeLocalEvent<RemoteVehicleComponent, RemoteVehicleScrewFinished>(OnScrewFinished);

            SubscribeLocalEvent<RemoteVehicleComponent, ExaminedEvent>(OnExamine);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var vehiclesQuery = AllEntityQuery<RemoteVehicleComponent>();
            while (vehiclesQuery.MoveNext(out var uid, out var comp))
            {
                comp.BatteryCharge = GetBatteryCharge(uid);
                comp.SignalLevel = GetSignalLevel(comp, uid);
                Dirty(comp);

                if (comp.Enabled)
                {
                    if (!HasSignal(comp, uid))
                        TryDisable(comp, uid);
                }
            }
        }

        public void TrySetInput(EntityUid uid, Vector2 dir)
        {
            if (!TryComp<InputMoverComponent>(uid, out var mover))
                return;

            if (!TryComp<RemoteVehicleComponent>(uid, out var vehicle) || !vehicle.Enabled)
                return;

            mover.CurTickWalkMovement = dir;
            mover.CurTickSprintMovement = dir;
            mover.LastInputTick = _timing.CurTick;
            mover.LastInputSubTick = 0;
        }

        public void HandleSetInput(EntityUid uid, Vector2 dir)
        {
            if (!TryComp<InputMoverComponent>(uid, out var mover))
                return;

            mover.CurTickWalkMovement = dir;
            mover.CurTickSprintMovement = dir;
            mover.LastInputTick = _timing.CurTick;
            mover.LastInputSubTick = 0;
        }

        public bool TryEnable(RemoteVehicleComponent component, EntityUid uid)
        {
            TryComp<PowerCellDrawComponent>(uid, out var draw);
            if (!HasSignal(component, uid) || !_powerCell.HasActivatableCharge(uid, battery: draw))
                return false;

            _powerCell.SetPowerCellDrawEnabled(uid, true, draw);
            _moduleSystem.SetVehicleModulesState(component, uid, true);

            if (component.ConnectedController != null)
                _controllerSystem.UpdateUi(component.ConnectedController.Owner, component.ConnectedController);

            component.Enabled = true;

            _appearance.SetData(uid, RemoteVehicleVisuals.Enabled, true);
            _ambientSound.SetAmbience(uid, true);

            Dirty(component);
            return true;
        }

        public bool TryDisable(RemoteVehicleComponent component, EntityUid uid)
        {
            HandleSetInput(uid, Vector2.Zero);

            _powerCell.SetPowerCellDrawEnabled(uid, false);
            _moduleSystem.SetVehicleModulesState(component, uid, false);

            if (component.ConnectedController != null)
                _controllerSystem.UpdateUi(component.ConnectedController.Owner, component.ConnectedController);

            component.Enabled = false;

            _appearance.SetData(uid, RemoteVehicleVisuals.Enabled, false);
            _ambientSound.SetAmbience(uid, false);

            Dirty(component);
            return true;
        }

        public void SetAssembleState(RemoteVehicleComponent component, EntityUid uid, bool assembled)
        {
            component.IsAssembled = assembled;
            _appearance.SetData(uid, RemoteVehicleVisuals.Assembled, assembled);

            if (TryComp<PowerCellSlotComponent>(uid, out var cellSlot))
                _itemSlots.SetLock(uid, cellSlot.CellSlotId, assembled);
        }

        private void OnComponentShutdown(EntityUid uid, RemoteVehicleComponent component, ComponentShutdown args)
        {
            if (component.ConnectedController != null)
                _controllerSystem.DisconnectVehicle(component.ConnectedController);
        }

        private void OnInteractUsing(EntityUid uid, RemoteVehicleComponent component, InteractUsingEvent args)
        {
            if (component.InsertedModules != null && component.InsertedModules.Any() && !component.IsAssembled)
                args.Handled = _toolSystem.UseTool(args.Used, args.User, uid, RemoteVehicleComponent.PryDelay, "Prying", new RemoteVehiclePryFinished());

            args.Handled = _toolSystem.UseTool(args.Used, args.User, uid, RemoteVehicleComponent.ScrewDelay, "Screwing", new RemoteVehicleScrewFinished());
        }

        private void OnPryFinished(EntityUid uid, RemoteVehicleComponent component, RemoteVehiclePryFinished args)
        {

        }

        private void OnScrewFinished(EntityUid uid, RemoteVehicleComponent component, RemoteVehicleScrewFinished args)
        {
            if (args.Cancelled)
                return;

            SetAssembleState(component, uid, !component.IsAssembled);
        }

        private void OnModulesChanged(EntityUid uid, RemoteVehicleComponent component, RemoteVehicleModulesChangedEvent args)
        {
            if (component.ConnectedController != null)
                _controllerSystem.UpdateUi(component.ConnectedController.Owner, component.ConnectedController);
        }

        private void OnPowerCellEmpty(EntityUid uid, RemoteVehicleComponent component, ref PowerCellSlotEmptyEvent args)
        {
            component.BatteryCharge = 0f;
            TryDisable(component, uid);
        }

        private void OnPowerCellChanged(EntityUid uid, RemoteVehicleComponent component, PowerCellChangedEvent args)
        {
            component.BatteryCharge = GetBatteryCharge(uid);
            Dirty(component);
        }

        private void OnExamine(EntityUid uid, RemoteVehicleComponent component, ExaminedEvent args)
        {
            var assembledText = Loc.GetString(component.IsAssembled ? "remote-vehicle-markup-assembled" : "remote-vehicle-markup-not-assembled");
            args.PushMarkup(assembledText);

            if (!component.IsAssembled)
            {
                if (!_powerCell.TryGetBatteryFromSlot(uid, out _))
                    args.PushMarkup(Loc.GetString("remote-vehicle-markup-no-battery"));
            }
        }

        public float GetBatteryCharge(EntityUid uid)
        {
            if (!_powerCell.TryGetBatteryFromSlot(uid, out var battery))
                return 0f;

            return battery.CurrentCharge / battery.MaxCharge;
        }

        public bool HasSignal(RemoteVehicleComponent component, EntityUid uid)
        {
            if (component.ConnectedController == null)
                return false;

            var vehicleTran = Transform(uid);
            var controllerTran = Transform(component.ConnectedController.Owner);

            float distance = Vector2.Distance(vehicleTran.MapPosition.Position, controllerTran.MapPosition.Position);
            if (distance < component.SignalRadiuse)
                return true;

            return false;
        }

        public float GetSignalLevel(RemoteVehicleComponent component, EntityUid uid)
        {
            if (component.ConnectedController == null)
                return 1f;

            var vehicleTran = Transform(uid);
            var controllerTran = Transform(component.ConnectedController.Owner);

            float distance = Vector2.Distance(vehicleTran.MapPosition.Position, controllerTran.MapPosition.Position);
            if (distance < component.SignalRadiuse)
                return distance / component.SignalRadiuse;

            return 1f;
        }
    }
}
