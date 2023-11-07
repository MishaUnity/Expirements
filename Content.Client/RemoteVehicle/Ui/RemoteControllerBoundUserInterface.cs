using Robust.Client.GameObjects;
using Content.Client.Eye;
using JetBrains.Annotations;
using Robust.Shared.Utility;
using Content.Shared.RemoteVehicle.Components;
using Robust.Client.Graphics;
using TerraFX.Interop.Windows;
using Content.Shared.RemoteVehicle.Systems;

namespace Content.Client.RemoteVehicle.Ui;

[UsedImplicitly]
public sealed class RemoteControllerBoundUserInterface : BoundUserInterface
{
    private readonly EyeLerpingSystem _eyeLerpingSystem;
    private readonly SpriteSystem _spriteSystem;

    [ViewVariables]
    private RemoteControllerMenu? _menu;

    [ViewVariables]
    private EntityUid? _currentVehicle;

    [ViewVariables]
    private string _windowName = Loc.GetString("remote-control-ui-default-title");

    public RemoteControllerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _eyeLerpingSystem = EntMan.System<EyeLerpingSystem>();
        _spriteSystem = EntMan.System<SpriteSystem>();
    }

    protected override void Open()
    {
        _menu = new RemoteControllerMenu(_windowName, Owner);

        _menu.OpenCentered();
        _menu.OnClose += Close;

        _menu.OnStartButtonPressed += OnStartButtonPressed;
        _menu.OnModuleUseButtonPressed += OnModuleUseButtonPressed;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        if (_currentVehicle != null)
        {
            _eyeLerpingSystem.RemoveEye(_currentVehicle.Value);
            _currentVehicle = null;
        }

        _menu?.Close();
        _menu?.Dispose();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_menu == null || state is not RemoteControlBoundUserInterfaceState cast)
            return;

        if (cast.ConnectedVehicle == null)
        {
            _menu.UpdateUiState(null, _spriteSystem, null, null);

            if (_currentVehicle != null)
            {
                _eyeLerpingSystem.RemoveEye(_currentVehicle.Value);
                _currentVehicle = null;
            }
        }
        else
        {
            if (_currentVehicle == null)
            {
                _eyeLerpingSystem.AddEye(cast.ConnectedVehicle.Value);
                _currentVehicle = cast.ConnectedVehicle;
            }
            else if (_currentVehicle != cast.ConnectedVehicle)
            {
                _eyeLerpingSystem.RemoveEye(_currentVehicle.Value);
                _eyeLerpingSystem.AddEye(cast.ConnectedVehicle.Value);
                _currentVehicle = cast.ConnectedVehicle;
            }

            if (EntMan.TryGetComponent<EyeComponent>(cast.ConnectedVehicle, out var eyeComp) &&
                EntMan.TryGetComponent<RemoteVehicleComponent>(cast.ConnectedVehicle, out var vehicleComp))
            {
                _menu.UpdateUiState(eyeComp.Eye, _spriteSystem, vehicleComp, GetModulesComp(cast.VehicleModules));
            }
        }
    }

    private void OnStartButtonPressed()
    {
        SendMessage(new RemoteControlStartMessage());
    }

    private void OnModuleUseButtonPressed(EntityUid moduleUid)
    {
        SendMessage(new RemoteControlModuleUseMessage(moduleUid));
    }

    private RemoteVehicleModuleComponent[]? GetModulesComp(EntityUid[]? modules)
    {
        if (modules == null)
            return null;

        var result = new RemoteVehicleModuleComponent[modules.Length];
        for (int i = 0; i < modules.Length; i ++)
        {
            result[i] = EntMan.GetComponent<RemoteVehicleModuleComponent>(modules[i]);
        }

        return result;
    }
}
