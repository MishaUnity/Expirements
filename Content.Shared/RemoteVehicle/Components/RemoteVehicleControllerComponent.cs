using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared.RemoteVehicle.Components;

[RegisterComponent, NetworkedComponent]
public sealed class RemoteVehicleControllerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public RemoteVehicleComponent? ConnectedVehicle;

    public List<InputMoverComponent> CurrentPilots = new List<InputMoverComponent>();

    [ViewVariables(VVAccess.ReadWrite)]
    public MoveButtons CurrentInput;
}

[Serializable, NetSerializable]
public enum RemoteControlUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class RemoteControlBoundUserInterfaceState : BoundUserInterfaceState
{
    public EntityUid? ConnectedVehicle;
    public EntityUid[]? VehicleModules;

    public RemoteControlBoundUserInterfaceState(EntityUid? vehicle, EntityUid[]? modules)
    {
        ConnectedVehicle = vehicle;
        VehicleModules = modules;
    }
}

[Serializable, NetSerializable]
public sealed class RemoteControlStartMessage : BoundUserInterfaceMessage
{
    public RemoteControlStartMessage()
    {

    }
}

[Serializable, NetSerializable]
public sealed class RemoteControlModuleUseMessage : BoundUserInterfaceMessage
{
    public EntityUid ModuleUid;

    public RemoteControlModuleUseMessage(EntityUid moduleUid)
    {
        ModuleUid = moduleUid;
    }
}

