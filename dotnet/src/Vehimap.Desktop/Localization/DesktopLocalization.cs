// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.Localization;

internal static class DesktopLocalization
{
    private static readonly IAppCultureService CultureService = new AppCultureService();
    private static CultureInfo _currentCulture = CultureInfo.CurrentUICulture;
    private static IAppLocalizer _localizer = new ResourceAppLocalizer(_currentCulture);
    private static readonly IAppLocalizer ForwardingLocalizer = new DelegatingAppLocalizer(() => Localizer);

    public static IAppLocalizer Localizer => _localizer;

    public static IAppLocalizer LiveLocalizer => ForwardingLocalizer;

    public static CultureInfo CurrentCulture => _currentCulture;

    public static void Configure(AppCulturePreferences preferences)
    {
        var normalized = CultureService.Normalize(preferences);
        CultureService.ApplyThreadCulture(normalized);
        _currentCulture = CultureService.ResolveCulture(normalized.Language);
        _localizer = new ResourceAppLocalizer(_currentCulture);
    }

    public static string GetString(string key) => _localizer.GetString(key);
}
