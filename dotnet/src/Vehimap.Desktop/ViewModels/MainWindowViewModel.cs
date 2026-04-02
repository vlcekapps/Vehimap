using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Vehimap.Storage.Legacy;

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

    private readonly DesktopSessionController _session;
    private readonly IFileLauncher _fileLauncher;
    private readonly IFilePickerService _filePickerService;
    private readonly IGlobalSearchService _globalSearchService;
    private readonly ITimelineService _timelineService;
    private readonly ICalendarExportService _calendarExportService;
    private readonly ITextFileSaveService _fileSaveService;
    private readonly IFileDialogService _fileDialogService;
    private readonly DesktopProjectionService _projectionService;
    private readonly DesktopNavigationCoordinator _navigationCoordinator;
    private VehimapDataRoot? _dataRoot => _session.DataRoot;
    private VehimapDataSet _dataSet => _session.DataSet;
    private IReadOnlyList<AuditItem> _auditItems => _session.AuditItems;
    private IReadOnlyDictionary<string, VehicleMeta> _metaByVehicleId => _session.MetaByVehicleId;

    public event Action<DesktopFocusTarget>? FocusRequested;

    [ObservableProperty]
    private string title = "Vehimap Desktop Preview";

    [ObservableProperty]
    private string subtitle = "První Avalonia shell nad legacy daty Vehimap.";

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
    private string selectedVehicleHeading = "Nevybrané vozidlo";

    [ObservableProperty]
    private string selectedVehicleOverview = "Vyberte vozidlo vlevo a zobrazí se jeho základní souhrn.";

    [ObservableProperty]
    private string selectedVehicleDates = string.Empty;

    [ObservableProperty]
    private string selectedVehicleProfile = string.Empty;

    [ObservableProperty]
    private string historySummary = "Historie vybraného vozidla se zobrazí po výběru vozidla.";

    [ObservableProperty]
    private string fuelSummary = "Tankování vybraného vozidla se zobrazí po výběru vozidla.";

    [ObservableProperty]
    private string reminderSummary = "Připomínky vybraného vozidla se zobrazí po výběru vozidla.";

    [ObservableProperty]
    private string maintenanceSummary = "Plán údržby vybraného vozidla se zobrazí po výběru vozidla.";

    [ObservableProperty]
    private string recordSummary = "Doklady a přílohy vybraného vozidla se zobrazí po výběru vozidla.";

    [ObservableProperty]
    private string shellStatus = "Desktopová větev je připravená.";

    [ObservableProperty]
    private int selectedVehicleTabIndex;

    [ObservableProperty]
    private VehicleListItemViewModel? selectedVehicle;

    public ObservableCollection<VehicleListItemViewModel> Vehicles { get; } = [];

    public ObservableCollection<AuditItemViewModel> AuditItems { get; } = [];

    public ObservableCollection<CostVehicleItemViewModel> CostVehicles { get; } = [];

    public ObservableCollection<VehicleTimelineItemViewModel> DashboardUpcomingTimeline { get; } = [];

    public ObservableCollection<VehicleHistoryItemViewModel> SelectedVehicleHistory { get; } = [];

    public ObservableCollection<VehicleFuelItemViewModel> SelectedVehicleFuel { get; } = [];

    public ObservableCollection<VehicleReminderItemViewModel> SelectedVehicleReminders { get; } = [];

    public ObservableCollection<VehicleMaintenanceItemViewModel> SelectedVehicleMaintenance { get; } = [];

    public ObservableCollection<VehicleTimelineItemViewModel> SelectedVehicleTimeline { get; } = [];

    public ObservableCollection<VehicleRecordItemViewModel> SelectedVehicleRecords { get; } = [];

    public ObservableCollection<GlobalSearchResultItemViewModel> GlobalSearchResults { get; } = [];

    internal DesktopAppShellController AppShellController { get; }

    public bool CanOpenReminderWindow => SelectedVehicle is not null;

    public bool CanOpenRecordWindow => SelectedVehicle is not null;

    public bool CanOpenHistoryWindow => SelectedVehicle is not null;

    public bool CanOpenFuelWindow => SelectedVehicle is not null;

    public bool CanOpenMaintenanceWindow => SelectedVehicle is not null;

    public bool CanOpenVehicleDetailWindow => SelectedVehicle is not null;

    public string HistoryWindowTitle =>
        SelectedVehicle is null
            ? "Historie vozidla"
            : $"Historie - {SelectedVehicle.Name}";

    public string FuelWindowTitle =>
        SelectedVehicle is null
            ? "Tankování vozidla"
            : $"Tankování - {SelectedVehicle.Name}";

    public string ReminderWindowTitle =>
        SelectedVehicle is null
            ? "Připomínky vozidla"
            : $"Připomínky - {SelectedVehicle.Name}";

    public string MaintenanceWindowTitle =>
        SelectedVehicle is null
            ? "Plán údržby vozidla"
            : $"Údržba - {SelectedVehicle.Name}";

    public string RecordWindowTitle =>
        SelectedVehicle is null
            ? "Doklady a přílohy"
            : $"Doklady a přílohy - {SelectedVehicle.Name}";

    public string VehicleDetailWindowTitle =>
        SelectedVehicle is null
            ? "Detail vozidla"
            : $"Detail - {SelectedVehicle.Name}";

    public string AuditWindowTitle => "Audit dat";

    public string DashboardWindowTitle => "Dashboard";

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

    public MainWindowViewModel()
        : this(
            new LegacyVehimapDataStore(),
            new LegacyVehimapBootstrapper(new LegacyDataRootLocator(), new LegacyVehimapDataStore()),
            new ManagedAttachmentPathService(),
            new ProcessFileLauncher(),
            new AvaloniaFilePickerService(),
            new LegacyGlobalSearchService(new ManagedAttachmentPathService()),
            new LegacyTimelineService(),
            new LegacyCalendarExportService(),
            new AvaloniaTextFileSaveService(),
            new LegacyBackupService(),
            new AvaloniaFileDialogService(),
            new DesktopSupportedSettingsService(),
            new AssemblyAppBuildInfoProvider(),
            new PlatformAutostartService(),
            null,
            new DesktopProjectionService(),
            new DesktopNavigationCoordinator(),
            new AvaloniaAppShellDialogService(),
            new ProcessUpdateInstallLauncher())
    {
    }

    internal MainWindowViewModel(
        ILegacyDataStore legacyDataStore,
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
        IAppShellDialogService? appShellDialogService = null,
        IUpdateInstallLauncher? updateInstallLauncher = null)
    {
        var sessionBackupService = backupService ?? new LegacyBackupService();
        var sessionSupportedSettingsService = supportedSettingsService ?? new DesktopSupportedSettingsService();
        var sessionAppBuildInfoProvider = appBuildInfoProvider ?? new AssemblyAppBuildInfoProvider();
        var sessionAutostartService = autostartService ?? new PlatformAutostartService();
        var sessionUpdateService = updateService ?? new LegacyUpdateService(sessionAppBuildInfoProvider);

        _session = new DesktopSessionController(
            bootstrapper,
            legacyDataStore,
            attachmentService,
            new LegacyAuditService(attachmentService),
            new LegacyCostAnalysisService(),
            sessionBackupService,
            sessionAutostartService,
            sessionSupportedSettingsService,
            sessionAppBuildInfoProvider,
            sessionUpdateService);
        _fileLauncher = fileLauncher;
        _filePickerService = filePickerService;
        _globalSearchService = globalSearchService;
        _timelineService = timelineService;
        _calendarExportService = calendarExportService;
        _fileSaveService = fileSaveService;
        _fileDialogService = fileDialogService ?? new AvaloniaFileDialogService();
        _projectionService = projectionService ?? new DesktopProjectionService();
        _navigationCoordinator = navigationCoordinator ?? new DesktopNavigationCoordinator();
        AppShellController = new DesktopAppShellController(
            appShellDialogService ?? new AvaloniaAppShellDialogService(),
            updateInstallLauncher ?? new ProcessUpdateInstallLauncher());
        InitializeWorkspaces();
        Load(applyLaunchTabPreference: true);
    }

    public bool CanOpenSelectedRecordFile => SelectedRecord is { FileExists: true } && !string.IsNullOrWhiteSpace(SelectedRecord.ResolvedPath);

    public bool CanOpenSelectedRecordFolder =>
        SelectedRecord is not null
        && !string.IsNullOrWhiteSpace(GetSelectedRecordFolderPath());

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
    }

    partial void OnSelectedVehicleChanged(VehicleListItemViewModel? value)
    {
        HandleVehicleSelectionChanged();
        EditSelectedVehicleCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanOpenVehicleDetailWindow));
        OnPropertyChanged(nameof(CanOpenHistoryWindow));
        OnPropertyChanged(nameof(CanOpenFuelWindow));
        OnPropertyChanged(nameof(CanOpenReminderWindow));
        OnPropertyChanged(nameof(CanOpenMaintenanceWindow));
        OnPropertyChanged(nameof(CanOpenRecordWindow));
        OnPropertyChanged(nameof(VehicleDetailWindowTitle));
        OnPropertyChanged(nameof(HistoryWindowTitle));
        OnPropertyChanged(nameof(FuelWindowTitle));
        OnPropertyChanged(nameof(ReminderWindowTitle));
        OnPropertyChanged(nameof(MaintenanceWindowTitle));
        OnPropertyChanged(nameof(RecordWindowTitle));

        if (value is null)
        {
            var projection = _projectionService.BuildVehicleDetail(null);
            SelectedVehicleHeading = projection.Heading;
            SelectedVehicleOverview = projection.Overview;
            SelectedVehicleDates = projection.Dates;
            SelectedVehicleProfile = projection.Profile;
            HistorySummary = "Historie vybraného vozidla se zobrazí po výběru vozidla.";
            FuelSummary = "Tankování vybraného vozidla se zobrazí po výběru vozidla.";
            ReminderSummary = "Připomínky vybraného vozidla se zobrazí po výběru vozidla.";
            MaintenanceSummary = "Plán údržby vybraného vozidla se zobrazí po výběru vozidla.";
            TimelineSummary = "Časová osa vybraného vozidla se zobrazí po výběru vozidla.";
            RecordSummary = "Doklady a přílohy vybraného vozidla se zobrazí po výběru vozidla.";
            SelectedVehicleHistory.Clear();
            SelectedVehicleFuel.Clear();
            SelectedVehicleReminders.Clear();
            SelectedVehicleMaintenance.Clear();
            SelectedVehicleTimeline.Clear();
            SelectedVehicleRecords.Clear();
            DashboardUpcomingTimeline.Clear();
            SelectedHistory = null;
            SelectedFuel = null;
            SelectedReminder = null;
            SelectedMaintenance = null;
            SelectedTimelineItem = null;
            SelectedDashboardAuditItem = null;
            SelectedDashboardCostVehicle = null;
            SelectedDashboardTimelineItem = null;
            SelectedRecord = null;
            DashboardTimelineSummary = "Nejbližší termíny napříč vozidly se zobrazí po načtení dat.";
            SelectedDashboardTimelineDetail = "Vyberte nejbližší termín a můžete přejít na související vozidlo nebo evidenci.";
            SelectedVehicleTabIndex = DetailTabIndex;
            RequestFocus(DesktopFocusTarget.VehicleList);
            return;
        }

        var detailProjection = _projectionService.BuildVehicleDetail(value);
        SelectedVehicleHeading = detailProjection.Heading;
        SelectedVehicleOverview = detailProjection.Overview;
        SelectedVehicleDates = detailProjection.Dates;
        SelectedVehicleProfile = detailProjection.Profile;

        PopulateVehicleHistory(value.Id);
        PopulateVehicleFuel(value.Id);
        PopulateVehicleReminders(value.Id);
        PopulateVehicleMaintenance(value.Id);
        PopulateVehicleTimeline(value.Id);
        PopulateVehicleRecords(value.Id);
    }

    [RelayCommand]
    private void Reload()
    {
        Load(SelectedVehicle?.Id, SelectedVehicleTabIndex, applyLaunchTabPreference: false);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    [RelayCommand]
    private void FocusTimelineSearch()
    {
        SelectedVehicleTabIndex = TimelineTabIndex;
        RequestFocus(DesktopFocusTarget.TimelineSearch);
    }

    [RelayCommand]
    private void FocusGlobalSearch()
    {
        SelectedVehicleTabIndex = SearchTabIndex;
        RequestFocus(DesktopFocusTarget.GlobalSearchBox);
    }

    [RelayCommand]
    private void FocusUpcomingOverview()
    {
        SelectedVehicleTabIndex = UpcomingOverviewTabIndex;
        RequestFocus(DesktopFocusTarget.UpcomingOverviewSearch);
    }

    [RelayCommand]
    private void FocusOverdueOverview()
    {
        SelectedVehicleTabIndex = OverdueOverviewTabIndex;
        RequestFocus(DesktopFocusTarget.OverdueOverviewSearch);
    }

    [RelayCommand]
    private void SelectVehicleTab(int tabIndex)
    {
        if (tabIndex < DetailTabIndex || tabIndex > OverdueOverviewTabIndex)
        {
            return;
        }

        SelectedVehicleTabIndex = tabIndex;
    }

    [RelayCommand]
    private async Task ExportCalendarAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var export = _calendarExportService.BuildUpcomingCalendar(_dataSet, today, DateTimeOffset.UtcNow);
        if (export.Items.Count == 0)
        {
            ExportStatus = "Kalendář zatím neobsahuje žádné budoucí položky s konkrétním datem.";
            return;
        }

        var suggestedFileName = $"vehimap-kalendar-{today:yyyy-MM-dd}.ics";
        var savedPath = await _fileSaveService
            .SaveTextAsync("Export termínů do kalendáře", suggestedFileName, export.IcsContent)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(savedPath))
        {
            ExportStatus = "Export kalendáře byl zrušen.";
            return;
        }

        ExportStatus = export.SkippedMaintenanceCount > 0
            ? $"Kalendář uložen do {savedPath}. Položek: {export.Items.Count}. Přeskočené servisní úkoly bez data: {export.SkippedMaintenanceCount}."
            : $"Kalendář uložen do {savedPath}. Položek: {export.Items.Count}.";
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedTimelineItem))]
    private void OpenSelectedTimelineItem()
    {
        if (SelectedTimelineItem is null)
        {
            return;
        }

        OpenTimelineItem(SelectedTimelineItem);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedRecordFile))]
    private async Task OpenSelectedRecordFileAsync()
    {
        if (!CanOpenSelectedRecordFile || SelectedRecord is null)
        {
            return;
        }

        await _fileLauncher.OpenAsync(SelectedRecord.ResolvedPath).ConfigureAwait(false);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedRecordFolder))]
    private async Task OpenSelectedRecordFolderAsync()
    {
        var folderPath = GetSelectedRecordFolderPath();
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return;
        }

        await _fileLauncher.OpenFolderAsync(folderPath).ConfigureAwait(false);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedDashboardAuditItem))]
    private void OpenSelectedDashboardAuditItem()
    {
        if (SelectedDashboardAuditItem is null)
        {
            return;
        }

        SelectVehicleAndOpenEntity(SelectedDashboardAuditItem.VehicleId, SelectedDashboardAuditItem.EntityKind, SelectedDashboardAuditItem.EntityId);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedDashboardCostVehicle))]
    private void OpenSelectedDashboardCostVehicle()
    {
        if (SelectedDashboardCostVehicle is null)
        {
            return;
        }

        SelectVehicleAndOpenEntity(SelectedDashboardCostVehicle.VehicleId, "Vozidlo", SelectedDashboardCostVehicle.VehicleId);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedDashboardTimelineItem))]
    private void OpenSelectedDashboardTimelineItem()
    {
        if (SelectedDashboardTimelineItem is null)
        {
            return;
        }

        OpenTimelineItem(SelectedDashboardTimelineItem);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedSearchResult))]
    private void OpenSelectedSearchResult()
    {
        if (SelectedSearchResult is null)
        {
            return;
        }

        SelectVehicleAndOpenEntity(SelectedSearchResult.VehicleId, SelectedSearchResult.EntityKind, SelectedSearchResult.EntityId);
    }

    private void Load(string? preferredVehicleId = null, int? preferredTabIndex = null, bool applyLaunchTabPreference = false)
    {
        try
        {
            var result = _session.LoadAsync(AppContext.BaseDirectory).GetAwaiter().GetResult();
            var costSummary = result.CostSummary;

            LoadError = string.Empty;
            DataMode = result.DataRoot.IsPortable ? "Portable data vedle aplikace" : "Systémová datová složka";
            DataPath = result.DataRoot.DataPath;
            VehicleCount = result.DataSet.Vehicles.Count;
            HistoryCount = result.DataSet.HistoryEntries.Count;
            FuelCount = result.DataSet.FuelEntries.Count;
            RecordsCount = result.DataSet.Records.Count;
            RemindersCount = result.DataSet.Reminders.Count;
            MaintenanceCount = result.DataSet.MaintenancePlans.Count;
            AuditCount = _auditItems.Count;
            AuditSummary = _projectionService.BuildAuditSummary(_auditItems);
            CostSummary = _projectionService.BuildCostSummary(costSummary);
            CostComparison = _projectionService.BuildCostComparison(costSummary);

            Vehicles.Clear();
            foreach (var vehicle in _projectionService.BuildVehicleList(result.DataSet, _metaByVehicleId, _auditItems))
            {
                Vehicles.Add(vehicle);
            }

            AuditItems.Clear();
            foreach (var item in _projectionService.BuildDashboardAuditItems(_auditItems))
            {
                AuditItems.Add(item);
            }

            CostVehicles.Clear();
            foreach (var row in _projectionService.BuildDashboardCostVehicles(costSummary))
            {
                CostVehicles.Add(row);
            }

            PopulateDashboardTimeline();
            RefreshFleetOverviews();
            RefreshGlobalSearch();
            SelectedDashboardAuditItem = AuditItems.FirstOrDefault();
            SelectedDashboardCostVehicle = CostVehicles.FirstOrDefault();
            SelectedDashboardTimelineItem = DashboardUpcomingTimeline.FirstOrDefault();

            SelectedVehicle = string.IsNullOrWhiteSpace(preferredVehicleId)
                ? Vehicles.FirstOrDefault()
                : FindById(Vehicles, item => item.Id, preferredVehicleId) ?? Vehicles.FirstOrDefault();
            if (SelectedVehicle is null)
            {
                OnSelectedVehicleChanged(null);
            }

            if (applyLaunchTabPreference && result.SupportedSettings.ShowDashboardOnLaunch && !result.SupportedSettings.HideOnLaunch)
            {
                SelectedVehicleTabIndex = DashboardTabIndex;
            }
            else if (preferredTabIndex.HasValue && preferredTabIndex.Value >= DetailTabIndex && preferredTabIndex.Value <= OverdueOverviewTabIndex)
            {
                SelectedVehicleTabIndex = preferredTabIndex.Value;
            }
        }
        catch (Exception ex)
        {
            LoadError = ex.Message;
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

        HistorySummary = projection.Summary;

        SelectedHistory = SelectedVehicleHistory.FirstOrDefault();
        if (SelectedHistory is null)
        {
            SelectedHistoryDetail = "Vyberte historický záznam a zobrazí se detail položky.";
            NotifyHistoryWorkspaceSelectionChanged();
        }
    }

    private void PopulateVehicleFuel(string vehicleId)
    {
        SelectedVehicleFuel.Clear();
        var projection = _projectionService.BuildFuel(_dataSet, vehicleId);
        foreach (var item in projection.Items)
        {
            SelectedVehicleFuel.Add(item);
        }

        FuelSummary = projection.Summary;

        SelectedFuel = SelectedVehicleFuel.FirstOrDefault();
        if (SelectedFuel is null)
        {
            SelectedFuelDetail = "Vyberte tankování a zobrazí se detail položky.";
            NotifyFuelWorkspaceSelectionChanged();
        }
    }

    private void PopulateVehicleReminders(string vehicleId)
    {
        SelectedVehicleReminders.Clear();
        var projection = _projectionService.BuildReminders(_dataSet, vehicleId, DateOnly.FromDateTime(DateTime.Today));
        foreach (var item in projection.Items)
        {
            SelectedVehicleReminders.Add(item);
        }

        ReminderSummary = projection.Summary;

        SelectedReminder = SelectedVehicleReminders.FirstOrDefault();
        if (SelectedReminder is null)
        {
            SelectedReminderDetail = "Vyberte připomínku a zobrazí se detail položky.";
            NotifyReminderWorkspaceSelectionChanged();
        }
    }

    private void PopulateVehicleMaintenance(string vehicleId)
    {
        SelectedVehicleMaintenance.Clear();
        var projection = _projectionService.BuildMaintenance(_dataSet, vehicleId, DateOnly.FromDateTime(DateTime.Today));
        foreach (var item in projection.Items)
        {
            SelectedVehicleMaintenance.Add(item);
        }

        MaintenanceSummary = projection.Summary;

        SelectedMaintenance = SelectedVehicleMaintenance.FirstOrDefault();
        if (SelectedMaintenance is null)
        {
            SelectedMaintenanceDetail = "Vyberte servisní úkon a zobrazí se detail položky.";
            NotifyMaintenanceWorkspaceSelectionChanged();
        }
    }

    private void PopulateVehicleTimeline(string vehicleId)
    {
        SelectedVehicleTimeline.Clear();
        var projection = _projectionService.BuildTimeline(
            _dataSet,
            _timelineService,
            vehicleId,
            DateOnly.FromDateTime(DateTime.Today),
            SelectedTimelineFilter,
            TimelineSearchText);
        foreach (var item in projection.Items)
        {
            SelectedVehicleTimeline.Add(item);
        }

        TimelineSummary = projection.Summary;
        SelectedTimelineItem = SelectedVehicleTimeline.FirstOrDefault();
        if (SelectedTimelineItem is null)
        {
            SelectedTimelineDetail = "Vyberte položku časové osy a zobrazí se detail.";
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

        RecordSummary = projection.Summary;

        SelectedRecord = SelectedVehicleRecords.FirstOrDefault();
        if (SelectedRecord is null)
        {
            SelectedRecordDetail = "Vyberte doklad a zobrazí se detail přílohy.";
            NotifyRecordWorkspaceSelectionChanged();
        }
    }

    private void RefreshTimeline()
    {
        if (SelectedVehicle is null)
        {
            TimelineSummary = "Časová osa vybraného vozidla se zobrazí po výběru vozidla.";
            SelectedTimelineItem = null;
            return;
        }

        var previousSelection = SelectedTimelineItem;
        var projection = _projectionService.BuildTimeline(
            _dataSet,
            _timelineService,
            SelectedVehicle.Id,
            DateOnly.FromDateTime(DateTime.Today),
            SelectedTimelineFilter,
            TimelineSearchText);
        SelectedVehicleTimeline.Clear();
        foreach (var item in projection.Items)
        {
            SelectedVehicleTimeline.Add(item);
        }

        TimelineSummary = projection.Summary;
        SelectedTimelineItem = previousSelection is null
            ? SelectedVehicleTimeline.FirstOrDefault()
            : FindTimelineItem(SelectedVehicleTimeline, previousSelection);
        if (SelectedTimelineItem is null)
        {
            SelectedTimelineDetail = "Vyberte položku časové osy a zobrazí se detail.";
            NotifyTimelineWorkspaceSelectionChanged();
        }
    }

    private void RefreshGlobalSearch()
    {
        var previousSelection = SelectedSearchResult;
        GlobalSearchResults.Clear();

        if (_dataRoot is null || string.IsNullOrWhiteSpace(GlobalSearchText))
        {
            GlobalSearchSummary = "Zadejte hledaný text a zobrazí se odpovídající vozidla i záznamy napříč aplikací.";
            SelectedSearchResult = null;
            return;
        }

        var results = _globalSearchService.Search(_dataRoot, _dataSet, GlobalSearchText);
        foreach (var result in results)
        {
            GlobalSearchResults.Add(new GlobalSearchResultItemViewModel(
                result.VehicleId,
                result.EntityKind,
                result.EntityId,
                result.VehicleName,
                result.SectionLabel,
                result.Title,
                result.Summary));
        }

        GlobalSearchSummary = results.Count == 0
            ? $"Pro dotaz „{GlobalSearchText.Trim()}“ nebyly nalezeny žádné výsledky."
            : $"Dotaz „{GlobalSearchText.Trim()}“: {results.Count} výsledků. Enter otevře vybranou položku.";

        var previousKey = previousSelection is null
            ? string.Empty
            : $"{previousSelection.EntityKind}|{previousSelection.EntityId}|{previousSelection.VehicleId}";
        SelectedSearchResult = FindById(GlobalSearchResults, item => $"{item.EntityKind}|{item.EntityId}|{item.VehicleId}", previousKey);
        if (SelectedSearchResult is null)
        {
            SelectedSearchResultDetail = "Vyberte výsledek a můžete přejít rovnou na správné vozidlo nebo evidenci.";
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

        DashboardTimelineSummary = projection.Summary;
    }

    private string? GetSelectedRecordFolderPath()
    {
        return _projectionService.GetRecordFolderPath(SelectedRecord);
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

    private void ApplyNavigationPlan(DesktopNavigationPlan plan, VehicleTimelineItemViewModel? timelineSelection = null)
    {
        if (!string.IsNullOrWhiteSpace(plan.VehicleId)
            && !string.Equals(SelectedVehicle?.Id, plan.VehicleId, StringComparison.Ordinal))
        {
            SelectedVehicle = Vehicles.FirstOrDefault(vehicle => string.Equals(vehicle.Id, plan.VehicleId, StringComparison.Ordinal));
        }

        if (timelineSelection is not null)
        {
            SelectedTimelineItem = FindTimelineItem(SelectedVehicleTimeline, timelineSelection);
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

    private void RequestFocus(DesktopFocusTarget target)
    {
        FocusRequested?.Invoke(target);
    }

    private static string FormatCostValue(string? value)
    {
        if (VehimapValueParser.TryParseMoney(value, out var parsed))
        {
            return FormatMoney(parsed);
        }

        return FormatValue(value, "bez ceny");
    }

    private static string FormatFuelLiters(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "bez množství";
        }

        return value.Contains('l', StringComparison.OrdinalIgnoreCase)
            ? value
            : $"{value} l";
    }

    private static string FormatOdometerValue(string? value)
    {
        if (!VehimapValueParser.TryParseOdometer(value, out var parsed))
        {
            return FormatValue(value, "bez tachometru");
        }

        return $"{parsed} km";
    }

    private static string FormatMoney(decimal value) => $"{value:0.00} Kč";

    private static string FormatValue(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}
