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



