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
    public bool CanCreateHistory => SelectedVehicle is not null && !HasPendingEdits;
    public bool CanEditSelectedHistory => SelectedHistory is not null && !HasPendingEdits;
    public bool CanDeleteSelectedHistory => SelectedHistory is not null && !HasPendingEdits;
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
    public bool CanCreateFuel => SelectedVehicle is not null && !HasPendingEdits;
    public bool CanEditSelectedFuel => SelectedFuel is not null && !HasPendingEdits;
    public bool CanDeleteSelectedFuel => SelectedFuel is not null && !HasPendingEdits;
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
    public bool CanCreateReminder => SelectedVehicle is not null && !HasPendingEdits;
    public bool CanEditSelectedReminder => SelectedReminder is not null && !HasPendingEdits;
    public bool CanDeleteSelectedReminder => SelectedReminder is not null && !HasPendingEdits;
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
    public bool CanCreateMaintenance => SelectedVehicle is not null && !HasPendingEdits;
    public bool CanEditSelectedMaintenance => SelectedMaintenance is not null && !HasPendingEdits;
    public bool CanDeleteSelectedMaintenance => SelectedMaintenance is not null && !HasPendingEdits;
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
    public bool CanCreateRecord => SelectedVehicle is not null && !HasPendingEdits;
    public bool CanEditSelectedRecord => SelectedRecord is not null && !HasPendingEdits;
    public bool CanDeleteSelectedRecord => SelectedRecord is not null && !HasPendingEdits;
    public bool CanSaveRecord => SelectedVehicle is not null && IsEditingRecord;
    public bool CanCancelRecordEdit => IsEditingRecord;
    public bool CanBrowseRecordAttachment => IsEditingRecord;
    public bool CanMoveSelectedRecordToManaged =>
        SelectedRecord is not null
        && !HasPendingEdits
        && !string.Equals(SelectedRecord.AttachmentMode, "Spravovaná kopie", StringComparison.CurrentCulture)
        && !string.IsNullOrWhiteSpace(SelectedRecord.ResolvedPath);
    public bool IsRecordEditorManaged => RecordWorkspace.IsRecordEditorManaged;
    public string RecordEditorPathInputLabel => RecordWorkspace.RecordEditorPathInputLabel;
    public string RecordEditorPathInputHelp => RecordWorkspace.RecordEditorPathInputHelp;

    public string AuditSummary
    {
        get => AuditWorkspace.AuditSummary;
        set => AuditWorkspace.AuditSummary = value;
    }

    public AuditItemViewModel? SelectedDashboardAuditItem
    {
        get => AuditWorkspace.SelectedDashboardAuditItem;
        set => AuditWorkspace.SelectedDashboardAuditItem = value;
    }

    public string CostSummary
    {
        get => CostWorkspace.CostSummary;
        set => CostWorkspace.CostSummary = value;
    }

    public string CostComparison
    {
        get => CostWorkspace.CostComparison;
        set => CostWorkspace.CostComparison = value;
    }

    public CostVehicleItemViewModel? SelectedDashboardCostVehicle
    {
        get => CostWorkspace.SelectedDashboardCostVehicle;
        set => CostWorkspace.SelectedDashboardCostVehicle = value;
    }

    public string DashboardTimelineSummary
    {
        get => DashboardWorkspace.DashboardTimelineSummary;
        set => DashboardWorkspace.DashboardTimelineSummary = value;
    }

    public string SelectedDashboardTimelineDetail
    {
        get => DashboardWorkspace.SelectedDashboardTimelineDetail;
        set => DashboardWorkspace.SelectedDashboardTimelineDetail = value;
    }

    public VehicleTimelineItemViewModel? SelectedDashboardTimelineItem
    {
        get => DashboardWorkspace.SelectedDashboardTimelineItem;
        set => DashboardWorkspace.SelectedDashboardTimelineItem = value;
    }

    public string TimelineSummary
    {
        get => TimelineWorkspace.TimelineSummary;
        set => TimelineWorkspace.TimelineSummary = value;
    }

    public string TimelineSearchText
    {
        get => TimelineWorkspace.TimelineSearchText;
        set => TimelineWorkspace.TimelineSearchText = value;
    }

    public string SelectedTimelineFilter
    {
        get => TimelineWorkspace.SelectedTimelineFilter;
        set => TimelineWorkspace.SelectedTimelineFilter = value;
    }

    public VehicleTimelineItemViewModel? SelectedTimelineItem
    {
        get => TimelineWorkspace.SelectedTimelineItem;
        set => TimelineWorkspace.SelectedTimelineItem = value;
    }

    public string SelectedTimelineDetail
    {
        get => TimelineWorkspace.SelectedTimelineDetail;
        set => TimelineWorkspace.SelectedTimelineDetail = value;
    }

    public string ExportStatus
    {
        get => TimelineWorkspace.ExportStatus;
        set => TimelineWorkspace.ExportStatus = value;
    }

    public IReadOnlyList<string> TimelineFilters => TimelineWorkspace.TimelineFilters;
    public bool CanOpenSelectedTimelineItem => SelectedTimelineItem is not null;
    public bool CanOpenSelectedDashboardAuditItem => SelectedDashboardAuditItem is not null;
    public bool CanOpenSelectedDashboardCostVehicle => SelectedDashboardCostVehicle is not null;
    public bool CanOpenSelectedDashboardTimelineItem => SelectedDashboardTimelineItem is not null;

    public string GlobalSearchSummary
    {
        get => GlobalSearchWorkspace.GlobalSearchSummary;
        set => GlobalSearchWorkspace.GlobalSearchSummary = value;
    }

    public string GlobalSearchText
    {
        get => GlobalSearchWorkspace.GlobalSearchText;
        set => GlobalSearchWorkspace.GlobalSearchText = value;
    }

    public GlobalSearchResultItemViewModel? SelectedSearchResult
    {
        get => GlobalSearchWorkspace.SelectedSearchResult;
        set => GlobalSearchWorkspace.SelectedSearchResult = value;
    }

    public string SelectedSearchResultDetail
    {
        get => GlobalSearchWorkspace.SelectedSearchResultDetail;
        set => GlobalSearchWorkspace.SelectedSearchResultDetail = value;
    }

    public bool CanOpenSelectedSearchResult => SelectedSearchResult is not null;

    public string UpcomingOverviewSearchText
    {
        get => UpcomingOverviewWorkspace.UpcomingOverviewSearchText;
        set => UpcomingOverviewWorkspace.UpcomingOverviewSearchText = value;
    }

    public string OverdueOverviewSearchText
    {
        get => OverdueOverviewWorkspace.OverdueOverviewSearchText;
        set => OverdueOverviewWorkspace.OverdueOverviewSearchText = value;
    }

    public string SelectedUpcomingOverviewFilter
    {
        get => UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter;
        set => UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter = value;
    }

    public string SelectedOverdueOverviewFilter
    {
        get => OverdueOverviewWorkspace.SelectedOverdueOverviewFilter;
        set => OverdueOverviewWorkspace.SelectedOverdueOverviewFilter = value;
    }

    public string UpcomingOverviewSummary
    {
        get => UpcomingOverviewWorkspace.UpcomingOverviewSummary;
        set => UpcomingOverviewWorkspace.UpcomingOverviewSummary = value;
    }

    public string OverdueOverviewSummary
    {
        get => OverdueOverviewWorkspace.OverdueOverviewSummary;
        set => OverdueOverviewWorkspace.OverdueOverviewSummary = value;
    }

    public string SelectedUpcomingOverviewDetail
    {
        get => UpcomingOverviewWorkspace.SelectedUpcomingOverviewDetail;
        set => UpcomingOverviewWorkspace.SelectedUpcomingOverviewDetail = value;
    }

    public string SelectedOverdueOverviewDetail
    {
        get => OverdueOverviewWorkspace.SelectedOverdueOverviewDetail;
        set => OverdueOverviewWorkspace.SelectedOverdueOverviewDetail = value;
    }

    public VehicleTimelineItemViewModel? SelectedUpcomingOverviewItem
    {
        get => UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem;
        set => UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem = value;
    }

    public VehicleTimelineItemViewModel? SelectedOverdueOverviewItem
    {
        get => OverdueOverviewWorkspace.SelectedOverdueOverviewItem;
        set => OverdueOverviewWorkspace.SelectedOverdueOverviewItem = value;
    }

    public bool CanOpenSelectedUpcomingOverviewItem => SelectedUpcomingOverviewItem is not null;
    public bool CanOpenSelectedUpcomingOverviewVehicle => SelectedUpcomingOverviewItem is not null;
    public bool CanOpenSelectedOverdueOverviewItem => SelectedOverdueOverviewItem is not null;
    public bool CanOpenSelectedOverdueOverviewVehicle => SelectedOverdueOverviewItem is not null;

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
        NotifyPendingEditStateChanged();
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
        NotifyPendingEditStateChanged();
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
        NotifyPendingEditStateChanged();
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
        NotifyPendingEditStateChanged();
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
        NotifyPendingEditStateChanged();
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

    internal void NotifyTimelineWorkspaceSelectionChanged()
    {
        OpenSelectedTimelineItemCommand.NotifyCanExecuteChanged();
    }

    internal void HandleTimelineWorkspaceSearchChanged()
    {
        RefreshTimeline();
    }

    internal void HandleTimelineWorkspaceFilterChanged()
    {
        RefreshTimeline();
    }

    internal void NotifyAuditWorkspaceSelectionChanged()
    {
        OpenSelectedDashboardAuditItemCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyCostWorkspaceSelectionChanged()
    {
        OpenSelectedDashboardCostVehicleCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyDashboardWorkspaceTimelineSelectionChanged()
    {
        OpenSelectedDashboardTimelineItemCommand.NotifyCanExecuteChanged();
    }

    internal void HandleGlobalSearchWorkspaceSearchChanged()
    {
        RefreshGlobalSearch();
    }

    internal void NotifyGlobalSearchWorkspaceSelectionChanged()
    {
        OpenSelectedSearchResultCommand.NotifyCanExecuteChanged();
    }

    internal void HandleUpcomingOverviewWorkspaceSearchChanged()
    {
        RefreshUpcomingOverview();
    }

    internal void HandleUpcomingOverviewWorkspaceFilterChanged()
    {
        RefreshUpcomingOverview();
    }

    internal void NotifyUpcomingOverviewWorkspaceSelectionChanged()
    {
        OpenSelectedUpcomingOverviewItemCommand.NotifyCanExecuteChanged();
        OpenSelectedUpcomingOverviewVehicleCommand.NotifyCanExecuteChanged();
    }

    internal void HandleOverdueOverviewWorkspaceSearchChanged()
    {
        RefreshOverdueOverview();
    }

    internal void HandleOverdueOverviewWorkspaceFilterChanged()
    {
        RefreshOverdueOverview();
    }

    internal void NotifyOverdueOverviewWorkspaceSelectionChanged()
    {
        OpenSelectedOverdueOverviewItemCommand.NotifyCanExecuteChanged();
        OpenSelectedOverdueOverviewVehicleCommand.NotifyCanExecuteChanged();
    }
}
