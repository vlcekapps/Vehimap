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

public enum UpdateDialogAction
{
    Close,
    PrimaryAction,
    OpenAsset
}

public partial class UpdateCheckWindow : Window
{
    public UpdateCheckWindow()
    {
        AvaloniaXamlLoader.Load(this);
        KeyboardAccessibilityHelper.RegisterWindow(this);
        AddHandler(InputElement.KeyDownEvent, OnUpdateCheckKeyDown, RoutingStrategies.Tunnel);
        Opened += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            var primaryButton = this.FindControl<Button>("PrimaryActionButton");
            if (primaryButton is not null && primaryButton.IsVisible)
            {
                primaryButton.Focus();
                return;
            }

            this.FindControl<Button>("CloseButton")?.Focus();
        });
    }

    private void OnPrimaryActionClick(object? sender, RoutedEventArgs e) => Close(UpdateDialogAction.PrimaryAction);

    private void OnAssetActionClick(object? sender, RoutedEventArgs e) => Close(UpdateDialogAction.OpenAsset);

    private async void OnCopyDetailsClick(object? sender, RoutedEventArgs e) => await CopyDetailsAsync();

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close(UpdateDialogAction.Close);

    private async void OnUpdateCheckKeyDown(object? sender, KeyEventArgs e)
    {
        if (KeyboardAccessibilityHelper.ShouldSkipGlobalShortcut(e))
        {
            return;
        }

        if (e.Key == Key.C && e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift))
        {
            e.Handled = true;
            await CopyDetailsAsync();
            return;
        }

        if (e.Key != Key.Escape || e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        e.Handled = true;
        Close(UpdateDialogAction.Close);
    }

    private async Task CopyDetailsAsync()
    {
        if (DataContext is not UpdateDialogViewModel model)
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
            model.StatusMessage = DesktopLocalization.Localizer.GetString("UpdateCheck.Status.DetailsCopied");
        }
        catch (Exception ex)
        {
            model.StatusMessage = DesktopLocalization.Localizer.Format("UpdateCheck.Status.CopyFailed", ex.Message);
        }
    }
}
