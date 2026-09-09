// SPDX-License-Identifier: GPL-3.0-or-later
using Xunit;

namespace Vehimap.Tests.UI;

public sealed class AppiumFactAttribute : FactAttribute
{
    public AppiumFactAttribute()
    {
        Skip = GetSkipReason(DesktopUiTestConfiguration.RequireAvailability,
            Environment.GetEnvironmentVariable("VEHIMAP_APPIUM_SERVER_URL"));
    }

    internal static string? GetSkipReason(bool required, string? serverUrl) =>
        !required && string.IsNullOrWhiteSpace(serverUrl)
            ? "Live Appium tests are opt-in. Run Test-DotnetWindowsUi.ps1 to execute them."
            : null;
}

public sealed class AppiumTheoryAttribute : TheoryAttribute
{
    public AppiumTheoryAttribute()
    {
        Skip = AppiumFactAttribute.GetSkipReason(DesktopUiTestConfiguration.RequireAvailability,
            Environment.GetEnvironmentVariable("VEHIMAP_APPIUM_SERVER_URL"));
    }
}
