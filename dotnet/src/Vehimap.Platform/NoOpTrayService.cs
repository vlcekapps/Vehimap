using Vehimap.Application.Abstractions;

using Vehimap.Application.Models;

namespace Vehimap.Platform;

public sealed class NoOpTrayService : ITrayService
{
    public bool IsSupported => false;

    public Task InitializeAsync(TrayServiceConfiguration configuration, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task UpdateToolTipAsync(string toolTipText, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
