// SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.Services;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    internal const string VehicleCategoryAllFilterKey = "all";
    internal const string VehicleStatusAllFilterKey = "all";
    internal const string VehicleStatusAttentionFilterKey = "attention";
    internal const string VehicleStatusOverdueFilterKey = "overdue";
    internal const string VehicleStatusMissingGreenCardFilterKey = "missing_green_card";

    internal static string AllVehicleCategoriesLabel => LO("VehicleList.FilterOption.AllCategories");
    internal static string AllVehicleStatusFilterLabel => LO("VehicleList.FilterOption.AllVehicles");
    internal static string AttentionVehicleStatusFilterLabel => LO("VehicleList.FilterOption.Attention");
    internal static string OverdueVehicleStatusFilterLabel => LO("VehicleList.FilterOption.Overdue");
    internal static string MissingGreenVehicleStatusFilterLabel => LO("VehicleList.FilterOption.MissingGreenCard");
    private static LocalizedOptionViewModel AllVehicleCategoriesOption => new(VehicleCategoryAllFilterKey, AllVehicleCategoriesLabel);
    private static LocalizedOptionViewModel AllVehicleStatusFilterOption => new(VehicleStatusAllFilterKey, AllVehicleStatusFilterLabel);
    private static LocalizedOptionViewModel AttentionVehicleStatusFilterOption => new(VehicleStatusAttentionFilterKey, AttentionVehicleStatusFilterLabel);
    private static LocalizedOptionViewModel OverdueVehicleStatusFilterOption => new(VehicleStatusOverdueFilterKey, OverdueVehicleStatusFilterLabel);
    private static LocalizedOptionViewModel MissingGreenVehicleStatusFilterOption => new(VehicleStatusMissingGreenCardFilterKey, MissingGreenVehicleStatusFilterLabel);

    private const string VehicleListCategoryFilterSettingKey = "vehicle_category_filter";
    private const string VehicleListStatusFilterSettingKey = "vehicle_status_filter";
    private const string VehicleListHideInactiveSettingKey = "hide_inactive_vehicles";

    private bool _suppressVehicleListFilterRefresh;

    [ObservableProperty]
    private string vehicleListSummary = LO("VehicleList.Summary.NotLoaded");

    [ObservableProperty]
    private string vehicleSearchText = string.Empty;

    [ObservableProperty]
    private LocalizedOptionViewModel selectedVehicleCategoryFilter = AllVehicleCategoriesOption;

    [ObservableProperty]
    private LocalizedOptionViewModel selectedVehicleStatusFilter = AllVehicleStatusFilterOption;

    [ObservableProperty]
    private bool hideInactiveVehicles;

    public IReadOnlyList<LocalizedOptionViewModel> VehicleCategoryFilters =>
    [
        AllVehicleCategoriesOption,
        .. LegacyKnownValues.Categories.Select(category => new LocalizedOptionViewModel(category, LegacyKnownValueDisplayService.FormatCategory(category, DesktopLocalization.Localizer)))
    ];

    public IReadOnlyList<LocalizedOptionViewModel> VehicleStatusFilters =>
    [
        AllVehicleStatusFilterOption,
        AttentionVehicleStatusFilterOption,
        OverdueVehicleStatusFilterOption,
        MissingGreenVehicleStatusFilterOption
    ];

    public bool CanClearVehicleFilters =>
        CanUseVehicleList
        && (!string.IsNullOrWhiteSpace(VehicleSearchText)
            || !IsAllVehicleCategoryFilter(SelectedVehicleCategoryFilter.Value)
            || !IsAllVehicleStatusFilter(SelectedVehicleStatusFilter.Value)
            || HideInactiveVehicles);

    partial void OnVehicleSearchTextChanged(string value)
    {
        HandleVehicleListFiltersChanged();
    }

    partial void OnSelectedVehicleCategoryFilterChanged(LocalizedOptionViewModel value)
    {
        HandleVehicleListFiltersChanged(persistVehicleListPreferences: true);
    }

    partial void OnSelectedVehicleStatusFilterChanged(LocalizedOptionViewModel value)
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
            SelectedVehicleCategoryFilter = AllVehicleCategoriesOption;
            SelectedVehicleStatusFilter = AllVehicleStatusFilterOption;
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
            SelectedVehicleCategoryFilter = NormalizeVehicleCategoryFilter(_dataSet.Settings.GetValue("app", VehicleListCategoryFilterSettingKey, VehicleCategoryAllFilterKey));
            SelectedVehicleStatusFilter = NormalizeVehicleStatusFilter(_dataSet.Settings.GetValue("app", VehicleListStatusFilterSettingKey, VehicleStatusAllFilterKey));
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
        var categoryFilter = NormalizeVehicleCategoryFilter(SelectedVehicleCategoryFilter.Value).Value;
        var statusFilter = NormalizeVehicleStatusFilter(SelectedVehicleStatusFilter.Value).Value;
        PersistPreferenceSettingsAsync(
            settings =>
            {
                settings.SetValue("app", VehicleListHideInactiveSettingKey, hideInactiveValue);
                settings.SetValue("app", VehicleListCategoryFilterSettingKey, categoryFilter);
                settings.SetValue("app", VehicleListStatusFilterSettingKey, statusFilter);
            },
            LO("VehicleList.Persistence.FiltersFailed"));
    }

    private LocalizedOptionViewModel NormalizeVehicleCategoryFilter(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? VehicleCategoryAllFilterKey : value.Trim();
        if (IsAllVehicleCategoryFilter(normalized))
        {
            return AllVehicleCategoriesOption;
        }

        var category = LegacyKnownValues.Categories.FirstOrDefault(item =>
            string.Equals(item, normalized, StringComparison.Ordinal)
            || string.Equals(LegacyKnownValueDisplayService.FormatCategory(item, DesktopLocalization.Localizer), normalized, StringComparison.OrdinalIgnoreCase));

        return category is not null
            ? new LocalizedOptionViewModel(category, LegacyKnownValueDisplayService.FormatCategory(category, DesktopLocalization.Localizer))
            : AllVehicleCategoriesOption;
    }

    private LocalizedOptionViewModel NormalizeVehicleStatusFilter(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? VehicleStatusAllFilterKey : value.Trim();
        if (IsAllVehicleStatusFilter(normalized))
        {
            return AllVehicleStatusFilterOption;
        }

        if (IsAttentionVehicleStatusFilter(normalized))
        {
            return AttentionVehicleStatusFilterOption;
        }

        if (IsOverdueVehicleStatusFilter(normalized))
        {
            return OverdueVehicleStatusFilterOption;
        }

        if (IsMissingGreenCardVehicleStatusFilter(normalized))
        {
            return MissingGreenVehicleStatusFilterOption;
        }

        return AllVehicleStatusFilterOption;
    }

    internal static bool IsAllVehicleCategoryFilter(string? value) =>
        LocalizedCompatibilityAliases.MatchesStableValueOrResource(value, VehicleCategoryAllFilterKey, "VehicleList.FilterOption.AllCategories");

    internal static bool IsAllVehicleStatusFilter(string? value) =>
        LocalizedCompatibilityAliases.MatchesStableValueOrResource(value, VehicleStatusAllFilterKey, "VehicleList.FilterOption.AllVehicles");

    internal static bool IsAttentionVehicleStatusFilter(string? value) =>
        LocalizedCompatibilityAliases.MatchesStableValueOrResource(value, VehicleStatusAttentionFilterKey, "VehicleList.FilterOption.Attention");

    internal static bool IsOverdueVehicleStatusFilter(string? value) =>
        LocalizedCompatibilityAliases.MatchesStableValueOrResource(value, VehicleStatusOverdueFilterKey, "VehicleList.FilterOption.Overdue");

    internal static bool IsMissingGreenCardVehicleStatusFilter(string? value) =>
        LocalizedCompatibilityAliases.MatchesStableValueOrResource(value, VehicleStatusMissingGreenCardFilterKey, "VehicleList.FilterOption.MissingGreenCard");

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
                SelectedVehicleCategoryFilter.Value,
                SelectedVehicleStatusFilter.Value,
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
