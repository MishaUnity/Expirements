using Robust.Shared.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.RemoteVehicle.Components;

[RegisterComponent]
public sealed class RemoteVehicleModuleContainerComponent : Component
{
    [DataField("containerId", required: true)]
    public string ContainerId = string.Empty;
    [DataField("containerCapacity", required: true)]
    public int ContainerCapacity = 2;

    [DataField("insertSound")]
    public SoundSpecifier? InsertSound;
    [DataField("ejectSound")]
    public SoundSpecifier? EjectSound;
}

public sealed class RemoteVehicleModulesChangedEvent : EntityEventArgs
{
    public RemoteVehicleModulesChangedEvent()
    {

    }
}

