using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private string? _editingHistoryId;
    private string? _editingFuelId;
    private string? _editingMaintenanceId;

    [ObservableProperty]
    private bool isEditingHistory;

    [ObservableProperty]
    private string historyPanelHeading = "Detail historie";

    [ObservableProperty]
    private string historyEditorStatus = string.Empty;

    [ObservableProperty]
    private string historyEditorDate = string.Empty;

    [ObservableProperty]
    private string historyEditorType = string.Empty;

    [ObservableProperty]
    private string historyEditorOdometer = string.Empty;

    [ObservableProperty]
    private string historyEditorCost = string.Empty;

    [ObservableProperty]
    private string historyEditorNote = string.Empty;

    [ObservableProperty]
    private bool isEditingFuel;

    [ObservableProperty]
    private string fuelPanelHeading = "Detail tankování";

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

    [ObservableProperty]
    private bool isEditingMaintenance;

    [ObservableProperty]
    private string maintenancePanelHeading = "Detail údržby";

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

    public bool IsHistoryDetailVisible => !IsEditingHistory;

    public bool IsFuelDetailVisible => !IsEditingFuel;

    public bool IsMaintenanceDetailVisible => !IsEditingMaintenance;

    public bool CanCreateHistory => SelectedVehicle is not null && !IsEditingHistory;

    public bool CanEditSelectedHistory => SelectedHistory is not null && !IsEditingHistory;

    public bool CanDeleteSelectedHistory => SelectedHistory is not null && !IsEditingHistory;

    public bool CanSaveHistory => SelectedVehicle is not null && IsEditingHistory;

    public bool CanCancelHistoryEdit => IsEditingHistory;

    public bool CanCreateFuel => SelectedVehicle is not null && !IsEditingFuel;

    public bool CanEditSelectedFuel => SelectedFuel is not null && !IsEditingFuel;

    public bool CanDeleteSelectedFuel => SelectedFuel is not null && !IsEditingFuel;

    public bool CanSaveFuel => SelectedVehicle is not null && IsEditingFuel;

    public bool CanCancelFuelEdit => IsEditingFuel;

    public bool CanCreateMaintenance => SelectedVehicle is not null && !IsEditingMaintenance;

    public bool CanEditSelectedMaintenance => SelectedMaintenance is not null && !IsEditingMaintenance;

    public bool CanDeleteSelectedMaintenance => SelectedMaintenance is not null && !IsEditingMaintenance;

    public bool CanSaveMaintenance => SelectedVehicle is not null && IsEditingMaintenance;

    public bool CanCancelMaintenanceEdit => IsEditingMaintenance;

    partial void OnIsEditingHistoryChanged(bool value)
    {
        HistoryPanelHeading = value
            ? (_editingHistoryId is null ? "Nový záznam historie" : "Upravit historii")
            : "Detail historie";

        OnPropertyChanged(nameof(IsHistoryDetailVisible));
        CreateHistoryCommand.NotifyCanExecuteChanged();
        EditSelectedHistoryCommand.NotifyCanExecuteChanged();
        DeleteSelectedHistoryCommand.NotifyCanExecuteChanged();
        SaveHistoryCommand.NotifyCanExecuteChanged();
        CancelHistoryEditCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsEditingFuelChanged(bool value)
    {
        FuelPanelHeading = value
            ? (_editingFuelId is null ? "Nové tankování" : "Upravit tankování")
            : "Detail tankování";

        OnPropertyChanged(nameof(IsFuelDetailVisible));
        CreateFuelCommand.NotifyCanExecuteChanged();
        EditSelectedFuelCommand.NotifyCanExecuteChanged();
        DeleteSelectedFuelCommand.NotifyCanExecuteChanged();
        SaveFuelCommand.NotifyCanExecuteChanged();
        CancelFuelEditCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsEditingMaintenanceChanged(bool value)
    {
        MaintenancePanelHeading = value
            ? (_editingMaintenanceId is null ? "Nový servisní plán" : "Upravit údržbu")
            : "Detail údržby";

        OnPropertyChanged(nameof(IsMaintenanceDetailVisible));
        CreateMaintenanceCommand.NotifyCanExecuteChanged();
        EditSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        DeleteSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        SaveMaintenanceCommand.NotifyCanExecuteChanged();
        CancelMaintenanceEditCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCreateHistory))]
    private void CreateHistory()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        _editingHistoryId = null;
        HistoryEditorDate = string.Empty;
        HistoryEditorType = string.Empty;
        HistoryEditorOdometer = string.Empty;
        HistoryEditorCost = string.Empty;
        HistoryEditorNote = string.Empty;
        HistoryEditorStatus = "Vyplňte nový historický záznam a uložte jej.";
        IsEditingHistory = true;
        SelectedVehicleTabIndex = HistoryTabIndex;
        RequestFocus(DesktopFocusTarget.HistoryEditorDate);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelectedHistory))]
    private void EditSelectedHistory()
    {
        var entry = GetSelectedHistoryModel();
        if (entry is null)
        {
            return;
        }

        _editingHistoryId = entry.Id;
        HistoryEditorDate = entry.EventDate;
        HistoryEditorType = entry.EventType;
        HistoryEditorOdometer = entry.Odometer;
        HistoryEditorCost = entry.Cost;
        HistoryEditorNote = entry.Note;
        HistoryEditorStatus = "Upravte historický záznam a uložte změny.";
        IsEditingHistory = true;
        SelectedVehicleTabIndex = HistoryTabIndex;
        RequestFocus(DesktopFocusTarget.HistoryEditorDate);
    }

    [RelayCommand(CanExecute = nameof(CanSaveHistory))]
    private async Task SaveHistoryAsync()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        var historyId = _editingHistoryId ?? GenerateLegacyId(_dataSet.HistoryEntries.Select(item => item.Id));
        var updatedEntry = new VehicleHistoryEntry(
            historyId,
            SelectedVehicle.Id,
            (HistoryEditorDate ?? string.Empty).Trim(),
            string.IsNullOrWhiteSpace(HistoryEditorType) ? "Událost" : HistoryEditorType.Trim(),
            (HistoryEditorOdometer ?? string.Empty).Trim(),
            (HistoryEditorCost ?? string.Empty).Trim(),
            (HistoryEditorNote ?? string.Empty).Trim());

        UpsertHistoryEntry(updatedEntry);
        await PersistDataAndRestoreSelectionAsync(SelectedVehicle.Id, HistoryTabIndex, historyId: historyId);

        var wasNew = _editingHistoryId is null;
        CancelHistoryEditCore(clearStatus: false);
        HistoryEditorStatus = wasNew
            ? "Nový historický záznam byl uložen."
            : "Historický záznam byl upraven.";
        SelectedHistory = FindById(SelectedVehicleHistory, item => item.Id, historyId);
        RequestFocus(DesktopFocusTarget.HistoryList);
    }

    [RelayCommand(CanExecute = nameof(CanCancelHistoryEdit))]
    private void CancelHistoryEdit()
    {
        CancelHistoryEditCore(clearStatus: true);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedHistory))]
    private async Task DeleteSelectedHistoryAsync()
    {
        if (SelectedVehicle is null || SelectedHistory is null)
        {
            return;
        }

        _dataSet.HistoryEntries.RemoveAll(item => string.Equals(item.Id, SelectedHistory.Id, StringComparison.Ordinal));
        await PersistDataAndRestoreSelectionAsync(SelectedVehicle.Id, HistoryTabIndex);
        HistoryEditorStatus = "Historický záznam byl odstraněn.";
        RequestFocus(DesktopFocusTarget.HistoryList);
    }

    [RelayCommand(CanExecute = nameof(CanCreateFuel))]
    private void CreateFuel()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        _editingFuelId = null;
        FuelEditorDate = string.Empty;
        FuelEditorFuelType = string.Empty;
        FuelEditorLiters = string.Empty;
        FuelEditorTotalCost = string.Empty;
        FuelEditorOdometer = string.Empty;
        FuelEditorFullTank = true;
        FuelEditorNote = string.Empty;
        FuelEditorStatus = "Vyplňte nové tankování a uložte jej.";
        IsEditingFuel = true;
        SelectedVehicleTabIndex = FuelTabIndex;
        RequestFocus(DesktopFocusTarget.FuelEditorDate);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelectedFuel))]
    private void EditSelectedFuel()
    {
        var entry = GetSelectedFuelModel();
        if (entry is null)
        {
            return;
        }

        _editingFuelId = entry.Id;
        FuelEditorDate = entry.EntryDate;
        FuelEditorFuelType = entry.FuelType;
        FuelEditorLiters = entry.Liters;
        FuelEditorTotalCost = entry.TotalCost;
        FuelEditorOdometer = entry.Odometer;
        FuelEditorFullTank = entry.FullTank;
        FuelEditorNote = entry.Note;
        FuelEditorStatus = "Upravte tankování a uložte změny.";
        IsEditingFuel = true;
        SelectedVehicleTabIndex = FuelTabIndex;
        RequestFocus(DesktopFocusTarget.FuelEditorDate);
    }

    [RelayCommand(CanExecute = nameof(CanSaveFuel))]
    private async Task SaveFuelAsync()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        var fuelId = _editingFuelId ?? GenerateLegacyId(_dataSet.FuelEntries.Select(item => item.Id));
        var updatedEntry = new FuelEntry(
            fuelId,
            SelectedVehicle.Id,
            (FuelEditorDate ?? string.Empty).Trim(),
            (FuelEditorOdometer ?? string.Empty).Trim(),
            (FuelEditorLiters ?? string.Empty).Trim(),
            (FuelEditorTotalCost ?? string.Empty).Trim(),
            FuelEditorFullTank,
            string.IsNullOrWhiteSpace(FuelEditorFuelType) ? "Palivo" : FuelEditorFuelType.Trim(),
            (FuelEditorNote ?? string.Empty).Trim());

        UpsertFuelEntry(updatedEntry);
        await PersistDataAndRestoreSelectionAsync(SelectedVehicle.Id, FuelTabIndex, fuelId: fuelId);

        var wasNew = _editingFuelId is null;
        CancelFuelEditCore(clearStatus: false);
        FuelEditorStatus = wasNew
            ? "Nové tankování bylo uloženo."
            : "Tankování bylo upraveno.";
        SelectedFuel = FindById(SelectedVehicleFuel, item => item.Id, fuelId);
        RequestFocus(DesktopFocusTarget.FuelList);
    }

    [RelayCommand(CanExecute = nameof(CanCancelFuelEdit))]
    private void CancelFuelEdit()
    {
        CancelFuelEditCore(clearStatus: true);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedFuel))]
    private async Task DeleteSelectedFuelAsync()
    {
        if (SelectedVehicle is null || SelectedFuel is null)
        {
            return;
        }

        _dataSet.FuelEntries.RemoveAll(item => string.Equals(item.Id, SelectedFuel.Id, StringComparison.Ordinal));
        await PersistDataAndRestoreSelectionAsync(SelectedVehicle.Id, FuelTabIndex);
        FuelEditorStatus = "Tankování bylo odstraněno.";
        RequestFocus(DesktopFocusTarget.FuelList);
    }

    [RelayCommand(CanExecute = nameof(CanCreateMaintenance))]
    private void CreateMaintenance()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        _editingMaintenanceId = null;
        MaintenanceEditorTitle = string.Empty;
        MaintenanceEditorIntervalKm = string.Empty;
        MaintenanceEditorIntervalMonths = string.Empty;
        MaintenanceEditorLastServiceDate = string.Empty;
        MaintenanceEditorLastServiceOdometer = string.Empty;
        MaintenanceEditorIsActive = true;
        MaintenanceEditorNote = string.Empty;
        MaintenanceEditorStatus = "Vyplňte servisní plán a uložte jej.";
        IsEditingMaintenance = true;
        SelectedVehicleTabIndex = MaintenanceTabIndex;
        RequestFocus(DesktopFocusTarget.MaintenanceEditorTitle);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelectedMaintenance))]
    private void EditSelectedMaintenance()
    {
        var plan = GetSelectedMaintenanceModel();
        if (plan is null)
        {
            return;
        }

        _editingMaintenanceId = plan.Id;
        MaintenanceEditorTitle = plan.Title;
        MaintenanceEditorIntervalKm = plan.IntervalKm;
        MaintenanceEditorIntervalMonths = plan.IntervalMonths;
        MaintenanceEditorLastServiceDate = plan.LastServiceDate;
        MaintenanceEditorLastServiceOdometer = plan.LastServiceOdometer;
        MaintenanceEditorIsActive = plan.IsActive;
        MaintenanceEditorNote = plan.Note;
        MaintenanceEditorStatus = "Upravte servisní plán a uložte změny.";
        IsEditingMaintenance = true;
        SelectedVehicleTabIndex = MaintenanceTabIndex;
        RequestFocus(DesktopFocusTarget.MaintenanceEditorTitle);
    }

    [RelayCommand(CanExecute = nameof(CanSaveMaintenance))]
    private async Task SaveMaintenanceAsync()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        var title = (MaintenanceEditorTitle ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MaintenanceEditorStatus = "Servisní plán musí mít název.";
            RequestFocus(DesktopFocusTarget.MaintenanceEditorTitle);
            return;
        }

        var maintenanceId = _editingMaintenanceId ?? GenerateLegacyId(_dataSet.MaintenancePlans.Select(item => item.Id));
        var updatedPlan = new MaintenancePlan(
            maintenanceId,
            SelectedVehicle.Id,
            title,
            (MaintenanceEditorIntervalKm ?? string.Empty).Trim(),
            (MaintenanceEditorIntervalMonths ?? string.Empty).Trim(),
            (MaintenanceEditorLastServiceDate ?? string.Empty).Trim(),
            (MaintenanceEditorLastServiceOdometer ?? string.Empty).Trim(),
            MaintenanceEditorIsActive,
            (MaintenanceEditorNote ?? string.Empty).Trim());

        UpsertMaintenancePlan(updatedPlan);
        await PersistDataAndRestoreSelectionAsync(SelectedVehicle.Id, MaintenanceTabIndex, maintenanceId: maintenanceId);

        var wasNew = _editingMaintenanceId is null;
        CancelMaintenanceEditCore(clearStatus: false);
        MaintenanceEditorStatus = wasNew
            ? "Nový servisní plán byl uložen."
            : "Servisní plán byl upraven.";
        SelectedMaintenance = FindById(SelectedVehicleMaintenance, item => item.Id, maintenanceId);
        RequestFocus(DesktopFocusTarget.MaintenanceList);
    }

    [RelayCommand(CanExecute = nameof(CanCancelMaintenanceEdit))]
    private void CancelMaintenanceEdit()
    {
        CancelMaintenanceEditCore(clearStatus: true);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedMaintenance))]
    private async Task DeleteSelectedMaintenanceAsync()
    {
        if (SelectedVehicle is null || SelectedMaintenance is null)
        {
            return;
        }

        _dataSet.MaintenancePlans.RemoveAll(item => string.Equals(item.Id, SelectedMaintenance.Id, StringComparison.Ordinal));
        await PersistDataAndRestoreSelectionAsync(SelectedVehicle.Id, MaintenanceTabIndex);
        MaintenanceEditorStatus = "Servisní plán byl odstraněn.";
        RequestFocus(DesktopFocusTarget.MaintenanceList);
    }

    private VehicleHistoryEntry? GetSelectedHistoryModel()
    {
        if (SelectedHistory is null)
        {
            return null;
        }

        return _dataSet.HistoryEntries.FirstOrDefault(item => string.Equals(item.Id, SelectedHistory.Id, StringComparison.Ordinal));
    }

    private FuelEntry? GetSelectedFuelModel()
    {
        if (SelectedFuel is null)
        {
            return null;
        }

        return _dataSet.FuelEntries.FirstOrDefault(item => string.Equals(item.Id, SelectedFuel.Id, StringComparison.Ordinal));
    }

    private MaintenancePlan? GetSelectedMaintenanceModel()
    {
        if (SelectedMaintenance is null)
        {
            return null;
        }

        return _dataSet.MaintenancePlans.FirstOrDefault(item => string.Equals(item.Id, SelectedMaintenance.Id, StringComparison.Ordinal));
    }

    private void UpsertHistoryEntry(VehicleHistoryEntry updatedEntry)
    {
        var index = _dataSet.HistoryEntries.FindIndex(item => string.Equals(item.Id, updatedEntry.Id, StringComparison.Ordinal));
        if (index >= 0)
        {
            _dataSet.HistoryEntries[index] = updatedEntry;
        }
        else
        {
            _dataSet.HistoryEntries.Add(updatedEntry);
        }
    }

    private void UpsertFuelEntry(FuelEntry updatedEntry)
    {
        var index = _dataSet.FuelEntries.FindIndex(item => string.Equals(item.Id, updatedEntry.Id, StringComparison.Ordinal));
        if (index >= 0)
        {
            _dataSet.FuelEntries[index] = updatedEntry;
        }
        else
        {
            _dataSet.FuelEntries.Add(updatedEntry);
        }
    }

    private void UpsertMaintenancePlan(MaintenancePlan updatedPlan)
    {
        var index = _dataSet.MaintenancePlans.FindIndex(item => string.Equals(item.Id, updatedPlan.Id, StringComparison.Ordinal));
        if (index >= 0)
        {
            _dataSet.MaintenancePlans[index] = updatedPlan;
        }
        else
        {
            _dataSet.MaintenancePlans.Add(updatedPlan);
        }
    }

    private void CancelHistoryEditCore(bool clearStatus)
    {
        _editingHistoryId = null;
        IsEditingHistory = false;
        HistoryEditorDate = string.Empty;
        HistoryEditorType = string.Empty;
        HistoryEditorOdometer = string.Empty;
        HistoryEditorCost = string.Empty;
        HistoryEditorNote = string.Empty;
        if (clearStatus)
        {
            HistoryEditorStatus = string.Empty;
        }
    }

    private void CancelFuelEditCore(bool clearStatus)
    {
        _editingFuelId = null;
        IsEditingFuel = false;
        FuelEditorDate = string.Empty;
        FuelEditorFuelType = string.Empty;
        FuelEditorLiters = string.Empty;
        FuelEditorTotalCost = string.Empty;
        FuelEditorOdometer = string.Empty;
        FuelEditorFullTank = true;
        FuelEditorNote = string.Empty;
        if (clearStatus)
        {
            FuelEditorStatus = string.Empty;
        }
    }

    private void CancelMaintenanceEditCore(bool clearStatus)
    {
        _editingMaintenanceId = null;
        IsEditingMaintenance = false;
        MaintenanceEditorTitle = string.Empty;
        MaintenanceEditorIntervalKm = string.Empty;
        MaintenanceEditorIntervalMonths = string.Empty;
        MaintenanceEditorLastServiceDate = string.Empty;
        MaintenanceEditorLastServiceOdometer = string.Empty;
        MaintenanceEditorIsActive = true;
        MaintenanceEditorNote = string.Empty;
        if (clearStatus)
        {
            MaintenanceEditorStatus = string.Empty;
        }
    }
}
