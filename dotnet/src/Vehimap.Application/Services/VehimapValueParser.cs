using System.Globalization;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

internal static class VehimapValueParser
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

    private static readonly string[] MonthYearFormats =
    [
        "MM/yyyy",
        "M/yyyy",
        "MM.yyyy",
        "M.yyyy",
        "MM-yyyy",
        "M-yyyy",
        "yyyy-MM",
        "yyyy/MM"
    ];

    public static bool TryParseEventDate(string? text, out DateOnly value)
    {
        return DateOnly.TryParseExact(
            (text ?? string.Empty).Trim(),
            EventDateFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out value);
    }

    public static bool TryParseMonthYear(string? text, out DateOnly value)
    {
        var input = (text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            value = default;
            return false;
        }

        if (DateOnly.TryParseExact(input, MonthYearFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
        {
            value = new DateOnly(value.Year, value.Month, 1);
            return true;
        }

        value = default;
        return false;
    }

    public static bool TryResolveRecordDate(VehicleRecord record, out DateOnly value)
    {
        if (TryParseMonthYear(record.ValidTo, out value))
        {
            return true;
        }

        return TryParseMonthYear(record.ValidFrom, out value);
    }

    public static bool TryParseMoney(string? text, out decimal value)
    {
        value = 0m;
        var clean = (text ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(clean))
        {
            return false;
        }

        clean = clean
            .Replace("kč", string.Empty, StringComparison.Ordinal)
            .Replace("czk", string.Empty, StringComparison.Ordinal)
            .Replace("\u00A0", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace(",-", string.Empty, StringComparison.Ordinal)
            .Replace(".-", string.Empty, StringComparison.Ordinal);

        clean = string.Concat(clean.Where(ch => char.IsDigit(ch) || ch is ',' or '.' or '-'));
        if (string.IsNullOrWhiteSpace(clean) || clean == "-")
        {
            return false;
        }

        if (clean.Contains(',') && clean.Contains('.'))
        {
            var lastComma = clean.LastIndexOf(',');
            var lastDot = clean.LastIndexOf('.');
            if (lastComma > lastDot)
            {
                clean = clean.Replace(".", string.Empty, StringComparison.Ordinal)
                    .Replace(',', '.');
            }
            else
            {
                clean = clean.Replace(",", string.Empty, StringComparison.Ordinal);
            }
        }
        else if (clean.Contains(','))
        {
            clean = clean.Replace(',', '.');
        }

        if (!decimal.TryParse(clean, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
        {
            var lastDot = clean.LastIndexOf('.');
            if (lastDot <= 0 || lastDot >= clean.Length - 1)
            {
                return false;
            }

            var integerPart = clean[..lastDot].Replace(".", string.Empty, StringComparison.Ordinal);
            var decimalPart = clean[(lastDot + 1)..];
            clean = $"{integerPart}.{decimalPart}";

            if (!decimal.TryParse(clean, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
            {
                return false;
            }
        }

        return true;
    }

    public static bool TryParseOdometer(string? text, out int value)
    {
        value = 0;
        var clean = (text ?? string.Empty).Trim()
            .Replace("\u00A0", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        return !string.IsNullOrWhiteSpace(clean)
            && int.TryParse(clean, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
            && value >= 0;
    }
}
