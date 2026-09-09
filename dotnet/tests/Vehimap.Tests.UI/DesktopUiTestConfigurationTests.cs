// SPDX-License-Identifier: GPL-3.0-or-later
using Xunit;

namespace Vehimap.Tests.UI;

public sealed class DesktopUiTestConfigurationTests
{
    [Theory]
    [InlineData(null, 45)]
    [InlineData("", 45)]
    [InlineData("5", 5)]
    [InlineData("0", 0)]
    [InlineData("50", 50)]
    public void Launch_delay_preserves_ci_default_and_accepts_supported_driver_values(string? value, int expected)
    {
        Assert.Equal(expected, DesktopUiTestConfiguration.ResolveAppLaunchWaitSeconds(value));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("51")]
    [InlineData("1.5")]
    [InlineData("invalid")]
    public void Invalid_launch_delay_does_not_silently_change_test_timing(string value)
    {
        Assert.Throws<ArgumentException>(() => DesktopUiTestConfiguration.ResolveAppLaunchWaitSeconds(value));
    }

    [Theory]
    [InlineData(null, "Windows")]
    [InlineData("", "Windows")]
    [InlineData("Windows", "Windows")]
    [InlineData(" windows ", "Windows")]
    [InlineData("NovaWindows", "NovaWindows")]
    [InlineData(" novawindows ", "NovaWindows")]
    public void Driver_selection_is_explicit_and_preserves_existing_ci_default(string? value, string expected)
    {
        Assert.Equal(expected, DesktopUiTestConfiguration.ResolveAutomationName(value));
    }

    [Fact]
    public void Unknown_driver_does_not_silently_fall_back_to_winappdriver()
    {
        Assert.Throws<ArgumentException>(() => DesktopUiTestConfiguration.ResolveAutomationName("NovaWindow"));
    }

    [Theory]
    [InlineData("Windows", false, true)]
    [InlineData("Windows", true, false)]
    [InlineData("NovaWindows", false, false)]
    [InlineData("NovaWindows", true, false)]
    public void Isolated_comparisons_never_attach_to_an_existing_window(
        string automationName, bool isolatedLaunchOnly, bool allowFallback)
    {
        var configuration = new DesktopUiTestConfiguration(
            new Uri("http://127.0.0.1:4725/"), "unused-app-path", TimeSpan.FromSeconds(90),
            automationName, isolatedLaunchOnly);

        Assert.Equal(allowFallback, configuration.AllowRootWindowFallback);
    }

    [Theory]
    [InlineData("Windows", false, false)]
    [InlineData("Windows", true, true)]
    [InlineData("NovaWindows", false, false)]
    [InlineData("NovaWindows", true, false)]
    public void Force_quit_is_limited_to_explicit_isolated_windows_launches(
        string automationName, bool isolatedLaunchOnly, bool expected)
    {
        var configuration = new DesktopUiTestConfiguration(
            new Uri("http://127.0.0.1:4725/"), "unused-app-path", TimeSpan.FromSeconds(90),
            automationName, isolatedLaunchOnly);

        Assert.Equal(expected, configuration.ForceQuitIsolatedApplication);
        Assert.False(configuration.ForceQuitIsolatedApplication && configuration.AllowRootWindowFallback);
    }
}
