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
}
