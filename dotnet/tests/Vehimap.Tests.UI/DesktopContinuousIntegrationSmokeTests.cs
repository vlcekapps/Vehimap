using Xunit;

namespace Vehimap.Tests.UI;

[Trait("UiProfile", "Smoke")]
public sealed class DesktopContinuousIntegrationSmokeTests
{
    [Fact]
    public void Main_shell_exposes_vehicle_list_and_app_shell_actions_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            Assert.NotNull(session.WaitForElementByAccessibilityId("VehicleListBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("SettingsButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("AboutButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("UpdateCheckButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ReloadButton"));
        }
    }

    [Fact]
    public void Primary_workspace_headers_are_exposed_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            Assert.NotNull(session.WaitForElementByAccessibilityId("DetailTabButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("HistoryTabButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("RecordTabButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("SearchTabButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("UpcomingOverviewTabButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("DashboardTabButton"));
        }
    }
}
