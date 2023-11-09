using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared.RemoteVehicle.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class RemoteVehicleModuleComponent : Component
{
    [DataField("moduleName")]
    public string ModuleName = string.Empty;
    [DataField("moduleDesc")]
    public string ModuleDesc = string.Empty;

    [DataField("needUiCard")]
    public bool NeedUiCard = true;
    [DataField("accentColor")]
    public Color AccentColor = Color.White;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public string ModuleStatus = string.Empty;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool Enabled;
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool UseAvalible = true;
}

public sealed class RemoteVehicleModuleInsertedEvent : EntityEventArgs
{
    public EntityUid VehicleUid { get; }

    public RemoteVehicleModuleInsertedEvent(EntityUid vehicleUid)
    {
        VehicleUid = vehicleUid;
    }
}

public sealed class RemoteVehicleModuleRemovedEvent : EntityEventArgs
{
    public EntityUid VehicleUid { get; }

    public RemoteVehicleModuleRemovedEvent(EntityUid vehicleUid)
    {
        VehicleUid = vehicleUid;
    }
}

public sealed class RemoteVehicleModuleStateChangedEvent : EntityEventArgs
{
    public EntityUid VehicleUid { get; }
    public bool Enabled { get; }

    public RemoteVehicleModuleStateChangedEvent(EntityUid vehicleUid, bool enabled)
    {
        VehicleUid = vehicleUid;
        Enabled = enabled;
    }
}

public sealed class RemoteVehicleModuleUseEvent : EntityEventArgs
{
    public EntityUid? User { get; }

    public EntityUid VehicleUid { get; }
    public EntityUid ControllerUid { get; }

    public RemoteVehicleModuleUseEvent(EntityUid? user, EntityUid vehicleUid, EntityUid controllerUid)
    {
        User = user;

        VehicleUid = vehicleUid;
        ControllerUid = controllerUid;
    }
}
