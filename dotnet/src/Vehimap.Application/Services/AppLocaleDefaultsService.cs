// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Application.Services;

public sealed class AppLocaleDefaultsService
{
    public static AppLocaleDefaults GetCurrentCultureDefaults() =>
        new AppLocaleDefaultsService().GetDefaultsForLanguage(
            string.Equals(System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "cs", StringComparison.OrdinalIgnoreCase)
                ? AppCultureService.CzechLanguage
                : AppCultureService.EnglishLanguage);

    public AppLocaleDefaults GetDefaultsForLanguage(string? language)
    {
        var normalized = AppCultureService.NormalizeLanguage(language);
        if (string.Equals(normalized, AppCultureService.SystemLanguage, StringComparison.Ordinal))
        {
            normalized = new AppCultureService().ResolveCulture(AppCultureService.SystemLanguage).Name;
        }

        return string.Equals(normalized, AppCultureService.CzechLanguage, StringComparison.Ordinal)
            ? new AppLocaleDefaults(
                AppCultureService.CzechLanguage,
                AppCultureService.NoSeparator,
                AppCultureService.CommaSeparator,
                AppUnitFormatService.Kilometers,
                AppUnitFormatService.Liters,
                AppCurrencyFormatService.CzechCrowns)
            : new AppLocaleDefaults(
                AppCultureService.EnglishLanguage,
                AppCultureService.CommaSeparator,
                AppCultureService.DotSeparator,
                AppUnitFormatService.Miles,
                AppUnitFormatService.UsGallons,
                AppCurrencyFormatService.UsDollars);
    }
}
