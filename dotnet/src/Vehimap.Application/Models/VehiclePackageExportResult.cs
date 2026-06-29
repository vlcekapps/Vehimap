namespace Vehimap.Application.Models;

public sealed record VehiclePackageExportResult(
    string PackagePath,
    string VehicleId,
    string VehicleName,
    int IncludedAttachmentCount,
    int MissingAttachmentCount);
