// SPDX-License-Identifier: GPL-3.0-or-later
using System.Text.RegularExpressions;

namespace Vehimap.Storage.Legacy;

public static partial class LegacyVehicleValueNormalization
{
    private const string ShortCategoryPassengerVehicles = "Osobn\u00ED";
    private const string ShortCategoryTrucks = "N\u00E1kladn\u00ED";

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
        if (string.Equals(value, ShortCategoryPassengerVehicles, StringComparison.Ordinal))
        {
            return LegacyKnownValues.CategoryPassengerVehicles;
        }

        if (string.Equals(value, ShortCategoryTrucks, StringComparison.Ordinal))
        {
            return LegacyKnownValues.CategoryTrucks;
        }

        foreach (var allowed in LegacyKnownValues.Categories)
        {
            if (string.Equals(allowed, value, StringComparison.Ordinal))
            {
                return allowed;
            }
        }

        return LegacyKnownValues.CategoryOther;
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

    public static string NormalizeFuelType(string? fuelType)
    {
        return NormalizeKnownOption(fuelType, LegacyKnownValues.FuelTypes, LegacyKnownValues.FuelTypes[0]);
    }

    public static string NormalizeVehicleState(string? state)
    {
        return NormalizeKnownOption(state, LegacyKnownValues.VehicleStates, LegacyKnownValues.VehicleStates[0]);
    }

    public static string NormalizeVehiclePowertrain(string? powertrain)
    {
        return NormalizeKnownOption(powertrain, LegacyKnownValues.VehiclePowertrains, LegacyKnownValues.VehiclePowertrains[0]);
    }

    public static string NormalizeVehicleClimateProfile(string? climateProfile)
    {
        return NormalizeKnownOption(climateProfile, LegacyKnownValues.VehicleClimateProfiles, LegacyKnownValues.VehicleClimateProfiles[0]);
    }

    public static string NormalizeVehicleTimingDrive(string? timingDrive)
    {
        return NormalizeKnownOption(timingDrive, LegacyKnownValues.VehicleTimingDrives, LegacyKnownValues.VehicleTimingDrives[0]);
    }

    public static string NormalizeVehicleTransmission(string? transmission)
    {
        return NormalizeKnownOption(transmission, LegacyKnownValues.VehicleTransmissions, LegacyKnownValues.VehicleTransmissions[0]);
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

        if (folded.Contains("5"))
        {
            return LegacyKnownValues.ReminderRepeatEveryFiveYears;
        }

        if (folded.Contains('2'))
        {
            return LegacyKnownValues.ReminderRepeatEveryTwoYears;
        }

        if (folded.Contains("rok", StringComparison.Ordinal) || folded.Contains("rocne", StringComparison.Ordinal))
        {
            return LegacyKnownValues.ReminderRepeatYearly;
        }

        return LegacyKnownValues.ReminderRepeatModes[0];
    }

    private static string NormalizeKnownOption(string? value, IReadOnlyList<string> allowedValues, string fallback)
    {
        var trimmed = (value ?? string.Empty).Trim();
        foreach (var allowed in allowedValues)
        {
            if (string.Equals(allowed, trimmed, StringComparison.Ordinal))
            {
                return allowed;
            }
        }

        return fallback;
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
