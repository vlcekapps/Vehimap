using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views;

public partial class RecordEditorWindow : Window
{
    private readonly EditorDialogLifecycle<RecordWorkspaceViewModel> _lifecycle;

    public RecordEditorWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _lifecycle = EditorDialogFocusHelpers.CreateLifecycle<RecordWorkspaceViewModel>(
            this,
            "RecordEditorTypeBox",
            "CancelRecordButton",
            viewModel => viewModel.SaveRecordCommand,
            viewModel => viewModel.CancelRecordEditCommand,
            viewModel => viewModel.IsEditingRecord,
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
            DesktopFocusTarget.RecordEditorType => "RecordEditorTypeBox",
            DesktopFocusTarget.RecordEditorTitle => "RecordEditorTitleBox",
            DesktopFocusTarget.RecordEditorValidFrom => "RecordEditorValidFromBox",
            DesktopFocusTarget.RecordEditorValidTo => "RecordEditorValidToBox",
            DesktopFocusTarget.RecordEditorPrice => "RecordEditorPriceBox",
            DesktopFocusTarget.RecordEditorPathInput => "RecordEditorPathInputBox",
            _ => null
        };
}
