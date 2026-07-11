// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public static class KnownValueOptions
{
    private static readonly KnownValueDefinition[] VehicleCategoryDefinitions =
    [
        Definition(LegacyKnownValues.Categories[0], "KnownValue.Category.PassengerVehicles", "KnownValue.Category.PassengerVehicles.LegacyShort"),
        Definition(LegacyKnownValues.Categories[1], "KnownValue.Category.Motorcycles"),
        Definition(LegacyKnownValues.Categories[2], "KnownValue.Category.Trucks", "KnownValue.Category.Trucks.LegacyShort"),
        Definition(LegacyKnownValues.Categories[3], "KnownValue.Category.Buses"),
        Definition(LegacyKnownValues.Categories[4], "KnownValue.Category.Other")
    ];

    private static readonly KnownValueDefinition[] RecordTypeDefinitions =
    [
        Definition(LegacyKnownValues.RecordTypes[0], "KnownValue.RecordType.LiabilityInsurance"),
        Definition(LegacyKnownValues.RecordTypes[1], "KnownValue.RecordType.ComprehensiveInsurance"),
        Definition(LegacyKnownValues.RecordTypes[2], "KnownValue.RecordType.Assistance"),
        Definition(LegacyKnownValues.RecordTypes[3], "KnownValue.RecordType.Document"),
        Definition(LegacyKnownValues.RecordTypes[4], "KnownValue.RecordType.ServiceDocument"),
        Definition(LegacyKnownValues.RecordTypes[5], "KnownValue.RecordType.Other")
    ];

    private static readonly KnownValueDefinition[] VehicleStateDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.VehicleStates[1], "KnownValue.VehicleState.NormalOperation"),
        Definition(LegacyKnownValues.VehicleStates[2], "KnownValue.VehicleState.Veteran"),
        Definition(LegacyKnownValues.VehicleStates[3], "KnownValue.VehicleState.OutOfService"),
        Definition(LegacyKnownValues.VehicleStates[4], "KnownValue.VehicleState.InRenovation"),
        Definition(LegacyKnownValues.VehicleStates[5], "KnownValue.VehicleState.ForSale"),
        Definition(LegacyKnownValues.VehicleStates[6], "KnownValue.VehicleState.Archive")
    ];

    private static readonly KnownValueDefinition[] VehiclePowertrainDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.VehiclePowertrains[1], "KnownValue.Powertrain.Gasoline", "KnownValue.Powertrain.Gasoline.LegacyAscii"),
        Definition(LegacyKnownValues.VehiclePowertrains[2], "KnownValue.Powertrain.Diesel"),
        Definition(LegacyKnownValues.VehiclePowertrains[3], "KnownValue.Powertrain.Hybrid"),
        Definition(LegacyKnownValues.VehiclePowertrains[4], "KnownValue.Powertrain.PluginHybrid"),
        Definition(LegacyKnownValues.VehiclePowertrains[5], "KnownValue.Powertrain.Electric"),
        Definition(LegacyKnownValues.VehiclePowertrains[6], "KnownValue.Powertrain.LpgCng"),
        Definition(LegacyKnownValues.VehiclePowertrains[7], "KnownValue.Powertrain.Other")
    ];

    private static readonly KnownValueDefinition[] VehicleClimateProfileDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.VehicleClimateProfiles[1], "KnownValue.Climate.HasAirConditioning"),
        Definition(LegacyKnownValues.VehicleClimateProfiles[2], "KnownValue.Climate.NoAirConditioning")
    ];

    private static readonly KnownValueDefinition[] VehicleTimingDriveDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.VehicleTimingDrives[1], "KnownValue.TimingDrive.Belt"),
        Definition(LegacyKnownValues.VehicleTimingDrives[2], "KnownValue.TimingDrive.Chain"),
        Definition(LegacyKnownValues.VehicleTimingDrives[3], "KnownValue.Common.NotRelevant")
    ];

    private static readonly KnownValueDefinition[] VehicleTransmissionDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.VehicleTransmissions[1], "KnownValue.Transmission.Manual"),
        Definition(LegacyKnownValues.VehicleTransmissions[2], "KnownValue.Transmission.Automatic"),
        Definition(LegacyKnownValues.VehicleTransmissions[3], "KnownValue.Common.NotRelevant")
    ];

    private static readonly KnownValueDefinition[] FuelTypeDefinitions =
    [
        EmptyDefinition(),
        Definition(LegacyKnownValues.FuelTypes[1], "KnownValue.FuelType.Gasoline", "KnownValue.FuelType.Gasoline.LegacyAscii"),
        Definition(LegacyKnownValues.FuelTypes[2], "KnownValue.FuelType.Diesel"),
        Definition(LegacyKnownValues.FuelTypes[3], "KnownValue.FuelType.Lpg"),
        Definition(LegacyKnownValues.FuelTypes[4], "KnownValue.FuelType.Cng"),
        Definition(LegacyKnownValues.FuelTypes[5], "KnownValue.FuelType.Electricity"),
        Definition(LegacyKnownValues.FuelTypes[6], "KnownValue.FuelType.Other")
    ];

    private static readonly KnownValueDefinition[] ReminderRepeatModeDefinitions =
    [
        Definition(LegacyKnownValues.ReminderRepeatNone, "KnownValue.ReminderRepeat.None", "KnownValue.ReminderRepeat.None.LegacyShort"),
        Definition(LegacyKnownValues.ReminderRepeatYearly, "KnownValue.ReminderRepeat.Yearly", "KnownValue.ReminderRepeat.Yearly.LegacyAdverb", "KnownValue.ReminderRepeat.Yearly.LegacyAscii"),
        Definition(LegacyKnownValues.ReminderRepeatEveryTwoYears, "KnownValue.ReminderRepeat.EveryTwoYears", "KnownValue.ReminderRepeat.EveryTwoYears.LegacyWords"),
        Definition(LegacyKnownValues.ReminderRepeatEveryFiveYears, "KnownValue.ReminderRepeat.EveryFiveYears", "KnownValue.ReminderRepeat.EveryFiveYears.LegacyWords")
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

        return definition.AliasResourceKeys.Any(
            resourceKey => LocalizedCompatibilityAliases.MatchesAnyResource(value, resourceKey));
    }

    private static LocalizedOptionViewModel CreateOption(KnownValueDefinition definition)
    {
        var label = string.IsNullOrWhiteSpace(definition.ResourceKey)
            ? definition.Value
            : DesktopLocalization.Localizer.GetString(definition.ResourceKey);
        return new LocalizedOptionViewModel(definition.Value, label);
    }

    private static KnownValueDefinition Definition(string value, string resourceKey, params string[] aliasResourceKeys) =>
        new(value, resourceKey, aliasResourceKeys);

    private static KnownValueDefinition EmptyDefinition() =>
        new(string.Empty, string.Empty, []);

    private sealed record KnownValueDefinition(string Value, string ResourceKey, IReadOnlyList<string> AliasResourceKeys);
}
