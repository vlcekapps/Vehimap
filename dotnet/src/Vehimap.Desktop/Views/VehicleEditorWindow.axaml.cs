using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views;

public partial class VehicleEditorWindow : Window
{
    private VehicleDetailWorkspaceViewModel? _viewModel;

    public VehicleEditorWindow()
    {
        AvaloniaXamlLoader.Load(this);
        DataContextChanged += OnDataContextChanged;
        EditorDialogFocusHelpers.RegisterEditorDialog(
            this,
            "VehicleEditorNameBox",
            "CancelVehicleButton",
            CancelAndClose);
        AddHandler(InputElement.KeyDownEvent, OnVehicleEditorKeyDown, RoutingStrategies.Tunnel);
        Closing += OnClosing;
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        await SaveAndCloseIfValidAsync();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        CancelAndClose();
    }

    private async void OnVehicleEditorKeyDown(object? sender, KeyEventArgs e)
    {
        if (KeyboardAccessibilityHelper.ShouldSkipGlobalShortcut(e))
        {
            return;
        }

        if (e.Key == Key.S && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            await SaveAndCloseIfValidAsync();
        }
    }

    private async Task SaveAndCloseIfValidAsync()
    {
        if (_viewModel is null)
        {
            Close(false);
            return;
        }

        if (!_viewModel.SaveVehicleCommand.CanExecute(null))
        {
            return;
        }

        await _viewModel.SaveVehicleCommand.ExecuteAsync(null);
        if (_viewModel.IsEditingVehicle)
        {
            return;
        }

        Close(true);
    }

    private void CancelAndClose()
    {
        if (_viewModel?.CancelVehicleEditCommand.CanExecute(null) == true)
        {
            _viewModel.CancelVehicleEditCommand.Execute(null);
        }

        Close(false);
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_viewModel?.IsEditingVehicle == true
            && _viewModel.CancelVehicleEditCommand.CanExecute(null))
        {
            _viewModel.CancelVehicleEditCommand.Execute(null);
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.FocusRequested -= OnFocusRequested;
        }

        _viewModel = DataContext as VehicleDetailWorkspaceViewModel;
        if (_viewModel is not null)
        {
            _viewModel.FocusRequested += OnFocusRequested;
        }
    }

    private void OnFocusRequested(DesktopFocusTarget target)
    {
        var controlName = target switch
        {
            DesktopFocusTarget.VehicleEditorName => "VehicleEditorNameBox",
            DesktopFocusTarget.VehicleEditorCategory => "VehicleEditorCategoryBox",
            DesktopFocusTarget.VehicleEditorMakeModel => "VehicleEditorMakeModelBox",
            DesktopFocusTarget.VehicleEditorYear => "VehicleEditorYearBox",
            DesktopFocusTarget.VehicleEditorLastTk => "VehicleEditorLastTkBox",
            DesktopFocusTarget.VehicleEditorNextTk => "VehicleEditorNextTkBox",
            DesktopFocusTarget.VehicleEditorGreenCardFrom => "VehicleEditorGreenCardFromBox",
            DesktopFocusTarget.VehicleEditorGreenCardTo => "VehicleEditorGreenCardToBox",
            DesktopFocusTarget.VehicleEditorCancel => "CancelVehicleButton",
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(controlName))
        {
            return;
        }

        Dispatcher.UIThread.Post(
            () => this.FindControl<Control>(controlName)?.Focus(NavigationMethod.Unspecified, KeyModifiers.None),
            DispatcherPriority.Background);
    }
}
