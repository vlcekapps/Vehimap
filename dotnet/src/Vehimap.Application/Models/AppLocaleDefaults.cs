// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record AppLocaleDefaults(
    string Language,
    string ThousandsSeparator,
    string DecimalSeparator,
    string DistanceUnit,
    string VolumeUnit,
    string Currency = "CZK");
