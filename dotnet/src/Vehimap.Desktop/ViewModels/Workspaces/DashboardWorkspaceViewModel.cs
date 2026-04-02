using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class DashboardWorkspaceViewModel : WorkspaceViewModelBase
{
    public DashboardWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.DashboardWindowTitle;
    public string AuditSummary => Root.AuditSummary;
    public string CostSummary => Root.CostSummary;
    public string CostComparison => Root.CostComparison;
    public string DashboardTimelineSummary => Root.DashboardTimelineSummary;
    public string SelectedDashboardTimelineDetail => Root.SelectedDashboardTimelineDetail;
    public ObservableCollection<AuditItemViewModel> AuditItems => Root.AuditItems;
    public ObservableCollection<CostVehicleItemViewModel> CostVehicles => Root.CostVehicles;
    public ObservableCollection<VehicleTimelineItemViewModel> DashboardUpcomingTimeline => Root.DashboardUpcomingTimeline;

    public AuditItemViewModel? SelectedDashboardAuditItem
    {
        get => Root.SelectedDashboardAuditItem;
        set => Root.SelectedDashboardAuditItem = value;
    }

    public CostVehicleItemViewModel? SelectedDashboardCostVehicle
    {
        get => Root.SelectedDashboardCostVehicle;
        set => Root.SelectedDashboardCostVehicle = value;
    }

    public VehicleTimelineItemViewModel? SelectedDashboardTimelineItem
    {
        get => Root.SelectedDashboardTimelineItem;
        set => Root.SelectedDashboardTimelineItem = value;
    }

    public ICommand OpenSelectedDashboardAuditItemCommand => Root.OpenSelectedDashboardAuditItemCommand;
    public ICommand OpenSelectedDashboardCostVehicleCommand => Root.OpenSelectedDashboardCostVehicleCommand;
    public ICommand OpenSelectedDashboardTimelineItemCommand => Root.OpenSelectedDashboardTimelineItemCommand;
}

