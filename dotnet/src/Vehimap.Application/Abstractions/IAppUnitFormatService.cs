using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface IAppUnitFormatService
{
    AppUnitPreferences Normalize(AppUnitPreferences preferences);

    string FormatDistanceFromKilometers(decimal kilometers, AppCulturePreferences culturePreferences, AppUnitPreferences unitPreferences, int decimalPlaces = 1);

    decimal ConvertDistanceToKilometers(decimal value, AppUnitPreferences unitPreferences);

    string FormatVolumeFromLiters(decimal liters, AppCulturePreferences culturePreferences, AppUnitPreferences unitPreferences, int decimalPlaces = 2);

    decimal ConvertVolumeToLiters(decimal value, AppUnitPreferences unitPreferences);
}
