// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record AppUnitPreferences(
    string DistanceUnit = "km",
    string VolumeUnit = "l");
