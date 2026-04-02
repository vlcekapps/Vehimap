using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class MaintenanceWorkspaceViewModel : WorkspaceViewModelBase
{
    public MaintenanceWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.MaintenanceWindowTitle;
    public string MaintenanceSummary => Root.MaintenanceSummary;
    public ObservableCollection<VehicleMaintenanceItemViewModel> SelectedVehicleMaintenance => Root.SelectedVehicleMaintenance;

    [ObservableProperty]
    private VehicleMaintenanceItemViewModel? selectedMaintenance;

    [ObservableProperty]
    private string selectedMaintenanceDetail = "Vyberte servisní úkon a zobrazí se detail položky.";

    [ObservableProperty]
    private string maintenancePanelHeading = "Detail údržby";

    [ObservableProperty]
    private bool isEditingMaintenance;

    [ObservableProperty]
    private string maintenanceEditorStatus = string.Empty;

    [ObservableProperty]
    private string maintenanceEditorTitle = string.Empty;

    [ObservableProperty]
    private string maintenanceEditorIntervalKm = string.Empty;

    [ObservableProperty]
    private string maintenanceEditorIntervalMonths = string.Empty;

    [ObservableProperty]
    private string maintenanceEditorLastServiceDate = string.Empty;

    [ObservableProperty]
    private string maintenanceEditorLastServiceOdometer = string.Empty;

    [ObservableProperty]
    private bool maintenanceEditorIsActive = true;

    [ObservableProperty]
    private string maintenanceEditorNote = string.Empty;

    public bool IsMaintenanceDetailVisible => !IsEditingMaintenance;

    public ICommand CreateMaintenanceCommand => Root.CreateMaintenanceCommand;
    public ICommand EditSelectedMaintenanceCommand => Root.EditSelectedMaintenanceCommand;
    public ICommand DeleteSelectedMaintenanceCommand => Root.DeleteSelectedMaintenanceCommand;
    public ICommand SaveMaintenanceCommand => Root.SaveMaintenanceCommand;
    public ICommand CancelMaintenanceEditCommand => Root.CancelMaintenanceEditCommand;

    partial void OnSelectedMaintenanceChanged(VehicleMaintenanceItemViewModel? value)
    {
        SelectedMaintenanceDetail = value is null
            ? "Vyberte servisní úkon a zobrazí se detail položky."
            : $"Úkon: {value.Title}\nInterval: {value.Interval}\nPoslední servis: {value.LastService}\nStav: {value.Status}\nPoznámka: {Root.FormatWorkspaceValue(value.Note, "bez poznámky")}";

        Root.NotifyMaintenanceWorkspaceSelectionChanged();
    }

    partial void OnIsEditingMaintenanceChanged(bool value)
    {
        MaintenancePanelHeading = value
            ? (Root.GetEditingMaintenanceId() is null ? "Nový servisní plán" : "Upravit údržbu")
            : "Detail údržby";

        OnPropertyChanged(nameof(IsMaintenanceDetailVisible));
        Root.NotifyMaintenanceWorkspaceEditingChanged();
    }
}
