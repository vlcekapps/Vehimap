using OpenQA.Selenium;
using Xunit;

namespace Vehimap.Tests.UI;

[Trait("UiProfile", "Extended")]
public sealed class DesktopContinuousIntegrationSmokeTests
{
    [Fact]
    [Trait("UiProfile", "Smoke")]
    public void Main_shell_exposes_visible_startup_controls_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            Assert.NotNull(session.WaitForElementByAccessibilityId("VehicleListBox"));
            Assert.Equal("VehicleListBox", session.WaitForFocusedAutomationId(12, "VehicleListBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ShellStatusText"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("DetailTabButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenVehicleDetailWindowButton"));
        }
    }

    [Fact]
    public void App_menu_opens_accessible_tray_actions_dialog_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickMenuItem("AppMenuRoot", "OpenTrayActionsButton");

            Assert.NotNull(session.WaitForElementByAccessibilityId("TrayActionsWindow"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("TrayActionsBackgroundStatusText"));
            Assert.Equal(
                "ShowMainWindowTrayActionButton",
                session.WaitForFocusedAutomationId(12, "ShowMainWindowTrayActionButton"));
            Assert.True(session.IsEnabledByAccessibilityId("OpenBackgroundStatusTrayActionButton"));
            Assert.True(session.IsEnabledByAccessibilityId("CloseTrayActionsButton"));

            session.ClickByAccessibilityId("CloseTrayActionsButton");
            session.WaitForElementToDisappearByAccessibilityId("TrayActionsWindow");
        }
    }

    [Fact]
    public void Settings_dialog_persists_background_options_and_creates_backup_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            Assert.NotNull(session.TemporaryDataPath);
            var settingsPath = Path.Combine(session.TemporaryDataPath!, "settings.ini");
            var backupDirectory = Path.Combine(session.TemporaryDataPath!, "auto-backups");
            var backupCountBefore = Directory.Exists(backupDirectory)
                ? Directory.GetFiles(backupDirectory, "*.vehimapbak").Length
                : 0;

            session.ClickMenuItem("AppMenuRoot", "SettingsButton");

            Assert.NotNull(session.WaitForElementByAccessibilityId("RunAtStartupCheckBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("HideOnLaunchCheckBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("AutomaticBackupsEnabledCheckBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("AutomaticBackupIntervalDaysBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("AutomaticBackupKeepCountBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("CreateAutomaticBackupButton"));

            if (!session.IsSelectedByAccessibilityId("AutomaticBackupsEnabledCheckBox"))
            {
                session.ClickByAccessibilityId("AutomaticBackupsEnabledCheckBox");
            }

            session.WaitUntilCondition(
                () => session.IsEnabledByAccessibilityId("AutomaticBackupIntervalDaysBox", 1)
                    && session.IsEnabledByAccessibilityId("AutomaticBackupKeepCountBox", 1),
                "Po zapnuti automatickych zaloh maji byt pole intervalu a poctu ponechanych zaloh aktivni.");
            session.ReplaceTextByAccessibilityId("AutomaticBackupIntervalDaysBox", "2");
            session.ReplaceTextByAccessibilityId("AutomaticBackupKeepCountBox", "5");
            session.ClickByAccessibilityId("CreateAutomaticBackupButton");

            session.WaitForElementToDisappearByAccessibilityId("SettingsWindow");
            session.WaitUntilCondition(
                () => File.ReadAllText(settingsPath).Contains("automatic_backups_enabled=1", StringComparison.Ordinal)
                    && File.ReadAllText(settingsPath).Contains("automatic_backup_interval_days=2", StringComparison.Ordinal)
                    && File.ReadAllText(settingsPath).Contains("automatic_backup_keep_count=5", StringComparison.Ordinal),
                "Ulozeni nastaveni ma zapsat podporovane volby automatickych zaloh do settings.ini.");
            session.WaitUntilCondition(
                () => Directory.Exists(backupDirectory)
                    && Directory.GetFiles(backupDirectory, "*.vehimapbak").Length > backupCountBefore,
                "Tlacitko Zalohovat ihned v nastaveni ma vytvorit automatickou zalohu v izolovane datove slozce.");
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
            Assert.NotNull(session.WaitForElementByAccessibilityId("RunAtStartupCheckBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("HideOnLaunchCheckBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("AutomaticBackupsEnabledCheckBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("CreateAutomaticBackupButton"));
            session.ClickByAccessibilityId("CancelSettingsButton");
            session.WaitForElementToDisappearByAccessibilityId("SettingsWindow");

            session.ClickMenuItem("AppMenuRoot", "AboutButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("ReleaseNotesButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ThankAuthorButton"));
            session.ClickByAccessibilityId("CloseAboutButton");
            session.WaitForElementToDisappearByAccessibilityId("AboutWindow");

            session.ClickMenuItem("AppMenuRoot", "UpdateCheckButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("UpdateCloseButton"));
            session.ClickByAccessibilityId("UpdateCloseButton");
            session.WaitForElementToDisappearByAccessibilityId("UpdateCheckWindow");
        }
    }

    [Fact]
    public void Settings_dialog_saves_dashboard_launch_preference_and_updates_dashboard_state_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            Assert.NotNull(session.TemporaryDataPath);
            var settingsPath = Path.Combine(session.TemporaryDataPath!, "settings.ini");

            session.ClickMenuItem("AppMenuRoot", "SettingsButton");
            Assert.False(session.IsSelectedByAccessibilityId("ShowDashboardOnLaunchCheckBox"));

            session.ClickByAccessibilityId("ShowDashboardOnLaunchCheckBox");
            Assert.True(session.IsSelectedByAccessibilityId("ShowDashboardOnLaunchCheckBox"));

            session.ClickByAccessibilityId("SaveSettingsButton");
            session.WaitForElementToDisappearByAccessibilityId("SettingsWindow");

            session.WaitUntilCondition(
                () => File.ReadAllText(settingsPath).Contains("show_dashboard_on_launch=1", StringComparison.Ordinal),
                "Ulozeni nastaveni ma zapsat show_dashboard_on_launch=1 do settings.ini.");

            session.ClickByAccessibilityId("DashboardTabButton");
            session.WaitUntilCondition(
                () => session.IsSelectedByAccessibilityId("DashboardShowOnLaunchCheckBox", 1),
                "Dashboard ma po ulozeni nastaveni ukazat zapnutou volbu zobrazeni pri startu.");

            session.ClickMenuItem("AppMenuRoot", "SettingsButton");
            Assert.True(session.IsSelectedByAccessibilityId("ShowDashboardOnLaunchCheckBox"));
            session.ClickByAccessibilityId("CancelSettingsButton");
            session.WaitForElementToDisappearByAccessibilityId("SettingsWindow");
        }
    }

    [Fact]
    public void Settings_dialog_keeps_focusable_error_state_for_invalid_values_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickMenuItem("AppMenuRoot", "SettingsButton");

            session.ReplaceTextByAccessibilityId("TechnicalReminderDaysBox", "abc");
            session.ClickByAccessibilityId("SaveSettingsButton");

            Assert.NotNull(session.WaitForElementByAccessibilityId("SettingsWindow"));
            Assert.Contains("rozsahu", session.GetNameByAccessibilityId("SettingsStatusText"), StringComparison.CurrentCulture);
            session.ClickByAccessibilityId("CancelSettingsButton");
        }
    }

    [Fact]
    public void Vehicle_menu_opens_manual_starter_bundle_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("VehicleMenuRoot");

            Assert.Equal("Otevřít balíček pro vybrané vozidlo", session.GetNameByAccessibilityId("OpenVehicleStarterBundleMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("OpenVehicleStarterBundleMenuItem"));

            session.ClickByAccessibilityId("OpenVehicleStarterBundleMenuItem");

            Assert.NotNull(session.WaitForElementByAccessibilityId("VehicleStarterBundleWindow"));
            var summary = session.GetNameByAccessibilityId("BundleSummaryText");
            Assert.Contains("Servis", summary, StringComparison.CurrentCulture);
            Assert.Contains("Doklady", summary, StringComparison.CurrentCulture);
            Assert.Contains("Připomínky", summary, StringComparison.CurrentCulture);
            Assert.NotNull(session.WaitForElementByAccessibilityId("BundleItemsListBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ApplyBundleButton"));

            session.ClickByAccessibilityId("CancelBundleButton");
            session.WaitForElementToDisappearByAccessibilityId("VehicleStarterBundleWindow");
        }
    }

    [Fact]
    public void Vehicle_menu_opens_accessible_service_book_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("VehicleMenuRoot");

            Assert.Equal("Otevřít servisní knížku vybraného vozidla", session.GetNameByAccessibilityId("OpenServiceBookMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("OpenServiceBookMenuItem"));

            session.ClickByAccessibilityId("OpenServiceBookMenuItem");

            Assert.NotNull(session.WaitForElementByAccessibilityId("ServiceBookWindow"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ServiceBookVehicleSummaryText"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ServiceBookStatusText"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ServiceBookContentScrollViewer"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ExportServiceBookHtmlButton"));

            session.ClickByAccessibilityId("CloseServiceBookWindowButton");
            session.WaitForElementToDisappearByAccessibilityId("ServiceBookWindow");
        }
    }

    [Fact]
    public void Overview_menu_opens_accessible_smart_advisor_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("OverviewMenuRoot");

            Assert.Equal("Otevřít chytrého poradce", session.GetNameByAccessibilityId("OpenSmartAdvisorMenuItem"));

            session.ClickByAccessibilityId("OpenSmartAdvisorMenuItem");

            Assert.NotNull(session.WaitForElementByAccessibilityId("SmartAdvisorWindow"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("SmartAdvisorSummaryText"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("SmartAdvisorSearchBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("SmartAdvisorListBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("SmartAdvisorOpenItemButton"));

            session.ClickByAccessibilityId("CloseSmartAdvisorWindowButton");
            session.WaitForElementToDisappearByAccessibilityId("SmartAdvisorWindow");
        }
    }

    [Fact]
    public void Vehicle_detail_editor_saves_new_vehicle_and_opens_starter_bundle_when_appium_is_available()
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

            Assert.Equal("Název vozidla", session.GetNameByAccessibilityId("VehicleEditorNameBox"));
            Assert.Equal("Kategorie vozidla", session.GetNameByAccessibilityId("VehicleEditorCategoryBox"));

            session.SendKeysByAccessibilityId("VehicleEditorNameBox", "CI vozidlo");
            session.SendKeysByAccessibilityId("VehicleEditorMakeModelBox", "Test model");
            session.SendKeysByAccessibilityId("VehicleEditorNextTkBox", "12/2099");
            session.SendKeysByAccessibilityId("VehicleEditorStateBox", "Běžný provoz");
            session.SendKeysByAccessibilityId("VehicleEditorPowertrainBox", "Benzín");
            session.SendKeysByAccessibilityId("VehicleEditorNoteBox", "Vytvořeno CI smoke testem");
            session.ClickByAccessibilityId("SaveVehicleButton");

            Assert.NotNull(session.WaitForElementByAccessibilityId("VehicleStarterBundleWindow"));
            var summary = session.GetNameByAccessibilityId("BundleSummaryText");
            Assert.Contains("Servis", summary, StringComparison.CurrentCulture);
            Assert.Contains("Doklady", summary, StringComparison.CurrentCulture);
            Assert.Contains("Připomínky", summary, StringComparison.CurrentCulture);
            Assert.NotNull(session.WaitForElementByAccessibilityId("BundleItemsListBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ApplyBundleButton"));

            session.ClickByAccessibilityId("CancelBundleButton");
            session.WaitForElementToDisappearByAccessibilityId("VehicleStarterBundleWindow");
            session.ClickByAccessibilityId("CloseVehicleDetailWindowButton");
        }
    }

    [Fact]
    public void Maintenance_workspace_opens_recommended_templates_when_appium_is_available()
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

            Assert.NotNull(session.WaitForElementByAccessibilityId("CloseMaintenanceWindowButton"));
            Assert.True(session.IsEnabledByAccessibilityId("OpenMaintenanceTemplatesButton"));

            session.ClickByAccessibilityId("OpenMaintenanceTemplatesButton");

            Assert.NotNull(session.WaitForElementByAccessibilityId("VehicleStarterBundleWindow"));
            Assert.Contains("Doporučené servisní šablony", session.GetNameByAccessibilityId("VehicleStarterBundleWindow"), StringComparison.CurrentCulture);
            Assert.Contains("servisních šablon", session.GetNameByAccessibilityId("BundleSummaryText"), StringComparison.CurrentCulture);
            Assert.NotNull(session.WaitForElementByAccessibilityId("BundleItemsListBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("BundleMaintenanceTitleBox"));

            session.ClickByAccessibilityId("CancelBundleButton");
            session.WaitForElementToDisappearByAccessibilityId("VehicleStarterBundleWindow");
            session.ClickByAccessibilityId("CloseMaintenanceWindowButton");
        }
    }

    [Fact]
    public void Maintenance_workspace_opens_completion_dialog_when_appium_is_available()
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

            Assert.NotNull(session.WaitForElementByAccessibilityId("CloseMaintenanceWindowButton"));
            session.ClickByAccessibilityId("MaintenanceListBox");
            Assert.True(session.IsEnabledByAccessibilityId("CompleteMaintenanceButton"));

            session.ClickByAccessibilityId("CompleteMaintenanceButton");

            Assert.NotNull(session.WaitForElementByAccessibilityId("MaintenanceCompletionWindow"));
            Assert.Equal("Datum provedení servisního úkonu", session.GetNameByAccessibilityId("MaintenanceCompletionDateBox"));
            Assert.Equal("Tachometr při provedení servisního úkonu", session.GetNameByAccessibilityId("MaintenanceCompletionOdometerBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("MaintenanceCompletionAddHistoryCheckBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("MaintenanceCompletionHistoryCostBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("MaintenanceCompletionHistoryNoteBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("SaveMaintenanceCompletionButton"));

            session.ClickByAccessibilityId("CancelMaintenanceCompletionButton");
            session.WaitForElementToDisappearByAccessibilityId("MaintenanceCompletionWindow");
            session.ClickByAccessibilityId("CloseMaintenanceWindowButton");
        }
    }

    [Fact]
    public void Dashboard_opens_maintenance_completion_dialog_when_maintenance_term_is_selected()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("DashboardTabButton");

            Assert.NotNull(session.WaitForElementByAccessibilityId("DashboardTimelineListBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("DashboardCompleteMaintenanceButton"));

            session.ClickByAccessibilityId("DashboardTimelineListBox");
            session.SendKeysByAccessibilityId("DashboardTimelineListBox", Keys.Home);

            for (var attempt = 0; attempt < 12 && !session.IsEnabledByAccessibilityId("DashboardCompleteMaintenanceButton", 2); attempt++)
            {
                session.SendKeysByAccessibilityId("DashboardTimelineListBox", Keys.ArrowDown, 2);
            }

            Assert.True(session.IsEnabledByAccessibilityId("DashboardCompleteMaintenanceButton"));

            session.ClickByAccessibilityId("DashboardCompleteMaintenanceButton");

            Assert.NotNull(session.WaitForElementByAccessibilityId("MaintenanceCompletionWindow"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("MaintenanceCompletionDateBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("MaintenanceCompletionOdometerBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("MaintenanceCompletionAddHistoryCheckBox"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("SaveMaintenanceCompletionButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("CancelMaintenanceCompletionButton"));

            session.ClickByAccessibilityId("CancelMaintenanceCompletionButton");
            session.WaitForElementToDisappearByAccessibilityId("MaintenanceCompletionWindow");
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

            var focusedId = session.WaitForFocusedAutomationId(12, "FileMenuRoot");

            Assert.Equal("FileMenuRoot", focusedId);

            session.SendKeysToActiveElement(Keys.F10);
            Assert.Equal("VehicleListBox", session.WaitForFocusedAutomationId(12, "VehicleListBox"));

            session.SendKeysToActiveElement(Keys.F10);
            Assert.Equal("FileMenuRoot", session.WaitForFocusedAutomationId(12, "FileMenuRoot"));

            session.SendKeysToActiveElement(Keys.ArrowDown);

            Assert.NotNull(session.WaitForElementByAccessibilityId("PrintableReportButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("FileExitAppButton"));
        }
    }

    [Fact]
    public void Main_menu_roots_are_skipped_by_regular_tab_navigation_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            var menuRoots = new[] { "FileMenuRoot", "VehicleMenuRoot", "OverviewMenuRoot", "QuickActionsMenuRoot", "AppMenuRoot" };

            session.ClickByAccessibilityId("HideInactiveVehiclesCheckBox");
            session.SendKeysToActiveElement(Keys.Tab);

            Assert.DoesNotContain(session.GetFocusedAutomationId(), menuRoots);

            session.ClickByAccessibilityId("VehicleCategoryFilterBox");
            session.SendKeysToActiveElement(Keys.Shift + Keys.Tab);

            Assert.DoesNotContain(session.GetFocusedAutomationId(), menuRoots);
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
    public void Workspace_windows_open_from_selected_tabs_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            OpenAndCloseWorkspaceWindow(
                session,
                "DetailTabButton",
                "OpenVehicleDetailWindowButton",
                "CloseVehicleDetailWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "HistoryTabButton",
                "OpenHistoryWindowButton",
                "CloseHistoryWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "FuelTabButton",
                "OpenFuelWindowButton",
                "CloseFuelWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "ReminderTabButton",
                "OpenReminderWindowButton",
                "CloseRemindersWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "MaintenanceTabButton",
                "OpenMaintenanceWindowButton",
                "CloseMaintenanceWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "TimelineTabButton",
                "OpenTimelineWindowButton",
                "CloseTimelineWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "RecordTabButton",
                "OpenRecordWindowButton",
                "CloseRecordsWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "AuditTabButton",
                "OpenAuditWindowButton",
                "CloseAuditWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "CostTabButton",
                "OpenCostWindowButton",
                "CloseCostWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "DashboardTabButton",
                "OpenDashboardWindowButton",
                "CloseDashboardWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "SearchTabButton",
                "OpenGlobalSearchWindowButton",
                "CloseGlobalSearchWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "UpcomingOverviewTabButton",
                "OpenUpcomingOverviewWindowButton",
                "CloseUpcomingOverviewWindowButton");
            OpenAndCloseWorkspaceWindow(
                session,
                "OverdueOverviewTabButton",
                "OpenOverdueOverviewWindowButton",
                "CloseOverdueOverviewWindowButton");
        }
    }

    [Fact]
    public void Workspace_window_can_be_closed_with_escape_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("AuditTabButton");
            session.ClickByAccessibilityId("OpenAuditWindowButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("CloseAuditWindowButton"));

            session.SendKeysToActiveElement(Keys.Escape);

            session.WaitForElementToDisappearByAccessibilityId("CloseAuditWindowButton");
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
            session.SendKeysByAccessibilityId("OverdueOverviewSearchBox", "Propadlá pojistka");
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

    [Fact]
    public void Fuel_analysis_block_is_accessible_and_can_open_related_fuel_when_appium_is_available()
    {
        if (!DesktopAppiumTestSession.TryStart(out var startedSession, out _))
        {
            return;
        }

        var session = startedSession!;
        using (session)
        {
            session.ClickByAccessibilityId("FuelTabButton");

            var summary = session.GetNameByAccessibilityId("FuelAnalysisSummaryText");
            Assert.Contains("Průměrná spotřeba", summary, StringComparison.Ordinal);
            Assert.Equal("Vývoj spotřeby tankování", session.GetNameByAccessibilityId("FuelConsumptionSegmentsListBox"));
            Assert.Equal("Místa tankování a paliva", session.GetNameByAccessibilityId("FuelGroupSummariesListBox"));
            Assert.Equal("Upozornění analýzy tankování", session.GetNameByAccessibilityId("FuelAnalysisWarningsListBox"));

            session.ClickByAccessibilityId("OpenFuelConsumptionSegmentButton");

            Assert.Contains(
                "Související tankování bylo vybráno",
                session.GetNameByAccessibilityId("FuelEditorStatusText"),
                StringComparison.Ordinal);
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
            Assert.Equal("Název připomínky", session.GetNameByAccessibilityId("ReminderEditorTitleBox"));
            Assert.Equal("Termín připomínky", session.GetNameByAccessibilityId("ReminderEditorDueDateBox"));

            session.SendKeysByAccessibilityId("ReminderEditorTitleBox", "Appium připomínka");
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
            Assert.Equal("Název dokladu", session.GetNameByAccessibilityId("RecordEditorTitleBox"));
            Assert.Equal("Režim přílohy dokladu", session.GetNameByAccessibilityId("RecordAttachmentModeComboBox"));

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

            Assert.Equal("Datum tankování", session.GetNameByAccessibilityId("FuelEditorDateBox"));
            Assert.Equal("Typ paliva", session.GetNameByAccessibilityId("FuelEditorFuelTypeBox"));
            Assert.Equal("Detail paliva", session.GetNameByAccessibilityId("FuelEditorFuelDetailBox"));
            Assert.Equal("Místo tankování", session.GetNameByAccessibilityId("FuelEditorStationBox"));

            session.SendKeysByAccessibilityId("FuelEditorDateBox", "20.10.2026");
            session.SendKeysByAccessibilityId("FuelEditorFuelTypeBox", "Nafta");
            session.SendKeysByAccessibilityId("FuelEditorFuelDetailBox", "Shell FuelSave");
            session.SendKeysByAccessibilityId("FuelEditorStationBox", "Shell Brno Vídeňská");
            session.SendKeysByAccessibilityId("FuelEditorLitersBox", "38.5");
            session.SendKeysByAccessibilityId("FuelEditorTotalCostBox", "1890");
            session.SendKeysByAccessibilityId("FuelEditorOdometerBox", "123789");
            session.SendKeysByAccessibilityId("FuelEditorNoteBox", "Appium tankování");
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
            session.SendKeysByAccessibilityId("ReminderEditorTitleBox", "Rozpracovaná připomínka");

            session.ClickByAccessibilityId("CloseRemindersWindowButton");
            Assert.NotNull(session.WaitForElementByAccessibilityId("ConfirmationCancelButton"));
            session.ClickByAccessibilityId("ConfirmationCancelButton");

            Assert.NotNull(session.WaitForElementByAccessibilityId("SaveReminderButton"));
            Assert.Equal("ReminderEditorTitleBox", session.GetFocusedAutomationId());
        }
    }

    [Fact]
    public void Main_menu_data_and_quick_actions_expose_expected_action_states_and_route_current_alert_when_appium_is_available()
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
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenBackgroundNotificationMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("OpenBackgroundNotificationMenuItem"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenNearestReminderMenuItem"));
            Assert.False(session.IsEnabledByAccessibilityId("OpenNearestTechnicalMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("OpenNearestReminderMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("ReviewRecordsMenuItem"));

            session.ClickByAccessibilityId("OpenBackgroundNotificationMenuItem");

            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenReminderWindowButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ReminderListBox"));
        }
    }

    private static void OpenAndCloseWorkspaceWindow(
        DesktopAppiumTestSession session,
        string tabButtonAutomationId,
        string openButtonAutomationId,
        string closeButtonAutomationId)
    {
        session.ClickByAccessibilityId(tabButtonAutomationId);
        session.ClickByAccessibilityId(openButtonAutomationId);
        Assert.NotNull(session.WaitForElementByAccessibilityId(closeButtonAutomationId));
        session.ClickByAccessibilityId(closeButtonAutomationId);
        session.WaitForElementToDisappearByAccessibilityId(closeButtonAutomationId);
    }
}
