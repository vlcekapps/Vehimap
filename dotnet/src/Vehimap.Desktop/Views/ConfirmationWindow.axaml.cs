using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace Vehimap.Desktop.Views;

public partial class ConfirmationWindow : Window
{
    public ConfirmationWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += (_, _) => Dispatcher.UIThread.Post(() => this.FindControl<Button>("ConfirmButton")?.Focus());
    }

    private void OnConfirmClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(true);

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(false);
}
