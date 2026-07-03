// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public static class KnownValueOptions
{
    private static readonly KnownValueDefinition[] VehicleCategoryDefinitions =
    [
        Definition("Osobní vozidla", "KnownValue.Category.PassengerVehicles", "Osobní", "Passenger vehicles"),
        Definition("Motocykly", "KnownValue.Category.Motorcycles", "Motorcycles"),
        Definition("Nákladní vozidla", "KnownValue.Category.Trucks", "Nákladní", "Trucks"),
        Definition("Autobusy", "KnownValue.Category.Buses", "Buses"),
        Definition("Ostatní", "KnownValue.Category.Other", "Other")
    ];

    private static readonly KnownValueDefinition[] RecordTypeDefinitions =
    [
        Definition("Povinné ručení", "KnownValue.RecordType.LiabilityInsurance", "Liability insurance"),
        Definition("Havarijní pojištění", "KnownValue.RecordType.ComprehensiveInsurance", "Comprehensive insurance"),
        Definition("Asistence", "KnownValue.RecordType.Assistance", "Assistance"),
        Definition("Doklad", "KnownValue.RecordType.Document", "Document"),
        Definition("Servisní dokument", "KnownValue.RecordType.ServiceDocument", "Service document"),
        Definition("Jiné", "KnownValue.RecordType.Other", "Other")
    ];

    private static readonly KnownValueDefinition[] VehicleStateDefinitions =
    [
        EmptyDefinition(),
        Definition("Běžný provoz", "KnownValue.VehicleState.NormalOperation", "Normal operation"),
        Definition("Veterán", "KnownValue.VehicleState.Veteran", "Veteran"),
        Definition("Odstaveno", "KnownValue.VehicleState.OutOfService", "Out of service"),
        Definition("V renovaci", "KnownValue.VehicleState.InRenovation", "In renovation"),
        Definition("Na prodej", "KnownValue.VehicleState.ForSale", "For sale"),
        Definition("Archiv", "KnownValue.VehicleState.Archive", "Archive")
    ];

    private static readonly KnownValueDefinition[] VehiclePowertrainDefinitions =
    [
        EmptyDefinition(),
        Definition("Benzín", "KnownValue.Powertrain.Gasoline", "Benzin", "Gasoline"),
        Definition("Nafta", "KnownValue.Powertrain.Diesel", "Diesel"),
        Definition("Hybrid", "KnownValue.Powertrain.Hybrid", "Hybrid"),
        Definition("Plug-in hybrid", "KnownValue.Powertrain.PluginHybrid", "Plug-in hybrid"),
        Definition("Elektro", "KnownValue.Powertrain.Electric", "Electric"),
        Definition("LPG / CNG", "KnownValue.Powertrain.LpgCng", "LPG / CNG"),
        Definition("Jiné", "KnownValue.Powertrain.Other", "Other")
    ];

    private static readonly KnownValueDefinition[] VehicleClimateProfileDefinitions =
    [
        EmptyDefinition(),
        Definition("Má klimatizaci", "KnownValue.Climate.HasAirConditioning", "Has air conditioning"),
        Definition("Bez klimatizace", "KnownValue.Climate.NoAirConditioning", "No air conditioning")
    ];

    private static readonly KnownValueDefinition[] VehicleTimingDriveDefinitions =
    [
        EmptyDefinition(),
        Definition("Řemen", "KnownValue.TimingDrive.Belt", "Belt"),
        Definition("Řetěz", "KnownValue.TimingDrive.Chain", "Chain"),
        Definition("Není relevantní", "KnownValue.Common.NotRelevant", "Not relevant")
    ];

    private static readonly KnownValueDefinition[] VehicleTransmissionDefinitions =
    [
        EmptyDefinition(),
        Definition("Manuální", "KnownValue.Transmission.Manual", "Manual"),
        Definition("Automatická", "KnownValue.Transmission.Automatic", "Automatic"),
        Definition("Není relevantní", "KnownValue.Common.NotRelevant", "Not relevant")
    ];

    private static readonly KnownValueDefinition[] FuelTypeDefinitions =
    [
        EmptyDefinition(),
        Definition("Benzin", "KnownValue.FuelType.Gasoline", "Benzín", "Gasoline"),
        Definition("Nafta", "KnownValue.FuelType.Diesel", "Diesel"),
        Definition("LPG", "KnownValue.FuelType.Lpg", "LPG"),
        Definition("CNG", "KnownValue.FuelType.Cng", "CNG"),
        Definition("Elektřina", "KnownValue.FuelType.Electricity", "Electricity"),
        Definition("Jiné", "KnownValue.FuelType.Other", "Other")
    ];

    private static readonly KnownValueDefinition[] ReminderRepeatModeDefinitions =
    [
        Definition("Neopakovat", "KnownValue.ReminderRepeat.None", "Do not repeat", "None"),
        Definition("Každý rok", "KnownValue.ReminderRepeat.Yearly", "Every year", "Yearly", "Rocne", "Ročně"),
        Definition("Každé 2 roky", "KnownValue.ReminderRepeat.EveryTwoYears", "Every 2 years", "Every two years"),
        Definition("Každých 5 let", "KnownValue.ReminderRepeat.EveryFiveYears", "Every 5 years", "Every five years")
    ];

    public static LocalizedOptionViewModel DefaultVehicleCategory => SelectVehicleCategory("Osobní vozidla");

    public static LocalizedOptionViewModel DefaultRecordType => SelectRecordType("Doklad");

    public static LocalizedOptionViewModel DefaultReminderRepeatMode => SelectReminderRepeatMode("Neopakovat");

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
            && string.Equals(DesktopLocalization.Localizer.GetString(definition.ResourceKey), value, StringComparison.CurrentCultureIgnoreCase))
        {
            return true;
        }

        return definition.Aliases.Any(alias => string.Equals(alias, value, StringComparison.OrdinalIgnoreCase));
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

    private sealed record KnownValueDefinition(string Value, string ResourceKey, IReadOnlyList<string> Aliases);
}
