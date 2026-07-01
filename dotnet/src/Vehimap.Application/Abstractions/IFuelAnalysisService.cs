// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Abstractions;

public interface IFuelAnalysisService
{
    FuelAnalysisSummary BuildVehicleFuelAnalysis(VehimapDataSet dataSet, string vehicleId);
}
