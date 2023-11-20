using Robust.Client.GameObjects;
using Content.Client.Eye;
using JetBrains.Annotations;
using Content.Shared.RemoteVehicle.Components;

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

        EntityUid? vehicleEntity = EntMan.GetEntity(cast.ConnectedVehicle);

        if (vehicleEntity == null)
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
                _eyeLerpingSystem.AddEye(vehicleEntity.Value);
                _currentVehicle = vehicleEntity;
            }
            else if (_currentVehicle != vehicleEntity)
            {
                _eyeLerpingSystem.RemoveEye(_currentVehicle.Value);
                _eyeLerpingSystem.AddEye(vehicleEntity.Value);
                _currentVehicle = vehicleEntity;
            }

            if (EntMan.TryGetComponent<EyeComponent>(vehicleEntity, out var eyeComp) &&
                EntMan.TryGetComponent<RemoteVehicleComponent>(vehicleEntity, out var vehicleComp))
            {
                _menu.UpdateUiState(eyeComp.Eye, _spriteSystem, vehicleComp, GetModulesComp(cast.VehicleModules == null ? null : EntMan.GetEntityArray(cast.VehicleModules)));
            }
        }
    }

    private void OnStartButtonPressed()
    {
        SendMessage(new RemoteControlStartMessage());
    }

    private void OnModuleUseButtonPressed(EntityUid moduleUid)
    {
        SendMessage(new RemoteControlModuleUseMessage(EntMan.GetNetEntity(moduleUid)));
    }

    private RemoteVehicleModuleComponent[]? GetModulesComp(EntityUid[]? modules)
    {
        if (modules == null)
            return null;

        var result = new RemoteVehicleModuleComponent[modules.Length];
        for (int i = 0; i < modules.Length; i++)
        {
            result[i] = EntMan.GetComponent<RemoteVehicleModuleComponent>(modules[i]);
        }

        return result;
    }
}
