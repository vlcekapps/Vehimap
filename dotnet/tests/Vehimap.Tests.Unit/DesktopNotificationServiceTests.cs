using Vehimap.Desktop.Services;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopNotificationServiceTests
{
    [Fact]
    public void Windows_notification_process_info_uses_powershell_and_encoded_command()
    {
        var startInfo = DesktopNotificationService.BuildWindowsNotificationProcessStartInfo(
            "Vehimap: propadlá technická kontrola",
            "Božena: technická kontrola je po termínu.",
            @"C:\vehimap\Vehimap.Desktop.exe");

        Assert.Contains("powershell.exe", startInfo.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("-EncodedCommand", startInfo.Arguments, StringComparison.Ordinal);
        Assert.True(startInfo.CreateNoWindow);
        Assert.False(startInfo.UseShellExecute);
    }

    [Fact]
    public void Normalize_balloon_text_truncates_and_flattens_multiline_content()
    {
        var value = DesktopNotificationService.NormalizeBalloonText("První řádek\r\nDruhý řádek s delším obsahem", 18);

        Assert.DoesNotContain('\r', value);
        Assert.DoesNotContain('\n', value);
        Assert.True(value.Length <= 18);
    }

    [Fact]
    public void Windows_notification_script_contains_expected_balloon_api_calls()
    {
        var script = DesktopNotificationService.BuildWindowsNotificationScript(
            "Vehimap",
            "Propadlá technická kontrola",
            @"C:\vehimap\Vehimap.Desktop.exe");

        Assert.Contains("System.Windows.Forms.NotifyIcon", script, StringComparison.Ordinal);
        Assert.Contains("ShowBalloonTip", script, StringComparison.Ordinal);
        Assert.Contains("ExtractAssociatedIcon", script, StringComparison.Ordinal);
    }
}
