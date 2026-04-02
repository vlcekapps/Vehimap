using Avalonia.Threading;
using Vehimap.Application.Abstractions;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopNotificationService : INotificationService
{
    public Task ShowAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            var notification = new NotificationWindow(title, message);
            notification.Show();
        }).GetTask();
    }
}
