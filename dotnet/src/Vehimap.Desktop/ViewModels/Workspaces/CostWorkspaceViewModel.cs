// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class CostWorkspaceViewModel : WorkspaceViewModelBase
{
    public CostWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    [ObservableProperty]
    private string costSummary = string.Empty;

    [ObservableProperty]
    private string costComparison = string.Empty;

    [ObservableProperty]
    private string selectedCostPeriodPreset = string.Empty;

    [ObservableProperty]
    private string costPeriodStartText = string.Empty;

    [ObservableProperty]
    private string costPeriodEndText = string.Empty;

    [ObservableProperty]
    private string costPeriodStatus = L("CostWorkspace.PeriodStatus.Initial");

    [ObservableProperty]
    private string costExportStatus = L("CostWorkspace.ExportStatus.Initial");

    [ObservableProperty]
    private CostVehicleItemViewModel? selectedDashboardCostVehicle;

    [ObservableProperty]
    private string selectedCostVehicleDetail = L("CostWorkspace.Detail.Empty");

    public string WindowTitle => Root.CostWindowTitle;

    public ObservableCollection<CostVehicleItemViewModel> CostVehicles { get; } = [];

    public ObservableCollection<CostVehicleItemViewModel> VisibleCostVehicles { get; } = [];

    public IReadOnlyList<string> CostPeriodPresets { get; } =
    [
        L("CostPeriod.YearToDate"),
        L("CostPeriod.Last30Days"),
        L("CostPeriod.Last90Days"),
        L("CostPeriod.CurrentYear"),
        L("CostPeriod.PreviousYear"),
        L("CostPeriod.Custom")
    ];

    public bool CanUseSelectedCostVehicle => SelectedDashboardCostVehicle is not null;
    public bool CanClearCostSearch => !string.IsNullOrWhiteSpace(CostSearchText);

    [ObservableProperty]
    private string costSearchText = string.Empty;

    [ObservableProperty]
    private string costSearchSummary = L("CostWorkspace.SearchSummary.Initial");

    public ICommand OpenSelectedDashboardCostVehicleCommand => Root.OpenSelectedDashboardCostVehicleCommand;
    public ICommand OpenSelectedCostVehicleCommand => Root.OpenSelectedDashboardCostVehicleCommand;
    public IAsyncRelayCommand ExportFleetCostSummaryCommand => Root.ExportFleetCostSummaryCommand;
    public IAsyncRelayCommand ExportSelectedVehicleCostDetailCommand => Root.ExportSelectedVehicleCostDetailCommand;
    public IAsyncRelayCommand ExportSelectedVehicleCostReportCommand => Root.ExportSelectedVehicleCostReportCommand;

    [RelayCommand]
    private void ApplyCostPeriod()
    {
        Root.ApplyCostPeriodFromWorkspace();
    }

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.CostSearch);
    }

    [RelayCommand]
    private void RefreshCost()
    {
        Root.RefreshCostWorkspace();
    }

    [RelayCommand(CanExecute = nameof(CanClearCostSearch))]
    private void ClearCostSearch()
    {
        CostSearchText = string.Empty;
        RequestFocus(DesktopFocusTarget.CostSearch);
    }

    [RelayCommand(CanExecute = nameof(CanUseSelectedCostVehicle))]
    private void FocusSelectedCostDetail()
    {
        RequestFocus(DesktopFocusTarget.CostDetail);
    }

    [RelayCommand(CanExecute = nameof(CanUseSelectedCostVehicle))]
    private async Task EditSelectedCostVehicleAsync()
    {
        await Root.EditSelectedCostVehicleFromCostsAsync(SelectedDashboardCostVehicle).ConfigureAwait(true);
    }

    public void RefreshVisibleCostVehicles(bool preserveSelection = true)
    {
        var previousSelection = preserveSelection ? SelectedDashboardCostVehicle : null;
        var filteredItems = CostVehicles
            .Where(MatchesSearch)
            .ToList();

        VisibleCostVehicles.Clear();
        foreach (var item in filteredItems)
        {
            VisibleCostVehicles.Add(item);
        }

        SelectedDashboardCostVehicle = previousSelection is not null
            ? VisibleCostVehicles.FirstOrDefault(item => string.Equals(item.VehicleId, previousSelection.VehicleId, StringComparison.Ordinal))
            : null;

        SelectedDashboardCostVehicle ??= VisibleCostVehicles.FirstOrDefault();
        if (SelectedDashboardCostVehicle is null)
        {
            SelectedCostVehicleDetail = L("CostWorkspace.Detail.Empty");
            Root.NotifyCostWorkspaceSelectionChanged();
        }

        UpdateSearchSummary();
    }

    partial void OnSelectedDashboardCostVehicleChanged(CostVehicleItemViewModel? value)
    {
        SelectedCostVehicleDetail = value is null
            ? L("CostWorkspace.Detail.Empty")
            : LF(
                "CostWorkspace.Detail.Selected",
                value.VehicleName,
                value.Category,
                value.FuelCost,
                value.HistoryCost,
                value.RecordCost,
                value.TotalCost,
                value.Distance,
                value.CostPerKm,
                value.Status);
        OnPropertyChanged(nameof(CanUseSelectedCostVehicle));
        FocusSelectedCostDetailCommand.NotifyCanExecuteChanged();
        EditSelectedCostVehicleCommand.NotifyCanExecuteChanged();
        Root.NotifyCostWorkspaceSelectionChanged();
    }

    partial void OnCostSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanClearCostSearch));
        ClearCostSearchCommand.NotifyCanExecuteChanged();
        RefreshVisibleCostVehicles();
    }

    partial void OnSelectedCostPeriodPresetChanged(string value)
    {
        Root.HandleCostPeriodPresetChanged(value);
    }

    partial void OnCostPeriodStartTextChanged(string value)
    {
        Root.HandleCostPeriodCustomDateChanged();
    }

    partial void OnCostPeriodEndTextChanged(string value)
    {
        Root.HandleCostPeriodCustomDateChanged();
    }

    private bool MatchesSearch(CostVehicleItemViewModel item)
    {
        if (string.IsNullOrWhiteSpace(CostSearchText))
        {
            return true;
        }

        var query = CostSearchText.Trim();
        return Contains(item.VehicleName, query)
            || Contains(item.Category, query)
            || Contains(item.FuelCost, query)
            || Contains(item.HistoryCost, query)
            || Contains(item.RecordCost, query)
            || Contains(item.TotalCost, query)
            || Contains(item.Distance, query)
            || Contains(item.CostPerKm, query)
            || Contains(item.Status, query)
            || Contains(item.AccessibleLabel, query);
    }

    private void UpdateSearchSummary()
    {
        if (string.IsNullOrWhiteSpace(CostSearchText))
        {
            CostSearchSummary = LF("CostWorkspace.SearchSummary.Visible", VisibleCostVehicles.Count);
            return;
        }

        CostSearchSummary = VisibleCostVehicles.Count == 0
            ? LF("CostWorkspace.SearchSummary.Empty", CostSearchText.Trim())
            : LF("CostWorkspace.SearchSummary.WithResults", CostSearchText.Trim(), VisibleCostVehicles.Count);
    }

    private static bool Contains(string value, string query) =>
        value.Contains(query, StringComparison.CurrentCultureIgnoreCase);
}
