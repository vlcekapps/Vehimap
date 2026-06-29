using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views;

public partial class FuelEditorWindow : Window
{
    private readonly EditorDialogLifecycle<FuelWorkspaceViewModel> _lifecycle;

    public FuelEditorWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _lifecycle = EditorDialogFocusHelpers.CreateLifecycle<FuelWorkspaceViewModel>(
            this,
            "FuelEditorDateBox",
            "CancelFuelButton",
            viewModel => viewModel.SaveFuelCommand,
            viewModel => viewModel.CancelFuelEditCommand,
            viewModel => viewModel.IsEditingFuel,
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
            DesktopFocusTarget.FuelEditorDate => "FuelEditorDateBox",
            DesktopFocusTarget.FuelEditorOdometer => "FuelEditorOdometerBox",
            DesktopFocusTarget.FuelEditorLiters => "FuelEditorLitersBox",
            DesktopFocusTarget.FuelEditorTotalCost => "FuelEditorTotalCostBox",
            _ => null
        };
}
