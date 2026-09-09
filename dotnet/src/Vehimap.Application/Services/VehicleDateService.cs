// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Models;

namespace Vehimap.Application.Services;

public static class VehicleDateService
{
    private static readonly AppDateFormatService DateFormat = new();

    public static string NormalizeInput(string? text, AppCulturePreferences preferences)
    {
        var input = (text ?? string.Empty).Trim();
        // Check month precision first: DateOnly.TryParse can otherwise invent a day or year.
        if (VehimapValueParser.TryParseMonthYear(input, out var month))
        {
            return month.Year is >= 1900 and <= 2200
                ? month.ToString("MM/yyyy", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        var parts = input.Split(['.', '/', '-'], StringSplitOptions.TrimEntries);
        if (parts.Length != 3 || parts.Any(part => part.Length == 0 || !part.All(char.IsAsciiDigit))
            || (parts[0].Length != 4 && parts[2].Length != 4)
            || !DateFormat.TryParseDate(input, preferences, out var date)
            || date.Year is < 1900 or > 2200)
        {
            return string.Empty;
        }

        return VehimapValueParser.FormatCanonicalEventDate(date);
    }

    public static string FormatForDisplay(string? value, AppCulturePreferences preferences) =>
        VehimapValueParser.TryParseEventDate(value, out var date)
            ? DateFormat.FormatDate(date, preferences)
            : (value ?? string.Empty).Trim();

    public static bool TryGetBounds(string? value, out DateOnly first, out DateOnly last)
    {
        if (VehimapValueParser.TryParseEventDate(value, out first))
        {
            last = first;
            return true;
        }

        if (VehimapValueParser.TryParseMonthYear(value, out first))
        {
            last = new DateOnly(first.Year, first.Month, DateTime.DaysInMonth(first.Year, first.Month));
            return true;
        }

        last = default;
        return false;
    }

    public static bool TryGetDueDate(string? value, out DateOnly dueDate) =>
        TryGetBounds(value, out _, out dueDate);

    // A mixed-precision interval is invalid only when its bounds cannot overlap.
    public static bool HasInvalidRange(string? from, string? to) =>
        TryGetBounds(from, out var first, out _)
        && TryGetBounds(to, out _, out var last)
        && first > last;
}
