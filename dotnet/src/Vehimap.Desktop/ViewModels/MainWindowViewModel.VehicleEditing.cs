using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private readonly VehicleStarterBundleService _vehicleStarterBundleService = new();
    private string? _editingVehicleId;
    private string? _pendingVehicleStarterBundleOfferVehicleId;

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

    [ObservableProperty]
    private string vehicleEditorClimateProfile = string.Empty;

    [ObservableProperty]
    private string vehicleEditorTimingDrive = string.Empty;

    [ObservableProperty]
    private string vehicleEditorTransmission = string.Empty;

    public bool IsVehicleDetailVisible => !IsEditingVehicle;

    public bool CanCreateVehicle => !IsEditingVehicle;

    public bool CanEditSelectedVehicle => SelectedVehicle is not null && !IsEditingVehicle;

    public bool CanSaveVehicle => IsEditingVehicle;

    public bool CanCancelVehicleEdit => IsEditingVehicle;

    public bool CanOpenVehicleStarterBundle => SelectedVehicle is not null && !IsEditingVehicle;

    partial void OnIsEditingVehicleChanged(bool value)
    {
        VehiclePanelHeading = value
            ? (_editingVehicleId is null ? "Nové vozidlo" : "Upravit vozidlo")
            : "Detail vozidla";

        OnPropertyChanged(nameof(IsVehicleDetailVisible));
        OnPropertyChanged(nameof(CanOpenVehicleStarterBundle));
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
        VehicleEditorClimateProfile = string.Empty;
        VehicleEditorTimingDrive = string.Empty;
        VehicleEditorTransmission = string.Empty;
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
        VehicleEditorClimateProfile = meta?.ClimateProfile ?? string.Empty;
        VehicleEditorTimingDrive = meta?.TimingDrive ?? string.Empty;
        VehicleEditorTransmission = meta?.Transmission ?? string.Empty;
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
        SelectedVehicle = FindById(Vehicles, item => item.Id, vehicleId);
        _pendingVehicleStarterBundleOfferVehicleId = wasNew ? vehicleId : null;

        VehicleEditorStatus = wasNew
            ? "Nové vozidlo bylo uloženo."
            : "Vozidlo bylo upraveno.";
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    [RelayCommand(CanExecute = nameof(CanCancelVehicleEdit))]
    private void CancelVehicleEdit()
    {
        CancelVehicleEditCore(clearStatus: true);
    }

    internal VehicleStarterBundlePreview BuildVehicleStarterBundlePreview(string vehicleId) =>
        _vehicleStarterBundleService.BuildPreview(_dataSet, vehicleId, DateOnly.FromDateTime(DateTime.Today));

    internal bool TryConsumePendingVehicleStarterBundleOffer(string vehicleId)
    {
        if (!string.Equals(_pendingVehicleStarterBundleOfferVehicleId, vehicleId, StringComparison.Ordinal))
        {
            return false;
        }

        _pendingVehicleStarterBundleOfferVehicleId = null;
        return true;
    }

    internal async Task<string> ApplyVehicleStarterBundleAsync(string vehicleId, IReadOnlyList<VehicleStarterBundleTemplate> items)
    {
        if (_dataRoot is null)
        {
            return "Balíček pro vozidlo nelze použít bez načtených dat.";
        }

        if (items.Count == 0)
        {
            return "Balíček pro vozidlo neobsahoval žádné vybrané položky.";
        }

        var maintenanceKeys = _dataSet.MaintenancePlans
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal))
            .Select(item => NormalizeBundleKey(item.Title))
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .ToHashSet(StringComparer.Ordinal);

        var recordKeys = _dataSet.Records
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal))
            .Select(item => BuildBundleRecordKey(item.RecordType, item.Title))
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .ToHashSet(StringComparer.Ordinal);

        var reminderKeys = _dataSet.Reminders
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal))
            .Select(item => BuildBundleReminderKey(item.Title, item.RepeatMode))
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .ToHashSet(StringComparer.Ordinal);

        var addedMaintenance = 0;
        var addedRecords = 0;
        var addedReminders = 0;

        foreach (var item in items)
        {
            switch (item.Section)
            {
                case VehicleStarterBundleSection.Maintenance:
                {
                    var key = NormalizeBundleKey(item.Title);
                    if (string.IsNullOrWhiteSpace(key) || maintenanceKeys.Contains(key))
                    {
                        break;
                    }

                    _dataSet.MaintenancePlans.Add(new MaintenancePlan(
                        GenerateLegacyId(_dataSet.MaintenancePlans.Select(entry => entry.Id)),
                        vehicleId,
                        item.Title.Trim(),
                        item.IntervalKm.Trim(),
                        item.IntervalMonths.Trim(),
                        string.Empty,
                        string.Empty,
                        true,
                        item.Note.Trim()));
                    maintenanceKeys.Add(key);
                    addedMaintenance++;
                    break;
                }
                case VehicleStarterBundleSection.Record:
                {
                    var recordType = string.IsNullOrWhiteSpace(item.RecordType) ? "Doklad" : item.RecordType.Trim();
                    var key = BuildBundleRecordKey(recordType, item.Title);
                    if (string.IsNullOrWhiteSpace(key) || recordKeys.Contains(key))
                    {
                        break;
                    }

                    _dataSet.Records.Add(new VehicleRecord(
                        GenerateLegacyId(_dataSet.Records.Select(entry => entry.Id)),
                        vehicleId,
                        recordType,
                        item.Title.Trim(),
                        item.Provider.Trim(),
                        item.ValidFrom.Trim(),
                        item.ValidTo.Trim(),
                        item.Price.Trim(),
                        VehicleRecordAttachmentMode.Managed,
                        string.Empty,
                        item.Note.Trim()));
                    recordKeys.Add(key);
                    addedRecords++;
                    break;
                }
                case VehicleStarterBundleSection.Reminder:
                {
                    var key = BuildBundleReminderKey(item.Title, item.RepeatMode);
                    if (string.IsNullOrWhiteSpace(key) || reminderKeys.Contains(key))
                    {
                        break;
                    }

                    _dataSet.Reminders.Add(new VehicleReminder(
                        GenerateLegacyId(_dataSet.Reminders.Select(entry => entry.Id)),
                        vehicleId,
                        item.Title.Trim(),
                        item.DueDate.Trim(),
                        item.ReminderDays.Trim(),
                        item.RepeatMode.Trim(),
                        item.Note.Trim()));
                    reminderKeys.Add(key);
                    addedReminders++;
                    break;
                }
            }
        }

        var addedCount = addedMaintenance + addedRecords + addedReminders;
        if (addedCount == 0)
        {
            return "Balíček pro vozidlo už neměl žádné nové položky k doplnění.";
        }

        await PersistDataAndRestoreSelectionAsync(vehicleId, DetailTabIndex);
        SelectedVehicle = FindById(Vehicles, item => item.Id, vehicleId);

        var parts = new List<string>();
        if (addedMaintenance > 0)
        {
            parts.Add($"{addedMaintenance}× servis");
        }

        if (addedRecords > 0)
        {
            parts.Add($"{addedRecords}× doklad");
        }

        if (addedReminders > 0)
        {
            parts.Add($"{addedReminders}× připomínka");
        }

        return $"Balíček pro vozidlo přidal {addedCount} položek: {string.Join(", ", parts)}.";
    }

    internal void SetVehicleStarterBundleStatus(string message)
    {
        VehicleEditorStatus = message;
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
            (VehicleEditorClimateProfile ?? string.Empty).Trim(),
            (VehicleEditorTimingDrive ?? string.Empty).Trim(),
            (VehicleEditorTransmission ?? string.Empty).Trim());

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
        VehicleEditorClimateProfile = string.Empty;
        VehicleEditorTimingDrive = string.Empty;
        VehicleEditorTransmission = string.Empty;
        if (clearStatus)
        {
            VehicleEditorStatus = string.Empty;
        }
    }

    private static string NormalizeBundleKey(string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    private static string BuildBundleRecordKey(string recordType, string title) =>
        $"{NormalizeBundleKey(recordType)}|{NormalizeBundleKey(title)}";

    private static string BuildBundleReminderKey(string title, string repeatMode) =>
        $"{NormalizeBundleKey(title)}|{NormalizeBundleKey(repeatMode)}";
}
