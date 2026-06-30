using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class HistoryWorkspaceViewModel : WorkspaceViewModelBase
{
    public HistoryWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.HistoryWindowTitle;
    public ObservableCollection<VehicleHistoryItemViewModel> SelectedVehicleHistory { get; } = [];
    public ObservableCollection<VehicleHistoryItemViewModel> VisibleHistoryItems { get; } = [];

    [ObservableProperty]
    private string historySummary = L("HistoryWorkspace.Summary.Initial");

    [ObservableProperty]
    private string historySearchText = string.Empty;

    [ObservableProperty]
    private string historySearchSummary = L("HistoryWorkspace.SearchSummary.Initial");

    [ObservableProperty]
    private string selectedHistorySortOption = WorkspaceSortHelpers.DateSortLabel;

    [ObservableProperty]
    private bool historySortDescending = true;

    public IReadOnlyList<string> HistorySortOptions => WorkspaceSortHelpers.HistorySortOptions;

    public bool CanClearHistorySearch => !string.IsNullOrWhiteSpace(HistorySearchText);

    [ObservableProperty]
    private VehicleHistoryItemViewModel? selectedHistory;

    [ObservableProperty]
    private string selectedHistoryDetail = L("HistoryWorkspace.Detail.Empty");

    [ObservableProperty]
    private string historyPanelHeading = L("HistoryWorkspace.PanelHeading");

    [ObservableProperty]
    private string historyEditorHeading = L("HistoryEditor.NewTitle");

    [ObservableProperty]
    private bool isEditingHistory;

    [ObservableProperty]
    private string historyEditorStatus = string.Empty;

    [ObservableProperty]
    private string historyEditorDate = string.Empty;

    [ObservableProperty]
    private string historyEditorType = string.Empty;

    [ObservableProperty]
    private string historyEditorOdometer = string.Empty;

    public string HistoryEditorOdometerLabel => LF("HistoryEditor.OdometerLabel", Root.CurrentDistanceUnitLabel);

    public string HistoryEditorOdometerName => LF("HistoryEditor.OdometerName", Root.CurrentDistanceUnitLabel);

    public string HistoryEditorOdometerHelp => LF("HistoryEditor.OdometerHelp", Root.CurrentDistanceUnitLabel);

    [ObservableProperty]
    private string historyEditorCost = string.Empty;

    [ObservableProperty]
    private string historyEditorNote = string.Empty;

    public bool IsHistoryDetailVisible => !IsEditingHistory;

    public ICommand CreateHistoryCommand => Root.CreateHistoryCommand;
    public ICommand EditSelectedHistoryCommand => Root.EditSelectedHistoryCommand;
    public ICommand DeleteSelectedHistoryCommand => Root.DeleteSelectedHistoryCommand;
    public ICommand SaveHistoryCommand => Root.SaveHistoryCommand;
    public ICommand CancelHistoryEditCommand => Root.CancelHistoryEditCommand;

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.HistorySearch);
    }

    [RelayCommand(CanExecute = nameof(CanClearHistorySearch))]
    private void ClearHistorySearch()
    {
        HistorySearchText = string.Empty;
        RequestFocus(DesktopFocusTarget.HistorySearch);
    }

    public void RefreshVisibleHistoryItems(bool preserveSelection = true)
    {
        var previousSelection = preserveSelection ? SelectedHistory : null;
        var filteredItems = WorkspaceSortHelpers
            .SortHistory(SelectedVehicleHistory.Where(MatchesSearch), SelectedHistorySortOption, HistorySortDescending)
            .ToList();

        VisibleHistoryItems.Clear();
        foreach (var item in filteredItems)
        {
            VisibleHistoryItems.Add(item);
        }

        SelectedHistory = previousSelection is not null
            ? VisibleHistoryItems.FirstOrDefault(item => string.Equals(item.Id, previousSelection.Id, StringComparison.Ordinal))
            : null;

        SelectedHistory ??= VisibleHistoryItems.FirstOrDefault();
        if (SelectedHistory is null)
        {
            SelectedHistoryDetail = L("HistoryWorkspace.Detail.Empty");
            Root.NotifyHistoryWorkspaceSelectionChanged();
        }

        UpdateSearchSummary();
    }

    partial void OnSelectedHistoryChanged(VehicleHistoryItemViewModel? value)
    {
        SelectedHistoryDetail = value is null
            ? L("HistoryWorkspace.Detail.Empty")
            : string.Join(
                Environment.NewLine,
                LF("HistoryWorkspace.Detail.Date", value.Date),
                LF("HistoryWorkspace.Detail.EventType", value.EventType),
                LF("HistoryWorkspace.Detail.Odometer", value.Odometer),
                LF("HistoryWorkspace.Detail.Cost", value.Cost),
                LF("HistoryWorkspace.Detail.Note", Root.FormatWorkspaceValue(value.Note, L("Common.NoNote"))));

        Root.NotifyHistoryWorkspaceSelectionChanged();
    }

    partial void OnHistorySearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanClearHistorySearch));
        ClearHistorySearchCommand.NotifyCanExecuteChanged();
        RefreshVisibleHistoryItems();
    }

    partial void OnSelectedHistorySortOptionChanged(string value)
    {
        Root.HandleHistoryWorkspaceSortChanged();
    }

    partial void OnHistorySortDescendingChanged(bool value)
    {
        Root.HandleHistoryWorkspaceSortChanged();
    }

    partial void OnIsEditingHistoryChanged(bool value)
    {
        if (value)
        {
            HistoryEditorHeading = Root.GetEditingHistoryId() is null
                ? L("HistoryEditor.NewTitle")
                : L("HistoryEditor.EditTitle");
            NotifyUnitMetadataChanged();
        }

        OnPropertyChanged(nameof(IsHistoryDetailVisible));
        Root.NotifyHistoryWorkspaceEditingChanged();
    }

    private bool MatchesSearch(VehicleHistoryItemViewModel item)
    {
        if (string.IsNullOrWhiteSpace(HistorySearchText))
        {
            return true;
        }

        var query = HistorySearchText.Trim();
        return Contains(item.Date, query)
            || Contains(item.EventType, query)
            || Contains(item.Odometer, query)
            || Contains(item.Cost, query)
            || Contains(item.Note, query)
            || Contains(item.AccessibleLabel, query);
    }

    private void UpdateSearchSummary()
    {
        if (string.IsNullOrWhiteSpace(HistorySearchText))
        {
            HistorySearchSummary = LF("HistoryWorkspace.SearchSummary.All", VisibleHistoryItems.Count);
            return;
        }

        HistorySearchSummary = VisibleHistoryItems.Count == 0
            ? LF("HistoryWorkspace.SearchSummary.Empty", HistorySearchText.Trim())
            : LF("HistoryWorkspace.SearchSummary.Filtered", HistorySearchText.Trim(), VisibleHistoryItems.Count);
    }

    private static bool Contains(string value, string query) =>
        value.Contains(query, StringComparison.CurrentCultureIgnoreCase);

    internal void NotifyUnitMetadataChanged()
    {
        OnPropertyChanged(nameof(HistoryEditorOdometerLabel));
        OnPropertyChanged(nameof(HistoryEditorOdometerName));
        OnPropertyChanged(nameof(HistoryEditorOdometerHelp));
    }
}
