using Robust.Shared.GameStates;
using Content.Shared.Movement.Systems;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.RemoteVehicle.Components;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class RemoteVehicleControllerUserComponent : Component
{
    public EntityUid? Controller;

    public float BreakDistance = 0.25f;

    [ViewVariables]
    public MoveButtons HeldButtons = MoveButtons.None;
}
