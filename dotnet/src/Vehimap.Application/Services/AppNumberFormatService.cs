// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

namespace Vehimap.Application.Services;

public sealed class AppNumberFormatService : IAppNumberFormatService
{
    private readonly IAppCultureService _cultureService;

    public AppNumberFormatService()
        : this(new AppCultureService())
    {
    }

    public AppNumberFormatService(IAppCultureService cultureService)
    {
        _cultureService = cultureService;
    }

    public NumberFormatInfo CreateNumberFormat(AppCulturePreferences preferences)
    {
        var normalized = _cultureService.Normalize(preferences);
        var format = (NumberFormatInfo)_cultureService.ResolveCulture(normalized.Language).NumberFormat.Clone();

        var groupSeparator = ResolveThousandsSeparator(format.NumberGroupSeparator, normalized.ThousandsSeparator);
        var decimalSeparator = ResolveDecimalSeparator(format.NumberDecimalSeparator, normalized.DecimalSeparator);

        format.NumberGroupSeparator = groupSeparator;
        format.CurrencyGroupSeparator = groupSeparator;
        format.PercentGroupSeparator = groupSeparator;
        format.NumberDecimalSeparator = decimalSeparator;
        format.CurrencyDecimalSeparator = decimalSeparator;
        format.PercentDecimalSeparator = decimalSeparator;
        return format;
    }

    public string FormatDecimal(decimal value, AppCulturePreferences preferences, int decimalPlaces = 2)
    {
        var places = Math.Clamp(decimalPlaces, 0, 9);
        return value.ToString("N" + places.ToString(CultureInfo.InvariantCulture), CreateNumberFormat(preferences));
    }

    public string FormatMoney(decimal value, AppCulturePreferences preferences, string currency, int decimalPlaces = 2)
    {
        var places = Math.Clamp(decimalPlaces, 0, 9);
        var format = CreateNumberFormat(preferences);
        format.CurrencySymbol = AppCurrencyFormatService.GetCurrencySymbol(currency);
        return value.ToString("C" + places.ToString(CultureInfo.InvariantCulture), format);
    }

    public bool TryParseDecimal(string text, AppCulturePreferences preferences, out decimal value)
    {
        var format = CreateNumberFormat(preferences);
        return decimal.TryParse(
            text.Trim(),
            NumberStyles.Number,
            format,
            out value);
    }

    private static string ResolveThousandsSeparator(string cultureSeparator, string option) =>
        option switch
        {
            AppCultureService.SpaceSeparator => " ",
            AppCultureService.CommaSeparator => ",",
            AppCultureService.DotSeparator => ".",
            AppCultureService.NoSeparator => string.Empty,
            _ => cultureSeparator
        };

    private static string ResolveDecimalSeparator(string cultureSeparator, string option) =>
        option switch
        {
            AppCultureService.CommaSeparator => ",",
            AppCultureService.DotSeparator => ".",
            _ => cultureSeparator
        };
}
