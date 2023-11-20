namespace Content.Server.RemoteVehicle.Modules.Components;

[RegisterComponent]
public sealed partial class RemoteVehicleStealthModuleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? VehicleUid;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool InvisibleEnabled;

    [DataField("transparentDelta")]
    public float TransparentDelta = 1f;
    [DataField("drawRateModifier")]
    public float DrawRateModifier = 3.5f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentTransparentDelta;

    public bool DrawRateApplied;
}
