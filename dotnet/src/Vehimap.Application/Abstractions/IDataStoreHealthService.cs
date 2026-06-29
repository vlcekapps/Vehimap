using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IDataStoreHealthService
{
    Task<DataStoreHealthReport> CheckAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default);
}
