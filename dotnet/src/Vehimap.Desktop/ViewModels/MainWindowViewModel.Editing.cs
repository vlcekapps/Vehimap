using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private string? _editingReminderId;
    private string? _editingRecordId;

    [ObservableProperty]
    private bool isEditingReminder;

    [ObservableProperty]
    private string reminderPanelHeading = "Detail připomínky";

    [ObservableProperty]
    private string reminderEditorStatus = string.Empty;

    [ObservableProperty]
    private string reminderEditorTitle = string.Empty;

    [ObservableProperty]
    private string reminderEditorDueDate = string.Empty;

    [ObservableProperty]
    private string reminderEditorDays = string.Empty;

    [ObservableProperty]
    private string reminderEditorRepeatMode = string.Empty;

    [ObservableProperty]
    private string reminderEditorNote = string.Empty;

    [ObservableProperty]
    private bool isEditingRecord;

    [ObservableProperty]
    private string recordPanelHeading = "Detail dokladu";

    [ObservableProperty]
    private string recordEditorStatus = string.Empty;

    [ObservableProperty]
    private string recordEditorRecordType = string.Empty;

    [ObservableProperty]
    private string recordEditorTitle = string.Empty;

    [ObservableProperty]
    private string recordEditorProvider = string.Empty;

    [ObservableProperty]
    private string recordEditorValidFrom = string.Empty;

    [ObservableProperty]
    private string recordEditorValidTo = string.Empty;

    [ObservableProperty]
    private string recordEditorPrice = string.Empty;

    [ObservableProperty]
    private string selectedRecordEditorAttachmentMode = "Spravovaná kopie";

    [ObservableProperty]
    private string recordEditorPathInput = string.Empty;

    [ObservableProperty]
    private string recordEditorStoredPath = string.Empty;

    [ObservableProperty]
    private string recordEditorResolvedPath = string.Empty;

    [ObservableProperty]
    private string recordEditorAvailability = "Vyberte soubor nebo zadejte cestu přílohy.";

    [ObservableProperty]
    private string recordEditorNote = string.Empty;

    public IReadOnlyList<string> RecordAttachmentModes { get; } = ["Spravovaná kopie", "Externí cesta"];

    public bool IsReminderDetailVisible => !IsEditingReminder;

    public bool IsRecordDetailVisible => !IsEditingRecord;

    public bool CanCreateReminder => SelectedVehicle is not null && !IsEditingReminder;

    public bool CanEditSelectedReminder => SelectedReminder is not null && !IsEditingReminder;

    public bool CanDeleteSelectedReminder => SelectedReminder is not null && !IsEditingReminder;

    public bool CanSaveReminder => SelectedVehicle is not null && IsEditingReminder;

    public bool CanCancelReminderEdit => IsEditingReminder;

    public bool CanCreateRecord => SelectedVehicle is not null && !IsEditingRecord;

    public bool CanEditSelectedRecord => SelectedRecord is not null && !IsEditingRecord;

    public bool CanDeleteSelectedRecord => SelectedRecord is not null && !IsEditingRecord;

    public bool CanSaveRecord => SelectedVehicle is not null && IsEditingRecord;

    public bool CanCancelRecordEdit => IsEditingRecord;

    public bool CanBrowseRecordAttachment => IsEditingRecord;

    public bool CanMoveSelectedRecordToManaged =>
        SelectedRecord is not null
        && !IsEditingRecord
        && !string.Equals(SelectedRecord.AttachmentMode, "Spravovaná kopie", StringComparison.CurrentCulture)
        && !string.IsNullOrWhiteSpace(SelectedRecord.ResolvedPath);

    public bool IsRecordEditorManaged =>
        string.Equals(SelectedRecordEditorAttachmentMode, "Spravovaná kopie", StringComparison.CurrentCulture);

    public string RecordEditorPathInputLabel =>
        IsRecordEditorManaged ? "Zdroj souboru pro import" : "Externí cesta k souboru";

    public string RecordEditorPathInputHelp =>
        IsRecordEditorManaged
            ? "Vybraný soubor se po uložení zkopíruje do spravovaných příloh."
            : "Zadejte nebo vyberte externí cestu, která se nebude kopírovat.";

    partial void OnIsEditingReminderChanged(bool value)
    {
        ReminderPanelHeading = value
            ? (_editingReminderId is null ? "Nová připomínka" : "Upravit připomínku")
            : "Detail připomínky";

        OnPropertyChanged(nameof(IsReminderDetailVisible));
        CreateReminderCommand.NotifyCanExecuteChanged();
        EditSelectedReminderCommand.NotifyCanExecuteChanged();
        DeleteSelectedReminderCommand.NotifyCanExecuteChanged();
        SaveReminderCommand.NotifyCanExecuteChanged();
        CancelReminderEditCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsEditingRecordChanged(bool value)
    {
        RecordPanelHeading = value
            ? (_editingRecordId is null ? "Nový doklad" : "Upravit doklad")
            : "Detail dokladu";

        OnPropertyChanged(nameof(IsRecordDetailVisible));
        OnPropertyChanged(nameof(CanMoveSelectedRecordToManaged));
        CreateRecordCommand.NotifyCanExecuteChanged();
        EditSelectedRecordCommand.NotifyCanExecuteChanged();
        DeleteSelectedRecordCommand.NotifyCanExecuteChanged();
        SaveRecordCommand.NotifyCanExecuteChanged();
        CancelRecordEditCommand.NotifyCanExecuteChanged();
        BrowseRecordAttachmentCommand.NotifyCanExecuteChanged();
        MoveSelectedRecordToManagedCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedRecordEditorAttachmentModeChanged(string value)
    {
        PrimeRecordEditorPathForMode();
        RefreshRecordEditorAttachmentPreview();
        OnPropertyChanged(nameof(IsRecordEditorManaged));
        OnPropertyChanged(nameof(RecordEditorPathInputLabel));
        OnPropertyChanged(nameof(RecordEditorPathInputHelp));
    }

    partial void OnRecordEditorPathInputChanged(string value)
    {
        RefreshRecordEditorAttachmentPreview();
    }

    internal void HandleVehicleSelectionChanged()
    {
        CancelHistoryEditCore(clearStatus: true);
        CancelFuelEditCore(clearStatus: true);
        CancelReminderEditCore(clearStatus: true);
        CancelMaintenanceEditCore(clearStatus: true);
        CancelRecordEditCore(clearStatus: true);
        CreateHistoryCommand.NotifyCanExecuteChanged();
        CreateFuelCommand.NotifyCanExecuteChanged();
        CreateReminderCommand.NotifyCanExecuteChanged();
        CreateMaintenanceCommand.NotifyCanExecuteChanged();
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
        ReminderEditorDays = string.Empty;
        ReminderEditorRepeatMode = string.Empty;
        ReminderEditorNote = string.Empty;
        ReminderEditorStatus = "Vyplňte připomínku a uložte ji.";
        IsEditingReminder = true;
        SelectedVehicleTabIndex = ReminderTabIndex;
        RequestFocus(DesktopFocusTarget.ReminderEditorTitle);
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
        ReminderEditorRepeatMode = reminder.RepeatMode;
        ReminderEditorNote = reminder.Note;
        ReminderEditorStatus = "Upravte připomínku a uložte změny.";
        IsEditingReminder = true;
        SelectedVehicleTabIndex = ReminderTabIndex;
        RequestFocus(DesktopFocusTarget.ReminderEditorTitle);
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
            ReminderEditorStatus = "Připomínka musí mít název.";
            RequestFocus(DesktopFocusTarget.ReminderEditorTitle);
            return;
        }

        var reminderId = _editingReminderId ?? GenerateLegacyId(_dataSet.Reminders.Select(item => item.Id));
        var updatedReminder = new VehicleReminder(
            reminderId,
            SelectedVehicle.Id,
            title,
            (ReminderEditorDueDate ?? string.Empty).Trim(),
            (ReminderEditorDays ?? string.Empty).Trim(),
            (ReminderEditorRepeatMode ?? string.Empty).Trim(),
            (ReminderEditorNote ?? string.Empty).Trim());

        UpsertReminder(updatedReminder);
        var wasNew = _editingReminderId is null;

        await PersistDataAndRestoreSelectionAsync(SelectedVehicle.Id, ReminderTabIndex, reminderId: reminderId);

        CancelReminderEditCore(clearStatus: false);
        ReminderEditorStatus = wasNew
            ? "Nová připomínka byla uložena."
            : "Připomínka byla upravena.";
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

        _dataSet.Reminders.RemoveAll(item => string.Equals(item.Id, SelectedReminder.Id, StringComparison.Ordinal));
        await PersistDataAndRestoreSelectionAsync(SelectedVehicle.Id, ReminderTabIndex);
        ReminderEditorStatus = "Připomínka byla odstraněna.";
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
        RecordEditorAvailability = "Vyberte soubor, který se po uložení zkopíruje do spravovaných příloh.";
        RecordEditorNote = string.Empty;
        RecordEditorStatus = "Vyplňte doklad a podle potřeby vyberte přílohu.";
        IsEditingRecord = true;
        SelectedVehicleTabIndex = RecordTabIndex;
        RequestFocus(DesktopFocusTarget.RecordEditorTitle);
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
            IsRecordEditorManaged ? "Vyberte soubor pro spravovanou kopii" : "Vyberte externí soubor");

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
        if (string.IsNullOrWhiteSpace(title))
        {
            RecordEditorStatus = "Doklad musí mít název.";
            RequestFocus(DesktopFocusTarget.RecordEditorTitle);
            return;
        }

        var recordId = _editingRecordId ?? GenerateLegacyId(_dataSet.Records.Select(item => item.Id));
        var existingRecord = _dataSet.Records.FirstOrDefault(item => string.Equals(item.Id, recordId, StringComparison.Ordinal));
        var attachmentMode = IsRecordEditorManaged ? VehicleRecordAttachmentMode.Managed : VehicleRecordAttachmentMode.External;

        string filePath;
        try
        {
            filePath = await BuildRecordFilePathAsync(existingRecord, attachmentMode);
        }
        catch (Exception ex)
        {
            RecordEditorStatus = ex.Message;
            return;
        }

        var previousManagedPath = existingRecord?.AttachmentMode == VehicleRecordAttachmentMode.Managed ? existingRecord.FilePath : null;
        var updatedRecord = new VehicleRecord(
            recordId,
            SelectedVehicle.Id,
            string.IsNullOrWhiteSpace(RecordEditorRecordType) ? "Doklad" : RecordEditorRecordType.Trim(),
            title,
            (RecordEditorProvider ?? string.Empty).Trim(),
            (RecordEditorValidFrom ?? string.Empty).Trim(),
            (RecordEditorValidTo ?? string.Empty).Trim(),
            (RecordEditorPrice ?? string.Empty).Trim(),
            attachmentMode,
            filePath,
            (RecordEditorNote ?? string.Empty).Trim());

        UpsertRecord(updatedRecord);
        DeleteManagedAttachmentIfUnused(previousManagedPath);

        await PersistDataAndRestoreSelectionAsync(SelectedVehicle.Id, RecordTabIndex, recordId: recordId);

        CancelRecordEditCore(clearStatus: false);
        RecordEditorStatus = existingRecord is null
            ? "Nový doklad byl uložen."
            : "Doklad byl upraven.";
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
        _dataSet.Records.RemoveAll(item => string.Equals(item.Id, SelectedRecord.Id, StringComparison.Ordinal));
        DeleteManagedAttachmentIfUnused(existingRecord?.AttachmentMode == VehicleRecordAttachmentMode.Managed ? existingRecord.FilePath : null);

        await PersistDataAndRestoreSelectionAsync(SelectedVehicle.Id, RecordTabIndex);

        RecordEditorStatus = "Doklad byl odstraněn.";
        RequestFocus(DesktopFocusTarget.RecordList);
    }

    private void BeginRecordEdit(VehicleRecord record, bool preferManagedImport)
    {
        _editingRecordId = record.Id;
        RecordEditorRecordType = record.RecordType;
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
            ? "Po uložení se současná externí příloha zkopíruje do spravovaných příloh."
            : "Upravte doklad a uložte změny.";
        IsEditingRecord = true;
        RefreshRecordEditorAttachmentPreview();
        SelectedVehicleTabIndex = RecordTabIndex;
        RequestFocus(DesktopFocusTarget.RecordEditorTitle);
    }

    private VehicleReminder? GetSelectedReminderModel()
    {
        if (SelectedReminder is null)
        {
            return null;
        }

        return _dataSet.Reminders.FirstOrDefault(item => string.Equals(item.Id, SelectedReminder.Id, StringComparison.Ordinal));
    }

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

    private async Task PersistDataAndRestoreSelectionAsync(
        string vehicleId,
        int tabIndex,
        string? historyId = null,
        string? fuelId = null,
        string? reminderId = null,
        string? maintenanceId = null,
        string? recordId = null)
    {
        if (_dataRoot is null)
        {
            return;
        }

        await _legacyDataStore.SaveAsync(_dataRoot, _dataSet);
        Load();

        SelectedVehicle = FindById(Vehicles, item => item.Id, vehicleId);
        if (SelectedVehicle is null)
        {
            return;
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
            throw new InvalidOperationException("Vybraný soubor pro spravovanou kopii se nepodařilo najít.");
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
        RecordEditorAvailability = "Vyberte soubor nebo zadejte cestu přílohy.";
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
                    ? "Po uložení se vybraný soubor zkopíruje do spravovaných příloh."
                    : "Vybraný zdrojový soubor se zatím nepodařilo najít.";
            }
            else if (!string.IsNullOrWhiteSpace(RecordEditorStoredPath))
            {
                RecordEditorAvailability = File.Exists(RecordEditorResolvedPath)
                    ? "Použije se stávající spravovaná příloha."
                    : "Stávající spravovaná příloha chybí. Vyberte náhradní soubor.";
            }
            else
            {
                RecordEditorAvailability = "Vyberte soubor, který se po uložení zkopíruje do spravovaných příloh.";
            }

            return;
        }

        RecordEditorStoredPath = (RecordEditorPathInput ?? string.Empty).Trim();
        RecordEditorResolvedPath = string.IsNullOrWhiteSpace(RecordEditorStoredPath) ? string.Empty : ResolvePotentialPath(RecordEditorStoredPath);
        if (string.IsNullOrWhiteSpace(RecordEditorStoredPath))
        {
            RecordEditorAvailability = "Doklad bude uložen bez připojeného souboru.";
        }
        else
        {
            RecordEditorAvailability = File.Exists(RecordEditorResolvedPath)
                ? "Externí soubor je dostupný."
                : "Externí soubor se zatím nepodařilo najít.";
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
        if (_dataRoot is null)
        {
            return string.Empty;
        }

        return _attachmentService.ResolveManagedAttachmentPath(_dataRoot, relativePath);
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
