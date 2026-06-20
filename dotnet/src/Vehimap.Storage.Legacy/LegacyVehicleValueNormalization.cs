using System.Text.RegularExpressions;

namespace Vehimap.Storage.Legacy;

public static partial class LegacyVehicleValueNormalization
{
    private static readonly string[] EventDateFormats =
    [
        "dd.MM.yyyy",
        "d.M.yyyy",
        "dd/MM/yyyy",
        "d/M/yyyy",
        "dd-MM-yyyy",
        "d-M-yyyy",
        "yyyy-MM-dd",
        "yyyy/MM/dd"
    ];

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

    public static string NormalizeRecordType(string? recordType)
    {
        var value = (recordType ?? string.Empty).Trim();
        foreach (var allowed in LegacyKnownValues.RecordTypes)
        {
            if (string.Equals(allowed, value, StringComparison.Ordinal))
            {
                return allowed;
            }
        }

        return LegacyKnownValues.RecordTypes[0];
    }

    public static string NormalizeReminderRepeatMode(string? repeatMode)
    {
        var value = (repeatMode ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return LegacyKnownValues.ReminderRepeatModes[0];
        }

        foreach (var allowed in LegacyKnownValues.ReminderRepeatModes)
        {
            if (string.Equals(allowed, value, StringComparison.Ordinal))
            {
                return allowed;
            }
        }

        var folded = value
            .ToLowerInvariant()
            .Replace("\u00A0", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("á", "a", StringComparison.Ordinal)
            .Replace("č", "c", StringComparison.Ordinal)
            .Replace("ď", "d", StringComparison.Ordinal)
            .Replace("é", "e", StringComparison.Ordinal)
            .Replace("ě", "e", StringComparison.Ordinal)
            .Replace("í", "i", StringComparison.Ordinal)
            .Replace("ň", "n", StringComparison.Ordinal)
            .Replace("ó", "o", StringComparison.Ordinal)
            .Replace("ř", "r", StringComparison.Ordinal)
            .Replace("š", "s", StringComparison.Ordinal)
            .Replace("ť", "t", StringComparison.Ordinal)
            .Replace("ú", "u", StringComparison.Ordinal)
            .Replace("ů", "u", StringComparison.Ordinal)
            .Replace("ý", "y", StringComparison.Ordinal)
            .Replace("ž", "z", StringComparison.Ordinal);

        if (folded.Contains("5"))
        {
            return "Každých 5 let";
        }

        if (folded.Contains('2'))
        {
            return "Každé 2 roky";
        }

        if (folded.Contains("rok", StringComparison.Ordinal) || folded.Contains("rocne", StringComparison.Ordinal))
        {
            return "Každý rok";
        }

        return LegacyKnownValues.ReminderRepeatModes[0];
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

    public static string NormalizeEventDate(string? eventDate)
    {
        if (!DateOnly.TryParseExact(
                (eventDate ?? string.Empty).Trim(),
                EventDateFormats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var value)
            || value.Year is < 1900 or > 2200)
        {
            return string.Empty;
        }

        return value.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string NormalizeOdometer(string? odometer)
    {
        var value = (odometer ?? string.Empty).Trim()
            .Replace("\u00A0", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        return int.TryParse(value, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
               && parsed >= 0
            ? parsed.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : string.Empty;
    }

    public static string NormalizePositiveInteger(string? value)
    {
        var normalized = (value ?? string.Empty).Trim()
            .Replace("\u00A0", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        return int.TryParse(normalized, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
               && parsed > 0
            ? parsed.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : string.Empty;
    }

    public static string NormalizeReminderDays(string? value)
    {
        var normalized = (value ?? string.Empty).Trim()
            .Replace("\u00A0", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        return int.TryParse(normalized, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
               && parsed is >= 0 and <= 999
            ? parsed.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : string.Empty;
    }

    public static string NormalizeDecimal(string? value)
    {
        var normalized = (value ?? string.Empty).Trim()
            .Replace("\u00A0", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace(',', '.');

        return decimal.TryParse(normalized, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
               && parsed >= 0
            ? parsed.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)
            : string.Empty;
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
