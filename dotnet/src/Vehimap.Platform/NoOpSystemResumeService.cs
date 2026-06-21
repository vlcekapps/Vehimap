using Vehimap.Application.Abstractions;

namespace Vehimap.Platform;

public sealed class NoOpSystemResumeService : ISystemResumeService
{
    public event EventHandler? Resumed;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _ = Resumed;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
