// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Abstractions;

namespace Vehimap.Application.Services;

public static class LocalizedResourceValueMatcher
{
    private static readonly IAppLocalizer EnglishLocalizer =
        new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));

    private static readonly IAppLocalizer CzechLocalizer =
        new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));

    public static bool Matches(
        IAppLocalizer currentLocalizer,
        string? value,
        params string[] resourceKeys)
    {
        ArgumentNullException.ThrowIfNull(currentLocalizer);

        var normalized = (value ?? string.Empty).Trim();
        return normalized.Length > 0
            && EnumerateValues(currentLocalizer, resourceKeys)
                .Any(candidate => string.Equals(normalized, candidate, StringComparison.OrdinalIgnoreCase));
    }

    public static bool MatchesStableValueOrResource(
        IAppLocalizer currentLocalizer,
        string? value,
        string stableValue,
        string resourceKey)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.Equals(normalized, stableValue, StringComparison.OrdinalIgnoreCase)
            || Matches(currentLocalizer, normalized, resourceKey);
    }

    public static IEnumerable<string> EnumerateValues(
        IAppLocalizer currentLocalizer,
        IEnumerable<string> resourceKeys)
    {
        ArgumentNullException.ThrowIfNull(currentLocalizer);
        ArgumentNullException.ThrowIfNull(resourceKeys);

        foreach (var resourceKey in resourceKeys)
        {
            yield return currentLocalizer.GetString(resourceKey);
            yield return EnglishLocalizer.GetString(resourceKey);
            yield return CzechLocalizer.GetString(resourceKey);
        }
    }
}
