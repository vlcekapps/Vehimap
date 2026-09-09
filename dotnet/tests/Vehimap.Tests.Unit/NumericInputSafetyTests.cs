// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class NumericInputSafetyTests
{
    [Theory]
    [InlineData("1,23")]
    [InlineData("1,,234")]
    [InlineData("1,234,56")]
    [InlineData("1e3")]
    [InlineData("12 liters")]
    [InlineData("1 2")]
    [InlineData("123-")]
    public void Localized_input_rejects_malformed_groups_and_non_numeric_text(string input)
    {
        Assert.False(new AppNumberFormatService().TryParseDecimal(input, new("en-US", "comma", "dot"), out _));
    }

    [Theory]
    [InlineData("1,234.56", "en-US", "comma", "dot", 1234.56)]
    [InlineData("1234,56", "cs-CZ", "none", "comma", 1234.56)]
    [InlineData("1 234,56", "cs-CZ", "space", "comma", 1234.56)]
    [InlineData("1\u00a0234,56", "cs-CZ", "space", "comma", 1234.56)]
    [InlineData(".5", "en-US", "none", "dot", 0.5)]
    public void Localized_input_preserves_valid_values(string input, string language, string groups, string decimals, decimal expected)
    {
        Assert.True(new AppNumberFormatService().TryParseDecimal(input, new(language, groups, decimals), out var result));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("km", "l")]
    [InlineData("mi", "us_gal")]
    [InlineData("mi", "imp_gal")]
    public void Excessive_distance_and_volume_return_validation_failure_without_overflow(string distance, string volume)
    {
        var preferences = new AppUnitPreferences(distance, volume);
        Assert.False(AppUnitFormatService.TryConvertWholeKilometers(decimal.MaxValue, preferences, out _));
        Assert.False(AppUnitFormatService.TryConvertWholeKilometers(-1, preferences, out _));
        Assert.Equal(volume == "l", AppUnitFormatService.TryConvertLiters(decimal.MaxValue, preferences, out _));
    }

    [Theory]
    [InlineData("us_gal")]
    [InlineData("imp_gal")]
    public void Editor_precision_roundtrips_hundredths_of_liters_and_whole_kilometers(string volume)
    {
        var preferences = new AppUnitPreferences("mi", volume);
        var service = new AppUnitFormatService();
        for (var i = 1; i < 10_000; i++)
        {
            var liters = i / 100m;
            var displayVolume = decimal.Round(service.ConvertVolumeFromLiters(liters, preferences), 5);
            Assert.True(AppUnitFormatService.TryConvertLiters(displayVolume, preferences, out var restoredLiters));
            Assert.Equal(liters, decimal.Round(restoredLiters, 2));
            var displayDistance = decimal.Round(service.ConvertDistanceFromKilometers(i, preferences), 1);
            Assert.True(AppUnitFormatService.TryConvertWholeKilometers(displayDistance, preferences, out var restoredDistance));
            Assert.Equal(i, restoredDistance);
        }
    }
}
