// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

namespace Vehimap.Application.Services;

public sealed class AppDateFormatService : IAppDateFormatService
{
    private readonly IAppCultureService _cultureService;

    public AppDateFormatService()
        : this(new AppCultureService())
    {
    }

    public AppDateFormatService(IAppCultureService cultureService)
    {
        _cultureService = cultureService;
    }

    public string FormatDate(DateOnly value, AppCulturePreferences preferences) =>
        value.ToString("d", ResolveCulture(preferences));

    public string FormatDateTime(DateTime value, AppCulturePreferences preferences) =>
        value.ToString("g", ResolveCulture(preferences));

    public bool TryParseDate(string? text, AppCulturePreferences preferences, out DateOnly value)
    {
        var input = (text ?? string.Empty).Trim();
        if (input.Length == 0)
        {
            value = default;
            return false;
        }

        if (LooksLikeLegacyDayFirstDate(input)
            && VehimapValueParser.TryParseEventDate(input, out value))
        {
            return true;
        }

        return DateOnly.TryParse(input, ResolveCulture(preferences), DateTimeStyles.AllowWhiteSpaces, out value)
            || VehimapValueParser.TryParseEventDate(input, out value);
    }

    private CultureInfo ResolveCulture(AppCulturePreferences preferences) =>
        _cultureService.ResolveCulture(_cultureService.Normalize(preferences).Language);

    private static bool LooksLikeLegacyDayFirstDate(string value)
    {
        var separator = value.Contains('.')
            ? '.'
            : value.Contains('-') ? '-' : '\0';
        if (separator == '\0')
        {
            return false;
        }

        var parts = value.Split(separator);
        return parts.Length == 3
            && parts[0].Length is 1 or 2
            && parts[1].Length is 1 or 2
            && parts[2].Length == 4;
    }
}
