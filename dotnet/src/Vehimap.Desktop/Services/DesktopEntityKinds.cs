// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

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

        if (MatchesEntityKind(normalized, Vehicle, "DesktopEntityKind.Vehicle"))
        {
            return Vehicle;
        }

        if (MatchesEntityKind(normalized, History, "DesktopEntityKind.History"))
        {
            return History;
        }

        if (MatchesEntityKind(normalized, Fuel, "DesktopEntityKind.Fuel", "tankovani", "fuel"))
        {
            return Fuel;
        }

        if (MatchesEntityKind(normalized, Record, "DesktopEntityKind.Record", "DesktopEntityKind.Records", "doklad", "doklady"))
        {
            return Record;
        }

        if (MatchesEntityKind(normalized, Maintenance, "DesktopEntityKind.Maintenance", "DesktopEntityKind.Maintenance.ServiceAlias", "udrzba", "servis"))
        {
            return Maintenance;
        }

        if (MatchesEntityKind(normalized, Reminder, "DesktopEntityKind.Reminder", "DesktopEntityKind.Reminders", "pripominka", "pripominky"))
        {
            return Reminder;
        }

        if (MatchesEntityKind(normalized, Costs, "DesktopEntityKind.Costs", "naklady", "cost"))
        {
            return Costs;
        }

        return normalized;
    }

    private static bool MatchesEntityKind(string value, string stableValue, params string[] aliases)
    {
        if (string.Equals(value, stableValue, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var resourceKeys = aliases
            .Where(alias => alias.StartsWith("DesktopEntityKind.", StringComparison.Ordinal))
            .ToArray();
        if (resourceKeys.Length > 0 && LocalizedCompatibilityAliases.MatchesAnyResource(value, resourceKeys))
        {
            return true;
        }

        return aliases
            .Where(alias => !alias.StartsWith("DesktopEntityKind.", StringComparison.Ordinal))
            .Any(alias => string.Equals(value, alias, StringComparison.OrdinalIgnoreCase));
    }
}
