// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Abstractions;

public interface IVehiclePackageService
{
    Task<VehiclePackageExportResult> ExportVehicleAsync(
        string packagePath,
        VehimapDataRoot dataRoot,
        VehimapDataSet dataSet,
        string vehicleId,
        CancellationToken cancellationToken = default);

    Task<VehiclePackageImportResult> ImportVehicleAsync(
        string packagePath,
        VehimapDataRoot dataRoot,
        VehimapDataSet currentDataSet,
        CancellationToken cancellationToken = default);
}
