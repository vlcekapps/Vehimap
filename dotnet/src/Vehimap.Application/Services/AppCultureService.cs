using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

namespace Vehimap.Application.Services;

public sealed class AppCultureService : IAppCultureService
{
    public const string SystemLanguage = "system";
    public const string CzechLanguage = "cs-CZ";
    public const string EnglishLanguage = "en-US";
    public const string CultureSeparator = "culture";
    public const string SpaceSeparator = "space";
    public const string CommaSeparator = "comma";
    public const string DotSeparator = "dot";
    public const string NoSeparator = "none";

    private static readonly string[] SupportedLanguages = [SystemLanguage, CzechLanguage, EnglishLanguage];
    private static readonly string[] SupportedThousandsSeparators = [CultureSeparator, SpaceSeparator, CommaSeparator, DotSeparator, NoSeparator];
    private static readonly string[] SupportedDecimalSeparators = [CultureSeparator, CommaSeparator, DotSeparator];

    public CultureInfo ResolveCulture(string language)
    {
        var normalized = NormalizeLanguage(language);
        if (string.Equals(normalized, SystemLanguage, StringComparison.Ordinal))
        {
            var systemCulture = CultureInfo.CurrentUICulture;
            return string.Equals(systemCulture.TwoLetterISOLanguageName, "cs", StringComparison.OrdinalIgnoreCase)
                ? CultureInfo.GetCultureInfo(CzechLanguage)
                : CultureInfo.GetCultureInfo(EnglishLanguage);
        }

        return CultureInfo.GetCultureInfo(normalized);
    }

    public AppCulturePreferences Normalize(AppCulturePreferences preferences) =>
        new(
            NormalizeLanguage(preferences.Language),
            NormalizeThousandsSeparator(preferences.ThousandsSeparator),
            NormalizeDecimalSeparator(preferences.DecimalSeparator));

    public void ApplyThreadCulture(AppCulturePreferences preferences)
    {
        var culture = ResolveCulture(preferences.Language);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    public static string NormalizeLanguage(string? language) =>
        NormalizeOption(language, SupportedLanguages, SystemLanguage);

    public static string NormalizeThousandsSeparator(string? separator) =>
        NormalizeOption(separator, SupportedThousandsSeparators, CultureSeparator);

    public static string NormalizeDecimalSeparator(string? separator) =>
        NormalizeOption(separator, SupportedDecimalSeparators, CultureSeparator);

    private static string NormalizeOption(string? value, IReadOnlyCollection<string> supportedValues, string defaultValue)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? defaultValue
            : value.Trim();
        return supportedValues.Contains(normalized, StringComparer.OrdinalIgnoreCase)
            ? supportedValues.First(candidate => string.Equals(candidate, normalized, StringComparison.OrdinalIgnoreCase))
            : defaultValue;
    }
}
