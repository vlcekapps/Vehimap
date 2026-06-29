using Vehimap.Application.Models;

namespace Vehimap.Application.Services;

public sealed class AppLocaleDefaultsService
{
    public AppLocaleDefaults GetDefaultsForLanguage(string? language)
    {
        var normalized = AppCultureService.NormalizeLanguage(language);
        if (string.Equals(normalized, AppCultureService.SystemLanguage, StringComparison.Ordinal))
        {
            normalized = ResolveSystemDefaultLanguage();
        }

        return string.Equals(normalized, AppCultureService.CzechLanguage, StringComparison.Ordinal)
            ? new AppLocaleDefaults(
                AppCultureService.CzechLanguage,
                AppCultureService.NoSeparator,
                AppCultureService.CommaSeparator,
                AppUnitFormatService.Kilometers,
                AppUnitFormatService.Liters)
            : new AppLocaleDefaults(
                AppCultureService.EnglishLanguage,
                AppCultureService.CommaSeparator,
                AppCultureService.DotSeparator,
                AppUnitFormatService.Miles,
                AppUnitFormatService.UsGallons);
    }

    private static string ResolveSystemDefaultLanguage() =>
        string.Equals(System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "cs", StringComparison.OrdinalIgnoreCase)
            ? AppCultureService.CzechLanguage
            : AppCultureService.EnglishLanguage;
}
