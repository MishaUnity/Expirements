using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared.RemoteVehicle.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class RemoteVehicleControllerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public RemoteVehicleComponent? ConnectedVehicle;

    public List<EntityUid> CurrentUsers = new List<EntityUid>();

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
    public NetEntity? ConnectedVehicle;
    public NetEntity[]? VehicleModules;

    public RemoteControlBoundUserInterfaceState(NetEntity? vehicle, NetEntity[]? modules)
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
    public NetEntity ModuleUid;

    public RemoteControlModuleUseMessage(NetEntity moduleUid)
    {
        ModuleUid = moduleUid;
    }
}

