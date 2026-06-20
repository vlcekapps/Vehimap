using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private readonly VehicleStarterBundleService _vehicleStarterBundleService = new();
    private string? _editingVehicleId;
    private string? _pendingVehicleStarterBundleOfferVehicleId;

    public bool CanCreateVehicle => !HasPendingEdits;

    public bool CanEditSelectedVehicle => SelectedVehicle is not null && !HasPendingEdits;

    public bool CanDeleteSelectedVehicle => SelectedVehicle is not null && !HasPendingEdits;

    public bool CanSaveVehicle => VehicleDetailWorkspace.IsEditingVehicle;

    public bool CanCancelVehicleEdit => VehicleDetailWorkspace.IsEditingVehicle;

    public bool CanOpenVehicleStarterBundle => SelectedVehicle is not null && !HasPendingEdits;

    private void SetVehicleEditingState(bool value)
    {
        VehicleDetailWorkspace.SetVehicleEditingState(value, _editingVehicleId is null);
        OnPropertyChanged(nameof(CanCreateVehicle));
        OnPropertyChanged(nameof(CanEditSelectedVehicle));
        OnPropertyChanged(nameof(CanDeleteSelectedVehicle));
        OnPropertyChanged(nameof(CanSaveVehicle));
        OnPropertyChanged(nameof(CanCancelVehicleEdit));
        OnPropertyChanged(nameof(CanOpenSelectedVehicleCosts));
        OnPropertyChanged(nameof(CanOpenVehicleStarterBundle));
        CreateVehicleCommand.NotifyCanExecuteChanged();
        EditSelectedVehicleCommand.NotifyCanExecuteChanged();
        DeleteSelectedVehicleCommand.NotifyCanExecuteChanged();
        OpenSelectedVehicleCostsCommand.NotifyCanExecuteChanged();
        SaveVehicleCommand.NotifyCanExecuteChanged();
        CancelVehicleEditCommand.NotifyCanExecuteChanged();
        NotifyPendingEditStateChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCreateVehicle))]
    private void CreateVehicle()
    {
        _editingVehicleId = null;
        VehicleDetailWorkspace.VehicleEditorName = string.Empty;
        VehicleDetailWorkspace.VehicleEditorCategory = "Osobní vozidla";
        VehicleDetailWorkspace.VehicleEditorNote = string.Empty;
        VehicleDetailWorkspace.VehicleEditorMakeModel = string.Empty;
        VehicleDetailWorkspace.VehicleEditorPlate = string.Empty;
        VehicleDetailWorkspace.VehicleEditorYear = string.Empty;
        VehicleDetailWorkspace.VehicleEditorPower = string.Empty;
        VehicleDetailWorkspace.VehicleEditorLastTk = string.Empty;
        VehicleDetailWorkspace.VehicleEditorNextTk = string.Empty;
        VehicleDetailWorkspace.VehicleEditorGreenCardFrom = string.Empty;
        VehicleDetailWorkspace.VehicleEditorGreenCardTo = string.Empty;
        VehicleDetailWorkspace.VehicleEditorState = string.Empty;
        VehicleDetailWorkspace.VehicleEditorTags = string.Empty;
        VehicleDetailWorkspace.VehicleEditorPowertrain = string.Empty;
        VehicleDetailWorkspace.VehicleEditorClimateProfile = string.Empty;
        VehicleDetailWorkspace.VehicleEditorTimingDrive = string.Empty;
        VehicleDetailWorkspace.VehicleEditorTransmission = string.Empty;
        VehicleDetailWorkspace.VehicleEditorStatus = "Vyplňte základní údaje o vozidle a uložte je.";
        SetVehicleEditingState(true);
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
        VehicleDetailWorkspace.VehicleEditorName = vehicle.Name;
        VehicleDetailWorkspace.VehicleEditorCategory = vehicle.Category;
        VehicleDetailWorkspace.VehicleEditorNote = vehicle.VehicleNote;
        VehicleDetailWorkspace.VehicleEditorMakeModel = vehicle.MakeModel;
        VehicleDetailWorkspace.VehicleEditorPlate = vehicle.Plate;
        VehicleDetailWorkspace.VehicleEditorYear = vehicle.Year;
        VehicleDetailWorkspace.VehicleEditorPower = vehicle.Power;
        VehicleDetailWorkspace.VehicleEditorLastTk = vehicle.LastTk;
        VehicleDetailWorkspace.VehicleEditorNextTk = vehicle.NextTk;
        VehicleDetailWorkspace.VehicleEditorGreenCardFrom = vehicle.GreenCardFrom;
        VehicleDetailWorkspace.VehicleEditorGreenCardTo = vehicle.GreenCardTo;
        VehicleDetailWorkspace.VehicleEditorState = LegacyVehicleValueNormalization.NormalizeVehicleState(meta?.State);
        VehicleDetailWorkspace.VehicleEditorTags = meta?.Tags ?? string.Empty;
        VehicleDetailWorkspace.VehicleEditorPowertrain = LegacyVehicleValueNormalization.NormalizeVehiclePowertrain(meta?.Powertrain);
        VehicleDetailWorkspace.VehicleEditorClimateProfile = LegacyVehicleValueNormalization.NormalizeVehicleClimateProfile(meta?.ClimateProfile);
        VehicleDetailWorkspace.VehicleEditorTimingDrive = LegacyVehicleValueNormalization.NormalizeVehicleTimingDrive(meta?.TimingDrive);
        VehicleDetailWorkspace.VehicleEditorTransmission = LegacyVehicleValueNormalization.NormalizeVehicleTransmission(meta?.Transmission);
        VehicleDetailWorkspace.VehicleEditorStatus = "Upravte údaje vozidla a uložte změny.";
        SetVehicleEditingState(true);
        SelectedVehicleTabIndex = DetailTabIndex;
        RequestFocus(DesktopFocusTarget.VehicleEditorName);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedVehicle))]
    private async Task DeleteSelectedVehicleAsync()
    {
        if (SelectedVehicle is null || _dataRoot is null)
        {
            return;
        }

        if (_confirmVehicleDeleteHandler is null)
        {
            return;
        }

        var vehicleId = SelectedVehicle.Id;
        var vehicleName = SelectedVehicle.Name;
        var confirmationMessage = BuildVehicleDeleteConfirmationMessage(vehicleId, vehicleName);
        if (!await _confirmVehicleDeleteHandler(confirmationMessage).ConfigureAwait(true))
        {
            return;
        }

        var nextVehicleId = GetNextVisibleVehicleIdAfterDelete(vehicleId);
        _dataSet.Vehicles.RemoveAll(item => string.Equals(item.Id, vehicleId, StringComparison.Ordinal));
        _dataSet.HistoryEntries.RemoveAll(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        _dataSet.FuelEntries.RemoveAll(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        _dataSet.Records.RemoveAll(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        _dataSet.Reminders.RemoveAll(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        _dataSet.MaintenancePlans.RemoveAll(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        _dataSet.VehicleMetaEntries.RemoveAll(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));

        var attachmentCleanupWarning = DeleteManagedAttachmentDirectory(vehicleId);
        await _session.PersistAsync().ConfigureAwait(true);
        Load(nextVehicleId, DetailTabIndex, applyLaunchTabPreference: false);

        var status = $"Vozidlo {vehicleName} bylo odstraněno.";
        if (!string.IsNullOrWhiteSpace(attachmentCleanupWarning))
        {
            status += $" {attachmentCleanupWarning}";
        }

        VehicleDetailWorkspace.VehicleEditorStatus = status;
        ShellStatus = status;
        SelectedVehicleTabIndex = DetailTabIndex;
        RequestFocus(DesktopFocusTarget.VehicleDetailPrimaryAction);
    }

    [RelayCommand(CanExecute = nameof(CanSaveVehicle))]
    private async Task SaveVehicleAsync()
    {
        if (_dataRoot is null)
        {
            return;
        }

        var name = (VehicleDetailWorkspace.VehicleEditorName ?? string.Empty).Trim();
        var category = (VehicleDetailWorkspace.VehicleEditorCategory ?? string.Empty).Trim();
        var makeModel = (VehicleDetailWorkspace.VehicleEditorMakeModel ?? string.Empty).Trim();
        var plate = (VehicleDetailWorkspace.VehicleEditorPlate ?? string.Empty).Trim().ToUpperInvariant();
        var year = (VehicleDetailWorkspace.VehicleEditorYear ?? string.Empty).Trim();
        var lastTkText = (VehicleDetailWorkspace.VehicleEditorLastTk ?? string.Empty).Trim();
        var nextTkText = (VehicleDetailWorkspace.VehicleEditorNextTk ?? string.Empty).Trim();
        var greenCardFromText = (VehicleDetailWorkspace.VehicleEditorGreenCardFrom ?? string.Empty).Trim();
        var greenCardToText = (VehicleDetailWorkspace.VehicleEditorGreenCardTo ?? string.Empty).Trim();
        var lastTk = LegacyVehicleValueNormalization.NormalizeMonthYear(lastTkText);
        var nextTk = LegacyVehicleValueNormalization.NormalizeMonthYear(nextTkText);
        var greenCardFrom = LegacyVehicleValueNormalization.NormalizeMonthYear(greenCardFromText);
        var greenCardTo = LegacyVehicleValueNormalization.NormalizeMonthYear(greenCardToText);

        if (string.IsNullOrWhiteSpace(name))
        {
            VehicleDetailWorkspace.VehicleEditorStatus = "Vozidlo musí mít název.";
            RequestFocus(DesktopFocusTarget.VehicleEditorName);
            return;
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            VehicleDetailWorkspace.VehicleEditorStatus = "Vozidlo musí mít kategorii.";
            RequestFocus(DesktopFocusTarget.VehicleEditorCategory);
            return;
        }

        if (string.IsNullOrWhiteSpace(makeModel))
        {
            VehicleDetailWorkspace.VehicleEditorStatus = "Vozidlo musí mít vyplněnou značku a model.";
            RequestFocus(DesktopFocusTarget.VehicleEditorMakeModel);
            return;
        }

        if (lastTkText.Length > 0 && lastTk.Length == 0)
        {
            VehicleDetailWorkspace.VehicleEditorStatus = "Pole Poslední TK musí být ve formátu MM/RRRR.";
            RequestFocus(DesktopFocusTarget.VehicleEditorLastTk);
            return;
        }

        if (nextTk.Length == 0)
        {
            VehicleDetailWorkspace.VehicleEditorStatus = "Pole Příští TK je povinné a musí být ve formátu MM/RRRR.";
            RequestFocus(DesktopFocusTarget.VehicleEditorNextTk);
            return;
        }

        if (greenCardFromText.Length > 0 && greenCardFrom.Length == 0)
        {
            VehicleDetailWorkspace.VehicleEditorStatus = "Pole Zelená karta od musí být ve formátu MM/RRRR.";
            RequestFocus(DesktopFocusTarget.VehicleEditorGreenCardFrom);
            return;
        }

        if (greenCardToText.Length > 0 && greenCardTo.Length == 0)
        {
            VehicleDetailWorkspace.VehicleEditorStatus = "Pole Zelená karta do musí být ve formátu MM/RRRR.";
            RequestFocus(DesktopFocusTarget.VehicleEditorGreenCardTo);
            return;
        }

        if (LegacyVehicleValueNormalization.TryGetMonthYearOrder(greenCardFrom, out var greenCardFromOrder)
            && LegacyVehicleValueNormalization.TryGetMonthYearOrder(greenCardTo, out var greenCardToOrder)
            && greenCardFromOrder > greenCardToOrder)
        {
            VehicleDetailWorkspace.VehicleEditorStatus = "Pole Zelená karta od nesmí být později než pole Zelená karta do.";
            RequestFocus(DesktopFocusTarget.VehicleEditorGreenCardFrom);
            return;
        }

        if (year.Length > 0 && (year.Length != 4 || !year.All(char.IsDigit)))
        {
            VehicleDetailWorkspace.VehicleEditorStatus = "Rok výroby zadejte jako čtyřciferný rok, nebo pole nechte prázdné.";
            RequestFocus(DesktopFocusTarget.VehicleEditorYear);
            return;
        }

        var vehicleId = _editingVehicleId ?? GenerateLegacyId(_dataSet.Vehicles.Select(item => item.Id));
        var existingMeta = _dataSet.VehicleMetaEntries.FirstOrDefault(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));

        var updatedVehicle = new Vehicle(
            vehicleId,
            name,
            LegacyVehicleValueNormalization.NormalizeCategory(category),
            (VehicleDetailWorkspace.VehicleEditorNote ?? string.Empty).Trim(),
            makeModel,
            plate,
            year,
            (VehicleDetailWorkspace.VehicleEditorPower ?? string.Empty).Trim(),
            lastTk,
            nextTk,
            greenCardFrom,
            greenCardTo);

        UpsertVehicle(updatedVehicle);
        UpsertVehicleMeta(BuildUpdatedVehicleMeta(vehicleId, existingMeta));

        var wasNew = _editingVehicleId is null;
        await PersistDataAndRestoreSelectionAsync(vehicleId, DetailTabIndex);

        CancelVehicleEditCore(clearStatus: false);
        SelectedVehicle = FindById(Vehicles, item => item.Id, vehicleId);
        _pendingVehicleStarterBundleOfferVehicleId = wasNew ? vehicleId : null;

        VehicleDetailWorkspace.VehicleEditorStatus = wasNew
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

    internal VehicleStarterBundlePreview BuildMaintenanceTemplatePreview(string vehicleId)
    {
        var preview = BuildVehicleStarterBundlePreview(vehicleId);
        var maintenanceItems = preview.Items
            .Where(item => item.Section == VehicleStarterBundleSection.Maintenance)
            .ToList();

        return new VehicleStarterBundlePreview(
            preview.VehicleId,
            preview.VehicleName,
            preview.ProfileLabel,
            maintenanceItems);
    }

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
                    var recordType = LegacyVehicleValueNormalization.NormalizeRecordType(item.RecordType);
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
                    var repeatMode = LegacyVehicleValueNormalization.NormalizeReminderRepeatMode(item.RepeatMode);
                    var key = BuildBundleReminderKey(item.Title, repeatMode);
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
                        repeatMode,
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

    internal async Task<string> ApplyMaintenanceTemplatesAsync(string vehicleId, IReadOnlyList<VehicleStarterBundleTemplate> items)
    {
        if (_dataRoot is null)
        {
            return "Doporučené šablony nelze použít bez načtených dat.";
        }

        var maintenanceItems = items
            .Where(item => item.Section == VehicleStarterBundleSection.Maintenance)
            .ToList();

        if (maintenanceItems.Count == 0)
        {
            return "Doporučené šablony neobsahovaly žádné vybrané servisní plány.";
        }

        var maintenanceKeys = _dataSet.MaintenancePlans
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal))
            .Select(item => NormalizeBundleKey(item.Title))
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .ToHashSet(StringComparer.Ordinal);

        var addedMaintenance = 0;
        foreach (var item in maintenanceItems)
        {
            var key = NormalizeBundleKey(item.Title);
            if (string.IsNullOrWhiteSpace(key) || maintenanceKeys.Contains(key))
            {
                continue;
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
        }

        if (addedMaintenance == 0)
        {
            return "Doporučené šablony už neměly žádné nové servisní plány k doplnění.";
        }

        await PersistDataAndRestoreSelectionAsync(vehicleId, MaintenanceTabIndex);
        SelectedVehicle = FindById(Vehicles, item => item.Id, vehicleId);

        return $"Doporučené šablony přidaly {addedMaintenance} servisních plánů.";
    }

    internal void SetVehicleStarterBundleStatus(string message)
    {
        VehicleDetailWorkspace.VehicleEditorStatus = message;
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

    private string BuildVehicleDeleteConfirmationMessage(string vehicleId, string vehicleName)
    {
        var historyCount = _dataSet.HistoryEntries.Count(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        var fuelCount = _dataSet.FuelEntries.Count(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        var recordCount = _dataSet.Records.Count(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        var reminderCount = _dataSet.Reminders.Count(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        var maintenanceCount = _dataSet.MaintenancePlans.Count(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));

        return string.Join(
            Environment.NewLine,
            [
                $"Opravdu chcete odstranit vozidlo „{vehicleName}“?",
                string.Empty,
                "Současně se odstraní navázaná evidence:",
                $"- historie: {historyCount}",
                $"- tankování: {fuelCount}",
                $"- doklady: {recordCount}",
                $"- připomínky: {reminderCount}",
                $"- údržba: {maintenanceCount}",
                string.Empty,
                "Spravované přílohy tohoto vozidla budou smazány z datové složky. Externí soubory dokladů zůstanou beze změny.",
                string.Empty,
                "Tuto akci nelze vrátit zpět."
            ]);
    }

    private string? GetNextVisibleVehicleIdAfterDelete(string vehicleId)
    {
        var selectedIndex = Vehicles.ToList().FindIndex(item => string.Equals(item.Id, vehicleId, StringComparison.Ordinal));
        if (selectedIndex < 0)
        {
            return _dataSet.Vehicles.FirstOrDefault(item => !string.Equals(item.Id, vehicleId, StringComparison.Ordinal))?.Id;
        }

        if (selectedIndex + 1 < Vehicles.Count)
        {
            return Vehicles[selectedIndex + 1].Id;
        }

        if (selectedIndex > 0)
        {
            return Vehicles[selectedIndex - 1].Id;
        }

        return _dataSet.Vehicles.FirstOrDefault(item => !string.Equals(item.Id, vehicleId, StringComparison.Ordinal))?.Id;
    }

    private string? DeleteManagedAttachmentDirectory(string vehicleId)
    {
        if (_dataRoot is null || string.IsNullOrWhiteSpace(vehicleId))
        {
            return null;
        }

        try
        {
            var attachmentsRoot = Path.GetFullPath(Path.Combine(_dataRoot.DataPath, "attachments"));
            var vehicleAttachmentDirectory = Path.GetFullPath(Path.Combine(attachmentsRoot, vehicleId));
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var safeRootPrefix = attachmentsRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (!vehicleAttachmentDirectory.StartsWith(safeRootPrefix, comparison))
            {
                return "Spravované přílohy nebyly smazány, protože cílová cesta neleží v datové složce příloh.";
            }

            if (Directory.Exists(vehicleAttachmentDirectory))
            {
                Directory.Delete(vehicleAttachmentDirectory, recursive: true);
            }

            return null;
        }
        catch (Exception ex)
        {
            return $"Spravované přílohy se nepodařilo smazat: {ex.Message}";
        }
    }

    private VehicleMeta? BuildUpdatedVehicleMeta(string vehicleId, VehicleMeta? existingMeta)
    {
        var updatedMeta = new VehicleMeta(
            vehicleId,
            LegacyVehicleValueNormalization.NormalizeVehicleState(VehicleDetailWorkspace.VehicleEditorState),
            LegacyVehicleMetaNormalization.NormalizeTagList(VehicleDetailWorkspace.VehicleEditorTags),
            LegacyVehicleValueNormalization.NormalizeVehiclePowertrain(VehicleDetailWorkspace.VehicleEditorPowertrain),
            LegacyVehicleValueNormalization.NormalizeVehicleClimateProfile(VehicleDetailWorkspace.VehicleEditorClimateProfile),
            LegacyVehicleValueNormalization.NormalizeVehicleTimingDrive(VehicleDetailWorkspace.VehicleEditorTimingDrive),
            LegacyVehicleValueNormalization.NormalizeVehicleTransmission(VehicleDetailWorkspace.VehicleEditorTransmission));

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
        SetVehicleEditingState(false);
        VehicleDetailWorkspace.VehicleEditorName = string.Empty;
        VehicleDetailWorkspace.VehicleEditorCategory = string.Empty;
        VehicleDetailWorkspace.VehicleEditorNote = string.Empty;
        VehicleDetailWorkspace.VehicleEditorMakeModel = string.Empty;
        VehicleDetailWorkspace.VehicleEditorPlate = string.Empty;
        VehicleDetailWorkspace.VehicleEditorYear = string.Empty;
        VehicleDetailWorkspace.VehicleEditorPower = string.Empty;
        VehicleDetailWorkspace.VehicleEditorLastTk = string.Empty;
        VehicleDetailWorkspace.VehicleEditorNextTk = string.Empty;
        VehicleDetailWorkspace.VehicleEditorGreenCardFrom = string.Empty;
        VehicleDetailWorkspace.VehicleEditorGreenCardTo = string.Empty;
        VehicleDetailWorkspace.VehicleEditorState = string.Empty;
        VehicleDetailWorkspace.VehicleEditorTags = string.Empty;
        VehicleDetailWorkspace.VehicleEditorPowertrain = string.Empty;
        VehicleDetailWorkspace.VehicleEditorClimateProfile = string.Empty;
        VehicleDetailWorkspace.VehicleEditorTimingDrive = string.Empty;
        VehicleDetailWorkspace.VehicleEditorTransmission = string.Empty;
        if (clearStatus)
        {
            VehicleDetailWorkspace.VehicleEditorStatus = string.Empty;
        }
    }

    private static string NormalizeBundleKey(string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    private static string BuildBundleRecordKey(string recordType, string title) =>
        $"{NormalizeBundleKey(recordType)}|{NormalizeBundleKey(title)}";

    private static string BuildBundleReminderKey(string title, string repeatMode) =>
        $"{NormalizeBundleKey(title)}|{NormalizeBundleKey(LegacyVehicleValueNormalization.NormalizeReminderRepeatMode(repeatMode))}";
}
