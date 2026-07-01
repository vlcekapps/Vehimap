// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Domain.Models;

namespace Vehimap.Application.Models;

public sealed record VehiclePackageImportResult(
    VehimapDataSet DataSet,
    string ImportedVehicleId,
    string ImportedVehicleName,
    int RestoredAttachmentCount);
