using System.Collections.ObjectModel;
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

    public string CostExportStatus => Root.CostExportStatus;

    public ICommand OpenSelectedDashboardCostVehicleCommand => Root.OpenSelectedDashboardCostVehicleCommand;
    public IAsyncRelayCommand ExportFleetCostSummaryCommand => Root.ExportFleetCostSummaryCommand;
    public IAsyncRelayCommand ExportSelectedVehicleCostDetailCommand => Root.ExportSelectedVehicleCostDetailCommand;
    public IAsyncRelayCommand ExportSelectedVehicleCostReportCommand => Root.ExportSelectedVehicleCostReportCommand;

    partial void OnSelectedDashboardCostVehicleChanged(CostVehicleItemViewModel? value)
    {
        SelectedCostVehicleDetail = value is null
            ? "Vyberte vozidlo v seznamu a zobrazí se rozpad nákladů."
            : $"Vozidlo: {value.VehicleName}\nKategorie: {value.Category}\nPalivo: {value.FuelCost}\nHistorie: {value.HistoryCost}\nDoklady: {value.RecordCost}\nCelkem: {value.TotalCost}\nUjeto: {value.Distance}\nCena / km: {value.CostPerKm}\nStav výpočtu: {value.Status}";
        Root.NotifyCostWorkspaceSelectionChanged();
    }
}
