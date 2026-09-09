// SPDX-License-Identifier: GPL-3.0-or-later
using Xunit;

namespace Vehimap.Tests.UI;

public sealed class DesktopUiTestConfigurationTests
{
    [Theory]
    [InlineData(0, true, 0)]
    [InlineData(1, true, 1)]
    [InlineData(100, false, 50)]
    public void Teardown_removes_fixture_only_after_confirmed_process_exit(int runningProbes, bool expected, int expectedWaits)
    {
        var waits = 0;
        Assert.Equal(expected, DesktopAppiumTestSession.WaitForApplicationExit(() => runningProbes-- > 0, () => waits++));
        Assert.Equal(expectedWaits, waits);
    }

    [Fact]
    public void Teardown_preserves_fixture_when_process_status_cannot_be_read()
    {
        Assert.False(DesktopAppiumTestSession.WaitForApplicationExit(
            () => throw new System.ComponentModel.Win32Exception("unavailable"), () => { }));
    }

    [Theory]
    [InlineData("<Window IsModal='False' />", false)]
    [InlineData("<Window><Window IsModal='True' IsOffscreen='False' /></Window>", true)]
    [InlineData("<Window><Window IsModal='true' /></Window>", true)]
    [InlineData("<Window><Window IsModal='True' IsOffscreen='True' /></Window>", false)]
    [InlineData("<Window><Menu IsModal='True' /></Window>", false)]
    public void Teardown_dismisses_visible_modal_windows_before_using_the_owner_menu(string source, bool expected)
    {
        Assert.Equal(expected, DesktopAppiumTestSession.HasOpenModalWindow(source));
    }

    [Theory]
    [InlineData(false, null, true)]
    [InlineData(false, "", true)]
    [InlineData(false, "http://127.0.0.1:4725", false)]
    [InlineData(true, null, false)]
    public void Live_tests_are_skipped_only_when_not_requested(bool required, string? url, bool expectedSkip)
    {
        Assert.Equal(expectedSkip, AppiumFactAttribute.GetSkipReason(required, url) is not null);
    }

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
