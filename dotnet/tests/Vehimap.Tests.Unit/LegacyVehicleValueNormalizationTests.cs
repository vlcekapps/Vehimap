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
    [InlineData("Osobní", "Osobní vozidla")]
    [InlineData("Nákladní", "Nákladní vozidla")]
    [InlineData("Motocykly", "Motocykly")]
    [InlineData("Neznámá kategorie", "Ostatní")]
    [InlineData("", "Ostatní")]
    public void Normalize_category_matches_legacy_ahk_rules(string value, string expected)
    {
        Assert.Equal(expected, LegacyVehicleValueNormalization.NormalizeCategory(value));
    }
}
