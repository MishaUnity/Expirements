using Content.Shared.Stealth.Components;
using Robust.Shared.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.RemoteVehicle.Modules.Components;

[RegisterComponent]
public sealed class RemoteVehicleContainerModuleComponent : Component
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
