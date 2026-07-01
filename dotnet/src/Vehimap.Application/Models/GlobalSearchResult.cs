// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record GlobalSearchResult(
    string VehicleId,
    string VehicleName,
    string EntityKind,
    string EntityId,
    string SectionLabel,
    string Title,
    string Summary,
    int Rank);
