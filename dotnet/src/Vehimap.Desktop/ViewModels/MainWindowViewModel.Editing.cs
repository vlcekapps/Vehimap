// SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private string? _editingReminderId;
    private string? _editingRecordId;

    internal void HandleVehicleSelectionChanged()
    {
        CancelVehicleEditCore(clearStatus: true);
        CancelHistoryEditCore(clearStatus: true);
        CancelFuelEditCore(clearStatus: true);
        CancelReminderEditCore(clearStatus: true);
        CancelMaintenanceEditCore(clearStatus: true);
        CancelRecordEditCore(clearStatus: true);
        CreateVehicleCommand.NotifyCanExecuteChanged();
        EditSelectedVehicleCommand.NotifyCanExecuteChanged();
        DeleteSelectedVehicleCommand.NotifyCanExecuteChanged();
        OpenSelectedVehicleCostsCommand.NotifyCanExecuteChanged();
        CreateHistoryCommand.NotifyCanExecuteChanged();
        CreateFuelCommand.NotifyCanExecuteChanged();
        CreateReminderCommand.NotifyCanExecuteChanged();
        AdvanceSelectedReminderCommand.NotifyCanExecuteChanged();
        CreateMaintenanceCommand.NotifyCanExecuteChanged();
        CompleteSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        CreateRecordCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCreateReminder))]
    private void CreateReminder()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        _editingReminderId = null;
        ReminderEditorTitle = string.Empty;
        ReminderEditorDueDate = string.Empty;
        ReminderEditorDays = "30";
        ReminderEditorRepeatMode = "Neopakovat";
        ReminderEditorNote = string.Empty;
        ReminderEditorStatus = LO("ReminderEditor.Status.CreatePrompt");
        IsEditingReminder = true;
        SelectedVehicleTabIndex = ReminderTabIndex;
        RequestWorkspaceEditorDialog(WorkspaceEditorKind.Reminder, DesktopFocusTarget.ReminderList);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelectedReminder))]
    private void EditSelectedReminder()
    {
        var reminder = GetSelectedReminderModel();
        if (reminder is null)
        {
            return;
        }

        _editingReminderId = reminder.Id;
        ReminderEditorTitle = reminder.Title;
        ReminderEditorDueDate = reminder.DueDate;
        ReminderEditorDays = reminder.ReminderDays;
        ReminderEditorRepeatMode = LegacyVehicleValueNormalization.NormalizeReminderRepeatMode(reminder.RepeatMode);
        ReminderEditorNote = reminder.Note;
        ReminderEditorStatus = LO("ReminderEditor.Status.EditPrompt");
        IsEditingReminder = true;
        SelectedVehicleTabIndex = ReminderTabIndex;
        RequestWorkspaceEditorDialog(WorkspaceEditorKind.Reminder, DesktopFocusTarget.ReminderList);
    }

    [RelayCommand(CanExecute = nameof(CanSaveReminder))]
    private async Task SaveReminderAsync()
    {
        if (SelectedVehicle is null || _dataRoot is null)
        {
            return;
        }

        var title = (ReminderEditorTitle ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            ReminderEditorStatus = LO("ReminderEditor.Validation.TitleRequired");
            RequestFocus(DesktopFocusTarget.ReminderEditorTitle);
            return;
        }

        var dueDateText = (ReminderEditorDueDate ?? string.Empty).Trim();
        var dueDate = LegacyVehicleValueNormalization.NormalizeEventDate(dueDateText);
        if (dueDate.Length == 0)
        {
            ReminderEditorStatus = LO("ReminderEditor.Validation.DueDateRequired");
            RequestFocus(DesktopFocusTarget.ReminderEditorDueDate);
            return;
        }

        var reminderDaysText = (ReminderEditorDays ?? string.Empty).Trim();
        var reminderDays = LegacyVehicleValueNormalization.NormalizeReminderDays(reminderDaysText);
        if (reminderDays.Length == 0)
        {
            ReminderEditorStatus = LO("ReminderEditor.Validation.ReminderDaysInvalid");
            RequestFocus(DesktopFocusTarget.ReminderEditorDays);
            return;
        }

        var reminderId = _editingReminderId ?? GenerateLegacyId(_dataSet.Reminders.Select(item => item.Id));
        var updatedReminder = new VehicleReminder(
            reminderId,
            SelectedVehicle.Id,
            title,
            dueDate,
            reminderDays,
            LegacyVehicleValueNormalization.NormalizeReminderRepeatMode(ReminderEditorRepeatMode),
            (ReminderEditorNote ?? string.Empty).Trim());

        var rollbackDataSet = CloneDataSet(_dataSet);
        UpsertReminder(updatedReminder);
        var wasNew = _editingReminderId is null;

        if (!await PersistDataAndRestoreSelectionAsync(
                SelectedVehicle.Id,
                ReminderTabIndex,
                reminderId: reminderId,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => ReminderEditorStatus = status,
                failureFocus: DesktopFocusTarget.ReminderEditorTitle,
                failurePrefix: LO("ReminderEditor.Persistence.SaveFailed")))
        {
            return;
        }

        CancelReminderEditCore(clearStatus: false);
        ReminderEditorStatus = wasNew
            ? LO("ReminderEditor.Status.Created")
            : LO("ReminderEditor.Status.Updated");
        SelectedReminder = FindById(SelectedVehicleReminders, item => item.Id, reminderId);
        RequestFocus(DesktopFocusTarget.ReminderList);
    }

    [RelayCommand(CanExecute = nameof(CanCancelReminderEdit))]
    private void CancelReminderEdit()
    {
        CancelReminderEditCore(clearStatus: true);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedReminder))]
    private async Task DeleteSelectedReminderAsync()
    {
        if (SelectedVehicle is null || SelectedReminder is null)
        {
            return;
        }

        var rollbackDataSet = CloneDataSet(_dataSet);
        _dataSet.Reminders.RemoveAll(item => string.Equals(item.Id, SelectedReminder.Id, StringComparison.Ordinal));
        if (!await PersistDataAndRestoreSelectionAsync(
                SelectedVehicle.Id,
                ReminderTabIndex,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => ReminderEditorStatus = status,
                failureFocus: DesktopFocusTarget.ReminderList,
                failurePrefix: LO("ReminderEditor.Persistence.DeleteFailed")))
        {
            return;
        }
        ReminderEditorStatus = LO("ReminderEditor.Status.Deleted");
        RequestFocus(DesktopFocusTarget.ReminderList);
    }

    [RelayCommand(CanExecute = nameof(CanAdvanceSelectedReminder))]
    private async Task AdvanceSelectedReminderAsync()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        var reminder = GetSelectedReminderModel();
        if (!TryBuildNextReminderDueDate(reminder, out var nextDueDate))
        {
            ReminderEditorStatus = LO("ReminderEditor.Status.AdvanceUnavailable");
            RequestFocus(DesktopFocusTarget.ReminderList);
            return;
        }

        var nextDueDateText = FormatReminderDueDate(nextDueDate);
        var updatedReminder = reminder! with { DueDate = nextDueDateText };
        var rollbackDataSet = CloneDataSet(_dataSet);
        UpsertReminder(updatedReminder);

        if (!await PersistDataAndRestoreSelectionAsync(
                SelectedVehicle.Id,
                ReminderTabIndex,
                reminderId: updatedReminder.Id,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => ReminderEditorStatus = status,
                failureFocus: DesktopFocusTarget.ReminderList,
                failurePrefix: LO("ReminderEditor.Persistence.AdvanceFailed")))
        {
            return;
        }

        ReminderEditorStatus = LFO("ReminderEditor.Status.Advanced", nextDueDateText);
        SelectedReminder = FindById(SelectedVehicleReminders, item => item.Id, updatedReminder.Id);
        RequestFocus(DesktopFocusTarget.ReminderList);
    }

    [RelayCommand(CanExecute = nameof(CanCreateRecord))]
    private void CreateRecord()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        _editingRecordId = null;
        RecordEditorRecordType = "Doklad";
        RecordEditorTitle = string.Empty;
        RecordEditorProvider = string.Empty;
        RecordEditorValidFrom = string.Empty;
        RecordEditorValidTo = string.Empty;
        RecordEditorPrice = string.Empty;
        SelectedRecordEditorAttachmentMode = "Spravovaná kopie";
        RecordEditorPathInput = string.Empty;
        RecordEditorStoredPath = string.Empty;
        RecordEditorResolvedPath = string.Empty;
        RecordEditorAvailability = LO("RecordEditor.AttachmentAvailability.ManagedImportPrompt");
        RecordEditorNote = string.Empty;
        RecordEditorStatus = LO("RecordEditor.Status.CreatePrompt");
        IsEditingRecord = true;
        SelectedVehicleTabIndex = RecordTabIndex;
        RequestWorkspaceEditorDialog(WorkspaceEditorKind.Record, DesktopFocusTarget.RecordList);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelectedRecord))]
    private void EditSelectedRecord()
    {
        var record = GetSelectedRecordModel();
        if (record is null)
        {
            return;
        }

        BeginRecordEdit(record, preferManagedImport: false);
    }

    [RelayCommand(CanExecute = nameof(CanMoveSelectedRecordToManaged))]
    private void MoveSelectedRecordToManaged()
    {
        var record = GetSelectedRecordModel();
        if (record is null)
        {
            return;
        }

        BeginRecordEdit(record, preferManagedImport: true);
    }

    [RelayCommand(CanExecute = nameof(CanBrowseRecordAttachment))]
    private async Task BrowseRecordAttachmentAsync()
    {
        var filePath = await _filePickerService.PickFileAsync(
            IsRecordEditorManaged ? LO("RecordEditor.FileDialog.ManagedTitle") : LO("RecordEditor.FileDialog.ExternalTitle"));

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        RecordEditorPathInput = filePath;
        RefreshRecordEditorAttachmentPreview();
    }

    [RelayCommand(CanExecute = nameof(CanSaveRecord))]
    private async Task SaveRecordAsync()
    {
        if (SelectedVehicle is null || _dataRoot is null)
        {
            return;
        }

        var title = (RecordEditorTitle ?? string.Empty).Trim();
        var recordTypeText = (RecordEditorRecordType ?? string.Empty).Trim();
        var recordType = LegacyVehicleValueNormalization.NormalizeRecordType(recordTypeText);
        if (string.IsNullOrWhiteSpace(recordTypeText))
        {
            RecordEditorStatus = LO("RecordEditor.Validation.TypeRequired");
            RequestFocus(DesktopFocusTarget.RecordEditorType);
            return;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            RecordEditorStatus = LO("RecordEditor.Validation.TitleRequired");
            RequestFocus(DesktopFocusTarget.RecordEditorTitle);
            return;
        }

        var validFromText = (RecordEditorValidFrom ?? string.Empty).Trim();
        var validToText = (RecordEditorValidTo ?? string.Empty).Trim();
        var validFrom = LegacyVehicleValueNormalization.NormalizeMonthYear(validFromText);
        var validTo = LegacyVehicleValueNormalization.NormalizeMonthYear(validToText);
        var priceText = (RecordEditorPrice ?? string.Empty).Trim();
        var price = string.Empty;

        if (validFromText.Length > 0 && validFrom.Length == 0)
        {
            RecordEditorStatus = LO("RecordEditor.Validation.ValidFromInvalid");
            RequestFocus(DesktopFocusTarget.RecordEditorValidFrom);
            return;
        }

        if (validToText.Length > 0 && validTo.Length == 0)
        {
            RecordEditorStatus = LO("RecordEditor.Validation.ValidToInvalid");
            RequestFocus(DesktopFocusTarget.RecordEditorValidTo);
            return;
        }

        if (LegacyVehicleValueNormalization.TryGetMonthYearOrder(validFrom, out var validFromOrder)
            && LegacyVehicleValueNormalization.TryGetMonthYearOrder(validTo, out var validToOrder)
            && validFromOrder > validToOrder)
        {
            RecordEditorStatus = LO("RecordEditor.Validation.ValidRangeInvalid");
            RequestFocus(DesktopFocusTarget.RecordEditorValidFrom);
            return;
        }

        if (priceText.Length > 0)
        {
            if (!VehimapValueParser.TryParseMoney(priceText, out var parsedPrice) || parsedPrice < 0)
            {
                RecordEditorStatus = LO("RecordEditor.Validation.PriceInvalid");
                RequestFocus(DesktopFocusTarget.RecordEditorPrice);
                return;
            }

            price = parsedPrice.ToString("0.##", CultureInfo.InvariantCulture);
        }

        var recordId = _editingRecordId ?? GenerateLegacyId(_dataSet.Records.Select(item => item.Id));
        var existingRecord = _dataSet.Records.FirstOrDefault(item => string.Equals(item.Id, recordId, StringComparison.Ordinal));
        var attachmentMode = IsRecordEditorManaged ? VehicleRecordAttachmentMode.Managed : VehicleRecordAttachmentMode.External;
        var rollbackDataSet = CloneDataSet(_dataSet);

        string filePath;
        try
        {
            filePath = await BuildRecordFilePathAsync(existingRecord, attachmentMode);
        }
        catch (Exception ex)
        {
            RecordEditorStatus = ex.Message;
            RequestFocus(DesktopFocusTarget.RecordEditorPathInput);
            return;
        }

        var previousManagedPath = existingRecord?.AttachmentMode == VehicleRecordAttachmentMode.Managed ? existingRecord.FilePath : null;
        var updatedRecord = new VehicleRecord(
            recordId,
            SelectedVehicle.Id,
            recordType,
            title,
            (RecordEditorProvider ?? string.Empty).Trim(),
            validFrom,
            validTo,
            price,
            attachmentMode,
            filePath,
            (RecordEditorNote ?? string.Empty).Trim());

        UpsertRecord(updatedRecord);
        var createdManagedPath = attachmentMode == VehicleRecordAttachmentMode.Managed
            && !string.IsNullOrWhiteSpace(filePath)
            && !string.Equals(NormalizeManagedRelativePath(filePath), NormalizeManagedRelativePath(previousManagedPath), StringComparison.OrdinalIgnoreCase)
                ? filePath
                : null;

        if (!await PersistDataAndRestoreSelectionAsync(
                SelectedVehicle.Id,
                RecordTabIndex,
                recordId: recordId,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => RecordEditorStatus = status,
                failureFocus: DesktopFocusTarget.RecordEditorTitle,
                failurePrefix: LO("RecordEditor.Persistence.SaveFailed")))
        {
            DeleteManagedAttachmentIfUnused(createdManagedPath);
            return;
        }

        DeleteManagedAttachmentIfUnused(previousManagedPath);
        CancelRecordEditCore(clearStatus: false);
        RecordEditorStatus = existingRecord is null
            ? LO("RecordEditor.Status.Created")
            : LO("RecordEditor.Status.Updated");
        SelectedRecord = FindById(SelectedVehicleRecords, item => item.Id, recordId);
        RequestFocus(DesktopFocusTarget.RecordList);
    }

    [RelayCommand(CanExecute = nameof(CanCancelRecordEdit))]
    private void CancelRecordEdit()
    {
        CancelRecordEditCore(clearStatus: true);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedRecord))]
    private async Task DeleteSelectedRecordAsync()
    {
        if (SelectedVehicle is null || SelectedRecord is null)
        {
            return;
        }

        var existingRecord = GetSelectedRecordModel();
        var deletedManagedPath = existingRecord?.AttachmentMode == VehicleRecordAttachmentMode.Managed ? existingRecord.FilePath : null;
        var rollbackDataSet = CloneDataSet(_dataSet);
        _dataSet.Records.RemoveAll(item => string.Equals(item.Id, SelectedRecord.Id, StringComparison.Ordinal));

        if (!await PersistDataAndRestoreSelectionAsync(
                SelectedVehicle.Id,
                RecordTabIndex,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => RecordEditorStatus = status,
                failureFocus: DesktopFocusTarget.RecordList,
                failurePrefix: LO("RecordEditor.Persistence.DeleteFailed")))
        {
            return;
        }

        RecordEditorStatus = LO("RecordEditor.Status.Deleted");
        DeleteManagedAttachmentIfUnused(deletedManagedPath);
        RequestFocus(DesktopFocusTarget.RecordList);
    }

    private void BeginRecordEdit(VehicleRecord record, bool preferManagedImport)
    {
        _editingRecordId = record.Id;
        RecordEditorRecordType = LegacyVehicleValueNormalization.NormalizeRecordType(record.RecordType);
        RecordEditorTitle = record.Title;
        RecordEditorProvider = record.Provider;
        RecordEditorValidFrom = record.ValidFrom;
        RecordEditorValidTo = record.ValidTo;
        RecordEditorPrice = record.Price;
        SelectedRecordEditorAttachmentMode = preferManagedImport ? "Spravovaná kopie" : (record.AttachmentMode == VehicleRecordAttachmentMode.Managed ? "Spravovaná kopie" : "Externí cesta");
        RecordEditorPathInput = preferManagedImport
            ? ResolveExistingRecordPath(record)
            : (record.AttachmentMode == VehicleRecordAttachmentMode.External ? record.FilePath : string.Empty);
        RecordEditorNote = record.Note;
        RecordEditorStatus = preferManagedImport
            ? LO("RecordEditor.Status.MoveExternalToManagedPrompt")
            : LO("RecordEditor.Status.EditPrompt");
        IsEditingRecord = true;
        RefreshRecordEditorAttachmentPreview();
        SelectedVehicleTabIndex = RecordTabIndex;
        RequestWorkspaceEditorDialog(WorkspaceEditorKind.Record, DesktopFocusTarget.RecordList);
    }

    private VehicleReminder? GetSelectedReminderModel()
    {
        if (SelectedReminder is null)
        {
            return null;
        }

        return _dataSet.Reminders.FirstOrDefault(item => string.Equals(item.Id, SelectedReminder.Id, StringComparison.Ordinal));
    }

    private static bool TryBuildNextReminderDueDate(VehicleReminder? reminder, out DateOnly nextDueDate)
    {
        nextDueDate = default;
        if (reminder is null
            || !VehimapValueParser.TryParseEventDate(reminder.DueDate, out var currentDueDate)
            || !TryGetReminderRepeatIntervalMonths(reminder.RepeatMode, out var intervalMonths))
        {
            return false;
        }

        nextDueDate = currentDueDate.AddMonths(intervalMonths);
        return true;
    }

    private static bool TryGetReminderRepeatIntervalMonths(string? repeatMode, out int intervalMonths)
    {
        intervalMonths = 0;
        var normalized = LegacyVehicleValueNormalization.NormalizeReminderRepeatMode(repeatMode);
        if (string.Equals(normalized, "Každých 5 let", StringComparison.Ordinal))
        {
            intervalMonths = 60;
            return true;
        }

        if (string.Equals(normalized, "Každé 2 roky", StringComparison.Ordinal))
        {
            intervalMonths = 24;
            return true;
        }

        if (string.Equals(normalized, "Každý rok", StringComparison.Ordinal))
        {
            intervalMonths = 12;
            return true;
        }

        return false;
    }

    private static string FormatReminderDueDate(DateOnly date) =>
        date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    private VehicleRecord? GetSelectedRecordModel()
    {
        if (SelectedRecord is null)
        {
            return null;
        }

        return _dataSet.Records.FirstOrDefault(item => string.Equals(item.Id, SelectedRecord.Id, StringComparison.Ordinal));
    }

    private void UpsertReminder(VehicleReminder updatedReminder)
    {
        var index = _dataSet.Reminders.FindIndex(item => string.Equals(item.Id, updatedReminder.Id, StringComparison.Ordinal));
        if (index >= 0)
        {
            _dataSet.Reminders[index] = updatedReminder;
        }
        else
        {
            _dataSet.Reminders.Add(updatedReminder);
        }
    }

    private void UpsertRecord(VehicleRecord updatedRecord)
    {
        var index = _dataSet.Records.FindIndex(item => string.Equals(item.Id, updatedRecord.Id, StringComparison.Ordinal));
        if (index >= 0)
        {
            _dataSet.Records[index] = updatedRecord;
        }
        else
        {
            _dataSet.Records.Add(updatedRecord);
        }
    }

    private async Task<bool> PersistDataAndRestoreSelectionAsync(
        string vehicleId,
        int tabIndex,
        string? historyId = null,
        string? fuelId = null,
        string? reminderId = null,
        string? maintenanceId = null,
        string? recordId = null,
        VehimapDataSet? rollbackDataSet = null,
        Action<string>? setFailureStatus = null,
        DesktopFocusTarget? failureFocus = null,
        bool requireVehicleSelection = true,
        string failurePrefix = "Změny se nepodařilo uložit")
    {
        if (_dataRoot is null)
        {
            return false;
        }

        try
        {
            await _session.PersistAsync().ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (rollbackDataSet is not null)
            {
                _session.RestoreDataSet(rollbackDataSet);
            }

            var message = $"{failurePrefix}: {ex.Message}";
            setFailureStatus?.Invoke(message);
            ShellStatus = message;
            if (failureFocus is { } focusTarget)
            {
                RequestFocus(focusTarget);
            }

            return false;
        }

        Load();

        SelectedVehicle = FindById(Vehicles, item => item.Id, vehicleId);
        if (SelectedVehicle is null)
        {
            if (!requireVehicleSelection)
            {
                RequestBackgroundRefresh();
                return true;
            }

            return false;
        }

        SelectedVehicleTabIndex = tabIndex;

        if (tabIndex == ReminderTabIndex)
        {
            PopulateVehicleReminders(vehicleId);
            SelectedReminder = FindById(SelectedVehicleReminders, item => item.Id, reminderId ?? string.Empty);
        }
        else if (tabIndex == HistoryTabIndex)
        {
            PopulateVehicleHistory(vehicleId);
            SelectedHistory = FindById(SelectedVehicleHistory, item => item.Id, historyId ?? string.Empty);
        }
        else if (tabIndex == FuelTabIndex)
        {
            PopulateVehicleFuel(vehicleId);
            SelectedFuel = FindById(SelectedVehicleFuel, item => item.Id, fuelId ?? string.Empty);
        }
        else if (tabIndex == MaintenanceTabIndex)
        {
            PopulateVehicleMaintenance(vehicleId);
            SelectedMaintenance = FindById(SelectedVehicleMaintenance, item => item.Id, maintenanceId ?? string.Empty);
        }
        else if (tabIndex == RecordTabIndex)
        {
            PopulateVehicleRecords(vehicleId);
            SelectedRecord = FindById(SelectedVehicleRecords, item => item.Id, recordId ?? string.Empty);
        }

        RequestBackgroundRefresh();
        return true;
    }

    private static VehimapDataSet CloneDataSet(VehimapDataSet source)
    {
        return new VehimapDataSet
        {
            Settings = CloneSettings(source.Settings),
            Vehicles = [.. source.Vehicles],
            HistoryEntries = [.. source.HistoryEntries],
            FuelEntries = [.. source.FuelEntries],
            Records = [.. source.Records],
            VehicleMetaEntries = [.. source.VehicleMetaEntries],
            Reminders = [.. source.Reminders],
            MaintenancePlans = [.. source.MaintenancePlans]
        };
    }

    private static VehimapSettings CloneSettings(VehimapSettings source)
    {
        var clone = new VehimapSettings();
        foreach (var (section, values) in source.Sections)
        {
            foreach (var (key, value) in values)
            {
                clone.SetValue(section, key, value);
            }
        }

        return clone;
    }

    private async Task<string> BuildRecordFilePathAsync(VehicleRecord? existingRecord, VehicleRecordAttachmentMode attachmentMode)
    {
        if (_dataRoot is null)
        {
            return string.Empty;
        }

        var inputPath = (RecordEditorPathInput ?? string.Empty).Trim();
        if (attachmentMode == VehicleRecordAttachmentMode.External)
        {
            if (!string.IsNullOrWhiteSpace(inputPath))
            {
                return inputPath;
            }

            if (existingRecord is not null)
            {
                return existingRecord.AttachmentMode == VehicleRecordAttachmentMode.External
                    ? existingRecord.FilePath
                    : ResolveExistingRecordPath(existingRecord);
            }

            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return existingRecord?.AttachmentMode == VehicleRecordAttachmentMode.Managed
                ? NormalizeManagedRelativePath(existingRecord.FilePath)
                : string.Empty;
        }

        var sourcePath = ResolvePotentialPath(inputPath);
        if (!File.Exists(sourcePath))
        {
            throw new InvalidOperationException(LO("RecordEditor.Validation.ManagedSourceMissing"));
        }

        var existingManagedPath = existingRecord?.AttachmentMode == VehicleRecordAttachmentMode.Managed
            ? NormalizeManagedRelativePath(existingRecord.FilePath)
            : string.Empty;
        var existingManagedAbsolutePath = !string.IsNullOrWhiteSpace(existingManagedPath)
            ? ResolveManagedAttachmentAbsolutePath(existingManagedPath)
            : string.Empty;

        if (!string.IsNullOrWhiteSpace(existingManagedAbsolutePath)
            && PathsEqual(existingManagedAbsolutePath, sourcePath))
        {
            return existingManagedPath;
        }

        var relativePath = BuildManagedAttachmentRelativePath(SelectedVehicle!.Id, sourcePath, existingManagedPath);
        var targetPath = ResolveManagedAttachmentAbsolutePath(relativePath);
        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var sourceStream = File.OpenRead(sourcePath);
        await using var targetStream = File.Create(targetPath);
        await sourceStream.CopyToAsync(targetStream);
        return relativePath;
    }

    private void CancelReminderEditCore(bool clearStatus)
    {
        _editingReminderId = null;
        IsEditingReminder = false;
        ReminderEditorTitle = string.Empty;
        ReminderEditorDueDate = string.Empty;
        ReminderEditorDays = string.Empty;
        ReminderEditorRepeatMode = string.Empty;
        ReminderEditorNote = string.Empty;
        if (clearStatus)
        {
            ReminderEditorStatus = string.Empty;
        }
    }

    private void CancelRecordEditCore(bool clearStatus)
    {
        _editingRecordId = null;
        IsEditingRecord = false;
        RecordEditorRecordType = string.Empty;
        RecordEditorTitle = string.Empty;
        RecordEditorProvider = string.Empty;
        RecordEditorValidFrom = string.Empty;
        RecordEditorValidTo = string.Empty;
        RecordEditorPrice = string.Empty;
        SelectedRecordEditorAttachmentMode = "Spravovaná kopie";
        RecordEditorPathInput = string.Empty;
        RecordEditorStoredPath = string.Empty;
        RecordEditorResolvedPath = string.Empty;
        RecordEditorAvailability = LO("RecordEditor.AttachmentAvailability.SelectOrEnterPath");
        RecordEditorNote = string.Empty;
        if (clearStatus)
        {
            RecordEditorStatus = string.Empty;
        }
    }

    private void PrimeRecordEditorPathForMode()
    {
        var existingRecord = _editingRecordId is null
            ? null
            : _dataSet.Records.FirstOrDefault(item => string.Equals(item.Id, _editingRecordId, StringComparison.Ordinal));
        if (existingRecord is null)
        {
            return;
        }

        if (IsRecordEditorManaged)
        {
            if (existingRecord.AttachmentMode == VehicleRecordAttachmentMode.External && string.IsNullOrWhiteSpace(RecordEditorPathInput))
            {
                RecordEditorPathInput = ResolveExistingRecordPath(existingRecord);
            }

            return;
        }

        if (existingRecord.AttachmentMode == VehicleRecordAttachmentMode.Managed && string.IsNullOrWhiteSpace(RecordEditorPathInput))
        {
            RecordEditorPathInput = ResolveExistingRecordPath(existingRecord);
        }
    }

    private void RefreshRecordEditorAttachmentPreview()
    {
        if (!IsEditingRecord)
        {
            return;
        }

        var existingRecord = _editingRecordId is null
            ? null
            : _dataSet.Records.FirstOrDefault(item => string.Equals(item.Id, _editingRecordId, StringComparison.Ordinal));

        if (IsRecordEditorManaged)
        {
            RecordEditorStoredPath = existingRecord?.AttachmentMode == VehicleRecordAttachmentMode.Managed
                ? NormalizeManagedRelativePath(existingRecord.FilePath)
                : string.Empty;
            RecordEditorResolvedPath = string.IsNullOrWhiteSpace(RecordEditorStoredPath)
                ? string.Empty
                : ResolveManagedAttachmentAbsolutePath(RecordEditorStoredPath);

            if (!string.IsNullOrWhiteSpace(RecordEditorPathInput))
            {
                var sourcePath = ResolvePotentialPath(RecordEditorPathInput);
                RecordEditorAvailability = File.Exists(sourcePath)
                    ? LO("RecordEditor.AttachmentAvailability.SourceReady")
                    : LO("RecordEditor.AttachmentAvailability.SourceMissing");
            }
            else if (!string.IsNullOrWhiteSpace(RecordEditorStoredPath))
            {
                RecordEditorAvailability = File.Exists(RecordEditorResolvedPath)
                    ? LO("RecordEditor.AttachmentAvailability.ExistingManagedReady")
                    : LO("RecordEditor.AttachmentAvailability.ExistingManagedMissing");
            }
            else
            {
                RecordEditorAvailability = LO("RecordEditor.AttachmentAvailability.ManagedImportPrompt");
            }

            return;
        }

        RecordEditorStoredPath = (RecordEditorPathInput ?? string.Empty).Trim();
        RecordEditorResolvedPath = string.IsNullOrWhiteSpace(RecordEditorStoredPath) ? string.Empty : ResolvePotentialPath(RecordEditorStoredPath);
        if (string.IsNullOrWhiteSpace(RecordEditorStoredPath))
        {
            RecordEditorAvailability = LO("RecordEditor.AttachmentAvailability.NoAttachment");
        }
        else
        {
            RecordEditorAvailability = File.Exists(RecordEditorResolvedPath)
                ? LO("RecordEditor.AttachmentAvailability.ExternalReady")
                : LO("RecordEditor.AttachmentAvailability.ExternalMissing");
        }
    }

    private string ResolveExistingRecordPath(VehicleRecord record)
    {
        if (_dataRoot is null || string.IsNullOrWhiteSpace(record.FilePath))
        {
            return string.Empty;
        }

        return record.AttachmentMode == VehicleRecordAttachmentMode.Managed
            ? ResolveManagedAttachmentAbsolutePath(record.FilePath)
            : ResolvePotentialPath(record.FilePath);
    }

    private string BuildManagedAttachmentRelativePath(string vehicleId, string sourcePath, string? currentRelativePath)
    {
        var normalizedCurrent = NormalizeManagedRelativePath(currentRelativePath);
        var fileName = SanitizeFileName(Path.GetFileNameWithoutExtension(sourcePath));
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "priloha";
        }

        var extension = Path.GetExtension(sourcePath);
        var relativeDirectory = $"attachments/{vehicleId}";
        var relativePath = $"{relativeDirectory}/{fileName}{extension}";
        var suffix = 2;

        while (IsManagedAttachmentPathUsed(relativePath, normalizedCurrent))
        {
            relativePath = $"{relativeDirectory}/{fileName}-{suffix}{extension}";
            suffix += 1;
        }

        return relativePath;
    }

    private bool IsManagedAttachmentPathUsed(string relativePath, string? ignoreRelativePath)
    {
        var normalizedCandidate = NormalizeManagedRelativePath(relativePath);
        if (string.IsNullOrWhiteSpace(normalizedCandidate))
        {
            return false;
        }

        foreach (var record in _dataSet.Records.Where(item => item.AttachmentMode == VehicleRecordAttachmentMode.Managed))
        {
            var normalized = NormalizeManagedRelativePath(record.FilePath);
            if (!string.IsNullOrWhiteSpace(ignoreRelativePath)
                && string.Equals(normalized, ignoreRelativePath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.Equals(normalized, normalizedCandidate, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(ignoreRelativePath)
            && string.Equals(normalizedCandidate, ignoreRelativePath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return File.Exists(ResolveManagedAttachmentAbsolutePath(normalizedCandidate));
    }

    private void DeleteManagedAttachmentIfUnused(string? relativePath)
    {
        if (_dataRoot is null)
        {
            return;
        }

        var normalized = NormalizeManagedRelativePath(relativePath);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        if (_dataSet.Records.Any(item =>
                item.AttachmentMode == VehicleRecordAttachmentMode.Managed
                && string.Equals(NormalizeManagedRelativePath(item.FilePath), normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var absolutePath = ResolveManagedAttachmentAbsolutePath(normalized);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }

    private string ResolveManagedAttachmentAbsolutePath(string relativePath)
    {
        return _session.ResolveManagedAttachmentPath(relativePath);
    }

    private string ResolvePotentialPath(string path)
    {
        var trimmedPath = (path ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmedPath))
        {
            return string.Empty;
        }

        if (Path.IsPathRooted(trimmedPath))
        {
            return trimmedPath;
        }

        if (_dataRoot is null)
        {
            return Path.GetFullPath(trimmedPath);
        }

        return Path.GetFullPath(Path.Combine(_dataRoot.AppBasePath, trimmedPath));
    }

    private static string NormalizeManagedRelativePath(string? path)
    {
        var normalized = (path ?? string.Empty).Trim().Replace('\\', '/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        if (normalized.StartsWith("data/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[5..];
        }

        while (normalized.StartsWith("/", StringComparison.Ordinal))
        {
            normalized = normalized[1..];
        }

        return normalized;
    }

    private static string SanitizeFileName(string? fileName)
    {
        var value = (fileName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return "priloha";
        }

        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidChar, '_');
        }

        return value.Trim(' ', '.');
    }

    private static string GenerateLegacyId(IEnumerable<string> existingIds)
    {
        var ids = new HashSet<string>(existingIds, StringComparer.Ordinal);
        string candidate;
        do
        {
            candidate = $"{DateTime.Now:yyyyMMddHHmmss}_{Random.Shared.Next(1000, 9999)}";
        } while (!ids.Add(candidate));

        return candidate;
    }

    private static bool PathsEqual(string left, string right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        return string.Equals(
            Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }
}
