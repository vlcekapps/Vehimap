// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Domain.Models;

public sealed record Vehicle(
    string Id,
    string Name,
    string Category,
    string VehicleNote,
    string MakeModel,
    string Plate,
    string Year,
    string Power,
    string LastTk,
    string NextTk,
    string GreenCardFrom,
    string GreenCardTo);
