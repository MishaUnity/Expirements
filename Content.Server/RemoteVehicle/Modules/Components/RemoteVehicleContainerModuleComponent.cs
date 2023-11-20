using Robust.Shared.Audio;

namespace Content.Server.RemoteVehicle.Modules.Components;

[RegisterComponent]
public sealed partial class RemoteVehicleContainerModuleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? VehicleUid;

    [DataField("containerCapacity")]
    public int ContainerCapacity = 5;

    [DataField("insertSound")]
    public SoundSpecifier? InsertSound;
    [DataField("ejectSound")]
    public SoundSpecifier? EjectSound;
}
