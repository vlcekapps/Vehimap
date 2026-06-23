using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        AvaloniaXamlLoader.Load(this);
        AddHandler(InputElement.KeyDownEvent, OnAboutKeyDown, RoutingStrategies.Tunnel);
        Opened += (_, _) => Dispatcher.UIThread.Post(() => this.FindControl<Button>("ReleaseNotesButton")?.Focus());
    }

    private void OnReleaseNotesClick(object? sender, RoutedEventArgs e) => Close(true);

    private async void OnCopyDetailsClick(object? sender, RoutedEventArgs e) => await CopyDetailsAsync();

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close(false);

    private async void OnAboutKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            Close(false);
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
            Close(true);
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
            model.StatusMessage = "Schránka není dostupná.";
            return;
        }

        try
        {
            await Clipboard.SetTextAsync(model.ClipboardText).ConfigureAwait(true);
            model.StatusMessage = "Informace byly zkopírovány do schránky.";
        }
        catch (Exception ex)
        {
            model.StatusMessage = $"Informace se nepodařilo zkopírovat: {ex.Message}";
        }
    }
}
