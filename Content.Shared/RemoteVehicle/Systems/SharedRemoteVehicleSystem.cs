using Content.Shared.DoAfter;
using Content.Shared.PowerCell;
using Robust.Shared.Serialization;

namespace Content.Shared.RemoteVehicle.Systems;

public abstract class SharedRemoteVehicleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }
}

[Serializable, NetSerializable]
public sealed partial class RemoteVehiclePryFinished : SimpleDoAfterEvent
{

}

[Serializable, NetSerializable]
public sealed partial class RemoteVehicleScrewFinished : SimpleDoAfterEvent
{

}
