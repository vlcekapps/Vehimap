// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Desktop.Services;

internal static class DesktopEntityKinds
{
    public const string Vehicle = "vehicle";
    public const string History = "history";
    public const string Fuel = "fuel";
    public const string Record = "record";
    public const string Maintenance = "maintenance";
    public const string Reminder = "reminder";
    public const string Costs = "costs";

    public static string Normalize(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Vehicle;
        }

        return normalized.ToLowerInvariant() switch
        {
            Vehicle or "vozidlo" => Vehicle,
            History or "historie" => History,
            Fuel or "tankování" or "tankovani" or "fuel" => Fuel,
            Record or "doklad" or "doklady" => Record,
            Maintenance or "údržba" or "udrzba" or "servis" => Maintenance,
            Reminder or "připomínka" or "pripominka" or "připomínky" or "pripominky" => Reminder,
            Costs or "náklady" or "naklady" or "cost" => Costs,
            _ => normalized
        };
    }
}
