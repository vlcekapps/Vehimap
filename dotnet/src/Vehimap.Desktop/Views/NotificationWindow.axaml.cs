using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace Vehimap.Desktop.Views;

public partial class NotificationWindow : Window
{
    private const int ScreenMargin = 18;

    public NotificationWindow()
    {
        AvaloniaXamlLoader.Load(this);
        KeyboardAccessibilityHelper.RegisterWindow(this);
        AddHandler(InputElement.KeyDownEvent, OnNotificationKeyDown, RoutingStrategies.Tunnel);
    }

    public NotificationWindow(string notificationTitle, string notificationMessage)
        : this()
    {
        Title = notificationTitle;
        var titleBlock = this.FindControl<TextBlock>("NotificationTitleTextBlock")!;
        titleBlock.Text = notificationTitle;
        AutomationProperties.SetName(titleBlock, notificationTitle);
        this.FindControl<TextBlock>("NotificationMessageTextBlock")!.Text = notificationMessage;
        Opened += OnNotificationOpened;
    }

    private void OnNotificationOpened(object? sender, EventArgs e)
    {
        PositionNearScreenEdge();
        DispatcherTimer.RunOnce(Close, TimeSpan.FromSeconds(8));
    }

    private void PositionNearScreenEdge()
    {
        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null)
        {
            return;
        }

        var area = screen.WorkingArea;
        var x = Math.Max(area.X, area.X + area.Width - (int)Width - ScreenMargin);
        var y = Math.Max(area.Y, area.Y + area.Height - (int)Height - ScreenMargin);
        Position = new PixelPoint(x, y);
    }

    private void OnNotificationKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}
