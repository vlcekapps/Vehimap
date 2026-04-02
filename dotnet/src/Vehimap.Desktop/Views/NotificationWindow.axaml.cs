using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace Vehimap.Desktop.Views;

public partial class NotificationWindow : Window
{
    public NotificationWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public NotificationWindow(string notificationTitle, string notificationMessage)
        : this()
    {
        Title = notificationTitle;
        this.FindControl<TextBlock>("NotificationTitleTextBlock")!.Text = notificationTitle;
        this.FindControl<TextBlock>("NotificationMessageTextBlock")!.Text = notificationMessage;
        Opened += (_, _) => DispatcherTimer.RunOnce(Close, TimeSpan.FromSeconds(8));
    }
}
