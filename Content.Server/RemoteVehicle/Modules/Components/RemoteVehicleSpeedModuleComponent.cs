namespace Content.Server.RemoteVehicle.Modules.Components;

[RegisterComponent]
public sealed partial class RemoteVehicleSpeedModuleComponent : Component
{
    [DataField("speedMultiplier")]
    public float SpeedMultiplier = 1.5f;
}

