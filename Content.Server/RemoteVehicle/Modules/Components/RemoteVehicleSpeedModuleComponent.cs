using Content.Shared.Stealth.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.RemoteVehicle.Modules.Components;

[RegisterComponent]
public sealed class RemoteVehicleSpeedModuleComponent : Component
{
    [DataField("speedMultiplier")]
    public float SpeedMultiplier = 1.5f;
}

