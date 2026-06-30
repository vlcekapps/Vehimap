using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

namespace Vehimap.Application.Services;

public sealed class AppUnitFormatService : IAppUnitFormatService
{
    public const string Kilometers = "km";
    public const string Miles = "mi";
    public const string Liters = "l";
    public const string UsGallons = "us_gal";
    public const string ImperialGallons = "imp_gal";

    private const decimal KilometersPerMile = 1.609344m;
    private const decimal LitersPerUsGallon = 3.785411784m;
    private const decimal LitersPerImperialGallon = 4.54609m;

    private static readonly string[] SupportedDistanceUnits = [Kilometers, Miles];
    private static readonly string[] SupportedVolumeUnits = [Liters, UsGallons, ImperialGallons];

    private readonly IAppNumberFormatService _numberFormatService;

    public AppUnitFormatService()
        : this(new AppNumberFormatService())
    {
    }

    public AppUnitFormatService(IAppNumberFormatService numberFormatService)
    {
        _numberFormatService = numberFormatService;
    }

    public AppUnitPreferences Normalize(AppUnitPreferences preferences) =>
        new(
            NormalizeDistanceUnit(preferences.DistanceUnit),
            NormalizeVolumeUnit(preferences.VolumeUnit));

    public decimal ConvertDistanceFromKilometers(decimal kilometers, AppUnitPreferences unitPreferences)
    {
        var normalized = Normalize(unitPreferences);
        return string.Equals(normalized.DistanceUnit, Miles, StringComparison.Ordinal)
            ? kilometers / KilometersPerMile
            : kilometers;
    }

    public string FormatDistanceFromKilometers(decimal kilometers, AppCulturePreferences culturePreferences, AppUnitPreferences unitPreferences, int decimalPlaces = 1)
    {
        var normalized = Normalize(unitPreferences);
        var value = ConvertDistanceFromKilometers(kilometers, normalized);
        var unitLabel = string.Equals(normalized.DistanceUnit, Miles, StringComparison.Ordinal) ? "mi" : "km";
        return _numberFormatService.FormatDecimal(value, culturePreferences, decimalPlaces) + " " + unitLabel;
    }

    public decimal ConvertDistanceToKilometers(decimal value, AppUnitPreferences unitPreferences)
    {
        var normalized = Normalize(unitPreferences);
        return string.Equals(normalized.DistanceUnit, Miles, StringComparison.Ordinal)
            ? value * KilometersPerMile
            : value;
    }

    public string FormatVolumeFromLiters(decimal liters, AppCulturePreferences culturePreferences, AppUnitPreferences unitPreferences, int decimalPlaces = 2)
    {
        var normalized = Normalize(unitPreferences);
        var (value, unitLabel) = normalized.VolumeUnit switch
        {
            UsGallons => (liters / LitersPerUsGallon, "US gal"),
            ImperialGallons => (liters / LitersPerImperialGallon, "imp gal"),
            _ => (liters, "l")
        };
        return _numberFormatService.FormatDecimal(value, culturePreferences, decimalPlaces) + " " + unitLabel;
    }

    public decimal ConvertVolumeToLiters(decimal value, AppUnitPreferences unitPreferences)
    {
        var normalized = Normalize(unitPreferences);
        return normalized.VolumeUnit switch
        {
            UsGallons => value * LitersPerUsGallon,
            ImperialGallons => value * LitersPerImperialGallon,
            _ => value
        };
    }

    public static string NormalizeDistanceUnit(string? unit) =>
        NormalizeOption(unit, SupportedDistanceUnits, Kilometers);

    public static string NormalizeVolumeUnit(string? unit) =>
        NormalizeOption(unit, SupportedVolumeUnits, Liters);

    private static string NormalizeOption(string? value, IReadOnlyCollection<string> supportedValues, string defaultValue)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? defaultValue
            : value.Trim();
        return supportedValues.Contains(normalized, StringComparer.OrdinalIgnoreCase)
            ? supportedValues.First(candidate => string.Equals(candidate, normalized, StringComparison.OrdinalIgnoreCase))
            : defaultValue;
    }
}
