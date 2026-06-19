using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

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

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close(false);

    private void OnAboutKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            Close(false);
            return;
        }

        if (e.Key == Key.O && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            Close(true);
        }
    }
}
