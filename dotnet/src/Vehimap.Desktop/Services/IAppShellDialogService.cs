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

    Task<bool> ShowAboutAsync(Window owner, AboutDialogViewModel model);

    Task<UpdateDialogAction> ShowUpdateAsync(Window owner, UpdateDialogViewModel model);

    Task<TrayActionsDialogAction> ShowTrayActionsAsync(Window? owner, TrayActionsDialogViewModel model);
}
