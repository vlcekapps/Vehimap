namespace Vehimap.Application.Models;

public sealed record AppUnitPreferences(
    string DistanceUnit = "km",
    string VolumeUnit = "l");
