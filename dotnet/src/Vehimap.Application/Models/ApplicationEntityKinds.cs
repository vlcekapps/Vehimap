// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public static class ApplicationEntityKinds
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

        return NormalizeForComparison(normalized) switch
        {
            Vehicle or "vozidlo" => Vehicle,
            History or "historie" => History,
            Fuel or "tankovani" => Fuel,
            Record or "doklad" or "doklady" => Record,
            Maintenance or "udrzba" or "servis" => Maintenance,
            Reminder or "pripominka" or "pripominky" => Reminder,
            Costs or "naklady" or "cost" => Costs,
            _ => normalized
        };
    }

    private static string NormalizeForComparison(string value) =>
        value.Trim().ToLowerInvariant()
            .Replace("\u00E1", "a", StringComparison.Ordinal)
            .Replace("\u010D", "c", StringComparison.Ordinal)
            .Replace("\u010F", "d", StringComparison.Ordinal)
            .Replace("\u00E9", "e", StringComparison.Ordinal)
            .Replace("\u011B", "e", StringComparison.Ordinal)
            .Replace("\u00ED", "i", StringComparison.Ordinal)
            .Replace("\u0148", "n", StringComparison.Ordinal)
            .Replace("\u00F3", "o", StringComparison.Ordinal)
            .Replace("\u0159", "r", StringComparison.Ordinal)
            .Replace("\u0161", "s", StringComparison.Ordinal)
            .Replace("\u0165", "t", StringComparison.Ordinal)
            .Replace("\u00FA", "u", StringComparison.Ordinal)
            .Replace("\u016F", "u", StringComparison.Ordinal)
            .Replace("\u00FD", "y", StringComparison.Ordinal)
            .Replace("\u017E", "z", StringComparison.Ordinal);
}
