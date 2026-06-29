using Avalonia.Controls;
using Vehimap.Application.Models;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Services;

internal interface IAppShellDialogService
{
    Task<SettingsDialogResult?> ShowSettingsAsync(Window owner, DesktopSupportedSettingsSnapshot snapshot, string automaticBackupStatus);

    Task<bool> ConfirmBackupImportAsync(Window owner, string backupPath);

    Task<bool> ConfirmDiscardPendingChangesAsync(Window owner, string pendingEditLabel, string actionDescription);

    Task<DataStoreHealthDialogAction> ShowDataStoreHealthAsync(Window owner, DataStoreHealthDialogViewModel model);

    Task<AboutDialogAction> ShowAboutAsync(Window owner, AboutDialogViewModel model);

    Task<UpdateDialogAction> ShowUpdateAsync(Window owner, UpdateDialogViewModel model);

    Task<UpdateInstallResult> ShowUpdateInstallProgressAsync(
        Window owner,
        UpdateInstallProgressDialogViewModel model,
        Func<IProgress<UpdateInstallProgress>, CancellationToken, Task<UpdateInstallResult>> prepareInstallAsync);

    Task<TrayActionsDialogAction> ShowTrayActionsAsync(Window? owner, TrayActionsDialogViewModel model);
}
