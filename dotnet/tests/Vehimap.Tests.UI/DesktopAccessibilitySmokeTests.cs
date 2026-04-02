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

    [Fact]
    public void Reminder_editor_exposes_accessible_names_and_can_save_new_item_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("ReminderTabButton");
            session.ClickByAccessibilityId("CreateReminderButton");

            Assert.Equal("Název připomínky", session.GetNameByAccessibilityId("ReminderEditorTitleBox"));
            Assert.Equal("Termín připomínky", session.GetNameByAccessibilityId("ReminderEditorDueDateBox"));

            session.SendKeysByAccessibilityId("ReminderEditorTitleBox", "Appium připomínka");
            session.SendKeysByAccessibilityId("ReminderEditorDueDateBox", "12/2026");
            session.ClickByAccessibilityId("SaveReminderButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveReminderButton");
        }
    }

    [Fact]
    public void Record_editor_exposes_accessible_names_and_can_save_new_item_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("RecordTabButton");
            session.ClickByAccessibilityId("CreateRecordButton");

            Assert.Equal("Název dokladu", session.GetNameByAccessibilityId("RecordEditorTitleBox"));
            Assert.Equal("Režim přílohy dokladu", session.GetNameByAccessibilityId("RecordAttachmentModeComboBox"));

            session.SendKeysByAccessibilityId("RecordEditorTitleBox", "Appium doklad");
            session.ClickByAccessibilityId("SaveRecordButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveRecordButton");
        }
    }
}
