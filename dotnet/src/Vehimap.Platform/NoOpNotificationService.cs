using Vehimap.Application.Abstractions;

namespace Vehimap.Platform;

public sealed class NoOpNotificationService : INotificationService
{
    public Task ShowAsync(string title, string message, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
