using Content.Shared.RemoteVehicle.Components;
using Robust.Client.GameObjects;

namespace Content.Client.RemoteVehicle;

public sealed class RemoteVehicleSystem : VisualizerSystem<RemoteVehicleComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, RemoteVehicleComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<bool>(uid, RemoteVehicleVisuals.Enabled, out var enabled, args.Component))
            args.Sprite.LayerSetState(RemoteVehicleVisualsLayers.Animated, enabled ? "on" : "off");

        if (AppearanceSystem.TryGetData<bool>(uid, RemoteVehicleVisuals.Assembled, out var assembled, args.Component))
            args.Sprite.LayerSetVisible(RemoteVehicleVisualsLayers.Wires, !assembled);
    }
}
