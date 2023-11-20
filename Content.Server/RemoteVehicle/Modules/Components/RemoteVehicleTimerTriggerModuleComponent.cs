using Robust.Shared.Audio;

namespace Content.Server.RemoteVehicle.Modules.Components;

[RegisterComponent]
public sealed partial class RemoteVehicleTimerTriggerModuleComponent : Component
{
    [DataField("timerDelay")]
    public float TimerDelay = 3f;

    [DataField("beepDelay")]
    public float BeepDeley = 1f;
    [DataField("initialBeepDelay")]
    public float InitialBeepDeley;

    [DataField("beepSound")]
    public SoundSpecifier? BeepSound;
}
