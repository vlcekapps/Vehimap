// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Domain.Models;

namespace Vehimap.Application.Abstractions;

public interface IVehimapDataStore
{
    Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default);

    Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default);
}
