using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

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

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close(UpdateDialogAction.Close);

    private void OnUpdateCheckKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        e.Handled = true;
        Close(UpdateDialogAction.Close);
    }
}
