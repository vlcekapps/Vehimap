// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Runtime.CompilerServices;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;

namespace Vehimap.Tests.Unit;

internal static class TestCultureInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        ResetToCzech();
    }

    internal static void ResetToCzech()
    {
        var culture = CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        DesktopLocalization.Configure(new AppCulturePreferences(
            AppCultureService.CzechLanguage,
            AppCultureService.NoSeparator,
            AppCultureService.CommaSeparator));
    }
}
