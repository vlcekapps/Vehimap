using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class ReminderWorkspaceViewModel : WorkspaceViewModelBase
{
    public ReminderWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.ReminderWindowTitle;
    public string ReminderSummary => Root.ReminderSummary;
    public ObservableCollection<VehicleReminderItemViewModel> SelectedVehicleReminders => Root.SelectedVehicleReminders;

    [ObservableProperty]
    private VehicleReminderItemViewModel? selectedReminder;

    [ObservableProperty]
    private string selectedReminderDetail = "Vyberte připomínku a zobrazí se detail položky.";

    [ObservableProperty]
    private string reminderPanelHeading = "Detail připomínky";

    [ObservableProperty]
    private bool isEditingReminder;

    [ObservableProperty]
    private string reminderEditorStatus = string.Empty;

    [ObservableProperty]
    private string reminderEditorTitle = string.Empty;

    [ObservableProperty]
    private string reminderEditorDueDate = string.Empty;

    [ObservableProperty]
    private string reminderEditorDays = string.Empty;

    [ObservableProperty]
    private string reminderEditorRepeatMode = string.Empty;

    [ObservableProperty]
    private string reminderEditorNote = string.Empty;

    public bool IsReminderDetailVisible => !IsEditingReminder;

    public ICommand CreateReminderCommand => Root.CreateReminderCommand;
    public ICommand EditSelectedReminderCommand => Root.EditSelectedReminderCommand;
    public ICommand DeleteSelectedReminderCommand => Root.DeleteSelectedReminderCommand;
    public ICommand SaveReminderCommand => Root.SaveReminderCommand;
    public ICommand CancelReminderEditCommand => Root.CancelReminderEditCommand;

    partial void OnSelectedReminderChanged(VehicleReminderItemViewModel? value)
    {
        SelectedReminderDetail = value is null
            ? "Vyberte připomínku a zobrazí se detail položky."
            : $"Název: {value.Title}\nTermín: {value.DueDate}\nStav: {value.Status}\nOpakování: {value.RepeatMode}\nPoznámka: {Root.FormatWorkspaceValue(value.Note, "bez poznámky")}";

        Root.NotifyReminderWorkspaceSelectionChanged();
    }

    partial void OnIsEditingReminderChanged(bool value)
    {
        ReminderPanelHeading = value
            ? (Root.GetEditingReminderId() is null ? "Nová připomínka" : "Upravit připomínku")
            : "Detail připomínky";

        OnPropertyChanged(nameof(IsReminderDetailVisible));
        Root.NotifyReminderWorkspaceEditingChanged();
    }
}
