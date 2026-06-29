using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views;

public partial class ReminderEditorWindow : Window
{
    private readonly EditorDialogLifecycle<ReminderWorkspaceViewModel> _lifecycle;

    public ReminderEditorWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _lifecycle = EditorDialogFocusHelpers.CreateLifecycle<ReminderWorkspaceViewModel>(
            this,
            "ReminderEditorTitleBox",
            "CancelReminderButton",
            viewModel => viewModel.SaveReminderCommand,
            viewModel => viewModel.CancelReminderEditCommand,
            viewModel => viewModel.IsEditingReminder,
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
            DesktopFocusTarget.ReminderEditorTitle => "ReminderEditorTitleBox",
            DesktopFocusTarget.ReminderEditorDueDate => "ReminderEditorDueDateBox",
            DesktopFocusTarget.ReminderEditorDays => "ReminderEditorDaysBox",
            _ => null
        };
}
