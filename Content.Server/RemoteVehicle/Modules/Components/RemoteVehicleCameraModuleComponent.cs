namespace Content.Server.RemoteVehicle.Modules.Components;

[RegisterComponent]
public sealed partial class RemoteVehicleCameraModuleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public bool CameraEnabled;
}
