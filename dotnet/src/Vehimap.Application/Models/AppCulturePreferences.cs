// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record AppCulturePreferences(
    string Language = "system",
    string ThousandsSeparator = "culture",
    string DecimalSeparator = "culture");
