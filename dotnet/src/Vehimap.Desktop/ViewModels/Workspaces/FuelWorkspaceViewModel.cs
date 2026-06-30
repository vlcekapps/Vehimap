using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Desktop.Services;
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
    public ObservableCollection<FuelConsumptionSegmentItemViewModel> FuelConsumptionSegments { get; } = [];
    public ObservableCollection<FuelGroupSummaryItemViewModel> FuelGroupSummaries { get; } = [];
    public ObservableCollection<FuelAnalysisWarningItemViewModel> FuelAnalysisWarnings { get; } = [];

    [ObservableProperty]
    private string fuelSummary = L("FuelWorkspace.Summary.Initial");

    [ObservableProperty]
    private string fuelSearchText = string.Empty;

    [ObservableProperty]
    private string fuelSearchSummary = L("FuelWorkspace.SearchSummary.Initial");

    [ObservableProperty]
    private string fuelAnalysisSummaryText = L("FuelWorkspace.AnalysisSummary.Initial");

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
    private string selectedFuelDetail = L("FuelWorkspace.Detail.Empty");

    [ObservableProperty]
    private FuelConsumptionSegmentItemViewModel? selectedFuelConsumptionSegment;

    [ObservableProperty]
    private FuelGroupSummaryItemViewModel? selectedFuelGroupSummary;

    [ObservableProperty]
    private FuelAnalysisWarningItemViewModel? selectedFuelAnalysisWarning;

    [ObservableProperty]
    private string fuelPanelHeading = L("FuelWorkspace.PanelHeading");

    [ObservableProperty]
    private string fuelEditorHeading = L("FuelEditor.NewTitle");

    [ObservableProperty]
    private bool isEditingFuel;

    [ObservableProperty]
    private string fuelEditorStatus = string.Empty;

    [ObservableProperty]
    private string fuelEditorDate = string.Empty;

    [ObservableProperty]
    private string fuelEditorFuelType = string.Empty;

    [ObservableProperty]
    private string fuelEditorFuelDetail = string.Empty;

    [ObservableProperty]
    private string fuelEditorStation = string.Empty;

    [ObservableProperty]
    private string fuelEditorLiters = string.Empty;

    public string FuelEditorVolumeLabel => LF("FuelEditor.VolumeLabel", Root.CurrentVolumeUnitLabel);

    public string FuelEditorVolumeName => LF("FuelEditor.VolumeName", Root.CurrentVolumeUnitLabel);

    public string FuelEditorVolumeHelp => LF("FuelEditor.VolumeHelp", Root.CurrentVolumeUnitLabel);

    [ObservableProperty]
    private string fuelEditorTotalCost = string.Empty;

    [ObservableProperty]
    private string fuelEditorOdometer = string.Empty;

    public string FuelEditorOdometerLabel => LF("FuelEditor.OdometerLabel", Root.CurrentDistanceUnitLabel);

    public string FuelEditorOdometerName => LF("FuelEditor.OdometerName", Root.CurrentDistanceUnitLabel);

    public string FuelEditorOdometerHelp => LF("FuelEditor.OdometerHelp", Root.CurrentDistanceUnitLabel);

    [ObservableProperty]
    private bool fuelEditorFullTank = true;

    [ObservableProperty]
    private string fuelEditorNote = string.Empty;

    public bool IsFuelDetailVisible => !IsEditingFuel;
    public bool HasFuelConsumptionSegments => FuelConsumptionSegments.Count > 0;
    public bool HasFuelGroupSummaries => FuelGroupSummaries.Count > 0;
    public bool HasFuelAnalysisWarnings => FuelAnalysisWarnings.Count > 0;
    public bool CanOpenSelectedFuelConsumptionSegment => !string.IsNullOrWhiteSpace(SelectedFuelConsumptionSegment?.FuelEntryId);
    public bool CanOpenSelectedFuelGroupSummary => !string.IsNullOrWhiteSpace(SelectedFuelGroupSummary?.FuelEntryId);
    public bool CanOpenSelectedFuelAnalysisWarning => !string.IsNullOrWhiteSpace(SelectedFuelAnalysisWarning?.FuelEntryId);

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

    [RelayCommand(CanExecute = nameof(CanOpenSelectedFuelConsumptionSegment))]
    private void OpenSelectedFuelConsumptionSegment()
    {
        Root.SelectFuelAnalysisTarget(SelectedFuelConsumptionSegment?.FuelEntryId);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedFuelGroupSummary))]
    private void OpenSelectedFuelGroupSummary()
    {
        Root.SelectFuelAnalysisTarget(SelectedFuelGroupSummary?.FuelEntryId);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedFuelAnalysisWarning))]
    private void OpenSelectedFuelAnalysisWarning()
    {
        Root.SelectFuelAnalysisTarget(SelectedFuelAnalysisWarning?.FuelEntryId);
    }

    internal void ApplyFuelAnalysis(DesktopFuelAnalysisProjection projection)
    {
        FuelAnalysisSummaryText = projection.Summary;

        FuelConsumptionSegments.Clear();
        foreach (var item in projection.ConsumptionSegments)
        {
            FuelConsumptionSegments.Add(item);
        }

        FuelGroupSummaries.Clear();
        foreach (var item in projection.GroupSummaries)
        {
            FuelGroupSummaries.Add(item);
        }

        FuelAnalysisWarnings.Clear();
        foreach (var item in projection.Warnings)
        {
            FuelAnalysisWarnings.Add(item);
        }

        SelectedFuelConsumptionSegment = FuelConsumptionSegments.FirstOrDefault();
        SelectedFuelGroupSummary = FuelGroupSummaries.FirstOrDefault();
        SelectedFuelAnalysisWarning = FuelAnalysisWarnings.FirstOrDefault();
        NotifyFuelAnalysisStateChanged();
    }

    internal void ClearFuelAnalysis()
    {
        FuelAnalysisSummaryText = L("FuelWorkspace.AnalysisSummary.Initial");
        FuelConsumptionSegments.Clear();
        FuelGroupSummaries.Clear();
        FuelAnalysisWarnings.Clear();
        SelectedFuelConsumptionSegment = null;
        SelectedFuelGroupSummary = null;
        SelectedFuelAnalysisWarning = null;
        NotifyFuelAnalysisStateChanged();
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
            SelectedFuelDetail = L("FuelWorkspace.Detail.Empty");
            Root.NotifyFuelWorkspaceSelectionChanged();
        }

        UpdateSearchSummary();
    }

    partial void OnSelectedFuelChanged(VehicleFuelItemViewModel? value)
    {
        SelectedFuelDetail = value is null
            ? L("FuelWorkspace.Detail.Empty")
            : string.Join(
                Environment.NewLine,
                LF("FuelWorkspace.Detail.Date", value.Date),
                LF("FuelWorkspace.Detail.Fuel", value.FuelType),
                LF("FuelWorkspace.Detail.FuelDetail", value.FuelDetail),
                LF("FuelWorkspace.Detail.Station", value.Station),
                LF("FuelWorkspace.Detail.Volume", value.Liters),
                LF("FuelWorkspace.Detail.TotalCost", value.TotalCost),
                LF("FuelWorkspace.Detail.Odometer", value.Odometer),
                LF("FuelWorkspace.Detail.TankState", value.TankState),
                LF("FuelWorkspace.Detail.Note", Root.FormatWorkspaceValue(value.Note, L("Common.NoNote"))));

        Root.NotifyFuelWorkspaceSelectionChanged();
    }

    partial void OnSelectedFuelConsumptionSegmentChanged(FuelConsumptionSegmentItemViewModel? value)
    {
        OpenSelectedFuelConsumptionSegmentCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedFuelGroupSummaryChanged(FuelGroupSummaryItemViewModel? value)
    {
        OpenSelectedFuelGroupSummaryCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedFuelAnalysisWarningChanged(FuelAnalysisWarningItemViewModel? value)
    {
        OpenSelectedFuelAnalysisWarningCommand.NotifyCanExecuteChanged();
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
        if (value)
        {
            FuelEditorHeading = Root.GetEditingFuelId() is null
                ? L("FuelEditor.NewTitle")
                : L("FuelEditor.EditTitle");
            NotifyUnitMetadataChanged();
        }

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
            || Contains(item.FuelDetail, query)
            || Contains(item.Station, query)
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
            FuelSearchSummary = LF("FuelWorkspace.SearchSummary.All", VisibleFuelItems.Count);
            return;
        }

        FuelSearchSummary = VisibleFuelItems.Count == 0
            ? LF("FuelWorkspace.SearchSummary.Empty", FuelSearchText.Trim())
            : LF("FuelWorkspace.SearchSummary.Filtered", FuelSearchText.Trim(), VisibleFuelItems.Count);
    }

    private void NotifyFuelAnalysisStateChanged()
    {
        OnPropertyChanged(nameof(HasFuelConsumptionSegments));
        OnPropertyChanged(nameof(HasFuelGroupSummaries));
        OnPropertyChanged(nameof(HasFuelAnalysisWarnings));
        OnPropertyChanged(nameof(CanOpenSelectedFuelConsumptionSegment));
        OnPropertyChanged(nameof(CanOpenSelectedFuelGroupSummary));
        OnPropertyChanged(nameof(CanOpenSelectedFuelAnalysisWarning));
        OpenSelectedFuelConsumptionSegmentCommand.NotifyCanExecuteChanged();
        OpenSelectedFuelGroupSummaryCommand.NotifyCanExecuteChanged();
        OpenSelectedFuelAnalysisWarningCommand.NotifyCanExecuteChanged();
    }

    private static bool Contains(string value, string query) =>
        value.Contains(query, StringComparison.CurrentCultureIgnoreCase);

    internal void NotifyUnitMetadataChanged()
    {
        OnPropertyChanged(nameof(FuelEditorVolumeLabel));
        OnPropertyChanged(nameof(FuelEditorVolumeName));
        OnPropertyChanged(nameof(FuelEditorVolumeHelp));
        OnPropertyChanged(nameof(FuelEditorOdometerLabel));
        OnPropertyChanged(nameof(FuelEditorOdometerName));
        OnPropertyChanged(nameof(FuelEditorOdometerHelp));
    }
}
