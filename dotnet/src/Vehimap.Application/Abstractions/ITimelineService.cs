// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface ITimelineService
{
    IReadOnlyList<VehicleTimelineItem> BuildVehicleTimeline(Vehimap.Domain.Models.VehimapDataSet dataSet, string vehicleId, DateOnly today);
}
