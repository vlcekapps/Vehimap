namespace Vehimap.Storage.Legacy;

public static class LegacyKnownValues
{
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
        "Běžný provoz",
        "Veterán",
        "Odstaveno",
        "V renovaci",
        "Na prodej",
        "Archiv"
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

    public static readonly string[] ReminderRepeatModes =
    [
        "Neopakovat",
        "Každý rok",
        "Každé 2 roky",
        "Každých 5 let"
    ];
}
