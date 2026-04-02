using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface ITrayService : IAsyncDisposable
{
    bool IsSupported { get; }

    Task InitializeAsync(TrayServiceConfiguration configuration, CancellationToken cancellationToken = default);

    Task UpdateToolTipAsync(string toolTipText, CancellationToken cancellationToken = default);
}
