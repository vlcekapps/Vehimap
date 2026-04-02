using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class DashboardWorkspaceViewModel : WorkspaceViewModelBase
{
    public DashboardWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    [ObservableProperty]
    private string dashboardTimelineSummary = "Nejbližší termíny napříč vozidly se zobrazí po načtení dat.";

    [ObservableProperty]
    private string selectedDashboardTimelineDetail = "Vyberte nejbližší termín a můžete přejít na související vozidlo nebo evidenci.";

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedDashboardTimelineItem;

    public string WindowTitle => Root.DashboardWindowTitle;

    public string AuditSummary => Root.AuditSummary;

    public string CostSummary => Root.CostSummary;

    public string CostComparison => Root.CostComparison;

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

    public ICommand OpenSelectedDashboardAuditItemCommand => Root.OpenSelectedDashboardAuditItemCommand;

    public ICommand OpenSelectedDashboardCostVehicleCommand => Root.OpenSelectedDashboardCostVehicleCommand;

    public ICommand OpenSelectedDashboardTimelineItemCommand => Root.OpenSelectedDashboardTimelineItemCommand;

    partial void OnSelectedDashboardTimelineItemChanged(VehicleTimelineItemViewModel? value)
    {
        SelectedDashboardTimelineDetail = value is null
            ? "Vyberte nejbližší termín a můžete přejít na související vozidlo nebo evidenci."
            : $"Vozidlo: {value.VehicleName}\nDatum: {value.Date}\nDruh: {value.KindLabel}\nPoložka: {value.Title}\nStav: {Root.FormatWorkspaceValue(value.Status, "-")}\nDetail: {Root.FormatWorkspaceValue(value.Detail, "-")}";

        Root.NotifyDashboardWorkspaceTimelineSelectionChanged();
    }
}
