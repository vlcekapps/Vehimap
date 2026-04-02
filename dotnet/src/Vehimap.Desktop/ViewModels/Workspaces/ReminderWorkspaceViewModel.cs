using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class ReminderWorkspaceViewModel : WorkspaceViewModelBase
{
    public ReminderWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.ReminderWindowTitle;
    public string ReminderSummary => Root.ReminderSummary;
    public ObservableCollection<VehicleReminderItemViewModel> SelectedVehicleReminders => Root.SelectedVehicleReminders;
    public VehicleReminderItemViewModel? SelectedReminder
    {
        get => Root.SelectedReminder;
        set => Root.SelectedReminder = value;
    }

    public string SelectedReminderDetail => Root.SelectedReminderDetail;
    public string ReminderPanelHeading => Root.ReminderPanelHeading;
    public bool IsEditingReminder => Root.IsEditingReminder;
    public bool IsReminderDetailVisible => Root.IsReminderDetailVisible;
    public string ReminderEditorStatus => Root.ReminderEditorStatus;
    public string ReminderEditorTitle
    {
        get => Root.ReminderEditorTitle;
        set => Root.ReminderEditorTitle = value;
    }

    public string ReminderEditorDueDate
    {
        get => Root.ReminderEditorDueDate;
        set => Root.ReminderEditorDueDate = value;
    }

    public string ReminderEditorDays
    {
        get => Root.ReminderEditorDays;
        set => Root.ReminderEditorDays = value;
    }

    public string ReminderEditorRepeatMode
    {
        get => Root.ReminderEditorRepeatMode;
        set => Root.ReminderEditorRepeatMode = value;
    }

    public string ReminderEditorNote
    {
        get => Root.ReminderEditorNote;
        set => Root.ReminderEditorNote = value;
    }

    public ICommand CreateReminderCommand => Root.CreateReminderCommand;
    public ICommand EditSelectedReminderCommand => Root.EditSelectedReminderCommand;
    public ICommand DeleteSelectedReminderCommand => Root.DeleteSelectedReminderCommand;
    public ICommand SaveReminderCommand => Root.SaveReminderCommand;
    public ICommand CancelReminderEditCommand => Root.CancelReminderEditCommand;
}

