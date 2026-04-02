using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class FuelWorkspaceViewModel : WorkspaceViewModelBase
{
    public FuelWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.FuelWindowTitle;
    public string FuelSummary => Root.FuelSummary;
    public ObservableCollection<VehicleFuelItemViewModel> SelectedVehicleFuel => Root.SelectedVehicleFuel;

    [ObservableProperty]
    private VehicleFuelItemViewModel? selectedFuel;

    [ObservableProperty]
    private string selectedFuelDetail = "Vyberte tankování a zobrazí se detail položky.";

    [ObservableProperty]
    private string fuelPanelHeading = "Detail tankování";

    [ObservableProperty]
    private bool isEditingFuel;

    [ObservableProperty]
    private string fuelEditorStatus = string.Empty;

    [ObservableProperty]
    private string fuelEditorDate = string.Empty;

    [ObservableProperty]
    private string fuelEditorFuelType = string.Empty;

    [ObservableProperty]
    private string fuelEditorLiters = string.Empty;

    [ObservableProperty]
    private string fuelEditorTotalCost = string.Empty;

    [ObservableProperty]
    private string fuelEditorOdometer = string.Empty;

    [ObservableProperty]
    private bool fuelEditorFullTank = true;

    [ObservableProperty]
    private string fuelEditorNote = string.Empty;

    public bool IsFuelDetailVisible => !IsEditingFuel;

    public ICommand CreateFuelCommand => Root.CreateFuelCommand;
    public ICommand EditSelectedFuelCommand => Root.EditSelectedFuelCommand;
    public ICommand DeleteSelectedFuelCommand => Root.DeleteSelectedFuelCommand;
    public ICommand SaveFuelCommand => Root.SaveFuelCommand;
    public ICommand CancelFuelEditCommand => Root.CancelFuelEditCommand;

    partial void OnSelectedFuelChanged(VehicleFuelItemViewModel? value)
    {
        SelectedFuelDetail = value is null
            ? "Vyberte tankování a zobrazí se detail položky."
            : $"Datum: {value.Date}\nPalivo: {value.FuelType}\nMnožství: {value.Liters}\nCena celkem: {value.TotalCost}\nTachometr: {value.Odometer}\nStav nádrže: {value.TankState}\nPoznámka: {Root.FormatWorkspaceValue(value.Note, "bez poznámky")}";

        Root.NotifyFuelWorkspaceSelectionChanged();
    }

    partial void OnIsEditingFuelChanged(bool value)
    {
        FuelPanelHeading = value
            ? (Root.GetEditingFuelId() is null ? "Nové tankování" : "Upravit tankování")
            : "Detail tankování";

        OnPropertyChanged(nameof(IsFuelDetailVisible));
        Root.NotifyFuelWorkspaceEditingChanged();
    }
}
