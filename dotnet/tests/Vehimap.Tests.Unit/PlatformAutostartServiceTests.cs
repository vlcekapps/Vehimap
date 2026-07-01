// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class PlatformAutostartServiceTests
{
    [Fact]
    public void Linux_desktop_entry_uses_product_name_without_preview_label()
    {
        var content = PlatformAutostartService.BuildLinuxDesktopEntryContent(
            new PlatformAutostartService.LaunchCommand(
                "/opt/Vehimap Desktop/Vehimap.Desktop",
                ["--data", "/home/test/Vehimap data"]));

        Assert.Contains("Name=Vehimap Desktop", content, StringComparison.Ordinal);
        Assert.Contains("Comment=Vehimap desktop", content, StringComparison.Ordinal);
        Assert.DoesNotContain("preview", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Exec=\"/opt/Vehimap Desktop/Vehimap.Desktop\" --data \"/home/test/Vehimap data\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Linux_desktop_entry_escapes_quotes_inside_exec_arguments()
    {
        var content = PlatformAutostartService.BuildLinuxDesktopEntryContent(
            new PlatformAutostartService.LaunchCommand(
                "/opt/Vehimap \"Desktop\"/Vehimap.Desktop",
                ["--data", "/home/test/Vehimap \"portable\" data"]));

        Assert.Contains("Exec=\"/opt/Vehimap \\\"Desktop\\\"/Vehimap.Desktop\" --data \"/home/test/Vehimap \\\"portable\\\" data\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Mac_launch_agent_escapes_program_arguments_and_uses_working_directory()
    {
        var content = PlatformAutostartService.BuildMacLaunchAgentContent(
            new PlatformAutostartService.LaunchCommand(
                "/Applications/Vehimap & Tools/Vehimap.Desktop",
                ["--data", "/Users/test/Vehimap <portable>"]));

        Assert.Contains("<key>Label</key>", content, StringComparison.Ordinal);
        Assert.Contains("<string>cz.vlcekapps.vehimap.desktop</string>", content, StringComparison.Ordinal);
        Assert.Contains("<key>ProgramArguments</key>", content, StringComparison.Ordinal);
        Assert.Contains("<string>/Applications/Vehimap &amp; Tools/Vehimap.Desktop</string>", content, StringComparison.Ordinal);
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
    [InlineData("/opt/Vehimap/Vehimap.Desktop", "/opt/Vehimap/Vehimap.Desktop")]
    [InlineData("/opt/Vehimap Desktop/Vehimap.Desktop", "\"/opt/Vehimap Desktop/Vehimap.Desktop\"")]
    [InlineData("/opt/Vehimap \"Desktop\"/Vehimap.Desktop", "\"/opt/Vehimap \\\"Desktop\\\"/Vehimap.Desktop\"")]
    public void Quote_command_argument_matches_desktop_entry_expectations(string value, string expected)
    {
        Assert.Equal(expected, PlatformAutostartService.QuoteCommandArgument(value));
    }
}
