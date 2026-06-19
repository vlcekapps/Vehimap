using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace Vehimap.Desktop.Views;

public partial class ConfirmationWindow : Window
{
    public ConfirmationWindow()
    {
        AvaloniaXamlLoader.Load(this);
        AddHandler(InputElement.KeyDownEvent, OnConfirmationKeyDown, RoutingStrategies.Tunnel);
        Opened += (_, _) => Dispatcher.UIThread.Post(() => this.FindControl<Button>("ConfirmButton")?.Focus());
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e) => Close(true);

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(false);

    private void OnConfirmationKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        e.Handled = true;
        Close(false);
    }
}
