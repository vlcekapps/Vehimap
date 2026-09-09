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
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var format = CreateNumberFormat(preferences);
        var input = text.Trim();
        if (format.NumberGroupSeparator == format.NumberDecimalSeparator) return false;
        if (!string.IsNullOrEmpty(format.NumberGroupSeparator) && string.IsNullOrWhiteSpace(format.NumberGroupSeparator))
        {
            input = input.Replace('\u00a0', ' ').Replace('\u202f', ' ');
            format.NumberGroupSeparator = " ";
        }
        var unsigned = input.StartsWith(format.NegativeSign, StringComparison.Ordinal) ? input[format.NegativeSign.Length..]
            : input.StartsWith(format.PositiveSign, StringComparison.Ordinal) ? input[format.PositiveSign.Length..] : input;
        var parts = unsigned.Split(format.NumberDecimalSeparator, StringSplitOptions.None);
        if (parts.Length > 2 || (parts.Length == 2 && !parts[1].All(char.IsAsciiDigit))) return false;
        var integer = parts[0];
        if (!string.IsNullOrEmpty(format.NumberGroupSeparator) && integer.Contains(format.NumberGroupSeparator, StringComparison.Ordinal))
        {
            var groups = integer.Split(format.NumberGroupSeparator, StringSplitOptions.None);
            if (groups[0].Length is < 1 or > 3 || !groups[0].All(char.IsAsciiDigit)
                || groups.Skip(1).Any(group => group.Length != 3 || !group.All(char.IsAsciiDigit))) return false;
        }
        else if (!integer.All(char.IsAsciiDigit)) return false;
        return decimal.TryParse(
            input,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
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
