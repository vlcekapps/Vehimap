// SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Desktop.Services;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private const string LegacyAllVehicleCategoriesLabel = "Všechny kategorie";
    private const string LegacyAllVehicleStatusFilterLabel = "Všechna vozidla";
    private const string LegacyAttentionVehicleStatusFilterLabel = "Jen s blížícím se termínem";
    private const string LegacyOverdueVehicleStatusFilterLabel = "Jen po termínu";
    private const string LegacyMissingGreenVehicleStatusFilterLabel = "Jen bez zelené karty";
    private const string EnglishAllVehicleCategoriesLabel = "All categories";
    private const string EnglishAllVehicleStatusFilterLabel = "All vehicles";
    private const string EnglishAttentionVehicleStatusFilterLabel = "Only vehicles needing review";
    private const string EnglishOverdueVehicleStatusFilterLabel = "Only overdue vehicles";
    private const string EnglishMissingGreenVehicleStatusFilterLabel = "Only missing green card";

    internal static string AllVehicleCategoriesLabel => LO("VehicleList.FilterOption.AllCategories");
    internal static string AllVehicleStatusFilterLabel => LO("VehicleList.FilterOption.AllVehicles");
    internal static string AttentionVehicleStatusFilterLabel => LO("VehicleList.FilterOption.Attention");
    internal static string OverdueVehicleStatusFilterLabel => LO("VehicleList.FilterOption.Overdue");
    internal static string MissingGreenVehicleStatusFilterLabel => LO("VehicleList.FilterOption.MissingGreenCard");

    private const string VehicleListCategoryFilterSettingKey = "vehicle_category_filter";
    private const string VehicleListStatusFilterSettingKey = "vehicle_status_filter";
    private const string VehicleListHideInactiveSettingKey = "hide_inactive_vehicles";

    private bool _suppressVehicleListFilterRefresh;

    [ObservableProperty]
    private string vehicleListSummary = LO("VehicleList.Summary.NotLoaded");

    [ObservableProperty]
    private string vehicleSearchText = string.Empty;

    [ObservableProperty]
    private string selectedVehicleCategoryFilter = AllVehicleCategoriesLabel;

    [ObservableProperty]
    private string selectedVehicleStatusFilter = AllVehicleStatusFilterLabel;

    [ObservableProperty]
    private bool hideInactiveVehicles;

    public IReadOnlyList<string> VehicleCategoryFilters =>
    [
        AllVehicleCategoriesLabel,
        .. LegacyKnownValues.Categories
    ];

    public IReadOnlyList<string> VehicleStatusFilters =>
    [
        AllVehicleStatusFilterLabel,
        AttentionVehicleStatusFilterLabel,
        OverdueVehicleStatusFilterLabel,
        MissingGreenVehicleStatusFilterLabel
    ];

    public bool CanClearVehicleFilters =>
        CanUseVehicleList
        && (!string.IsNullOrWhiteSpace(VehicleSearchText)
            || !IsAllVehicleCategoryFilter(SelectedVehicleCategoryFilter)
            || !IsAllVehicleStatusFilter(SelectedVehicleStatusFilter)
            || HideInactiveVehicles);

    partial void OnVehicleSearchTextChanged(string value)
    {
        HandleVehicleListFiltersChanged();
    }

    partial void OnSelectedVehicleCategoryFilterChanged(string value)
    {
        HandleVehicleListFiltersChanged(persistVehicleListPreferences: true);
    }

    partial void OnSelectedVehicleStatusFilterChanged(string value)
    {
        HandleVehicleListFiltersChanged(persistVehicleListPreferences: true);
    }

    partial void OnHideInactiveVehiclesChanged(bool value)
    {
        HandleVehicleListFiltersChanged(persistVehicleListPreferences: true);
    }

    [RelayCommand(CanExecute = nameof(CanClearVehicleFilters))]
    private void ClearVehicleFilters()
    {
        _suppressVehicleListFilterRefresh = true;
        try
        {
            VehicleSearchText = string.Empty;
            SelectedVehicleCategoryFilter = AllVehicleCategoriesLabel;
            SelectedVehicleStatusFilter = AllVehicleStatusFilterLabel;
            HideInactiveVehicles = false;
        }
        finally
        {
            _suppressVehicleListFilterRefresh = false;
        }

        RefreshVehicleList();
        NotifyVehicleListFilterStateChanged();
        PersistVehicleListFilterPreferencesAsync();
        ShellStatus = LO("VehicleList.Status.FiltersCleared");
        RequestFocus(DesktopFocusTarget.VehicleSearch);
    }

    private void ApplyVehicleListFilterPreferences()
    {
        _suppressVehicleListFilterRefresh = true;
        try
        {
            HideInactiveVehicles = GetHideInactiveVehiclesEnabled();
            SelectedVehicleCategoryFilter = NormalizeVehicleCategoryFilter(_dataSet.Settings.GetValue("app", VehicleListCategoryFilterSettingKey, AllVehicleCategoriesLabel));
            SelectedVehicleStatusFilter = NormalizeVehicleStatusFilter(_dataSet.Settings.GetValue("app", VehicleListStatusFilterSettingKey, AllVehicleStatusFilterLabel));
        }
        finally
        {
            _suppressVehicleListFilterRefresh = false;
        }

        NotifyVehicleListFilterStateChanged();
    }

    private bool GetHideInactiveVehiclesEnabled()
    {
        return string.Equals(
            _dataSet.Settings.GetValue("app", VehicleListHideInactiveSettingKey, "0").Trim(),
            "1",
            StringComparison.Ordinal);
    }

    private void HandleVehicleListFiltersChanged(bool persistVehicleListPreferences = false)
    {
        if (_suppressVehicleListFilterRefresh)
        {
            return;
        }

        RefreshVehicleList();
        NotifyVehicleListFilterStateChanged();
        if (persistVehicleListPreferences)
        {
            PersistVehicleListFilterPreferencesAsync();
        }
    }

    private void NotifyVehicleListFilterStateChanged()
    {
        OnPropertyChanged(nameof(CanClearVehicleFilters));
        ClearVehicleFiltersCommand.NotifyCanExecuteChanged();
    }

    private void PersistVehicleListFilterPreferencesAsync()
    {
        if (!_session.IsLoaded)
        {
            return;
        }

        var hideInactiveValue = HideInactiveVehicles ? "1" : "0";
        var categoryFilter = NormalizeVehicleCategoryFilter(SelectedVehicleCategoryFilter);
        var statusFilter = NormalizeVehicleStatusFilter(SelectedVehicleStatusFilter);
        PersistPreferenceSettingsAsync(
            settings =>
            {
                settings.SetValue("app", VehicleListHideInactiveSettingKey, hideInactiveValue);
                settings.SetValue("app", VehicleListCategoryFilterSettingKey, categoryFilter);
                settings.SetValue("app", VehicleListStatusFilterSettingKey, statusFilter);
            },
            LO("VehicleList.Persistence.FiltersFailed"));
    }

    private string NormalizeVehicleCategoryFilter(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? AllVehicleCategoriesLabel : value.Trim();
        if (IsAllVehicleCategoryFilter(normalized))
        {
            return AllVehicleCategoriesLabel;
        }

        return VehicleCategoryFilters.Any(item => string.Equals(item, normalized, StringComparison.Ordinal))
            ? normalized
            : AllVehicleCategoriesLabel;
    }

    private string NormalizeVehicleStatusFilter(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? AllVehicleStatusFilterLabel : value.Trim();
        if (IsAllVehicleStatusFilter(normalized))
        {
            return AllVehicleStatusFilterLabel;
        }

        if (IsAttentionVehicleStatusFilter(normalized))
        {
            return AttentionVehicleStatusFilterLabel;
        }

        if (IsOverdueVehicleStatusFilter(normalized))
        {
            return OverdueVehicleStatusFilterLabel;
        }

        if (IsMissingGreenCardVehicleStatusFilter(normalized))
        {
            return MissingGreenVehicleStatusFilterLabel;
        }

        return VehicleStatusFilters.Any(item => string.Equals(item, normalized, StringComparison.Ordinal))
            ? normalized
            : AllVehicleStatusFilterLabel;
    }

    internal static bool IsAllVehicleCategoryFilter(string? value) =>
        MatchesVehicleFilterLabel(value, AllVehicleCategoriesLabel, LegacyAllVehicleCategoriesLabel, EnglishAllVehicleCategoriesLabel);

    internal static bool IsAllVehicleStatusFilter(string? value) =>
        MatchesVehicleFilterLabel(value, AllVehicleStatusFilterLabel, LegacyAllVehicleStatusFilterLabel, EnglishAllVehicleStatusFilterLabel);

    internal static bool IsAttentionVehicleStatusFilter(string? value) =>
        MatchesVehicleFilterLabel(value, AttentionVehicleStatusFilterLabel, LegacyAttentionVehicleStatusFilterLabel, EnglishAttentionVehicleStatusFilterLabel);

    internal static bool IsOverdueVehicleStatusFilter(string? value) =>
        MatchesVehicleFilterLabel(value, OverdueVehicleStatusFilterLabel, LegacyOverdueVehicleStatusFilterLabel, EnglishOverdueVehicleStatusFilterLabel);

    internal static bool IsMissingGreenCardVehicleStatusFilter(string? value) =>
        MatchesVehicleFilterLabel(value, MissingGreenVehicleStatusFilterLabel, LegacyMissingGreenVehicleStatusFilterLabel, EnglishMissingGreenVehicleStatusFilterLabel);

    private static bool MatchesVehicleFilterLabel(string? value, params string[] candidates)
    {
        var normalized = (value ?? string.Empty).Trim();
        return candidates.Any(candidate => string.Equals(normalized, candidate, StringComparison.OrdinalIgnoreCase));
    }

    private void RefreshVehicleList(string? preferredVehicleId = null)
    {
        if (!_session.IsLoaded)
        {
            Vehicles.Clear();
            VehicleListSummary = LO("VehicleList.Summary.NotLoaded");
            return;
        }

        var projection = _projectionService.BuildVehicleList(
            _dataSet,
            _metaByVehicleId,
            _auditItems,
            _timelineService,
            new DesktopVehicleListFilters(
                VehicleSearchText,
                SelectedVehicleCategoryFilter,
                SelectedVehicleStatusFilter,
                HideInactiveVehicles),
            DateOnly.FromDateTime(DateTime.Today));

        Vehicles.Clear();
        foreach (var vehicle in projection.Items)
        {
            Vehicles.Add(vehicle);
        }

        VehicleListSummary = projection.Summary;

        var selectionId = preferredVehicleId ?? SelectedVehicle?.Id;
        var nextSelection = FindById(Vehicles, item => item.Id, selectionId ?? string.Empty);
        ReplaceSelectedVehicle(nextSelection);
    }

    private void ReplaceSelectedVehicle(VehicleListItemViewModel? nextSelection)
    {
        if (SelectedVehicle is null && nextSelection is null)
        {
            return;
        }

        if (SelectedVehicle is not null
            && nextSelection is not null
            && string.Equals(SelectedVehicle.Id, nextSelection.Id, StringComparison.Ordinal))
        {
            SelectedVehicle = null;
        }

        SelectedVehicle = nextSelection;
    }
}
