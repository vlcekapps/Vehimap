using Avalonia.Controls;
using Vehimap.Application.Models;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Services;

internal sealed class AvaloniaAppShellDialogService : IAppShellDialogService
{
    public async Task<SettingsDialogResult?> ShowSettingsAsync(Window owner, DesktopSupportedSettingsSnapshot snapshot, string automaticBackupStatus)
    {
        var dialog = new SettingsWindow
        {
            DataContext = SettingsDialogViewModel.FromSnapshot(snapshot, automaticBackupStatus)
        };

        return await dialog.ShowDialog<SettingsDialogResult?>(owner);
    }

    public async Task<bool> ConfirmBackupImportAsync(Window owner, string backupPath)
    {
        var confirmation = new ConfirmationWindow
        {
            DataContext = new ConfirmationDialogViewModel(
                "Obnovit data ze zálohy",
                $"Opravdu chcete nahradit aktuální načtená data obsahem zálohy?\n\n{backupPath}\n\nTento krok přepíše aktuální pracovní data v desktopové větvi.",
                "Obnovit data",
                "Zrušit")
        };

        return await confirmation.ShowDialog<bool>(owner);
    }

    public async Task<bool> ConfirmDiscardPendingChangesAsync(Window owner, string pendingEditLabel, string actionDescription)
    {
        var subject = string.IsNullOrWhiteSpace(pendingEditLabel)
            ? "rozpracované změny"
            : pendingEditLabel.Trim();
        var actionText = string.IsNullOrWhiteSpace(actionDescription)
            ? "pokračovat"
            : actionDescription.Trim();

        var confirmation = new ConfirmationWindow
        {
            DataContext = new ConfirmationDialogViewModel(
                "Neuložené změny",
                $"Máte rozpracované změny v části „{subject}“.\n\nPokud budete pokračovat a zvolíte akci „{actionText}“, všechny neuložené změny se zahodí.\n\nOpravdu chcete pokračovat?",
                "Zahodit změny",
                "Pokračovat v úpravách")
        };

        return await confirmation.ShowDialog<bool>(owner);
    }

    public async Task<bool> ShowAboutAsync(Window owner, AboutDialogViewModel model)
    {
        var dialog = new AboutWindow
        {
            DataContext = model
        };

        return await dialog.ShowDialog<bool>(owner);
    }

    public async Task<UpdateDialogAction> ShowUpdateAsync(Window owner, UpdateDialogViewModel model)
    {
        var dialog = new UpdateCheckWindow
        {
            DataContext = model
        };

        return await dialog.ShowDialog<UpdateDialogAction>(owner);
    }
}
