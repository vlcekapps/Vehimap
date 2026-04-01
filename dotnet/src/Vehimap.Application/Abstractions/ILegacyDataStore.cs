using Vehimap.Domain.Models;

namespace Vehimap.Application.Abstractions;

public interface ILegacyDataStore
{
    Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default);
    Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default);
}
