// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public static class KnownValueOptions
{
    private const string ResourceAliasPrefix = "res:";

    private static readonly KnownValueDefinition[] VehicleCategoryDefinitions =
    [
        Definition(LegacyKnownValues.Categories[0], "KnownValue.Category.PassengerVehicles", ResourceAlias("KnownValue.Category.PassengerVehicles.LegacyShort"), "Passenger vehicles"),
        Definition(LegacyKnownValues.Categories[1], "KnownValue.Category.Motorcycles", "Motorcycles"),
        Definition(LegacyKnownValues.Categories[2], "KnownValue.Category.Trucks", ResourceAlias("KnownValue.Category.Trucks.LegacyShort"), "Trucks"),
        Definition(LegacyKnownValues.Categories[3], "KnownValue.Category.Buses", "Buses"),
        Definition(LegacyKnownValues.Categories[4], "KnownValue.Category.Other", "Other")
    ];

    private static readonly KnownValueDefinition[] RecordTypeDefinitions =
    [
        Definition(LegacyKnownValues.RecordTypes[0], "KnownValue.RecordType.LiabilityInsurance", "Liability insurance"),
        Definition(LegacyKnownValues.RecordTypes[1], "KnownValue.RecordType.ComprehensiveInsurance", "Comprehensive insurance"),
        Definition(LegacyKnownValues.RecordTypes[2], "KnownValue.RecordType.Assistance", "Assistance"),
        Definition(LegacyKnownValues.RecordTypes[3], "KnownValue.RecordType.Document", "Document"),
        Definition(LegacyKnownValues.RecordTypes[4], "KnownValue.RecordType.ServiceDocument", "Service document"),
        Definition(LegacyKnownValues.RecordTypes[5], "KnownValue.RecordType.Other", "Other")
    ];

    private static readonly KnownValueDefinition[] VehicleStateDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.VehicleStates[1], "KnownValue.VehicleState.NormalOperation", "Normal operation"),
        Definition(LegacyKnownValues.VehicleStates[2], "KnownValue.VehicleState.Veteran", "Veteran"),
        Definition(LegacyKnownValues.VehicleStates[3], "KnownValue.VehicleState.OutOfService", "Out of service"),
        Definition(LegacyKnownValues.VehicleStates[4], "KnownValue.VehicleState.InRenovation", "In renovation"),
        Definition(LegacyKnownValues.VehicleStates[5], "KnownValue.VehicleState.ForSale", "For sale"),
        Definition(LegacyKnownValues.VehicleStates[6], "KnownValue.VehicleState.Archive", "Archive")
    ];

    private static readonly KnownValueDefinition[] VehiclePowertrainDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.VehiclePowertrains[1], "KnownValue.Powertrain.Gasoline", "Benzin", "Gasoline"),
        Definition(LegacyKnownValues.VehiclePowertrains[2], "KnownValue.Powertrain.Diesel", "Diesel"),
        Definition(LegacyKnownValues.VehiclePowertrains[3], "KnownValue.Powertrain.Hybrid", "Hybrid"),
        Definition(LegacyKnownValues.VehiclePowertrains[4], "KnownValue.Powertrain.PluginHybrid", "Plug-in hybrid"),
        Definition(LegacyKnownValues.VehiclePowertrains[5], "KnownValue.Powertrain.Electric", "Electric"),
        Definition(LegacyKnownValues.VehiclePowertrains[6], "KnownValue.Powertrain.LpgCng", "LPG / CNG"),
        Definition(LegacyKnownValues.VehiclePowertrains[7], "KnownValue.Powertrain.Other", "Other")
    ];

    private static readonly KnownValueDefinition[] VehicleClimateProfileDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.VehicleClimateProfiles[1], "KnownValue.Climate.HasAirConditioning", "Has air conditioning"),
        Definition(LegacyKnownValues.VehicleClimateProfiles[2], "KnownValue.Climate.NoAirConditioning", "No air conditioning")
    ];

    private static readonly KnownValueDefinition[] VehicleTimingDriveDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.VehicleTimingDrives[1], "KnownValue.TimingDrive.Belt", "Belt"),
        Definition(LegacyKnownValues.VehicleTimingDrives[2], "KnownValue.TimingDrive.Chain", "Chain"),
        Definition(LegacyKnownValues.VehicleTimingDrives[3], "KnownValue.Common.NotRelevant", "Not relevant")
    ];

    private static readonly KnownValueDefinition[] VehicleTransmissionDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.VehicleTransmissions[1], "KnownValue.Transmission.Manual", "Manual"),
        Definition(LegacyKnownValues.VehicleTransmissions[2], "KnownValue.Transmission.Automatic", "Automatic"),
        Definition(LegacyKnownValues.VehicleTransmissions[3], "KnownValue.Common.NotRelevant", "Not relevant")
    ];

    private static readonly KnownValueDefinition[] FuelTypeDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.FuelTypes[1], "KnownValue.FuelType.Gasoline", "Gasoline"),
        Definition(LegacyKnownValues.FuelTypes[2], "KnownValue.FuelType.Diesel", "Diesel"),
        Definition(LegacyKnownValues.FuelTypes[3], "KnownValue.FuelType.Lpg", "LPG"),
        Definition(LegacyKnownValues.FuelTypes[4], "KnownValue.FuelType.Cng", "CNG"),
        Definition(LegacyKnownValues.FuelTypes[5], "KnownValue.FuelType.Electricity", "Electricity"),
        Definition(LegacyKnownValues.FuelTypes[6], "KnownValue.FuelType.Other", "Other")
    ];

    private static readonly KnownValueDefinition[] ReminderRepeatModeDefinitions =
    [
        Definition(LegacyKnownValues.ReminderRepeatNone, "KnownValue.ReminderRepeat.None", "Do not repeat", "None"),
        Definition(LegacyKnownValues.ReminderRepeatYearly, "KnownValue.ReminderRepeat.Yearly", "Every year", "Yearly", "Rocne", ResourceAlias("KnownValue.ReminderRepeat.Yearly.LegacyAdverb")),
        Definition(LegacyKnownValues.ReminderRepeatEveryTwoYears, "KnownValue.ReminderRepeat.EveryTwoYears", "Every 2 years", "Every two years"),
        Definition(LegacyKnownValues.ReminderRepeatEveryFiveYears, "KnownValue.ReminderRepeat.EveryFiveYears", "Every 5 years", "Every five years")
    ];

    public static LocalizedOptionViewModel DefaultVehicleCategory => SelectVehicleCategory(LegacyKnownValues.Categories[0]);

    public static LocalizedOptionViewModel DefaultRecordType => SelectRecordType(LegacyKnownValues.RecordTypes[3]);

    public static LocalizedOptionViewModel DefaultReminderRepeatMode => SelectReminderRepeatMode(LegacyKnownValues.ReminderRepeatNone);

    public static IReadOnlyList<LocalizedOptionViewModel> VehicleCategories(string? currentValue = null) =>
        BuildOptions(VehicleCategoryDefinitions, currentValue);

    public static IReadOnlyList<LocalizedOptionViewModel> RecordTypes(string? currentValue = null) =>
        BuildOptions(RecordTypeDefinitions, currentValue);

    public static IReadOnlyList<LocalizedOptionViewModel> VehicleStates(string? currentValue = null) =>
        BuildOptions(VehicleStateDefinitions, currentValue);

    public static IReadOnlyList<LocalizedOptionViewModel> VehiclePowertrains(string? currentValue = null) =>
        BuildOptions(VehiclePowertrainDefinitions, currentValue);

    public static IReadOnlyList<LocalizedOptionViewModel> VehicleClimateProfiles(string? currentValue = null) =>
        BuildOptions(VehicleClimateProfileDefinitions, currentValue);

    public static IReadOnlyList<LocalizedOptionViewModel> VehicleTimingDrives(string? currentValue = null) =>
        BuildOptions(VehicleTimingDriveDefinitions, currentValue);

    public static IReadOnlyList<LocalizedOptionViewModel> VehicleTransmissions(string? currentValue = null) =>
        BuildOptions(VehicleTransmissionDefinitions, currentValue);

    public static IReadOnlyList<LocalizedOptionViewModel> FuelTypes(string? currentValue = null) =>
        BuildOptions(FuelTypeDefinitions, currentValue);

    public static IReadOnlyList<LocalizedOptionViewModel> ReminderRepeatModes(string? currentValue = null) =>
        BuildOptions(ReminderRepeatModeDefinitions, currentValue);

    public static LocalizedOptionViewModel SelectVehicleCategory(string? value) =>
        SelectOption(VehicleCategoryDefinitions, value);

    public static LocalizedOptionViewModel SelectRecordType(string? value) =>
        SelectOption(RecordTypeDefinitions, value);

    public static LocalizedOptionViewModel SelectVehicleState(string? value) =>
        SelectOption(VehicleStateDefinitions, value);

    public static LocalizedOptionViewModel SelectVehiclePowertrain(string? value) =>
        SelectOption(VehiclePowertrainDefinitions, value);

    public static LocalizedOptionViewModel SelectVehicleClimateProfile(string? value) =>
        SelectOption(VehicleClimateProfileDefinitions, value);

    public static LocalizedOptionViewModel SelectVehicleTimingDrive(string? value) =>
        SelectOption(VehicleTimingDriveDefinitions, value);

    public static LocalizedOptionViewModel SelectVehicleTransmission(string? value) =>
        SelectOption(VehicleTransmissionDefinitions, value);

    public static LocalizedOptionViewModel SelectFuelType(string? value) =>
        SelectOption(FuelTypeDefinitions, value);

    public static LocalizedOptionViewModel SelectReminderRepeatMode(string? value) =>
        SelectReminderRepeatOption(value);

    public static string NormalizeVehicleCategoryValue(string? value) =>
        SelectVehicleCategory(value).Value;

    public static string NormalizeRecordTypeValue(string? value) =>
        SelectRecordType(value).Value;

    public static string NormalizeVehicleStateValue(string? value) =>
        SelectVehicleState(value).Value;

    public static string NormalizeVehiclePowertrainValue(string? value) =>
        SelectVehiclePowertrain(value).Value;

    public static string NormalizeVehicleClimateProfileValue(string? value) =>
        SelectVehicleClimateProfile(value).Value;

    public static string NormalizeVehicleTimingDriveValue(string? value) =>
        SelectVehicleTimingDrive(value).Value;

    public static string NormalizeVehicleTransmissionValue(string? value) =>
        SelectVehicleTransmission(value).Value;

    public static string NormalizeFuelTypeValue(string? value) =>
        SelectFuelType(value).Value;

    public static string NormalizeReminderRepeatModeValue(string? value) =>
        SelectReminderRepeatMode(value).Value;

    private static IReadOnlyList<LocalizedOptionViewModel> BuildOptions(IReadOnlyList<KnownValueDefinition> definitions, string? currentValue)
    {
        var options = definitions.Select(CreateOption).ToList();
        var selected = SelectOption(definitions, currentValue);
        if (!options.Any(option => string.Equals(option.Value, selected.Value, StringComparison.OrdinalIgnoreCase)))
        {
            options.Add(selected);
        }

        return options;
    }

    private static LocalizedOptionViewModel SelectReminderRepeatOption(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return SelectOption(ReminderRepeatModeDefinitions, LegacyKnownValues.ReminderRepeatModes[0]);
        }

        var selected = SelectOption(ReminderRepeatModeDefinitions, value);
        if (!string.Equals(selected.Value, (value ?? string.Empty).Trim(), StringComparison.Ordinal))
        {
            return selected;
        }

        return LegacyVehicleValueNormalization.NormalizeReminderRepeatMode(value) is { } normalized
            && !string.Equals(normalized, LegacyKnownValues.ReminderRepeatModes[0], StringComparison.Ordinal)
                ? SelectOption(ReminderRepeatModeDefinitions, normalized)
                : selected;
    }

    private static LocalizedOptionViewModel SelectOption(IReadOnlyList<KnownValueDefinition> definitions, string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        foreach (var definition in definitions)
        {
            if (Matches(definition, normalized))
            {
                return CreateOption(definition);
            }
        }

        return new LocalizedOptionViewModel(normalized, normalized);
    }

    private static bool Matches(KnownValueDefinition definition, string value)
    {
        if (string.Equals(definition.Value, value, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(definition.ResourceKey)
            && LocalizedCompatibilityAliases.MatchesAnyResource(value, definition.ResourceKey))
        {
            return true;
        }

        return definition.Aliases.Any(alias => MatchesAlias(alias, value));
    }

    private static LocalizedOptionViewModel CreateOption(KnownValueDefinition definition)
    {
        var label = string.IsNullOrWhiteSpace(definition.ResourceKey)
            ? definition.Value
            : DesktopLocalization.Localizer.GetString(definition.ResourceKey);
        return new LocalizedOptionViewModel(definition.Value, label);
    }

    private static KnownValueDefinition Definition(string value, string resourceKey, params string[] aliases) =>
        new(value, resourceKey, aliases);

    private static KnownValueDefinition EmptyDefinition() =>
        new(string.Empty, string.Empty, []);

    private static string ResourceAlias(string resourceKey) =>
        ResourceAliasPrefix + resourceKey;

    private static bool MatchesAlias(string alias, string value)
    {
        if (alias.StartsWith(ResourceAliasPrefix, StringComparison.Ordinal))
        {
            return LocalizedCompatibilityAliases.MatchesAnyResource(value, alias[ResourceAliasPrefix.Length..]);
        }

        return string.Equals(alias, value, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record KnownValueDefinition(string Value, string ResourceKey, IReadOnlyList<string> Aliases);
}
