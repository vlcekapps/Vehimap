using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views;

public partial class HistoryEditorWindow : Window
{
    private readonly EditorDialogLifecycle<HistoryWorkspaceViewModel> _lifecycle;

    public HistoryEditorWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _lifecycle = EditorDialogFocusHelpers.CreateLifecycle<HistoryWorkspaceViewModel>(
            this,
            "HistoryEditorDateBox",
            "CancelHistoryButton",
            viewModel => viewModel.SaveHistoryCommand,
            viewModel => viewModel.CancelHistoryEditCommand,
            viewModel => viewModel.IsEditingHistory,
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
            DesktopFocusTarget.HistoryEditorDate => "HistoryEditorDateBox",
            DesktopFocusTarget.HistoryEditorType => "HistoryEditorTypeBox",
            DesktopFocusTarget.HistoryEditorOdometer => "HistoryEditorOdometerBox",
            DesktopFocusTarget.HistoryEditorCost => "HistoryEditorCostBox",
            _ => null
        };
}
