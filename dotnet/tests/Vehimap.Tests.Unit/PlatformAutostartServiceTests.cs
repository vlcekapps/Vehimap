// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Services;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class PlatformAutostartServiceTests
{
    [Theory]
    [InlineData("en-US", "Comment=Start Vehimap automatically after sign-in")]
    [InlineData("cs-CZ", "Comment=Spouštět Vehimap automaticky po přihlášení")]
    public void Linux_desktop_entry_uses_localized_autostart_description(string language, string expectedComment)
    {
        var content = PlatformAutostartService.BuildLinuxDesktopEntryContent(
            new PlatformAutostartService.LaunchCommand(
                "/opt/Vehimap Desktop/Vehimap",
                ["--data", "/home/test/Vehimap data"]),
            new ResourceAppLocalizer(CultureInfo.GetCultureInfo(language)));

        Assert.Contains("Name=Vehimap", content, StringComparison.Ordinal);
        Assert.Contains(expectedComment, content, StringComparison.Ordinal);
        Assert.DoesNotContain("preview", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Exec=\"/opt/Vehimap Desktop/Vehimap\" --data \"/home/test/Vehimap data\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Linux_desktop_entry_escapes_quotes_inside_exec_arguments()
    {
        var content = PlatformAutostartService.BuildLinuxDesktopEntryContent(
            new PlatformAutostartService.LaunchCommand(
                "/opt/Vehimap \"Desktop\"/Vehimap",
                ["--data", "/home/test/Vehimap \"portable\" data"]));

        Assert.Contains("Exec=\"/opt/Vehimap \\\"Desktop\\\"/Vehimap\" --data \"/home/test/Vehimap \\\"portable\\\" data\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Mac_launch_agent_escapes_program_arguments_and_uses_working_directory()
    {
        var content = PlatformAutostartService.BuildMacLaunchAgentContent(
            new PlatformAutostartService.LaunchCommand(
                "/Applications/Vehimap & Tools/Vehimap",
                ["--data", "/Users/test/Vehimap <portable>"]));

        Assert.Contains("<key>Label</key>", content, StringComparison.Ordinal);
        Assert.Contains("<string>cz.vlcekapps.vehimap.desktop</string>", content, StringComparison.Ordinal);
        Assert.Contains("<key>ProgramArguments</key>", content, StringComparison.Ordinal);
        Assert.Contains("<string>/Applications/Vehimap &amp; Tools/Vehimap</string>", content, StringComparison.Ordinal);
        Assert.Contains("<string>--data</string>", content, StringComparison.Ordinal);
        Assert.Contains("<string>/Users/test/Vehimap &lt;portable&gt;</string>", content, StringComparison.Ordinal);
        Assert.Contains("<key>RunAtLoad</key>", content, StringComparison.Ordinal);
        Assert.Contains("<true/>", content, StringComparison.Ordinal);
        Assert.Contains("<key>WorkingDirectory</key>", content, StringComparison.Ordinal);
        Assert.Contains("<string>/Applications/Vehimap &amp; Tools</string>", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("", "\"\"")]
    [InlineData(" ", "\"\"")]
    [InlineData("/opt/Vehimap/Vehimap", "/opt/Vehimap/Vehimap")]
    [InlineData("/opt/Vehimap Desktop/Vehimap", "\"/opt/Vehimap Desktop/Vehimap\"")]
    [InlineData("/opt/Vehimap \"Desktop\"/Vehimap", "\"/opt/Vehimap \\\"Desktop\\\"/Vehimap\"")]
    public void Quote_command_argument_matches_desktop_entry_expectations(string value, string expected)
    {
        Assert.Equal(expected, PlatformAutostartService.QuoteCommandArgument(value));
    }
}
