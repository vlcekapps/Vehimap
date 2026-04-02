using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    public VehicleDetailWorkspaceViewModel VehicleDetailWorkspace { get; private set; } = null!;
    public HistoryWorkspaceViewModel HistoryWorkspace { get; private set; } = null!;
    public FuelWorkspaceViewModel FuelWorkspace { get; private set; } = null!;
    public ReminderWorkspaceViewModel ReminderWorkspace { get; private set; } = null!;
    public MaintenanceWorkspaceViewModel MaintenanceWorkspace { get; private set; } = null!;
    public TimelineWorkspaceViewModel TimelineWorkspace { get; private set; } = null!;
    public RecordWorkspaceViewModel RecordWorkspace { get; private set; } = null!;
    public AuditWorkspaceViewModel AuditWorkspace { get; private set; } = null!;
    public CostWorkspaceViewModel CostWorkspace { get; private set; } = null!;
    public DashboardWorkspaceViewModel DashboardWorkspace { get; private set; } = null!;
    public GlobalSearchWorkspaceViewModel GlobalSearchWorkspace { get; private set; } = null!;
    public UpcomingOverviewWorkspaceViewModel UpcomingOverviewWorkspace { get; private set; } = null!;
    public OverdueOverviewWorkspaceViewModel OverdueOverviewWorkspace { get; private set; } = null!;

    private void InitializeWorkspaces()
    {
        VehicleDetailWorkspace = new VehicleDetailWorkspaceViewModel(this);
        HistoryWorkspace = new HistoryWorkspaceViewModel(this);
        FuelWorkspace = new FuelWorkspaceViewModel(this);
        ReminderWorkspace = new ReminderWorkspaceViewModel(this);
        MaintenanceWorkspace = new MaintenanceWorkspaceViewModel(this);
        TimelineWorkspace = new TimelineWorkspaceViewModel(this);
        RecordWorkspace = new RecordWorkspaceViewModel(this);
        AuditWorkspace = new AuditWorkspaceViewModel(this);
        CostWorkspace = new CostWorkspaceViewModel(this);
        DashboardWorkspace = new DashboardWorkspaceViewModel(this);
        GlobalSearchWorkspace = new GlobalSearchWorkspaceViewModel(this);
        UpcomingOverviewWorkspace = new UpcomingOverviewWorkspaceViewModel(this);
        OverdueOverviewWorkspace = new OverdueOverviewWorkspaceViewModel(this);
    }

    internal void RequestWorkspaceFocus(DesktopFocusTarget target)
    {
        RequestFocus(target);
    }
}
