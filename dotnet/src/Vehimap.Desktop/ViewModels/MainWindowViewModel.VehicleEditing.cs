using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private string? _editingVehicleId;

    [ObservableProperty]
    private bool isEditingVehicle;

    [ObservableProperty]
    private string vehiclePanelHeading = "Detail vozidla";

    [ObservableProperty]
    private string vehicleEditorStatus = string.Empty;

    [ObservableProperty]
    private string vehicleEditorName = string.Empty;

    [ObservableProperty]
    private string vehicleEditorCategory = string.Empty;

    [ObservableProperty]
    private string vehicleEditorNote = string.Empty;

    [ObservableProperty]
    private string vehicleEditorMakeModel = string.Empty;

    [ObservableProperty]
    private string vehicleEditorPlate = string.Empty;

    [ObservableProperty]
    private string vehicleEditorYear = string.Empty;

    [ObservableProperty]
    private string vehicleEditorPower = string.Empty;

    [ObservableProperty]
    private string vehicleEditorLastTk = string.Empty;

    [ObservableProperty]
    private string vehicleEditorNextTk = string.Empty;

    [ObservableProperty]
    private string vehicleEditorGreenCardFrom = string.Empty;

    [ObservableProperty]
    private string vehicleEditorGreenCardTo = string.Empty;

    [ObservableProperty]
    private string vehicleEditorState = string.Empty;

    [ObservableProperty]
    private string vehicleEditorPowertrain = string.Empty;

    public bool IsVehicleDetailVisible => !IsEditingVehicle;

    public bool CanCreateVehicle => !IsEditingVehicle;

    public bool CanEditSelectedVehicle => SelectedVehicle is not null && !IsEditingVehicle;

    public bool CanSaveVehicle => IsEditingVehicle;

    public bool CanCancelVehicleEdit => IsEditingVehicle;

    partial void OnIsEditingVehicleChanged(bool value)
    {
        VehiclePanelHeading = value
            ? (_editingVehicleId is null ? "Nové vozidlo" : "Upravit vozidlo")
            : "Detail vozidla";

        OnPropertyChanged(nameof(IsVehicleDetailVisible));
        CreateVehicleCommand.NotifyCanExecuteChanged();
        EditSelectedVehicleCommand.NotifyCanExecuteChanged();
        SaveVehicleCommand.NotifyCanExecuteChanged();
        CancelVehicleEditCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCreateVehicle))]
    private void CreateVehicle()
    {
        _editingVehicleId = null;
        VehicleEditorName = string.Empty;
        VehicleEditorCategory = "Osobní vozidla";
        VehicleEditorNote = string.Empty;
        VehicleEditorMakeModel = string.Empty;
        VehicleEditorPlate = string.Empty;
        VehicleEditorYear = string.Empty;
        VehicleEditorPower = string.Empty;
        VehicleEditorLastTk = string.Empty;
        VehicleEditorNextTk = string.Empty;
        VehicleEditorGreenCardFrom = string.Empty;
        VehicleEditorGreenCardTo = string.Empty;
        VehicleEditorState = string.Empty;
        VehicleEditorPowertrain = string.Empty;
        VehicleEditorStatus = "Vyplňte základní údaje o vozidle a uložte je.";
        IsEditingVehicle = true;
        SelectedVehicleTabIndex = DetailTabIndex;
        RequestFocus(DesktopFocusTarget.VehicleEditorName);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelectedVehicle))]
    private void EditSelectedVehicle()
    {
        var vehicle = GetSelectedVehicleModel();
        if (vehicle is null)
        {
            return;
        }

        var meta = GetSelectedVehicleMetaModel();
        _editingVehicleId = vehicle.Id;
        VehicleEditorName = vehicle.Name;
        VehicleEditorCategory = vehicle.Category;
        VehicleEditorNote = vehicle.VehicleNote;
        VehicleEditorMakeModel = vehicle.MakeModel;
        VehicleEditorPlate = vehicle.Plate;
        VehicleEditorYear = vehicle.Year;
        VehicleEditorPower = vehicle.Power;
        VehicleEditorLastTk = vehicle.LastTk;
        VehicleEditorNextTk = vehicle.NextTk;
        VehicleEditorGreenCardFrom = vehicle.GreenCardFrom;
        VehicleEditorGreenCardTo = vehicle.GreenCardTo;
        VehicleEditorState = meta?.State ?? string.Empty;
        VehicleEditorPowertrain = meta?.Powertrain ?? string.Empty;
        VehicleEditorStatus = "Upravte údaje vozidla a uložte změny.";
        IsEditingVehicle = true;
        SelectedVehicleTabIndex = DetailTabIndex;
        RequestFocus(DesktopFocusTarget.VehicleEditorName);
    }

    [RelayCommand(CanExecute = nameof(CanSaveVehicle))]
    private async Task SaveVehicleAsync()
    {
        if (_dataRoot is null)
        {
            return;
        }

        var name = (VehicleEditorName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            VehicleEditorStatus = "Vozidlo musí mít název.";
            RequestFocus(DesktopFocusTarget.VehicleEditorName);
            return;
        }

        var vehicleId = _editingVehicleId ?? GenerateLegacyId(_dataSet.Vehicles.Select(item => item.Id));
        var existingMeta = _dataSet.VehicleMetaEntries.FirstOrDefault(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));

        var updatedVehicle = new Vehicle(
            vehicleId,
            name,
            (VehicleEditorCategory ?? string.Empty).Trim(),
            (VehicleEditorNote ?? string.Empty).Trim(),
            (VehicleEditorMakeModel ?? string.Empty).Trim(),
            (VehicleEditorPlate ?? string.Empty).Trim(),
            (VehicleEditorYear ?? string.Empty).Trim(),
            (VehicleEditorPower ?? string.Empty).Trim(),
            (VehicleEditorLastTk ?? string.Empty).Trim(),
            (VehicleEditorNextTk ?? string.Empty).Trim(),
            (VehicleEditorGreenCardFrom ?? string.Empty).Trim(),
            (VehicleEditorGreenCardTo ?? string.Empty).Trim());

        UpsertVehicle(updatedVehicle);
        UpsertVehicleMeta(BuildUpdatedVehicleMeta(vehicleId, existingMeta));

        var wasNew = _editingVehicleId is null;
        await PersistDataAndRestoreSelectionAsync(vehicleId, DetailTabIndex);

        CancelVehicleEditCore(clearStatus: false);
        VehicleEditorStatus = wasNew
            ? "Nové vozidlo bylo uloženo."
            : "Vozidlo bylo upraveno.";
        SelectedVehicle = FindById(Vehicles, item => item.Id, vehicleId);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    [RelayCommand(CanExecute = nameof(CanCancelVehicleEdit))]
    private void CancelVehicleEdit()
    {
        CancelVehicleEditCore(clearStatus: true);
    }

    private Vehicle? GetSelectedVehicleModel()
    {
        if (SelectedVehicle is null)
        {
            return null;
        }

        return _dataSet.Vehicles.FirstOrDefault(item => string.Equals(item.Id, SelectedVehicle.Id, StringComparison.Ordinal));
    }

    private VehicleMeta? GetSelectedVehicleMetaModel()
    {
        if (SelectedVehicle is null)
        {
            return null;
        }

        return _dataSet.VehicleMetaEntries.FirstOrDefault(item => string.Equals(item.VehicleId, SelectedVehicle.Id, StringComparison.Ordinal));
    }

    private VehicleMeta? BuildUpdatedVehicleMeta(string vehicleId, VehicleMeta? existingMeta)
    {
        var updatedMeta = new VehicleMeta(
            vehicleId,
            (VehicleEditorState ?? string.Empty).Trim(),
            existingMeta?.Tags ?? string.Empty,
            (VehicleEditorPowertrain ?? string.Empty).Trim(),
            existingMeta?.ClimateProfile ?? string.Empty,
            existingMeta?.TimingDrive ?? string.Empty,
            existingMeta?.Transmission ?? string.Empty);

        if (string.IsNullOrWhiteSpace(updatedMeta.State)
            && string.IsNullOrWhiteSpace(updatedMeta.Tags)
            && string.IsNullOrWhiteSpace(updatedMeta.Powertrain)
            && string.IsNullOrWhiteSpace(updatedMeta.ClimateProfile)
            && string.IsNullOrWhiteSpace(updatedMeta.TimingDrive)
            && string.IsNullOrWhiteSpace(updatedMeta.Transmission))
        {
            return null;
        }

        return updatedMeta;
    }

    private void UpsertVehicle(Vehicle updatedVehicle)
    {
        var index = _dataSet.Vehicles.FindIndex(item => string.Equals(item.Id, updatedVehicle.Id, StringComparison.Ordinal));
        if (index >= 0)
        {
            _dataSet.Vehicles[index] = updatedVehicle;
        }
        else
        {
            _dataSet.Vehicles.Add(updatedVehicle);
        }
    }

    private void UpsertVehicleMeta(VehicleMeta? updatedMeta)
    {
        if (updatedMeta is null)
        {
            if (!string.IsNullOrWhiteSpace(_editingVehicleId))
            {
                _dataSet.VehicleMetaEntries.RemoveAll(item => string.Equals(item.VehicleId, _editingVehicleId, StringComparison.Ordinal));
            }

            return;
        }

        var index = _dataSet.VehicleMetaEntries.FindIndex(item => string.Equals(item.VehicleId, updatedMeta.VehicleId, StringComparison.Ordinal));
        if (index >= 0)
        {
            _dataSet.VehicleMetaEntries[index] = updatedMeta;
        }
        else
        {
            _dataSet.VehicleMetaEntries.Add(updatedMeta);
        }
    }

    private void CancelVehicleEditCore(bool clearStatus)
    {
        _editingVehicleId = null;
        IsEditingVehicle = false;
        VehicleEditorName = string.Empty;
        VehicleEditorCategory = string.Empty;
        VehicleEditorNote = string.Empty;
        VehicleEditorMakeModel = string.Empty;
        VehicleEditorPlate = string.Empty;
        VehicleEditorYear = string.Empty;
        VehicleEditorPower = string.Empty;
        VehicleEditorLastTk = string.Empty;
        VehicleEditorNextTk = string.Empty;
        VehicleEditorGreenCardFrom = string.Empty;
        VehicleEditorGreenCardTo = string.Empty;
        VehicleEditorState = string.Empty;
        VehicleEditorPowertrain = string.Empty;
        if (clearStatus)
        {
            VehicleEditorStatus = string.Empty;
        }
    }
}
