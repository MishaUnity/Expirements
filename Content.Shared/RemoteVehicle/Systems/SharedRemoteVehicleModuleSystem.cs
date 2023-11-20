using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.RemoteVehicle.Components;
using Content.Shared.Tools;
using Content.Shared.Tools.Components;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared.RemoteVehicle.Systems;

public abstract class SharedRemoteVehicleModuleSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RemoteVehicleModuleContainerComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, RemoteVehicleModuleContainerComponent component, InteractUsingEvent args)
    {
        if (TryInsertModule(uid, component, args.Used))
        {
            _audioSystem.PlayPvs(component.InsertSound, uid);
        }
    }

    public bool TryInsertModule(EntityUid uid, RemoteVehicleModuleContainerComponent component, EntityUid moduleUid)
    {
        if (!_containerSystem.TryGetContainer(uid, component.ContainerId, out var container) || container.ContainedEntities.Count >= component.ContainerCapacity)
            return false;
        if (!TryComp<RemoteVehicleComponent>(uid, out var vehicle) || vehicle.IsAssembled || !HasComp<RemoteVehicleModuleComponent>(moduleUid))
            return false;

        container.Insert(moduleUid);
        RaiseLocalEvent(moduleUid, new RemoteVehicleModuleInsertedEvent(uid));

        UpdateVehicleModules(component, uid);

        return true;
    }

    public void UpdateVehicleModules(RemoteVehicleModuleContainerComponent component, EntityUid uid)
    {
        if (TryComp<RemoteVehicleComponent>(uid, out var vehicle))
            vehicle.InsertedModules = TryGetModules(uid);

        RaiseLocalEvent(uid, new RemoteVehicleModulesChangedEvent(), false);
    }

    public EntityUid[]? TryGetModulesEntities(EntityUid uid)
    {
        if (!TryComp<RemoteVehicleModuleContainerComponent>(uid, out var comp))
            return null;

        var moduleEnities = _containerSystem.GetContainer(uid, comp.ContainerId).ContainedEntities;

        var result = new EntityUid[moduleEnities.Count];
        for (int i = 0; i < moduleEnities.Count; i++)
        {
            result[i] = moduleEnities[i];
        }

        return result;
    }

    public RemoteVehicleModuleComponent[]? TryGetModules(EntityUid uid)
    {
        if (!TryComp<RemoteVehicleModuleContainerComponent>(uid, out var comp))
            return null;

        var moduleEnities = _containerSystem.GetContainer(uid, comp.ContainerId).ContainedEntities;

        var result = new RemoteVehicleModuleComponent[moduleEnities.Count];
        for (int i = 0; i < moduleEnities.Count; i++)
        {
            result[i] = Comp<RemoteVehicleModuleComponent>(moduleEnities[i]);
        }

        return result;
    }
}
