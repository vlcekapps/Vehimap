using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class MaintenanceWorkspaceViewModel : WorkspaceViewModelBase
{
    public MaintenanceWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.MaintenanceWindowTitle;
    public string MaintenanceSummary => Root.MaintenanceSummary;
    public ObservableCollection<VehicleMaintenanceItemViewModel> SelectedVehicleMaintenance => Root.SelectedVehicleMaintenance;
    public VehicleMaintenanceItemViewModel? SelectedMaintenance
    {
        get => Root.SelectedMaintenance;
        set => Root.SelectedMaintenance = value;
    }

    public string SelectedMaintenanceDetail => Root.SelectedMaintenanceDetail;
    public string MaintenancePanelHeading => Root.MaintenancePanelHeading;
    public bool IsEditingMaintenance => Root.IsEditingMaintenance;
    public bool IsMaintenanceDetailVisible => Root.IsMaintenanceDetailVisible;
    public string MaintenanceEditorStatus => Root.MaintenanceEditorStatus;
    public string MaintenanceEditorTitle
    {
        get => Root.MaintenanceEditorTitle;
        set => Root.MaintenanceEditorTitle = value;
    }

    public string MaintenanceEditorIntervalKm
    {
        get => Root.MaintenanceEditorIntervalKm;
        set => Root.MaintenanceEditorIntervalKm = value;
    }

    public string MaintenanceEditorIntervalMonths
    {
        get => Root.MaintenanceEditorIntervalMonths;
        set => Root.MaintenanceEditorIntervalMonths = value;
    }

    public string MaintenanceEditorLastServiceDate
    {
        get => Root.MaintenanceEditorLastServiceDate;
        set => Root.MaintenanceEditorLastServiceDate = value;
    }

    public string MaintenanceEditorLastServiceOdometer
    {
        get => Root.MaintenanceEditorLastServiceOdometer;
        set => Root.MaintenanceEditorLastServiceOdometer = value;
    }

    public bool MaintenanceEditorIsActive
    {
        get => Root.MaintenanceEditorIsActive;
        set => Root.MaintenanceEditorIsActive = value;
    }

    public string MaintenanceEditorNote
    {
        get => Root.MaintenanceEditorNote;
        set => Root.MaintenanceEditorNote = value;
    }

    public ICommand CreateMaintenanceCommand => Root.CreateMaintenanceCommand;
    public ICommand EditSelectedMaintenanceCommand => Root.EditSelectedMaintenanceCommand;
    public ICommand DeleteSelectedMaintenanceCommand => Root.DeleteSelectedMaintenanceCommand;
    public ICommand SaveMaintenanceCommand => Root.SaveMaintenanceCommand;
    public ICommand CancelMaintenanceEditCommand => Root.CancelMaintenanceEditCommand;
}

