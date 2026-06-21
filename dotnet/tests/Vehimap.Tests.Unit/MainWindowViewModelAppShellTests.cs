using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class MainWindowViewModelAppShellTests
{
    [Fact]
    public void Minimize_to_tray_menu_state_is_exposed_by_view_model()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        var viewModel = CreateViewModel(dataRoot, new StubLegacyDataStore(dataSet));

        Assert.False(viewModel.IsMinimizeToTrayAvailable);

        viewModel.IsMinimizeToTrayAvailable = true;

        Assert.True(viewModel.IsMinimizeToTrayAvailable);
    }

    [Fact]
    public void Show_dashboard_on_launch_opens_dashboard_tab_after_load()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        dataSet.Settings.SetValue("app", "show_dashboard_on_launch", "1");

        var viewModel = CreateViewModel(dataRoot, new StubLegacyDataStore(dataSet));

        Assert.True(viewModel.IsDashboardTabSelected);
    }

    [Fact]
    public void Hide_on_launch_suppresses_dashboard_auto_open()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        dataSet.Settings.SetValue("app", "show_dashboard_on_launch", "1");
        dataSet.Settings.SetValue("app", "hide_on_launch", "1");

        var viewModel = CreateViewModel(dataRoot, new StubLegacyDataStore(dataSet));

        Assert.False(viewModel.IsDashboardTabSelected);
        Assert.True(viewModel.IsDetailTabSelected);
    }

    [Fact]
    public void Dashboard_workspace_syncs_show_on_launch_from_settings()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        dataSet.Settings.SetValue("app", "show_dashboard_on_launch", "1");

        var viewModel = CreateViewModel(dataRoot, new StubLegacyDataStore(dataSet));

        Assert.True(viewModel.DashboardWorkspace.ShowDashboardOnLaunch);
    }

    [Fact]
    public async Task Dashboard_workspace_show_on_launch_toggle_persists_supported_setting()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        dataSet.Settings.SetValue("app", "show_dashboard_on_launch", "0");
        dataSet.Settings.SetValue("app", "hide_on_launch", "1");
        var dataStore = new StubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        await viewModel.SetDashboardShowOnLaunchAsync(true);

        Assert.True(viewModel.DashboardWorkspace.ShowDashboardOnLaunch);
        Assert.Equal("1", dataStore.CurrentDataSet.Settings.GetValue("app", "show_dashboard_on_launch"));
        Assert.Equal("1", dataStore.CurrentDataSet.Settings.GetValue("app", "hide_on_launch"));
        Assert.Contains("Dashboard se bude zobrazovat", viewModel.ShellStatus);
    }

    [Fact]
    public async Task Saving_supported_settings_persists_values_and_keeps_other_keys()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        dataSet.Settings.SetValue("app", "hide_on_launch", "1");
        var dataStore = new StubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        await viewModel.SaveSupportedSettingsAsync(new DesktopSupportedSettingsSnapshot(60, 25, 14, 1500, false, true, true, false, 1, 30));

        Assert.Equal("60", dataStore.CurrentDataSet.Settings.GetValue("notifications", "technical_reminder_days"));
        Assert.Equal("25", dataStore.CurrentDataSet.Settings.GetValue("notifications", "green_card_reminder_days"));
        Assert.Equal("14", dataStore.CurrentDataSet.Settings.GetValue("notifications", "maintenance_reminder_days"));
        Assert.Equal("1500", dataStore.CurrentDataSet.Settings.GetValue("notifications", "maintenance_reminder_km"));
        Assert.Equal("1", dataStore.CurrentDataSet.Settings.GetValue("app", "show_dashboard_on_launch"));
        Assert.Equal("1", dataStore.CurrentDataSet.Settings.GetValue("app", "hide_on_launch"));
    }

    [Fact]
    public async Task Export_backup_uses_backup_service_and_reports_status()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        var backupService = new StubBackupService
        {
            ExportResult = new BackupExportResult(string.Empty, 2, 1)
        };
        var viewModel = CreateViewModel(dataRoot, new StubLegacyDataStore(dataSet), backupService: backupService);

        var status = await viewModel.ExportBackupAsync(@"C:\backups\vehimap.vehimapbak");

        Assert.Equal(@"C:\backups\vehimap.vehimapbak", backupService.ExportedPath);
        Assert.Contains("Záloha byla uložena", status);
        Assert.Contains("Spravovaných příloh v záloze: 2", status);
        Assert.Contains("Přeskočených chybějících spravovaných příloh: 1", status);
        Assert.Equal(status, viewModel.ShellStatus);
    }

    [Fact]
    public async Task Import_backup_restores_bundle_and_reports_status()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        var importedData = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_9", "Božena", "Osobní vozidla", "Srazové", "Škoda 100", "2AB3456", "1973", "35", "", "09/2026", "", "10/2026")
            ]
        };
        var dataStore = new StubLegacyDataStore(dataSet);
        var backupService = new StubBackupService
        {
            ImportBundle = new VehimapBackupBundle(importedData, []),
            RestoreCallback = bundle => dataStore.CurrentDataSet = bundle.Data,
            RestoreResult = new BackupRestoreResult(@"C:\vehimap-test\data\import-backups\2026-04-02_10-00-00", 2)
        };
        var viewModel = CreateViewModel(dataRoot, dataStore, backupService: backupService);

        var status = await viewModel.ImportBackupAsync(@"C:\backups\vehimap.vehimapbak");

        Assert.Equal(@"C:\backups\vehimap.vehimapbak", backupService.ImportedPath);
        Assert.Equal(@"C:\backups\vehimap.vehimapbak", backupService.RestoredFromPath);
        Assert.Equal("Božena", viewModel.SelectedVehicle?.Name);
        Assert.Contains("Data byla obnovena ze zálohy", status);
        Assert.Contains(@"C:\vehimap-test\data\import-backups\2026-04-02_10-00-00", status);
        Assert.Contains("Obnoveno spravovaných příloh: 2", status);
        Assert.Equal(status, viewModel.ShellStatus);
    }

    [Fact]
    public async Task Open_data_folder_uses_current_data_root_and_reports_status()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "vehimap-open-data-folder-test", Guid.NewGuid().ToString("N"));
        var dataPath = Path.Combine(rootPath, "data");
        var dataRoot = new VehimapDataRoot(rootPath, dataPath, true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        var fileLauncher = new StubFileLauncher();
        var viewModel = CreateViewModel(dataRoot, new StubLegacyDataStore(dataSet), fileLauncher: fileLauncher);

        Assert.True(viewModel.CanOpenDataFolder);

        var status = await viewModel.OpenDataFolderAsync();

        Assert.Equal(dataPath, fileLauncher.LastOpenedFolderPath);
        Assert.True(Directory.Exists(dataPath));
        Assert.Contains("Datová složka byla otevřena", status);
        Assert.Contains(dataPath, status);
        Assert.Equal(status, viewModel.ShellStatus);
    }

    [Fact]
    public void Background_snapshot_reports_overdue_technical_control_even_without_future_dashboard_items()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "03/2026", "05/2025", "06/2026")
            ]
        };
        dataSet.Settings.SetValue("notifications", "technical_reminder_days", "30");
        dataSet.Settings.SetValue("notifications", "green_card_reminder_days", "30");
        dataSet.Settings.SetValue("notifications", "maintenance_reminder_days", "31");
        dataSet.Settings.SetValue("notifications", "maintenance_reminder_km", "1000");

        var viewModel = CreateViewModel(dataRoot, new StubLegacyDataStore(dataSet));

        var snapshot = viewModel.BuildBackgroundSnapshot();

        Assert.True(snapshot.HasNotification);
        Assert.StartsWith("timeline|", snapshot.NotificationKey, StringComparison.Ordinal);
        Assert.Contains("Milena", snapshot.NotificationMessage, StringComparison.Ordinal);
        Assert.Contains("Po termínu", snapshot.NotificationMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void Background_snapshot_prioritizes_due_timeline_before_general_audit_items()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "", "1988", "43", "", "01/2000", "05/2025", "12/2099")
            ]
        };
        dataSet.Settings.SetValue("notifications", "technical_reminder_days", "30");
        dataSet.Settings.SetValue("notifications", "green_card_reminder_days", "30");
        dataSet.Settings.SetValue("notifications", "maintenance_reminder_days", "31");
        dataSet.Settings.SetValue("notifications", "maintenance_reminder_km", "1000");

        var viewModel = CreateViewModel(dataRoot, new StubLegacyDataStore(dataSet));

        var snapshot = viewModel.BuildBackgroundSnapshot();

        Assert.True(snapshot.HasNotification);
        Assert.StartsWith("timeline|", snapshot.NotificationKey, StringComparison.Ordinal);
        Assert.Contains("termínů k řešení", snapshot.NotificationTitle, StringComparison.Ordinal);
        Assert.Contains("Milena", snapshot.NotificationMessage, StringComparison.Ordinal);
        Assert.Contains("Po termínu", snapshot.NotificationMessage, StringComparison.Ordinal);
        Assert.Contains("Nejbližší k řešení", snapshot.ToolTipText, StringComparison.Ordinal);
        Assert.Contains("Po termínu", snapshot.ToolTipText, StringComparison.Ordinal);
    }

    [Fact]
    public void Background_snapshot_uses_audit_notification_when_no_due_timeline_item_exists()
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "", "1988", "43", "", "12/2099", "05/2025", "12/2099")
            ]
        };
        dataSet.Settings.SetValue("notifications", "technical_reminder_days", "30");
        dataSet.Settings.SetValue("notifications", "green_card_reminder_days", "30");
        dataSet.Settings.SetValue("notifications", "maintenance_reminder_days", "31");
        dataSet.Settings.SetValue("notifications", "maintenance_reminder_km", "1000");

        var viewModel = CreateViewModel(dataRoot, new StubLegacyDataStore(dataSet));

        var snapshot = viewModel.BuildBackgroundSnapshot();

        Assert.True(snapshot.HasNotification);
        Assert.StartsWith("audit|", snapshot.NotificationKey, StringComparison.Ordinal);
        Assert.Contains("položek k řešení", snapshot.NotificationTitle, StringComparison.Ordinal);
        Assert.Contains("Chybí SPZ", snapshot.NotificationMessage, StringComparison.Ordinal);
    }

    private static MainWindowViewModel CreateViewModel(
        VehimapDataRoot dataRoot,
        StubLegacyDataStore dataStore,
        IBackupService? backupService = null,
        IFileLauncher? fileLauncher = null)
    {
        var bootstrapper = new LegacyVehimapBootstrapper(new StubDataRootLocator(dataRoot), dataStore);
        return new MainWindowViewModel(
            dataStore,
            bootstrapper,
            new ManagedAttachmentPathService(),
            fileLauncher ?? new StubFileLauncher(),
            new StubFilePickerService(),
            new LegacyGlobalSearchService(new ManagedAttachmentPathService()),
            new LegacyTimelineService(),
            new LegacyCalendarExportService(),
            new StubTextFileSaveService(),
            backupService ?? new StubBackupService(),
            new StubFileDialogService(),
            new DesktopSupportedSettingsService(),
            new StubBuildInfoProvider(),
            new StubAutostartService());
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

    private sealed class StubFileLauncher : IFileLauncher
    {
        public string? LastOpenedFolderPath { get; private set; }

        public Task OpenAsync(string path, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task OpenFolderAsync(string path, CancellationToken cancellationToken = default)
        {
            LastOpenedFolderPath = path;
            return Task.CompletedTask;
        }
    }

    private sealed class StubTextFileSaveService : ITextFileSaveService
    {
        public Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class StubFilePickerService : IFilePickerService
    {
        public Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class StubFileDialogService : IFileDialogService
    {
        public Task<string?> PickOpenFileAsync(string title, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);

        public Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class StubBuildInfoProvider : IAppBuildInfoProvider
    {
        public AppBuildInfo GetCurrent() => new(
            "Vehimap",
            "1.0.2",
            "1.0.2.0",
            "vývojový Avalonia shell",
            @"C:\vehimap\Vehimap.Desktop.exe",
            "Windows",
            ".NET 10",
            "https://example.com/latest.ini",
            "https://example.com/release",
            @"C:\vehimap\Vehimap.Updater.exe",
            false);
    }

    private sealed class StubAutostartService : IAutostartService
    {
        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubBackupService : IBackupService
    {
        public string? ExportedPath { get; private set; }
        public string? ImportedPath { get; private set; }
        public string? RestoredFromPath { get; private set; }
        public VehimapBackupBundle ImportBundle { get; set; } = new(new VehimapDataSet(), []);
        public BackupRestoreResult RestoreResult { get; set; } = new(null, 0);
        public Action<VehimapBackupBundle>? RestoreCallback { get; set; }

        public BackupExportResult ExportResult { get; set; } = new(string.Empty, 0, 0);

        public Task<BackupExportResult> ExportAsync(string backupPath, VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
        {
            ExportedPath = backupPath;
            return Task.FromResult(ExportResult with { BackupPath = backupPath });
        }

        public Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default)
        {
            ImportedPath = backupPath;
            return Task.FromResult(ImportBundle);
        }

        public Task<BackupRestoreResult> RestoreAsync(VehimapDataRoot dataRoot, VehimapBackupBundle backupBundle, CancellationToken cancellationToken = default)
        {
            RestoredFromPath = ImportedPath;
            RestoreCallback?.Invoke(backupBundle);
            return Task.FromResult(RestoreResult);
        }
    }
}
