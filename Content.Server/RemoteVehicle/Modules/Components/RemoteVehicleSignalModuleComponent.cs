namespace Content.Server.RemoteVehicle.Modules.Components;

[RegisterComponent]
public sealed partial class RemoteVehicleSignalModuleComponent : Component
{
    [DataField("radiuseMultiplier")]
    public float RadiuseMultiplier = 2f;
}
