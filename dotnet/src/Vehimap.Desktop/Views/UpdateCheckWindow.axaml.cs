using Avalonia.Controls;
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

    private void OnPrimaryActionClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(UpdateDialogAction.PrimaryAction);

    private void OnAssetActionClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(UpdateDialogAction.OpenAsset);

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(UpdateDialogAction.Close);
}
