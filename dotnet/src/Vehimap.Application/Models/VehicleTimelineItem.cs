// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record VehicleTimelineItem(
    string Kind,
    string KindLabel,
    string VehicleId,
    string VehicleName,
    string VehiclePlate,
    string VehicleMakeModel,
    DateOnly Date,
    string DateText,
    string Title,
    string Detail,
    string Status,
    string EntryId,
    string Note,
    bool IsFuture);
