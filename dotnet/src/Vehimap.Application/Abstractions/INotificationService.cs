namespace Vehimap.Application.Abstractions;

public interface INotificationService
{
    Task ShowAsync(string title, string message, CancellationToken cancellationToken = default);
}
