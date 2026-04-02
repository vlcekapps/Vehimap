using OpenQA.Selenium;
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

    [Fact]
    public void History_editor_exposes_accessible_names_and_can_save_new_item_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("HistoryTabButton");
            session.ClickByAccessibilityId("CreateHistoryButton");

            Assert.Equal("Datum historického záznamu", session.GetNameByAccessibilityId("HistoryEditorDateBox"));
            Assert.Equal("Typ historické události", session.GetNameByAccessibilityId("HistoryEditorTypeBox"));

            session.SendKeysByAccessibilityId("HistoryEditorDateBox", "15.10.2026");
            session.SendKeysByAccessibilityId("HistoryEditorTypeBox", "Servis");
            session.SendKeysByAccessibilityId("HistoryEditorOdometerBox", "123456");
            session.SendKeysByAccessibilityId("HistoryEditorCostBox", "2500");
            session.SendKeysByAccessibilityId("HistoryEditorNoteBox", "Appium historický záznam");
            session.ClickByAccessibilityId("SaveHistoryButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveHistoryButton");
        }
    }

    [Fact]
    public void Fuel_editor_exposes_accessible_names_and_can_save_new_item_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("FuelTabButton");
            session.ClickByAccessibilityId("CreateFuelButton");

            Assert.Equal("Datum tankování", session.GetNameByAccessibilityId("FuelEditorDateBox"));
            Assert.Equal("Typ paliva", session.GetNameByAccessibilityId("FuelEditorFuelTypeBox"));

            session.SendKeysByAccessibilityId("FuelEditorDateBox", "20.10.2026");
            session.SendKeysByAccessibilityId("FuelEditorFuelTypeBox", "Natural 95");
            session.SendKeysByAccessibilityId("FuelEditorLitersBox", "38.5");
            session.SendKeysByAccessibilityId("FuelEditorTotalCostBox", "1890");
            session.SendKeysByAccessibilityId("FuelEditorOdometerBox", "123789");
            session.SendKeysByAccessibilityId("FuelEditorNoteBox", "Appium tankování");
            session.ClickByAccessibilityId("SaveFuelButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveFuelButton");
        }
    }

    [Fact]
    public void Maintenance_editor_exposes_accessible_names_and_can_save_new_item_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("MaintenanceTabButton");
            session.ClickByAccessibilityId("CreateMaintenanceButton");

            Assert.Equal("Název servisního úkonu", session.GetNameByAccessibilityId("MaintenanceEditorTitleBox"));
            Assert.Equal("Interval údržby v kilometrech", session.GetNameByAccessibilityId("MaintenanceEditorIntervalKmBox"));

            session.SendKeysByAccessibilityId("MaintenanceEditorTitleBox", "Motorový olej");
            session.SendKeysByAccessibilityId("MaintenanceEditorIntervalKmBox", "15000");
            session.SendKeysByAccessibilityId("MaintenanceEditorIntervalMonthsBox", "12");
            session.SendKeysByAccessibilityId("MaintenanceEditorLastServiceDateBox", "01.04.2026");
            session.SendKeysByAccessibilityId("MaintenanceEditorLastServiceOdometerBox", "120000");
            session.SendKeysByAccessibilityId("MaintenanceEditorNoteBox", "Appium servisní plán");
            session.ClickByAccessibilityId("SaveMaintenanceButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveMaintenanceButton");
        }
    }

    [Fact]
    public void Vehicle_detail_editor_exposes_accessible_names_and_can_save_new_vehicle_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("DetailTabButton");
            session.ClickByAccessibilityId("CreateVehicleButton");

            Assert.Equal("Název vozidla", session.GetNameByAccessibilityId("VehicleEditorNameBox"));
            Assert.Equal("Kategorie vozidla", session.GetNameByAccessibilityId("VehicleEditorCategoryBox"));

            session.SendKeysByAccessibilityId("VehicleEditorNameBox", "Appium vozidlo");
            session.SendKeysByAccessibilityId("VehicleEditorMakeModelBox", "Test model");
            session.SendKeysByAccessibilityId("VehicleEditorStateBox", "Běžný provoz");
            session.SendKeysByAccessibilityId("VehicleEditorPowertrainBox", "benzín");
            session.SendKeysByAccessibilityId("VehicleEditorNoteBox", "Vytvořeno UI testem");
            session.ClickByAccessibilityId("SaveVehicleButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveVehicleButton");
        }
    }

    [Fact]
    public void Global_search_can_open_matching_record_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("SearchTabButton");
            session.SendKeysByAccessibilityId("GlobalSearchTextBox", "Kooperativa");
            session.ClickByAccessibilityId("SearchOpenButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("CreateRecordButton"));
        }
    }

    [Fact]
    public void Timeline_can_open_matching_reminder_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("TimelineTabButton");
            session.SendKeysByAccessibilityId("TimelineSearchBox", "Objednat servis");
            session.ClickByAccessibilityId("TimelineOpenButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("CreateReminderButton"));
        }
    }

    [Fact]
    public void Upcoming_overview_can_open_matching_item_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("UpcomingOverviewTabButton");
            session.SendKeysByAccessibilityId("UpcomingOverviewSearchBox", "Objednat servis");
            session.ClickByAccessibilityId("UpcomingOverviewOpenButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("CreateReminderButton"));
        }
    }

    [Fact]
    public void Overdue_overview_can_open_matching_item_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("OverdueOverviewTabButton");
            session.SendKeysByAccessibilityId("OverdueOverviewSearchBox", "Propadlá pojistka");
            session.ClickByAccessibilityId("OverdueOverviewOpenButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("CreateRecordButton"));
        }
    }

    [Fact]
    public void Cost_workspace_can_open_selected_vehicle_with_enter_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("CostTabButton");
            session.SendKeysByAccessibilityId("CostListBox", Keys.Enter);
            Assert.NotNull(session.WaitForElementByAccessibilityId("CreateVehicleButton"));
        }
    }
}
