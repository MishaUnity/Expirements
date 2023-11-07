using Content.Shared.Stealth.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.RemoteVehicle.Modules.Components;

[RegisterComponent]
public sealed class RemoteVehicleCameraModuleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public bool CameraEnabled;
}
