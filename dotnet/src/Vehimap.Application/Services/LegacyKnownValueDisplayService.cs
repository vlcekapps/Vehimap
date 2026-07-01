// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;

namespace Vehimap.Application.Services;

public static class LegacyKnownValueDisplayService
{
    private static readonly IReadOnlyDictionary<string, string> CategoryKeys = CreateMap(
        ("Osobní vozidla", "KnownValue.Category.PassengerVehicles"),
        ("Motocykly", "KnownValue.Category.Motorcycles"),
        ("Nákladní vozidla", "KnownValue.Category.Trucks"),
        ("Autobusy", "KnownValue.Category.Buses"),
        ("Ostatní", "KnownValue.Category.Other"));

    private static readonly IReadOnlyDictionary<string, string> RecordTypeKeys = CreateMap(
        ("Povinné ručení", "KnownValue.RecordType.LiabilityInsurance"),
        ("Havarijní pojištění", "KnownValue.RecordType.ComprehensiveInsurance"),
        ("Asistence", "KnownValue.RecordType.Assistance"),
        ("Doklad", "KnownValue.RecordType.Document"),
        ("Servisní dokument", "KnownValue.RecordType.ServiceDocument"),
        ("Jiné", "KnownValue.RecordType.Other"));

    private static readonly IReadOnlyDictionary<string, string> VehicleStateKeys = CreateMap(
        ("Běžný provoz", "KnownValue.VehicleState.NormalOperation"),
        ("Veterán", "KnownValue.VehicleState.Veteran"),
        ("Odstaveno", "KnownValue.VehicleState.OutOfService"),
        ("V renovaci", "KnownValue.VehicleState.InRenovation"),
        ("Na prodej", "KnownValue.VehicleState.ForSale"),
        ("Archiv", "KnownValue.VehicleState.Archive"));

    private static readonly IReadOnlyDictionary<string, string> PowertrainKeys = CreateMap(
        ("Benzín", "KnownValue.Powertrain.Gasoline"),
        ("Benzin", "KnownValue.Powertrain.Gasoline"),
        ("Nafta", "KnownValue.Powertrain.Diesel"),
        ("Hybrid", "KnownValue.Powertrain.Hybrid"),
        ("Plug-in hybrid", "KnownValue.Powertrain.PluginHybrid"),
        ("Elektro", "KnownValue.Powertrain.Electric"),
        ("LPG / CNG", "KnownValue.Powertrain.LpgCng"),
        ("Jiné", "KnownValue.Powertrain.Other"));

    private static readonly IReadOnlyDictionary<string, string> ClimateProfileKeys = CreateMap(
        ("Má klimatizaci", "KnownValue.Climate.HasAirConditioning"),
        ("Bez klimatizace", "KnownValue.Climate.NoAirConditioning"));

    private static readonly IReadOnlyDictionary<string, string> TimingDriveKeys = CreateMap(
        ("Řemen", "KnownValue.TimingDrive.Belt"),
        ("Řetěz", "KnownValue.TimingDrive.Chain"),
        ("Není relevantní", "KnownValue.Common.NotRelevant"));

    private static readonly IReadOnlyDictionary<string, string> TransmissionKeys = CreateMap(
        ("Manuální", "KnownValue.Transmission.Manual"),
        ("Automatická", "KnownValue.Transmission.Automatic"),
        ("Není relevantní", "KnownValue.Common.NotRelevant"));

    private static readonly IReadOnlyDictionary<string, string> FuelTypeKeys = CreateMap(
        ("Benzín", "KnownValue.FuelType.Gasoline"),
        ("Benzin", "KnownValue.FuelType.Gasoline"),
        ("Nafta", "KnownValue.FuelType.Diesel"),
        ("LPG", "KnownValue.FuelType.Lpg"),
        ("CNG", "KnownValue.FuelType.Cng"),
        ("Elektřina", "KnownValue.FuelType.Electricity"),
        ("Jiné", "KnownValue.FuelType.Other"));

    private static readonly IReadOnlyDictionary<string, string> ReminderRepeatModeKeys = CreateMap(
        ("Neopakovat", "KnownValue.ReminderRepeat.None"),
        ("Každý rok", "KnownValue.ReminderRepeat.Yearly"),
        ("Každé 2 roky", "KnownValue.ReminderRepeat.EveryTwoYears"),
        ("Každých 5 let", "KnownValue.ReminderRepeat.EveryFiveYears"));

    public static string FormatCategory(string? value, IAppLocalizer localizer) =>
        FormatKnownValue(value, localizer, CategoryKeys);

    public static string FormatRecordType(string? value, IAppLocalizer localizer) =>
        FormatKnownValue(value, localizer, RecordTypeKeys);

    public static string FormatVehicleState(string? value, IAppLocalizer localizer) =>
        FormatKnownValue(value, localizer, VehicleStateKeys);

    public static string FormatPowertrain(string? value, IAppLocalizer localizer) =>
        FormatKnownValue(value, localizer, PowertrainKeys);

    public static string FormatClimateProfile(string? value, IAppLocalizer localizer) =>
        FormatKnownValue(value, localizer, ClimateProfileKeys);

    public static string FormatTimingDrive(string? value, IAppLocalizer localizer) =>
        FormatKnownValue(value, localizer, TimingDriveKeys);

    public static string FormatTransmission(string? value, IAppLocalizer localizer) =>
        FormatKnownValue(value, localizer, TransmissionKeys);

    public static string FormatFuelType(string? value, IAppLocalizer localizer) =>
        FormatKnownValue(value, localizer, FuelTypeKeys);

    public static string FormatReminderRepeatMode(string? value, IAppLocalizer localizer) =>
        FormatKnownValue(value, localizer, ReminderRepeatModeKeys);

    private static string FormatKnownValue(string? value, IAppLocalizer localizer, IReadOnlyDictionary<string, string> keyByLegacyValue)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            return string.Empty;
        }

        return keyByLegacyValue.TryGetValue(normalized, out var key)
            ? localizer.GetString(key)
            : normalized;
    }

    private static IReadOnlyDictionary<string, string> CreateMap(params (string Value, string ResourceKey)[] pairs) =>
        pairs.ToDictionary(pair => pair.Value, pair => pair.ResourceKey, StringComparer.OrdinalIgnoreCase);
}
