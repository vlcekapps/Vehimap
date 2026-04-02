using Avalonia.Controls;
using Vehimap.Application;
using Vehimap.Application.Models;
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
        var snapshot = await _dialogService
            .ShowSettingsAsync(owner, shell.GetSupportedSettingsSnapshot())
            .ConfigureAwait(true);
        if (snapshot is null)
        {
            return;
        }

        await shell.SaveSupportedSettingsAsync(snapshot).ConfigureAwait(true);
    }

    public async Task ExportBackupAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        var backupPath = await shell.PickBackupExportPathAsync(cancellationToken).ConfigureAwait(true);
        if (string.IsNullOrWhiteSpace(backupPath))
        {
            return;
        }

        await shell.ExportBackupAsync(backupPath, cancellationToken).ConfigureAwait(true);
    }

    public async Task ImportBackupAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        var backupPath = await shell.PickBackupImportPathAsync(cancellationToken).ConfigureAwait(true);
        if (string.IsNullOrWhiteSpace(backupPath))
        {
            return;
        }

        var confirmed = await _dialogService.ConfirmBackupImportAsync(owner, backupPath).ConfigureAwait(true);
        if (!confirmed)
        {
            return;
        }

        await shell.ImportBackupAsync(backupPath, cancellationToken).ConfigureAwait(true);
    }

    public async Task OpenAboutAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        var aboutModel = shell.BuildAboutDialogModel();
        var openReleaseNotes = await _dialogService
            .ShowAboutAsync(owner, aboutModel)
            .ConfigureAwait(true);
        if (!openReleaseNotes)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(aboutModel.ReleaseNotesUrl))
        {
            await shell.OpenExternalAsync(aboutModel.ReleaseNotesUrl, cancellationToken).ConfigureAwait(true);
        }
    }

    public async Task<bool> CheckForUpdatesAsync(Window owner, MainWindowViewModel shell, CancellationToken cancellationToken = default)
    {
        var result = await shell.CheckForUpdatesAsync(cancellationToken).ConfigureAwait(true);
        var action = await _dialogService.ShowUpdateAsync(owner, new UpdateDialogViewModel(result)).ConfigureAwait(true);

        switch (action)
        {
            case UpdateDialogAction.PrimaryAction:
                if (result.IsUpdateAvailable && result.CanInstallAutomatically)
                {
                    var installResult = await shell.PrepareUpdateInstallAsync(result, cancellationToken).ConfigureAwait(true);
                    if (installResult.IsReady && installResult.InstallPlan is not null)
                    {
                        _updateInstallLauncher.Launch(installResult.InstallPlan);
                        return true;
                    }

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
}
