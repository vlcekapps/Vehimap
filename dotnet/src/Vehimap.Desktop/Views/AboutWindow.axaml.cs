using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        AvaloniaXamlLoader.Load(this);
        KeyboardAccessibilityHelper.RegisterWindow(this);
        AddHandler(InputElement.KeyDownEvent, OnAboutKeyDown, RoutingStrategies.Tunnel);
        Opened += (_, _) => Dispatcher.UIThread.Post(() => this.FindControl<Button>("ReleaseNotesButton")?.Focus());
    }

    private void OnReleaseNotesClick(object? sender, RoutedEventArgs e) => Close(AboutDialogAction.OpenReleaseNotes);

    private void OnThankAuthorClick(object? sender, RoutedEventArgs e) => Close(AboutDialogAction.ThankAuthor);

    private void OnToggleDiagnosticsClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AboutDialogViewModel model)
        {
            model.ToggleDiagnostics();
        }
    }

    private async void OnCopyDetailsClick(object? sender, RoutedEventArgs e) => await CopyDetailsAsync();

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close(AboutDialogAction.None);

    private async void OnAboutKeyDown(object? sender, KeyEventArgs e)
    {
        if (KeyboardAccessibilityHelper.ShouldSkipGlobalShortcut(e))
        {
            return;
        }

        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            Close(AboutDialogAction.None);
            return;
        }

        if (e.Key == Key.C && e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift))
        {
            e.Handled = true;
            await CopyDetailsAsync();
            return;
        }

        if (e.Key == Key.O && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            Close(AboutDialogAction.OpenReleaseNotes);
            return;
        }

        if (e.Key == Key.K && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            Close(AboutDialogAction.ThankAuthor);
        }
    }

    private async Task CopyDetailsAsync()
    {
        if (DataContext is not AboutDialogViewModel model)
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
            model.StatusMessage = DesktopLocalization.Localizer.GetString("About.Status.DiagnosticsCopied");
        }
        catch (Exception ex)
        {
            model.StatusMessage = DesktopLocalization.Localizer.Format("About.Status.DiagnosticsCopyFailed", ex.Message);
        }
    }
}
