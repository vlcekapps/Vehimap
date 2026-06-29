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
    public async Task Open_settings_async_creates_automatic_backup_when_dialog_requests_it()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "vehimap-settings-backup-test", Guid.NewGuid().ToString("N"));
        var dataRoot = new VehimapDataRoot(rootPath, Path.Combine(rootPath, "data"), true);
        var backupService = new StubBackupService();
        var dialogService = new StubAppShellDialogService
        {
            SettingsResult = new SettingsDialogResult(
                new DesktopSupportedSettingsSnapshot(45, 20, 10, 900, false, false, true, true, 2, 14),
                true)
        };
        var controller = new DesktopAppShellController(dialogService, new StubUpdateInstallLauncher());
        var dataStore = new StubLegacyDataStore(CreateDataSet());
        var viewModel = CreateViewModel(dataRoot, dataStore, backupService: backupService);

        try
        {
            await controller.OpenSettingsAsync(null!, viewModel);

            Assert.True(dialogService.ShowSettingsCalled);
            Assert.NotNull(backupService.ExportedPath);
            Assert.StartsWith(Path.Combine(dataRoot.DataPath, "auto-backups"), backupService.ExportedPath, StringComparison.Ordinal);
            Assert.EndsWith(".vehimapbak", backupService.ExportedPath, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("1", dataStore.CurrentDataSet.Settings.GetValue("backups", "automatic_backups_enabled"));
            Assert.Equal("2", dataStore.CurrentDataSet.Settings.GetValue("backups", "automatic_backup_interval_days"));
            Assert.Equal("14", dataStore.CurrentDataSet.Settings.GetValue("backups", "automatic_backup_keep_count"));
            Assert.Equal(backupService.ExportedPath, dataStore.CurrentDataSet.Settings.GetValue("backups", "last_automatic_backup_path"));
            Assert.Contains("Automatick", viewModel.ShellStatus, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Open_settings_async_reports_save_failure_without_throwing()
    {
        var dialogService = new StubAppShellDialogService
        {
            SettingsResult = new SettingsDialogResult(
                new DesktopSupportedSettingsSnapshot(45, 20, 10, 900, false, false, true, true, 2, 14),
                false)
        };
        var controller = new DesktopAppShellController(dialogService, new StubUpdateInstallLauncher());
        var dataStore = new StubLegacyDataStore(CreateDataSet())
        {
            SaveException = new IOException("settings.ini nelze zapsat.")
        };
        var viewModel = CreateViewModel(new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true), dataStore);

        await controller.OpenSettingsAsync(null!, viewModel);

        Assert.Contains("Nastavení se nepodařilo dokončit", viewModel.ShellStatus);
        Assert.Contains("settings.ini nelze zapsat", viewModel.ShellStatus);
    }

    [Fact]
    public async Task Open_about_async_opens_release_notes_when_dialog_requests_it()
    {
        var dialogService = new StubAppShellDialogService
        {
            AboutResult = AboutDialogAction.OpenReleaseNotes
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
    public async Task Open_about_async_reports_release_notes_failure_without_throwing()
    {
        var dialogService = new StubAppShellDialogService
        {
            AboutResult = AboutDialogAction.OpenReleaseNotes
        };
        var controller = new DesktopAppShellController(dialogService, new StubUpdateInstallLauncher());
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            fileLauncher: new FailingFileLauncher(new InvalidOperationException("Prohlížeč není dostupný.")));

        await controller.OpenAboutAsync(null!, viewModel);

        Assert.Contains("Externí odkaz se nepodařilo otevřít", viewModel.ShellStatus);
        Assert.Contains("Prohlížeč není dostupný", viewModel.ShellStatus);
    }

    [Fact]
    public async Task Open_about_async_opens_author_support_when_dialog_requests_it()
    {
        var dialogService = new StubAppShellDialogService
        {
            AboutResult = AboutDialogAction.ThankAuthor
        };
        var fileLauncher = new StubFileLauncher();
        var controller = new DesktopAppShellController(dialogService, new StubUpdateInstallLauncher());
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            fileLauncher: fileLauncher);

        await controller.OpenAboutAsync(null!, viewModel);

        Assert.Equal(AboutDialogViewModel.AuthorSupportUrl, fileLauncher.LastOpenedPath);
    }

    [Fact]
    public async Task Open_author_support_async_opens_thank_author_page()
    {
        var fileLauncher = new StubFileLauncher();
        var controller = new DesktopAppShellController(new StubAppShellDialogService(), new StubUpdateInstallLauncher());
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            fileLauncher: fileLauncher);

        await controller.OpenAuthorSupportAsync(viewModel);

        Assert.Equal(AboutDialogViewModel.AuthorSupportUrl, fileLauncher.LastOpenedPath);
    }

    [Fact]
    public async Task Open_feedback_issue_async_opens_prefilled_github_issue()
    {
        var fileLauncher = new StubFileLauncher();
        var controller = new DesktopAppShellController(new StubAppShellDialogService(), new StubUpdateInstallLauncher());
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            fileLauncher: fileLauncher);

        await controller.OpenFeedbackIssueAsync(viewModel);

        Assert.NotNull(fileLauncher.LastOpenedPath);
        Assert.StartsWith(FeedbackIssueUrlBuilder.IssueBaseUrl, fileLauncher.LastOpenedPath, StringComparison.Ordinal);
        Assert.Contains("title=", fileLauncher.LastOpenedPath, StringComparison.Ordinal);
        Assert.Contains("body=", fileLauncher.LastOpenedPath, StringComparison.Ordinal);
        Assert.Contains("Externí odkaz byl otevřen", viewModel.ShellStatus, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Open_printable_report_async_creates_html_and_opens_it()
    {
        var controller = new DesktopAppShellController(new StubAppShellDialogService(), new StubUpdateInstallLauncher());
        var fileLauncher = new StubFileLauncher();
        var reportPath = Path.Combine(Path.GetTempPath(), $"vehimap-report-{Guid.NewGuid():N}.html");
        var textFileSaveService = new CapturingTextFileSaveService(reportPath);
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            fileLauncher: fileLauncher,
            textFileSaveService: textFileSaveService);

        await controller.OpenPrintableReportAsync(viewModel);

        Assert.Equal(reportPath, fileLauncher.LastOpenedPath);
        Assert.Equal("Uložit tiskový přehled vozidel", textFileSaveService.LastTitle);
        Assert.Equal("HTML soubor", textFileSaveService.LastFileTypeName);
        Assert.Equal("html", textFileSaveService.LastDefaultExtension);
        Assert.Contains("*.html", textFileSaveService.LastPatterns);
        Assert.Contains("vehimap-tiskovy-prehled-", textFileSaveService.LastSuggestedFileName);
        Assert.EndsWith(".html", textFileSaveService.LastSuggestedFileName, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Vehimap - Tiskový přehled vozidel", textFileSaveService.LastContent);
        Assert.Contains(reportPath, viewModel.ShellStatus);
    }

    [Fact]
    public async Task Open_printable_report_async_reports_cancelled_save()
    {
        var controller = new DesktopAppShellController(new StubAppShellDialogService(), new StubUpdateInstallLauncher());
        var fileLauncher = new StubFileLauncher();
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            fileLauncher: fileLauncher);

        await controller.OpenPrintableReportAsync(viewModel);

        Assert.Null(fileLauncher.LastOpenedPath);
        Assert.Equal("Uložení tiskového přehledu bylo zrušeno.", viewModel.ShellStatus);
    }

    [Fact]
    public async Task Export_backup_async_reports_cancelled_file_picker()
    {
        var controller = new DesktopAppShellController(new StubAppShellDialogService(), new StubUpdateInstallLauncher());
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()));

        await controller.ExportBackupAsync(null!, viewModel);

        Assert.Equal("Export zálohy byl zrušen.", viewModel.ShellStatus);
    }

    [Fact]
    public async Task Import_backup_async_reports_cancelled_file_picker()
    {
        var dialogService = new StubAppShellDialogService();
        var controller = new DesktopAppShellController(dialogService, new StubUpdateInstallLauncher());
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()));

        await controller.ImportBackupAsync(null!, viewModel);

        Assert.Equal("Obnova ze zálohy byla zrušena.", viewModel.ShellStatus);
        Assert.False(dialogService.ConfirmBackupImportCalled);
    }

    [Fact]
    public async Task Check_for_updates_async_launches_installer_and_requests_close_when_install_is_ready()
    {
        var dialogService = new StubAppShellDialogService
        {
            UpdateResult = UpdateDialogAction.PrimaryAction,
            ConfirmDiscardResult = true
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
        Assert.True(dialogService.ShowUpdateInstallProgressCalled);
        Assert.NotNull(updateService.LastProgress);
        Assert.NotNull(launcher.LastPlan);
        Assert.Equal("1.0.9", launcher.LastPlan!.ExpectedVersion);
    }

    [Fact]
    public async Task Check_for_updates_async_can_cancel_download_without_launching_installer()
    {
        var dialogService = new StubAppShellDialogService
        {
            UpdateResult = UpdateDialogAction.PrimaryAction,
            ConfirmDiscardResult = true,
            ProgressResult = new UpdateInstallResult(false, "Stahování aktualizace bylo zrušeno.", null)
        };
        var launcher = new StubUpdateInstallLauncher();
        var controller = new DesktopAppShellController(dialogService, launcher);
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            updateService: new StubUpdateService());

        var shouldClose = await controller.CheckForUpdatesAsync(null!, viewModel);

        Assert.False(shouldClose);
        Assert.True(dialogService.ShowUpdateInstallProgressCalled);
        Assert.Null(launcher.LastPlan);
        Assert.Contains("zrušeno", viewModel.ShellStatus, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Installer_update_launcher_uses_interactive_inno_close_arguments()
    {
        var installerPath = Path.Combine(Path.GetTempPath(), "vehimap-update-setup.exe");
        var plan = new UpdateInstallPlan(
            installerPath,
            Path.GetTempPath(),
            AppContext.BaseDirectory,
            Path.Combine(AppContext.BaseDirectory, "Vehimap.Desktop.exe"),
            1234,
            "1.0.9",
            "installer",
            installerPath);

        var startInfo = ProcessUpdateInstallLauncher.BuildStartInfo(plan);

        Assert.Equal(installerPath, startInfo.FileName);
        Assert.True(startInfo.UseShellExecute);
        Assert.Equal(["/CLOSEAPPLICATIONS", "/NORESTARTAPPLICATIONS"], startInfo.ArgumentList);
    }

    [Fact]
    public async Task Check_for_updates_async_shows_failure_result_when_check_throws()
    {
        var dialogService = new StubAppShellDialogService();
        var controller = new DesktopAppShellController(dialogService, new StubUpdateInstallLauncher());
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            updateService: new FailingUpdateService(checkException: new InvalidOperationException("Manifest nejde načíst.")));

        var shouldClose = await controller.CheckForUpdatesAsync(null!, viewModel);

        Assert.False(shouldClose);
        Assert.NotNull(dialogService.LastUpdateModel);
        Assert.Equal("Kontrola aktualizací se nepodařila", dialogService.LastUpdateModel!.Heading);
        Assert.Contains("Manifest nejde načíst", dialogService.LastUpdateModel.Summary);
        Assert.Equal(dialogService.LastUpdateModel.Summary, viewModel.ShellStatus);
    }

    [Fact]
    public async Task Check_for_updates_async_reports_launcher_failure_without_throwing()
    {
        var dialogService = new StubAppShellDialogService
        {
            UpdateResult = UpdateDialogAction.PrimaryAction,
            ConfirmDiscardResult = true
        };
        var launcher = new FailingUpdateInstallLauncher(new InvalidOperationException("Updater nelze spustit."));
        var controller = new DesktopAppShellController(dialogService, launcher);
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            updateService: new StubUpdateService());

        var shouldClose = await controller.CheckForUpdatesAsync(null!, viewModel);

        Assert.False(shouldClose);
        Assert.Contains("Kontrolu aktualizací se nepodařilo dokončit", viewModel.ShellStatus);
        Assert.Contains("Updater nelze spustit", viewModel.ShellStatus);
    }

    [Fact]
    public async Task Import_backup_async_stops_when_pending_edits_are_not_discarded()
    {
        var dialogService = new StubAppShellDialogService
        {
            ConfirmDiscardResult = false
        };
        var controller = new DesktopAppShellController(dialogService, new StubUpdateInstallLauncher());
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()));

        viewModel.CreateReminderCommand.Execute(null);

        await controller.ImportBackupAsync(null!, viewModel);

        Assert.True(dialogService.ConfirmDiscardPendingChangesCalled);
        Assert.False(dialogService.ConfirmBackupImportCalled);
    }

    [Fact]
    public async Task Check_for_updates_async_does_not_install_when_pending_edits_are_not_discarded()
    {
        var dialogService = new StubAppShellDialogService
        {
            UpdateResult = UpdateDialogAction.PrimaryAction,
            ConfirmDiscardResult = false
        };
        var launcher = new StubUpdateInstallLauncher();
        var controller = new DesktopAppShellController(dialogService, launcher);
        var viewModel = CreateViewModel(
            new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true),
            new StubLegacyDataStore(CreateDataSet()),
            updateService: new StubUpdateService());

        viewModel.CreateReminderCommand.Execute(null);

        var shouldClose = await controller.CheckForUpdatesAsync(null!, viewModel);

        Assert.False(shouldClose);
        Assert.True(dialogService.ConfirmDiscardPendingChangesCalled);
        Assert.Null(launcher.LastPlan);
    }

    private static MainWindowViewModel CreateViewModel(
        VehimapDataRoot dataRoot,
        StubLegacyDataStore dataStore,
        IFileLauncher? fileLauncher = null,
        ITextFileSaveService? textFileSaveService = null,
        IUpdateService? updateService = null,
        IFileDialogService? fileDialogService = null,
        IBackupService? backupService = null)
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
            textFileSaveService ?? new StubTextFileSaveService(),
            backupService ?? new StubBackupService(),
            fileDialogService ?? new StubFileDialogService(),
            new DesktopSupportedSettingsService(),
            new StubBuildInfoProvider(),
            new StubAutostartService(),
            updateService,
            new DesktopProjectionService(),
            new DesktopNavigationCoordinator(),
            new DesktopPrintableVehicleReportService(),
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

    private sealed class FailingFileLauncher : IFileLauncher
    {
        private readonly Exception _exception;

        public FailingFileLauncher(Exception exception)
        {
            _exception = exception;
        }

        public Task OpenAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromException(_exception);

        public Task OpenFolderAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromException(_exception);
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

    private sealed class CapturingTextFileSaveService : ITextFileSaveService
    {
        private readonly string _savedPath;

        public CapturingTextFileSaveService(string savedPath)
        {
            _savedPath = savedPath;
        }

        public string LastTitle { get; private set; } = string.Empty;

        public string LastSuggestedFileName { get; private set; } = string.Empty;

        public string LastContent { get; private set; } = string.Empty;

        public string LastFileTypeName { get; private set; } = string.Empty;

        public string LastDefaultExtension { get; private set; } = string.Empty;

        public IReadOnlyList<string> LastPatterns { get; private set; } = [];

        public Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default)
        {
            LastTitle = title;
            LastSuggestedFileName = suggestedFileName;
            LastContent = content;
            return Task.FromResult<string?>(_savedPath);
        }

        public Task<string?> SaveTextAsync(
            string title,
            string suggestedFileName,
            string content,
            string fileTypeName,
            string defaultExtension,
            IReadOnlyList<string> patterns,
            CancellationToken cancellationToken = default)
        {
            LastTitle = title;
            LastSuggestedFileName = suggestedFileName;
            LastContent = content;
            LastFileTypeName = fileTypeName;
            LastDefaultExtension = defaultExtension;
            LastPatterns = patterns;
            return Task.FromResult<string?>(_savedPath);
        }
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
        public string? ExportedPath { get; private set; }

        public Task<BackupExportResult> ExportAsync(string backupPath, VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
        {
            ExportedPath = backupPath;
            return Task.FromResult(new BackupExportResult(backupPath, 0, 0));
        }

        public Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default)
            => Task.FromResult(new VehimapBackupBundle(new VehimapDataSet(), []));

        public Task<BackupRestoreResult> RestoreAsync(VehimapDataRoot dataRoot, VehimapBackupBundle backupBundle, CancellationToken cancellationToken = default)
            => Task.FromResult(new BackupRestoreResult(null, 0));
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

        public IProgress<UpdateInstallProgress>? LastProgress { get; private set; }

        public Task<UpdateInstallResult> PrepareInstallAsync(
            UpdateCheckResult update,
            IProgress<UpdateInstallProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            LastProgress = progress;
            progress?.Report(new UpdateInstallProgress("Stahuji testovací aktualizaci.", 2048, 2048));
            return Task.FromResult(new UpdateInstallResult(
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
    }

    private sealed class FailingUpdateService : IUpdateService
    {
        private readonly Exception? _checkException;
        private readonly Exception? _prepareException;

        public FailingUpdateService(Exception? checkException = null, Exception? prepareException = null)
        {
            _checkException = checkException;
            _prepareException = prepareException;
        }

        public Task<UpdateCheckResult> CheckForUpdatesAsync(string currentVersion, CancellationToken cancellationToken = default)
            => _checkException is not null
                ? Task.FromException<UpdateCheckResult>(_checkException)
                : Task.FromResult(new UpdateCheckResult(
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

        public Task<UpdateInstallResult> PrepareInstallAsync(
            UpdateCheckResult update,
            IProgress<UpdateInstallProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => _prepareException is not null
                ? Task.FromException<UpdateInstallResult>(_prepareException)
                : Task.FromResult(new UpdateInstallResult(false, "Testovací aktualizace není připravená.", null));
    }

    private sealed class StubAppShellDialogService : IAppShellDialogService
    {
        public SettingsDialogResult? SettingsResult { get; set; }
        public AboutDialogAction AboutResult { get; set; }
        public UpdateDialogAction UpdateResult { get; set; }
        public bool ConfirmDiscardResult { get; set; } = true;
        public bool ShowSettingsCalled { get; private set; }
        public bool ConfirmBackupImportCalled { get; private set; }
        public bool ConfirmDiscardPendingChangesCalled { get; private set; }
        public bool ShowUpdateInstallProgressCalled { get; private set; }
        public bool ShowDataStoreHealthCalled { get; private set; }
        public UpdateDialogViewModel? LastUpdateModel { get; private set; }
        public UpdateInstallResult? ProgressResult { get; set; }

        public Task<SettingsDialogResult?> ShowSettingsAsync(Window owner, DesktopSupportedSettingsSnapshot snapshot, string automaticBackupStatus)
        {
            ShowSettingsCalled = true;
            return Task.FromResult(SettingsResult);
        }

        public Task<bool> ConfirmBackupImportAsync(Window owner, string backupPath)
        {
            ConfirmBackupImportCalled = true;
            return Task.FromResult(true);
        }

        public Task<bool> ConfirmDiscardPendingChangesAsync(Window owner, string pendingEditLabel, string actionDescription)
        {
            ConfirmDiscardPendingChangesCalled = true;
            return Task.FromResult(ConfirmDiscardResult);
        }

        public Task<AboutDialogAction> ShowAboutAsync(Window owner, AboutDialogViewModel model) => Task.FromResult(AboutResult);

        public Task<DataStoreHealthDialogAction> ShowDataStoreHealthAsync(Window owner, DataStoreHealthDialogViewModel model)
        {
            ShowDataStoreHealthCalled = true;
            return Task.FromResult(DataStoreHealthDialogAction.None);
        }

        public Task<UpdateDialogAction> ShowUpdateAsync(Window owner, UpdateDialogViewModel model)
        {
            LastUpdateModel = model;
            return Task.FromResult(UpdateResult);
        }

        public async Task<UpdateInstallResult> ShowUpdateInstallProgressAsync(
            Window owner,
            UpdateInstallProgressDialogViewModel model,
            Func<IProgress<UpdateInstallProgress>, CancellationToken, Task<UpdateInstallResult>> prepareInstallAsync)
        {
            ShowUpdateInstallProgressCalled = true;
            if (ProgressResult is not null)
            {
                return ProgressResult;
            }

            return await prepareInstallAsync(new Progress<UpdateInstallProgress>(model.ApplyProgress), CancellationToken.None);
        }

        public Task<TrayActionsDialogAction> ShowTrayActionsAsync(Window? owner, TrayActionsDialogViewModel model)
            => Task.FromResult(TrayActionsDialogAction.None);
    }

    private sealed class StubUpdateInstallLauncher : IUpdateInstallLauncher
    {
        public UpdateInstallPlan? LastPlan { get; private set; }

        public void Launch(UpdateInstallPlan plan)
        {
            LastPlan = plan;
        }
    }

    private sealed class FailingUpdateInstallLauncher : IUpdateInstallLauncher
    {
        private readonly Exception _exception;

        public FailingUpdateInstallLauncher(Exception exception)
        {
            _exception = exception;
        }

        public void Launch(UpdateInstallPlan plan)
        {
            throw _exception;
        }
    }

    private sealed class StubAutostartService : IAutostartService
    {
        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
