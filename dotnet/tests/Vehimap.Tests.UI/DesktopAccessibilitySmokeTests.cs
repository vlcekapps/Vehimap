using OpenQA.Selenium;
using Xunit;

namespace Vehimap.Tests.UI;

[Trait("UiProfile", "Extended")]
public sealed class DesktopAccessibilitySmokeTests
{
    [Fact]
    public void Main_shell_exposes_vehicle_list_and_menu_actions_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            Assert.NotNull(session.WaitForElementByAccessibilityId("VehicleListBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("AppMenuBar"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("FileMenuRoot"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("VehicleMenuRoot"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("QuickActionsMenuRoot"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("AppMenuRoot"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("VehicleCategoryFilterBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("VehicleSearchBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("VehicleStatusFilterBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("HideInactiveVehiclesCheckBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("DashboardTabButton"));
        }
    }

    [Fact]
    public void App_shell_dialogs_open_and_close_from_menu_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickMenuItem("AppMenuRoot", "SettingsButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("TechnicalReminderDaysBox"));
            session.ClickByAccessibilityId("CancelSettingsButton");

            session.ClickMenuItem("AppMenuRoot", "AboutButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("ReleaseNotesButton"));
            session.ClickByAccessibilityId("CloseAboutButton");

            session.ClickMenuItem("AppMenuRoot", "UpdateCheckButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("UpdateCloseButton"));
            session.ClickByAccessibilityId("UpdateCloseButton");
        }
    }

    [Fact]
    public void Main_menu_can_be_invoked_with_f10_without_entering_regular_tab_order_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("VehicleListBox");
            session.SendKeysToActiveElement(Keys.F10);
            var focusedId = session.WaitForFocusedAutomationId(12, "FileMenuRoot", "PrintableReportButton");
            Assert.Contains(focusedId, new[] { "FileMenuRoot", "PrintableReportButton" });
            Assert.NotNull(session.WaitForElementByAccessibilityId("PrintableReportButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ExitAppButton"));
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
    public void Reminder_editor_runs_in_standalone_window_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("ReminderTabButton");
            session.ClickByAccessibilityId("OpenReminderWindowButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("CloseRemindersWindowButton"));

            session.ClickByAccessibilityId("CreateReminderButton");
            Assert.Equal("NĂˇzev pĹ™ipomĂ­nky", session.GetNameByAccessibilityId("ReminderEditorTitleBox"));
            Assert.Equal("TermĂ­n pĹ™ipomĂ­nky", session.GetNameByAccessibilityId("ReminderEditorDueDateBox"));

            session.SendKeysByAccessibilityId("ReminderEditorTitleBox", "Appium pĹ™ipomĂ­nka");
            session.SendKeysByAccessibilityId("ReminderEditorDueDateBox", "12/2026");
            session.ClickByAccessibilityId("SaveReminderButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveReminderButton");
        }
    }

    [Fact]
    public void Record_editor_runs_in_standalone_window_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("RecordTabButton");
            session.ClickByAccessibilityId("OpenRecordWindowButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("CloseRecordsWindowButton"));

            session.ClickByAccessibilityId("CreateRecordButton");
            Assert.Equal("NĂˇzev dokladu", session.GetNameByAccessibilityId("RecordEditorTitleBox"));
            Assert.Equal("ReĹľim pĹ™Ă­lohy dokladu", session.GetNameByAccessibilityId("RecordAttachmentModeComboBox"));

            session.SendKeysByAccessibilityId("RecordEditorTitleBox", "Appium doklad");
            session.ClickByAccessibilityId("SaveRecordButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveRecordButton");
        }
    }

    [Fact]
    public void History_editor_runs_in_standalone_window_when_appium_is_available()
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

            session.ClickByAccessibilityId("CreateHistoryButton");
            Assert.Equal("Datum historickĂ©ho zĂˇznamu", session.GetNameByAccessibilityId("HistoryEditorDateBox"));
            Assert.Equal("Typ historickĂ© udĂˇlosti", session.GetNameByAccessibilityId("HistoryEditorTypeBox"));

            session.SendKeysByAccessibilityId("HistoryEditorDateBox", "15.10.2026");
            session.SendKeysByAccessibilityId("HistoryEditorTypeBox", "Servis");
            session.SendKeysByAccessibilityId("HistoryEditorOdometerBox", "123456");
            session.SendKeysByAccessibilityId("HistoryEditorCostBox", "2500");
            session.SendKeysByAccessibilityId("HistoryEditorNoteBox", "Appium historickĂ˝ zĂˇznam");
            session.ClickByAccessibilityId("SaveHistoryButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveHistoryButton");
        }
    }

    [Fact]
    public void Fuel_editor_runs_in_standalone_window_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("FuelTabButton");
            session.ClickByAccessibilityId("OpenFuelWindowButton");
            session.ClickByAccessibilityId("CreateFuelButton");

            Assert.Equal("Datum tankovĂˇnĂ­", session.GetNameByAccessibilityId("FuelEditorDateBox"));
            Assert.Equal("Typ paliva", session.GetNameByAccessibilityId("FuelEditorFuelTypeBox"));

            session.SendKeysByAccessibilityId("FuelEditorDateBox", "20.10.2026");
            session.SendKeysByAccessibilityId("FuelEditorFuelTypeBox", "Natural 95");
            session.SendKeysByAccessibilityId("FuelEditorLitersBox", "38.5");
            session.SendKeysByAccessibilityId("FuelEditorTotalCostBox", "1890");
            session.SendKeysByAccessibilityId("FuelEditorOdometerBox", "123789");
            session.SendKeysByAccessibilityId("FuelEditorNoteBox", "Appium tankovĂˇnĂ­");
            session.ClickByAccessibilityId("SaveFuelButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveFuelButton");
        }
    }

    [Fact]
    public void Maintenance_editor_runs_in_standalone_window_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("MaintenanceTabButton");
            session.ClickByAccessibilityId("OpenMaintenanceWindowButton");
            session.ClickByAccessibilityId("CreateMaintenanceButton");

            Assert.Equal("NĂˇzev servisnĂ­ho Ăşkonu", session.GetNameByAccessibilityId("MaintenanceEditorTitleBox"));
            Assert.Equal("Interval ĂşdrĹľby v kilometrech", session.GetNameByAccessibilityId("MaintenanceEditorIntervalKmBox"));

            session.SendKeysByAccessibilityId("MaintenanceEditorTitleBox", "MotorovĂ˝ olej");
            session.SendKeysByAccessibilityId("MaintenanceEditorIntervalKmBox", "15000");
            session.SendKeysByAccessibilityId("MaintenanceEditorIntervalMonthsBox", "12");
            session.SendKeysByAccessibilityId("MaintenanceEditorLastServiceDateBox", "01.04.2026");
            session.SendKeysByAccessibilityId("MaintenanceEditorLastServiceOdometerBox", "120000");
            session.SendKeysByAccessibilityId("MaintenanceEditorNoteBox", "Appium servisnĂ­ plĂˇn");
            session.ClickByAccessibilityId("SaveMaintenanceButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveMaintenanceButton");
        }
    }

    [Fact]
    public void Vehicle_detail_editor_runs_in_standalone_window_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("DetailTabButton");
            session.ClickByAccessibilityId("OpenVehicleDetailWindowButton");
            session.ClickByAccessibilityId("CreateVehicleButton");

            Assert.Equal("NĂˇzev vozidla", session.GetNameByAccessibilityId("VehicleEditorNameBox"));
            Assert.Equal("Kategorie vozidla", session.GetNameByAccessibilityId("VehicleEditorCategoryBox"));

            session.SendKeysByAccessibilityId("VehicleEditorNameBox", "Appium vozidlo");
            session.SendKeysByAccessibilityId("VehicleEditorMakeModelBox", "Test model");
            session.SendKeysByAccessibilityId("VehicleEditorStateBox", "BÄ›ĹľnĂ˝ provoz");
            session.SendKeysByAccessibilityId("VehicleEditorPowertrainBox", "benzĂ­n");
            session.SendKeysByAccessibilityId("VehicleEditorNoteBox", "VytvoĹ™eno UI testem");
            session.ClickByAccessibilityId("SaveVehicleButton");
            session.WaitForElementToDisappearByAccessibilityId("SaveVehicleButton");
        }
    }

    [Fact]
    public void Vehicle_detail_editor_shift_tab_from_name_returns_focus_to_cancel_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("DetailTabButton");
            session.ClickByAccessibilityId("OpenVehicleDetailWindowButton");
            session.ClickByAccessibilityId("CreateVehicleButton");
            session.ClickByAccessibilityId("VehicleEditorNameBox");
            session.SendKeysByAccessibilityId("VehicleEditorNameBox", Keys.Shift + Keys.Tab);

            Assert.Equal("CancelVehicleButton", session.GetFocusedAutomationId());
        }
    }

    [Fact]
    public void Closing_standalone_editor_with_pending_changes_prompts_for_discard_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("ReminderTabButton");
            session.ClickByAccessibilityId("OpenReminderWindowButton");
            session.ClickByAccessibilityId("CreateReminderButton");
            session.SendKeysByAccessibilityId("ReminderEditorTitleBox", "RozpracovanĂˇ pĹ™ipomĂ­nka");

            session.ClickByAccessibilityId("CloseRemindersWindowButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("ConfirmationCancelButton"));
            session.ClickByAccessibilityId("ConfirmationCancelButton");

            Assert.NotNull(session.WaitForElementByAccessibilityId("SaveReminderButton"));
            Assert.Equal("ReminderEditorTitleBox", session.GetFocusedAutomationId());
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
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenRecordWindowButton"));
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
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenReminderWindowButton"));
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
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenReminderWindowButton"));
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
            session.SendKeysByAccessibilityId("OverdueOverviewSearchBox", "PropadlĂˇ pojistka");
            session.ClickByAccessibilityId("OverdueOverviewOpenButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenRecordWindowButton"));
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
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenVehicleDetailWindowButton"));
        }
    }
}
