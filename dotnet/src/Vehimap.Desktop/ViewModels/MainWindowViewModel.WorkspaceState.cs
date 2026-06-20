namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private VehicleHistoryItemViewModel? SelectedHistory
    {
        get => HistoryWorkspace.SelectedHistory;
        set => HistoryWorkspace.SelectedHistory = value;
    }

    private string SelectedHistoryDetail
    {
        get => HistoryWorkspace.SelectedHistoryDetail;
        set => HistoryWorkspace.SelectedHistoryDetail = value;
    }

    private bool IsEditingHistory
    {
        get => HistoryWorkspace.IsEditingHistory;
        set => HistoryWorkspace.IsEditingHistory = value;
    }

    private string HistoryPanelHeading
    {
        get => HistoryWorkspace.HistoryPanelHeading;
        set => HistoryWorkspace.HistoryPanelHeading = value;
    }

    private string HistoryEditorStatus
    {
        get => HistoryWorkspace.HistoryEditorStatus;
        set => HistoryWorkspace.HistoryEditorStatus = value;
    }

    private string HistoryEditorDate
    {
        get => HistoryWorkspace.HistoryEditorDate;
        set => HistoryWorkspace.HistoryEditorDate = value;
    }

    private string HistoryEditorType
    {
        get => HistoryWorkspace.HistoryEditorType;
        set => HistoryWorkspace.HistoryEditorType = value;
    }

    private string HistoryEditorOdometer
    {
        get => HistoryWorkspace.HistoryEditorOdometer;
        set => HistoryWorkspace.HistoryEditorOdometer = value;
    }

    private string HistoryEditorCost
    {
        get => HistoryWorkspace.HistoryEditorCost;
        set => HistoryWorkspace.HistoryEditorCost = value;
    }

    private string HistoryEditorNote
    {
        get => HistoryWorkspace.HistoryEditorNote;
        set => HistoryWorkspace.HistoryEditorNote = value;
    }

    private bool IsHistoryDetailVisible => HistoryWorkspace.IsHistoryDetailVisible;
    private bool CanCreateHistory => SelectedVehicle is not null && !HasPendingEdits;
    private bool CanEditSelectedHistory => SelectedHistory is not null && !HasPendingEdits;
    private bool CanDeleteSelectedHistory => SelectedHistory is not null && !HasPendingEdits;
    private bool CanSaveHistory => SelectedVehicle is not null && IsEditingHistory;
    private bool CanCancelHistoryEdit => IsEditingHistory;

    private VehicleFuelItemViewModel? SelectedFuel
    {
        get => FuelWorkspace.SelectedFuel;
        set => FuelWorkspace.SelectedFuel = value;
    }

    private string SelectedFuelDetail
    {
        get => FuelWorkspace.SelectedFuelDetail;
        set => FuelWorkspace.SelectedFuelDetail = value;
    }

    private bool IsEditingFuel
    {
        get => FuelWorkspace.IsEditingFuel;
        set => FuelWorkspace.IsEditingFuel = value;
    }

    private string FuelPanelHeading
    {
        get => FuelWorkspace.FuelPanelHeading;
        set => FuelWorkspace.FuelPanelHeading = value;
    }

    private string FuelEditorStatus
    {
        get => FuelWorkspace.FuelEditorStatus;
        set => FuelWorkspace.FuelEditorStatus = value;
    }

    private string FuelEditorDate
    {
        get => FuelWorkspace.FuelEditorDate;
        set => FuelWorkspace.FuelEditorDate = value;
    }

    private string FuelEditorFuelType
    {
        get => FuelWorkspace.FuelEditorFuelType;
        set => FuelWorkspace.FuelEditorFuelType = value;
    }

    private string FuelEditorLiters
    {
        get => FuelWorkspace.FuelEditorLiters;
        set => FuelWorkspace.FuelEditorLiters = value;
    }

    private string FuelEditorTotalCost
    {
        get => FuelWorkspace.FuelEditorTotalCost;
        set => FuelWorkspace.FuelEditorTotalCost = value;
    }

    private string FuelEditorOdometer
    {
        get => FuelWorkspace.FuelEditorOdometer;
        set => FuelWorkspace.FuelEditorOdometer = value;
    }

    private bool FuelEditorFullTank
    {
        get => FuelWorkspace.FuelEditorFullTank;
        set => FuelWorkspace.FuelEditorFullTank = value;
    }

    private string FuelEditorNote
    {
        get => FuelWorkspace.FuelEditorNote;
        set => FuelWorkspace.FuelEditorNote = value;
    }

    private bool IsFuelDetailVisible => FuelWorkspace.IsFuelDetailVisible;
    private bool CanCreateFuel => SelectedVehicle is not null && !HasPendingEdits;
    private bool CanEditSelectedFuel => SelectedFuel is not null && !HasPendingEdits;
    private bool CanDeleteSelectedFuel => SelectedFuel is not null && !HasPendingEdits;
    private bool CanSaveFuel => SelectedVehicle is not null && IsEditingFuel;
    private bool CanCancelFuelEdit => IsEditingFuel;

    private VehicleReminderItemViewModel? SelectedReminder
    {
        get => ReminderWorkspace.SelectedReminder;
        set => ReminderWorkspace.SelectedReminder = value;
    }

    private string SelectedReminderDetail
    {
        get => ReminderWorkspace.SelectedReminderDetail;
        set => ReminderWorkspace.SelectedReminderDetail = value;
    }

    private bool IsEditingReminder
    {
        get => ReminderWorkspace.IsEditingReminder;
        set => ReminderWorkspace.IsEditingReminder = value;
    }

    private string ReminderPanelHeading
    {
        get => ReminderWorkspace.ReminderPanelHeading;
        set => ReminderWorkspace.ReminderPanelHeading = value;
    }

    private string ReminderEditorStatus
    {
        get => ReminderWorkspace.ReminderEditorStatus;
        set => ReminderWorkspace.ReminderEditorStatus = value;
    }

    private string ReminderEditorTitle
    {
        get => ReminderWorkspace.ReminderEditorTitle;
        set => ReminderWorkspace.ReminderEditorTitle = value;
    }

    private string ReminderEditorDueDate
    {
        get => ReminderWorkspace.ReminderEditorDueDate;
        set => ReminderWorkspace.ReminderEditorDueDate = value;
    }

    private string ReminderEditorDays
    {
        get => ReminderWorkspace.ReminderEditorDays;
        set => ReminderWorkspace.ReminderEditorDays = value;
    }

    private string ReminderEditorRepeatMode
    {
        get => ReminderWorkspace.ReminderEditorRepeatMode;
        set => ReminderWorkspace.ReminderEditorRepeatMode = value;
    }

    private string ReminderEditorNote
    {
        get => ReminderWorkspace.ReminderEditorNote;
        set => ReminderWorkspace.ReminderEditorNote = value;
    }

    private bool IsReminderDetailVisible => ReminderWorkspace.IsReminderDetailVisible;
    private bool CanCreateReminder => SelectedVehicle is not null && !HasPendingEdits;
    private bool CanEditSelectedReminder => SelectedReminder is not null && !HasPendingEdits;
    private bool CanDeleteSelectedReminder => SelectedReminder is not null && !HasPendingEdits;
    private bool CanAdvanceSelectedReminder => SelectedVehicle is not null && SelectedReminder is not null && !HasPendingEdits && TryBuildNextReminderDueDate(GetSelectedReminderModel(), out _);
    private bool CanSaveReminder => SelectedVehicle is not null && IsEditingReminder;
    private bool CanCancelReminderEdit => IsEditingReminder;

    private VehicleMaintenanceItemViewModel? SelectedMaintenance
    {
        get => MaintenanceWorkspace.SelectedMaintenance;
        set => MaintenanceWorkspace.SelectedMaintenance = value;
    }

    private string SelectedMaintenanceDetail
    {
        get => MaintenanceWorkspace.SelectedMaintenanceDetail;
        set => MaintenanceWorkspace.SelectedMaintenanceDetail = value;
    }

    private bool IsEditingMaintenance
    {
        get => MaintenanceWorkspace.IsEditingMaintenance;
        set => MaintenanceWorkspace.IsEditingMaintenance = value;
    }

    private string MaintenancePanelHeading
    {
        get => MaintenanceWorkspace.MaintenancePanelHeading;
        set => MaintenanceWorkspace.MaintenancePanelHeading = value;
    }

    private string MaintenanceEditorStatus
    {
        get => MaintenanceWorkspace.MaintenanceEditorStatus;
        set => MaintenanceWorkspace.MaintenanceEditorStatus = value;
    }

    private string SelectedMaintenanceTemplate
    {
        get => MaintenanceWorkspace.SelectedMaintenanceTemplate;
        set => MaintenanceWorkspace.SelectedMaintenanceTemplate = value;
    }

    private string MaintenanceEditorTitle
    {
        get => MaintenanceWorkspace.MaintenanceEditorTitle;
        set => MaintenanceWorkspace.MaintenanceEditorTitle = value;
    }

    private string MaintenanceEditorIntervalKm
    {
        get => MaintenanceWorkspace.MaintenanceEditorIntervalKm;
        set => MaintenanceWorkspace.MaintenanceEditorIntervalKm = value;
    }

    private string MaintenanceEditorIntervalMonths
    {
        get => MaintenanceWorkspace.MaintenanceEditorIntervalMonths;
        set => MaintenanceWorkspace.MaintenanceEditorIntervalMonths = value;
    }

    private string MaintenanceEditorLastServiceDate
    {
        get => MaintenanceWorkspace.MaintenanceEditorLastServiceDate;
        set => MaintenanceWorkspace.MaintenanceEditorLastServiceDate = value;
    }

    private string MaintenanceEditorLastServiceOdometer
    {
        get => MaintenanceWorkspace.MaintenanceEditorLastServiceOdometer;
        set => MaintenanceWorkspace.MaintenanceEditorLastServiceOdometer = value;
    }

    private bool MaintenanceEditorIsActive
    {
        get => MaintenanceWorkspace.MaintenanceEditorIsActive;
        set => MaintenanceWorkspace.MaintenanceEditorIsActive = value;
    }

    private string MaintenanceEditorNote
    {
        get => MaintenanceWorkspace.MaintenanceEditorNote;
        set => MaintenanceWorkspace.MaintenanceEditorNote = value;
    }

    private bool IsMaintenanceDetailVisible => MaintenanceWorkspace.IsMaintenanceDetailVisible;
    private bool CanCreateMaintenance => SelectedVehicle is not null && !HasPendingEdits;
    private bool CanEditSelectedMaintenance => SelectedMaintenance is not null && !HasPendingEdits;
    private bool CanDeleteSelectedMaintenance => SelectedMaintenance is not null && !HasPendingEdits;
    internal bool CanCompleteSelectedMaintenance => SelectedMaintenance is not null && !HasPendingEdits && GetSelectedMaintenanceModel()?.IsActive == true;
    internal bool CanOpenMaintenanceRecommendations => SelectedVehicle is not null && !HasPendingEdits;
    private bool CanSaveMaintenance => SelectedVehicle is not null && IsEditingMaintenance;
    private bool CanCancelMaintenanceEdit => IsEditingMaintenance;

    private VehicleRecordItemViewModel? SelectedRecord
    {
        get => RecordWorkspace.SelectedRecord;
        set => RecordWorkspace.SelectedRecord = value;
    }

    private string SelectedRecordDetail
    {
        get => RecordWorkspace.SelectedRecordDetail;
        set => RecordWorkspace.SelectedRecordDetail = value;
    }

    private bool IsEditingRecord
    {
        get => RecordWorkspace.IsEditingRecord;
        set => RecordWorkspace.IsEditingRecord = value;
    }

    private string RecordPanelHeading
    {
        get => RecordWorkspace.RecordPanelHeading;
        set => RecordWorkspace.RecordPanelHeading = value;
    }

    private string RecordEditorStatus
    {
        get => RecordWorkspace.RecordEditorStatus;
        set => RecordWorkspace.RecordEditorStatus = value;
    }

    private string RecordEditorRecordType
    {
        get => RecordWorkspace.RecordEditorRecordType;
        set => RecordWorkspace.RecordEditorRecordType = value;
    }

    private string RecordEditorTitle
    {
        get => RecordWorkspace.RecordEditorTitle;
        set => RecordWorkspace.RecordEditorTitle = value;
    }

    private string RecordEditorProvider
    {
        get => RecordWorkspace.RecordEditorProvider;
        set => RecordWorkspace.RecordEditorProvider = value;
    }

    private string RecordEditorValidFrom
    {
        get => RecordWorkspace.RecordEditorValidFrom;
        set => RecordWorkspace.RecordEditorValidFrom = value;
    }

    private string RecordEditorValidTo
    {
        get => RecordWorkspace.RecordEditorValidTo;
        set => RecordWorkspace.RecordEditorValidTo = value;
    }

    private string RecordEditorPrice
    {
        get => RecordWorkspace.RecordEditorPrice;
        set => RecordWorkspace.RecordEditorPrice = value;
    }

    private string SelectedRecordEditorAttachmentMode
    {
        get => RecordWorkspace.SelectedRecordEditorAttachmentMode;
        set => RecordWorkspace.SelectedRecordEditorAttachmentMode = value;
    }

    private string RecordEditorPathInput
    {
        get => RecordWorkspace.RecordEditorPathInput;
        set => RecordWorkspace.RecordEditorPathInput = value;
    }

    private string RecordEditorStoredPath
    {
        get => RecordWorkspace.RecordEditorStoredPath;
        set => RecordWorkspace.RecordEditorStoredPath = value;
    }

    private string RecordEditorResolvedPath
    {
        get => RecordWorkspace.RecordEditorResolvedPath;
        set => RecordWorkspace.RecordEditorResolvedPath = value;
    }

    private string RecordEditorAvailability
    {
        get => RecordWorkspace.RecordEditorAvailability;
        set => RecordWorkspace.RecordEditorAvailability = value;
    }

    private string RecordEditorNote
    {
        get => RecordWorkspace.RecordEditorNote;
        set => RecordWorkspace.RecordEditorNote = value;
    }

    private bool IsRecordDetailVisible => RecordWorkspace.IsRecordDetailVisible;
    private bool CanCreateRecord => SelectedVehicle is not null && !HasPendingEdits;
    private bool CanEditSelectedRecord => SelectedRecord is not null && !HasPendingEdits;
    private bool CanDeleteSelectedRecord => SelectedRecord is not null && !HasPendingEdits;
    private bool CanSaveRecord => SelectedVehicle is not null && IsEditingRecord;
    private bool CanCancelRecordEdit => IsEditingRecord;
    private bool CanBrowseRecordAttachment => IsEditingRecord;
    private bool CanMoveSelectedRecordToManaged =>
        SelectedRecord is not null
        && !HasPendingEdits
        && !string.Equals(SelectedRecord.AttachmentMode, "Spravovaná kopie", StringComparison.CurrentCulture)
        && !string.IsNullOrWhiteSpace(SelectedRecord.ResolvedPath);
    private bool IsRecordEditorManaged => RecordWorkspace.IsRecordEditorManaged;
    private string RecordEditorPathInputLabel => RecordWorkspace.RecordEditorPathInputLabel;
    private string RecordEditorPathInputHelp => RecordWorkspace.RecordEditorPathInputHelp;

    public bool CanOpenSelectedTimelineItem => TimelineWorkspace.SelectedTimelineItem is not null;
    public bool CanOpenSelectedDashboardAuditItem => AuditWorkspace.SelectedDashboardAuditItem is not null;
    public bool CanOpenSelectedDashboardCostVehicle => CostWorkspace.SelectedDashboardCostVehicle is not null;
    public bool CanOpenSelectedDashboardTimelineItem => DashboardWorkspace.SelectedDashboardTimelineItem is not null;
    public bool CanOpenSelectedDashboardVehicle => GetSelectedDashboardVehicleId() is not null;
    public bool CanEditSelectedDashboardVehicle => CanOpenSelectedDashboardVehicle && !HasPendingEdits;

    public bool CanOpenSelectedSearchResult => GlobalSearchWorkspace.SelectedSearchResult is not null;

    public bool CanOpenSelectedUpcomingOverviewItem => UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem is not null;
    public bool CanOpenSelectedUpcomingOverviewVehicle => UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem is not null;
    public bool CanOpenSelectedOverdueOverviewItem => OverdueOverviewWorkspace.SelectedOverdueOverviewItem is not null;
    public bool CanOpenSelectedOverdueOverviewVehicle => OverdueOverviewWorkspace.SelectedOverdueOverviewItem is not null;

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
        AdvanceSelectedReminderCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyReminderWorkspaceEditingChanged()
    {
        CreateReminderCommand.NotifyCanExecuteChanged();
        EditSelectedReminderCommand.NotifyCanExecuteChanged();
        DeleteSelectedReminderCommand.NotifyCanExecuteChanged();
        AdvanceSelectedReminderCommand.NotifyCanExecuteChanged();
        SaveReminderCommand.NotifyCanExecuteChanged();
        CancelReminderEditCommand.NotifyCanExecuteChanged();
        NotifyPendingEditStateChanged();
    }

    internal void NotifyMaintenanceWorkspaceSelectionChanged()
    {
        EditSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        DeleteSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        CompleteSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        MaintenanceWorkspace.NotifyMaintenanceCompletionStateChanged();
    }

    internal void NotifyMaintenanceWorkspaceEditingChanged()
    {
        CreateMaintenanceCommand.NotifyCanExecuteChanged();
        EditSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        DeleteSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        CompleteSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanOpenMaintenanceRecommendations));
        MaintenanceWorkspace.NotifyMaintenanceRecommendationStateChanged();
        MaintenanceWorkspace.NotifyMaintenanceCompletionStateChanged();
        SaveMaintenanceCommand.NotifyCanExecuteChanged();
        CancelMaintenanceEditCommand.NotifyCanExecuteChanged();
        NotifyPendingEditStateChanged();
    }

    internal void NotifyRecordWorkspaceSelectionChanged()
    {
        OpenSelectedRecordFileCommand.NotifyCanExecuteChanged();
        OpenSelectedRecordFolderCommand.NotifyCanExecuteChanged();
        CopySelectedRecordPathCommand.NotifyCanExecuteChanged();
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
        if (_suppressTimelinePreferenceRefresh)
        {
            return;
        }

        var normalizedFilter = NormalizeTimelineFilter(TimelineWorkspace.SelectedTimelineFilter);
        if (!string.Equals(TimelineWorkspace.SelectedTimelineFilter, normalizedFilter, StringComparison.Ordinal))
        {
            TimelineWorkspace.SelectedTimelineFilter = normalizedFilter;
            return;
        }

        RefreshTimeline();
        PersistTimelinePreferencesAsync();
    }

    internal void RefreshTimelineWorkspace()
    {
        RefreshTimeline();
        ShellStatus = "Časová osa byla obnovena.";
        RequestFocus(SelectedVehicleTimeline.Count == 0 ? DesktopFocusTarget.TimelineSearch : DesktopFocusTarget.TimelineList);
    }

    internal void NotifyAuditWorkspaceSelectionChanged()
    {
        DashboardWorkspace.NotifyDashboardAuditSelectionChanged();
        OpenSelectedDashboardAuditItemCommand.NotifyCanExecuteChanged();
        OpenSelectedDashboardVehicleCommand.NotifyCanExecuteChanged();
        EditSelectedDashboardVehicleCommand.NotifyCanExecuteChanged();
    }

    internal void RefreshAuditWorkspace()
    {
        AuditWorkspace.SetAuditSummary(_projectionService.BuildAuditSummary(_auditItems));

        AuditItems.Clear();
        foreach (var item in _projectionService.BuildAuditItems(_auditItems))
        {
            AuditItems.Add(item);
        }

        DashboardAuditItems.Clear();
        foreach (var item in _projectionService.BuildDashboardAuditItems(_auditItems))
        {
            DashboardAuditItems.Add(item);
        }

        AuditWorkspace.RefreshVisibleAuditItems();
        DashboardWorkspace.NotifyDashboardSummariesChanged();

        ShellStatus = "Audit dat byl obnoven.";
        RequestFocus(AuditWorkspace.VisibleAuditItems.Count == 0 ? DesktopFocusTarget.AuditSearch : DesktopFocusTarget.AuditList);
    }

    internal void NotifyCostWorkspaceSelectionChanged()
    {
        DashboardWorkspace.NotifyDashboardCostSelectionChanged();
        OpenSelectedDashboardCostVehicleCommand.NotifyCanExecuteChanged();
        OpenSelectedDashboardVehicleCommand.NotifyCanExecuteChanged();
        EditSelectedDashboardVehicleCommand.NotifyCanExecuteChanged();
        ExportSelectedVehicleCostDetailCommand.NotifyCanExecuteChanged();
        ExportSelectedVehicleCostReportCommand.NotifyCanExecuteChanged();
    }

    internal void RefreshCostWorkspace()
    {
        var previousCostVehicleId = CostWorkspace.SelectedDashboardCostVehicle?.VehicleId ?? string.Empty;
        _currentCostSummary = BuildSelectedCostSummary();

        CostWorkspace.CostSummary = _projectionService.BuildCostSummary(_currentCostSummary);
        CostWorkspace.CostComparison = _projectionService.BuildCostComparison(_currentCostSummary);

        CostVehicles.Clear();
        foreach (var row in _projectionService.BuildDashboardCostVehicles(_currentCostSummary))
        {
            CostVehicles.Add(row);
        }

        CostWorkspace.SelectedDashboardCostVehicle = FindById(CostVehicles, item => item.VehicleId, previousCostVehicleId);
        CostWorkspace.RefreshVisibleCostVehicles();
        DashboardWorkspace.NotifyDashboardSummariesChanged();
        ExportFleetCostSummaryCommand.NotifyCanExecuteChanged();
        ExportSelectedVehicleCostDetailCommand.NotifyCanExecuteChanged();
        ExportSelectedVehicleCostReportCommand.NotifyCanExecuteChanged();

        CostWorkspace.CostExportStatus = "Nákladový přehled byl obnoven.";
        ShellStatus = "Nákladový přehled byl obnoven.";
        RequestFocus(CostWorkspace.VisibleCostVehicles.Count == 0 ? DesktopFocusTarget.CostSearch : DesktopFocusTarget.CostList);
    }

    internal void NotifyDashboardWorkspaceTimelineSelectionChanged()
    {
        OpenSelectedDashboardTimelineItemCommand.NotifyCanExecuteChanged();
        OpenSelectedDashboardVehicleCommand.NotifyCanExecuteChanged();
        EditSelectedDashboardVehicleCommand.NotifyCanExecuteChanged();
    }

    internal void RefreshDashboardWorkspace()
    {
        var previousAuditKey = BuildDashboardAuditSelectionKey(AuditWorkspace.SelectedDashboardAuditItem);
        var previousCostVehicleId = CostWorkspace.SelectedDashboardCostVehicle?.VehicleId ?? string.Empty;
        var previousTimelineItem = DashboardWorkspace.SelectedDashboardTimelineItem;

        AuditWorkspace.SetAuditSummary(_projectionService.BuildAuditSummary(_auditItems));
        if (_session.IsLoaded)
        {
            _currentCostSummary = BuildSelectedCostSummary();
        }

        if (_currentCostSummary is not null)
        {
            CostWorkspace.CostSummary = _projectionService.BuildCostSummary(_currentCostSummary);
            CostWorkspace.CostComparison = _projectionService.BuildCostComparison(_currentCostSummary);
        }

        DashboardAuditItems.Clear();
        foreach (var item in _projectionService.BuildDashboardAuditItems(_auditItems))
        {
            DashboardAuditItems.Add(item);
        }

        CostVehicles.Clear();
        if (_currentCostSummary is not null)
        {
            foreach (var row in _projectionService.BuildDashboardCostVehicles(_currentCostSummary))
            {
                CostVehicles.Add(row);
            }
        }

        PopulateDashboardTimeline();

        AuditWorkspace.SelectedDashboardAuditItem = FindById(DashboardAuditItems, BuildDashboardAuditSelectionKey, previousAuditKey);
        CostWorkspace.SelectedDashboardCostVehicle = FindById(CostVehicles, item => item.VehicleId, previousCostVehicleId);
        CostWorkspace.RefreshVisibleCostVehicles();
        DashboardWorkspace.SelectedDashboardTimelineItem = previousTimelineItem is null
            ? DashboardUpcomingTimeline.FirstOrDefault()
            : FindTimelineItem(DashboardUpcomingTimeline, previousTimelineItem);

        DashboardWorkspace.NotifyDashboardSummariesChanged();
        ExportFleetCostSummaryCommand.NotifyCanExecuteChanged();
        ExportSelectedVehicleCostDetailCommand.NotifyCanExecuteChanged();
        ExportSelectedVehicleCostReportCommand.NotifyCanExecuteChanged();

        ShellStatus = "Dashboard byl obnoven.";
        RequestFocus(GetDashboardRefreshFocusTarget());
    }

    private DesktopFocusTarget GetDashboardRefreshFocusTarget()
    {
        if (DashboardAuditItems.Count > 0)
        {
            return DesktopFocusTarget.DashboardAuditList;
        }

        if (CostVehicles.Count > 0)
        {
            return DesktopFocusTarget.DashboardCostList;
        }

        return DesktopFocusTarget.DashboardTimelineList;
    }

    private static string BuildDashboardAuditSelectionKey(AuditItemViewModel? item) =>
        item is null ? string.Empty : $"{item.VehicleId}|{item.EntityKind}|{item.EntityId}";

    internal void HandleGlobalSearchWorkspaceSearchChanged()
    {
        RefreshGlobalSearch();
    }

    internal void RefreshGlobalSearchWorkspace()
    {
        RefreshGlobalSearch();
        ShellStatus = "Globální hledání bylo obnoveno.";
        RequestFocus(GlobalSearchResults.Count == 0 ? DesktopFocusTarget.GlobalSearchBox : DesktopFocusTarget.GlobalSearchList);
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
        if (_suppressOverviewPreferenceRefresh)
        {
            return;
        }

        var normalizedFilter = NormalizeUpcomingOverviewFilter(UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter);
        if (!string.Equals(UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter, normalizedFilter, StringComparison.Ordinal))
        {
            UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter = normalizedFilter;
            return;
        }

        RefreshUpcomingOverview();
        PersistOverviewPreferencesAsync();
    }

    internal void HandleUpcomingOverviewWorkspaceSortChanged()
    {
        if (_suppressOverviewPreferenceRefresh)
        {
            return;
        }

        RefreshUpcomingOverview();
        PersistOverviewPreferencesAsync();
    }

    internal void HandleUpcomingOverviewWorkspaceOptionsChanged()
    {
        if (_suppressOverviewPreferenceRefresh)
        {
            return;
        }

        RefreshUpcomingOverview();
        PersistOverviewPreferencesAsync();
    }

    internal void RefreshUpcomingOverviewWorkspace()
    {
        RefreshUpcomingOverview();
        ShellStatus = "Přehled blížících se termínů byl obnoven.";
        RequestFocus(UpcomingOverviewItems.Count == 0 ? DesktopFocusTarget.UpcomingOverviewSearch : DesktopFocusTarget.UpcomingOverviewList);
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
        if (_suppressOverviewPreferenceRefresh)
        {
            return;
        }

        var normalizedFilter = NormalizeOverdueOverviewFilter(OverdueOverviewWorkspace.SelectedOverdueOverviewFilter);
        if (!string.Equals(OverdueOverviewWorkspace.SelectedOverdueOverviewFilter, normalizedFilter, StringComparison.Ordinal))
        {
            OverdueOverviewWorkspace.SelectedOverdueOverviewFilter = normalizedFilter;
            return;
        }

        RefreshOverdueOverview();
        PersistOverviewPreferencesAsync();
    }

    internal void HandleOverdueOverviewWorkspaceSortChanged()
    {
        if (_suppressOverviewPreferenceRefresh)
        {
            return;
        }

        RefreshOverdueOverview();
        PersistOverviewPreferencesAsync();
    }

    internal void RefreshOverdueOverviewWorkspace()
    {
        RefreshOverdueOverview();
        ShellStatus = "Přehled propadlých termínů byl obnoven.";
        RequestFocus(OverdueOverviewItems.Count == 0 ? DesktopFocusTarget.OverdueOverviewSearch : DesktopFocusTarget.OverdueOverviewList);
    }

    internal void NotifyOverdueOverviewWorkspaceSelectionChanged()
    {
        OpenSelectedOverdueOverviewItemCommand.NotifyCanExecuteChanged();
        OpenSelectedOverdueOverviewVehicleCommand.NotifyCanExecuteChanged();
    }
}
