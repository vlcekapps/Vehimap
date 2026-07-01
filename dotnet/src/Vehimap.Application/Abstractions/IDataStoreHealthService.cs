// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IDataStoreHealthService
{
    Task<DataStoreHealthReport> CheckAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default);
}
