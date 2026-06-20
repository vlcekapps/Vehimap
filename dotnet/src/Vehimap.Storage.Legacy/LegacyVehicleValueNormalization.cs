using System.Text.RegularExpressions;

namespace Vehimap.Storage.Legacy;

public static partial class LegacyVehicleValueNormalization
{
    public static string NormalizeCategory(string? category)
    {
        var value = (category ?? string.Empty).Trim();
        if (string.Equals(value, "Osobní", StringComparison.Ordinal))
        {
            return "Osobní vozidla";
        }

        if (string.Equals(value, "Nákladní", StringComparison.Ordinal))
        {
            return "Nákladní vozidla";
        }

        foreach (var allowed in LegacyKnownValues.Categories)
        {
            if (string.Equals(allowed, value, StringComparison.Ordinal))
            {
                return allowed;
            }
        }

        return "Ostatní";
    }

    public static string NormalizeMonthYear(string? monthYear)
    {
        var value = (monthYear ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return string.Empty;
        }

        var match = MonthYearRegex().Match(value);
        if (!match.Success
            || !int.TryParse(match.Groups[1].Value, out var month)
            || !int.TryParse(match.Groups[2].Value, out var year)
            || month is < 1 or > 12
            || year is < 1900 or > 2200)
        {
            return string.Empty;
        }

        return $"{month:00}/{year:0000}";
    }

    public static bool TryGetMonthYearOrder(string? monthYear, out int order)
    {
        var normalized = NormalizeMonthYear(monthYear);
        if (normalized.Length == 0)
        {
            order = 0;
            return false;
        }

        order = int.Parse(normalized[3..], System.Globalization.CultureInfo.InvariantCulture) * 100
            + int.Parse(normalized[..2], System.Globalization.CultureInfo.InvariantCulture);
        return true;
    }

    [GeneratedRegex(@"^\s*(\d{1,2})\s*[/.-]\s*(\d{4})\s*$")]
    private static partial Regex MonthYearRegex();
}
