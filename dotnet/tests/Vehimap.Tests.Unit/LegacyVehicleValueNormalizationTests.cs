using Vehimap.Storage.Legacy;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LegacyVehicleValueNormalizationTests
{
    [Theory]
    [InlineData("9/2026", "09/2026")]
    [InlineData(" 9-2026 ", "09/2026")]
    [InlineData("09.2026", "09/2026")]
    [InlineData("13/2026", "")]
    [InlineData("09/1899", "")]
    [InlineData("09/2201", "")]
    [InlineData("2026/09", "")]
    public void Normalize_month_year_matches_legacy_ahk_rules(string value, string expected)
    {
        Assert.Equal(expected, LegacyVehicleValueNormalization.NormalizeMonthYear(value));
    }

    [Theory]
    [InlineData("15.6.2026", "15.06.2026")]
    [InlineData(" 15/06/2026 ", "15.06.2026")]
    [InlineData("2026-06-15", "15.06.2026")]
    [InlineData("31.02.2026", "")]
    [InlineData("15.06.1899", "")]
    [InlineData("15.06.2201", "")]
    public void Normalize_event_date_matches_legacy_editor_rules(string value, string expected)
    {
        Assert.Equal(expected, LegacyVehicleValueNormalization.NormalizeEventDate(value));
    }

    [Theory]
    [InlineData("123 456", "123456")]
    [InlineData("0", "0")]
    [InlineData("-1", "")]
    [InlineData("12.5", "")]
    public void Normalize_odometer_accepts_only_non_negative_whole_numbers(string value, string expected)
    {
        Assert.Equal(expected, LegacyVehicleValueNormalization.NormalizeOdometer(value));
    }

    [Theory]
    [InlineData("42,50", "42.5")]
    [InlineData(" 0012.00 ", "12")]
    [InlineData("-1", "")]
    [InlineData("abc", "")]
    public void Normalize_decimal_accepts_non_negative_decimal_numbers(string value, string expected)
    {
        Assert.Equal(expected, LegacyVehicleValueNormalization.NormalizeDecimal(value));
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("999", "999")]
    [InlineData("1000", "")]
    [InlineData("-1", "")]
    [InlineData("", "")]
    public void Normalize_reminder_days_matches_legacy_bounds(string value, string expected)
    {
        Assert.Equal(expected, LegacyVehicleValueNormalization.NormalizeReminderDays(value));
    }

    [Theory]
    [InlineData("Osobní", "Osobní vozidla")]
    [InlineData("Nákladní", "Nákladní vozidla")]
    [InlineData("Motocykly", "Motocykly")]
    [InlineData("Neznámá kategorie", "Ostatní")]
    [InlineData("", "Ostatní")]
    public void Normalize_category_matches_legacy_ahk_rules(string value, string expected)
    {
        Assert.Equal(expected, LegacyVehicleValueNormalization.NormalizeCategory(value));
    }

    [Theory]
    [InlineData("Povinné ručení", "Povinné ručení")]
    [InlineData("Doklad", "Doklad")]
    [InlineData("Vlastní typ", "Povinné ručení")]
    [InlineData("", "Povinné ručení")]
    public void Normalize_record_type_matches_legacy_dropdown_rules(string value, string expected)
    {
        Assert.Equal(expected, LegacyVehicleValueNormalization.NormalizeRecordType(value));
    }

    [Theory]
    [InlineData("", "Neopakovat")]
    [InlineData("Neopakovat", "Neopakovat")]
    [InlineData("Ročně", "Každý rok")]
    [InlineData("ročně", "Každý rok")]
    [InlineData("Každý rok", "Každý rok")]
    [InlineData("Každé 2 roky", "Každé 2 roky")]
    [InlineData("Každých 5 let", "Každých 5 let")]
    [InlineData("něco jiného", "Neopakovat")]
    public void Normalize_reminder_repeat_mode_matches_legacy_dropdown_rules(string value, string expected)
    {
        Assert.Equal(expected, LegacyVehicleValueNormalization.NormalizeReminderRepeatMode(value));
    }
}
