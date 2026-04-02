using System.Windows.Input;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class VehicleDetailWorkspaceViewModel : WorkspaceViewModelBase
{
    public VehicleDetailWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.VehicleDetailWindowTitle;
    public string VehiclePanelHeading => Root.VehiclePanelHeading;
    public string SelectedVehicleHeading => Root.SelectedVehicleHeading;
    public string SelectedVehicleOverview => Root.SelectedVehicleOverview;
    public string SelectedVehicleDates => Root.SelectedVehicleDates;
    public string SelectedVehicleProfile => Root.SelectedVehicleProfile;
    public bool IsEditingVehicle => Root.IsEditingVehicle;
    public bool IsVehicleDetailVisible => Root.IsVehicleDetailVisible;
    public string VehicleEditorStatus => Root.VehicleEditorStatus;
    public IReadOnlyList<string> VehicleCategoryOptions => LegacyKnownValues.Categories;

    public string VehicleEditorName
    {
        get => Root.VehicleEditorName;
        set => Root.VehicleEditorName = value;
    }

    public string VehicleEditorCategory
    {
        get => Root.VehicleEditorCategory;
        set => Root.VehicleEditorCategory = value;
    }

    public string VehicleEditorNote
    {
        get => Root.VehicleEditorNote;
        set => Root.VehicleEditorNote = value;
    }

    public string VehicleEditorMakeModel
    {
        get => Root.VehicleEditorMakeModel;
        set => Root.VehicleEditorMakeModel = value;
    }

    public string VehicleEditorPlate
    {
        get => Root.VehicleEditorPlate;
        set => Root.VehicleEditorPlate = value;
    }

    public string VehicleEditorYear
    {
        get => Root.VehicleEditorYear;
        set => Root.VehicleEditorYear = value;
    }

    public string VehicleEditorPower
    {
        get => Root.VehicleEditorPower;
        set => Root.VehicleEditorPower = value;
    }

    public string VehicleEditorLastTk
    {
        get => Root.VehicleEditorLastTk;
        set => Root.VehicleEditorLastTk = value;
    }

    public string VehicleEditorNextTk
    {
        get => Root.VehicleEditorNextTk;
        set => Root.VehicleEditorNextTk = value;
    }

    public string VehicleEditorGreenCardFrom
    {
        get => Root.VehicleEditorGreenCardFrom;
        set => Root.VehicleEditorGreenCardFrom = value;
    }

    public string VehicleEditorGreenCardTo
    {
        get => Root.VehicleEditorGreenCardTo;
        set => Root.VehicleEditorGreenCardTo = value;
    }

    public string VehicleEditorState
    {
        get => Root.VehicleEditorState;
        set => Root.VehicleEditorState = value;
    }

    public string VehicleEditorPowertrain
    {
        get => Root.VehicleEditorPowertrain;
        set => Root.VehicleEditorPowertrain = value;
    }

    public ICommand CreateVehicleCommand => Root.CreateVehicleCommand;
    public ICommand EditSelectedVehicleCommand => Root.EditSelectedVehicleCommand;
    public ICommand SaveVehicleCommand => Root.SaveVehicleCommand;
    public ICommand CancelVehicleEditCommand => Root.CancelVehicleEditCommand;
}
