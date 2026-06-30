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
    private string historySummary = "Historie vybraného vozidla se zobrazí po výběru vozidla.";

    [ObservableProperty]
    private string historySearchText = string.Empty;

    [ObservableProperty]
    private string historySearchSummary = "Ctrl+F přesune fokus do hledání historie.";

    [ObservableProperty]
    private string selectedHistorySortOption = WorkspaceSortHelpers.DateSortLabel;

    [ObservableProperty]
    private bool historySortDescending = true;

    public IReadOnlyList<string> HistorySortOptions => WorkspaceSortHelpers.HistorySortOptions;

    public bool CanClearHistorySearch => !string.IsNullOrWhiteSpace(HistorySearchText);

    [ObservableProperty]
    private VehicleHistoryItemViewModel? selectedHistory;

    [ObservableProperty]
    private string selectedHistoryDetail = "Vyberte historický záznam a zobrazí se detail položky.";

    [ObservableProperty]
    private string historyPanelHeading = "Detail historie";

    [ObservableProperty]
    private string historyEditorHeading = "Nový historický záznam";

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

    public string HistoryEditorOdometerLabel => $"Tachometr ({Root.CurrentDistanceUnitLabel})";

    public string HistoryEditorOdometerName => $"Tachometr historického záznamu v {Root.CurrentDistanceUnitLabel}";

    public string HistoryEditorOdometerHelp => $"Zadejte stav tachometru v {Root.CurrentDistanceUnitLabel}. Vehimap hodnotu uloží interně v kilometrech.";

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
            SelectedHistoryDetail = "Vyberte historický záznam a zobrazí se detail položky.";
            Root.NotifyHistoryWorkspaceSelectionChanged();
        }

        UpdateSearchSummary();
    }

    partial void OnSelectedHistoryChanged(VehicleHistoryItemViewModel? value)
    {
        SelectedHistoryDetail = value is null
            ? "Vyberte historický záznam a zobrazí se detail položky."
            : $"Datum: {value.Date}\nTyp události: {value.EventType}\nTachometr: {value.Odometer}\nCena: {value.Cost}\nPoznámka: {Root.FormatWorkspaceValue(value.Note, "bez poznámky")}";

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
                ? "Nový historický záznam"
                : "Upravit historický záznam";
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
            HistorySearchSummary = $"Zobrazeno {VisibleHistoryItems.Count} historických záznamů. Ctrl+F přesune fokus do hledání.";
            return;
        }

        HistorySearchSummary = VisibleHistoryItems.Count == 0
            ? $"Hledání „{HistorySearchText.Trim()}“ nenašlo v historii žádný záznam."
            : $"Hledání „{HistorySearchText.Trim()}“ našlo {VisibleHistoryItems.Count} historických záznamů.";
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
