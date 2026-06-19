using Vehimap.Desktop.ViewModels;
using Xunit;
using System.Text.RegularExpressions;

namespace Vehimap.Tests.Unit;

public sealed class DesktopAccessibilityLabelTests
{
    [Fact]
    public void Vehicle_list_items_should_expose_human_readable_labels()
    {
        var item = new VehicleListItemViewModel(
            "veh_1",
            "Božena",
            "Osobní vozidla",
            "Bez SPZ",
            "Škoda 100",
            "Srazové",
            "09/2026",
            "10/2026",
            "Veterán",
            string.Empty,
            "Veterán | 1 položek k řešení");

        Assert.Equal(item.AccessibleLabel, item.ToString());
        Assert.Contains("Božena", item.AccessibleLabel);
        Assert.Contains("Škoda 100", item.AccessibleLabel);
        Assert.DoesNotContain(nameof(VehicleListItemViewModel), item.AccessibleLabel);
    }

    [Fact]
    public void Vehicle_record_items_should_expose_human_readable_labels()
    {
        var item = new VehicleRecordItemViewModel(
            "rec_1",
            "Povinné ručení",
            "Povinné ručení",
            "Kooperativa",
            "03/2026 až 03/2027",
            "2 000 Kč",
            "Spravovaná kopie",
            "Příloha je dostupná",
            "attachments/veh_1/povinne-ruceni.pdf",
            @"C:\vehimap\data\attachments\veh_1\povinne-ruceni.pdf",
            true,
            "Roční smlouva");

        Assert.Equal(item.AccessibleLabel, item.ToString());
        Assert.Contains("Kooperativa", item.AccessibleLabel);
        Assert.Contains("Povinné ručení", item.AccessibleLabel);
        Assert.DoesNotContain(nameof(VehicleRecordItemViewModel), item.AccessibleLabel);
    }

    [Fact]
    public void Tray_actions_dialog_defaults_should_expose_overview_actions()
    {
        var model = TrayActionsDialogViewModel.CreateDefault();

        Assert.Equal("Zobrazit Vehimap", model.ShowMainWindowLabel);
        Assert.Equal("Otevřít Dashboard", model.ShowDashboardLabel);
        Assert.Equal("Blížící se termíny", model.ShowUpcomingOverviewLabel);
        Assert.Equal("Propadlé termíny", model.ShowOverdueOverviewLabel);
        Assert.Equal("Nejbližší TK", model.OpenNearestTechnicalLabel);
        Assert.Equal("Nejbližší ZK", model.OpenNearestGreenCardLabel);
        Assert.Equal("Nejbližší připomínka", model.OpenNearestReminderLabel);
        Assert.Equal("Nejbližší servis", model.OpenNearestMaintenanceLabel);
        Assert.Equal("Nejbližší doklad", model.OpenNearestRecordLabel);
        Assert.Equal("Zkontrolovat TK", model.ReviewTechnicalLabel);
        Assert.Equal("Zkontrolovat ZK", model.ReviewGreenCardsLabel);
        Assert.Equal("Zkontrolovat připomínky", model.ReviewRemindersLabel);
        Assert.Equal("Zkontrolovat údržbu", model.ReviewMaintenanceLabel);
        Assert.Equal("Zkontrolovat doklady", model.ReviewRecordsLabel);
        Assert.Equal("Tiskový přehled", model.OpenPrintableReportLabel);
        Assert.Equal("Export dat do zálohy", model.ExportBackupLabel);
        Assert.Equal("Obnovit data ze zálohy", model.ImportBackupLabel);
        Assert.Equal("Nastavení", model.OpenSettingsLabel);
        Assert.Equal("Export termínů do kalendáře", model.ExportCalendarLabel);
        Assert.Equal("Načíst data znovu", model.ReloadDataLabel);
        Assert.Equal("O programu", model.OpenAboutLabel);
        Assert.Equal("Zkontrolovat aktualizace", model.CheckForUpdatesLabel);
        Assert.Equal("Ukončit aplikaci", model.ExitLabel);
    }

    [Fact]
    public void Main_window_xaml_should_define_accessible_shell_metadata()
    {
        var xaml = ReadViewFile("MainWindow.axaml");
        var normalizedXaml = Regex.Replace(xaml, "\\s+", " ");

        Assert.Contains("AutomationProperties.Name=\"Vehimap\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AppMenuBar\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FileMenuRoot\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleMenuRoot\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverviewMenuRoot\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"QuickActionsMenuRoot\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AppMenuRoot\"", xaml);
        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusCurrentSearchCommand}\"", xaml);
        Assert.Contains("Gesture=\"Ctrl+D\" Command=\"{Binding FocusDashboardCommand}\"", xaml);
        Assert.Contains("Gesture=\"Ctrl+T\" Command=\"{Binding FocusUpcomingOverviewCommand}\"", xaml);
        Assert.Contains("Gesture=\"Ctrl+Shift+T\" Command=\"{Binding FocusOverdueOverviewCommand}\"", xaml);
        Assert.DoesNotContain("Gesture=\"Ctrl+F\" Command=\"{Binding FocusTimelineSearchCommand}\"", xaml);
        Assert.DoesNotContain("Gesture=\"Ctrl+Shift+D\"", xaml);
        Assert.Contains("x:Name=\"AppMenuBar\"", xaml);
        Assert.Contains("x:Name=\"FileMenuRoot\" Header=\"_Soubor\" IsTabStop=\"False\"", normalizedXaml);
        Assert.Contains("x:Name=\"VehicleMenuRoot\" Header=\"_Vozidlo\" IsTabStop=\"False\"", normalizedXaml);
        Assert.Contains("x:Name=\"OverviewMenuRoot\" Header=\"_Přehledy\" IsTabStop=\"False\"", normalizedXaml);
        Assert.Contains("x:Name=\"QuickActionsMenuRoot\" Header=\"_Rychlé akce\" IsTabStop=\"False\"", normalizedXaml);
        Assert.Contains("x:Name=\"AppMenuRoot\" Header=\"_Aplikace\" IsTabStop=\"False\"", normalizedXaml);
        Assert.Contains("x:Name=\"MinimizeToTrayButton\" Header=\"Minimalizovat na lištu\" Click=\"OnMinimizeToTrayClick\" IsEnabled=\"{Binding IsMinimizeToTrayAvailable}\"", normalizedXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"PrintableReportButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MinimizeToTrayButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SettingsButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AboutButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpdateCheckButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExitAppButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CreateVehicleMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"EditVehicleMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DeleteVehicleMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenVehicleDetailMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenHistoryMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenFuelMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenRecordsMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenRemindersMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenMaintenanceMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenVehicleStarterBundleMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenSelectedVehicleCostsMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenTimelineMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenGlobalSearchMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenAuditMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDashboardMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenCostMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenUpcomingOverviewMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenOverdueOverviewMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestTechnicalMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewTechnicalMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestGreenCardMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewGreenCardsMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestReminderMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewRemindersMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestMaintenanceMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewMaintenanceMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestRecordMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewRecordsMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CalendarExportButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReloadButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleCategoryFilterBox\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleSearchBox\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleStatusFilterBox\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearVehicleFiltersButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HideInactiveVehiclesCheckBox\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleListBox\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenHistoryWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenTimelineWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenCostWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDashboardWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenGlobalSearchWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenUpcomingOverviewWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenOverdueOverviewWindowButton\"", xaml);
        Assert.Contains("AllowEditing=\"False\"", xaml);
        Assert.Contains("Click=\"OnSettingsClick\"", xaml);
        Assert.Contains("Click=\"OnAboutClick\"", xaml);
        Assert.Contains("Click=\"OnUpdateCheckClick\"", xaml);
        Assert.Contains("Click=\"OnMinimizeToTrayClick\"", xaml);
        Assert.Contains("Click=\"OnExitClick\"", xaml);
        Assert.Contains("Click=\"OnPrintableReportClick\"", xaml);
        Assert.Contains("Click=\"OnCalendarExportClick\"", xaml);
        Assert.Contains("Click=\"OnReloadClick\"", xaml);
        Assert.Contains("Click=\"OnCreateVehicleMenuClick\"", xaml);
        Assert.Contains("Click=\"OnDeleteVehicleMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenSelectedVehicleCostsMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenTimelineMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenGlobalSearchMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenAuditMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenDashboardMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenCostMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenUpcomingOverviewMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenOverdueOverviewMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenNearestTechnicalMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenNearestReminderMenuClick\"", xaml);
        Assert.Contains("Click=\"OnReviewRemindersMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenNearestMaintenanceMenuClick\"", xaml);
        Assert.Contains("Click=\"OnReviewMaintenanceMenuClick\"", xaml);
        Assert.Contains("Click=\"OnOpenNearestRecordMenuClick\"", xaml);
        Assert.Contains("Click=\"OnReviewRecordsMenuClick\"", xaml);

        var codeBehind = ReadViewCodeBehind("MainWindow.axaml.cs");
        Assert.Contains("Key.F10", codeBehind);
        Assert.Contains("Key.LeftAlt", codeBehind);
        Assert.Contains("Key.RightAlt", codeBehind);
        Assert.Contains("HandleCurrentWorkspacePrimaryOpenShortcutAsync", codeBehind);
        Assert.Contains("HandleCurrentWorkspaceItemOpenShortcutAsync", codeBehind);
        Assert.Contains("HandleCurrentWorkspaceCreateShortcut", codeBehind);
        Assert.Contains("HandleCurrentWorkspaceEditShortcut", codeBehind);
        Assert.Contains("HandleCurrentWorkspaceEditShortcutAsync", codeBehind);
        Assert.Contains("HandleCurrentWorkspaceSaveShortcutAsync", codeBehind);
        Assert.Contains("FocusAndOpenMainMenu()", codeBehind);
        Assert.Contains("case Key.N", codeBehind);
        Assert.Contains("case Key.S", codeBehind);
        Assert.Contains("Key.F2", codeBehind);
        Assert.Contains("case Key.R", codeBehind);
        Assert.Contains("case Key.M", codeBehind);
    }

    [Fact]
    public void Dialog_xaml_files_should_define_expected_automation_ids()
    {
        var settingsXaml = ReadViewFile("SettingsWindow.axaml");
        var settingsCodeBehind = ReadViewCodeBehind("SettingsWindow.axaml.cs");
        var aboutXaml = ReadViewFile("AboutWindow.axaml");
        var aboutCodeBehind = ReadViewCodeBehind("AboutWindow.axaml.cs");
        var updateXaml = ReadViewFile("UpdateCheckWindow.axaml");
        var updateCodeBehind = ReadViewCodeBehind("UpdateCheckWindow.axaml.cs");
        var notificationXaml = ReadViewFile("NotificationWindow.axaml");
        var notificationCodeBehind = ReadViewCodeBehind("NotificationWindow.axaml.cs");
        var maintenanceCompletionXaml = ReadViewFile("MaintenanceCompletionWindow.axaml");
        var maintenanceCompletionCodeBehind = ReadViewCodeBehind("MaintenanceCompletionWindow.axaml.cs");
        var vehicleDetailXaml = ReadViewFile("VehicleDetailWindow.axaml");
        var historyXaml = ReadViewFile("HistoryWindow.axaml");
        var fuelXaml = ReadViewFile("FuelWindow.axaml");
        var remindersXaml = ReadViewFile("RemindersWindow.axaml");
        var maintenanceXaml = ReadViewFile("MaintenanceWindow.axaml");
        var recordsXaml = ReadViewFile("RecordsWindow.axaml");
        var auditXaml = ReadViewFile("AuditWindow.axaml");
        var dashboardXaml = ReadViewFile("DashboardWindow.axaml");
        var timelineXaml = ReadViewFile("TimelineWindow.axaml");
        var costXaml = ReadViewFile("CostWindow.axaml");
        var globalSearchXaml = ReadViewFile("GlobalSearchWindow.axaml");
        var upcomingOverviewXaml = ReadViewFile("UpcomingOverviewWindow.axaml");
        var overdueOverviewXaml = ReadViewFile("OverdueOverviewWindow.axaml");
        var bundleXaml = ReadViewFile("VehicleStarterBundleWindow.axaml");
        var confirmationXaml = ReadViewFile("ConfirmationWindow.axaml");
        var confirmationCodeBehind = ReadViewCodeBehind("ConfirmationWindow.axaml.cs");
        var trayActionsXaml = ReadViewFile("TrayActionsWindow.axaml");
        var trayActionsCodeBehind = ReadViewCodeBehind("TrayActionsWindow.axaml.cs");

        Assert.Contains("AutomationProperties.AutomationId=\"TechnicalReminderDaysBox\"", settingsXaml);
        Assert.Contains("Ctrl+S uloží nastavení, Ctrl+B vytvoří zálohu ihned a Escape dialog zavře bez uložení.", settingsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanConfigureAutomaticBackups}\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CreateAutomaticBackupButton\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SaveSettingsButton\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CancelSettingsButton\"", settingsXaml);
        Assert.Contains("Key.Escape", settingsCodeBehind);
        Assert.Contains("case Key.S", settingsCodeBehind);
        Assert.Contains("case Key.B", settingsCodeBehind);
        Assert.Contains("AutomationProperties.AutomationId=\"ReleaseNotesButton\"", aboutXaml);
        Assert.Contains("Ctrl+O otevře release poznámky a Escape dialog zavře.", aboutXaml);
        Assert.Contains("Key.Escape", aboutCodeBehind);
        Assert.Contains("Key.O", aboutCodeBehind);
        Assert.Contains("AutomationProperties.AutomationId=\"UpdateCloseButton\"", updateXaml);
        Assert.Contains("AutomationProperties.Name=\"Kontrola aktualizací\"", updateXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpdateCheckWindow\"", updateXaml);
        Assert.Contains("Escape dialog zavře.", updateXaml);
        Assert.Contains("Key.Escape", updateCodeBehind);
        Assert.Contains("AutomationProperties.Name=\"{Binding PrimaryActionLabel}\"", updateXaml);
        Assert.Contains("AutomationProperties.Name=\"Zavřít kontrolu aktualizací\"", updateXaml);
        Assert.Contains("AutomationProperties.Name=\"Upozornění Vehimapu\"", notificationXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"NotificationWindow\"", notificationXaml);
        Assert.Contains("Zavře se samo; při aktivaci ho lze zavřít klávesou Escape.", notificationXaml);
        Assert.Contains("Key.Escape", notificationCodeBehind);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceCompletionWindow\"", maintenanceCompletionXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceCompletionDateBox\"", maintenanceCompletionXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceCompletionOdometerBox\"", maintenanceCompletionXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceCompletionAddHistoryCheckBox\"", maintenanceCompletionXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceCompletionHistoryCostBox\"", maintenanceCompletionXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceCompletionHistoryNoteBox\"", maintenanceCompletionXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SaveMaintenanceCompletionButton\"", maintenanceCompletionXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CancelMaintenanceCompletionButton\"", maintenanceCompletionXaml);
        Assert.Contains("Ctrl+S uloží, Escape zavře bez změn.", maintenanceCompletionXaml);
        Assert.Contains("Key.Escape", maintenanceCompletionCodeBehind);
        Assert.Contains("Key.S", maintenanceCompletionCodeBehind);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseVehicleDetailWindowButton\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseHistoryWindowButton\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseFuelWindowButton\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseRemindersWindowButton\"", remindersXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseMaintenanceWindowButton\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseRecordsWindowButton\"", recordsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseAuditWindowButton\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseDashboardWindowButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseTimelineWindowButton\"", timelineXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseCostWindowButton\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseGlobalSearchWindowButton\"", globalSearchXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseUpcomingOverviewWindowButton\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseOverdueOverviewWindowButton\"", overdueOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"BundleItemsListBox\"", bundleXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ApplyBundleButton\"", bundleXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ConfirmationConfirmButton\"", confirmationXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ConfirmationCancelButton\"", confirmationXaml);
        Assert.Contains("Escape akci zruší.", confirmationXaml);
        Assert.Contains("Key.Escape", confirmationCodeBehind);
        Assert.Contains("AutomationProperties.AutomationId=\"ShowMainWindowTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ShowDashboardTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ShowUpcomingOverviewTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ShowOverdueOverviewTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestTechnicalTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestGreenCardTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestReminderTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestMaintenanceTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestRecordTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewTechnicalTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewGreenCardsTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewRemindersTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewMaintenanceTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewRecordsTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenPrintableReportTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExportBackupTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ImportBackupTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenSettingsTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExportCalendarTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReloadDataTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenAboutTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CheckForUpdatesTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExitTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseTrayActionsButton\"", trayActionsXaml);
        Assert.Contains("Escape okno zavře bez akce.", trayActionsXaml);
        Assert.Contains("OnShowUpcomingOverviewClick", trayActionsCodeBehind);
        Assert.Contains("OnShowOverdueOverviewClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenNearestTechnicalClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenNearestGreenCardClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenNearestReminderClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenNearestMaintenanceClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenNearestRecordClick", trayActionsCodeBehind);
        Assert.Contains("OnReviewTechnicalClick", trayActionsCodeBehind);
        Assert.Contains("OnReviewGreenCardsClick", trayActionsCodeBehind);
        Assert.Contains("OnReviewRemindersClick", trayActionsCodeBehind);
        Assert.Contains("OnReviewMaintenanceClick", trayActionsCodeBehind);
        Assert.Contains("OnReviewRecordsClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenPrintableReportClick", trayActionsCodeBehind);
        Assert.Contains("OnExportBackupClick", trayActionsCodeBehind);
        Assert.Contains("OnImportBackupClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenSettingsClick", trayActionsCodeBehind);
        Assert.Contains("OnExportCalendarClick", trayActionsCodeBehind);
        Assert.Contains("OnReloadDataClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenAboutClick", trayActionsCodeBehind);
        Assert.Contains("OnCheckForUpdatesClick", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ShowUpcomingOverview", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ShowOverdueOverview", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenNearestTechnical", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenNearestGreenCard", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenNearestReminder", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenNearestMaintenance", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenNearestRecord", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ReviewTechnical", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ReviewGreenCards", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ReviewReminders", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ReviewMaintenance", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ReviewRecords", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenPrintableReport", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ExportBackup", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ImportBackup", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenSettings", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ExportCalendar", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ReloadData", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenAbout", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.CheckForUpdates", trayActionsCodeBehind);
        Assert.Contains("Key.Escape", trayActionsCodeBehind);
    }

    [Fact]
    public void Tray_runtime_controller_should_route_quick_actions_to_shell_commands()
    {
        var runtimeController = ReadDesktopServiceFile("DesktopAppRuntimeController.cs");

        Assert.Contains("TrayActionsDialogAction.OpenNearestTechnical", runtimeController);
        Assert.Contains("_shell.OpenNearestTechnicalCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.OpenNearestGreenCard", runtimeController);
        Assert.Contains("_shell.OpenNearestGreenCardCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.OpenNearestReminder", runtimeController);
        Assert.Contains("_shell.OpenNearestReminderCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.OpenNearestMaintenance", runtimeController);
        Assert.Contains("_shell.OpenNearestMaintenanceCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.OpenNearestRecord", runtimeController);
        Assert.Contains("_shell.OpenNearestRecordCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ReviewTechnical", runtimeController);
        Assert.Contains("_shell.ReviewTechnicalCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ReviewGreenCards", runtimeController);
        Assert.Contains("_shell.ReviewGreenCardsCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ReviewReminders", runtimeController);
        Assert.Contains("_shell.ReviewRemindersCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ReviewMaintenance", runtimeController);
        Assert.Contains("_shell.ReviewMaintenanceCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ReviewRecords", runtimeController);
        Assert.Contains("_shell.ReviewRecordsCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.OpenPrintableReport", runtimeController);
        Assert.Contains("_shell.AppShellController.OpenPrintableReportAsync(_shell)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ExportBackup", runtimeController);
        Assert.Contains("_shell.AppShellController.ExportBackupAsync(_mainWindow, _shell)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ImportBackup", runtimeController);
        Assert.Contains("_shell.AppShellController.ImportBackupAsync(_mainWindow, _shell)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.OpenSettings", runtimeController);
        Assert.Contains("_shell.AppShellController.OpenSettingsAsync(_mainWindow, _shell)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ExportCalendar", runtimeController);
        Assert.Contains("_shell.ExportCalendarCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ReloadData", runtimeController);
        Assert.Contains("_shell.ReloadCommand.Execute(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.OpenAbout", runtimeController);
        Assert.Contains("_shell.AppShellController.OpenAboutAsync(_mainWindow, _shell)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.CheckForUpdates", runtimeController);
        Assert.Contains("_shell.AppShellController.CheckForUpdatesAsync(_mainWindow, _shell)", runtimeController);
    }

    [Fact]
    public void Window_roots_should_define_accessible_name_and_automation_id()
    {
        var windowFiles = new[]
        {
            "AboutWindow.axaml",
            "AuditWindow.axaml",
            "ConfirmationWindow.axaml",
            "CostWindow.axaml",
            "DashboardWindow.axaml",
            "FuelWindow.axaml",
            "GlobalSearchWindow.axaml",
            "HistoryWindow.axaml",
            "MaintenanceWindow.axaml",
            "MainWindow.axaml",
            "MaintenanceCompletionWindow.axaml",
            "NotificationWindow.axaml",
            "OverdueOverviewWindow.axaml",
            "RecordsWindow.axaml",
            "RemindersWindow.axaml",
            "SettingsWindow.axaml",
            "TimelineWindow.axaml",
            "TrayActionsWindow.axaml",
            "UpcomingOverviewWindow.axaml",
            "UpdateCheckWindow.axaml",
            "VehicleDetailWindow.axaml",
            "VehicleStarterBundleWindow.axaml",
        };

        foreach (var fileName in windowFiles)
        {
            var root = ReadWindowRootElement(fileName);
            Assert.Contains("AutomationProperties.Name=", root);
            Assert.Contains("AutomationProperties.AutomationId=", root);
        }
    }

    [Fact]
    public void Editor_fields_should_define_explicit_accessibility_automation_ids()
    {
        var settingsXaml = ReadWorkspaceOrView("SettingsWindow.axaml", false);
        var auditXaml = ReadWorkspaceOrView("AuditWorkspaceView.axaml", true);
        var dashboardXaml = ReadWorkspaceOrView("DashboardWorkspaceView.axaml", true);
        var fuelXaml = ReadWorkspaceOrView("FuelWorkspaceView.axaml", true);
        var historyXaml = ReadWorkspaceOrView("HistoryWorkspaceView.axaml", true);
        var maintenanceXaml = ReadWorkspaceOrView("MaintenanceWorkspaceView.axaml", true);
        var recordXaml = ReadWorkspaceOrView("RecordWorkspaceView.axaml", true);
        var reminderXaml = ReadWorkspaceOrView("ReminderWorkspaceView.axaml", true);
        var timelineXaml = ReadWorkspaceOrView("TimelineWorkspaceView.axaml", true);
        var globalSearchXaml = ReadWorkspaceOrView("GlobalSearchWorkspaceView.axaml", true);
        var upcomingOverviewXaml = ReadWorkspaceOrView("UpcomingOverviewWorkspaceView.axaml", true);
        var overdueOverviewXaml = ReadWorkspaceOrView("OverdueOverviewWorkspaceView.axaml", true);
        var costXaml = ReadWorkspaceOrView("CostWorkspaceView.axaml", true);
        var vehicleDetailXaml = ReadWorkspaceOrView("VehicleDetailWorkspaceView.axaml", true);

        Assert.Contains("AutomationProperties.AutomationId=\"TechnicalReminderDaysBox\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"GreenCardReminderDaysBox\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelEditorDateBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelEditorFuelTypeBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HistoryEditorDateBox\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HistoryEditorTypeBox\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceEditorTitleBox\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceTemplateComboBox\"", maintenanceXaml);
        Assert.Contains("ItemsSource=\"{Binding MaintenanceTemplateOptions}\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceEditorIntervalKmBox\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenMaintenanceTemplatesButton\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CompleteMaintenanceButton\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"RecordEditorTypeBox\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"RecordAttachmentModeComboBox\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CopyRecordPathButton\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReminderEditorTitleBox\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReminderEditorDueDateBox\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AdvanceReminderButton\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorNameBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorCategoryBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorClimateProfileBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorTimingDriveBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorTransmissionBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditOpenItemButton\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditOpenVehicleButton\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditEditItemButton\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditSearchBox\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditListBox\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardAuditOpenButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardAuditListBox\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardCostOpenButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardCostListBox\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardTimelineOpenButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardTimelineListBox\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TimelineSearchBox\"", timelineXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TimelineOpenButton\"", timelineXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"GlobalSearchTextBox\"", globalSearchXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SearchOpenButton\"", globalSearchXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewSearchBox\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewOpenButton\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewIncludeMissingGreenCardsCheckBox\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewIncludeDataIssuesCheckBox\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverdueOverviewSearchBox\"", overdueOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverdueOverviewOpenButton\"", overdueOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CostListBox\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SelectedCostVehicleDetailText\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FocusCostDetailButton\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenCostVehicleButton\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"EditCostVehicleButton\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExportFleetCostSummaryButton\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExportSelectedVehicleCostDetailButton\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExportSelectedVehicleCostReportButton\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CostExportStatusText\"", costXaml);
        Assert.Contains("AllowEditing=\"False\"", ReadViewFile("MainWindow.axaml"));
        Assert.Contains("x:Name=\"CancelVehicleButton\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DeleteVehicleButton\"", vehicleDetailXaml);
        Assert.Contains("ItemsSource=\"{Binding VehicleCategoryOptions}\"", vehicleDetailXaml);
    }

    [Fact]
    public void Overview_workspace_xaml_should_define_keyboard_first_shortcuts()
    {
        var timelineXaml = ReadWorkspaceOrView("TimelineWorkspaceView.axaml", true);
        var auditXaml = ReadWorkspaceOrView("AuditWorkspaceView.axaml", true);
        var globalSearchXaml = ReadWorkspaceOrView("GlobalSearchWorkspaceView.axaml", true);
        var upcomingOverviewXaml = ReadWorkspaceOrView("UpcomingOverviewWorkspaceView.axaml", true);
        var overdueOverviewXaml = ReadWorkspaceOrView("OverdueOverviewWorkspaceView.axaml", true);
        var historyXaml = ReadWorkspaceOrView("HistoryWorkspaceView.axaml", true);
        var fuelXaml = ReadWorkspaceOrView("FuelWorkspaceView.axaml", true);
        var reminderXaml = ReadWorkspaceOrView("ReminderWorkspaceView.axaml", true);
        var maintenanceXaml = ReadWorkspaceOrView("MaintenanceWorkspaceView.axaml", true);
        var recordXaml = ReadWorkspaceOrView("RecordWorkspaceView.axaml", true);
        var costXaml = ReadWorkspaceOrView("CostWorkspaceView.axaml", true);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", timelineXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding OpenSelectedTimelineItemCommand}\"", timelineXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", auditXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedAuditVehicleCommand}\"", auditXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding OpenSelectedAuditItemCommand}\"", auditXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedAuditItemCommand}\"", auditXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedAuditItemCommand}\"", auditXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", globalSearchXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedSearchResultCommand}\"", globalSearchXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding OpenSelectedSearchResultCommand}\"", globalSearchXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", upcomingOverviewXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedUpcomingOverviewVehicleCommand}\"", upcomingOverviewXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding OpenSelectedUpcomingOverviewItemCommand}\"", upcomingOverviewXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", overdueOverviewXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedOverdueOverviewVehicleCommand}\"", overdueOverviewXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding OpenSelectedOverdueOverviewItemCommand}\"", overdueOverviewXaml);

        Assert.Contains("Gesture=\"Ctrl+N\" Command=\"{Binding CreateHistoryCommand}\"", historyXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedHistoryCommand}\"", historyXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedHistoryCommand}\"", historyXaml);
        Assert.Contains("Gesture=\"Ctrl+S\" Command=\"{Binding SaveHistoryCommand}\"", historyXaml);

        Assert.Contains("Gesture=\"Ctrl+N\" Command=\"{Binding CreateFuelCommand}\"", fuelXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedFuelCommand}\"", fuelXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedFuelCommand}\"", fuelXaml);
        Assert.Contains("Gesture=\"Ctrl+S\" Command=\"{Binding SaveFuelCommand}\"", fuelXaml);

        Assert.Contains("Gesture=\"Ctrl+N\" Command=\"{Binding CreateReminderCommand}\"", reminderXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedReminderCommand}\"", reminderXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedReminderCommand}\"", reminderXaml);
        Assert.Contains("Gesture=\"Ctrl+Shift+N\" Command=\"{Binding AdvanceSelectedReminderCommand}\"", reminderXaml);
        Assert.Contains("Gesture=\"Ctrl+S\" Command=\"{Binding SaveReminderCommand}\"", reminderXaml);

        Assert.Contains("Gesture=\"Ctrl+N\" Command=\"{Binding CreateMaintenanceCommand}\"", maintenanceXaml);
        Assert.Contains("Gesture=\"Ctrl+Shift+N\" Command=\"{Binding OpenMaintenanceTemplatesCommand}\"", maintenanceXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedMaintenanceCommand}\"", maintenanceXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedMaintenanceCommand}\"", maintenanceXaml);
        Assert.Contains("Gesture=\"Ctrl+L\" Command=\"{Binding CompleteSelectedMaintenanceCommand}\"", maintenanceXaml);
        Assert.Contains("Gesture=\"Ctrl+S\" Command=\"{Binding SaveMaintenanceCommand}\"", maintenanceXaml);

        Assert.Contains("Gesture=\"Ctrl+N\" Command=\"{Binding CreateRecordCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedRecordCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedRecordCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"Ctrl+S\" Command=\"{Binding SaveRecordCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedRecordFileCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"Ctrl+Shift+O\" Command=\"{Binding OpenSelectedRecordFolderCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"Ctrl+Shift+C\" Command=\"{Binding CopySelectedRecordPathCommand}\"", recordXaml);

        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding FocusSelectedCostDetailCommand}\"", costXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedCostVehicleCommand}\"", costXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedCostVehicleCommand}\"", costXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedCostVehicleCommand}\"", costXaml);
    }

    [Fact]
    public void Desktop_ui_sources_should_not_contain_common_mojibake_markers()
    {
        var repositoryRoot = FindRepositoryRoot();
        var scannedRoots = new[]
        {
            Path.Combine(repositoryRoot, "dotnet", "src", "Vehimap.Desktop"),
            Path.Combine(repositoryRoot, "dotnet", "tests", "Vehimap.Tests.UI")
        };
        var scannedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".axaml",
            ".cs"
        };
        var suspiciousCharacters = new[]
        {
            '\u00c2',
            '\u00c3',
            '\u00c4',
            '\u00c5',
            '\u00e2',
            '\u0102',
            '\u0139',
            '\ufffd'
        };
        var failures = new List<string>();

        foreach (var root in scannedRoots)
        {
            foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                         .Where(path => scannedExtensions.Contains(Path.GetExtension(path))))
            {
                var content = File.ReadAllText(file);
                var badIndex = content.IndexOfAny(suspiciousCharacters);
                if (badIndex < 0)
                {
                    continue;
                }

                var badCharacter = content[badIndex];
                failures.Add($"{Path.GetRelativePath(repositoryRoot, file)} obsahuje podezřelý znak U+{(int)badCharacter:X4}.");
            }
        }

        Assert.True(failures.Count == 0, "UI zdroje obsahují znaky typické pro rozbitou UTF-8 diakritiku:" + Environment.NewLine + string.Join(Environment.NewLine, failures));
    }

    private static string ReadViewFile(string fileName)
    {
        var desktopRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "Vehimap.Desktop",
            "Views"));

        return File.ReadAllText(Path.Combine(desktopRoot, fileName));
    }

    private static string ReadWindowRootElement(string fileName)
    {
        var xaml = ReadViewFile(fileName);
        var rootEnd = xaml.IndexOf(">\r\n", StringComparison.Ordinal);
        if (rootEnd < 0)
        {
            rootEnd = xaml.IndexOf(">\n", StringComparison.Ordinal);
        }

        Assert.True(rootEnd > 0, $"Soubor {fileName} nemá čitelný kořenový Window element.");
        return xaml[..rootEnd];
    }

    private static string ReadWorkspaceOrView(string fileName, bool workspace)
    {
        if (!workspace)
        {
            return ReadViewFile(fileName);
        }

        var desktopRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "Vehimap.Desktop",
            "Views",
            "Workspaces"));

        return File.ReadAllText(Path.Combine(desktopRoot, fileName));
    }

    private static string ReadViewCodeBehind(string fileName)
    {
        var desktopRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "Vehimap.Desktop",
            "Views"));

        return File.ReadAllText(Path.Combine(desktopRoot, fileName));
    }

    private static string ReadDesktopServiceFile(string fileName)
    {
        var servicesRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "Vehimap.Desktop",
            "Services"));

        return File.ReadAllText(Path.Combine(servicesRoot, fileName));
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "src", "VERSION"))
                && Directory.Exists(Path.Combine(current.FullName, "dotnet")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Nepodařilo se najít kořen repozitáře Vehimapu.");
    }
}
