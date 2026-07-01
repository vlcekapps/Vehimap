// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record VehiclePackageExportResult(
    string PackagePath,
    string VehicleId,
    string VehicleName,
    int IncludedAttachmentCount,
    int MissingAttachmentCount);
