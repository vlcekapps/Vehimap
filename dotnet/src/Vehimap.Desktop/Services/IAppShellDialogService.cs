using Avalonia.Controls;
using Vehimap.Application.Models;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Services;

internal interface IAppShellDialogService
{
    Task<DesktopSupportedSettingsSnapshot?> ShowSettingsAsync(Window owner, DesktopSupportedSettingsSnapshot snapshot);

    Task<bool> ConfirmBackupImportAsync(Window owner, string backupPath);

    Task<bool> ShowAboutAsync(Window owner, AboutDialogViewModel model);

    Task<UpdateDialogAction> ShowUpdateAsync(Window owner, UpdateDialogViewModel model);
}
