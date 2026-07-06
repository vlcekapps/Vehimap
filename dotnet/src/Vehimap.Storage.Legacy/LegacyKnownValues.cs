// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Storage.Legacy;

public static class LegacyKnownValues
{
    public const string VehicleStateNormalOperation = "Běžný provoz";
    public const string VehicleStateOutOfService = "Odstaveno";
    public const string VehicleStateArchive = "Archiv";
    public const string ReminderRepeatNone = "Neopakovat";
    public const string ReminderRepeatYearly = "Každý rok";
    public const string ReminderRepeatEveryTwoYears = "Každé 2 roky";
    public const string ReminderRepeatEveryFiveYears = "Každých 5 let";

    public static readonly string[] Categories =
    [
        "Osobní vozidla",
        "Motocykly",
        "Nákladní vozidla",
        "Autobusy",
        "Ostatní"
    ];

    public static readonly string[] RecordTypes =
    [
        "Povinné ručení",
        "Havarijní pojištění",
        "Asistence",
        "Doklad",
        "Servisní dokument",
        "Jiné"
    ];

    public static readonly string[] VehicleStates =
    [
        string.Empty,
        VehicleStateNormalOperation,
        "Veterán",
        VehicleStateOutOfService,
        "V renovaci",
        "Na prodej",
        VehicleStateArchive
    ];

    public static readonly string[] VehiclePowertrains =
    [
        string.Empty,
        "Benzín",
        "Nafta",
        "Hybrid",
        "Plug-in hybrid",
        "Elektro",
        "LPG / CNG",
        "Jiné"
    ];

    public static readonly string[] VehicleClimateProfiles =
    [
        string.Empty,
        "Má klimatizaci",
        "Bez klimatizace"
    ];

    public static readonly string[] VehicleTimingDrives =
    [
        string.Empty,
        "Řemen",
        "Řetěz",
        "Není relevantní"
    ];

    public static readonly string[] VehicleTransmissions =
    [
        string.Empty,
        "Manuální",
        "Automatická",
        "Není relevantní"
    ];

    public static readonly string[] FuelTypes =
    [
        string.Empty,
        "Benzin",
        "Nafta",
        "LPG",
        "CNG",
        "Elektřina",
        "Jiné"
    ];

    public static readonly string[] ReminderRepeatModes =
    [
        ReminderRepeatNone,
        ReminderRepeatYearly,
        ReminderRepeatEveryTwoYears,
        ReminderRepeatEveryFiveYears
    ];
}
