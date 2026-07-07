// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.Services;

internal static class DesktopEntityKinds
{
    public const string Vehicle = ApplicationEntityKinds.Vehicle;
    public const string History = ApplicationEntityKinds.History;
    public const string Fuel = ApplicationEntityKinds.Fuel;
    public const string Record = ApplicationEntityKinds.Record;
    public const string Maintenance = ApplicationEntityKinds.Maintenance;
    public const string Reminder = ApplicationEntityKinds.Reminder;
    public const string Costs = ApplicationEntityKinds.Costs;

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
