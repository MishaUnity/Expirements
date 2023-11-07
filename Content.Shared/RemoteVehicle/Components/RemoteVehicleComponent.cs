using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.RemoteVehicle.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class RemoteVehicleComponent : Component
{
    public static float PryDelay = 1f;
    public static float ScrewDelay = 1f;

    [ViewVariables(VVAccess.ReadWrite)]
    public RemoteVehicleControllerComponent? ConnectedController;
    [ViewVariables(VVAccess.ReadWrite)]
    public RemoteVehicleModuleComponent[]? InsertedModules;

    [ViewVariables(VVAccess.ReadWrite)]
    public float SignalRadiuse = 35f;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float BatteryCharge = 0f;
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float SignalLevel = 0f;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool IsAssembled = false;
    [ViewVariables(VVAccess.ReadWrite)]
    public bool IsRunning = false;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool Enabled;
}

[Serializable, NetSerializable]
public enum RemoteVehicleVisuals : byte
{
    Enabled,
    Assembled,
}

[Serializable, NetSerializable]
public enum RemoteVehicleVisualsLayers : byte
{
    Base,
    Animated,
    Wires
}
