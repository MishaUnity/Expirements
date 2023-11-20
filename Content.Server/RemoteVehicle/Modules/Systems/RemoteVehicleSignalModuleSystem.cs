using Content.Server.RemoteVehicle.Modules.Components;
using Content.Shared.RemoteVehicle.Components;

namespace Content.Server.RemoteVehicle.Modules.Systems
{
    public sealed class RemoteVehicleSignalModuleSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RemoteVehicleSignalModuleComponent, RemoteVehicleModuleInsertedEvent>(OnModuleInserted);
            SubscribeLocalEvent<RemoteVehicleSignalModuleComponent, RemoteVehicleModuleRemovedEvent>(OnModuleRemoved);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
        }

        private void OnModuleInserted(EntityUid uid, RemoteVehicleSignalModuleComponent component, RemoteVehicleModuleInsertedEvent args)
        {
            var vehicle = Comp<RemoteVehicleComponent>(args.VehicleUid);

            vehicle.SignalRadiuse = vehicle.SignalRadiuse * component.RadiuseMultiplier;
        }

        private void OnModuleRemoved(EntityUid uid, RemoteVehicleSignalModuleComponent component, RemoteVehicleModuleRemovedEvent args)
        {
            var vehicle = Comp<RemoteVehicleComponent>(args.VehicleUid);

            vehicle.SignalRadiuse = vehicle.SignalRadiuse / component.RadiuseMultiplier;
        }
    }
}



