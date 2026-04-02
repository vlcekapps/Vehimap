using Xunit;

namespace Vehimap.Tests.UI;

public sealed class DesktopAccessibilitySmokeTests
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
            Assert.NotNull(session.WaitForElementByAccessibilityId("DashboardTabButton"));
        }
    }

    [Fact]
    public void App_shell_dialogs_open_and_close_from_main_window_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("SettingsButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("TechnicalReminderDaysBox"));
            session.ClickByAccessibilityId("CancelSettingsButton");

            session.ClickByAccessibilityId("AboutButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("ReleaseNotesButton"));
            session.ClickByAccessibilityId("CloseAboutButton");

            session.ClickByAccessibilityId("UpdateCheckButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("UpdateCloseButton"));
            session.ClickByAccessibilityId("UpdateCloseButton");
        }
    }

    [Fact]
    public void Workspace_windows_open_from_selected_tabs_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("HistoryTabButton");
            session.ClickByAccessibilityId("OpenHistoryWindowButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("CloseHistoryWindowButton"));
            session.ClickByAccessibilityId("CloseHistoryWindowButton");

            session.ClickByAccessibilityId("DashboardTabButton");
            session.ClickByAccessibilityId("OpenDashboardWindowButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("CloseDashboardWindowButton"));
            session.ClickByAccessibilityId("CloseDashboardWindowButton");
        }
    }
}
