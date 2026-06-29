using Vehimap.Domain.Models;

namespace Vehimap.Application.Models;

public sealed record VehiclePackageImportResult(
    VehimapDataSet DataSet,
    string ImportedVehicleId,
    string ImportedVehicleName,
    int RestoredAttachmentCount);
