using Vehimap.Application.Abstractions;

namespace Vehimap.Platform;

public sealed class NoOpTrayService : ITrayService
{
    public Task InitializeAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
