// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IDataMigrationService
{
    Task<DataMigrationResult> MigrateIfNeededAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default);
}
