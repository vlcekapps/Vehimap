// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class ReminderWorkspaceViewModel : WorkspaceViewModelBase
{
    public ReminderWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.ReminderWindowTitle;
    public ObservableCollection<VehicleReminderItemViewModel> SelectedVehicleReminders { get; } = [];
    public ObservableCollection<VehicleReminderItemViewModel> VisibleReminderItems { get; } = [];

    [ObservableProperty]
    private string reminderSummary = L("ReminderWorkspace.Summary.Initial");

    [ObservableProperty]
    private string reminderSearchText = string.Empty;

    [ObservableProperty]
    private string reminderSearchSummary = L("ReminderWorkspace.SearchSummary.Initial");

    [ObservableProperty]
    private string selectedReminderSortOption = WorkspaceSortHelpers.DueDateSortLabel;

    [ObservableProperty]
    private bool reminderSortDescending;

    public IReadOnlyList<string> ReminderSortOptions => WorkspaceSortHelpers.ReminderSortOptions;
    public IReadOnlyList<string> ReminderRepeatModeOptions => LegacyKnownValues.ReminderRepeatModes;

    public bool CanClearReminderSearch => !string.IsNullOrWhiteSpace(ReminderSearchText);

    [ObservableProperty]
    private VehicleReminderItemViewModel? selectedReminder;

    [ObservableProperty]
    private string selectedReminderDetail = L("ReminderWorkspace.Detail.Empty");

    [ObservableProperty]
    private string reminderPanelHeading = L("ReminderWorkspace.PanelHeading");

    [ObservableProperty]
    private string reminderEditorHeading = L("ReminderEditor.NewTitle");

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
    public ICommand AdvanceSelectedReminderCommand => Root.AdvanceSelectedReminderCommand;
    public ICommand SaveReminderCommand => Root.SaveReminderCommand;
    public ICommand CancelReminderEditCommand => Root.CancelReminderEditCommand;

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.ReminderSearch);
    }

    [RelayCommand(CanExecute = nameof(CanClearReminderSearch))]
    private void ClearReminderSearch()
    {
        ReminderSearchText = string.Empty;
        RequestFocus(DesktopFocusTarget.ReminderSearch);
    }

    public void RefreshVisibleReminderItems(bool preserveSelection = true)
    {
        var previousSelection = preserveSelection ? SelectedReminder : null;
        var filteredItems = WorkspaceSortHelpers
            .SortReminders(SelectedVehicleReminders.Where(MatchesSearch), SelectedReminderSortOption, ReminderSortDescending)
            .ToList();

        VisibleReminderItems.Clear();
        foreach (var item in filteredItems)
        {
            VisibleReminderItems.Add(item);
        }

        SelectedReminder = previousSelection is not null
            ? VisibleReminderItems.FirstOrDefault(item => string.Equals(item.Id, previousSelection.Id, StringComparison.Ordinal))
            : null;

        SelectedReminder ??= VisibleReminderItems.FirstOrDefault();
        if (SelectedReminder is null)
        {
            SelectedReminderDetail = L("ReminderWorkspace.Detail.Empty");
            Root.NotifyReminderWorkspaceSelectionChanged();
        }

        UpdateSearchSummary();
    }

    partial void OnSelectedReminderChanged(VehicleReminderItemViewModel? value)
    {
        SelectedReminderDetail = value is null
            ? L("ReminderWorkspace.Detail.Empty")
            : string.Join(
                Environment.NewLine,
                LF("ReminderWorkspace.Detail.Title", value.Title),
                LF("ReminderWorkspace.Detail.DueDate", value.DueDate),
                LF("ReminderWorkspace.Detail.Status", value.Status),
                LF("ReminderWorkspace.Detail.Repeat", value.RepeatMode),
                LF("ReminderWorkspace.Detail.Note", Root.FormatWorkspaceValue(value.Note, L("Common.NoNote"))));

        Root.NotifyReminderWorkspaceSelectionChanged();
    }

    partial void OnReminderSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanClearReminderSearch));
        ClearReminderSearchCommand.NotifyCanExecuteChanged();
        RefreshVisibleReminderItems();
    }

    partial void OnSelectedReminderSortOptionChanged(string value)
    {
        Root.HandleReminderWorkspaceSortChanged();
    }

    partial void OnReminderSortDescendingChanged(bool value)
    {
        Root.HandleReminderWorkspaceSortChanged();
    }

    partial void OnIsEditingReminderChanged(bool value)
    {
        if (value)
        {
            ReminderEditorHeading = Root.GetEditingReminderId() is null
                ? L("ReminderEditor.NewTitle")
                : L("ReminderEditor.EditTitle");
        }

        OnPropertyChanged(nameof(IsReminderDetailVisible));
        Root.NotifyReminderWorkspaceEditingChanged();
    }

    private bool MatchesSearch(VehicleReminderItemViewModel item)
    {
        if (string.IsNullOrWhiteSpace(ReminderSearchText))
        {
            return true;
        }

        var query = ReminderSearchText.Trim();
        return Contains(item.Title, query)
            || Contains(item.DueDate, query)
            || Contains(item.Status, query)
            || Contains(item.RepeatMode, query)
            || Contains(item.Note, query)
            || Contains(item.AccessibleLabel, query);
    }

    private void UpdateSearchSummary()
    {
        if (string.IsNullOrWhiteSpace(ReminderSearchText))
        {
            ReminderSearchSummary = LF("ReminderWorkspace.SearchSummary.All", VisibleReminderItems.Count);
            return;
        }

        ReminderSearchSummary = VisibleReminderItems.Count == 0
            ? LF("ReminderWorkspace.SearchSummary.Empty", ReminderSearchText.Trim())
            : LF("ReminderWorkspace.SearchSummary.Filtered", ReminderSearchText.Trim(), VisibleReminderItems.Count);
    }

    private static bool Contains(string value, string query) =>
        value.Contains(query, StringComparison.CurrentCultureIgnoreCase);
}
