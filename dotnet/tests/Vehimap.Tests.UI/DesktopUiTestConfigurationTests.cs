// SPDX-License-Identifier: GPL-3.0-or-later
using Xunit;

namespace Vehimap.Tests.UI;

public sealed class DesktopUiTestConfigurationTests
{
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
}
