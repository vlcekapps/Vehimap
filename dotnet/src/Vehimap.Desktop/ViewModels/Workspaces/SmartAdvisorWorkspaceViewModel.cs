// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class SmartAdvisorWorkspaceViewModel : WorkspaceViewModelBase
{
    public static string AllPrioritiesLabel => L("SmartAdvisor.Filter.AllPriorities");
    public static string AllCategoriesLabel => L("SmartAdvisor.Filter.AllCategories");
    public static string AllVehiclesLabel => L("SmartAdvisor.Filter.AllVehicles");

    public SmartAdvisorWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
        PriorityFilters.Add(AllPrioritiesLabel);
        CategoryFilters.Add(AllCategoriesLabel);
        VehicleFilters.Add(AllVehiclesLabel);
    }

    private string unfilteredSmartAdvisorSummary = L("SmartAdvisor.Summary.Initial");

    [ObservableProperty]
    private string smartAdvisorSummary = L("SmartAdvisor.Summary.Initial");

    [ObservableProperty]
    private string smartAdvisorSearchText = string.Empty;

    [ObservableProperty]
    private string selectedSmartAdvisorPriorityFilter = AllPrioritiesLabel;

    [ObservableProperty]
    private string selectedSmartAdvisorCategoryFilter = AllCategoriesLabel;

    [ObservableProperty]
    private string selectedSmartAdvisorVehicleFilter = AllVehiclesLabel;

    [ObservableProperty]
    private SmartAdvisorItemViewModel? selectedSmartAdvisorItem;

    [ObservableProperty]
    private string selectedSmartAdvisorDetail = L("SmartAdvisor.Detail.Empty");

    public string WindowTitle => Root.SmartAdvisorWindowTitle;

    public ObservableCollection<SmartAdvisorItemViewModel> SmartAdvisorItems { get; } = [];

    public ObservableCollection<SmartAdvisorItemViewModel> VisibleSmartAdvisorItems { get; } = [];

    public ObservableCollection<string> PriorityFilters { get; } = [];

    public ObservableCollection<string> CategoryFilters { get; } = [];

    public ObservableCollection<string> VehicleFilters { get; } = [];

    public bool CanOpenSelectedSmartAdvisorItem => SelectedSmartAdvisorItem is not null;

    public bool CanClearSmartAdvisorFilters =>
        !string.IsNullOrWhiteSpace(SmartAdvisorSearchText)
        || !string.Equals(SelectedSmartAdvisorPriorityFilter, AllPrioritiesLabel, StringComparison.Ordinal)
        || !string.Equals(SelectedSmartAdvisorCategoryFilter, AllCategoriesLabel, StringComparison.Ordinal)
        || !string.Equals(SelectedSmartAdvisorVehicleFilter, AllVehiclesLabel, StringComparison.Ordinal);

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.SmartAdvisorSearch);
    }

    [RelayCommand]
    private void RefreshSmartAdvisor()
    {
        Root.RefreshSmartAdvisorWorkspace();
    }

    [RelayCommand(CanExecute = nameof(CanClearSmartAdvisorFilters))]
    private void ClearSmartAdvisorFilters()
    {
        SmartAdvisorSearchText = string.Empty;
        SelectedSmartAdvisorPriorityFilter = AllPrioritiesLabel;
        SelectedSmartAdvisorCategoryFilter = AllCategoriesLabel;
        SelectedSmartAdvisorVehicleFilter = AllVehiclesLabel;
        RefreshVisibleSmartAdvisorItems();
        RequestFocus(DesktopFocusTarget.SmartAdvisorSearch);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedSmartAdvisorItem))]
    private async Task OpenSelectedSmartAdvisorItemAsync()
    {
        await Root.OpenSmartAdvisorItemAsync(SelectedSmartAdvisorItem).ConfigureAwait(true);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedSmartAdvisorItem))]
    private async Task OpenSelectedSmartAdvisorVehicleAsync()
    {
        await Root.OpenSmartAdvisorVehicleAsync(SelectedSmartAdvisorItem).ConfigureAwait(true);
    }

    internal void ApplyProjection(DesktopSmartAdvisorProjection projection, bool preserveSelection = true)
    {
        unfilteredSmartAdvisorSummary = projection.Summary;
        SmartAdvisorSummary = projection.Summary;

        var previousSelection = preserveSelection ? SelectedSmartAdvisorItem : null;
        SmartAdvisorItems.Clear();
        foreach (var item in projection.Items)
        {
            SmartAdvisorItems.Add(item);
        }

        RebuildFilters();
        RefreshVisibleSmartAdvisorItems(previousSelection);
    }

    public void RefreshVisibleSmartAdvisorItems(bool preserveSelection = true)
    {
        RefreshVisibleSmartAdvisorItems(preserveSelection ? SelectedSmartAdvisorItem : null);
    }

    private void RefreshVisibleSmartAdvisorItems(SmartAdvisorItemViewModel? previousSelection)
    {
        var filteredItems = SmartAdvisorItems
            .Where(MatchesFilters)
            .OrderByDescending(item => item.PriorityRank)
            .ThenBy(item => item.DueDate, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        VisibleSmartAdvisorItems.Clear();
        foreach (var item in filteredItems)
        {
            VisibleSmartAdvisorItems.Add(item);
        }

        SelectedSmartAdvisorItem = previousSelection is not null
            ? VisibleSmartAdvisorItems.FirstOrDefault(item => string.Equals(item.Id, previousSelection.Id, StringComparison.Ordinal))
            : null;

        SelectedSmartAdvisorItem ??= VisibleSmartAdvisorItems.FirstOrDefault();
        UpdateFilteredSummary();
    }

    partial void OnSelectedSmartAdvisorItemChanged(SmartAdvisorItemViewModel? value)
    {
        SelectedSmartAdvisorDetail = value is null
            ? L("SmartAdvisor.Detail.Empty")
            : LF(
                "SmartAdvisor.Detail.Selected",
                value.Priority,
                value.Category,
                value.VehicleName,
                value.Title,
                value.Summary,
                value.Detail,
                value.ActionLabel);

        OnPropertyChanged(nameof(CanOpenSelectedSmartAdvisorItem));
        OpenSelectedSmartAdvisorItemCommand.NotifyCanExecuteChanged();
        OpenSelectedSmartAdvisorVehicleCommand.NotifyCanExecuteChanged();
        Root.NotifySmartAdvisorWorkspaceSelectionChanged();
    }

    partial void OnSmartAdvisorSearchTextChanged(string value)
    {
        NotifyFilterStateChanged();
        RefreshVisibleSmartAdvisorItems();
    }

    partial void OnSelectedSmartAdvisorPriorityFilterChanged(string value)
    {
        NotifyFilterStateChanged();
        RefreshVisibleSmartAdvisorItems();
    }

    partial void OnSelectedSmartAdvisorCategoryFilterChanged(string value)
    {
        NotifyFilterStateChanged();
        RefreshVisibleSmartAdvisorItems();
    }

    partial void OnSelectedSmartAdvisorVehicleFilterChanged(string value)
    {
        NotifyFilterStateChanged();
        RefreshVisibleSmartAdvisorItems();
    }

    private void RebuildFilters()
    {
        RebuildFilter(PriorityFilters, AllPrioritiesLabel, SmartAdvisorItems.Select(item => item.Priority), SelectedSmartAdvisorPriorityFilter, value => SelectedSmartAdvisorPriorityFilter = value);
        RebuildFilter(CategoryFilters, AllCategoriesLabel, SmartAdvisorItems.Select(item => item.Category), SelectedSmartAdvisorCategoryFilter, value => SelectedSmartAdvisorCategoryFilter = value);
        RebuildFilter(VehicleFilters, AllVehiclesLabel, SmartAdvisorItems.Select(item => item.VehicleName), SelectedSmartAdvisorVehicleFilter, value => SelectedSmartAdvisorVehicleFilter = value);
    }

    private static void RebuildFilter(
        ObservableCollection<string> target,
        string allLabel,
        IEnumerable<string> values,
        string currentValue,
        Action<string> setCurrentValue)
    {
        var normalizedValues = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        target.Clear();
        target.Add(allLabel);
        foreach (var value in normalizedValues)
        {
            target.Add(value);
        }

        if (!target.Contains(currentValue))
        {
            setCurrentValue(allLabel);
        }
    }

    private bool MatchesFilters(SmartAdvisorItemViewModel item)
    {
        return MatchesSelected(SelectedSmartAdvisorPriorityFilter, AllPrioritiesLabel, item.Priority)
            && MatchesSelected(SelectedSmartAdvisorCategoryFilter, AllCategoriesLabel, item.Category)
            && MatchesSelected(SelectedSmartAdvisorVehicleFilter, AllVehiclesLabel, item.VehicleName)
            && MatchesSearch(item);
    }

    private bool MatchesSearch(SmartAdvisorItemViewModel item)
    {
        if (string.IsNullOrWhiteSpace(SmartAdvisorSearchText))
        {
            return true;
        }

        var query = SmartAdvisorSearchText.Trim();
        return Contains(item.VehicleName, query)
            || Contains(item.Priority, query)
            || Contains(item.Category, query)
            || Contains(item.Title, query)
            || Contains(item.Summary, query)
            || Contains(item.Detail, query)
            || Contains(item.ActionLabel, query)
            || Contains(item.EntityKind, query);
    }

    private void UpdateFilteredSummary()
    {
        if (!CanClearSmartAdvisorFilters)
        {
            SmartAdvisorSummary = unfilteredSmartAdvisorSummary;
            return;
        }

        SmartAdvisorSummary = VisibleSmartAdvisorItems.Count == 0
            ? L("SmartAdvisor.Summary.FilteredEmpty")
            : LF("SmartAdvisor.Summary.FilteredCount", VisibleSmartAdvisorItems.Count, SmartAdvisorItems.Count);
    }

    private void NotifyFilterStateChanged()
    {
        OnPropertyChanged(nameof(CanClearSmartAdvisorFilters));
        ClearSmartAdvisorFiltersCommand.NotifyCanExecuteChanged();
    }

    private static bool MatchesSelected(string selectedValue, string allLabel, string itemValue) =>
        string.Equals(selectedValue, allLabel, StringComparison.Ordinal)
        || string.Equals(selectedValue, itemValue, StringComparison.CurrentCultureIgnoreCase);

    private static bool Contains(string value, string query) =>
        value.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0;
}
