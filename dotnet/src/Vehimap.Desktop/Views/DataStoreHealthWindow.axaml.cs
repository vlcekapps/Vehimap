// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class DataStoreHealthWindow : Window
{
    public DataStoreHealthWindow()
    {
        AvaloniaXamlLoader.Load(this);
        KeyboardAccessibilityHelper.RegisterWindow(this);
        AddHandler(InputElement.KeyDownEvent, OnHealthKeyDown, RoutingStrategies.Tunnel);
        Opened += (_, _) => Dispatcher.UIThread.Post(() => this.FindControl<Button>("CopyHealthDetailsButton")?.Focus());
    }

    private async void OnCopyDetailsClick(object? sender, RoutedEventArgs e) => await CopyDetailsAsync();

    private void OnOpenDataFolderClick(object? sender, RoutedEventArgs e) =>
        Close(DataStoreHealthDialogAction.OpenDataFolder);

    private void OnOpenPreMigrationBackupFolderClick(object? sender, RoutedEventArgs e) =>
        Close(DataStoreHealthDialogAction.OpenPreMigrationBackupFolder);

    private void OnCloseClick(object? sender, RoutedEventArgs e) =>
        Close(DataStoreHealthDialogAction.None);

    private async void OnHealthKeyDown(object? sender, KeyEventArgs e)
    {
        if (KeyboardAccessibilityHelper.ShouldSkipGlobalShortcut(e))
        {
            return;
        }

        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            Close(DataStoreHealthDialogAction.None);
            return;
        }

        if (e.Key == Key.C && e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift))
        {
            e.Handled = true;
            await CopyDetailsAsync();
        }
    }

    private async Task CopyDetailsAsync()
    {
        if (DataContext is not DataStoreHealthDialogViewModel model)
        {
            return;
        }

        if (Clipboard is null)
        {
            model.StatusMessage = DesktopLocalization.Localizer.GetString("Common.ClipboardUnavailable");
            return;
        }

        try
        {
            await Clipboard.SetTextAsync(model.ClipboardText).ConfigureAwait(true);
            model.StatusMessage = DesktopLocalization.Localizer.GetString("DataStoreHealth.CopySuccessStatus");
        }
        catch (Exception ex)
        {
            model.StatusMessage = DesktopLocalization.Localizer.Format("DataStoreHealth.CopyFailedStatus", ex.Message);
        }
    }
}
