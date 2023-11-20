using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.RemoteVehicle.Modules.Components;
using Content.Shared.RemoteVehicle.Components;

namespace Content.Server.RemoteVehicle.Modules.Systems
{
    public sealed class RemoteVehicleTimerTriggerModuleSystem : EntitySystem
    {
        [Dependency] private readonly TriggerSystem _trigger = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RemoteVehicleTimerTriggerModuleComponent, ComponentInit>(OnModuleInit);

            SubscribeLocalEvent<RemoteVehicleTimerTriggerModuleComponent, RemoteVehicleModuleUseEvent>(OnModuleUse);
            SubscribeLocalEvent<RemoteVehicleTimerTriggerModuleComponent, TriggerEvent>(OnModuleTriggered);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var modulesQuery = AllEntityQuery<RemoteVehicleTimerTriggerModuleComponent, ActiveTimerTriggerComponent>();
            while (modulesQuery.MoveNext(out var uid, out var moduleComp, out var timerComp))
            {
                UpdateModule(uid, true, timerComp.TimeRemaining);
            }
        }

        private void OnModuleInit(EntityUid uid, RemoteVehicleTimerTriggerModuleComponent component, ComponentInit args)
        {
            UpdateModule(uid, false, 0f);
        }

        private void OnModuleUse(EntityUid uid, RemoteVehicleTimerTriggerModuleComponent component, RemoteVehicleModuleUseEvent args)
        {
            _trigger.HandleTimerTrigger(uid, args.User, component.TimerDelay, component.BeepDeley, component.InitialBeepDeley, component.BeepSound);
        }

        private void OnModuleTriggered(EntityUid uid, RemoteVehicleTimerTriggerModuleComponent component, TriggerEvent args)
        {
            UpdateModule(uid, false, 0f);
        }

        private void UpdateModule(EntityUid uid, bool timerActive, float timeRemaining)
        {
            var moduleComp = Comp<RemoteVehicleModuleComponent>(uid);

            if (timerActive)
            {
                moduleComp.ModuleStatus = Math.Ceiling(timeRemaining) + "s";
                moduleComp.UseAvalible = false;
            }
            else
            {
                moduleComp.ModuleStatus = Loc.GetString("remote-vehicle-module-timer-trigger-avalible");
                moduleComp.UseAvalible = true;
            }

            Dirty(moduleComp);
        }
    }
}
