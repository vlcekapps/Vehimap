using Vehimap.Application.Abstractions;

namespace Vehimap.Platform;

public sealed class NoOpAutostartService : IAutostartService
{
    public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
