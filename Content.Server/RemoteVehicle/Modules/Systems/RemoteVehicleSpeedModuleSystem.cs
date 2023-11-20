using Content.Server.RemoteVehicle.Modules.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.RemoteVehicle.Components;

namespace Content.Server.RemoteVehicle.Modules.Systems
{
    public sealed class RemoteVehicleSpeedModuleSystem : EntitySystem
    {
        [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RemoteVehicleSpeedModuleComponent, RemoteVehicleModuleInsertedEvent>(OnModuleInserted);
            SubscribeLocalEvent<RemoteVehicleSpeedModuleComponent, RemoteVehicleModuleRemovedEvent>(OnModuleRemoved);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
        }

        private void OnModuleInserted(EntityUid uid, RemoteVehicleSpeedModuleComponent component, RemoteVehicleModuleInsertedEvent args)
        {
            var speedModifier = EnsureComp<MovementSpeedModifierComponent>(args.VehicleUid);

            _movementSpeed.ChangeBaseSpeed(args.VehicleUid, speedModifier.CurrentWalkSpeed * component.SpeedMultiplier,
                                           speedModifier.CurrentSprintSpeed * component.SpeedMultiplier,
                                           speedModifier.Acceleration * component.SpeedMultiplier);
        }

        private void OnModuleRemoved(EntityUid uid, RemoteVehicleSpeedModuleComponent component, RemoteVehicleModuleRemovedEvent args)
        {
            var speedModifier = EnsureComp<MovementSpeedModifierComponent>(args.VehicleUid);

            _movementSpeed.ChangeBaseSpeed(args.VehicleUid, speedModifier.BaseWalkSpeed / component.SpeedMultiplier,
                                           speedModifier.CurrentSprintSpeed / component.SpeedMultiplier,
                                           speedModifier.Acceleration / component.SpeedMultiplier);
        }
    }
}




