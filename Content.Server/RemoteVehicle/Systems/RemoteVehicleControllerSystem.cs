using Content.Server.Physics.Controllers;
using Content.Shared.RemoteVehicle.Components;
using Content.Shared.RemoteVehicle.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Verbs;
using Content.Shared.Alert;
using Content.Shared.Movement.Systems;
using Content.Shared.Interaction;
using Content.Server.Popups;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Components;
using Content.Server.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server.RemoteVehicle.Systems
{
    public sealed class RemoteVehicleControllerSystem : SharedRemoteVehicleControllerSystem
    {
        [Dependency] private readonly UserInterfaceSystem _ui = default!;
        [Dependency] private readonly MoverController _moverController = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
        [Dependency] private readonly AlertsSystem _alerts = default!;
        [Dependency] private readonly ViewSubscriberSystem _viewSubscriber = default!;

        [Dependency] private readonly RemoteVehicleSystem _vehicleSystem = default!;
        [Dependency] private readonly RemoteVehicleModuleSystem _moduleSystem = default!;


        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RemoteVehicleControllerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<RemoteVehicleControllerComponent, ComponentShutdown>(OnShutdown);

            SubscribeLocalEvent<RemoteVehicleControllerComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<RemoteVehicleControllerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);

            SubscribeLocalEvent<RemoteVehicleControllerComponent, AfterActivatableUIOpenEvent>(OnControlUiOpen);
            SubscribeLocalEvent<RemoteVehicleControllerComponent, BoundUIClosedEvent>(OnControlUiClose);

            SubscribeLocalEvent<RemoteVehicleControllerComponent, RemoteControlStartMessage>(OnControlUiStartMessage);
            SubscribeLocalEvent<RemoteVehicleControllerComponent, RemoteControlModuleUseMessage>(OnControlUiModuleUseMessage);

            SubscribeLocalEvent<RemoteVehicleControllerUserComponent, UpdateCanMoveEvent>(HandleMovementBlock);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var controlQuery = AllEntityQuery<RemoteVehicleControllerComponent>();
            while (controlQuery.MoveNext(out var uid, out var comp))
            {
                foreach (var user in comp.CurrentUsers)
                {
                    var mover = Comp<InputMoverComponent>(user);

                    if (mover.HeldMoveButtons != MoveButtons.None)
                    {
                        SetInput(comp, mover.HeldMoveButtons);
                        break;
                    }

                    SetInput(comp, MoveButtons.None);
                }
            }
        }

        private void OnInit(EntityUid uid, RemoteVehicleControllerComponent component, ComponentInit args)
        {
            base.Initialize();
        }

        private void OnShutdown(EntityUid uid, RemoteVehicleControllerComponent component, ComponentShutdown args)
        {
            DisconnectVehicle(component);
            ClearUsers(component);
        }

        private void OnAfterInteract(EntityUid uid, RemoteVehicleControllerComponent component, AfterInteractEvent args)
        {
            if (args.Target == null || !args.CanReach)
                return;

            if (component.ConnectedVehicle != null && component.ConnectedVehicle.IsAssembled)
                return;

            if (TryComp<RemoteVehicleComponent>(args.Target, out var vehicle))
            {
                if (TryConnectVehicle(component, vehicle))
                    ShowPopup(Loc.GetString("remote-control-connected"), uid, args.User);
                else
                    ShowPopup(Loc.GetString("remote-control-already-connected"), uid, args.User);
            }
            else
                ShowPopup(Loc.GetString("remote-control-not-vehicle"), uid, args.User);

            UpdateUi(uid, component);
        }

        private void OnGetVerbs(EntityUid uid, RemoteVehicleControllerComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (component.ConnectedVehicle == null)
                return;

            AlternativeVerb verb = new();
            verb.Text = Loc.GetString("remote-control-disconnect-verb");
            verb.Act = () => OnDisconectVerb(component, uid, args.User);
            args.Verbs.Add(verb);
        }

        private void OnControlUiOpen(EntityUid uid, RemoteVehicleControllerComponent component, AfterActivatableUIOpenEvent args)
        {
            if (TryComp<InputMoverComponent>(args.User, out var mover))
            {
                AddUser(args.User, component);

                if (component.ConnectedVehicle != null)
                {
                    if (TryComp<ActorComponent>(args.User, out var actor))
                        _viewSubscriber.AddViewSubscriber(component.ConnectedVehicle.Owner, actor.PlayerSession);
                }
            }

            UpdateUi(uid, component);
        }

        private void OnControlUiClose(EntityUid uid, RemoteVehicleControllerComponent component, BoundUIClosedEvent args)
        {
            if ((RemoteControlUiKey) args.UiKey != RemoteControlUiKey.Key ||
                args.Session.AttachedEntity is not { } user)
            {
                return;
            }

            if (HasComp<RemoteVehicleControllerUserComponent>(user))
            {
                RemoveUser(user);

                if (component.ConnectedVehicle != null)
                {
                    if (TryComp<ActorComponent>(args.Session.AttachedEntity, out var actor))
                        _viewSubscriber.RemoveViewSubscriber(component.ConnectedVehicle.Owner, actor.PlayerSession);
                }
            }

            UpdateUi(uid, component);
        }

        private void OnControlUiStartMessage(EntityUid uid, RemoteVehicleControllerComponent component, RemoteControlStartMessage msg)
        {
            if (component.ConnectedVehicle != null)
            {
                if (component.ConnectedVehicle.Enabled)
                    _vehicleSystem.TryDisable(component.ConnectedVehicle, component.ConnectedVehicle.Owner);
                else
                    _vehicleSystem.TryEnable(component.ConnectedVehicle, component.ConnectedVehicle.Owner);
            }
            UpdateUi(uid, component);
        }

        private void OnControlUiModuleUseMessage(EntityUid uid, RemoteVehicleControllerComponent component, RemoteControlModuleUseMessage msg)
        {
            if (component.ConnectedVehicle != null)
            {
                _moduleSystem.TryUseModule(GetEntity(msg.ModuleUid), msg.Session.AttachedEntity, uid, component.ConnectedVehicle.Owner);
            }

            UpdateUi(uid, component);
        }

        public void UpdateUi(EntityUid uid, RemoteVehicleControllerComponent component)
        {
            if (!_ui.TryGetUi(uid, RemoteControlUiKey.Key, out _))
                return;

            RemoteControlBoundUserInterfaceState state;
            if (component.ConnectedVehicle != null)
            {
                var modulesEntities = _moduleSystem.TryGetModulesEntities(component.ConnectedVehicle.Owner);

                state = new RemoteControlBoundUserInterfaceState(GetNetEntity(component.ConnectedVehicle.Owner), modulesEntities == null ? null : GetNetEntityArray(modulesEntities));
            }
            else
                state = new RemoteControlBoundUserInterfaceState(null, null);

            _ui.TrySetUiState(uid, RemoteControlUiKey.Key, state);
        }

        public bool TryConnectVehicle(RemoteVehicleControllerComponent component, RemoteVehicleComponent target)
        {
            if (component.ConnectedVehicle == null && target.ConnectedController == null)
            {
                component.ConnectedVehicle = target;
                target.ConnectedController = component;

                return true;
            }

            return false;
        }

        public void OnDisconectVerb(RemoteVehicleControllerComponent component, EntityUid uid, EntityUid user)
        {
            DisconnectVehicle(component);

            ShowPopup(Loc.GetString("remote-control-disconnect"), uid, user);
        }

        public void DisconnectVehicle(RemoteVehicleControllerComponent component)
        {
            if (component.ConnectedVehicle != null)
            {
                component.ConnectedVehicle.ConnectedController = null;
                _vehicleSystem.TryDisable(component.ConnectedVehicle, component.ConnectedVehicle.Owner);
            }

            component.ConnectedVehicle = null;

            UpdateUi(component.Owner, component);
        }

        private void HandleMovementBlock(EntityUid uid, RemoteVehicleControllerUserComponent component, UpdateCanMoveEvent args)
        {
            if (component.LifeStage > ComponentLifeStage.Running)
                return;
            if (component.Controller == null)
                return;

            args.Cancel();
        }

        private void RemoveUser(EntityUid uid)
        {
            var userComp = Comp<RemoteVehicleControllerUserComponent>(uid);

            if (userComp.Controller == null)
                return;

            var controllerComp = Comp<RemoteVehicleControllerComponent>(userComp.Controller.Value);
            controllerComp.CurrentUsers.Remove(uid);

            RemComp<RemoteVehicleControllerUserComponent>(uid);

            _actionBlocker.UpdateCanMove(uid);
        }

        private void ClearUsers(RemoteVehicleControllerComponent component)
        {
            foreach (var user in component.CurrentUsers)
                RemoveUser(user);
        }

        private void AddUser(EntityUid uid, RemoteVehicleControllerComponent controller)
        {
            if (HasComp<RemoteVehicleControllerUserComponent>(uid))
                return;

            var userComp = AddComp<RemoteVehicleControllerUserComponent>(uid);

            userComp.Controller = controller.Owner;
            controller.CurrentUsers.Add(uid);

            _actionBlocker.UpdateCanMove(uid);
        }

        private void SetInput(RemoteVehicleControllerComponent component, MoveButtons buttons)
        {
            component.CurrentInput = buttons;

            if (component.ConnectedVehicle != null)
                _vehicleSystem.TrySetInput(component.ConnectedVehicle.Owner, _moverController.DirVecForButtons(component.CurrentInput));
        }

        private void ShowPopup(string message, EntityUid entity, EntityUid user)
        {
            _popupSystem.PopupEntity(message, entity, user);
        }
    }
}
