using Avalonia.Controls;
using Vehimap.Application.Models;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Services;

internal sealed class AvaloniaAppShellDialogService : IAppShellDialogService
{
    public async Task<SettingsDialogResult?> ShowSettingsAsync(Window owner, DesktopSupportedSettingsSnapshot snapshot, string automaticBackupStatus)
    {
        var dialog = new SettingsWindow
        {
            DataContext = SettingsDialogViewModel.FromSnapshot(snapshot, automaticBackupStatus, DesktopLocalization.Localizer)
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

    public async Task<DataStoreHealthDialogAction> ShowDataStoreHealthAsync(Window owner, DataStoreHealthDialogViewModel model)
    {
        var dialog = new DataStoreHealthWindow
        {
            DataContext = model
        };

        return await dialog.ShowDialog<DataStoreHealthDialogAction>(owner);
    }

    public async Task<AboutDialogAction> ShowAboutAsync(Window owner, AboutDialogViewModel model)
    {
        var dialog = new AboutWindow
        {
            DataContext = model
        };

        return await dialog.ShowDialog<AboutDialogAction>(owner);
    }

    public async Task<UpdateDialogAction> ShowUpdateAsync(Window owner, UpdateDialogViewModel model)
    {
        var dialog = new UpdateCheckWindow
        {
            DataContext = model
        };

        return await dialog.ShowDialog<UpdateDialogAction>(owner);
    }

    public async Task<UpdateInstallResult> ShowUpdateInstallProgressAsync(
        Window owner,
        UpdateInstallProgressDialogViewModel model,
        Func<IProgress<UpdateInstallProgress>, CancellationToken, Task<UpdateInstallResult>> prepareInstallAsync)
    {
        var dialog = new UpdateInstallProgressWindow
        {
            DataContext = model,
            PrepareInstallAsync = prepareInstallAsync
        };

        return await dialog.ShowDialog<UpdateInstallResult>(owner);
    }

    public async Task<TrayActionsDialogAction> ShowTrayActionsAsync(Window? owner, TrayActionsDialogViewModel model)
    {
        var dialog = new TrayActionsWindow
        {
            DataContext = model
        };

        if (owner is not null && owner.IsVisible)
        {
            await dialog.ShowDialog(owner);
            return dialog.Result;
        }

        var completion = new TaskCompletionSource<TrayActionsDialogAction>();
        void OnClosed(object? sender, EventArgs e)
        {
            dialog.Closed -= OnClosed;
            completion.TrySetResult(dialog.Result);
        }

        dialog.Closed += OnClosed;
        dialog.Show();
        return await completion.Task.ConfigureAwait(true);
    }
}
