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
    private static readonly string[] NotificationHistoryKeys =
    [
        "last_alert_day",
        "last_alert_signature",
        "last_green_alert_day",
        "last_green_alert_signature",
        "last_reminder_alert_day",
        "last_reminder_alert_signature",
        "last_maintenance_alert_day",
        "last_maintenance_alert_signature",
        "last_desktop_alert_day",
        "last_desktop_alert_signature"
    ];

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
    public async Task Build_cost_summary_uses_requested_period()
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
        var costSummary = new CostAnalysisSummary(
            "Unor 2026",
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 28),
            500m,
            100,
            5m,
            400m,
            4m,
            100m,
            1m,
            1,
            0,
            0,
            []);
        var costAnalysisService = new StubCostAnalysisService(costSummary);
        var session = CreateSessionController(dataRoot, new StubLegacyDataStore(dataSet), costAnalysisService: costAnalysisService);
        await session.LoadAsync(dataRoot.AppBasePath);

        var result = session.BuildCostSummary(new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28));

        Assert.Equal(costSummary, result);
        Assert.Equal(new DateOnly(2026, 2, 1), costAnalysisService.LastPeriodStart);
        Assert.Equal(new DateOnly(2026, 2, 28), costAnalysisService.LastPeriodEnd);
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
    public async Task Apply_supported_settings_updates_autostart_service()
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
        var autostartService = new StubAutostartService();
        var session = CreateSessionController(dataRoot, new StubLegacyDataStore(dataSet), autostartService: autostartService);
        await session.LoadAsync(dataRoot.AppBasePath);

        await session.ApplySupportedSettingsAsync(new DesktopSupportedSettingsSnapshot(30, 30, 31, 1000, true, false, false, false, 1, 30));

        Assert.True(autostartService.LastSetEnabled);
    }

    [Fact]
    public async Task Should_show_and_remember_due_notification_persists_daily_signature()
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
        var dataStore = new StubLegacyDataStore(dataSet);
        var session = CreateSessionController(dataRoot, dataStore);
        await session.LoadAsync(dataRoot.AppBasePath);

        var firstResult = await session.ShouldShowAndRememberDueNotificationAsync("timeline|veh_1|technical|20260402", new DateOnly(2026, 4, 2));
        var duplicateResult = await session.ShouldShowAndRememberDueNotificationAsync("timeline|veh_1|technical|20260402", new DateOnly(2026, 4, 2));
        var nextDayResult = await session.ShouldShowAndRememberDueNotificationAsync("timeline|veh_1|technical|20260402", new DateOnly(2026, 4, 3));

        Assert.True(firstResult);
        Assert.False(duplicateResult);
        Assert.True(nextDayResult);
        Assert.Equal("20260403", dataStore.CurrentDataSet.Settings.GetValue("notifications", "last_desktop_alert_day"));
        Assert.Equal("timeline|veh_1|technical|20260402", dataStore.CurrentDataSet.Settings.GetValue("notifications", "last_desktop_alert_signature"));
    }

    [Fact]
    public async Task Should_show_and_remember_due_notification_rolls_back_when_persist_fails()
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
        var dataStore = new StubLegacyDataStore(dataSet)
        {
            SaveException = new IOException("settings.ini nelze zapsat.")
        };
        var session = CreateSessionController(dataRoot, dataStore);
        await session.LoadAsync(dataRoot.AppBasePath);

        var result = await session.ShouldShowAndRememberDueNotificationAsync("timeline|veh_1|technical|20260402", new DateOnly(2026, 4, 2));

        Assert.False(result);
        Assert.Equal(string.Empty, session.DataSet.Settings.GetValue("notifications", "last_desktop_alert_day"));
        Assert.Equal(string.Empty, session.DataSet.Settings.GetValue("notifications", "last_desktop_alert_signature"));
    }

    [Fact]
    public async Task Apply_supported_settings_resets_notification_history()
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
        SeedNotificationHistory(dataSet.Settings);
        var dataStore = new StubLegacyDataStore(dataSet);
        var session = CreateSessionController(dataRoot, dataStore);
        await session.LoadAsync(dataRoot.AppBasePath);

        await session.ApplySupportedSettingsAsync(new DesktopSupportedSettingsSnapshot(45, 21, 14, 1500, false, true, false, false, 1, 30));

        AssertNotificationHistoryCleared(dataStore.CurrentDataSet.Settings);
    }

    [Fact]
    public async Task Apply_supported_settings_rolls_back_settings_when_persist_fails()
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
        dataSet.Settings.SetValue("notifications", "technical_reminder_days", "30");
        dataSet.Settings.SetValue("app", "show_dashboard_on_launch", "0");
        SeedNotificationHistory(dataSet.Settings);
        var dataStore = new StubLegacyDataStore(dataSet)
        {
            SaveException = new IOException("settings.ini nelze zapsat.")
        };
        var session = CreateSessionController(dataRoot, dataStore);
        await session.LoadAsync(dataRoot.AppBasePath);

        await Assert.ThrowsAsync<IOException>(() =>
            session.ApplySupportedSettingsAsync(new DesktopSupportedSettingsSnapshot(45, 21, 14, 1500, false, true, false, false, 1, 30)));

        Assert.Equal("30", session.DataSet.Settings.GetValue("notifications", "technical_reminder_days"));
        Assert.Equal("0", session.DataSet.Settings.GetValue("app", "show_dashboard_on_launch"));
        foreach (var key in NotificationHistoryKeys)
        {
            Assert.Equal("seed", session.DataSet.Settings.GetValue("notifications", key));
        }

        Assert.False(session.ReadSupportedSettings().ShowDashboardOnLaunch);
    }

    [Fact]
    public async Task Restore_backup_resets_imported_notification_history_before_restore()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var currentData = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobni vozidla", "Rodinne auto", "Skoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        var importedData = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_2", "Bozena", "Osobni vozidla", "Veteran", "Skoda 100", "", "1974", "35", "", "09/2026", "05/2025", "10/2026")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_2", "Veteran", "srazove", "benzin", "", "klidne", "manual")
            ]
        };
        importedData.Settings.SetValue("app", "show_dashboard_on_launch", "1");
        SeedNotificationHistory(importedData.Settings);
        var backupService = new StubBackupService
        {
            ImportBundle = new VehimapBackupBundle(importedData, [])
        };
        var session = CreateSessionController(dataRoot, new StubLegacyDataStore(currentData), backupService: backupService);
        await session.LoadAsync(dataRoot.AppBasePath);

        await session.RestoreBackupAsync(@"C:\backups\vehimap.vehimapbak");

        Assert.NotNull(backupService.RestoredBundle);
        AssertNotificationHistoryCleared(backupService.RestoredBundle.Data.Settings);
        Assert.NotSame(importedData, session.DataSet);
        Assert.Equal("veh_2", Assert.Single(session.DataSet.Vehicles).Id);
        Assert.True(session.MetaByVehicleId.ContainsKey("veh_2"));
        Assert.True(session.ReadSupportedSettings().ShowDashboardOnLaunch);
        AssertNotificationHistoryCleared(session.DataSet.Settings);
    }

    [Fact]
    public async Task Restore_backup_keeps_current_session_state_when_restore_fails()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var currentData = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobni vozidla", "Rodinne auto", "Skoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        currentData.Settings.SetValue("app", "show_dashboard_on_launch", "0");
        var importedData = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_2", "Bozena", "Osobni vozidla", "Veteran", "Skoda 100", "", "1974", "35", "", "09/2026", "05/2025", "10/2026")
            ]
        };
        importedData.Settings.SetValue("app", "show_dashboard_on_launch", "1");
        var backupService = new StubBackupService
        {
            ImportBundle = new VehimapBackupBundle(importedData, []),
            RestoreException = new IOException("restore failed")
        };
        var session = CreateSessionController(dataRoot, new StubLegacyDataStore(currentData), backupService: backupService);
        await session.LoadAsync(dataRoot.AppBasePath);

        await Assert.ThrowsAsync<IOException>(() => session.RestoreBackupAsync(@"C:\backups\vehimap.vehimapbak"));

        Assert.Equal("veh_1", Assert.Single(session.DataSet.Vehicles).Id);
        Assert.False(session.ReadSupportedSettings().ShowDashboardOnLaunch);
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
        var backupService = new StubBackupService
        {
            ExportResult = new BackupExportResult(string.Empty, 3, 1)
        };
        var dataStore = new StubLegacyDataStore(dataSet);
        var session = CreateSessionController(dataRoot, dataStore, backupService: backupService);
        await session.LoadAsync(dataRoot.AppBasePath);

        var result = await session.CreateAutomaticBackupAsync();

        Assert.True(result.Created);
        Assert.False(result.IsError);
        Assert.Equal(result.BackupPath, backupService.ExportedPath);
        Assert.Contains("Spravovaných příloh v záloze", result.Message);
        Assert.Contains("3", result.Message);
        Assert.Contains("Přeskočených chybějících spravovaných příloh", result.Message);
        Assert.False(string.IsNullOrWhiteSpace(dataStore.CurrentDataSet.Settings.GetValue("backups", "last_automatic_backup_stamp")));
        Assert.Equal(result.BackupPath, dataStore.CurrentDataSet.Settings.GetValue("backups", "last_automatic_backup_path"));
    }

    [Fact]
    public async Task Create_automatic_backup_async_rolls_back_metadata_when_settings_persist_fails()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "vehimap-session-test", Guid.NewGuid().ToString("N"));
        var dataRoot = new VehimapDataRoot(rootPath, Path.Combine(rootPath, "data"), true);
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
        var dataStore = new StubLegacyDataStore(dataSet)
        {
            SaveException = new IOException("settings.ini nelze zapsat.")
        };
        var session = CreateSessionController(dataRoot, dataStore, backupService: backupService);
        await session.LoadAsync(dataRoot.AppBasePath);

        var result = await session.CreateAutomaticBackupAsync();

        Assert.False(result.Created);
        Assert.True(result.IsError);
        Assert.Contains("settings.ini nelze zapsat", result.Message);
        Assert.True(File.Exists(backupService.ExportedPath));
        Assert.Equal(string.Empty, session.DataSet.Settings.GetValue("backups", "last_automatic_backup_stamp"));
        Assert.Equal(string.Empty, session.DataSet.Settings.GetValue("backups", "last_automatic_backup_path"));
    }

    [Fact]
    public async Task Run_automatic_backup_check_trims_old_files_when_backup_is_not_due()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "vehimap-session-test", Guid.NewGuid().ToString("N"));
        var dataRoot = new VehimapDataRoot(rootPath, Path.Combine(rootPath, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);
        var backupDirectory = Path.Combine(dataRoot.DataPath, "auto-backups");
        Directory.CreateDirectory(backupDirectory);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobni vozidla", "Rodinne auto", "Skoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        dataSet.Settings.SetValue("backups", "last_automatic_backup_stamp", DateTime.Now.ToString("yyyyMMddHHmmss"));
        File.WriteAllText(Path.Combine(backupDirectory, "Vehimap_auto_2026-04-01_10-00-00.vehimapbak"), "one");
        File.WriteAllText(Path.Combine(backupDirectory, "Vehimap_auto_2026-04-02_10-00-00.vehimapbak"), "two");
        File.WriteAllText(Path.Combine(backupDirectory, "Vehimap_auto_2026-04-03_10-00-00.vehimapbak"), "three");

        var session = CreateSessionController(dataRoot, new StubLegacyDataStore(dataSet));
        await session.LoadAsync(dataRoot.AppBasePath);
        await session.ApplySupportedSettingsAsync(new DesktopSupportedSettingsSnapshot(30, 30, 31, 1000, false, false, false, true, 7, 2));

        var result = await session.RunAutomaticBackupCheckAsync();

        Assert.False(result.Created);
        Assert.False(result.IsError);
        var remainingFiles = Directory.GetFiles(backupDirectory, "*.vehimapbak")
            .Select(Path.GetFileName)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "Vehimap_auto_2026-04-02_10-00-00.vehimapbak",
                "Vehimap_auto_2026-04-03_10-00-00.vehimapbak"
            },
            remainingFiles);
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

    private static void SeedNotificationHistory(VehimapSettings settings)
    {
        foreach (var key in NotificationHistoryKeys)
        {
            settings.SetValue("notifications", key, "seed");
        }
    }

    private static void AssertNotificationHistoryCleared(VehimapSettings settings)
    {
        foreach (var key in NotificationHistoryKeys)
        {
            Assert.Equal(string.Empty, settings.GetValue("notifications", key));
        }
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

        public Exception? SaveException { get; set; }

        public Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
            => Task.FromResult(CurrentDataSet);

        public Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
        {
            if (SaveException is not null)
            {
                throw SaveException;
            }

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

        public DateOnly? LastPeriodStart { get; private set; }

        public DateOnly? LastPeriodEnd { get; private set; }

        public CostAnalysisSummary BuildYearToDateSummary(VehimapDataSet dataSet, DateOnly today) => _summary;

        public CostAnalysisSummary BuildPeriodSummary(VehimapDataSet dataSet, DateOnly periodStart, DateOnly periodEnd)
        {
            LastPeriodStart = periodStart;
            LastPeriodEnd = periodEnd;
            return _summary;
        }
    }

    private sealed class StubBackupService : IBackupService
    {
        public string? ExportedPath { get; private set; }

        public VehimapBackupBundle ImportBundle { get; set; } = new(new VehimapDataSet(), []);

        public VehimapBackupBundle? RestoredBundle { get; private set; }

        public BackupRestoreResult RestoreResult { get; set; } = new(null, 0);

        public BackupExportResult ExportResult { get; set; } = new(string.Empty, 0, 0);

        public Exception? RestoreException { get; set; }

        public async Task<BackupExportResult> ExportAsync(string backupPath, VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
        {
            ExportedPath = backupPath;
            Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
            await File.WriteAllTextAsync(backupPath, "backup", cancellationToken);
            return ExportResult with { BackupPath = backupPath };
        }

        public Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default)
            => Task.FromResult(ImportBundle);

        public Task<BackupRestoreResult> RestoreAsync(VehimapDataRoot dataRoot, VehimapBackupBundle backupBundle, CancellationToken cancellationToken = default)
        {
            if (RestoreException is not null)
            {
                throw RestoreException;
            }

            RestoredBundle = backupBundle;
            return Task.FromResult(RestoreResult);
        }
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

        public bool LastSetEnabled { get; private set; }

        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default) => Task.FromResult(IsEnabledResult);

        public Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            LastSetEnabled = enabled;
            return Task.CompletedTask;
        }
    }
}
