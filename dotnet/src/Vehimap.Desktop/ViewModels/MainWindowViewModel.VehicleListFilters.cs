using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Desktop.Services;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    internal const string AllVehicleCategoriesLabel = "Všechny kategorie";
    internal const string AllVehicleStatusFilterLabel = "Všechna vozidla";
    internal const string AttentionVehicleStatusFilterLabel = "Jen s blížícím se termínem";
    internal const string OverdueVehicleStatusFilterLabel = "Jen po termínu";
    internal const string MissingGreenVehicleStatusFilterLabel = "Jen bez zelené karty";

    private bool _suppressVehicleListFilterRefresh;

    [ObservableProperty]
    private string vehicleListSummary = "Seznam vozidel: zatím nebyla načtena žádná data.";

    [ObservableProperty]
    private string vehicleSearchText = string.Empty;

    [ObservableProperty]
    private string selectedVehicleCategoryFilter = AllVehicleCategoriesLabel;

    [ObservableProperty]
    private string selectedVehicleStatusFilter = AllVehicleStatusFilterLabel;

    [ObservableProperty]
    private bool hideInactiveVehicles;

    public IReadOnlyList<string> VehicleCategoryFilters { get; } =
    [
        AllVehicleCategoriesLabel,
        .. LegacyKnownValues.Categories
    ];

    public IReadOnlyList<string> VehicleStatusFilters { get; } =
    [
        AllVehicleStatusFilterLabel,
        AttentionVehicleStatusFilterLabel,
        OverdueVehicleStatusFilterLabel,
        MissingGreenVehicleStatusFilterLabel
    ];

    public bool CanClearVehicleFilters =>
        !string.IsNullOrWhiteSpace(VehicleSearchText)
        || !string.Equals(SelectedVehicleCategoryFilter, AllVehicleCategoriesLabel, StringComparison.Ordinal)
        || !string.Equals(SelectedVehicleStatusFilter, AllVehicleStatusFilterLabel, StringComparison.Ordinal)
        || HideInactiveVehicles;

    partial void OnVehicleSearchTextChanged(string value)
    {
        HandleVehicleListFiltersChanged();
    }

    partial void OnSelectedVehicleCategoryFilterChanged(string value)
    {
        HandleVehicleListFiltersChanged();
    }

    partial void OnSelectedVehicleStatusFilterChanged(string value)
    {
        HandleVehicleListFiltersChanged();
    }

    partial void OnHideInactiveVehiclesChanged(bool value)
    {
        HandleVehicleListFiltersChanged(persistHideInactivePreference: true);
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
        PersistHideInactiveVehiclePreferenceAsync();
    }

    private void ApplyVehicleListFilterPreferences()
    {
        _suppressVehicleListFilterRefresh = true;
        try
        {
            HideInactiveVehicles = GetHideInactiveVehiclesEnabled();

            if (string.IsNullOrWhiteSpace(SelectedVehicleCategoryFilter))
            {
                SelectedVehicleCategoryFilter = AllVehicleCategoriesLabel;
            }

            if (string.IsNullOrWhiteSpace(SelectedVehicleStatusFilter))
            {
                SelectedVehicleStatusFilter = AllVehicleStatusFilterLabel;
            }
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
            _dataSet.Settings.GetValue("app", "hide_inactive_vehicles", "0").Trim(),
            "1",
            StringComparison.Ordinal);
    }

    private void HandleVehicleListFiltersChanged(bool persistHideInactivePreference = false)
    {
        if (_suppressVehicleListFilterRefresh)
        {
            return;
        }

        RefreshVehicleList();
        NotifyVehicleListFilterStateChanged();
        if (persistHideInactivePreference)
        {
            PersistHideInactiveVehiclePreferenceAsync();
        }
    }

    private void NotifyVehicleListFilterStateChanged()
    {
        OnPropertyChanged(nameof(CanClearVehicleFilters));
        ClearVehicleFiltersCommand.NotifyCanExecuteChanged();
    }

    private void PersistHideInactiveVehiclePreferenceAsync()
    {
        if (!_session.IsLoaded)
        {
            return;
        }

        _dataSet.Settings.SetValue("app", "hide_inactive_vehicles", HideInactiveVehicles ? "1" : "0");
        _ = PersistHideInactiveVehiclePreferenceCoreAsync();
    }

    private async Task PersistHideInactiveVehiclePreferenceCoreAsync()
    {
        try
        {
            await _session.PersistAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ShellStatus = $"Nepodařilo se uložit volbu skrytí neaktivních vozidel: {ex.Message}";
        }
    }

    private void RefreshVehicleList(string? preferredVehicleId = null)
    {
        if (!_session.IsLoaded)
        {
            Vehicles.Clear();
            VehicleListSummary = "Seznam vozidel: zatím nebyla načtena žádná data.";
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
