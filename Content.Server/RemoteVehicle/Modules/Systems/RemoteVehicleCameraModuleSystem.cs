using Content.Server.RemoteVehicle.Modules.Components;
using Content.Server.Stealth;
using Content.Server.SurveillanceCamera;
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
    public sealed class RemoteVehicleCameraModuleSystem : EntitySystem
    {
        [Dependency] private readonly SurveillanceCameraSystem _camera = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RemoteVehicleCameraModuleComponent, RemoteVehicleModuleInsertedEvent>(OnModuleInserted);
            SubscribeLocalEvent<RemoteVehicleCameraModuleComponent, RemoteVehicleModuleRemovedEvent>(OnModuleRemoved);

            SubscribeLocalEvent<RemoteVehicleCameraModuleComponent, RemoteVehicleModuleStateChangedEvent>(OnModuleStateChanged);
            SubscribeLocalEvent<RemoteVehicleCameraModuleComponent, RemoteVehicleModuleUseEvent>(OnModuleUse);
        }

        private void OnModuleInserted(EntityUid uid, RemoteVehicleCameraModuleComponent component, RemoteVehicleModuleInsertedEvent args)
        {
            SetCameraState(uid, component, component.CameraEnabled);
        }

        private void OnModuleRemoved(EntityUid uid, RemoteVehicleCameraModuleComponent component, RemoteVehicleModuleRemovedEvent args)
        {
            SetCameraState(uid, component, false);
        }

        private void OnModuleStateChanged(EntityUid uid, RemoteVehicleCameraModuleComponent component, RemoteVehicleModuleStateChangedEvent args)
        {
            if (!args.Enabled)
                SetCameraState(uid, component, false);
        }

        private void OnModuleUse(EntityUid uid, RemoteVehicleCameraModuleComponent component, RemoteVehicleModuleUseEvent args)
        {
            SetCameraState(uid, component, !component.CameraEnabled);
        }

        private void SetCameraState(EntityUid uid, RemoteVehicleCameraModuleComponent component, bool enabled)
        {
            component.CameraEnabled = enabled;

            _camera.SetActive(uid, enabled);

            var moduleComp = Comp<RemoteVehicleModuleComponent>(uid);
            moduleComp.ModuleStatus = Loc.GetString(enabled ? "remote-vehicle-module-camera-enabled" : "remote-vehicle-module-camera-disabled");
            Dirty(moduleComp);
        }
    }
}

