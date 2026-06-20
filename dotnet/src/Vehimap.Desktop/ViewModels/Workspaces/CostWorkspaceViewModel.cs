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
    private CostVehicleItemViewModel? selectedDashboardCostVehicle;

    [ObservableProperty]
    private string selectedCostVehicleDetail = "Vyberte vozidlo v seznamu a zobrazí se rozpad nákladů.";

    public string WindowTitle => Root.CostWindowTitle;

    public ObservableCollection<CostVehicleItemViewModel> CostVehicles => Root.CostVehicles;
    public ObservableCollection<CostVehicleItemViewModel> VisibleCostVehicles { get; } = [];

    public string CostExportStatus => Root.CostExportStatus;

    public bool CanUseSelectedCostVehicle => SelectedDashboardCostVehicle is not null;

    [ObservableProperty]
    private string costSearchText = string.Empty;

    [ObservableProperty]
    private string costSearchSummary = "Ctrl+F přesune fokus do hledání nákladového přehledu.";

    public ICommand OpenSelectedDashboardCostVehicleCommand => Root.OpenSelectedDashboardCostVehicleCommand;
    public ICommand OpenSelectedCostVehicleCommand => Root.OpenSelectedDashboardCostVehicleCommand;
    public IAsyncRelayCommand ExportFleetCostSummaryCommand => Root.ExportFleetCostSummaryCommand;
    public IAsyncRelayCommand ExportSelectedVehicleCostDetailCommand => Root.ExportSelectedVehicleCostDetailCommand;
    public IAsyncRelayCommand ExportSelectedVehicleCostReportCommand => Root.ExportSelectedVehicleCostReportCommand;

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
            SelectedCostVehicleDetail = "Vyberte vozidlo v seznamu a zobrazí se rozpad nákladů.";
            Root.NotifyCostWorkspaceSelectionChanged();
        }

        UpdateSearchSummary();
    }

    partial void OnSelectedDashboardCostVehicleChanged(CostVehicleItemViewModel? value)
    {
        SelectedCostVehicleDetail = value is null
            ? "Vyberte vozidlo v seznamu a zobrazí se rozpad nákladů."
            : $"Vozidlo: {value.VehicleName}\nKategorie: {value.Category}\nPalivo: {value.FuelCost}\nHistorie: {value.HistoryCost}\nDoklady: {value.RecordCost}\nCelkem: {value.TotalCost}\nUjeto: {value.Distance}\nCena / km: {value.CostPerKm}\nStav výpočtu: {value.Status}";
        OnPropertyChanged(nameof(CanUseSelectedCostVehicle));
        FocusSelectedCostDetailCommand.NotifyCanExecuteChanged();
        EditSelectedCostVehicleCommand.NotifyCanExecuteChanged();
        Root.NotifyCostWorkspaceSelectionChanged();
    }

    partial void OnCostSearchTextChanged(string value)
    {
        RefreshVisibleCostVehicles();
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
            CostSearchSummary = $"Zobrazeno {VisibleCostVehicles.Count} vozidel v nákladovém přehledu. Ctrl+F přesune fokus do hledání, Ctrl+R přehled obnoví.";
            return;
        }

        CostSearchSummary = VisibleCostVehicles.Count == 0
            ? $"Hledání „{CostSearchText.Trim()}“ nenašlo v nákladech žádné vozidlo."
            : $"Hledání „{CostSearchText.Trim()}“ našlo {VisibleCostVehicles.Count} vozidel v nákladech.";
    }

    internal void NotifyCostExportStatusChanged()
    {
        OnPropertyChanged(nameof(CostExportStatus));
    }

    private static bool Contains(string value, string query) =>
        value.Contains(query, StringComparison.CurrentCultureIgnoreCase);
}
