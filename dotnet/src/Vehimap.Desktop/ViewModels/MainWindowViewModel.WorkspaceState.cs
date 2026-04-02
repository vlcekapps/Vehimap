namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    public VehicleHistoryItemViewModel? SelectedHistory
    {
        get => HistoryWorkspace.SelectedHistory;
        set => HistoryWorkspace.SelectedHistory = value;
    }

    public string SelectedHistoryDetail
    {
        get => HistoryWorkspace.SelectedHistoryDetail;
        set => HistoryWorkspace.SelectedHistoryDetail = value;
    }

    public bool IsEditingHistory
    {
        get => HistoryWorkspace.IsEditingHistory;
        set => HistoryWorkspace.IsEditingHistory = value;
    }

    public string HistoryPanelHeading
    {
        get => HistoryWorkspace.HistoryPanelHeading;
        set => HistoryWorkspace.HistoryPanelHeading = value;
    }

    public string HistoryEditorStatus
    {
        get => HistoryWorkspace.HistoryEditorStatus;
        set => HistoryWorkspace.HistoryEditorStatus = value;
    }

    public string HistoryEditorDate
    {
        get => HistoryWorkspace.HistoryEditorDate;
        set => HistoryWorkspace.HistoryEditorDate = value;
    }

    public string HistoryEditorType
    {
        get => HistoryWorkspace.HistoryEditorType;
        set => HistoryWorkspace.HistoryEditorType = value;
    }

    public string HistoryEditorOdometer
    {
        get => HistoryWorkspace.HistoryEditorOdometer;
        set => HistoryWorkspace.HistoryEditorOdometer = value;
    }

    public string HistoryEditorCost
    {
        get => HistoryWorkspace.HistoryEditorCost;
        set => HistoryWorkspace.HistoryEditorCost = value;
    }

    public string HistoryEditorNote
    {
        get => HistoryWorkspace.HistoryEditorNote;
        set => HistoryWorkspace.HistoryEditorNote = value;
    }

    public bool IsHistoryDetailVisible => HistoryWorkspace.IsHistoryDetailVisible;
    public bool CanCreateHistory => SelectedVehicle is not null && !IsEditingHistory;
    public bool CanEditSelectedHistory => SelectedHistory is not null && !IsEditingHistory;
    public bool CanDeleteSelectedHistory => SelectedHistory is not null && !IsEditingHistory;
    public bool CanSaveHistory => SelectedVehicle is not null && IsEditingHistory;
    public bool CanCancelHistoryEdit => IsEditingHistory;

    public VehicleFuelItemViewModel? SelectedFuel
    {
        get => FuelWorkspace.SelectedFuel;
        set => FuelWorkspace.SelectedFuel = value;
    }

    public string SelectedFuelDetail
    {
        get => FuelWorkspace.SelectedFuelDetail;
        set => FuelWorkspace.SelectedFuelDetail = value;
    }

    public bool IsEditingFuel
    {
        get => FuelWorkspace.IsEditingFuel;
        set => FuelWorkspace.IsEditingFuel = value;
    }

    public string FuelPanelHeading
    {
        get => FuelWorkspace.FuelPanelHeading;
        set => FuelWorkspace.FuelPanelHeading = value;
    }

    public string FuelEditorStatus
    {
        get => FuelWorkspace.FuelEditorStatus;
        set => FuelWorkspace.FuelEditorStatus = value;
    }

    public string FuelEditorDate
    {
        get => FuelWorkspace.FuelEditorDate;
        set => FuelWorkspace.FuelEditorDate = value;
    }

    public string FuelEditorFuelType
    {
        get => FuelWorkspace.FuelEditorFuelType;
        set => FuelWorkspace.FuelEditorFuelType = value;
    }

    public string FuelEditorLiters
    {
        get => FuelWorkspace.FuelEditorLiters;
        set => FuelWorkspace.FuelEditorLiters = value;
    }

    public string FuelEditorTotalCost
    {
        get => FuelWorkspace.FuelEditorTotalCost;
        set => FuelWorkspace.FuelEditorTotalCost = value;
    }

    public string FuelEditorOdometer
    {
        get => FuelWorkspace.FuelEditorOdometer;
        set => FuelWorkspace.FuelEditorOdometer = value;
    }

    public bool FuelEditorFullTank
    {
        get => FuelWorkspace.FuelEditorFullTank;
        set => FuelWorkspace.FuelEditorFullTank = value;
    }

    public string FuelEditorNote
    {
        get => FuelWorkspace.FuelEditorNote;
        set => FuelWorkspace.FuelEditorNote = value;
    }

    public bool IsFuelDetailVisible => FuelWorkspace.IsFuelDetailVisible;
    public bool CanCreateFuel => SelectedVehicle is not null && !IsEditingFuel;
    public bool CanEditSelectedFuel => SelectedFuel is not null && !IsEditingFuel;
    public bool CanDeleteSelectedFuel => SelectedFuel is not null && !IsEditingFuel;
    public bool CanSaveFuel => SelectedVehicle is not null && IsEditingFuel;
    public bool CanCancelFuelEdit => IsEditingFuel;

    public VehicleReminderItemViewModel? SelectedReminder
    {
        get => ReminderWorkspace.SelectedReminder;
        set => ReminderWorkspace.SelectedReminder = value;
    }

    public string SelectedReminderDetail
    {
        get => ReminderWorkspace.SelectedReminderDetail;
        set => ReminderWorkspace.SelectedReminderDetail = value;
    }

    public bool IsEditingReminder
    {
        get => ReminderWorkspace.IsEditingReminder;
        set => ReminderWorkspace.IsEditingReminder = value;
    }

    public string ReminderPanelHeading
    {
        get => ReminderWorkspace.ReminderPanelHeading;
        set => ReminderWorkspace.ReminderPanelHeading = value;
    }

    public string ReminderEditorStatus
    {
        get => ReminderWorkspace.ReminderEditorStatus;
        set => ReminderWorkspace.ReminderEditorStatus = value;
    }

    public string ReminderEditorTitle
    {
        get => ReminderWorkspace.ReminderEditorTitle;
        set => ReminderWorkspace.ReminderEditorTitle = value;
    }

    public string ReminderEditorDueDate
    {
        get => ReminderWorkspace.ReminderEditorDueDate;
        set => ReminderWorkspace.ReminderEditorDueDate = value;
    }

    public string ReminderEditorDays
    {
        get => ReminderWorkspace.ReminderEditorDays;
        set => ReminderWorkspace.ReminderEditorDays = value;
    }

    public string ReminderEditorRepeatMode
    {
        get => ReminderWorkspace.ReminderEditorRepeatMode;
        set => ReminderWorkspace.ReminderEditorRepeatMode = value;
    }

    public string ReminderEditorNote
    {
        get => ReminderWorkspace.ReminderEditorNote;
        set => ReminderWorkspace.ReminderEditorNote = value;
    }

    public bool IsReminderDetailVisible => ReminderWorkspace.IsReminderDetailVisible;
    public bool CanCreateReminder => SelectedVehicle is not null && !IsEditingReminder;
    public bool CanEditSelectedReminder => SelectedReminder is not null && !IsEditingReminder;
    public bool CanDeleteSelectedReminder => SelectedReminder is not null && !IsEditingReminder;
    public bool CanSaveReminder => SelectedVehicle is not null && IsEditingReminder;
    public bool CanCancelReminderEdit => IsEditingReminder;

    public VehicleMaintenanceItemViewModel? SelectedMaintenance
    {
        get => MaintenanceWorkspace.SelectedMaintenance;
        set => MaintenanceWorkspace.SelectedMaintenance = value;
    }

    public string SelectedMaintenanceDetail
    {
        get => MaintenanceWorkspace.SelectedMaintenanceDetail;
        set => MaintenanceWorkspace.SelectedMaintenanceDetail = value;
    }

    public bool IsEditingMaintenance
    {
        get => MaintenanceWorkspace.IsEditingMaintenance;
        set => MaintenanceWorkspace.IsEditingMaintenance = value;
    }

    public string MaintenancePanelHeading
    {
        get => MaintenanceWorkspace.MaintenancePanelHeading;
        set => MaintenanceWorkspace.MaintenancePanelHeading = value;
    }

    public string MaintenanceEditorStatus
    {
        get => MaintenanceWorkspace.MaintenanceEditorStatus;
        set => MaintenanceWorkspace.MaintenanceEditorStatus = value;
    }

    public string MaintenanceEditorTitle
    {
        get => MaintenanceWorkspace.MaintenanceEditorTitle;
        set => MaintenanceWorkspace.MaintenanceEditorTitle = value;
    }

    public string MaintenanceEditorIntervalKm
    {
        get => MaintenanceWorkspace.MaintenanceEditorIntervalKm;
        set => MaintenanceWorkspace.MaintenanceEditorIntervalKm = value;
    }

    public string MaintenanceEditorIntervalMonths
    {
        get => MaintenanceWorkspace.MaintenanceEditorIntervalMonths;
        set => MaintenanceWorkspace.MaintenanceEditorIntervalMonths = value;
    }

    public string MaintenanceEditorLastServiceDate
    {
        get => MaintenanceWorkspace.MaintenanceEditorLastServiceDate;
        set => MaintenanceWorkspace.MaintenanceEditorLastServiceDate = value;
    }

    public string MaintenanceEditorLastServiceOdometer
    {
        get => MaintenanceWorkspace.MaintenanceEditorLastServiceOdometer;
        set => MaintenanceWorkspace.MaintenanceEditorLastServiceOdometer = value;
    }

    public bool MaintenanceEditorIsActive
    {
        get => MaintenanceWorkspace.MaintenanceEditorIsActive;
        set => MaintenanceWorkspace.MaintenanceEditorIsActive = value;
    }

    public string MaintenanceEditorNote
    {
        get => MaintenanceWorkspace.MaintenanceEditorNote;
        set => MaintenanceWorkspace.MaintenanceEditorNote = value;
    }

    public bool IsMaintenanceDetailVisible => MaintenanceWorkspace.IsMaintenanceDetailVisible;
    public bool CanCreateMaintenance => SelectedVehicle is not null && !IsEditingMaintenance;
    public bool CanEditSelectedMaintenance => SelectedMaintenance is not null && !IsEditingMaintenance;
    public bool CanDeleteSelectedMaintenance => SelectedMaintenance is not null && !IsEditingMaintenance;
    public bool CanSaveMaintenance => SelectedVehicle is not null && IsEditingMaintenance;
    public bool CanCancelMaintenanceEdit => IsEditingMaintenance;

    public VehicleRecordItemViewModel? SelectedRecord
    {
        get => RecordWorkspace.SelectedRecord;
        set => RecordWorkspace.SelectedRecord = value;
    }

    public string SelectedRecordDetail
    {
        get => RecordWorkspace.SelectedRecordDetail;
        set => RecordWorkspace.SelectedRecordDetail = value;
    }

    public bool IsEditingRecord
    {
        get => RecordWorkspace.IsEditingRecord;
        set => RecordWorkspace.IsEditingRecord = value;
    }

    public string RecordPanelHeading
    {
        get => RecordWorkspace.RecordPanelHeading;
        set => RecordWorkspace.RecordPanelHeading = value;
    }

    public string RecordEditorStatus
    {
        get => RecordWorkspace.RecordEditorStatus;
        set => RecordWorkspace.RecordEditorStatus = value;
    }

    public string RecordEditorRecordType
    {
        get => RecordWorkspace.RecordEditorRecordType;
        set => RecordWorkspace.RecordEditorRecordType = value;
    }

    public string RecordEditorTitle
    {
        get => RecordWorkspace.RecordEditorTitle;
        set => RecordWorkspace.RecordEditorTitle = value;
    }

    public string RecordEditorProvider
    {
        get => RecordWorkspace.RecordEditorProvider;
        set => RecordWorkspace.RecordEditorProvider = value;
    }

    public string RecordEditorValidFrom
    {
        get => RecordWorkspace.RecordEditorValidFrom;
        set => RecordWorkspace.RecordEditorValidFrom = value;
    }

    public string RecordEditorValidTo
    {
        get => RecordWorkspace.RecordEditorValidTo;
        set => RecordWorkspace.RecordEditorValidTo = value;
    }

    public string RecordEditorPrice
    {
        get => RecordWorkspace.RecordEditorPrice;
        set => RecordWorkspace.RecordEditorPrice = value;
    }

    public IReadOnlyList<string> RecordAttachmentModes => RecordWorkspace.RecordAttachmentModes;

    public string SelectedRecordEditorAttachmentMode
    {
        get => RecordWorkspace.SelectedRecordEditorAttachmentMode;
        set => RecordWorkspace.SelectedRecordEditorAttachmentMode = value;
    }

    public string RecordEditorPathInput
    {
        get => RecordWorkspace.RecordEditorPathInput;
        set => RecordWorkspace.RecordEditorPathInput = value;
    }

    public string RecordEditorStoredPath
    {
        get => RecordWorkspace.RecordEditorStoredPath;
        set => RecordWorkspace.RecordEditorStoredPath = value;
    }

    public string RecordEditorResolvedPath
    {
        get => RecordWorkspace.RecordEditorResolvedPath;
        set => RecordWorkspace.RecordEditorResolvedPath = value;
    }

    public string RecordEditorAvailability
    {
        get => RecordWorkspace.RecordEditorAvailability;
        set => RecordWorkspace.RecordEditorAvailability = value;
    }

    public string RecordEditorNote
    {
        get => RecordWorkspace.RecordEditorNote;
        set => RecordWorkspace.RecordEditorNote = value;
    }

    public bool IsRecordDetailVisible => RecordWorkspace.IsRecordDetailVisible;
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
    public bool IsRecordEditorManaged => RecordWorkspace.IsRecordEditorManaged;
    public string RecordEditorPathInputLabel => RecordWorkspace.RecordEditorPathInputLabel;
    public string RecordEditorPathInputHelp => RecordWorkspace.RecordEditorPathInputHelp;

    internal string? GetEditingHistoryId() => _editingHistoryId;
    internal string? GetEditingFuelId() => _editingFuelId;
    internal string? GetEditingReminderId() => _editingReminderId;
    internal string? GetEditingMaintenanceId() => _editingMaintenanceId;
    internal string? GetEditingRecordId() => _editingRecordId;

    internal string FormatWorkspaceValue(string? value, string fallback) => FormatValue(value, fallback);

    internal void NotifyHistoryWorkspaceSelectionChanged()
    {
        EditSelectedHistoryCommand.NotifyCanExecuteChanged();
        DeleteSelectedHistoryCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyHistoryWorkspaceEditingChanged()
    {
        CreateHistoryCommand.NotifyCanExecuteChanged();
        EditSelectedHistoryCommand.NotifyCanExecuteChanged();
        DeleteSelectedHistoryCommand.NotifyCanExecuteChanged();
        SaveHistoryCommand.NotifyCanExecuteChanged();
        CancelHistoryEditCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyFuelWorkspaceSelectionChanged()
    {
        EditSelectedFuelCommand.NotifyCanExecuteChanged();
        DeleteSelectedFuelCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyFuelWorkspaceEditingChanged()
    {
        CreateFuelCommand.NotifyCanExecuteChanged();
        EditSelectedFuelCommand.NotifyCanExecuteChanged();
        DeleteSelectedFuelCommand.NotifyCanExecuteChanged();
        SaveFuelCommand.NotifyCanExecuteChanged();
        CancelFuelEditCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyReminderWorkspaceSelectionChanged()
    {
        EditSelectedReminderCommand.NotifyCanExecuteChanged();
        DeleteSelectedReminderCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyReminderWorkspaceEditingChanged()
    {
        CreateReminderCommand.NotifyCanExecuteChanged();
        EditSelectedReminderCommand.NotifyCanExecuteChanged();
        DeleteSelectedReminderCommand.NotifyCanExecuteChanged();
        SaveReminderCommand.NotifyCanExecuteChanged();
        CancelReminderEditCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyMaintenanceWorkspaceSelectionChanged()
    {
        EditSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        DeleteSelectedMaintenanceCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyMaintenanceWorkspaceEditingChanged()
    {
        CreateMaintenanceCommand.NotifyCanExecuteChanged();
        EditSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        DeleteSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        SaveMaintenanceCommand.NotifyCanExecuteChanged();
        CancelMaintenanceEditCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyRecordWorkspaceSelectionChanged()
    {
        OpenSelectedRecordFileCommand.NotifyCanExecuteChanged();
        OpenSelectedRecordFolderCommand.NotifyCanExecuteChanged();
        EditSelectedRecordCommand.NotifyCanExecuteChanged();
        DeleteSelectedRecordCommand.NotifyCanExecuteChanged();
        MoveSelectedRecordToManagedCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyRecordWorkspaceEditingChanged()
    {
        CreateRecordCommand.NotifyCanExecuteChanged();
        EditSelectedRecordCommand.NotifyCanExecuteChanged();
        DeleteSelectedRecordCommand.NotifyCanExecuteChanged();
        SaveRecordCommand.NotifyCanExecuteChanged();
        CancelRecordEditCommand.NotifyCanExecuteChanged();
        BrowseRecordAttachmentCommand.NotifyCanExecuteChanged();
        MoveSelectedRecordToManagedCommand.NotifyCanExecuteChanged();
    }

    internal void HandleRecordAttachmentModeChanged()
    {
        PrimeRecordEditorPathForMode();
        RefreshRecordEditorAttachmentPreview();
    }

    internal void HandleRecordAttachmentPathChanged()
    {
        RefreshRecordEditorAttachmentPreview();
    }
}
