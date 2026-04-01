using Xunit;

namespace Vehimap.Tests.UI;

public sealed class DesktopAccessibilitySmokeTests
{
    [Fact(Skip = "Vyžaduje Appium server, zbuilděný Avalonia shell a běh na cílové platformě.")]
    public void Main_shell_should_expose_accessible_window_and_vehicle_list()
    {
    }
}
