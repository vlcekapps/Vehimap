using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Abstractions;

public interface IFuelAnalysisService
{
    FuelAnalysisSummary BuildVehicleFuelAnalysis(VehimapDataSet dataSet, string vehicleId);
}
