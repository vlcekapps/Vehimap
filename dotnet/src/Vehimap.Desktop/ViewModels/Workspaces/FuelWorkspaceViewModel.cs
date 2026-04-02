using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class FuelWorkspaceViewModel : WorkspaceViewModelBase
{
    public FuelWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.FuelWindowTitle;
    public string FuelSummary => Root.FuelSummary;
    public ObservableCollection<VehicleFuelItemViewModel> SelectedVehicleFuel => Root.SelectedVehicleFuel;
    public VehicleFuelItemViewModel? SelectedFuel
    {
        get => Root.SelectedFuel;
        set => Root.SelectedFuel = value;
    }

    public string SelectedFuelDetail => Root.SelectedFuelDetail;
    public string FuelPanelHeading => Root.FuelPanelHeading;
    public bool IsEditingFuel => Root.IsEditingFuel;
    public bool IsFuelDetailVisible => Root.IsFuelDetailVisible;
    public string FuelEditorStatus => Root.FuelEditorStatus;
    public string FuelEditorDate
    {
        get => Root.FuelEditorDate;
        set => Root.FuelEditorDate = value;
    }

    public string FuelEditorFuelType
    {
        get => Root.FuelEditorFuelType;
        set => Root.FuelEditorFuelType = value;
    }

    public string FuelEditorLiters
    {
        get => Root.FuelEditorLiters;
        set => Root.FuelEditorLiters = value;
    }

    public string FuelEditorTotalCost
    {
        get => Root.FuelEditorTotalCost;
        set => Root.FuelEditorTotalCost = value;
    }

    public string FuelEditorOdometer
    {
        get => Root.FuelEditorOdometer;
        set => Root.FuelEditorOdometer = value;
    }

    public bool FuelEditorFullTank
    {
        get => Root.FuelEditorFullTank;
        set => Root.FuelEditorFullTank = value;
    }

    public string FuelEditorNote
    {
        get => Root.FuelEditorNote;
        set => Root.FuelEditorNote = value;
    }

    public ICommand CreateFuelCommand => Root.CreateFuelCommand;
    public ICommand EditSelectedFuelCommand => Root.EditSelectedFuelCommand;
    public ICommand DeleteSelectedFuelCommand => Root.DeleteSelectedFuelCommand;
    public ICommand SaveFuelCommand => Root.SaveFuelCommand;
    public ICommand CancelFuelEditCommand => Root.CancelFuelEditCommand;
}

