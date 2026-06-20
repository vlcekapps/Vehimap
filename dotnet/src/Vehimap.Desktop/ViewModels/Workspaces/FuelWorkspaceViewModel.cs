using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class FuelWorkspaceViewModel : WorkspaceViewModelBase
{
    public FuelWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.FuelWindowTitle;
    public ObservableCollection<VehicleFuelItemViewModel> SelectedVehicleFuel { get; } = [];
    public ObservableCollection<VehicleFuelItemViewModel> VisibleFuelItems { get; } = [];

    [ObservableProperty]
    private string fuelSummary = "Tankování vybraného vozidla se zobrazí po výběru vozidla.";

    [ObservableProperty]
    private string fuelSearchText = string.Empty;

    [ObservableProperty]
    private string fuelSearchSummary = "Ctrl+F přesune fokus do hledání tankování.";

    [ObservableProperty]
    private string selectedFuelSortOption = WorkspaceSortHelpers.DateSortLabel;

    [ObservableProperty]
    private bool fuelSortDescending = true;

    public IReadOnlyList<string> FuelSortOptions => WorkspaceSortHelpers.FuelSortOptions;

    public IReadOnlyList<string> FuelTypeOptions => LegacyKnownValues.FuelTypes;

    public bool CanClearFuelSearch => !string.IsNullOrWhiteSpace(FuelSearchText);

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

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.FuelSearch);
    }

    [RelayCommand(CanExecute = nameof(CanClearFuelSearch))]
    private void ClearFuelSearch()
    {
        FuelSearchText = string.Empty;
        RequestFocus(DesktopFocusTarget.FuelSearch);
    }

    public void RefreshVisibleFuelItems(bool preserveSelection = true)
    {
        var previousSelection = preserveSelection ? SelectedFuel : null;
        var filteredItems = WorkspaceSortHelpers
            .SortFuel(SelectedVehicleFuel.Where(MatchesSearch), SelectedFuelSortOption, FuelSortDescending)
            .ToList();

        VisibleFuelItems.Clear();
        foreach (var item in filteredItems)
        {
            VisibleFuelItems.Add(item);
        }

        SelectedFuel = previousSelection is not null
            ? VisibleFuelItems.FirstOrDefault(item => string.Equals(item.Id, previousSelection.Id, StringComparison.Ordinal))
            : null;

        SelectedFuel ??= VisibleFuelItems.FirstOrDefault();
        if (SelectedFuel is null)
        {
            SelectedFuelDetail = "Vyberte tankování a zobrazí se detail položky.";
            Root.NotifyFuelWorkspaceSelectionChanged();
        }

        UpdateSearchSummary();
    }

    partial void OnSelectedFuelChanged(VehicleFuelItemViewModel? value)
    {
        SelectedFuelDetail = value is null
            ? "Vyberte tankování a zobrazí se detail položky."
            : $"Datum: {value.Date}\nPalivo: {value.FuelType}\nMnožství: {value.Liters}\nCena celkem: {value.TotalCost}\nTachometr: {value.Odometer}\nStav nádrže: {value.TankState}\nPoznámka: {Root.FormatWorkspaceValue(value.Note, "bez poznámky")}";

        Root.NotifyFuelWorkspaceSelectionChanged();
    }

    partial void OnFuelSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanClearFuelSearch));
        ClearFuelSearchCommand.NotifyCanExecuteChanged();
        RefreshVisibleFuelItems();
    }

    partial void OnSelectedFuelSortOptionChanged(string value)
    {
        Root.HandleFuelWorkspaceSortChanged();
    }

    partial void OnFuelSortDescendingChanged(bool value)
    {
        Root.HandleFuelWorkspaceSortChanged();
    }

    partial void OnIsEditingFuelChanged(bool value)
    {
        FuelPanelHeading = value
            ? (Root.GetEditingFuelId() is null ? "Nové tankování" : "Upravit tankování")
            : "Detail tankování";

        OnPropertyChanged(nameof(IsFuelDetailVisible));
        Root.NotifyFuelWorkspaceEditingChanged();
    }

    private bool MatchesSearch(VehicleFuelItemViewModel item)
    {
        if (string.IsNullOrWhiteSpace(FuelSearchText))
        {
            return true;
        }

        var query = FuelSearchText.Trim();
        return Contains(item.Date, query)
            || Contains(item.FuelType, query)
            || Contains(item.Liters, query)
            || Contains(item.TotalCost, query)
            || Contains(item.Odometer, query)
            || Contains(item.TankState, query)
            || Contains(item.Note, query)
            || Contains(item.AccessibleLabel, query);
    }

    private void UpdateSearchSummary()
    {
        if (string.IsNullOrWhiteSpace(FuelSearchText))
        {
            FuelSearchSummary = $"Zobrazeno {VisibleFuelItems.Count} tankování. Ctrl+F přesune fokus do hledání.";
            return;
        }

        FuelSearchSummary = VisibleFuelItems.Count == 0
            ? $"Hledání „{FuelSearchText.Trim()}“ nenašlo v tankování žádný záznam."
            : $"Hledání „{FuelSearchText.Trim()}“ našlo {VisibleFuelItems.Count} tankování.";
    }

    private static bool Contains(string value, string query) =>
        value.Contains(query, StringComparison.CurrentCultureIgnoreCase);
}
