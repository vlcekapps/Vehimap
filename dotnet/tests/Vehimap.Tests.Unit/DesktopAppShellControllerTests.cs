using Avalonia.Controls;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.Views;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopAppShellControllerTests
{
    [Fact]
    public async Task Open_settings_async_saves_snapshot_when_dialog_returns_value()
    {
        var dialogService = new StubAppShellDialogService
        {
            SettingsResult = new SettingsDialogResult(
                new DesktopSupportedSettingsSnapshot(45, 20, 10, 900, false, false, true, true, 2, 14),
                false)
        };
        var controller = new DesktopAppShellController(dialogService, new StubUpdateInstallLauncher());
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataStore = new StubLegacyDataStore(CreateDataSet());
        var viewModel = CreateViewModel(dataRoot, dataStore);

        await controller.OpenSettingsAsync(null!, viewModel);

        Assert.Equal("45", dataStore.CurrentDataSet.Settings.GetValue("notifications", "technical_reminder_days"));
        Assert.Equal("20", dataStore.CurrentDataSet.Settings.GetValue("notifications", "green_card_reminder_days"));
        Assert.Equal("1", dataStore.CurrentDataSet.Settings.GetValue("backups", "automatic_backups_enabled"));
        Assert.True(dialogService.ShowSettingsCalled);
    }

    [Fact]
    public async Task Open_about_async_opens_release_notes_when_dialog_requests_it()
    {
        var dialogService = new StubAppShellDialogService
        {
            AboutResult = true
        };
        var fileLauncher = new StubFileLauncher();
        var controller = new DesktopAppShellController(dialogService, new StubUpdateInstallLauncher());
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            fileLauncher: fileLauncher);

        await controller.OpenAboutAsync(null!, viewModel);

        Assert.Equal("https://example.com/release", fileLauncher.LastOpenedPath);
    }

    [Fact]
    public async Task Check_for_updates_async_launches_installer_and_requests_close_when_install_is_ready()
    {
        var dialogService = new StubAppShellDialogService
        {
            UpdateResult = UpdateDialogAction.PrimaryAction
        };
        var launcher = new StubUpdateInstallLauncher();
        var updateService = new StubUpdateService();
        var controller = new DesktopAppShellController(dialogService, launcher);
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            updateService: updateService);

        var shouldClose = await controller.CheckForUpdatesAsync(null!, viewModel);

        Assert.True(shouldClose);
        Assert.NotNull(launcher.LastPlan);
        Assert.Equal("1.0.9", launcher.LastPlan!.ExpectedVersion);
    }

    private static MainWindowViewModel CreateViewModel(
        VehimapDataRoot dataRoot,
        StubLegacyDataStore dataStore,
        StubFileLauncher? fileLauncher = null,
        IUpdateService? updateService = null)
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
            new StubBackupService(),
            new StubFileDialogService(),
            new DesktopSupportedSettingsService(),
            new StubBuildInfoProvider(),
            new StubAutostartService(),
            updateService,
            new DesktopProjectionService(),
            new DesktopNavigationCoordinator(),
            new StubAppShellDialogService(),
            new StubUpdateInstallLauncher());
    }

    private static VehimapDataSet CreateDataSet()
    {
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
        dataSet.Settings.SetValue("notifications", "technical_reminder_days", "30");
        dataSet.Settings.SetValue("notifications", "green_card_reminder_days", "15");
        dataSet.Settings.SetValue("notifications", "maintenance_reminder_days", "7");
        dataSet.Settings.SetValue("notifications", "maintenance_reminder_km", "500");
        dataSet.Settings.SetValue("app", "show_dashboard_on_launch", "0");
        return dataSet;
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

    private sealed class StubDataRootLocator : IDataRootLocator
    {
        private readonly VehimapDataRoot _dataRoot;

        public StubDataRootLocator(VehimapDataRoot dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public VehimapDataRoot Resolve(string appBasePath) => _dataRoot;
    }

    private sealed class StubFileLauncher : IFileLauncher
    {
        public string? LastOpenedPath { get; private set; }

        public Task OpenAsync(string path, CancellationToken cancellationToken = default)
        {
            LastOpenedPath = path;
            return Task.CompletedTask;
        }

        public Task OpenFolderAsync(string path, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubFilePickerService : IFilePickerService
    {
        public Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class StubTextFileSaveService : ITextFileSaveService
    {
        public Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default)
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
            true);
    }

    private sealed class StubBackupService : IBackupService
    {
        public Task ExportAsync(string backupPath, VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default)
            => Task.FromResult(new VehimapBackupBundle(new VehimapDataSet(), []));

        public Task RestoreAsync(VehimapDataRoot dataRoot, VehimapBackupBundle backupBundle, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StubUpdateService : IUpdateService
    {
        public Task<UpdateCheckResult> CheckForUpdatesAsync(string currentVersion, CancellationToken cancellationToken = default)
            => Task.FromResult(new UpdateCheckResult(
                currentVersion,
                "1.0.9",
                true,
                "2026-04-02",
                "https://example.com/release",
                "https://example.com/vehimap.zip",
                new string('a', 64),
                2048,
                true,
                "Je dostupná novější verze."));

        public Task<UpdateInstallResult> PrepareInstallAsync(UpdateCheckResult update, CancellationToken cancellationToken = default)
            => Task.FromResult(new UpdateInstallResult(
                true,
                "Aktualizace je připravená k instalaci.",
                new UpdateInstallPlan(
                    @"C:\vehimap\Vehimap.Updater.exe",
                    @"C:\vehimap\update-src",
                    @"C:\vehimap",
                    @"C:\vehimap\Vehimap.Desktop.exe",
                    1234,
                    update.LatestVersion)));
    }

    private sealed class StubAppShellDialogService : IAppShellDialogService
    {
        public SettingsDialogResult? SettingsResult { get; set; }
        public bool AboutResult { get; set; }
        public UpdateDialogAction UpdateResult { get; set; }
        public bool ShowSettingsCalled { get; private set; }

        public Task<SettingsDialogResult?> ShowSettingsAsync(Window owner, DesktopSupportedSettingsSnapshot snapshot, string automaticBackupStatus)
        {
            ShowSettingsCalled = true;
            return Task.FromResult(SettingsResult);
        }

        public Task<bool> ConfirmBackupImportAsync(Window owner, string backupPath) => Task.FromResult(true);

        public Task<bool> ShowAboutAsync(Window owner, AboutDialogViewModel model) => Task.FromResult(AboutResult);

        public Task<UpdateDialogAction> ShowUpdateAsync(Window owner, UpdateDialogViewModel model) => Task.FromResult(UpdateResult);
    }

    private sealed class StubUpdateInstallLauncher : IUpdateInstallLauncher
    {
        public UpdateInstallPlan? LastPlan { get; private set; }

        public void Launch(UpdateInstallPlan plan)
        {
            LastPlan = plan;
        }
    }

    private sealed class StubAutostartService : IAutostartService
    {
        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
