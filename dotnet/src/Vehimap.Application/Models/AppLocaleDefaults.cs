namespace Vehimap.Application.Models;

public sealed record AppLocaleDefaults(
    string Language,
    string ThousandsSeparator,
    string DecimalSeparator,
    string DistanceUnit,
    string VolumeUnit);
