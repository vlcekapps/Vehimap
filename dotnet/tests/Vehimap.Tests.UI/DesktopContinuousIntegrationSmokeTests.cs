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
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenTrayActionsButton"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("ReloadButton"));
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
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenBackgroundNotificationMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("OpenBackgroundNotificationMenuItem"));
            Assert.NotNull(session.WaitForElementByAccessibilityId("OpenNearestReminderMenuItem"));
            Assert.False(session.IsEnabledByAccessibilityId("OpenNearestTechnicalMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("OpenNearestReminderMenuItem"));
            Assert.True(session.IsEnabledByAccessibilityId("ReviewRecordsMenuItem"));
        }
    }
}
