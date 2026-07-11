// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;

namespace Vehimap.Application.Services;

public static class LegacyKnownValueDisplayService
{
    private static readonly KnownValueDisplayDefinition[] CategoryKeys =
    [
        Definition("KnownValue.Category.PassengerVehicles", "KnownValue.Category.PassengerVehicles.LegacyShort"),
        Definition("KnownValue.Category.Motorcycles"),
        Definition("KnownValue.Category.Trucks", "KnownValue.Category.Trucks.LegacyShort"),
        Definition("KnownValue.Category.Buses"),
        Definition("KnownValue.Category.Other")
    ];

    private static readonly KnownValueDisplayDefinition[] RecordTypeKeys = CreateDefinitions(
        "KnownValue.RecordType.LiabilityInsurance",
        "KnownValue.RecordType.ComprehensiveInsurance",
        "KnownValue.RecordType.Assistance",
        "KnownValue.RecordType.Document",
        "KnownValue.RecordType.ServiceDocument",
        "KnownValue.RecordType.Other");

    private static readonly KnownValueDisplayDefinition[] VehicleStateKeys = CreateDefinitions(
        "KnownValue.VehicleState.NormalOperation",
        "KnownValue.VehicleState.Veteran",
        "KnownValue.VehicleState.OutOfService",
        "KnownValue.VehicleState.InRenovation",
        "KnownValue.VehicleState.ForSale",
        "KnownValue.VehicleState.Archive");

    private static readonly KnownValueDisplayDefinition[] PowertrainKeys =
    [
        Definition("KnownValue.Powertrain.Gasoline", "KnownValue.Powertrain.Gasoline.LegacyAscii"),
        Definition("KnownValue.Powertrain.Diesel"),
        Definition("KnownValue.Powertrain.Hybrid"),
        Definition("KnownValue.Powertrain.PluginHybrid"),
        Definition("KnownValue.Powertrain.Electric"),
        Definition("KnownValue.Powertrain.LpgCng"),
        Definition("KnownValue.Powertrain.Other")
    ];

    private static readonly KnownValueDisplayDefinition[] ClimateProfileKeys = CreateDefinitions(
        "KnownValue.Climate.HasAirConditioning",
        "KnownValue.Climate.NoAirConditioning");

    private static readonly KnownValueDisplayDefinition[] TimingDriveKeys = CreateDefinitions(
        "KnownValue.TimingDrive.Belt",
        "KnownValue.TimingDrive.Chain",
        "KnownValue.Common.NotRelevant");

    private static readonly KnownValueDisplayDefinition[] TransmissionKeys = CreateDefinitions(
        "KnownValue.Transmission.Manual",
        "KnownValue.Transmission.Automatic",
        "KnownValue.Common.NotRelevant");

    private static readonly KnownValueDisplayDefinition[] FuelTypeKeys =
    [
        Definition("KnownValue.FuelType.Gasoline", "KnownValue.FuelType.Gasoline.LegacyAscii"),
        Definition("KnownValue.FuelType.Diesel"),
        Definition("KnownValue.FuelType.Lpg"),
        Definition("KnownValue.FuelType.Cng"),
        Definition("KnownValue.FuelType.Electricity"),
        Definition("KnownValue.FuelType.Other")
    ];

    private static readonly KnownValueDisplayDefinition[] ReminderRepeatModeKeys =
    [
        Definition("KnownValue.ReminderRepeat.None", "KnownValue.ReminderRepeat.None.LegacyShort"),
        Definition(
            "KnownValue.ReminderRepeat.Yearly",
            "KnownValue.ReminderRepeat.Yearly.LegacyAdverb",
            "KnownValue.ReminderRepeat.Yearly.LegacyAscii"),
        Definition("KnownValue.ReminderRepeat.EveryTwoYears", "KnownValue.ReminderRepeat.EveryTwoYears.LegacyWords"),
        Definition("KnownValue.ReminderRepeat.EveryFiveYears", "KnownValue.ReminderRepeat.EveryFiveYears.LegacyWords")
    ];

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

    private static string FormatKnownValue(string? value, IAppLocalizer localizer, IReadOnlyList<KnownValueDisplayDefinition> definitions)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            return string.Empty;
        }

        foreach (var definition in definitions)
        {
            if (LocalizedResourceValueMatcher.Matches(localizer, normalized, [.. definition.MatchResourceKeys]))
            {
                return localizer.GetString(definition.DisplayResourceKey);
            }
        }

        return normalized;
    }

    private static KnownValueDisplayDefinition[] CreateDefinitions(params string[] resourceKeys) =>
        resourceKeys.Select(resourceKey => Definition(resourceKey)).ToArray();

    private static KnownValueDisplayDefinition Definition(string displayResourceKey, params string[] extraMatchResourceKeys) =>
        new(displayResourceKey, [displayResourceKey, .. extraMatchResourceKeys]);

    private sealed record KnownValueDisplayDefinition(string DisplayResourceKey, IReadOnlyList<string> MatchResourceKeys);
}
