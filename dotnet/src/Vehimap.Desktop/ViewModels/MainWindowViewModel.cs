// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels.Workspaces;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Vehimap.Storage.Legacy;
using Vehimap.Storage.Sqlite;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private const int DetailTabIndex = DesktopTabIndexes.Detail;
    private const int HistoryTabIndex = DesktopTabIndexes.History;
    private const int FuelTabIndex = DesktopTabIndexes.Fuel;
    private const int ReminderTabIndex = DesktopTabIndexes.Reminder;
    private const int MaintenanceTabIndex = DesktopTabIndexes.Maintenance;
    private const int TimelineTabIndex = DesktopTabIndexes.Timeline;
    private const int RecordTabIndex = DesktopTabIndexes.Record;
    private const int AuditTabIndex = DesktopTabIndexes.Audit;
    private const int CostTabIndex = DesktopTabIndexes.Cost;
    private const int DashboardTabIndex = DesktopTabIndexes.Dashboard;
    private const int SearchTabIndex = DesktopTabIndexes.Search;
    private const int UpcomingOverviewTabIndex = DesktopTabIndexes.UpcomingOverview;
    private const int OverdueOverviewTabIndex = DesktopTabIndexes.OverdueOverview;
    private const int SmartAdvisorTabIndex = DesktopTabIndexes.SmartAdvisor;

    private readonly DesktopSessionController _session;
    private readonly IFileLauncher _fileLauncher;
    private readonly IFilePickerService _filePickerService;
    private readonly IClipboardService _clipboardService;
    private readonly IGlobalSearchService _globalSearchService;
    private readonly ITimelineService _timelineService;
    private readonly IFuelAnalysisService _fuelAnalysisService;
    private readonly IServiceBookService _serviceBookService;
    private readonly ISmartAdvisorService _smartAdvisorService;
    private readonly IVehiclePackageService _vehiclePackageService;
    private readonly ICalendarExportService _calendarExportService;
    private readonly ITextFileSaveService _fileSaveService;
    private readonly IFileDialogService _fileDialogService;
    private readonly DesktopProjectionService _projectionService;
    private readonly DesktopNavigationCoordinator _navigationCoordinator;
    private readonly DesktopPrintableVehicleReportService _printableVehicleReportService;
    private readonly DesktopServiceBookExportService _serviceBookExportService;
    private readonly DesktopCostExportService _costExportService = new();
    private VehimapDataRoot? _dataRoot => _session.DataRoot;
    private VehimapDataSet _dataSet => _session.DataSet;
    private IReadOnlyList<AuditItem> _auditItems => _session.AuditItems;
    private IReadOnlyDictionary<string, VehicleMeta> _metaByVehicleId => _session.MetaByVehicleId;
    private CostAnalysisSummary? _currentCostSummary;

    public event Action<DesktopFocusTarget>? FocusRequested;

    public event Action? BackgroundRefreshRequested;

    [ObservableProperty]
    private string title = "Vehimap";

    [ObservableProperty]
    private string subtitle = LO("Shell.Subtitle.Stable");

    [ObservableProperty]
    private string dataMode = string.Empty;

    [ObservableProperty]
    private string dataPath = string.Empty;

    [ObservableProperty]
    private string loadError = string.Empty;

    [ObservableProperty]
    private int vehicleCount;

    [ObservableProperty]
    private int historyCount;

    [ObservableProperty]
    private int fuelCount;

    [ObservableProperty]
    private int recordsCount;

    [ObservableProperty]
    private int remindersCount;

    [ObservableProperty]
    private int maintenanceCount;

    [ObservableProperty]
    private int auditCount;

    [ObservableProperty]
    private string shellStatus = LO("Shell.Status.Ready");

    [ObservableProperty]
    private bool isMinimizeToTrayAvailable;

    [ObservableProperty]
    private int selectedVehicleTabIndex;

    [ObservableProperty]
    private VehicleListItemViewModel? selectedVehicle;

    public ObservableCollection<VehicleListItemViewModel> Vehicles { get; } = [];

    private ObservableCollection<AuditItemViewModel> AuditItems => AuditWorkspace.AuditItems;

    private ObservableCollection<AuditItemViewModel> DashboardAuditItems => DashboardWorkspace.AuditItems;

    private ObservableCollection<CostVehicleItemViewModel> CostVehicles => CostWorkspace.CostVehicles;

    private ObservableCollection<VehicleTimelineItemViewModel> DashboardUpcomingTimeline => DashboardWorkspace.DashboardUpcomingTimeline;

    private ObservableCollection<VehicleHistoryItemViewModel> SelectedVehicleHistory => HistoryWorkspace.SelectedVehicleHistory;

    private ObservableCollection<VehicleFuelItemViewModel> SelectedVehicleFuel => FuelWorkspace.SelectedVehicleFuel;

    private ObservableCollection<VehicleReminderItemViewModel> SelectedVehicleReminders => ReminderWorkspace.SelectedVehicleReminders;

    private ObservableCollection<VehicleMaintenanceItemViewModel> SelectedVehicleMaintenance => MaintenanceWorkspace.SelectedVehicleMaintenance;

    private ObservableCollection<VehicleTimelineItemViewModel> SelectedVehicleTimeline => TimelineWorkspace.SelectedVehicleTimeline;

    private ObservableCollection<VehicleRecordItemViewModel> SelectedVehicleRecords => RecordWorkspace.SelectedVehicleRecords;

    private ObservableCollection<GlobalSearchResultItemViewModel> GlobalSearchResults => GlobalSearchWorkspace.GlobalSearchResults;

    internal DesktopAppShellController AppShellController { get; }

    public bool CanOpenReminderWindow => SelectedVehicle is not null && CanUseWorkspaceNavigation;

    public bool CanOpenRecordWindow => SelectedVehicle is not null && CanUseWorkspaceNavigation;

    public bool CanOpenHistoryWindow => SelectedVehicle is not null && CanUseWorkspaceNavigation;

    public bool CanOpenFuelWindow => SelectedVehicle is not null && CanUseWorkspaceNavigation;

    public bool CanOpenMaintenanceWindow => SelectedVehicle is not null && CanUseWorkspaceNavigation;

    public bool CanOpenVehicleDetailWindow => SelectedVehicle is not null && CanUseWorkspaceNavigation;

    public bool CanOpenSelectedVehicleCosts => SelectedVehicle is not null && !HasPendingEdits;

    public bool CanOpenSelectedVehicleServiceBook => SelectedVehicle is not null && !HasPendingEdits;

    public bool CanExportSelectedVehiclePackage => SelectedVehicle is not null && CanUseDataActions;

    public bool CanImportVehiclePackage => CanUseDataActions;

    public bool CanUseDataActions => _session.IsLoaded && !HasPendingEdits;

    public bool CanOpenDataFolder => CanUseDataActions && _dataRoot is not null && !string.IsNullOrWhiteSpace(_dataRoot.DataPath);

    public bool CanCreateAutomaticBackupNow => CanUseDataActions;

    public bool CanOpenAutomaticBackupFolder => CanUseDataActions && !string.IsNullOrWhiteSpace(_session.GetAutomaticBackupDirectoryPath());

    public bool CanOpenPreMigrationBackupFolder =>
        CanUseDataActions && Directory.Exists(_session.GetPreMigrationBackupPath());

    public bool CanCheckDataStoreHealth => CanUseDataActions;

    internal string HistoryWindowTitle =>
        SelectedVehicle is null
            ? LO("Window.History.Title.Empty")
            : LFO("Window.History.Title.Vehicle", SelectedVehicle.Name);

    internal string FuelWindowTitle =>
        SelectedVehicle is null
            ? LO("Window.Fuel.Title.Empty")
            : LFO("Window.Fuel.Title.Vehicle", SelectedVehicle.Name);

    internal string ReminderWindowTitle =>
        SelectedVehicle is null
            ? LO("Window.Reminder.Title.Empty")
            : LFO("Window.Reminder.Title.Vehicle", SelectedVehicle.Name);

    internal string MaintenanceWindowTitle =>
        SelectedVehicle is null
            ? LO("Window.Maintenance.Title.Empty")
            : LFO("Window.Maintenance.Title.Vehicle", SelectedVehicle.Name);

    internal string RecordWindowTitle =>
        SelectedVehicle is null
            ? LO("Window.Record.Title.Empty")
            : LFO("Window.Record.Title.Vehicle", SelectedVehicle.Name);

    internal string VehicleDetailWindowTitle =>
        SelectedVehicle is null
            ? LO("Window.VehicleDetail.Title.Empty")
            : LFO("Window.VehicleDetail.Title.Vehicle", SelectedVehicle.Name);

    internal string TimelineWindowTitle =>
        SelectedVehicle is null
            ? LO("Window.Timeline.Title.Empty")
            : LFO("Window.Timeline.Title.Vehicle", SelectedVehicle.Name);

    internal string AuditWindowTitle => LO("Window.Audit.Title");

    internal string CostWindowTitle => LO("Window.Cost.Title");

    internal string DashboardWindowTitle => LO("Window.Dashboard.Title");

    internal string GlobalSearchWindowTitle => LO("Window.GlobalSearch.Title");

    internal string UpcomingOverviewWindowTitle => LO("Window.Upcoming.Title");

    internal string OverdueOverviewWindowTitle => LO("Window.Overdue.Title");

    internal string SmartAdvisorWindowTitle => LO("Window.SmartAdvisor.Title");

    public bool IsDetailTabSelected => SelectedVehicleTabIndex == DetailTabIndex;

    public bool IsHistoryTabSelected => SelectedVehicleTabIndex == HistoryTabIndex;

    public bool IsFuelTabSelected => SelectedVehicleTabIndex == FuelTabIndex;

    public bool IsReminderTabSelected => SelectedVehicleTabIndex == ReminderTabIndex;

    public bool IsMaintenanceTabSelected => SelectedVehicleTabIndex == MaintenanceTabIndex;

    public bool IsTimelineTabSelected => SelectedVehicleTabIndex == TimelineTabIndex;

    public bool IsRecordTabSelected => SelectedVehicleTabIndex == RecordTabIndex;

    public bool IsAuditTabSelected => SelectedVehicleTabIndex == AuditTabIndex;

    public bool IsCostTabSelected => SelectedVehicleTabIndex == CostTabIndex;

    public bool IsDashboardTabSelected => SelectedVehicleTabIndex == DashboardTabIndex;

    public bool IsSearchTabSelected => SelectedVehicleTabIndex == SearchTabIndex;

    public bool IsUpcomingOverviewTabSelected => SelectedVehicleTabIndex == UpcomingOverviewTabIndex;

    public bool IsOverdueOverviewTabSelected => SelectedVehicleTabIndex == OverdueOverviewTabIndex;

    public bool IsSmartAdvisorTabSelected => SelectedVehicleTabIndex == SmartAdvisorTabIndex;

    public bool IsCurrentWorkspacePrimaryOpenShortcutContext =>
        SelectedVehicleTabIndex is RecordTabIndex or AuditTabIndex or CostTabIndex or DashboardTabIndex or SearchTabIndex or UpcomingOverviewTabIndex or OverdueOverviewTabIndex or SmartAdvisorTabIndex;

    public bool IsCurrentWorkspaceItemOpenShortcutContext =>
        SelectedVehicleTabIndex is TimelineTabIndex or AuditTabIndex or CostTabIndex or DashboardTabIndex or SearchTabIndex or UpcomingOverviewTabIndex or OverdueOverviewTabIndex or SmartAdvisorTabIndex;

    public bool IsCurrentWorkspaceCreateShortcutContext =>
        SelectedVehicleTabIndex is HistoryTabIndex or FuelTabIndex or ReminderTabIndex or MaintenanceTabIndex or RecordTabIndex;

    public bool IsCurrentWorkspaceEditShortcutContext =>
        SelectedVehicleTabIndex is HistoryTabIndex or FuelTabIndex or ReminderTabIndex or MaintenanceTabIndex or RecordTabIndex or AuditTabIndex or CostTabIndex or DashboardTabIndex;

    public bool IsCurrentWorkspaceSaveShortcutContext =>
        SelectedVehicleTabIndex is DetailTabIndex or HistoryTabIndex or FuelTabIndex or ReminderTabIndex or MaintenanceTabIndex or RecordTabIndex;

    public MainWindowViewModel()
        : this(
            new SqliteVehimapDataStore(),
            CreateDefaultBootstrapper(),
            new ManagedAttachmentPathService(),
            new ProcessFileLauncher(),
            new AvaloniaFilePickerService(),
            new LegacyGlobalSearchService(new ManagedAttachmentPathService(), new LegacyTimelineService(DesktopLocalization.LiveLocalizer), DesktopLocalization.LiveLocalizer),
            new LegacyTimelineService(DesktopLocalization.LiveLocalizer),
            new LegacyCalendarExportService(new LegacyTimelineService(DesktopLocalization.LiveLocalizer), DesktopLocalization.LiveLocalizer),
            new AvaloniaTextFileSaveService(),
            new SqliteBackupService(DesktopLocalization.LiveLocalizer),
            new AvaloniaFileDialogService(),
            new DesktopSupportedSettingsService(),
            new AssemblyAppBuildInfoProvider(() => DesktopLocalization.Localizer),
            new PlatformAutostartService(),
            null,
            new DesktopProjectionService(DesktopLocalization.LiveLocalizer, DesktopLocalization.CurrentCulture),
            new DesktopNavigationCoordinator(),
            new DesktopPrintableVehicleReportService(DesktopLocalization.LiveLocalizer),
            new AvaloniaAppShellDialogService(),
            new ProcessUpdateInstallLauncher())
    {
    }

    private static LegacyVehimapBootstrapper CreateDefaultBootstrapper()
    {
        var sqliteDataStore = new SqliteVehimapDataStore();
        var legacyDataStore = new LegacyVehimapDataStore(DesktopLocalization.LiveLocalizer);
        return new LegacyVehimapBootstrapper(
            new LegacyDataRootLocator(AssemblyAppBuildInfoProvider.ResolveCurrentApplicationDataFolderName()),
            sqliteDataStore,
            new SqliteDataMigrationService(legacyDataStore, sqliteDataStore, DesktopLocalization.LiveLocalizer));
    }

    internal MainWindowViewModel(
        IVehimapDataStore dataStore,
        LegacyVehimapBootstrapper bootstrapper,
        IFileAttachmentService attachmentService,
        IFileLauncher fileLauncher,
        IFilePickerService filePickerService,
        IGlobalSearchService globalSearchService,
        ITimelineService timelineService,
        ICalendarExportService calendarExportService,
        ITextFileSaveService fileSaveService,
        IBackupService? backupService = null,
        IFileDialogService? fileDialogService = null,
        DesktopSupportedSettingsService? supportedSettingsService = null,
        IAppBuildInfoProvider? appBuildInfoProvider = null,
        IAutostartService? autostartService = null,
        IUpdateService? updateService = null,
        DesktopProjectionService? projectionService = null,
        DesktopNavigationCoordinator? navigationCoordinator = null,
        DesktopPrintableVehicleReportService? printableVehicleReportService = null,
        IAppShellDialogService? appShellDialogService = null,
        IUpdateInstallLauncher? updateInstallLauncher = null,
        IClipboardService? clipboardService = null,
        IFuelAnalysisService? fuelAnalysisService = null,
        IServiceBookService? serviceBookService = null,
        DesktopServiceBookExportService? serviceBookExportService = null,
        ISmartAdvisorService? smartAdvisorService = null,
        IVehiclePackageService? vehiclePackageService = null,
        IDataStoreHealthService? dataStoreHealthService = null)
    {
        var sessionBackupService = backupService ?? new SqliteBackupService(DesktopLocalization.LiveLocalizer);
        var sessionSupportedSettingsService = supportedSettingsService ?? new DesktopSupportedSettingsService();
        var sessionAppBuildInfoProvider = appBuildInfoProvider ?? new AssemblyAppBuildInfoProvider(() => DesktopLocalization.Localizer);
        var sessionAutostartService = autostartService ?? new PlatformAutostartService();
        var sessionUpdateService = updateService ?? new LegacyUpdateService(
            sessionAppBuildInfoProvider,
            localizerProvider: () => DesktopLocalization.Localizer);

        _session = new DesktopSessionController(
            bootstrapper,
            dataStore,
            attachmentService,
            new LegacyAuditService(attachmentService, DesktopLocalization.LiveLocalizer),
            new LegacyCostAnalysisService(DesktopLocalization.LiveLocalizer),
            sessionBackupService,
            sessionAutostartService,
            sessionSupportedSettingsService,
            sessionAppBuildInfoProvider,
            sessionUpdateService,
            dataStoreHealthService ?? new SqliteDataStoreHealthService(DesktopLocalization.LiveLocalizer),
            supportedSettingsApplied: ApplyDesktopLocalization);
        _fileLauncher = fileLauncher;
        _filePickerService = filePickerService;
        _clipboardService = clipboardService ?? new AvaloniaClipboardService();
        _globalSearchService = globalSearchService;
        _timelineService = timelineService;
        _fuelAnalysisService = fuelAnalysisService ?? new LegacyFuelAnalysisService(DesktopLocalization.LiveLocalizer);
        _serviceBookService = serviceBookService ?? new LegacyServiceBookService(DesktopLocalization.LiveLocalizer);
        _smartAdvisorService = smartAdvisorService ?? new LegacySmartAdvisorService(_timelineService, _fuelAnalysisService, DesktopLocalization.LiveLocalizer);
        _vehiclePackageService = vehiclePackageService ?? new VehiclePackageService(DesktopLocalization.LiveLocalizer);
        _calendarExportService = calendarExportService;
        _fileSaveService = fileSaveService;
        _fileDialogService = fileDialogService ?? new AvaloniaFileDialogService();
        _projectionService = projectionService ?? new DesktopProjectionService(DesktopLocalization.LiveLocalizer, DesktopLocalization.CurrentCulture);
        _navigationCoordinator = navigationCoordinator ?? new DesktopNavigationCoordinator();
        _printableVehicleReportService = printableVehicleReportService ?? new DesktopPrintableVehicleReportService(DesktopLocalization.LiveLocalizer);
        _serviceBookExportService = serviceBookExportService ?? new DesktopServiceBookExportService(DesktopLocalization.LiveLocalizer);
        AppShellController = new DesktopAppShellController(
            appShellDialogService ?? new AvaloniaAppShellDialogService(),
            updateInstallLauncher ?? new ProcessUpdateInstallLauncher());
        InitializeWorkspaces();
        Load(applyLaunchTabPreference: true);
    }

    private static void ApplyDesktopLocalization(DesktopSupportedSettingsSnapshot settings) =>
        DesktopLocalization.Configure(new AppCulturePreferences(
            settings.Language,
            settings.ThousandsSeparator,
            settings.DecimalSeparator));

    private bool CanOpenSelectedRecordFile => SelectedRecord is { FileExists: true } && !string.IsNullOrWhiteSpace(SelectedRecord.ResolvedPath);

    private bool CanOpenSelectedRecordFolder =>
        SelectedRecord is not null
        && !string.IsNullOrWhiteSpace(GetSelectedRecordFolderPath());

    private bool CanCopySelectedRecordPath =>
        SelectedRecord is not null
        && !string.IsNullOrWhiteSpace(GetSelectedRecordCopyPath());

    partial void OnSelectedVehicleTabIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsDetailTabSelected));
        OnPropertyChanged(nameof(IsHistoryTabSelected));
        OnPropertyChanged(nameof(IsFuelTabSelected));
        OnPropertyChanged(nameof(IsReminderTabSelected));
        OnPropertyChanged(nameof(IsMaintenanceTabSelected));
        OnPropertyChanged(nameof(IsTimelineTabSelected));
        OnPropertyChanged(nameof(IsRecordTabSelected));
        OnPropertyChanged(nameof(IsAuditTabSelected));
        OnPropertyChanged(nameof(IsCostTabSelected));
        OnPropertyChanged(nameof(IsDashboardTabSelected));
        OnPropertyChanged(nameof(IsSearchTabSelected));
        OnPropertyChanged(nameof(IsUpcomingOverviewTabSelected));
        OnPropertyChanged(nameof(IsOverdueOverviewTabSelected));
        OnPropertyChanged(nameof(IsSmartAdvisorTabSelected));
        OnPropertyChanged(nameof(IsCurrentWorkspacePrimaryOpenShortcutContext));
        OnPropertyChanged(nameof(IsCurrentWorkspaceItemOpenShortcutContext));
        OnPropertyChanged(nameof(IsCurrentWorkspaceCreateShortcutContext));
        OnPropertyChanged(nameof(IsCurrentWorkspaceEditShortcutContext));
        OnPropertyChanged(nameof(IsCurrentWorkspaceSaveShortcutContext));
    }

    partial void OnSelectedVehicleChanged(VehicleListItemViewModel? value)
    {
        HandleVehicleSelectionChanged();
        OnPropertyChanged(nameof(CanEditSelectedVehicle));
        OnPropertyChanged(nameof(CanDeleteSelectedVehicle));
        OnPropertyChanged(nameof(CanOpenSelectedVehicleCosts));
        OnPropertyChanged(nameof(CanOpenSelectedVehicleServiceBook));
        OnPropertyChanged(nameof(CanExportSelectedVehiclePackage));
        OnPropertyChanged(nameof(CanImportVehiclePackage));
        EditSelectedVehicleCommand.NotifyCanExecuteChanged();
        DeleteSelectedVehicleCommand.NotifyCanExecuteChanged();
        OpenSelectedVehicleCostsCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanOpenVehicleStarterBundle));
        OnPropertyChanged(nameof(CanOpenMaintenanceRecommendations));
        MaintenanceWorkspace.NotifyMaintenanceRecommendationStateChanged();
        OnPropertyChanged(nameof(CanOpenVehicleDetailWindow));
        OnPropertyChanged(nameof(CanOpenHistoryWindow));
        OnPropertyChanged(nameof(CanOpenFuelWindow));
        OnPropertyChanged(nameof(CanOpenReminderWindow));
        OnPropertyChanged(nameof(CanOpenMaintenanceWindow));
        OnPropertyChanged(nameof(CanOpenRecordWindow));
        VehicleDetailWorkspace.NotifyVehicleRelatedWorkspaceStateChanged();
        OnPropertyChanged(nameof(VehicleDetailWindowTitle));
        OnPropertyChanged(nameof(HistoryWindowTitle));
        OnPropertyChanged(nameof(FuelWindowTitle));
        OnPropertyChanged(nameof(ReminderWindowTitle));
        OnPropertyChanged(nameof(MaintenanceWindowTitle));
        OnPropertyChanged(nameof(RecordWindowTitle));
        OnPropertyChanged(nameof(TimelineWindowTitle));

        if (value is null)
        {
            ApplyVehicleDetailProjection(_projectionService.BuildVehicleDetail(_dataSet, null));
            HistoryWorkspace.HistorySummary = LO("HistoryWorkspace.Summary.Initial");
            FuelWorkspace.FuelSummary = LO("FuelWorkspace.Summary.Initial");
            FuelWorkspace.ClearFuelAnalysis();
            ReminderWorkspace.ReminderSummary = LO("ReminderWorkspace.Summary.Initial");
            MaintenanceWorkspace.MaintenanceSummary = LO("MaintenanceWorkspace.Summary.Initial");
            TimelineWorkspace.TimelineSummary = LO("TimelineWorkspace.Summary.Initial");
            RecordWorkspace.RecordSummary = LO("RecordWorkspace.Summary.Initial");
            SelectedVehicleHistory.Clear();
            SelectedVehicleFuel.Clear();
            SelectedVehicleReminders.Clear();
            SelectedVehicleMaintenance.Clear();
            SelectedVehicleTimeline.Clear();
            SelectedVehicleRecords.Clear();
            HistoryWorkspace.RefreshVisibleHistoryItems(preserveSelection: false);
            FuelWorkspace.RefreshVisibleFuelItems(preserveSelection: false);
            ReminderWorkspace.RefreshVisibleReminderItems(preserveSelection: false);
            MaintenanceWorkspace.RefreshVisibleMaintenanceItems(preserveSelection: false);
            RecordWorkspace.RefreshVisibleRecordItems(preserveSelection: false);
            DashboardUpcomingTimeline.Clear();
            SelectedHistory = null;
            SelectedFuel = null;
            SelectedReminder = null;
            SelectedMaintenance = null;
            TimelineWorkspace.SelectedTimelineItem = null;
            AuditWorkspace.SelectedDashboardAuditItem = null;
            CostWorkspace.SelectedDashboardCostVehicle = null;
            DashboardWorkspace.SelectedDashboardTimelineItem = null;
            SelectedRecord = null;
            DashboardWorkspace.DashboardTimelineSummary = LO("Overview.Summary.DashboardInitial");
            DashboardWorkspace.SelectedDashboardTimelineDetail = LO("DashboardTimeline.Detail.Empty");
            SelectedVehicleTabIndex = DetailTabIndex;
            RequestFocus(DesktopFocusTarget.VehicleList);
            return;
        }

        ApplyVehicleDetailProjection(_projectionService.BuildVehicleDetail(
            _dataSet,
            value,
            _metaByVehicleId.GetValueOrDefault(value.Id),
            _dataRoot,
            ResolveManagedAttachmentAbsolutePath,
            DateOnly.FromDateTime(DateTime.Today)));

        PopulateVehicleHistory(value.Id);
        PopulateVehicleFuel(value.Id);
        PopulateVehicleReminders(value.Id);
        PopulateVehicleMaintenance(value.Id);
        PopulateVehicleTimeline(value.Id);
        PopulateVehicleRecords(value.Id);
    }

    private void ApplyVehicleDetailProjection(DesktopVehicleDetailProjection projection)
    {
        VehicleDetailWorkspace.SelectedVehicleHeading = projection.Heading;
        VehicleDetailWorkspace.SelectedVehicleOverview = projection.Overview;
        VehicleDetailWorkspace.SelectedVehicleDates = projection.Dates;
        VehicleDetailWorkspace.SelectedVehicleProfile = projection.Profile;
        VehicleDetailWorkspace.SelectedVehicleEvidenceSummary = projection.EvidenceSummary;
        VehicleDetailWorkspace.SelectedVehicleRecentHistorySummary = projection.RecentHistorySummary;

        VehicleDetailWorkspace.EvidenceSummaryItems.Clear();
        foreach (var item in projection.EvidenceSummaries)
        {
            VehicleDetailWorkspace.EvidenceSummaryItems.Add(item);
        }

        VehicleDetailWorkspace.RecentHistoryItems.Clear();
        foreach (var item in projection.RecentHistory)
        {
            VehicleDetailWorkspace.RecentHistoryItems.Add(item);
        }
    }

    [RelayCommand]
    private void Reload()
    {
        if (BlockDataActionIfEditing(LO("PendingEdits.Action.ReloadData")))
        {
            return;
        }

        Load(SelectedVehicle?.Id, SelectedVehicleTabIndex, applyLaunchTabPreference: false);
        RequestBackgroundRefresh();
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    [RelayCommand]
    private void FocusTimelineSearch()
    {
        if (BlockWorkspaceNavigationIfEditing())
        {
            return;
        }

        SelectedVehicleTabIndex = TimelineTabIndex;
        RequestFocus(DesktopFocusTarget.TimelineSearch);
    }

    [RelayCommand]
    private void FocusGlobalSearch()
    {
        if (BlockWorkspaceNavigationIfEditing())
        {
            return;
        }

        SelectedVehicleTabIndex = SearchTabIndex;
        RequestFocus(DesktopFocusTarget.GlobalSearchBox);
    }

    [RelayCommand]
    private void FocusCurrentSearch()
    {
        if (BlockWorkspaceNavigationIfEditing())
        {
            return;
        }

        var target = SelectedVehicleTabIndex switch
        {
            HistoryTabIndex => DesktopFocusTarget.HistorySearch,
            FuelTabIndex => DesktopFocusTarget.FuelSearch,
            ReminderTabIndex => DesktopFocusTarget.ReminderSearch,
            MaintenanceTabIndex => DesktopFocusTarget.MaintenanceSearch,
            TimelineTabIndex => DesktopFocusTarget.TimelineSearch,
            RecordTabIndex => DesktopFocusTarget.RecordSearch,
            AuditTabIndex => DesktopFocusTarget.AuditSearch,
            CostTabIndex => DesktopFocusTarget.CostSearch,
            SearchTabIndex => DesktopFocusTarget.GlobalSearchBox,
            UpcomingOverviewTabIndex => DesktopFocusTarget.UpcomingOverviewSearch,
            OverdueOverviewTabIndex => DesktopFocusTarget.OverdueOverviewSearch,
            SmartAdvisorTabIndex => DesktopFocusTarget.SmartAdvisorSearch,
            _ => DesktopFocusTarget.VehicleSearch
        };

        RequestFocus(target);
    }

    [RelayCommand]
    private void FocusDashboard()
    {
        if (BlockWorkspaceNavigationIfEditing())
        {
            return;
        }

        SelectedVehicleTabIndex = DashboardTabIndex;
        RequestFocus(DesktopFocusTarget.DashboardAuditList);
    }

    [RelayCommand]
    private void FocusUpcomingOverview()
    {
        if (BlockWorkspaceNavigationIfEditing())
        {
            return;
        }

        SelectedVehicleTabIndex = UpcomingOverviewTabIndex;
        RequestFocus(DesktopFocusTarget.UpcomingOverviewSearch);
    }

    [RelayCommand]
    private void FocusOverdueOverview()
    {
        if (BlockWorkspaceNavigationIfEditing())
        {
            return;
        }

        SelectedVehicleTabIndex = OverdueOverviewTabIndex;
        RequestFocus(DesktopFocusTarget.OverdueOverviewSearch);
    }

    [RelayCommand]
    private void SelectVehicleTab(int tabIndex)
    {
        if (tabIndex < DetailTabIndex || tabIndex > SmartAdvisorTabIndex)
        {
            return;
        }

        if (tabIndex != SelectedVehicleTabIndex && BlockWorkspaceNavigationIfEditing())
        {
            return;
        }

        SelectedVehicleTabIndex = tabIndex;
    }

    [RelayCommand]
    private async Task ExportCalendarAsync()
    {
        if (BlockDataActionIfEditing(LO("PendingEdits.Action.ExportCalendar")))
        {
            return;
        }

        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var export = _calendarExportService.BuildUpcomingCalendar(_dataSet, today, DateTimeOffset.UtcNow);
            if (export.Items.Count == 0)
            {
                SetCalendarExportStatus(LO("AppShell.CalendarExport.Empty"));
                return;
            }

            var suggestedFileName = $"vehimap-kalendar-{today:yyyy-MM-dd}.ics";
            var savedPath = await _fileSaveService
                .SaveTextAsync(LO("AppShell.FileDialog.CalendarExportTitle"), suggestedFileName, export.IcsContent)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(savedPath))
            {
                SetCalendarExportStatus(LO("AppShell.CalendarExport.Cancelled"));
                return;
            }

            SetCalendarExportStatus(export.SkippedMaintenanceCount > 0
                ? LFO("AppShell.CalendarExport.SavedWithSkippedMaintenance", savedPath, export.Items.Count, export.SkippedMaintenanceCount)
                : LFO("AppShell.CalendarExport.Saved", savedPath, export.Items.Count));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SetCalendarExportStatus(LFO("AppShell.CalendarExport.Failed", ex.Message));
        }
    }

    private void SetCalendarExportStatus(string status)
    {
        TimelineWorkspace.ExportStatus = status;
        ShellStatus = status;
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedTimelineItem))]
    private async Task OpenSelectedTimelineItemAsync()
    {
        if (TimelineWorkspace.SelectedTimelineItem is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.OpenSelectedTimelineItem")).ConfigureAwait(true))
        {
            return;
        }

        OpenTimelineItem(TimelineWorkspace.SelectedTimelineItem);
    }

    public async Task<bool> HandleCurrentWorkspacePrimaryOpenShortcutAsync()
    {
        switch (SelectedVehicleTabIndex)
        {
            case RecordTabIndex:
                await ExecuteWorkspaceShortcutAsync(OpenSelectedRecordFileCommand).ConfigureAwait(true);
                return true;
            case AuditTabIndex:
                await OpenAuditVehicleAsync(AuditWorkspace.SelectedDashboardAuditItem).ConfigureAwait(true);
                return true;
            case CostTabIndex:
                await ExecuteWorkspaceShortcutAsync(OpenSelectedDashboardCostVehicleCommand).ConfigureAwait(true);
                return true;
            case DashboardTabIndex:
                await ExecuteWorkspaceShortcutAsync(OpenSelectedDashboardVehicleCommand).ConfigureAwait(true);
                return true;
            case SearchTabIndex:
                await ExecuteWorkspaceShortcutAsync(OpenSelectedSearchResultCommand).ConfigureAwait(true);
                return true;
            case UpcomingOverviewTabIndex:
                await ExecuteWorkspaceShortcutAsync(OpenSelectedUpcomingOverviewVehicleCommand).ConfigureAwait(true);
                return true;
            case OverdueOverviewTabIndex:
                await ExecuteWorkspaceShortcutAsync(OpenSelectedOverdueOverviewVehicleCommand).ConfigureAwait(true);
                return true;
            case SmartAdvisorTabIndex:
                await ExecuteWorkspaceShortcutAsync(SmartAdvisorWorkspace.OpenSelectedSmartAdvisorVehicleCommand).ConfigureAwait(true);
                return true;
            default:
                return false;
        }
    }

    public async Task<bool> HandleCurrentWorkspaceItemOpenShortcutAsync()
    {
        switch (SelectedVehicleTabIndex)
        {
            case TimelineTabIndex:
                await ExecuteWorkspaceShortcutAsync(OpenSelectedTimelineItemCommand).ConfigureAwait(true);
                return true;
            case AuditTabIndex:
                await OpenAuditItemAsync(AuditWorkspace.SelectedDashboardAuditItem).ConfigureAwait(true);
                return true;
            case CostTabIndex:
                ExecuteWorkspaceShortcut(CostWorkspace.FocusSelectedCostDetailCommand);
                return true;
            case DashboardTabIndex:
                await ExecuteWorkspaceShortcutAsync(OpenSelectedDashboardTimelineItemCommand).ConfigureAwait(true);
                return true;
            case SearchTabIndex:
                await ExecuteWorkspaceShortcutAsync(OpenSelectedSearchResultCommand).ConfigureAwait(true);
                return true;
            case UpcomingOverviewTabIndex:
                await ExecuteWorkspaceShortcutAsync(OpenSelectedUpcomingOverviewItemCommand).ConfigureAwait(true);
                return true;
            case OverdueOverviewTabIndex:
                await ExecuteWorkspaceShortcutAsync(OpenSelectedOverdueOverviewItemCommand).ConfigureAwait(true);
                return true;
            case SmartAdvisorTabIndex:
                await ExecuteWorkspaceShortcutAsync(SmartAdvisorWorkspace.OpenSelectedSmartAdvisorItemCommand).ConfigureAwait(true);
                return true;
            default:
                return false;
        }
    }

    public bool HandleCurrentWorkspaceCreateShortcut()
    {
        switch (SelectedVehicleTabIndex)
        {
            case HistoryTabIndex:
                ExecuteWorkspaceShortcut(CreateHistoryCommand);
                return true;
            case FuelTabIndex:
                ExecuteWorkspaceShortcut(CreateFuelCommand);
                return true;
            case ReminderTabIndex:
                ExecuteWorkspaceShortcut(CreateReminderCommand);
                return true;
            case MaintenanceTabIndex:
                ExecuteWorkspaceShortcut(CreateMaintenanceCommand);
                return true;
            case RecordTabIndex:
                ExecuteWorkspaceShortcut(CreateRecordCommand);
                return true;
            default:
                return false;
        }
    }

    public bool HandleCurrentWorkspaceEditShortcut()
    {
        switch (SelectedVehicleTabIndex)
        {
            case HistoryTabIndex:
                ExecuteWorkspaceShortcut(EditSelectedHistoryCommand);
                return true;
            case FuelTabIndex:
                ExecuteWorkspaceShortcut(EditSelectedFuelCommand);
                return true;
            case ReminderTabIndex:
                ExecuteWorkspaceShortcut(EditSelectedReminderCommand);
                return true;
            case MaintenanceTabIndex:
                ExecuteWorkspaceShortcut(EditSelectedMaintenanceCommand);
                return true;
            case RecordTabIndex:
                ExecuteWorkspaceShortcut(EditSelectedRecordCommand);
                return true;
            case AuditTabIndex:
                return false;
            default:
                return false;
        }
    }

    public async Task<bool> HandleCurrentWorkspaceEditShortcutAsync()
    {
        if (SelectedVehicleTabIndex == AuditTabIndex)
        {
            return await EditAuditItemAsync(AuditWorkspace.SelectedDashboardAuditItem).ConfigureAwait(true);
        }

        if (SelectedVehicleTabIndex == CostTabIndex)
        {
            return await EditSelectedCostVehicleFromCostsAsync(CostWorkspace.SelectedDashboardCostVehicle).ConfigureAwait(true);
        }

        if (SelectedVehicleTabIndex == DashboardTabIndex)
        {
            await ExecuteWorkspaceShortcutAsync(EditSelectedDashboardVehicleCommand).ConfigureAwait(true);
            return true;
        }

        return HandleCurrentWorkspaceEditShortcut();
    }

    public async Task<bool> HandleCurrentWorkspaceSaveShortcutAsync()
    {
        switch (SelectedVehicleTabIndex)
        {
            case DetailTabIndex:
                await ExecuteWorkspaceShortcutAsync(SaveVehicleCommand).ConfigureAwait(true);
                return true;
            case HistoryTabIndex:
                await ExecuteWorkspaceShortcutAsync(SaveHistoryCommand).ConfigureAwait(true);
                return true;
            case FuelTabIndex:
                await ExecuteWorkspaceShortcutAsync(SaveFuelCommand).ConfigureAwait(true);
                return true;
            case ReminderTabIndex:
                await ExecuteWorkspaceShortcutAsync(SaveReminderCommand).ConfigureAwait(true);
                return true;
            case MaintenanceTabIndex:
                await ExecuteWorkspaceShortcutAsync(SaveMaintenanceCommand).ConfigureAwait(true);
                return true;
            case RecordTabIndex:
                await ExecuteWorkspaceShortcutAsync(SaveRecordCommand).ConfigureAwait(true);
                return true;
            default:
                return false;
        }
    }

    private static void ExecuteWorkspaceShortcut(IRelayCommand command)
    {
        if (command.CanExecute(null))
        {
            command.Execute(null);
        }
    }

    private static async Task ExecuteWorkspaceShortcutAsync(IAsyncRelayCommand command)
    {
        if (command.CanExecute(null))
        {
            await command.ExecuteAsync(null).ConfigureAwait(true);
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedRecordFile))]
    private async Task OpenSelectedRecordFileAsync()
    {
        if (SelectedRecord is null || string.IsNullOrWhiteSpace(SelectedRecord.ResolvedPath))
        {
            SetRecordAttachmentActionStatus(LO("RecordAttachmentAction.NoPath"));
            return;
        }

        var path = SelectedRecord.ResolvedPath;
        if (!CanOpenSelectedRecordFile)
        {
            SetRecordAttachmentActionStatus(LFO("RecordAttachmentAction.FileUnavailable", path));
            return;
        }

        try
        {
            await _fileLauncher.OpenAsync(path).ConfigureAwait(true);
            SetRecordAttachmentActionStatus(LFO("RecordAttachmentAction.FileOpened", path));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SetRecordAttachmentActionStatus(LFO("RecordAttachmentAction.FileOpenFailed", ex.Message));
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedRecordFolder))]
    private async Task OpenSelectedRecordFolderAsync()
    {
        var folderPath = GetSelectedRecordFolderPath();
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            SetRecordAttachmentActionStatus(LO("RecordAttachmentAction.NoFolder"));
            return;
        }

        try
        {
            await _fileLauncher.OpenFolderAsync(folderPath).ConfigureAwait(true);
            SetRecordAttachmentActionStatus(LFO("RecordAttachmentAction.FolderOpened", folderPath));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SetRecordAttachmentActionStatus(LFO("RecordAttachmentAction.FolderOpenFailed", ex.Message));
        }
    }

    [RelayCommand(CanExecute = nameof(CanCopySelectedRecordPath))]
    private async Task CopySelectedRecordPathAsync()
    {
        var path = GetSelectedRecordCopyPath();
        if (string.IsNullOrWhiteSpace(path))
        {
            SetRecordAttachmentActionStatus(LO("RecordAttachmentAction.NoCopyPath"));
            return;
        }

        try
        {
            await _clipboardService.SetTextAsync(path).ConfigureAwait(true);
            SetRecordAttachmentActionStatus(LO("RecordAttachmentAction.PathCopied"));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SetRecordAttachmentActionStatus(LFO("RecordAttachmentAction.CopyPathFailed", ex.Message));
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedDashboardAuditItem))]
    private async Task OpenSelectedDashboardAuditItemAsync()
    {
        await OpenAuditItemAsync(AuditWorkspace.SelectedDashboardAuditItem).ConfigureAwait(true);
    }

    internal async Task<bool> OpenAuditItemAsync(AuditItemViewModel? item)
    {
        if (item is null)
        {
            return false;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.OpenAuditItem")).ConfigureAwait(true))
        {
            return false;
        }

        SelectVehicleAndOpenEntity(item.VehicleId, item.EntityKind, item.EntityId);
        return true;
    }

    internal async Task<bool> OpenAuditVehicleAsync(AuditItemViewModel? item)
    {
        if (item is null)
        {
            return false;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.OpenAuditVehicle")).ConfigureAwait(true))
        {
            return false;
        }

        SelectVehicleAndOpenEntity(item.VehicleId, "Vozidlo", item.VehicleId);
        return true;
    }

    internal async Task<bool> EditAuditItemAsync(AuditItemViewModel? item)
    {
        if (item is null)
        {
            return false;
        }

        if (IsVehicleAuditTarget(item))
        {
            if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.EditAuditVehicle")).ConfigureAwait(true))
            {
                return false;
            }

            if (!SelectVehicleById(item.VehicleId))
            {
                return false;
            }

            SetNextVehicleEditorReturnFocusTarget(DesktopFocusTarget.AuditList);
            ExecuteWorkspaceShortcut(EditSelectedVehicleCommand);
            return true;
        }

        if (!await OpenAuditItemAsync(item).ConfigureAwait(true))
        {
            return false;
        }

        SetNextWorkspaceEditorReturnFocusTarget(DesktopFocusTarget.AuditList);
        StartEditForCurrentAuditTarget(item?.EntityKind ?? string.Empty);
        return true;
    }

    internal async Task<bool> EditSelectedCostVehicleFromCostsAsync(CostVehicleItemViewModel? item)
    {
        if (item is null)
        {
            return false;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.EditCostVehicle")).ConfigureAwait(true))
        {
            return false;
        }

        if (!SelectVehicleById(item.VehicleId))
        {
            return false;
        }

        SetNextVehicleEditorReturnFocusTarget(DesktopFocusTarget.CostList);
        ExecuteWorkspaceShortcut(EditSelectedVehicleCommand);
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedDashboardVehicle))]
    private async Task OpenSelectedDashboardVehicleAsync()
    {
        var vehicleId = GetSelectedDashboardVehicleId();
        if (vehicleId is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.OpenDashboardVehicle")).ConfigureAwait(true))
        {
            return;
        }

        SelectVehicleAndOpenEntity(vehicleId, "Vozidlo", vehicleId);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelectedDashboardVehicle))]
    private async Task EditSelectedDashboardVehicleAsync()
    {
        var vehicleId = GetSelectedDashboardVehicleId();
        if (vehicleId is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.EditDashboardVehicle")).ConfigureAwait(true))
        {
            return;
        }

        if (!SelectVehicleById(vehicleId))
        {
            return;
        }

        SetNextVehicleEditorReturnFocusTarget(GetDashboardVehicleReturnFocusTarget(vehicleId));
        ExecuteWorkspaceShortcut(EditSelectedVehicleCommand);
    }

    [RelayCommand(CanExecute = nameof(CanOpenDashboardCostOverview))]
    private async Task OpenDashboardCostOverviewAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.OpenDashboardCostOverview")).ConfigureAwait(true))
        {
            return;
        }

        SelectedVehicleTabIndex = CostTabIndex;
        RequestFocus(CostWorkspace.VisibleCostVehicles.Count == 0 ? DesktopFocusTarget.CostSearch : DesktopFocusTarget.CostList);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedDashboardVehicleHistory))]
    private async Task OpenSelectedDashboardVehicleHistoryAsync()
    {
        var vehicleId = GetSelectedDashboardVehicleId();
        if (vehicleId is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.OpenDashboardVehicleHistory")).ConfigureAwait(true))
        {
            return;
        }

        SelectVehicleAndOpenEntity(vehicleId, "Historie", string.Empty);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedDashboardVehicleCosts))]
    private async Task OpenSelectedDashboardVehicleCostsAsync()
    {
        var vehicleId = GetSelectedDashboardVehicleId();
        if (vehicleId is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.OpenDashboardVehicleCosts")).ConfigureAwait(true))
        {
            return;
        }

        if (!string.Equals(SelectedVehicle?.Id, vehicleId, StringComparison.Ordinal))
        {
            SelectedVehicle = Vehicles.FirstOrDefault(vehicle => string.Equals(vehicle.Id, vehicleId, StringComparison.Ordinal));
        }

        CostWorkspace.CostSearchText = string.Empty;
        CostWorkspace.SelectedDashboardCostVehicle = CostVehicles.FirstOrDefault(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        SelectedVehicleTabIndex = CostTabIndex;
        RequestFocus(CostWorkspace.SelectedDashboardCostVehicle is null ? DesktopFocusTarget.CostSearch : DesktopFocusTarget.CostList);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedDashboardCostVehicle))]
    private async Task OpenSelectedDashboardCostVehicleAsync()
    {
        var selectedCostVehicle = CostWorkspace.SelectedDashboardCostVehicle;
        if (selectedCostVehicle is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.OpenCostVehicle")).ConfigureAwait(true))
        {
            return;
        }

        SelectVehicleAndOpenEntity(selectedCostVehicle.VehicleId, "Vozidlo", selectedCostVehicle.VehicleId);
    }

    internal async Task<bool> SelectDashboardMaintenanceForCompletionAsync()
    {
        var selectedTimelineItem = DashboardWorkspace.SelectedDashboardTimelineItem;
        if (selectedTimelineItem is not { Kind: "maintenance", EntryId.Length: > 0 })
        {
            return false;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.CompleteDashboardMaintenance")).ConfigureAwait(true))
        {
            return false;
        }

        OpenTimelineItem(selectedTimelineItem);
        return SelectedMaintenance is not null
            && string.Equals(SelectedMaintenance.Id, selectedTimelineItem.EntryId, StringComparison.Ordinal)
            && CanCompleteSelectedMaintenance;
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedVehicleCosts))]
    private async Task OpenSelectedVehicleCostsAsync()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.OpenSelectedVehicleCosts")).ConfigureAwait(true))
        {
            return;
        }

        CostWorkspace.SelectedDashboardCostVehicle = FindById(CostVehicles, item => item.VehicleId, SelectedVehicle.Id);
        SelectedVehicleTabIndex = CostTabIndex;
        RequestFocus(DesktopFocusTarget.CostList);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedDashboardTimelineItem))]
    private async Task OpenSelectedDashboardTimelineItemAsync()
    {
        var selectedTimelineItem = DashboardWorkspace.SelectedDashboardTimelineItem;
        if (selectedTimelineItem is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.OpenDashboardTimelineItem")).ConfigureAwait(true))
        {
            return;
        }

        OpenTimelineItem(selectedTimelineItem);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedSearchResult))]
    private async Task OpenSelectedSearchResultAsync()
    {
        if (GlobalSearchWorkspace.SelectedSearchResult is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync(LO("PendingEdits.Action.OpenSearchResult")).ConfigureAwait(true))
        {
            return;
        }

        SelectVehicleAndOpenEntity(
            GlobalSearchWorkspace.SelectedSearchResult.VehicleId,
            GlobalSearchWorkspace.SelectedSearchResult.EntityKind,
            GlobalSearchWorkspace.SelectedSearchResult.EntityId);
    }

    private void Load(string? preferredVehicleId = null, int? preferredTabIndex = null, bool applyLaunchTabPreference = false)
    {
        try
        {
            _session.LoadAsync(AppContext.BaseDirectory).GetAwaiter().GetResult();
            RefreshShellFromSessionState(preferredVehicleId, preferredTabIndex, applyLaunchTabPreference);
            if (_session.LastMigrationResult is { } migrationResult
                && (migrationResult.Migrated || !string.IsNullOrWhiteSpace(migrationResult.PreMigrationBackupPath)))
            {
                ShellStatus = migrationResult.Message;
            }

            ApplyDataStoreHealthStatusToShell();
        }
        catch (Exception ex)
        {
            LoadError = BuildDataStoreLoadError(ex);
        }
    }

    private static string BuildDataStoreLoadError(Exception exception)
    {
        try
        {
            var dataRoot = new LegacyDataRootLocator(AssemblyAppBuildInfoProvider.ResolveCurrentApplicationDataFolderName())
                .Resolve(AppContext.BaseDirectory);
            var databasePath = Path.Combine(dataRoot.DataPath, "vehimap.db");
            return string.Join(
                Environment.NewLine,
                [
                    exception.Message,
                    string.Empty,
                    LFO("Shell.LoadError.DatabasePath", databasePath),
                    LFO("Shell.LoadError.DataFolder", dataRoot.DataPath),
                    LO("Shell.LoadError.CorruptDatabaseAdvice")
                ]);
        }
        catch
        {
            return exception.Message;
        }
    }

    private void ApplyDataStoreHealthStatusToShell()
    {
        if (_session.LastDataStoreHealthReport is not { HasWarningsOrErrors: true } report)
        {
            return;
        }

        var message = BuildDataStoreHealthShellMessage(report, manual: false);
        ShellStatus = string.IsNullOrWhiteSpace(ShellStatus) || string.Equals(ShellStatus, LO("Shell.Status.Ready"), StringComparison.Ordinal)
            ? message
            : $"{ShellStatus} {message}";
    }

    private void RefreshShellFromSessionState(string? preferredVehicleId = null, int? preferredTabIndex = null, bool applyLaunchTabPreference = false)
    {
        if (_dataRoot is null)
        {
            return;
        }

        var supportedSettings = _session.ReadSupportedSettings();
        var appInfo = _session.GetAppInfo();
        _projectionService.ApplySupportedSettings(supportedSettings);
        _costExportService.ApplySupportedSettings(supportedSettings);
        _serviceBookExportService.ApplySupportedSettings(supportedSettings);
        if (_timelineService is LegacyTimelineService legacyTimelineService)
        {
            legacyTimelineService.ApplySupportedSettings(supportedSettings);
        }

        if (_globalSearchService is LegacyGlobalSearchService legacyGlobalSearchService)
        {
            legacyGlobalSearchService.ApplySupportedSettings(supportedSettings);
        }

        if (_fuelAnalysisService is LegacyFuelAnalysisService legacyFuelAnalysisService)
        {
            legacyFuelAnalysisService.ApplySupportedSettings(supportedSettings);
        }

        if (_serviceBookService is LegacyServiceBookService legacyServiceBookService)
        {
            legacyServiceBookService.ApplySupportedSettings(supportedSettings);
        }

        ApplyCostPeriodPreferences();
        var costSummary = BuildSelectedCostSummary();
        _currentCostSummary = costSummary;

        LoadError = string.Empty;
        Title = appInfo.ApplicationName;
        Subtitle = appInfo.ReleaseChannel == ReleaseChannelService.Stable
            ? LO("Shell.Subtitle.Stable")
            : LFO("Shell.Subtitle.Channel", appInfo.ReleaseChannel);
        DataMode = _dataRoot.IsPortable
            ? LO("Shell.DataMode.Portable")
            : LO("Shell.DataMode.System");
        DataPath = _dataRoot.DataPath;
        OnPropertyChanged(nameof(CanUseDataActions));
        OnPropertyChanged(nameof(CanOpenDataFolder));
        OnPropertyChanged(nameof(CanCheckDataStoreHealth));
        OnPropertyChanged(nameof(CanCreateAutomaticBackupNow));
        OnPropertyChanged(nameof(CanOpenAutomaticBackupFolder));
        OnPropertyChanged(nameof(CanOpenPreMigrationBackupFolder));
        OnPropertyChanged(nameof(CanExportSelectedVehiclePackage));
        OnPropertyChanged(nameof(CanImportVehiclePackage));
        DashboardWorkspace.SyncShowDashboardOnLaunch(supportedSettings.ShowDashboardOnLaunch);
        VehicleCount = _dataSet.Vehicles.Count;
        HistoryCount = _dataSet.HistoryEntries.Count;
        FuelCount = _dataSet.FuelEntries.Count;
        RecordsCount = _dataSet.Records.Count;
        RemindersCount = _dataSet.Reminders.Count;
        MaintenanceCount = _dataSet.MaintenancePlans.Count;
        AuditCount = _auditItems.Count;
        AuditWorkspace.SetAuditSummary(_projectionService.BuildAuditSummary(_auditItems));
        CostWorkspace.CostSummary = _projectionService.BuildCostSummary(costSummary);
        CostWorkspace.CostComparison = _projectionService.BuildCostComparison(costSummary);
        DashboardWorkspace.NotifyDashboardSummariesChanged();

        ApplyVehicleListFilterPreferences();
        ApplyOverviewPreferences();
        ApplyTimelinePreferences();
        ApplyEvidenceSortPreferences();
        ApplyWorkspaceSortPreferences();
        RefreshVehicleList(preferredVehicleId);

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

        CostVehicles.Clear();
        foreach (var row in _projectionService.BuildDashboardCostVehicles(costSummary))
        {
            CostVehicles.Add(row);
        }

        PopulateDashboardTimeline();
        RefreshFleetOverviews();
        RefreshGlobalSearch();
        RefreshSmartAdvisorProjection(preserveSelection: false);
        NotifyQuickActionAvailabilityChanged();
        AuditWorkspace.RefreshVisibleAuditItems(preserveSelection: false);
        CostWorkspace.RefreshVisibleCostVehicles(preserveSelection: false);
        DashboardWorkspace.SelectedDashboardTimelineItem = DashboardUpcomingTimeline.FirstOrDefault();
        ExportFleetCostSummaryCommand.NotifyCanExecuteChanged();
        ExportSelectedVehicleCostDetailCommand.NotifyCanExecuteChanged();
        ExportSelectedVehicleCostReportCommand.NotifyCanExecuteChanged();
        OpenDashboardCostOverviewCommand.NotifyCanExecuteChanged();

        if (applyLaunchTabPreference && supportedSettings.ShowDashboardOnLaunch && !supportedSettings.HideOnLaunch)
        {
            SelectedVehicleTabIndex = DashboardTabIndex;
        }
        else if (preferredTabIndex.HasValue && preferredTabIndex.Value >= DetailTabIndex && preferredTabIndex.Value <= SmartAdvisorTabIndex)
        {
            SelectedVehicleTabIndex = preferredTabIndex.Value;
        }
    }

    private void PopulateVehicleHistory(string vehicleId)
    {
        SelectedVehicleHistory.Clear();
        var projection = _projectionService.BuildHistory(_dataSet, vehicleId);
        foreach (var item in projection.Items)
        {
            SelectedVehicleHistory.Add(item);
        }

        HistoryWorkspace.HistorySummary = projection.Summary;
        HistoryWorkspace.RefreshVisibleHistoryItems(preserveSelection: false);
    }

    private void PopulateVehicleFuel(string vehicleId)
    {
        SelectedVehicleFuel.Clear();
        var projection = _projectionService.BuildFuel(_dataSet, vehicleId);
        foreach (var item in projection.Items)
        {
            SelectedVehicleFuel.Add(item);
        }

        FuelWorkspace.FuelSummary = projection.Summary;
        FuelWorkspace.ApplyFuelAnalysis(_projectionService.BuildFuelAnalysis(
            _fuelAnalysisService.BuildVehicleFuelAnalysis(_dataSet, vehicleId)));
        FuelWorkspace.RefreshVisibleFuelItems(preserveSelection: false);
    }

    private void PopulateVehicleReminders(string vehicleId)
    {
        SelectedVehicleReminders.Clear();
        var projection = _projectionService.BuildReminders(_dataSet, vehicleId, DateOnly.FromDateTime(DateTime.Today));
        foreach (var item in projection.Items)
        {
            SelectedVehicleReminders.Add(item);
        }

        ReminderWorkspace.ReminderSummary = projection.Summary;
        ReminderWorkspace.RefreshVisibleReminderItems(preserveSelection: false);
    }

    private void PopulateVehicleMaintenance(string vehicleId)
    {
        SelectedVehicleMaintenance.Clear();
        var projection = _projectionService.BuildMaintenance(_dataSet, vehicleId, DateOnly.FromDateTime(DateTime.Today));
        foreach (var item in projection.Items)
        {
            SelectedVehicleMaintenance.Add(item);
        }

        MaintenanceWorkspace.MaintenanceSummary = projection.Summary;
        MaintenanceWorkspace.RefreshVisibleMaintenanceItems(preserveSelection: false);
    }

    private void PopulateVehicleTimeline(string vehicleId)
    {
        SelectedVehicleTimeline.Clear();
        var projection = _projectionService.BuildTimeline(
            _dataSet,
            _timelineService,
            vehicleId,
            DateOnly.FromDateTime(DateTime.Today),
            TimelineWorkspace.SelectedTimelineFilter,
            TimelineWorkspace.TimelineSearchText);
        foreach (var item in projection.Items)
        {
            SelectedVehicleTimeline.Add(item);
        }

        TimelineWorkspace.TimelineSummary = projection.Summary;
        TimelineWorkspace.SelectedTimelineItem = SelectedVehicleTimeline.FirstOrDefault();
        if (TimelineWorkspace.SelectedTimelineItem is null)
        {
            TimelineWorkspace.SelectedTimelineDetail = LO("TimelineWorkspace.Detail.Empty");
            NotifyTimelineWorkspaceSelectionChanged();
        }
    }

    private void PopulateVehicleRecords(string vehicleId)
    {
        SelectedVehicleRecords.Clear();
        var projection = _projectionService.BuildRecords(_dataRoot, _dataSet, vehicleId, ResolveManagedAttachmentAbsolutePath);
        foreach (var item in projection.Items)
        {
            SelectedVehicleRecords.Add(item);
        }

        RecordWorkspace.RecordSummary = projection.Summary;
        RecordWorkspace.RefreshVisibleRecordItems(preserveSelection: false);
    }

    private void RefreshTimeline()
    {
        if (SelectedVehicle is null)
        {
            TimelineWorkspace.TimelineSummary = LO("TimelineWorkspace.Summary.Initial");
            TimelineWorkspace.SelectedTimelineItem = null;
            return;
        }

        var previousSelection = TimelineWorkspace.SelectedTimelineItem;
        var projection = _projectionService.BuildTimeline(
            _dataSet,
            _timelineService,
            SelectedVehicle.Id,
            DateOnly.FromDateTime(DateTime.Today),
            TimelineWorkspace.SelectedTimelineFilter,
            TimelineWorkspace.TimelineSearchText);
        SelectedVehicleTimeline.Clear();
        foreach (var item in projection.Items)
        {
            SelectedVehicleTimeline.Add(item);
        }

        TimelineWorkspace.TimelineSummary = projection.Summary;
        TimelineWorkspace.SelectedTimelineItem = previousSelection is null
            ? SelectedVehicleTimeline.FirstOrDefault()
            : FindTimelineItem(SelectedVehicleTimeline, previousSelection);
        if (TimelineWorkspace.SelectedTimelineItem is null)
        {
            TimelineWorkspace.SelectedTimelineDetail = LO("TimelineWorkspace.Detail.Empty");
            NotifyTimelineWorkspaceSelectionChanged();
        }
    }

    private void RefreshGlobalSearch()
    {
        var previousSelection = GlobalSearchWorkspace.SelectedSearchResult;
        GlobalSearchResults.Clear();

        if (_dataRoot is null || string.IsNullOrWhiteSpace(GlobalSearchWorkspace.GlobalSearchText))
        {
            GlobalSearchWorkspace.GlobalSearchSummary = DesktopLocalization.Localizer.GetString("GlobalSearch.Summary.EmptyQuery");
            GlobalSearchWorkspace.SelectedSearchResult = null;
            return;
        }

        var results = _globalSearchService.Search(_dataRoot, _dataSet, GlobalSearchWorkspace.GlobalSearchText);
        var projectedResults = results
            .Select(result => new GlobalSearchResultItemViewModel(
                result.VehicleId,
                result.EntityKind,
                result.EntityId,
                result.VehicleName,
                result.SectionLabel,
                result.Title,
                result.Summary,
                DesktopLocalization.Localizer.GetString("GlobalSearch.Accessible.VehicleLabel")))
            .ToList();

        foreach (var result in WorkspaceSortHelpers.SortGlobalSearch(
                     projectedResults,
                     GlobalSearchWorkspace.SelectedGlobalSearchSortOption,
                     GlobalSearchWorkspace.GlobalSearchSortDescending))
        {
            GlobalSearchResults.Add(result);
        }

        GlobalSearchWorkspace.GlobalSearchSummary = results.Count == 0
            ? DesktopLocalization.Localizer.Format("GlobalSearch.Summary.NoResults", GlobalSearchWorkspace.GlobalSearchText.Trim())
            : DesktopLocalization.Localizer.Format("GlobalSearch.Summary.WithResults", GlobalSearchWorkspace.GlobalSearchText.Trim(), results.Count);

        var previousKey = previousSelection is null
            ? string.Empty
            : $"{previousSelection.EntityKind}|{previousSelection.EntityId}|{previousSelection.VehicleId}";
        GlobalSearchWorkspace.SelectedSearchResult = FindById(GlobalSearchResults, item => $"{item.EntityKind}|{item.EntityId}|{item.VehicleId}", previousKey);
        if (GlobalSearchWorkspace.SelectedSearchResult is null)
        {
            GlobalSearchWorkspace.SelectedSearchResultDetail = DesktopLocalization.Localizer.GetString("GlobalSearch.Detail.EmptySelection");
            NotifyGlobalSearchWorkspaceSelectionChanged();
        }
    }

    private void PopulateDashboardTimeline()
    {
        DashboardUpcomingTimeline.Clear();
        var projection = _projectionService.BuildDashboardTimeline(_dataSet, _timelineService, DateOnly.FromDateTime(DateTime.Today));
        foreach (var item in projection.Items)
        {
            DashboardUpcomingTimeline.Add(item);
        }

        DashboardWorkspace.DashboardTimelineSummary = projection.Summary;
    }

    private string? GetSelectedRecordFolderPath()
    {
        return _projectionService.GetRecordFolderPath(SelectedRecord);
    }

    private string? GetSelectedRecordCopyPath()
    {
        return string.IsNullOrWhiteSpace(SelectedRecord?.ResolvedPath)
            ? null
            : SelectedRecord.ResolvedPath;
    }

    private void SetRecordAttachmentActionStatus(string status)
    {
        RecordEditorStatus = status;
        ShellStatus = status;
        RequestFocus(DesktopFocusTarget.RecordList);
    }

    private static TItem? FindById<TItem>(IEnumerable<TItem> items, Func<TItem, string> idSelector, string entryId)
    {
        if (string.IsNullOrWhiteSpace(entryId))
        {
            return items.FirstOrDefault();
        }

        return items.FirstOrDefault(item => string.Equals(idSelector(item), entryId, StringComparison.Ordinal))
            ?? items.FirstOrDefault();
    }

    private static VehicleTimelineItemViewModel? FindTimelineItem(IEnumerable<VehicleTimelineItemViewModel> items, VehicleTimelineItemViewModel target)
    {
        if (!string.IsNullOrWhiteSpace(target.EntryId))
        {
            return items.FirstOrDefault(item => string.Equals(item.EntryId, target.EntryId, StringComparison.Ordinal))
                ?? items.FirstOrDefault();
        }

        return items.FirstOrDefault(item =>
                   string.Equals(item.Kind, target.Kind, StringComparison.Ordinal)
                   && string.Equals(item.Date, target.Date, StringComparison.Ordinal)
                   && string.Equals(item.Title, target.Title, StringComparison.Ordinal)
                   && string.Equals(item.VehicleId, target.VehicleId, StringComparison.Ordinal))
               ?? items.FirstOrDefault();
    }

    private string? GetSelectedDashboardVehicleId()
    {
        if (!string.IsNullOrWhiteSpace(CostWorkspace.SelectedDashboardCostVehicle?.VehicleId))
        {
            return CostWorkspace.SelectedDashboardCostVehicle.VehicleId;
        }

        if (!string.IsNullOrWhiteSpace(DashboardWorkspace.SelectedDashboardTimelineItem?.VehicleId))
        {
            return DashboardWorkspace.SelectedDashboardTimelineItem.VehicleId;
        }

        if (!string.IsNullOrWhiteSpace(AuditWorkspace.SelectedDashboardAuditItem?.VehicleId))
        {
            return AuditWorkspace.SelectedDashboardAuditItem.VehicleId;
        }

        return null;
    }

    private void OpenTimelineItem(VehicleTimelineItemViewModel item)
    {
        var plan = _navigationCoordinator.BuildForTimeline(item);
        ApplyNavigationPlan(plan, item);
    }

    private void SelectVehicleAndOpenEntity(string vehicleId, string entityKind, string entityId)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
        {
            return;
        }

        var plan = _navigationCoordinator.BuildForEntity(vehicleId, entityKind, entityId);
        ApplyNavigationPlan(plan);
    }

    private bool SelectVehicleById(string vehicleId)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
        {
            return false;
        }

        var vehicle = Vehicles.FirstOrDefault(item => string.Equals(item.Id, vehicleId, StringComparison.Ordinal));
        if (vehicle is null)
        {
            return false;
        }

        SelectedVehicle = vehicle;
        return true;
    }

    private DesktopFocusTarget GetDashboardVehicleReturnFocusTarget(string vehicleId)
    {
        if (string.Equals(CostWorkspace.SelectedDashboardCostVehicle?.VehicleId, vehicleId, StringComparison.Ordinal))
        {
            return DesktopFocusTarget.DashboardCostList;
        }

        if (string.Equals(DashboardWorkspace.SelectedDashboardTimelineItem?.VehicleId, vehicleId, StringComparison.Ordinal))
        {
            return DesktopFocusTarget.DashboardTimelineList;
        }

        return DesktopFocusTarget.DashboardAuditList;
    }

    private static bool IsVehicleAuditTarget(AuditItemViewModel item)
    {
        return string.IsNullOrWhiteSpace(item.EntityKind)
            || string.Equals(item.EntityKind, "Vozidlo", StringComparison.Ordinal)
            || string.Equals(item.EntityId, item.VehicleId, StringComparison.Ordinal);
    }

    private void ApplyNavigationPlan(DesktopNavigationPlan plan, VehicleTimelineItemViewModel? timelineSelection = null)
    {
        if (!string.IsNullOrWhiteSpace(plan.VehicleId)
            && !string.Equals(SelectedVehicle?.Id, plan.VehicleId, StringComparison.Ordinal))
        {
            SelectedVehicle = Vehicles.FirstOrDefault(vehicle => string.Equals(vehicle.Id, plan.VehicleId, StringComparison.Ordinal));
        }

        if (timelineSelection is not null)
        {
            TimelineWorkspace.SelectedTimelineItem = FindTimelineItem(SelectedVehicleTimeline, timelineSelection);
        }

        SelectedVehicleTabIndex = plan.TabIndex;
        switch (plan.SelectionKind)
        {
            case DesktopNavigationSelectionKind.History:
                SelectedHistory = FindById(SelectedVehicleHistory, item => item.Id, plan.EntityId ?? string.Empty);
                break;
            case DesktopNavigationSelectionKind.Fuel:
                SelectedFuel = FindById(SelectedVehicleFuel, item => item.Id, plan.EntityId ?? string.Empty);
                break;
            case DesktopNavigationSelectionKind.Reminder:
                SelectedReminder = FindById(SelectedVehicleReminders, item => item.Id, plan.EntityId ?? string.Empty);
                break;
            case DesktopNavigationSelectionKind.Maintenance:
                SelectedMaintenance = FindById(SelectedVehicleMaintenance, item => item.Id, plan.EntityId ?? string.Empty);
                break;
            case DesktopNavigationSelectionKind.Record:
                SelectedRecord = FindById(SelectedVehicleRecords, item => item.Id, plan.EntityId ?? string.Empty);
                break;
        }

        RequestFocus(plan.FocusTarget);
    }

    private void StartEditForCurrentAuditTarget(string entityKind)
    {
        switch (entityKind)
        {
            case "Historie":
                ExecuteWorkspaceShortcut(EditSelectedHistoryCommand);
                break;
            case "Tankování":
                ExecuteWorkspaceShortcut(EditSelectedFuelCommand);
                break;
            case "Doklad":
                ExecuteWorkspaceShortcut(EditSelectedRecordCommand);
                break;
            case "Údržba":
                ExecuteWorkspaceShortcut(EditSelectedMaintenanceCommand);
                break;
            case "Připomínka":
                ExecuteWorkspaceShortcut(EditSelectedReminderCommand);
                break;
            default:
                ExecuteWorkspaceShortcut(EditSelectedVehicleCommand);
                break;
        }
    }

    private void RequestFocus(DesktopFocusTarget target)
    {
        FocusRequested?.Invoke(target);
    }

    private void RequestBackgroundRefresh()
    {
        BackgroundRefreshRequested?.Invoke();
    }

    private static string FormatValue(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;

    private async Task<bool> ConfirmDiscardPendingEditsBeforeNavigationAsync(string actionDescription)
    {
        if (!HasPendingEdits)
        {
            return true;
        }

        if (!await ConfirmDiscardPendingEditsAsync(actionDescription).ConfigureAwait(true))
        {
            RequestFocus(GetPendingEditFocusTarget());
            return false;
        }

        DiscardPendingEdits();
        return true;
    }
}
