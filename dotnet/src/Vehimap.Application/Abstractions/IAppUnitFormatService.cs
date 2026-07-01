// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IAppUnitFormatService
{
    AppUnitPreferences Normalize(AppUnitPreferences preferences);

    decimal ConvertDistanceFromKilometers(decimal kilometers, AppUnitPreferences unitPreferences);

    string FormatDistanceFromKilometers(decimal kilometers, AppCulturePreferences culturePreferences, AppUnitPreferences unitPreferences, int decimalPlaces = 1);

    string GetDistanceUnitLabel(AppUnitPreferences unitPreferences);

    decimal ConvertDistanceToKilometers(decimal value, AppUnitPreferences unitPreferences);

    string FormatVolumeFromLiters(decimal liters, AppCulturePreferences culturePreferences, AppUnitPreferences unitPreferences, int decimalPlaces = 2);

    string GetVolumeUnitLabel(AppUnitPreferences unitPreferences);

    decimal ConvertVolumeFromLiters(decimal liters, AppUnitPreferences unitPreferences);

    decimal ConvertVolumeToLiters(decimal value, AppUnitPreferences unitPreferences);
}
