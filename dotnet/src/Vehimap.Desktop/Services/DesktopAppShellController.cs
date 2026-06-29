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
                shell.ShellStatus = "Export zálohy byl zrušen.";
                return;
            }

            await shell.ExportBackupAsync(backupPath, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = $"Export zálohy se nepodařilo spustit: {ex.Message}";
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
                shell.ShellStatus = "Export balíčku vozidla byl zrušen.";
                return;
            }

            await shell.ExportSelectedVehiclePackageAsync(packagePath, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = $"Export balíčku vozidla se nepodařilo spustit: {ex.Message}";
        }
    }

    public async Task ImportVehiclePackageAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await ConfirmDiscardPendingChangesAsync(owner, shell, "importovat balíček vozidla").ConfigureAwait(true))
            {
                shell.RequestWorkspaceFocus(shell.GetPendingEditFocusTarget());
                return;
            }

            var packagePath = await shell.PickVehiclePackageImportPathAsync(cancellationToken).ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(packagePath))
            {
                shell.ShellStatus = "Import balíčku vozidla byl zrušen.";
                return;
            }

            await shell.ImportVehiclePackageAsync(packagePath, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = $"Import balíčku vozidla se nepodařilo spustit: {ex.Message}";
        }
    }

    public async Task ImportBackupAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await ConfirmDiscardPendingChangesAsync(owner, shell, "obnovit data ze zálohy").ConfigureAwait(true))
            {
                shell.RequestWorkspaceFocus(shell.GetPendingEditFocusTarget());
                return;
            }

            var backupPath = await shell.PickBackupImportPathAsync(cancellationToken).ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(backupPath))
            {
                shell.ShellStatus = "Obnova ze zálohy byla zrušena.";
                return;
            }

            var confirmed = await _dialogService.ConfirmBackupImportAsync(owner, backupPath).ConfigureAwait(true);
            if (!confirmed)
            {
                shell.ShellStatus = "Obnova ze zálohy nebyla potvrzena.";
                return;
            }

            await shell.ImportBackupAsync(backupPath, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            shell.ShellStatus = $"Obnovu ze zálohy se nepodařilo spustit: {ex.Message}";
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
            shell.ShellStatus = $"Kontrolu datové sady se nepodařilo dokončit: {ex.Message}";
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
            shell.ShellStatus = $"Dialog O programu se nepodařilo dokončit: {ex.Message}";
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
            var action = await _dialogService.ShowUpdateAsync(owner, new UpdateDialogViewModel(result)).ConfigureAwait(true);

            switch (action)
            {
                case UpdateDialogAction.PrimaryAction:
                    if (result.IsUpdateAvailable && result.CanInstallAutomatically)
                    {
                        if (!await ConfirmDiscardPendingChangesAsync(owner, shell, "stáhnout a nainstalovat aktualizaci").ConfigureAwait(true))
                        {
                            shell.RequestWorkspaceFocus(shell.GetPendingEditFocusTarget());
                            return false;
                        }

                        var installResult = await _dialogService.ShowUpdateInstallProgressAsync(
                                owner,
                                new UpdateInstallProgressDialogViewModel(),
                                (progress, progressCancellationToken) => shell.PrepareUpdateInstallAsync(result, progress, progressCancellationToken))
                            .ConfigureAwait(true);
                        if (installResult.IsReady && installResult.InstallPlan is not null)
                        {
                            _updateInstallLauncher.Launch(installResult.InstallPlan);
                            shell.ShellStatus = "Instalátor aktualizace byl spuštěn. Vehimap se nyní ukončí, aby instalace mohla pokračovat.";
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
                                    installResult.Message)))
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
            shell.ShellStatus = $"Kontrolu aktualizací se nepodařilo dokončit: {ex.Message}";
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
}
