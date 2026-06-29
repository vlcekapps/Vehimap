using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed record VehimapBootstrapResult(
    VehimapDataRoot DataRoot,
    VehimapDataSet DataSet,
    DataMigrationResult? MigrationResult = null);
