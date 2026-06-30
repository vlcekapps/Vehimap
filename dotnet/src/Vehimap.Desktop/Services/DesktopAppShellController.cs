using Avalonia.Controls;
using Vehimap.Application;
using Vehimap.Application.Models;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopAppShellController
{
    private readonly IAppShellDialogService _dialogService;
    private readonly IUpdateInstallLauncher _updateInstallLauncher;

    public DesktopAppShellController(
        IAppShellDialogService dialogService,
        IUpdateInstallLauncher updateInstallLauncher)
    {
        _dialogService = dialogService;
        _updateInstallLauncher = updateInstallLauncher;
    }

    public async Task OpenSettingsAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _dialogService
                .ShowSettingsAsync(owner, shell.GetSupportedSettingsSnapshot(), shell.GetAutomaticBackupStatusText())
                .ConfigureAwait(true);
            if (result is null)
            {
                shell.ShellStatus = DesktopLocalization.Localizer.GetString("Shell.SettingsCanceled");
                return;
            }

            await shell.SaveSupportedSettingsAsync(result.Snapshot).ConfigureAwait(true);
            if (result.CreateBackupNow)
            {
                await shell.CreateAutomaticBackupNowAsync(cancellationToken).ConfigureAwait(true);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = DesktopLocalization.Localizer.Format("Shell.SettingsFailed", ex.Message);
        }
    }

    public async Task ExportBackupAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        try
        {
            var backupPath = await shell.PickBackupExportPathAsync(cancellationToken).ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(backupPath))
            {
                shell.ShellStatus = L("AppShell.Controller.ExportBackupCancelled");
                return;
            }

            await shell.ExportBackupAsync(backupPath, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = LF("AppShell.Controller.ExportBackupStartFailed", ex.Message);
        }
    }

    public Task OpenPrintableReportAsync(MainWindowViewModel shell, CancellationToken cancellationToken = default) =>
        shell.OpenPrintableVehicleReportAsync(cancellationToken);

    public async Task ExportVehiclePackageAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        try
        {
            var packagePath = await shell.PickVehiclePackageExportPathAsync(cancellationToken).ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(packagePath))
            {
                shell.ShellStatus = L("AppShell.Controller.ExportVehiclePackageCancelled");
                return;
            }

            await shell.ExportSelectedVehiclePackageAsync(packagePath, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = LF("AppShell.Controller.ExportVehiclePackageStartFailed", ex.Message);
        }
    }

    public async Task ImportVehiclePackageAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await ConfirmDiscardPendingChangesAsync(owner, shell, L("AppShell.Controller.ImportVehiclePackageAction")).ConfigureAwait(true))
            {
                shell.RequestWorkspaceFocus(shell.GetPendingEditFocusTarget());
                return;
            }

            var packagePath = await shell.PickVehiclePackageImportPathAsync(cancellationToken).ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(packagePath))
            {
                shell.ShellStatus = L("AppShell.Controller.ImportVehiclePackageCancelled");
                return;
            }

            await shell.ImportVehiclePackageAsync(packagePath, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = LF("AppShell.Controller.ImportVehiclePackageStartFailed", ex.Message);
        }
    }

    public async Task ImportBackupAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await ConfirmDiscardPendingChangesAsync(owner, shell, L("AppShell.Controller.ImportBackupAction")).ConfigureAwait(true))
            {
                shell.RequestWorkspaceFocus(shell.GetPendingEditFocusTarget());
                return;
            }

            var backupPath = await shell.PickBackupImportPathAsync(cancellationToken).ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(backupPath))
            {
                shell.ShellStatus = L("AppShell.Controller.ImportBackupCancelled");
                return;
            }

            var confirmed = await _dialogService.ConfirmBackupImportAsync(owner, backupPath).ConfigureAwait(true);
            if (!confirmed)
            {
                shell.ShellStatus = L("AppShell.Controller.ImportBackupNotConfirmed");
                return;
            }

            await shell.ImportBackupAsync(backupPath, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = LF("AppShell.Controller.ImportBackupStartFailed", ex.Message);
        }
    }

    public async Task OpenDataStoreHealthAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await shell.CheckDataStoreHealthAsync(cancellationToken).ConfigureAwait(true);
            var action = await _dialogService
                .ShowDataStoreHealthAsync(owner, new DataStoreHealthDialogViewModel(report))
                .ConfigureAwait(true);
            switch (action)
            {
                case DataStoreHealthDialogAction.OpenDataFolder:
                    await shell.OpenDataFolderAsync(cancellationToken).ConfigureAwait(true);
                    break;
                case DataStoreHealthDialogAction.OpenPreMigrationBackupFolder:
                    await shell.OpenPreMigrationBackupFolderAsync(cancellationToken).ConfigureAwait(true);
                    break;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = LF("AppShell.Controller.DataStoreHealthFailed", ex.Message);
        }
    }

    public async Task OpenAboutAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        try
        {
            var aboutModel = shell.BuildAboutDialogModel();
            var action = await _dialogService
                .ShowAboutAsync(owner, aboutModel)
                .ConfigureAwait(true);
            switch (action)
            {
                case AboutDialogAction.OpenReleaseNotes when !string.IsNullOrWhiteSpace(aboutModel.ReleaseNotesUrl):
                    await shell.OpenExternalAsync(aboutModel.ReleaseNotesUrl, cancellationToken).ConfigureAwait(true);
                    break;
                case AboutDialogAction.ThankAuthor:
                    await OpenAuthorSupportAsync(shell, cancellationToken).ConfigureAwait(true);
                    break;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = LF("AppShell.Controller.AboutFailed", ex.Message);
        }
    }

    public Task OpenAuthorSupportAsync(MainWindowViewModel shell, CancellationToken cancellationToken = default) =>
        shell.OpenExternalAsync(AboutDialogViewModel.AuthorSupportUrl, cancellationToken);

    public Task OpenFeedbackIssueAsync(MainWindowViewModel shell, CancellationToken cancellationToken = default) =>
        shell.OpenExternalAsync(shell.BuildFeedbackIssueUrl(), cancellationToken);

    public async Task<bool> CheckForUpdatesAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await shell.CheckForUpdatesAsync(cancellationToken).ConfigureAwait(true);
            var action = await _dialogService
                .ShowUpdateAsync(owner, new UpdateDialogViewModel(result, DesktopLocalization.Localizer))
                .ConfigureAwait(true);

            switch (action)
            {
                case UpdateDialogAction.PrimaryAction:
                    if (result.IsUpdateAvailable && result.CanInstallAutomatically)
                    {
                        if (!await ConfirmDiscardPendingChangesAsync(owner, shell, L("AppShell.Controller.InstallUpdateAction")).ConfigureAwait(true))
                        {
                            shell.RequestWorkspaceFocus(shell.GetPendingEditFocusTarget());
                            return false;
                        }

                        var installResult = await _dialogService.ShowUpdateInstallProgressAsync(
                                owner,
                                new UpdateInstallProgressDialogViewModel(DesktopLocalization.Localizer),
                                (progress, progressCancellationToken) => shell.PrepareUpdateInstallAsync(result, progress, progressCancellationToken))
                            .ConfigureAwait(true);
                        if (installResult.IsReady && installResult.InstallPlan is not null)
                        {
                            _updateInstallLauncher.Launch(installResult.InstallPlan);
                            shell.ShellStatus = L("AppShell.Controller.UpdateInstallerLaunched");
                            return true;
                        }

                        shell.ShellStatus = installResult.Message;

                        await _dialogService.ShowUpdateAsync(
                                owner,
                                new UpdateDialogViewModel(new UpdateCheckResult(
                                    result.CurrentVersion,
                                    result.LatestVersion,
                                    false,
                                    result.PublishedAt,
                                    result.NotesUrl,
                                    result.AssetUrl,
                                    result.Sha256,
                                    result.AssetSize,
                                    false,
                                    installResult.Message,
                                    installResult.Message),
                                    DesktopLocalization.Localizer))
                            .ConfigureAwait(true);
                        return false;
                    }

                    if (!string.IsNullOrWhiteSpace(result.NotesUrl))
                    {
                        await shell.OpenExternalAsync(result.NotesUrl, cancellationToken).ConfigureAwait(true);
                    }
                    else if (!string.IsNullOrWhiteSpace(result.AssetUrl))
                    {
                        await shell.OpenExternalAsync(result.AssetUrl, cancellationToken).ConfigureAwait(true);
                    }

                    return false;
                case UpdateDialogAction.OpenAsset:
                    if (!string.IsNullOrWhiteSpace(result.AssetUrl))
                    {
                        await shell.OpenExternalAsync(result.AssetUrl, cancellationToken).ConfigureAwait(true);
                    }

                    return false;
                default:
                    return false;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = LF("AppShell.Controller.UpdateCheckFailed", ex.Message);
            return false;
        }
    }

    public async Task<bool> ConfirmDiscardPendingChangesAsync(Window owner, MainWindowViewModel shell, string actionDescription)
    {
        if (!shell.HasPendingEdits)
        {
            return true;
        }

        return await _dialogService
            .ConfirmDiscardPendingChangesAsync(owner, shell.GetPendingEditLabel(), actionDescription)
            .ConfigureAwait(true);
    }

    private static string L(string key) => DesktopLocalization.Localizer.GetString(key);

    private static string LF(string key, params object?[] args) => DesktopLocalization.Localizer.Format(key, args);
}
