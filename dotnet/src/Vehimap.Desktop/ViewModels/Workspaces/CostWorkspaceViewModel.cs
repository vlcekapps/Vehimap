using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class CostWorkspaceViewModel : WorkspaceViewModelBase
{
    public CostWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string CostSummary => Root.CostSummary;

    public string CostComparison => Root.CostComparison;

    public ObservableCollection<CostVehicleItemViewModel> CostVehicles => Root.CostVehicles;

    public CostVehicleItemViewModel? SelectedDashboardCostVehicle
    {
        get => Root.SelectedDashboardCostVehicle;
        set => Root.SelectedDashboardCostVehicle = value;
    }

    public ICommand OpenSelectedDashboardCostVehicleCommand => Root.OpenSelectedDashboardCostVehicleCommand;
}
