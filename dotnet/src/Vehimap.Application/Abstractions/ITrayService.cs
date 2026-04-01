namespace Vehimap.Application.Abstractions;

public interface ITrayService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
