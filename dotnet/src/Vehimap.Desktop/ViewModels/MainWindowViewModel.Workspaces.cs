using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    public VehicleDetailWorkspaceViewModel VehicleDetailWorkspace { get; private set; } = null!;
    public HistoryWorkspaceViewModel HistoryWorkspace { get; private set; } = null!;
    public FuelWorkspaceViewModel FuelWorkspace { get; private set; } = null!;
    public ReminderWorkspaceViewModel ReminderWorkspace { get; private set; } = null!;
    public MaintenanceWorkspaceViewModel MaintenanceWorkspace { get; private set; } = null!;
    public RecordWorkspaceViewModel RecordWorkspace { get; private set; } = null!;
    public AuditWorkspaceViewModel AuditWorkspace { get; private set; } = null!;
    public DashboardWorkspaceViewModel DashboardWorkspace { get; private set; } = null!;

    private void InitializeWorkspaces()
    {
        VehicleDetailWorkspace = new VehicleDetailWorkspaceViewModel(this);
        HistoryWorkspace = new HistoryWorkspaceViewModel(this);
        FuelWorkspace = new FuelWorkspaceViewModel(this);
        ReminderWorkspace = new ReminderWorkspaceViewModel(this);
        MaintenanceWorkspace = new MaintenanceWorkspaceViewModel(this);
        RecordWorkspace = new RecordWorkspaceViewModel(this);
        AuditWorkspace = new AuditWorkspaceViewModel(this);
        DashboardWorkspace = new DashboardWorkspaceViewModel(this);
    }

    internal void RequestWorkspaceFocus(DesktopFocusTarget target)
    {
        RequestFocus(target);
    }
}
