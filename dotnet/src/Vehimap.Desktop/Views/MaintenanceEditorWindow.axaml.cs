using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views;

public partial class MaintenanceEditorWindow : Window
{
    private readonly EditorDialogLifecycle<MaintenanceWorkspaceViewModel> _lifecycle;

    public MaintenanceEditorWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _lifecycle = EditorDialogFocusHelpers.CreateLifecycle<MaintenanceWorkspaceViewModel>(
            this,
            "MaintenanceTemplateComboBox",
            "CancelMaintenanceButton",
            viewModel => viewModel.SaveMaintenanceCommand,
            viewModel => viewModel.CancelMaintenanceEditCommand,
            viewModel => viewModel.IsEditingMaintenance,
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
            DesktopFocusTarget.MaintenanceEditorTemplate => "MaintenanceTemplateComboBox",
            DesktopFocusTarget.MaintenanceEditorTitle => "MaintenanceEditorTitleBox",
            DesktopFocusTarget.MaintenanceEditorIntervalKm => "MaintenanceEditorIntervalKmBox",
            DesktopFocusTarget.MaintenanceEditorIntervalMonths => "MaintenanceEditorIntervalMonthsBox",
            DesktopFocusTarget.MaintenanceEditorLastServiceDate => "MaintenanceEditorLastServiceDateBox",
            DesktopFocusTarget.MaintenanceEditorLastServiceOdometer => "MaintenanceEditorLastServiceOdometerBox",
            _ => null
        };
}
