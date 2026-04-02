using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace Vehimap.Desktop.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += (_, _) => Dispatcher.UIThread.Post(() => this.FindControl<Button>("ReleaseNotesButton")?.Focus());
    }

    private void OnReleaseNotesClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(true);

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(false);
}
