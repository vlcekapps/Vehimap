// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Domain.Models;

public sealed record MaintenancePlan(
    string Id,
    string VehicleId,
    string Title,
    string IntervalKm,
    string IntervalMonths,
    string LastServiceDate,
    string LastServiceOdometer,
    bool IsActive,
    string Note);
