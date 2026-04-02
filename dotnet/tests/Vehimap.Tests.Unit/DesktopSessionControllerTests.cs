using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopSessionControllerTests
{
    [Fact]
    public async Task Load_async_populates_state_meta_audit_and_settings()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobni vozidla", "Rodinne auto", "Skoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "Bezny provoz", "rodinne", "benzin", "s klimou", "normalni", "manual")
            ]
        };
        dataSet.Settings.SetValue("app", "show_dashboard_on_launch", "1");

        IReadOnlyList<AuditItem> auditItems =
        [
            new AuditItem(AuditSeverity.Warning, "Doklady", "veh_1", "Milena", "record", "rec_1", "Chybi doklad", "Pojisteni nema prilohu.")
        ];
        var costSummary = new CostAnalysisSummary(
            "2026",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            1250m,
            2000,
            0.625m,
            1000m,
            0.5m,
            250m,
            0.125m,
            1,
            0,
            0,
            []);

        var session = CreateSessionController(
            dataRoot,
            new StubLegacyDataStore(dataSet),
            auditService: new StubAuditService(auditItems),
            costAnalysisService: new StubCostAnalysisService(costSummary));

        var result = await session.LoadAsync(dataRoot.AppBasePath);

        Assert.True(session.IsLoaded);
        Assert.Same(dataRoot, session.DataRoot);
        Assert.Same(dataSet, session.DataSet);
        Assert.Single(session.AuditItems);
        Assert.Equal("Chybi doklad", session.AuditItems[0].Title);
        Assert.True(session.MetaByVehicleId.ContainsKey("veh_1"));
        Assert.Equal("benzin", session.MetaByVehicleId["veh_1"].Powertrain);
        Assert.Equal(costSummary, result.CostSummary);
        Assert.True(result.SupportedSettings.ShowDashboardOnLaunch);
        Assert.Equal(dataRoot, result.DataRoot);
        Assert.Same(dataSet, result.DataSet);
    }

    [Fact]
    public async Task Apply_supported_settings_persists_values_and_keeps_other_keys()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobni vozidla", "Rodinne auto", "Skoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        dataSet.Settings.SetValue("app", "hide_on_launch", "1");
        var dataStore = new StubLegacyDataStore(dataSet);
        var session = CreateSessionController(dataRoot, dataStore);
        await session.LoadAsync(dataRoot.AppBasePath);

        await session.ApplySupportedSettingsAsync(new DesktopSupportedSettingsSnapshot(45, 21, 14, 1500, false, true, true, true, 2, 10));

        Assert.Equal("45", dataStore.CurrentDataSet.Settings.GetValue("notifications", "technical_reminder_days"));
        Assert.Equal("21", dataStore.CurrentDataSet.Settings.GetValue("notifications", "green_card_reminder_days"));
        Assert.Equal("14", dataStore.CurrentDataSet.Settings.GetValue("notifications", "maintenance_reminder_days"));
        Assert.Equal("1500", dataStore.CurrentDataSet.Settings.GetValue("notifications", "maintenance_reminder_km"));
        Assert.Equal("1", dataStore.CurrentDataSet.Settings.GetValue("app", "show_dashboard_on_launch"));
        Assert.Equal("1", dataStore.CurrentDataSet.Settings.GetValue("app", "hide_on_launch"));
        Assert.Equal("1", dataStore.CurrentDataSet.Settings.GetValue("backups", "automatic_backups_enabled"));
        Assert.Equal("2", dataStore.CurrentDataSet.Settings.GetValue("backups", "automatic_backup_interval_days"));
        Assert.Equal("10", dataStore.CurrentDataSet.Settings.GetValue("backups", "automatic_backup_keep_count"));
    }

    [Fact]
    public async Task Update_and_build_info_are_delegated_through_session_controller()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobni vozidla", "Rodinne auto", "Skoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        var buildInfo = new AppBuildInfo(
            "Vehimap",
            "1.2.3",
            "1.2.3.0",
            "Avalonia test",
            @"C:\vehimap\Vehimap.Desktop.exe",
            "Windows",
            ".NET 10",
            "https://example.com/latest.ini",
            "https://example.com/release",
            @"C:\vehimap\Vehimap.Updater.exe",
            true);
        var updateResult = new UpdateCheckResult("1.2.3", "1.2.4", true, "2026-04-02", "https://example.com/release", "https://example.com/vehimap.zip", "abc", 1234, true, "Nova verze je k dispozici.");
        var installResult = new UpdateInstallResult(true, "Pripraveno k instalaci.", new UpdateInstallPlan(@"C:\vehimap\Vehimap.Updater.exe", @"C:\temp\update", @"C:\vehimap", @"C:\vehimap\Vehimap.Desktop.exe", 123, "1.2.4"));
        var updateService = new StubUpdateService(updateResult, installResult);
        var session = CreateSessionController(
            dataRoot,
            new StubLegacyDataStore(dataSet),
            appBuildInfoProvider: new StubBuildInfoProvider(buildInfo),
            updateService: updateService);
        await session.LoadAsync(dataRoot.AppBasePath);

        var loadedBuildInfo = session.GetAppInfo();
        var checkedUpdate = await session.CheckForUpdatesAsync();
        var preparedInstall = await session.PrepareInstallAsync(updateResult);

        Assert.Equal(buildInfo, loadedBuildInfo);
        Assert.Equal("1.2.3", updateService.LastCurrentVersion);
        Assert.Equal(updateResult, checkedUpdate);
        Assert.Equal(installResult, preparedInstall);
    }

    [Fact]
    public async Task Resolve_managed_attachment_path_uses_loaded_data_root()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobni vozidla", "Rodinne auto", "Skoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        var attachmentService = new StubAttachmentService();
        var session = CreateSessionController(
            dataRoot,
            new StubLegacyDataStore(dataSet),
            attachmentService: attachmentService);
        await session.LoadAsync(dataRoot.AppBasePath);

        var resolvedPath = session.ResolveManagedAttachmentPath(@"attachments/veh_1/pojisteni.pdf");

        Assert.Equal(@"C:\vehimap-test\data\attachments\veh_1\pojisteni.pdf", resolvedPath);
        Assert.Equal(dataRoot, attachmentService.LastDataRoot);
        Assert.Equal(@"attachments/veh_1/pojisteni.pdf", attachmentService.LastRelativePath);
    }

    [Fact]
    public async Task Load_async_prefers_autostart_runtime_state_over_stored_value()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobni vozidla", "Rodinne auto", "Skoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        dataSet.Settings.SetValue("app", "run_at_startup", "0");
        var autostartService = new StubAutostartService { IsEnabledResult = true };
        var session = CreateSessionController(dataRoot, new StubLegacyDataStore(dataSet), autostartService: autostartService);

        var result = await session.LoadAsync(dataRoot.AppBasePath);

        Assert.True(result.SupportedSettings.RunAtStartup);
        Assert.True(session.ReadSupportedSettings().RunAtStartup);
    }

    [Fact]
    public async Task Create_automatic_backup_async_exports_file_and_updates_settings()
    {
        var dataRoot = new VehimapDataRoot(Path.Combine(Path.GetTempPath(), "vehimap-session-test", Guid.NewGuid().ToString("N")), Path.Combine(Path.GetTempPath(), "vehimap-session-test", Guid.NewGuid().ToString("N"), "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobni vozidla", "Rodinne auto", "Skoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        var backupService = new StubBackupService();
        var dataStore = new StubLegacyDataStore(dataSet);
        var session = CreateSessionController(dataRoot, dataStore, backupService: backupService);
        await session.LoadAsync(dataRoot.AppBasePath);

        var result = await session.CreateAutomaticBackupAsync();

        Assert.True(result.Created);
        Assert.False(result.IsError);
        Assert.Equal(result.BackupPath, backupService.ExportedPath);
        Assert.False(string.IsNullOrWhiteSpace(dataStore.CurrentDataSet.Settings.GetValue("backups", "last_automatic_backup_stamp")));
        Assert.Equal(result.BackupPath, dataStore.CurrentDataSet.Settings.GetValue("backups", "last_automatic_backup_path"));
    }

    private static DesktopSessionController CreateSessionController(
        VehimapDataRoot dataRoot,
        StubLegacyDataStore dataStore,
        IFileAttachmentService? attachmentService = null,
        IAuditService? auditService = null,
        ICostAnalysisService? costAnalysisService = null,
        IBackupService? backupService = null,
        IAutostartService? autostartService = null,
        IAppBuildInfoProvider? appBuildInfoProvider = null,
        IUpdateService? updateService = null)
    {
        var bootstrapper = new LegacyVehimapBootstrapper(new StubDataRootLocator(dataRoot), dataStore);
        return new DesktopSessionController(
            bootstrapper,
            dataStore,
            attachmentService ?? new StubAttachmentService(),
            auditService ?? new StubAuditService([]),
            costAnalysisService ?? new StubCostAnalysisService(new CostAnalysisSummary("2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 0m, null, null, 0m, null, 0m, null, 0, 0, 0, [])),
            backupService ?? new StubBackupService(),
            autostartService ?? new StubAutostartService(),
            new DesktopSupportedSettingsService(),
            appBuildInfoProvider ?? new StubBuildInfoProvider(new AppBuildInfo("Vehimap", "1.0.0", "1.0.0.0", "Test", @"C:\vehimap\Vehimap.Desktop.exe", "Windows", ".NET 10", "https://example.com/latest.ini", "https://example.com/release", @"C:\vehimap\Vehimap.Updater.exe", false)),
            updateService ?? new StubUpdateService(
                new UpdateCheckResult("1.0.0", "1.0.0", false, null, null, null, null, null, false, "Aktualni."),
                new UpdateInstallResult(false, "Bez nove verze.", null)));
    }

    private sealed class StubDataRootLocator : IDataRootLocator
    {
        private readonly VehimapDataRoot _dataRoot;

        public StubDataRootLocator(VehimapDataRoot dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public VehimapDataRoot Resolve(string appBasePath) => _dataRoot;
    }

    private sealed class StubLegacyDataStore : ILegacyDataStore
    {
        public StubLegacyDataStore(VehimapDataSet dataSet)
        {
            CurrentDataSet = dataSet;
        }

        public VehimapDataSet CurrentDataSet { get; set; }

        public Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
            => Task.FromResult(CurrentDataSet);

        public Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
        {
            CurrentDataSet = dataSet;
            return Task.CompletedTask;
        }
    }

    private sealed class StubAttachmentService : IFileAttachmentService
    {
        public VehimapDataRoot? LastDataRoot { get; private set; }

        public string? LastRelativePath { get; private set; }

        public string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath)
        {
            LastDataRoot = dataRoot;
            LastRelativePath = relativePath;
            return Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }

    private sealed class StubAuditService : IAuditService
    {
        private readonly IReadOnlyList<AuditItem> _auditItems;

        public StubAuditService(IReadOnlyList<AuditItem> auditItems)
        {
            _auditItems = auditItems;
        }

        public IReadOnlyList<AuditItem> BuildAudit(VehimapDataRoot dataRoot, VehimapDataSet dataSet) => _auditItems;
    }

    private sealed class StubCostAnalysisService : ICostAnalysisService
    {
        private readonly CostAnalysisSummary _summary;

        public StubCostAnalysisService(CostAnalysisSummary summary)
        {
            _summary = summary;
        }

        public CostAnalysisSummary BuildYearToDateSummary(VehimapDataSet dataSet, DateOnly today) => _summary;
    }

    private sealed class StubBackupService : IBackupService
    {
        public string? ExportedPath { get; private set; }

        public Task ExportAsync(string backupPath, VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
        {
            ExportedPath = backupPath;
            Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
            return File.WriteAllTextAsync(backupPath, "backup", cancellationToken);
        }

        public Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default)
            => Task.FromResult(new VehimapBackupBundle(new VehimapDataSet(), []));

        public Task RestoreAsync(VehimapDataRoot dataRoot, VehimapBackupBundle backupBundle, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StubBuildInfoProvider : IAppBuildInfoProvider
    {
        private readonly AppBuildInfo _buildInfo;

        public StubBuildInfoProvider(AppBuildInfo buildInfo)
        {
            _buildInfo = buildInfo;
        }

        public AppBuildInfo GetCurrent() => _buildInfo;
    }

    private sealed class StubUpdateService : IUpdateService
    {
        private readonly UpdateCheckResult _checkResult;
        private readonly UpdateInstallResult _installResult;

        public StubUpdateService(UpdateCheckResult checkResult, UpdateInstallResult installResult)
        {
            _checkResult = checkResult;
            _installResult = installResult;
        }

        public string? LastCurrentVersion { get; private set; }

        public Task<UpdateCheckResult> CheckForUpdatesAsync(string currentVersion, CancellationToken cancellationToken = default)
        {
            LastCurrentVersion = currentVersion;
            return Task.FromResult(_checkResult);
        }

        public Task<UpdateInstallResult> PrepareInstallAsync(UpdateCheckResult update, CancellationToken cancellationToken = default)
            => Task.FromResult(_installResult);
    }

    private sealed class StubAutostartService : IAutostartService
    {
        public bool IsEnabledResult { get; set; }

        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default) => Task.FromResult(IsEnabledResult);

        public Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
