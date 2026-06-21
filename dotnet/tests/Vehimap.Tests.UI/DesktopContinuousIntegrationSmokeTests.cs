using OpenQA.Selenium;
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

    [Fact]
    public void Main_menu_data_and_quick_actions_expose_expected_action_states_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("FileMenuRoot");
            Assert.NotNull(session.WaitForElementByAccessibilityId("CreateAutomaticBackupNowMenuItem"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenAutomaticBackupFolderMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("CreateAutomaticBackupNowMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("OpenAutomaticBackupFolderMenuItem"));
            session.SendKeysToActiveElement(Keys.Escape);

            session.ClickByAccessibilityId("QuickActionsMenuRoot");
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenNearestReminderMenuItem"));
            Assert.False(session.IsEnabledByAccessibilityId("OpenNearestTechnicalMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("OpenNearestReminderMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("ReviewRecordsMenuItem"));
        }
    }
}
