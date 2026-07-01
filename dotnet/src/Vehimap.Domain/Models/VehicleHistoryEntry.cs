// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Domain.Models;

public sealed record VehicleHistoryEntry(
    string Id,
    string VehicleId,
    string EventDate,
    string EventType,
    string Odometer,
    string Cost,
    string Note);
