using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views;

public partial class VehicleEditorWindow : Window
{
    private readonly EditorDialogLifecycle<VehicleDetailWorkspaceViewModel> _lifecycle;

    public VehicleEditorWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _lifecycle = EditorDialogFocusHelpers.CreateLifecycle<VehicleDetailWorkspaceViewModel>(
            this,
            "VehicleEditorNameBox",
            "CancelVehicleButton",
            viewModel => viewModel.SaveVehicleCommand,
            viewModel => viewModel.CancelVehicleEditCommand,
            viewModel => viewModel.IsEditingVehicle,
            (viewModel, handler) => viewModel.FocusRequested += handler,
            (viewModel, handler) => viewModel.FocusRequested -= handler,
            GetFocusControlName);
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        await _lifecycle.SaveAndCloseIfValidAsync();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        _lifecycle.CancelAndClose();
    }

    private static string? GetFocusControlName(DesktopFocusTarget target) =>
        target switch
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
            _ => null
        };
}
