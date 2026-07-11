// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Services;

namespace Vehimap.Desktop.Localization;

internal static class LocalizedCompatibilityAliases
{
    public static bool MatchesStableValueOrResource(string? value, string stableValue, string resourceKey)
        => LocalizedResourceValueMatcher.MatchesStableValueOrResource(
            DesktopLocalization.Localizer,
            value,
            stableValue,
            resourceKey);

    public static bool MatchesAnyResource(string? value, params string[] resourceKeys)
    {
        return LocalizedResourceValueMatcher.Matches(
            DesktopLocalization.Localizer,
            value,
            resourceKeys);
    }

    public static IEnumerable<string> EnumerateResourceValues(string resourceKey)
        => LocalizedResourceValueMatcher.EnumerateValues(
            DesktopLocalization.Localizer,
            [resourceKey]);
}
