// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.Localization;

internal static class LocalizedCompatibilityAliases
{
    public static bool MatchesStableValueOrResource(string? value, string stableValue, string resourceKey)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.Equals(normalized, stableValue, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return EnumerateResourceValues(resourceKey)
            .Any(candidate => string.Equals(normalized, candidate, StringComparison.OrdinalIgnoreCase));
    }

    public static bool MatchesAnyResource(string? value, params string[] resourceKeys)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        return resourceKeys
            .SelectMany(EnumerateResourceValues)
            .Any(candidate => string.Equals(normalized, candidate, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<string> EnumerateResourceValues(string resourceKey)
    {
        yield return DesktopLocalization.Localizer.GetString(resourceKey);
        yield return new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage)).GetString(resourceKey);
        yield return new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage)).GetString(resourceKey);
    }
}
