using Vehimap.Desktop.ViewModels;
using Vehimap.Application.Models;
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
    public void Smart_advisor_items_should_expose_human_readable_labels()
    {
        var item = new SmartAdvisorItemViewModel(
            "advisor_1",
            "Naléhavé",
            "Termíny",
            "Milena",
            "veh_1",
            "Vozidlo",
            string.Empty,
            "Technická kontrola: Příští TK",
            "Po termínu. 05/2026.",
            "SPZ 1A2 3456",
            "Otevřít vozidlo",
            "31.05.2026",
            3);

        Assert.Equal(item.AccessibleLabel, item.ToString());
        Assert.Contains("Naléhavé", item.AccessibleLabel);
        Assert.Contains("Milena", item.AccessibleLabel);
        Assert.Contains("Otevřít vozidlo", item.AccessibleLabel);
        Assert.DoesNotContain(nameof(SmartAdvisorItemViewModel), item.AccessibleLabel);
    }

    [Fact]
    public void Vehicle_detail_evidence_summary_items_should_expose_human_readable_labels()
    {
        var item = new VehicleDetailEvidenceSummaryItemViewModel(
            "Doklady",
            "Záznamů: 2. Bez vyplněné cesty: 1.");

        Assert.Equal(item.AccessibleLabel, item.ToString());
        Assert.Equal("Doklady: Záznamů: 2. Bez vyplněné cesty: 1.", item.AccessibleLabel);
        Assert.DoesNotContain(nameof(VehicleDetailEvidenceSummaryItemViewModel), item.AccessibleLabel);
    }

    [Fact]
    public void Vehicle_starter_bundle_items_should_expose_human_readable_labels()
    {
        var item = new VehicleStarterBundleItemEditorViewModel(
            new VehicleStarterBundleTemplate(
                VehicleStarterBundleSection.Record,
                "Doklad",
                "Povinné ručení",
                string.Empty,
                string.Empty,
                "Povinné ručení",
                "Kooperativa",
                "03/2026",
                "03/2027",
                "2 000 Kč",
                string.Empty,
                string.Empty,
                string.Empty,
                "Roční smlouva"));

        var accessibleLabelChanged = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(VehicleStarterBundleItemEditorViewModel.AccessibleLabel))
            {
                accessibleLabelChanged = true;
            }
        };

        Assert.Equal(item.AccessibleLabel, item.ToString());
        Assert.Contains("Doklad", item.AccessibleLabel);
        Assert.Contains("Povinné ručení", item.AccessibleLabel);
        Assert.DoesNotContain(nameof(VehicleStarterBundleItemEditorViewModel), item.AccessibleLabel);

        item.Title = "Havarijní pojištění";

        Assert.True(accessibleLabelChanged);
        Assert.Equal("Doklad: Havarijní pojištění", item.AccessibleLabel);
        Assert.Equal(item.AccessibleLabel, item.ToString());
    }

    [Fact]
    public void Vehicle_starter_bundle_items_should_normalize_dropdown_values()
    {
        var recordItem = new VehicleStarterBundleItemEditorViewModel(
            new VehicleStarterBundleTemplate(
                VehicleStarterBundleSection.Record,
                "Doklad",
                "Starý doklad",
                string.Empty,
                string.Empty,
                "Vlastní typ",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty));
        var reminderItem = new VehicleStarterBundleItemEditorViewModel(
            new VehicleStarterBundleTemplate(
                VehicleStarterBundleSection.Reminder,
                "Připomínka",
                "Kontrola",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                "10.10.2026",
                "30",
                "Ročně",
                string.Empty));

        Assert.Equal("Povinné ručení", recordItem.RecordType);
        Assert.Equal("Povinné ručení", recordItem.ToTemplate().RecordType);
        Assert.Equal("Každý rok", reminderItem.RepeatMode);
        Assert.Equal("Každý rok", reminderItem.ToTemplate().RepeatMode);
    }

    [Fact]
    public void Vehicle_starter_bundle_dialog_should_expose_dropdown_options()
    {
        var viewModel = new VehicleStarterBundleDialogViewModel(
            new VehicleStarterBundlePreview(
                "veh_1",
                "Milena",
                "Osobní vozidla",
                []));

        Assert.Contains("Povinné ručení", viewModel.RecordTypeOptions);
        Assert.Contains("Doklad", viewModel.RecordTypeOptions);
        Assert.Contains("Neopakovat", viewModel.ReminderRepeatModeOptions);
        Assert.Contains("Každý rok", viewModel.ReminderRepeatModeOptions);
    }

    [Fact]
    public void Tray_actions_dialog_defaults_should_expose_overview_actions()
    {
        var model = TrayActionsDialogViewModel.CreateDefault();

        Assert.Equal("Stav pozadí zatím není dostupný.", model.BackgroundStatus);
        Assert.Equal("Otevřít aktuální upozornění", model.OpenBackgroundStatusLabel);
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
        Assert.Equal("Zálohovat ihned", model.CreateAutomaticBackupNowLabel);
        Assert.Equal("Otevřít složku automatických záloh", model.OpenAutomaticBackupFolderLabel);
        Assert.Equal("Nastavení", model.OpenSettingsLabel);
        Assert.Equal("Export termínů do kalendáře", model.ExportCalendarLabel);
        Assert.Equal("Načíst data znovu", model.ReloadDataLabel);
        Assert.Equal("Otevřít datovou složku", model.OpenDataFolderLabel);
        Assert.Equal("O programu", model.OpenAboutLabel);
        Assert.Equal("Poděkovat autorovi", model.ThankAuthorLabel);
        Assert.Equal("Zkontrolovat aktualizace", model.CheckForUpdatesLabel);
        Assert.Equal("Ukončit aplikaci", model.ExitLabel);
        Assert.True(model.CanThankAuthor);
    }

    [Fact]
    public void Main_window_xaml_should_define_accessible_shell_metadata()
    {
        var xaml = ReadViewFile("MainWindow.axaml");
        var normalizedXaml = Regex.Replace(xaml, "\\s+", " ");

        Assert.Contains("AutomationProperties.Name=\"Vehimap\"", xaml);
        Assert.Contains("WindowState=\"Maximized\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AppMenuBar\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FileMenuRoot\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleMenuRoot\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverviewMenuRoot\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"QuickActionsMenuRoot\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AppMenuRoot\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DataModeText\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DataPathText\"", xaml);
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
        Assert.Contains("x:Name=\"OpenTrayActionsButton\" Header=\"Akce na liště\" Click=\"OnOpenTrayActionsClick\"", normalizedXaml);
        Assert.Contains("x:Name=\"MinimizeToTrayButton\" Header=\"Minimalizovat na lištu\" Click=\"OnMinimizeToTrayClick\" IsEnabled=\"{Binding IsMinimizeToTrayAvailable}\"", normalizedXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"PrintableReportButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CreateAutomaticBackupNowMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenAutomaticBackupFolderMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDataFolderMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CheckDataStoreHealthMenuItem\"", xaml);
        Assert.Contains("Click=\"OnCheckDataStoreHealthClick\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanCheckDataStoreHealth}\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Zkontrolovat datovou sadu 2.0\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenPreMigrationBackupFolderMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Otevřít složku předmigrační zálohy\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenPreMigrationBackupFolder}\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FileExitAppButton\"", xaml);
        Assert.Contains("x:Name=\"FileExitAppButton\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Ukončit aplikaci z nabídky Soubor\"", xaml);
        Assert.Contains("Click=\"OnOpenDataFolderClick\"", xaml);
        Assert.Contains("Click=\"OnCreateAutomaticBackupNowClick\"", xaml);
        Assert.Contains("Click=\"OnOpenAutomaticBackupFolderClick\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanUseDataActions}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanCreateAutomaticBackupNow}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenAutomaticBackupFolder}\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MinimizeToTrayButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SettingsButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AboutButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ThankAuthorButton\"", xaml);
        Assert.Contains("Click=\"OnThankAuthorClick\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FeedbackIssueButton\"", xaml);
        Assert.Contains("Click=\"OnFeedbackIssueClick\"", xaml);
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
        Assert.Contains("AutomationProperties.AutomationId=\"OpenVehicleTimelineMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenServiceBookMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Otevřít servisní knížku vybraného vozidla\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenVehicleStarterBundleMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExportVehiclePackageMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Exportovat vybrané vozidlo do balíčku\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanExportSelectedVehiclePackage}\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ImportVehiclePackageMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Importovat vozidlo z balíčku\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanImportVehiclePackage}\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenSelectedVehicleCostsMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenTimelineMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenGlobalSearchMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenAuditMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDashboardMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenSmartAdvisorMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenCostMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenUpcomingOverviewMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenOverdueOverviewMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverviewCalendarExportMenuItem\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenBackgroundNotificationMenuItem\"", xaml);
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
        Assert.Contains("IsEnabled=\"{Binding CanOpenBackgroundNotificationQuickAction}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenNearestTechnicalQuickAction}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanReviewTechnicalQuickAction}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenNearestGreenCardQuickAction}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanReviewGreenCardsQuickAction}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenNearestReminderQuickAction}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanReviewRemindersQuickAction}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenNearestMaintenanceQuickAction}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanReviewMaintenanceQuickAction}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenNearestRecordQuickAction}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanReviewRecordsQuickAction}\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CalendarExportButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReloadButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenTrayActionsButton\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+E\"", xaml);
        Assert.Contains("InputGesture=\"F5\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+N\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+U\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+O\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+H\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+K\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+P\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+R\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+M\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+Shift+F\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+D\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+T\"", xaml);
        Assert.Contains("InputGesture=\"Ctrl+Shift+T\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleCategoryFilterBox\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleSearchBox\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleStatusFilterBox\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearVehicleFiltersButton\"", xaml);
        Assert.Contains("TextWrapping=\"Wrap\" AutomationProperties.Name=\"{Binding VehicleListSummary}\"", normalizedXaml);
        Assert.Contains("ColumnDefinitions=\"*,*\" RowDefinitions=\"Auto,Auto,Auto,Auto\"", normalizedXaml);
        Assert.Contains("Grid.Row=\"1\" Grid.ColumnSpan=\"2\" Margin=\"0,12,0,0\"", normalizedXaml);
        Assert.DoesNotContain("ColumnDefinitions=\"170,*,220,Auto\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HideInactiveVehiclesCheckBox\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleListBox\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleListLockStatusText\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"WorkspaceNavigationLockStatusText\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanUseVehicleList}\"", xaml);
        Assert.Contains("IsEnabled=\"{Binding CanUseWorkspaceNavigation}\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenHistoryWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenTimelineWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenCostWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDashboardWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenGlobalSearchWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenUpcomingOverviewWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenOverdueOverviewWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SmartAdvisorTabButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenSmartAdvisorWindowButton\"", xaml);
        Assert.Contains("<workspaces:SmartAdvisorWorkspaceView Grid.Row=\"1\" Margin=\"0,16,0,0\"", normalizedXaml);
        Assert.Contains("<workspaces:HistoryWorkspaceView Grid.Row=\"1\" Margin=\"0,16,0,0\" AllowEditing=\"True\"", normalizedXaml);
        Assert.Contains("<workspaces:FuelWorkspaceView Grid.Row=\"1\" Margin=\"0,16,0,0\" AllowEditing=\"True\"", normalizedXaml);
        Assert.Contains("AllowEditing=\"False\"", xaml);
        Assert.Contains("Click=\"OnSettingsClick\"", xaml);
        Assert.Contains("Click=\"OnAboutClick\"", xaml);
        Assert.Contains("Click=\"OnUpdateCheckClick\"", xaml);
        Assert.Contains("Click=\"OnOpenTrayActionsClick\"", xaml);
        Assert.Contains("Click=\"OnMinimizeToTrayClick\"", xaml);
        Assert.Contains("Click=\"OnExitClick\"", xaml);
        Assert.Contains("x:Name=\"FileExitAppButton\" Header=\"Konec\" Click=\"OnExitClick\"", normalizedXaml);
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
        Assert.Contains("Click=\"OnOpenBackgroundNotificationMenuClick\"", xaml);
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
        Assert.Contains("OpenCurrentWorkspaceCreateWindowAsync", codeBehind);
        Assert.Contains("OpenCurrentWorkspaceEditWindowAsync", codeBehind);
        Assert.Contains("OpenActiveEditorWindowAsync", codeBehind);
        Assert.Contains("HandleCurrentWorkspaceEditShortcutAsync", codeBehind);
        Assert.Contains("HandleCurrentWorkspaceSaveShortcutAsync", codeBehind);
        Assert.Contains("KeyboardAccessibilityHelper.ShouldSkipGlobalShortcut(e)", codeBehind);
        Assert.Contains("ToggleMainMenuFocus(e.Source)", codeBehind);
        Assert.Contains("FocusMainMenuRoot()", codeBehind);
        Assert.Contains("CloseMainMenuAndRestoreFocus()", codeBehind);
        Assert.Contains("TryFocusInitialVehicleList()", codeBehind);
        Assert.Contains("InputElement.GotFocusEvent", codeBehind);
        Assert.Contains("OnElementGotFocus", codeBehind);
        Assert.Contains("_lastNonMenuFocusTarget", codeBehind);
        Assert.DoesNotContain("FocusAndOpenMainMenu", codeBehind);
        Assert.DoesNotContain("IsSubMenuOpen = true", codeBehind);
        Assert.Contains("case Key.N", codeBehind);
        Assert.Contains("case Key.S", codeBehind);
        Assert.Contains("Key.F2", codeBehind);
        Assert.Contains("case Key.R", codeBehind);
        Assert.Contains("case Key.M", codeBehind);
    }

    [Fact]
    public void Vehicle_detail_workspace_should_define_accessible_related_actions()
    {
        var xaml = ReadWorkspaceOrView("VehicleDetailWorkspaceView.axaml", true);

        Assert.Contains("AutomationProperties.AutomationId=\"VehicleDetailRelatedActionsPanel\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleDetailRelatedActionsHelpText\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDetailHistoryButton\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Otevřít historii vybraného vozidla\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDetailFuelButton\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Otevřít tankování vybraného vozidla\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDetailRemindersButton\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Otevřít připomínky vybraného vozidla\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDetailMaintenanceButton\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Otevřít plán údržby vybraného vozidla\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDetailRecordsButton\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Otevřít doklady a přílohy vybraného vozidla\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDetailTimelineButton\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Otevřít časovou osu vybraného vozidla\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDetailServiceBookButton\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Otevřít servisní knížku vybraného vozidla\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDetailCostsButton\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Otevřít náklady vybraného vozidla\"", xaml);
        Assert.Equal(8, Regex.Matches(xaml, "IsEnabled=\"\\{Binding CanOpenVehicleRelatedWorkspace\\}\"").Count);
    }

    [Fact]
    public void Dialog_xaml_files_should_define_expected_automation_ids()
    {
        var settingsXaml = ReadViewFile("SettingsWindow.axaml");
        var settingsCodeBehind = ReadViewCodeBehind("SettingsWindow.axaml.cs");
        var dataStoreHealthXaml = ReadViewFile("DataStoreHealthWindow.axaml");
        var dataStoreHealthCodeBehind = ReadViewCodeBehind("DataStoreHealthWindow.axaml.cs");
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
        var smartAdvisorXaml = ReadViewFile("SmartAdvisorWindow.axaml");
        var bundleXaml = ReadViewFile("VehicleStarterBundleWindow.axaml");
        var bundleCodeBehind = ReadViewCodeBehind("VehicleStarterBundleWindow.axaml.cs");
        var confirmationXaml = ReadViewFile("ConfirmationWindow.axaml");
        var confirmationCodeBehind = ReadViewCodeBehind("ConfirmationWindow.axaml.cs");
        var trayActionsXaml = ReadViewFile("TrayActionsWindow.axaml");
        var trayActionsCodeBehind = ReadViewCodeBehind("TrayActionsWindow.axaml.cs");
        var serviceBookXaml = ReadViewFile("ServiceBookWindow.axaml");
        var serviceBookCodeBehind = ReadViewCodeBehind("ServiceBookWindow.axaml.cs");

        Assert.Contains("CanResize=\"True\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SettingsContentScrollViewer\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TechnicalReminderDaysBox\"", settingsXaml);
        Assert.Contains("Ctrl+S uloží nastavení, Ctrl+B vytvoří zálohu ihned a Escape dialog zavře bez uložení.", settingsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanConfigureAutomaticBackups}\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CreateAutomaticBackupButton\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SaveSettingsButton\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CancelSettingsButton\"", settingsXaml);
        Assert.Contains("Key.Escape", settingsCodeBehind);
        Assert.Contains("case Key.S", settingsCodeBehind);
        Assert.Contains("case Key.B", settingsCodeBehind);
        Assert.Contains("CanResize=\"True\"", dataStoreHealthXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DataStoreHealthWindow\"", dataStoreHealthXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DataStoreHealthDetailsScrollViewer\"", dataStoreHealthXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CopyDataStoreHealthDetailsButton\"", dataStoreHealthXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenHealthDataFolderButton\"", dataStoreHealthXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenHealthPreMigrationBackupButton\"", dataStoreHealthXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseDataStoreHealthButton\"", dataStoreHealthXaml);
        Assert.Contains("Ctrl+Shift+C zkopíruje diagnostiku a Escape dialog zavře.", dataStoreHealthXaml);
        Assert.Contains("Key.Escape", dataStoreHealthCodeBehind);
        Assert.Contains("Key.C", dataStoreHealthCodeBehind);
        Assert.Contains("KeyModifiers.Control | KeyModifiers.Shift", dataStoreHealthCodeBehind);
        Assert.Contains("CanResize=\"True\"", aboutXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AboutSummaryPanel\"", aboutXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AboutAuthorText\"", aboutXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AboutVersionText\"", aboutXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ToggleAboutDiagnosticsButton\"", aboutXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AboutDetailsScrollViewer\"", aboutXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AboutDiagnosticsTextBox\"", aboutXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReleaseNotesButton\"", aboutXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ThankAuthorButton\"", aboutXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AboutStatusText\"", aboutXaml);
        Assert.Contains("Ctrl+O otevře release poznámky, Ctrl+K otevře poděkování autorovi, Ctrl+Shift+C zkopíruje diagnostická data a Escape dialog zavře.", aboutXaml);
        Assert.Contains("Key.Escape", aboutCodeBehind);
        Assert.Contains("Key.O", aboutCodeBehind);
        Assert.Contains("Key.K", aboutCodeBehind);
        Assert.Contains("Key.C", aboutCodeBehind);
        Assert.Contains("KeyModifiers.Control | KeyModifiers.Shift", aboutCodeBehind);
        Assert.Contains("AutomationProperties.AutomationId=\"CopyAboutDetailsButton\"", aboutXaml);
        Assert.Contains("AutomationProperties.Name=\"Kopírovat diagnostická data o aplikaci do schránky\"", aboutXaml);
        Assert.Contains("CanResize=\"True\"", updateXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpdateDetailsScrollViewer\"", updateXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpdateCloseButton\"", updateXaml);
        Assert.Contains("AutomationProperties.Name=\"Kontrola aktualizací\"", updateXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpdateCheckWindow\"", updateXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpdateStatusText\"", updateXaml);
        Assert.Contains("Ctrl+Shift+C zkopíruje detaily", updateXaml);
        Assert.Contains("Escape dialog zavře.", updateXaml);
        Assert.Contains("Key.Escape", updateCodeBehind);
        Assert.Contains("Key.C", updateCodeBehind);
        Assert.Contains("KeyModifiers.Control | KeyModifiers.Shift", updateCodeBehind);
        Assert.Contains("AutomationProperties.Name=\"{Binding PrimaryActionLabel}\"", updateXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CopyUpdateDetailsButton\"", updateXaml);
        Assert.Contains("AutomationProperties.Name=\"Kopírovat detaily kontroly aktualizací do schránky\"", updateXaml);
        Assert.Contains("AutomationProperties.Name=\"Zavřít kontrolu aktualizací\"", updateXaml);
        Assert.Contains("AutomationProperties.Name=\"Upozornění Vehimapu\"", notificationXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"NotificationWindow\"", notificationXaml);
        Assert.Contains("Zavře se samo; při aktivaci ho lze zavřít klávesou Escape.", notificationXaml);
        Assert.Contains("Key.Escape", notificationCodeBehind);
        Assert.Contains("CanResize=\"True\"", maintenanceCompletionXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceCompletionWindow\"", maintenanceCompletionXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceCompletionFieldsScrollViewer\"", maintenanceCompletionXaml);
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
        Assert.Contains("AutomationProperties.AutomationId=\"ServiceBookWindow\"", serviceBookXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ServiceBookVehicleSummaryText\"", serviceBookXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ServiceBookCostSummaryText\"", serviceBookXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ServiceBookStatusText\"", serviceBookXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ServiceBookHistoryListBox\"", serviceBookXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ServiceBookMaintenanceListBox\"", serviceBookXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ServiceBookRecordListBox\"", serviceBookXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenServiceBookItemButton\"", serviceBookXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExportServiceBookHtmlButton\"", serviceBookXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseServiceBookWindowButton\"", serviceBookXaml);
        Assert.Contains("Ctrl+O otevře vybranou položku, Ctrl+S exportuje HTML a Escape okno zavře.", serviceBookXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedServiceBookItemCommand}\"", serviceBookXaml);
        Assert.Contains("Gesture=\"Ctrl+S\" Command=\"{Binding ExportHtmlCommand}\"", serviceBookXaml);
        Assert.Contains("Gesture=\"Enter\" Command=\"{Binding OpenSelectedServiceBookItemCommand}\"", serviceBookXaml);

        Assert.Contains("AutomationProperties.AutomationId=\"SmartAdvisorWindow\"", smartAdvisorXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseSmartAdvisorWindowButton\"", smartAdvisorXaml);
        Assert.Contains("SmartAdvisorWorkspaceHost", smartAdvisorXaml);
        Assert.Contains("Key.Escape", serviceBookCodeBehind);
        Assert.Contains("FocusFirstLogicalControl", serviceBookCodeBehind);
        Assert.Contains("CanResize=\"True\"", bundleXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"BundleItemsListBox\"", bundleXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"BundleSummaryText\"", bundleXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"BundleDetailScrollViewer\"", bundleXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"BundleDetailHintText\"", bundleXaml);
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleLabel}\"", bundleXaml);
        Assert.Contains("Mezerník přepne, zda se položka přidá", bundleXaml);
        Assert.Contains("ItemsSource=\"{Binding RecordTypeOptions}\"", bundleXaml);
        Assert.Contains("SelectedItem=\"{Binding SelectedItem.RecordType}\"", bundleXaml);
        Assert.Contains("ItemsSource=\"{Binding ReminderRepeatModeOptions}\"", bundleXaml);
        Assert.Contains("SelectedItem=\"{Binding SelectedItem.RepeatMode}\"", bundleXaml);
        Assert.Contains("AutomationProperties.Name=\"Vybrat všechny položky balíčku\"", bundleXaml);
        Assert.Contains("AutomationProperties.Name=\"Zrušit výběr položek balíčku\"", bundleXaml);
        Assert.Contains("AutomationProperties.Name=\"Přidat vybrané položky balíčku\"", bundleXaml);
        Assert.Contains("AutomationProperties.Name=\"Zavřít balíček bez přidání\"", bundleXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ApplyBundleButton\"", bundleXaml);
        Assert.Contains("Key.Escape", bundleCodeBehind);
        Assert.Contains("Key.S", bundleCodeBehind);
        Assert.Contains("Key.A", bundleCodeBehind);
        Assert.Contains("Key.Space", bundleCodeBehind);
        Assert.Contains("KeyboardAccessibilityHelper.ShouldSkipGlobalShortcut(e)", bundleCodeBehind);
        Assert.Contains("CanResize=\"True\"", confirmationXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ConfirmationMessageScrollViewer\"", confirmationXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ConfirmationConfirmButton\"", confirmationXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ConfirmationCancelButton\"", confirmationXaml);
        Assert.Contains("Escape akci zruší.", confirmationXaml);
        Assert.Contains("Key.Escape", confirmationCodeBehind);
        Assert.Contains("CanResize=\"True\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TrayActionsBackgroundStatusText\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenBackgroundStatusTrayActionButton\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenBackgroundStatus}\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TrayActionsScrollViewer\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ShowMainWindowTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ShowDashboardTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ShowUpcomingOverviewTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ShowOverdueOverviewTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestTechnicalTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestGreenCardTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestReminderTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestMaintenanceTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenNearestRecordTrayActionButton\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenNearestTechnical}\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenNearestGreenCard}\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenNearestReminder}\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenNearestMaintenance}\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenNearestRecord}\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewTechnicalTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewGreenCardsTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewRemindersTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewMaintenanceTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReviewRecordsTrayActionButton\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanReviewTechnical}\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanReviewGreenCards}\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanReviewReminders}\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanReviewMaintenance}\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanReviewRecords}\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenPrintableReportTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExportBackupTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ImportBackupTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CreateAutomaticBackupNowTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenAutomaticBackupFolderTrayActionButton\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanCreateAutomaticBackupNow}\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanOpenAutomaticBackupFolder}\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenSettingsTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExportCalendarTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReloadDataTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDataFolderTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenAboutTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ThankAuthorTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FeedbackIssueTrayActionButton\"", trayActionsXaml);
        Assert.Contains("IsEnabled=\"{Binding CanReportFeedback}\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CheckForUpdatesTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ExitTrayActionButton\"", trayActionsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseTrayActionsButton\"", trayActionsXaml);
        Assert.Contains("Escape okno zavře bez akce.", trayActionsXaml);
        Assert.Contains("OnShowUpcomingOverviewClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenBackgroundStatusClick", trayActionsCodeBehind);
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
        Assert.Contains("OnCreateAutomaticBackupNowClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenAutomaticBackupFolderClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenSettingsClick", trayActionsCodeBehind);
        Assert.Contains("OnExportCalendarClick", trayActionsCodeBehind);
        Assert.Contains("OnReloadDataClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenDataFolderClick", trayActionsCodeBehind);
        Assert.Contains("OnOpenAboutClick", trayActionsCodeBehind);
        Assert.Contains("OnThankAuthorClick", trayActionsCodeBehind);
        Assert.Contains("OnFeedbackIssueClick", trayActionsCodeBehind);
        Assert.Contains("OnCheckForUpdatesClick", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ShowUpcomingOverview", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenBackgroundStatus", trayActionsCodeBehind);
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
        Assert.Contains("TrayActionsDialogAction.CreateAutomaticBackupNow", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenAutomaticBackupFolder", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenSettings", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ExportCalendar", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ReloadData", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenDataFolder", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.OpenAbout", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ThankAuthor", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.ReportFeedback", trayActionsCodeBehind);
        Assert.Contains("TrayActionsDialogAction.CheckForUpdates", trayActionsCodeBehind);
        Assert.Contains("Key.Escape", trayActionsCodeBehind);
    }

    [Fact]
    public void Tray_runtime_controller_should_route_quick_actions_to_shell_commands()
    {
        var runtimeController = ReadDesktopServiceFile("DesktopAppRuntimeController.cs");
        var mainWindowCodeBehind = ReadViewCodeBehind("MainWindow.axaml.cs");
        var appStartup = ReadDesktopRootFile("App.axaml.cs");

        Assert.Contains("public Task RequestOpenTrayActionsAsync() => OpenTrayActionsAsync();", runtimeController);
        Assert.Contains("public Func<Task>? OpenTrayActionsRequested { get; set; }", mainWindowCodeBehind);
        Assert.Contains("await OpenTrayActionsRequested().ConfigureAwait(true);", mainWindowCodeBehind);
        Assert.Contains("mainWindow.OpenTrayActionsRequested = _runtimeController.RequestOpenTrayActionsAsync;", appStartup);
        Assert.Contains("TrayActionsDialogAction.OpenBackgroundStatus", runtimeController);
        Assert.Contains("_shell.OpenBackgroundNotificationAsync()", runtimeController);
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
        Assert.Contains("TrayActionsDialogAction.CreateAutomaticBackupNow", runtimeController);
        Assert.Contains("_shell.CreateAutomaticBackupNowAsync()", runtimeController);
        Assert.Contains("TrayActionsDialogAction.OpenAutomaticBackupFolder", runtimeController);
        Assert.Contains("_shell.OpenAutomaticBackupFolderAsync()", runtimeController);
        Assert.Contains("TrayActionsDialogAction.OpenSettings", runtimeController);
        Assert.Contains("_shell.AppShellController.OpenSettingsAsync(_mainWindow, _shell)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ExportCalendar", runtimeController);
        Assert.Contains("_shell.ExportCalendarCommand.ExecuteAsync(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ReloadData", runtimeController);
        Assert.Contains("_shell.ReloadCommand.Execute(null)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.OpenDataFolder", runtimeController);
        Assert.Contains("_shell.OpenDataFolderAsync()", runtimeController);
        Assert.Contains("TrayActionsDialogAction.OpenAbout", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ThankAuthor", runtimeController);
        Assert.Contains("TrayActionsDialogAction.ReportFeedback", runtimeController);
        Assert.Contains("_shell.AppShellController.OpenAboutAsync(_mainWindow, _shell)", runtimeController);
        Assert.Contains("_shell.AppShellController.OpenAuthorSupportAsync(_shell)", runtimeController);
        Assert.Contains("_shell.AppShellController.OpenFeedbackIssueAsync(_shell)", runtimeController);
        Assert.Contains("TrayActionsDialogAction.CheckForUpdates", runtimeController);
        Assert.Contains("_shell.AppShellController.CheckForUpdatesAsync(_mainWindow, _shell)", runtimeController);
    }

    [Fact]
    public void Runtime_controller_should_refresh_tray_state_after_shell_data_changes_without_extra_reload()
    {
        var runtimeController = ReadDesktopServiceFile("DesktopAppRuntimeController.cs");

        Assert.Contains("_shell.BackgroundRefreshRequested += OnShellBackgroundRefreshRequested", runtimeController);
        Assert.Contains("_shell.BackgroundRefreshRequested -= OnShellBackgroundRefreshRequested", runtimeController);
        Assert.Contains("reloadData: false", runtimeController);
        Assert.Contains("if (reloadData && DesktopBackgroundRuntimePolicy.CanReloadInBackground(hasPendingEdits))", runtimeController);
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
            "SmartAdvisorWindow.axaml",
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
        var smartAdvisorXaml = ReadWorkspaceOrView("SmartAdvisorWorkspaceView.axaml", true);
        var vehicleDetailXaml = ReadWorkspaceOrView("VehicleDetailWorkspaceView.axaml", true);

        Assert.Contains("AutomationProperties.AutomationId=\"TechnicalReminderDaysBox\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"GreenCardReminderDaysBox\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HistorySearchBox\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearHistorySearchButton\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HistorySortComboBox\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HistorySortDescendingCheckBox\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HistoryListBox\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelSearchBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearFuelSearchButton\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelSortComboBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelSortDescendingCheckBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelListBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReminderSearchBox\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearReminderSearchButton\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReminderSortComboBox\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReminderSortDescendingCheckBox\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReminderListBox\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceSearchBox\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearMaintenanceSearchButton\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceSortComboBox\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceSortDescendingCheckBox\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceListBox\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"RecordSearchBox\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearRecordSearchButton\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"RecordSortComboBox\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"RecordSortDescendingCheckBox\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"RecordListBox\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SmartAdvisorSearchBox\"", smartAdvisorXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SmartAdvisorPriorityFilterBox\"", smartAdvisorXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SmartAdvisorCategoryFilterBox\"", smartAdvisorXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SmartAdvisorVehicleFilterBox\"", smartAdvisorXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SmartAdvisorListBox\"", smartAdvisorXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SmartAdvisorOpenItemButton\"", smartAdvisorXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SmartAdvisorOpenVehicleButton\"", smartAdvisorXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HistoryEditorScrollViewer\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelEditorScrollViewer\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReminderEditorScrollViewer\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceEditorScrollViewer\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"RecordEditorScrollViewer\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelEditorDateBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelEditorFuelTypeBox\"", fuelXaml);
        Assert.Contains("<ComboBox x:Name=\"FuelEditorFuelTypeBox\"", fuelXaml);
        Assert.Contains("ItemsSource=\"{Binding FuelTypeOptions}\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelEditorFuelDetailBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.Name=\"Detail paliva\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelEditorStationBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.Name=\"Místo tankování\"", fuelXaml);
        Assert.Contains("Grid.Column=\"1\" Spacing=\"6\"", fuelXaml);
        Assert.Contains("x:Name=\"FuelEditorOdometerBox\"", fuelXaml);
        Assert.Contains("x:Name=\"FuelEditorLitersBox\"", fuelXaml);
        Assert.Contains("x:Name=\"FuelEditorTotalCostBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HistoryEditorDateBox\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HistoryEditorTypeBox\"", historyXaml);
        Assert.Contains("Grid.Column=\"1\" Spacing=\"6\"", historyXaml);
        Assert.Contains("x:Name=\"HistoryEditorTypeBox\"", historyXaml);
        Assert.Contains("x:Name=\"HistoryEditorOdometerBox\"", historyXaml);
        Assert.Contains("x:Name=\"HistoryEditorCostBox\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceEditorTitleBox\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceTemplateComboBox\"", maintenanceXaml);
        Assert.Contains("ItemsSource=\"{Binding MaintenanceTemplateOptions}\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceEditorIntervalKmBox\"", maintenanceXaml);
        Assert.Contains("Grid.Column=\"1\" Spacing=\"6\"", maintenanceXaml);
        Assert.Contains("x:Name=\"MaintenanceEditorIntervalKmBox\"", maintenanceXaml);
        Assert.Contains("x:Name=\"MaintenanceEditorIntervalMonthsBox\"", maintenanceXaml);
        Assert.Contains("x:Name=\"MaintenanceEditorLastServiceDateBox\"", maintenanceXaml);
        Assert.Contains("x:Name=\"MaintenanceEditorLastServiceOdometerBox\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenMaintenanceTemplatesButton\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CompleteMaintenanceButton\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"RecordEditorTypeBox\"", recordXaml);
        Assert.Contains("x:Name=\"RecordEditorTypeBox\"", recordXaml);
        Assert.Contains("<ComboBox x:Name=\"RecordEditorTypeBox\"", recordXaml);
        Assert.Contains("ItemsSource=\"{Binding RecordTypeOptions}\"", recordXaml);
        Assert.Contains("x:Name=\"RecordEditorValidFromBox\"", recordXaml);
        Assert.Contains("x:Name=\"RecordEditorValidToBox\"", recordXaml);
        Assert.Contains("x:Name=\"RecordEditorPriceBox\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"RecordAttachmentModeComboBox\"", recordXaml);
        Assert.Contains("Grid.Column=\"1\" Spacing=\"6\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CopyRecordPathButton\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReminderEditorTitleBox\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReminderEditorDueDateBox\"", reminderXaml);
        Assert.Contains("x:Name=\"ReminderEditorDueDateBox\"", reminderXaml);
        Assert.Contains("x:Name=\"ReminderEditorDaysBox\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReminderEditorRepeatModeBox\"", reminderXaml);
        Assert.Contains("<ComboBox x:Name=\"ReminderEditorRepeatModeBox\"", reminderXaml);
        Assert.Contains("ItemsSource=\"{Binding ReminderRepeatModeOptions}\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AdvanceReminderButton\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorNameBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorCategoryBox\"", vehicleDetailXaml);
        Assert.Contains("ItemsSource=\"{Binding VehicleCategoryOptions}\"", vehicleDetailXaml);
        Assert.Contains("x:Name=\"VehicleEditorMakeModelBox\"", vehicleDetailXaml);
        Assert.Contains("x:Name=\"VehicleEditorYearBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorScrollViewer\"", vehicleDetailXaml);
        Assert.Contains("Text=\"Výkon v kW\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.Name=\"Výkon vozidla v kilowattech\"", vehicleDetailXaml);
        Assert.Contains("x:Name=\"VehicleEditorLastTkBox\"", vehicleDetailXaml);
        Assert.Contains("x:Name=\"VehicleEditorNextTkBox\"", vehicleDetailXaml);
        Assert.Contains("x:Name=\"VehicleEditorGreenCardFromBox\"", vehicleDetailXaml);
        Assert.Contains("x:Name=\"VehicleEditorGreenCardToBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.Name=\"Štítky vozidla\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorTagsBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorClimateProfileBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorTimingDriveBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorTransmissionBox\"", vehicleDetailXaml);
        Assert.Contains("ItemsSource=\"{Binding VehicleStateOptions}\"", vehicleDetailXaml);
        Assert.Contains("ItemsSource=\"{Binding VehiclePowertrainOptions}\"", vehicleDetailXaml);
        Assert.Contains("ItemsSource=\"{Binding VehicleClimateProfileOptions}\"", vehicleDetailXaml);
        Assert.Contains("ItemsSource=\"{Binding VehicleTimingDriveOptions}\"", vehicleDetailXaml);
        Assert.Contains("ItemsSource=\"{Binding VehicleTransmissionOptions}\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditRefreshButton\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditOpenItemButton\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditOpenVehicleButton\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditEditItemButton\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditSearchBox\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearAuditSearchButton\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditSortComboBox\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditSortDescendingCheckBox\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AuditListBox\"", auditXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardRefreshButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardShowOnLaunchCheckBox\"", dashboardXaml);
        Assert.Contains("AutomationProperties.Name=\"Zobrazovat dashboard při startu aplikace\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardSearchButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardCostOverviewButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardUpcomingButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardOverdueButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardOpenVehicleButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardVehicleHistoryButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardVehicleCostsButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardCompleteMaintenanceButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardEditVehicleButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardAuditOpenButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardAuditListBox\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardCostOpenButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardCostListBox\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardTimelineOpenButton\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardTimelineListBox\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"DashboardWorkspaceScrollViewer\"", dashboardXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TimelineSearchBox\"", timelineXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearTimelineSearchButton\"", timelineXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TimelineOpenButton\"", timelineXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"GlobalSearchTextBox\"", globalSearchXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearGlobalSearchButton\"", globalSearchXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"GlobalSearchSortComboBox\"", globalSearchXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"GlobalSearchSortDescendingCheckBox\"", globalSearchXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"GlobalSearchRefreshButton\"", globalSearchXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SearchOpenButton\"", globalSearchXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewSearchBox\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearUpcomingOverviewSearchButton\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewSortComboBox\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewSortDescendingCheckBox\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewRefreshButton\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewOpenButton\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewIncludeMissingGreenCardsCheckBox\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewIncludeDataIssuesCheckBox\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverdueOverviewSearchBox\"", overdueOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearOverdueOverviewSearchButton\"", overdueOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverdueOverviewSortComboBox\"", overdueOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverdueOverviewSortDescendingCheckBox\"", overdueOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverdueOverviewRefreshButton\"", overdueOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverdueOverviewOpenButton\"", overdueOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CostListBox\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CostWorkspaceScrollViewer\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CostPeriodPresetComboBox\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CostPeriodStartBox\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CostPeriodEndBox\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ApplyCostPeriodButton\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CostPeriodStatusText\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CostSearchBox\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ClearCostSearchButton\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SelectedCostVehicleDetailText\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CostRefreshButton\"", costXaml);
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
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleDetailRecentHistoryListBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleDetailEvidenceSummaryItems\"", vehicleDetailXaml);
        Assert.Contains("ItemsSource=\"{Binding EvidenceSummaryItems}\"", vehicleDetailXaml);
        Assert.Contains("x:DataType=\"itemvm:VehicleDetailEvidenceSummaryItemViewModel\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleLabel}\"", vehicleDetailXaml);
        Assert.Contains("ItemsSource=\"{Binding RecentHistoryItems}\"", vehicleDetailXaml);
        Assert.Contains("x:DataType=\"itemvm:VehicleHistoryItemViewModel\"", vehicleDetailXaml);
    }

    [Fact]
    public void Workspace_status_and_detail_texts_should_define_accessible_names_and_automation_ids()
    {
        var mainXaml = ReadViewFile("MainWindow.axaml");
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
        var smartAdvisorXaml = ReadWorkspaceOrView("SmartAdvisorWorkspaceView.axaml", true);
        var vehicleDetailXaml = ReadWorkspaceOrView("VehicleDetailWorkspaceView.axaml", true);

        AssertAccessibleBoundText(mainXaml, "LoadErrorText", "LoadError");
        AssertAccessibleBoundText(mainXaml, "ShellStatusText", "ShellStatus");
        AssertAccessibleBoundText(mainXaml, "VehicleListSummaryText", "VehicleListSummary");
        AssertAccessibleBoundText(mainXaml, "VehicleListLockStatusText", "VehicleListLockStatus");
        AssertAccessibleBoundText(mainXaml, "WorkspaceNavigationLockStatusText", "WorkspaceNavigationLockStatus");
        AssertAccessibleBoundText(auditXaml, "AuditSummaryText", "AuditSummary");
        AssertAccessibleBoundText(historyXaml, "HistorySummaryText", "HistorySummary");
        AssertAccessibleBoundText(historyXaml, "HistorySearchSummaryText", "HistorySearchSummary");
        AssertAccessibleBoundText(historyXaml, "HistoryEditorStatusText", "HistoryEditorStatus");
        AssertAccessibleBoundText(historyXaml, "SelectedHistoryDetailText", "SelectedHistoryDetail");
        AssertAccessibleBoundText(fuelXaml, "FuelSummaryText", "FuelSummary");
        AssertAccessibleBoundText(fuelXaml, "FuelSearchSummaryText", "FuelSearchSummary");
        AssertAccessibleBoundText(fuelXaml, "FuelEditorStatusText", "FuelEditorStatus");
        AssertAccessibleBoundText(fuelXaml, "SelectedFuelDetailText", "SelectedFuelDetail");
        AssertAccessibleBoundText(fuelXaml, "FuelAnalysisSummaryText", "FuelAnalysisSummaryText");
        Assert.Contains("AutomationProperties.AutomationId=\"FuelConsumptionSegmentsListBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelGroupSummariesListBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelAnalysisWarningsListBox\"", fuelXaml);
        Assert.Contains("x:DataType=\"itemvm:FuelConsumptionSegmentItemViewModel\"", fuelXaml);
        Assert.Contains("x:DataType=\"itemvm:FuelGroupSummaryItemViewModel\"", fuelXaml);
        Assert.Contains("x:DataType=\"itemvm:FuelAnalysisWarningItemViewModel\"", fuelXaml);
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleLabel}\"", fuelXaml);
        AssertAccessibleBoundText(reminderXaml, "ReminderSummaryText", "ReminderSummary");
        AssertAccessibleBoundText(reminderXaml, "ReminderSearchSummaryText", "ReminderSearchSummary");
        AssertAccessibleBoundText(reminderXaml, "ReminderEditorStatusText", "ReminderEditorStatus");
        AssertAccessibleBoundText(reminderXaml, "SelectedReminderDetailText", "SelectedReminderDetail");
        AssertAccessibleBoundText(maintenanceXaml, "MaintenanceSummaryText", "MaintenanceSummary");
        AssertAccessibleBoundText(maintenanceXaml, "MaintenanceSearchSummaryText", "MaintenanceSearchSummary");
        AssertAccessibleBoundText(maintenanceXaml, "MaintenanceEditorStatusText", "MaintenanceEditorStatus");
        AssertAccessibleBoundText(maintenanceXaml, "SelectedMaintenanceDetailText", "SelectedMaintenanceDetail");
        AssertAccessibleBoundText(recordXaml, "RecordSummaryText", "RecordSummary");
        AssertAccessibleBoundText(recordXaml, "RecordSearchSummaryText", "RecordSearchSummary");
        AssertAccessibleBoundText(recordXaml, "RecordEditorStatusText", "RecordEditorStatus");
        AssertAccessibleBoundText(recordXaml, "RecordEditorPathInputHelpText", "RecordEditorPathInputHelp");
        AssertAccessibleBoundText(recordXaml, "RecordEditorAvailabilityText", "RecordEditorAvailability");
        AssertAccessibleBoundText(recordXaml, "SelectedRecordDetailText", "SelectedRecordDetail");
        AssertAccessibleTextId(recordXaml, "RecordEditorStoredPathText");
        AssertAccessibleTextId(recordXaml, "RecordEditorResolvedPathText");
        AssertAccessibleBoundText(timelineXaml, "TimelineSummaryText", "TimelineSummary");
        AssertAccessibleBoundText(timelineXaml, "TimelineExportStatusText", "ExportStatus");
        AssertAccessibleBoundText(timelineXaml, "SelectedTimelineDetailText", "SelectedTimelineDetail");
        AssertAccessibleBoundText(globalSearchXaml, "GlobalSearchSummaryText", "GlobalSearchSummary");
        AssertAccessibleBoundText(globalSearchXaml, "SelectedSearchResultDetailText", "SelectedSearchResultDetail");
        AssertAccessibleBoundText(upcomingOverviewXaml, "UpcomingOverviewSummaryText", "UpcomingOverviewSummary");
        AssertAccessibleBoundText(upcomingOverviewXaml, "SelectedUpcomingOverviewDetailText", "SelectedUpcomingOverviewDetail");
        AssertAccessibleBoundText(overdueOverviewXaml, "OverdueOverviewSummaryText", "OverdueOverviewSummary");
        AssertAccessibleBoundText(overdueOverviewXaml, "SelectedOverdueOverviewDetailText", "SelectedOverdueOverviewDetail");
        AssertAccessibleBoundText(costXaml, "CostSummaryText", "CostSummary");
        AssertAccessibleBoundText(costXaml, "CostComparisonText", "CostComparison");
        AssertAccessibleBoundText(costXaml, "CostPeriodStatusText", "CostPeriodStatus");
        AssertAccessibleBoundText(costXaml, "CostSearchSummaryText", "CostSearchSummary");
        AssertAccessibleBoundText(smartAdvisorXaml, "SmartAdvisorSummaryText", "SmartAdvisorSummary");
        AssertAccessibleBoundText(smartAdvisorXaml, "SmartAdvisorSelectedDetailText", "SelectedSmartAdvisorDetail");
        Assert.Contains("AutomationProperties.AutomationId=\"SmartAdvisorKeyboardHelpText\"", smartAdvisorXaml);
        Assert.Contains("x:DataType=\"itemvm:SmartAdvisorItemViewModel\"", smartAdvisorXaml);
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleLabel}\"", smartAdvisorXaml);
        AssertAccessibleBoundText(dashboardXaml, "DashboardAuditSummaryText", "AuditSummary");
        AssertAccessibleBoundText(dashboardXaml, "DashboardCostSummaryText", "CostSummary");
        AssertAccessibleBoundText(dashboardXaml, "DashboardCostComparisonText", "CostComparison");
        AssertAccessibleBoundText(dashboardXaml, "DashboardTimelineSummaryText", "DashboardTimelineSummary");
        AssertAccessibleBoundText(dashboardXaml, "SelectedDashboardTimelineDetailText", "SelectedDashboardTimelineDetail");
        AssertAccessibleBoundText(vehicleDetailXaml, "VehicleEditorStatusText", "VehicleEditorStatus");
        AssertAccessibleBoundText(vehicleDetailXaml, "SelectedVehicleOverviewText", "SelectedVehicleOverview");
        AssertAccessibleBoundText(vehicleDetailXaml, "SelectedVehicleDatesText", "SelectedVehicleDates");
        AssertAccessibleBoundText(vehicleDetailXaml, "SelectedVehicleProfileText", "SelectedVehicleProfile");
        AssertAccessibleBoundText(vehicleDetailXaml, "SelectedVehicleEvidenceSummaryText", "SelectedVehicleEvidenceSummary");
        AssertAccessibleBoundText(vehicleDetailXaml, "SelectedVehicleRecentHistorySummaryText", "SelectedVehicleRecentHistorySummary");
    }

    [Fact]
    public void Interactive_desktop_controls_should_have_stable_accessibility_metadata()
    {
        var interactiveControlPattern = new Regex(
            "<(?<type>TextBox|ComboBox|CheckBox|Button|ListBox)(?=[\\s>/])(?<attributes>[\\s\\S]*?)(?:/>|>)",
            RegexOptions.Singleline);
        var failures = new List<string>();

        foreach (var (relativePath, xaml) in ReadAllDesktopXamlFiles())
        {
            foreach (Match match in interactiveControlPattern.Matches(xaml))
            {
                var attributes = match.Groups["attributes"].Value;
                var hasAutomationId = attributes.Contains("AutomationProperties.AutomationId=", StringComparison.Ordinal);
                var hasAccessibleName = attributes.Contains("AutomationProperties.Name=", StringComparison.Ordinal)
                    || attributes.Contains("AutomationProperties.LabeledBy=", StringComparison.Ordinal);

                if (hasAutomationId && hasAccessibleName)
                {
                    continue;
                }

                failures.Add($"{relativePath}:{GetLineNumber(xaml, match.Index)} <{match.Groups["type"].Value}> postrádá "
                    + (hasAutomationId ? "přístupné jméno" : "AutomationId"));
            }
        }

        Assert.True(
            failures.Count == 0,
            "Interaktivní Avalonia prvky musí mít stabilní AutomationId a lidské jméno přes AutomationProperties.Name nebo LabeledBy:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Placeholder_fields_should_expose_accessible_help_text()
    {
        var placeholderControlPattern = new Regex(
            "<(?<type>TextBox|ComboBox)(?=[\\s>/])(?<attributes>[^>]*PlaceholderText=\"[^\"]+\"[^>]*)>",
            RegexOptions.Singleline);
        var automationIdPattern = new Regex(
            "AutomationProperties\\.AutomationId=\"(?<id>[^\"]+)\"",
            RegexOptions.Singleline);
        var failures = new List<string>();

        foreach (var (relativePath, xaml) in ReadAllDesktopXamlFiles())
        {
            foreach (Match match in placeholderControlPattern.Matches(xaml))
            {
                var attributes = match.Groups["attributes"].Value;
                if (attributes.Contains("AutomationProperties.HelpText=", StringComparison.Ordinal))
                {
                    continue;
                }

                var automationIdMatch = automationIdPattern.Match(attributes);
                var controlLabel = automationIdMatch.Success
                    ? automationIdMatch.Groups["id"].Value
                    : $"<{match.Groups["type"].Value}>";
                failures.Add($"{relativePath}:{GetLineNumber(xaml, match.Index)} {controlLabel} má PlaceholderText bez AutomationProperties.HelpText.");
            }
        }

        Assert.True(
            failures.Count == 0,
            "PlaceholderText je vizuální nápověda; stejná instrukce musí být dostupná i přes AutomationProperties.HelpText:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Combo_boxes_should_inherit_accessible_keyboard_help_text()
    {
        var appXaml = ReadDesktopRootFile("App.axaml");

        Assert.Contains("<Style Selector=\"ComboBox\">", appXaml);
        Assert.Contains("Property=\"AutomationProperties.HelpText\"", appXaml);
        Assert.Contains("Šipkami nahoru nebo dolů otevřete seznam", appXaml);
    }

    [Fact]
    public void Required_editor_fields_should_expose_required_for_form_metadata()
    {
        var requiredFields = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["FuelEditorDateBox"] = "True",
            ["FuelEditorOdometerBox"] = "True",
            ["HistoryEditorDateBox"] = "True",
            ["HistoryEditorTypeBox"] = "True",
            ["MaintenanceCompletionDateBox"] = "True",
            ["MaintenanceCompletionOdometerBox"] = "{Binding RequiresOdometer}",
            ["MaintenanceEditorTitleBox"] = "True",
            ["RecordEditorTitleBox"] = "True",
            ["RecordEditorTypeBox"] = "True",
            ["ReminderEditorDaysBox"] = "True",
            ["ReminderEditorDueDateBox"] = "True",
            ["ReminderEditorTitleBox"] = "True",
            ["VehicleEditorCategoryBox"] = "True",
            ["VehicleEditorMakeModelBox"] = "True",
            ["VehicleEditorNameBox"] = "True",
            ["VehicleEditorNextTkBox"] = "True"
        };
        var formControlPattern = new Regex(
            "<(?<type>TextBox|ComboBox)(?=[\\s>/])(?<attributes>[\\s\\S]*?)(?:/>|>)",
            RegexOptions.Singleline);
        var automationIdPattern = new Regex(
            "AutomationProperties\\.AutomationId=\"(?<id>[^\"]+)\"",
            RegexOptions.Singleline);
        var requiredPattern = new Regex(
            "AutomationProperties\\.IsRequiredForForm=\"(?<value>[^\"]+)\"",
            RegexOptions.Singleline);
        var seenRequiredFields = new HashSet<string>(StringComparer.Ordinal);
        var failures = new List<string>();

        foreach (var (relativePath, xaml) in ReadAllDesktopXamlFiles())
        {
            foreach (Match match in formControlPattern.Matches(xaml))
            {
                var attributes = match.Groups["attributes"].Value;
                var automationIdMatch = automationIdPattern.Match(attributes);
                var requiredMatch = requiredPattern.Match(attributes);
                if (requiredMatch.Success && !automationIdMatch.Success)
                {
                    failures.Add($"{relativePath}:{GetLineNumber(xaml, match.Index)} <{match.Groups["type"].Value}> používá IsRequiredForForm bez AutomationId.");
                    continue;
                }

                if (!automationIdMatch.Success)
                {
                    continue;
                }

                var automationId = automationIdMatch.Groups["id"].Value;
                if (requiredFields.TryGetValue(automationId, out var expectedValue))
                {
                    seenRequiredFields.Add(automationId);
                    if (!requiredMatch.Success)
                    {
                        failures.Add($"{relativePath}:{GetLineNumber(xaml, match.Index)} {automationId} postrádá AutomationProperties.IsRequiredForForm.");
                        continue;
                    }

                    if (!string.Equals(requiredMatch.Groups["value"].Value, expectedValue, StringComparison.Ordinal))
                    {
                        failures.Add($"{relativePath}:{GetLineNumber(xaml, match.Index)} {automationId} má IsRequiredForForm={requiredMatch.Groups["value"].Value}, očekává se {expectedValue}.");
                    }

                    continue;
                }

                if (requiredMatch.Success)
                {
                    failures.Add($"{relativePath}:{GetLineNumber(xaml, match.Index)} {automationId} je označené jako povinné, ale není ve schváleném seznamu skutečně vyžadovaných polí.");
                }
            }
        }

        foreach (var missingId in requiredFields.Keys.Except(seenRequiredFields, StringComparer.Ordinal))
        {
            failures.Add($"{missingId} nebylo nalezeno mezi formulářovými poli.");
        }

        Assert.True(
            failures.Count == 0,
            "Pole, která runtime validace odmítá prázdná, musí vystavit IsRequiredForForm; volitelná pole jej používat nesmí:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Status_error_and_progress_texts_should_define_live_regions()
    {
        var textBlockPattern = new Regex(
            "<TextBlock(?<attributes>[^>]*)>",
            RegexOptions.Singleline);
        var liveRegionIdPattern = new Regex(
            "AutomationProperties\\.AutomationId=\"(?<id>[^\"]*(?:StatusText|ErrorText|ProgressText))\"",
            RegexOptions.Singleline);
        var failures = new List<string>();

        foreach (var (relativePath, xaml) in ReadAllDesktopXamlFiles())
        {
            foreach (Match match in textBlockPattern.Matches(xaml))
            {
                var attributes = match.Groups["attributes"].Value;
                var automationIdMatch = liveRegionIdPattern.Match(attributes);
                if (!automationIdMatch.Success
                    || attributes.Contains("AutomationProperties.LiveSetting=", StringComparison.Ordinal))
                {
                    continue;
                }

                failures.Add($"{relativePath}:{GetLineNumber(xaml, match.Index)} TextBlock {automationIdMatch.Groups["id"].Value} postrádá LiveSetting.");
            }
        }

        Assert.True(
            failures.Count == 0,
            "Stavové, chybové a průběhové texty musí být live regiony, aby je čtečka mohla oznámit:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Menu_items_with_input_gestures_should_expose_accelerator_keys()
    {
        var menuItemPattern = new Regex(
            "<MenuItem(?<attributes>[^>]*)>",
            RegexOptions.Singleline);
        var inputGesturePattern = new Regex(
            "InputGesture=\"(?<gesture>[^\"]+)\"",
            RegexOptions.Singleline);
        var acceleratorKeyPattern = new Regex(
            "AutomationProperties\\.AcceleratorKey=\"(?<accelerator>[^\"]+)\"",
            RegexOptions.Singleline);
        var failures = new List<string>();

        foreach (var (relativePath, xaml) in ReadAllDesktopXamlFiles())
        {
            foreach (Match match in menuItemPattern.Matches(xaml))
            {
                var attributes = match.Groups["attributes"].Value;
                var inputGesture = inputGesturePattern.Match(attributes);
                if (!inputGesture.Success)
                {
                    continue;
                }

                var acceleratorKey = acceleratorKeyPattern.Match(attributes);
                if (acceleratorKey.Success
                    && string.Equals(
                        inputGesture.Groups["gesture"].Value,
                        acceleratorKey.Groups["accelerator"].Value,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                failures.Add($"{relativePath}:{GetLineNumber(xaml, match.Index)} MenuItem se zkratkou {inputGesture.Groups["gesture"].Value} postrádá shodný AcceleratorKey.");
            }
        }

        Assert.True(
            failures.Count == 0,
            "Menu položky s InputGesture musí zkratku vystavit i přes AutomationProperties.AcceleratorKey:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Progress_bars_should_expose_name_id_and_help_text()
    {
        var progressBarPattern = new Regex(
            "<ProgressBar(?<attributes>[^>]*)>",
            RegexOptions.Singleline);
        var failures = new List<string>();

        foreach (var (relativePath, xaml) in ReadAllDesktopXamlFiles())
        {
            foreach (Match match in progressBarPattern.Matches(xaml))
            {
                var attributes = match.Groups["attributes"].Value;
                var hasAutomationId = attributes.Contains("AutomationProperties.AutomationId=", StringComparison.Ordinal);
                var hasAccessibleName = attributes.Contains("AutomationProperties.Name=", StringComparison.Ordinal);
                var hasHelpText = attributes.Contains("AutomationProperties.HelpText=", StringComparison.Ordinal);
                if (hasAutomationId && hasAccessibleName && hasHelpText)
                {
                    continue;
                }

                failures.Add($"{relativePath}:{GetLineNumber(xaml, match.Index)} ProgressBar postrádá "
                    + (hasAutomationId ? string.Empty : "AutomationId ")
                    + (hasAccessibleName ? string.Empty : "Name ")
                    + (hasHelpText ? string.Empty : "HelpText"));
            }
        }

        Assert.True(
            failures.Count == 0,
            "Progress bary musí mít stabilní AutomationId, lidský název a doplňkovou nápovědu:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Accessible_label_item_templates_should_define_item_type()
    {
        const string accessibleLabelBinding = "AutomationProperties.Name=\"{Binding AccessibleLabel}\"";
        var failures = new List<string>();

        foreach (var template in ReadDataTemplateRoots())
        {
            if (!template.TemplateContent.Contains(accessibleLabelBinding, StringComparison.Ordinal))
            {
                continue;
            }

            if (!template.RootAttributes.Contains(accessibleLabelBinding, StringComparison.Ordinal))
            {
                failures.Add($"{template.RelativePath}:{GetLineNumber(template.Xaml, template.RootAbsoluteIndex)} DataTemplate {template.ViewModelType} používá AccessibleLabel mimo kořen položky.");
                continue;
            }

            if (!template.RootAttributes.Contains("AutomationProperties.ItemType=", StringComparison.Ordinal))
            {
                failures.Add($"{template.RelativePath}:{GetLineNumber(template.Xaml, template.RootAbsoluteIndex)} DataTemplate {template.ViewModelType} postrádá AutomationProperties.ItemType.");
            }
        }

        Assert.True(
            failures.Count == 0,
            "Kořen každé seznamové položky s AccessibleLabel musí vystavit ItemType, aby čtečka znala druh položky:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Accessible_item_status_should_only_expose_real_status_properties()
    {
        var expectedStatusBindings = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["AuditItemViewModel"] = "Severity",
            ["CostVehicleItemViewModel"] = "Status",
            ["FuelAnalysisWarningItemViewModel"] = "Severity",
            ["ServiceBookItemViewModel"] = "Status",
            ["SmartAdvisorItemViewModel"] = "Priority",
            ["VehicleFuelItemViewModel"] = "TankState",
            ["VehicleListItemViewModel"] = "StatusSummary",
            ["VehicleMaintenanceItemViewModel"] = "Status",
            ["VehicleRecordItemViewModel"] = "AttachmentState",
            ["VehicleReminderItemViewModel"] = "Status",
            ["VehicleTimelineItemViewModel"] = "Status"
        };
        var forbiddenStatusBindings = new HashSet<string>(StringComparer.Ordinal)
        {
            "AccessibleLabel",
            "Detail",
            "SectionLabel",
            "Summary",
            "Title"
        };
        var statusPattern = new Regex(
            "AutomationProperties\\.ItemStatus=\"\\{Binding (?<property>[^}\\s]+)\\}\"",
            RegexOptions.Singleline);
        var failures = new List<string>();

        foreach (var template in ReadDataTemplateRoots())
        {
            if (!template.RootAttributes.Contains("AutomationProperties.Name=\"{Binding AccessibleLabel}\"", StringComparison.Ordinal))
            {
                continue;
            }

            var viewModelType = GetShortXamlTypeName(template.ViewModelType);
            var statusMatch = statusPattern.Match(template.RootAttributes);
            if (statusMatch.Success
                && forbiddenStatusBindings.Contains(statusMatch.Groups["property"].Value))
            {
                failures.Add($"{template.RelativePath}:{GetLineNumber(template.Xaml, template.RootAbsoluteIndex)} DataTemplate {template.ViewModelType} používá ItemStatus pro popis/detail místo skutečného stavu.");
            }

            if (expectedStatusBindings.TryGetValue(viewModelType, out var expectedProperty))
            {
                if (!statusMatch.Success)
                {
                    failures.Add($"{template.RelativePath}:{GetLineNumber(template.Xaml, template.RootAbsoluteIndex)} DataTemplate {template.ViewModelType} postrádá AutomationProperties.ItemStatus.");
                    continue;
                }

                if (!string.Equals(statusMatch.Groups["property"].Value, expectedProperty, StringComparison.Ordinal))
                {
                    failures.Add($"{template.RelativePath}:{GetLineNumber(template.Xaml, template.RootAbsoluteIndex)} DataTemplate {template.ViewModelType} má ItemStatus navázaný na {statusMatch.Groups["property"].Value}, očekává se {expectedProperty}.");
                }

                continue;
            }

            if (statusMatch.Success)
            {
                failures.Add($"{template.RelativePath}:{GetLineNumber(template.Xaml, template.RootAbsoluteIndex)} DataTemplate {template.ViewModelType} má ItemStatus bez schválené stavové vlastnosti.");
            }
        }

        Assert.True(
            failures.Count == 0,
            "ItemStatus smí být použitý jen pro skutečný stav, prioritu nebo dostupnost a na schválených viewmodelech:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Desktop_windows_should_define_one_primary_accessible_heading()
    {
        var headingPattern = new Regex(
            "<TextBlock(?<attributes>[^>]*AutomationProperties\\.HeadingLevel=\"1\"[^>]*)>",
            RegexOptions.Singleline);
        var failures = new List<string>();

        foreach (var (relativePath, xaml) in ReadTopLevelDesktopWindowXamlFiles())
        {
            var headings = headingPattern.Matches(xaml);
            if (headings.Count != 1)
            {
                failures.Add($"{relativePath}: očekáván právě jeden hlavní nadpis HeadingLevel=1, nalezeno {headings.Count}.");
                continue;
            }

            var attributes = headings[0].Groups["attributes"].Value;
            if (!attributes.Contains("AutomationProperties.AutomationId=", StringComparison.Ordinal)
                || !attributes.Contains("AutomationProperties.Name=", StringComparison.Ordinal))
            {
                failures.Add($"{relativePath}:{GetLineNumber(xaml, headings[0].Index)} hlavní nadpis postrádá AutomationId nebo Name.");
            }
        }

        Assert.True(
            failures.Count == 0,
            "Každé samostatné okno/dialog musí mít jeden dohledatelný hlavní nadpis s AutomationId a Name:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Landmark_elements_should_be_exposed_in_control_accessibility_view()
    {
        var landmarkPattern = new Regex(
            "<(?<type>[A-Za-z][A-Za-z0-9.:]*)(?<attributes>[^>]*AutomationProperties\\.LandmarkType=\"[^\"]+\"[^>]*)>",
            RegexOptions.Singleline);
        var failures = new List<string>();

        foreach (var (relativePath, xaml) in ReadAllDesktopXamlFiles())
        {
            foreach (Match match in landmarkPattern.Matches(xaml))
            {
                var attributes = match.Groups["attributes"].Value;
                if (attributes.Contains("AutomationProperties.AccessibilityView=\"Control\"", StringComparison.Ordinal))
                {
                    continue;
                }

                failures.Add($"{relativePath}:{GetLineNumber(xaml, match.Index)} <{match.Groups["type"].Value}> má LandmarkType bez AccessibilityView=Control.");
            }
        }

        Assert.True(
            failures.Count == 0,
            "Každý Avalonia landmark musí být zároveň v Control accessibility view, aby byl spolehlivě viditelný přes UIA:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Manual_keydown_handlers_should_stay_documented_accessibility_exceptions()
    {
        var expectedOccurrencesByFile = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["AboutWindow.axaml.cs"] = 1,
            ["ConfirmationWindow.axaml.cs"] = 1,
            ["DataStoreHealthWindow.axaml.cs"] = 1,
            ["MainWindow.axaml.cs"] = 4,
            ["MaintenanceCompletionWindow.axaml.cs"] = 1,
            ["ModalWorkspaceWindowHelpers.cs"] = 1,
            ["NotificationWindow.axaml.cs"] = 1,
            ["ServiceBookWindow.axaml.cs"] = 1,
            ["SettingsWindow.axaml.cs"] = 1,
            ["TrayActionsWindow.axaml.cs"] = 1,
            ["UpdateCheckWindow.axaml.cs"] = 1,
            ["UpdateInstallProgressWindow.axaml.cs"] = 1,
            ["VehicleStarterBundleWindow.axaml.cs"] = 2,
            ["WorkspaceViewBase.cs"] = 1
        };
        var actualOccurrencesByFile = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var (relativePath, code) in ReadAllDesktopViewCodeFiles())
        {
            var count = Regex.Matches(code, "InputElement\\.KeyDownEvent").Count;
            if (count > 0)
            {
                actualOccurrencesByFile[Path.GetFileName(relativePath)] = count;
            }
        }

        Assert.Equal(
            expectedOccurrencesByFile.OrderBy(pair => pair.Key).ToArray(),
            actualOccurrencesByFile.OrderBy(pair => pair.Key).ToArray());

        var accessibilityDocs = ReadDocumentationFile("ACCESSIBILITY.md");
        Assert.Contains("Documented keyboard/focus exceptions", accessibilityDocs);
        Assert.Contains("MainWindow.axaml.cs", accessibilityDocs);
        Assert.Contains("KeyboardAccessibilityHelper.cs", accessibilityDocs);
        Assert.Contains("WorkspaceViewBase.cs", accessibilityDocs);
        Assert.Contains("VehicleStarterBundleWindow.axaml.cs", accessibilityDocs);
    }

    [Fact]
    public void Accessibility_documentation_should_record_pre_conformance_status_and_evidence_path()
    {
        var accessibilityDocs = ReadDocumentationFile("ACCESSIBILITY.md");
        var evidenceReadme = ReadDocumentationFile(Path.Combine("accessibility-evidence", "README.md"));

        Assert.Contains("accessibility-oriented / pre-conformance", accessibilityDocs);
        Assert.Contains("We do not claim formal WCAG", accessibilityDocs);
        Assert.Contains("dotnet/docs/accessibility-evidence/", accessibilityDocs);
        Assert.Contains("Avalonia accessibility", accessibilityDocs);
        Assert.Contains("AutomationProperties.ItemType", accessibilityDocs);
        Assert.Contains("AutomationProperties.ItemStatus", accessibilityDocs);
        Assert.Contains("PlaceholderText", accessibilityDocs);
        Assert.Contains("AutomationProperties.HelpText", accessibilityDocs);
        Assert.Contains("ComboBox", accessibilityDocs);
        Assert.Contains("AutomationProperties.IsRequiredForForm", accessibilityDocs);
        Assert.Contains("Date:", evidenceReadme);
        Assert.Contains("Screen reader:", evidenceReadme);
        Assert.Contains("Known issues:", evidenceReadme);
    }

    [Fact]
    public void Main_shell_should_use_conservative_avalonia_landmarks_headings_and_live_statuses()
    {
        var mainXaml = ReadViewFile("MainWindow.axaml");
        var vehicleDetailXaml = ReadWorkspaceOrView("VehicleDetailWorkspaceView.axaml", true);

        Assert.Contains("AutomationProperties.LandmarkType=\"Navigation\"", mainXaml);
        Assert.Contains("AutomationProperties.LandmarkType=\"Search\"", mainXaml);
        Assert.Contains("AutomationProperties.LandmarkType=\"Main\"", mainXaml);
        Assert.Contains("AutomationProperties.AccessibilityView=\"Control\"", mainXaml);
        Assert.Contains("AutomationProperties.HeadingLevel=\"1\"", mainXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AppTitleText\"", mainXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ShellStatusText\" AutomationProperties.LiveSetting=\"Polite\"", mainXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"LoadErrorText\" AutomationProperties.LiveSetting=\"Assertive\"", mainXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorStatusText\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.LiveSetting=\"Polite\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.HeadingLevel=\"2\"", vehicleDetailXaml);
    }

    [Fact]
    public void Overview_workspace_xaml_should_define_keyboard_first_shortcuts()
    {
        var timelineXaml = ReadWorkspaceOrView("TimelineWorkspaceView.axaml", true);
        var auditXaml = ReadWorkspaceOrView("AuditWorkspaceView.axaml", true);
        var globalSearchXaml = ReadWorkspaceOrView("GlobalSearchWorkspaceView.axaml", true);
        var upcomingOverviewXaml = ReadWorkspaceOrView("UpcomingOverviewWorkspaceView.axaml", true);
        var overdueOverviewXaml = ReadWorkspaceOrView("OverdueOverviewWorkspaceView.axaml", true);
        var smartAdvisorXaml = ReadWorkspaceOrView("SmartAdvisorWorkspaceView.axaml", true);
        var historyXaml = ReadWorkspaceOrView("HistoryWorkspaceView.axaml", true);
        var fuelXaml = ReadWorkspaceOrView("FuelWorkspaceView.axaml", true);
        var reminderXaml = ReadWorkspaceOrView("ReminderWorkspaceView.axaml", true);
        var maintenanceXaml = ReadWorkspaceOrView("MaintenanceWorkspaceView.axaml", true);
        var recordXaml = ReadWorkspaceOrView("RecordWorkspaceView.axaml", true);
        var costXaml = ReadWorkspaceOrView("CostWorkspaceView.axaml", true);
        var dashboardXaml = ReadWorkspaceOrView("DashboardWorkspaceView.axaml", true);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", timelineXaml);
        Assert.Contains("Gesture=\"Ctrl+R\" Command=\"{Binding RefreshTimelineCommand}\"", timelineXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding OpenSelectedTimelineItemCommand}\"", timelineXaml);
        Assert.Contains("Command=\"{Binding ClearTimelineSearchCommand}\"", timelineXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TimelineRefreshButton\"", timelineXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", auditXaml);
        Assert.Contains("Gesture=\"Ctrl+R\" Command=\"{Binding RefreshAuditCommand}\"", auditXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedAuditVehicleCommand}\"", auditXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding OpenSelectedAuditItemCommand}\"", auditXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedAuditItemCommand}\"", auditXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedAuditItemCommand}\"", auditXaml);
        Assert.Contains("Command=\"{Binding ClearAuditSearchCommand}\"", auditXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", globalSearchXaml);
        Assert.Contains("Gesture=\"Ctrl+R\" Command=\"{Binding RefreshGlobalSearchCommand}\"", globalSearchXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedSearchResultCommand}\"", globalSearchXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding OpenSelectedSearchResultCommand}\"", globalSearchXaml);
        Assert.Contains("Command=\"{Binding ClearGlobalSearchCommand}\"", globalSearchXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", upcomingOverviewXaml);
        Assert.Contains("Gesture=\"Ctrl+R\" Command=\"{Binding RefreshUpcomingOverviewCommand}\"", upcomingOverviewXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedUpcomingOverviewVehicleCommand}\"", upcomingOverviewXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding OpenSelectedUpcomingOverviewItemCommand}\"", upcomingOverviewXaml);
        Assert.Contains("Command=\"{Binding ClearUpcomingOverviewSearchCommand}\"", upcomingOverviewXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", overdueOverviewXaml);
        Assert.Contains("Gesture=\"Ctrl+R\" Command=\"{Binding RefreshOverdueOverviewCommand}\"", overdueOverviewXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedOverdueOverviewVehicleCommand}\"", overdueOverviewXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding OpenSelectedOverdueOverviewItemCommand}\"", overdueOverviewXaml);
        Assert.Contains("Command=\"{Binding ClearOverdueOverviewSearchCommand}\"", overdueOverviewXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", smartAdvisorXaml);
        Assert.Contains("Gesture=\"Ctrl+R\" Command=\"{Binding RefreshSmartAdvisorCommand}\"", smartAdvisorXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedSmartAdvisorItemCommand}\"", smartAdvisorXaml);
        Assert.Contains("Gesture=\"Enter\" Command=\"{Binding OpenSelectedSmartAdvisorItemCommand}\"", smartAdvisorXaml);
        Assert.Contains("Command=\"{Binding ClearSmartAdvisorFiltersCommand}\"", smartAdvisorXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", historyXaml);
        Assert.Contains("Gesture=\"Ctrl+N\" Command=\"{Binding CreateHistoryCommand}\"", historyXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedHistoryCommand}\"", historyXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedHistoryCommand}\"", historyXaml);
        Assert.Contains("Gesture=\"Ctrl+S\" Command=\"{Binding SaveHistoryCommand}\"", historyXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", fuelXaml);
        Assert.Contains("Gesture=\"Ctrl+N\" Command=\"{Binding CreateFuelCommand}\"", fuelXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedFuelCommand}\"", fuelXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedFuelCommand}\"", fuelXaml);
        Assert.Contains("Gesture=\"Ctrl+S\" Command=\"{Binding SaveFuelCommand}\"", fuelXaml);
        Assert.Contains("Gesture=\"Enter\" Command=\"{Binding OpenSelectedFuelConsumptionSegmentCommand}\"", fuelXaml);
        Assert.Contains("Gesture=\"Enter\" Command=\"{Binding OpenSelectedFuelGroupSummaryCommand}\"", fuelXaml);
        Assert.Contains("Gesture=\"Enter\" Command=\"{Binding OpenSelectedFuelAnalysisWarningCommand}\"", fuelXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", reminderXaml);
        Assert.Contains("Gesture=\"Ctrl+N\" Command=\"{Binding CreateReminderCommand}\"", reminderXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedReminderCommand}\"", reminderXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedReminderCommand}\"", reminderXaml);
        Assert.Contains("Gesture=\"Ctrl+Shift+N\" Command=\"{Binding AdvanceSelectedReminderCommand}\"", reminderXaml);
        Assert.Contains("Gesture=\"Ctrl+S\" Command=\"{Binding SaveReminderCommand}\"", reminderXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", maintenanceXaml);
        Assert.Contains("Gesture=\"Ctrl+N\" Command=\"{Binding CreateMaintenanceCommand}\"", maintenanceXaml);
        Assert.Contains("Gesture=\"Ctrl+Shift+N\" Command=\"{Binding OpenMaintenanceTemplatesCommand}\"", maintenanceXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedMaintenanceCommand}\"", maintenanceXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedMaintenanceCommand}\"", maintenanceXaml);
        Assert.Contains("Gesture=\"Ctrl+L\" Command=\"{Binding CompleteSelectedMaintenanceCommand}\"", maintenanceXaml);
        Assert.Contains("Gesture=\"Ctrl+S\" Command=\"{Binding SaveMaintenanceCommand}\"", maintenanceXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"Ctrl+N\" Command=\"{Binding CreateRecordCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedRecordCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedRecordCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"Ctrl+S\" Command=\"{Binding SaveRecordCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedRecordFileCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"Ctrl+Shift+O\" Command=\"{Binding OpenSelectedRecordFolderCommand}\"", recordXaml);
        Assert.Contains("Gesture=\"Ctrl+Shift+C\" Command=\"{Binding CopySelectedRecordPathCommand}\"", recordXaml);

        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusSearchCommand}\"", costXaml);
        Assert.Contains("Gesture=\"Ctrl+R\" Command=\"{Binding RefreshCostCommand}\"", costXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding FocusSelectedCostDetailCommand}\"", costXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedCostVehicleCommand}\"", costXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedCostVehicleCommand}\"", costXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedCostVehicleCommand}\"", costXaml);
        Assert.Contains("Command=\"{Binding ClearCostSearchCommand}\"", costXaml);

        Assert.Contains("Gesture=\"Ctrl+R\" Command=\"{Binding RefreshDashboardCommand}\"", dashboardXaml);
        Assert.Contains("Gesture=\"Ctrl+F\" Command=\"{Binding FocusGlobalSearchCommand}\"", dashboardXaml);
        Assert.Contains("Gesture=\"Ctrl+T\" Command=\"{Binding FocusUpcomingOverviewCommand}\"", dashboardXaml);
        Assert.Contains("Gesture=\"Ctrl+Shift+T\" Command=\"{Binding FocusOverdueOverviewCommand}\"", dashboardXaml);
        Assert.Contains("Gesture=\"Ctrl+O\" Command=\"{Binding OpenSelectedDashboardVehicleCommand}\"", dashboardXaml);
        Assert.Contains("Gesture=\"Ctrl+P\" Command=\"{Binding OpenSelectedDashboardTimelineItemCommand}\"", dashboardXaml);
        Assert.Contains("Gesture=\"Ctrl+H\" Command=\"{Binding OpenSelectedDashboardVehicleHistoryCommand}\"", dashboardXaml);
        Assert.Contains("Gesture=\"Ctrl+L\" Command=\"{Binding CompleteSelectedDashboardMaintenanceCommand}\"", dashboardXaml);
        Assert.Contains("Gesture=\"Ctrl+U\" Command=\"{Binding EditSelectedDashboardVehicleCommand}\"", dashboardXaml);
        Assert.Contains("Gesture=\"F2\" Command=\"{Binding EditSelectedDashboardVehicleCommand}\"", dashboardXaml);
    }

    [Fact]
    public void Workspace_windows_should_use_shared_focus_and_close_lifecycle()
    {
        var lifecycleHelper = ReadViewCodeBehind("ModalWorkspaceWindowHelpers.cs");
        var workspaceWindows = new[]
        {
            ("AuditWindow.axaml.cs", "AuditWorkspaceHost"),
            ("CostWindow.axaml.cs", "CostWorkspaceHost"),
            ("DashboardWindow.axaml.cs", "DashboardWorkspaceHost"),
            ("FuelWindow.axaml.cs", "FuelWorkspaceHost"),
            ("GlobalSearchWindow.axaml.cs", "GlobalSearchWorkspaceHost"),
            ("HistoryWindow.axaml.cs", "HistoryWorkspaceHost"),
            ("MaintenanceWindow.axaml.cs", "MaintenanceWorkspaceHost"),
            ("OverdueOverviewWindow.axaml.cs", "OverdueOverviewWorkspaceHost"),
            ("RecordsWindow.axaml.cs", "RecordWorkspaceHost"),
            ("RemindersWindow.axaml.cs", "ReminderWorkspaceHost"),
            ("SmartAdvisorWindow.axaml.cs", "SmartAdvisorWorkspaceHost"),
            ("TimelineWindow.axaml.cs", "TimelineWorkspaceHost"),
            ("UpcomingOverviewWindow.axaml.cs", "UpcomingOverviewWorkspaceHost"),
            ("VehicleDetailWindow.axaml.cs", "VehicleDetailWorkspaceHost")
        };

        foreach (var (fileName, hostName) in workspaceWindows)
        {
            var codeBehind = ReadViewCodeBehind(fileName);
            var xaml = ReadViewFile(fileName.Replace(".axaml.cs", ".axaml", StringComparison.Ordinal));
            Assert.Contains($"RegisterWorkspaceLifecycle(this, \"{hostName}\"", codeBehind);
            Assert.Contains("Escape okno zavře.", xaml);
            Assert.DoesNotContain("Opened += OnOpened", codeBehind);
            Assert.DoesNotContain("Closing += OnClosing", codeBehind);
        }

        Assert.Contains("InputElement.KeyDownEvent", lifecycleHelper);
        Assert.Contains("RoutingStrategies.Bubble", lifecycleHelper);
        Assert.Contains("Key.Escape", lifecycleHelper);
        Assert.Contains("window.Close();", lifecycleHelper);
    }

    [Fact]
    public void Main_window_should_open_workspace_windows_through_shared_helper()
    {
        var codeBehind = ReadViewCodeBehind("MainWindow.axaml.cs");
        var workspaceWindows = new[]
        {
            "AuditWindow",
            "CostWindow",
            "DashboardWindow",
            "FuelWindow",
            "GlobalSearchWindow",
            "HistoryWindow",
            "MaintenanceWindow",
            "OverdueOverviewWindow",
            "RecordsWindow",
            "RemindersWindow",
            "TimelineWindow",
            "UpcomingOverviewWindow",
            "VehicleDetailWindow"
        };

        Assert.Contains("ShowWorkspaceWindowAsync<TWindow>", codeBehind);

        foreach (var windowName in workspaceWindows)
        {
            Assert.Contains($"ShowWorkspaceWindowAsync<{windowName}>", codeBehind);
            Assert.DoesNotContain($"new {windowName}", codeBehind);
        }
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

    private static string ReadDesktopRootFile(string fileName)
    {
        var desktopRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "Vehimap.Desktop"));

        return File.ReadAllText(Path.Combine(desktopRoot, fileName));
    }

    private static string ReadDocumentationFile(string relativePath)
    {
        var repositoryRoot = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine(repositoryRoot, "dotnet", "docs", relativePath));
    }

    private static IEnumerable<(string RelativePath, string Content)> ReadAllDesktopXamlFiles()
    {
        var repositoryRoot = FindRepositoryRoot();
        var viewsRoot = Path.Combine(repositoryRoot, "dotnet", "src", "Vehimap.Desktop", "Views");

        foreach (var file in Directory.EnumerateFiles(viewsRoot, "*.axaml", SearchOption.AllDirectories))
        {
            yield return (Path.GetRelativePath(repositoryRoot, file), File.ReadAllText(file));
        }
    }

    private static IEnumerable<(
        string RelativePath,
        string Xaml,
        string ViewModelType,
        string TemplateContent,
        string RootAttributes,
        int RootAbsoluteIndex)> ReadDataTemplateRoots()
    {
        var templatePattern = new Regex(
            "<DataTemplate\\s+x:DataType=\"(?<type>[^\"]+)\"[^>]*>(?<content>[\\s\\S]*?)</DataTemplate>",
            RegexOptions.Singleline);
        var rootPattern = new Regex(
            "^\\s*<(?<element>[A-Za-z][A-Za-z0-9.:]*)(?<attributes>[^>]*)>",
            RegexOptions.Singleline);

        foreach (var (relativePath, xaml) in ReadAllDesktopXamlFiles())
        {
            foreach (Match templateMatch in templatePattern.Matches(xaml))
            {
                var content = templateMatch.Groups["content"].Value;
                var rootMatch = rootPattern.Match(content);
                Assert.True(
                    rootMatch.Success,
                    $"{relativePath}:{GetLineNumber(xaml, templateMatch.Index)} DataTemplate nemá čitelný kořenový prvek.");

                yield return (
                    relativePath,
                    xaml,
                    templateMatch.Groups["type"].Value,
                    content,
                    rootMatch.Groups["attributes"].Value,
                    templateMatch.Groups["content"].Index + rootMatch.Index);
            }
        }
    }

    private static IEnumerable<(string RelativePath, string Content)> ReadTopLevelDesktopWindowXamlFiles()
    {
        var repositoryRoot = FindRepositoryRoot();
        var viewsRoot = Path.Combine(repositoryRoot, "dotnet", "src", "Vehimap.Desktop", "Views");

        foreach (var file in Directory.EnumerateFiles(viewsRoot, "*.axaml", SearchOption.TopDirectoryOnly))
        {
            yield return (Path.GetRelativePath(repositoryRoot, file), File.ReadAllText(file));
        }
    }

    private static IEnumerable<(string RelativePath, string Content)> ReadAllDesktopViewCodeFiles()
    {
        var repositoryRoot = FindRepositoryRoot();
        var viewsRoot = Path.Combine(repositoryRoot, "dotnet", "src", "Vehimap.Desktop", "Views");

        foreach (var file in Directory.EnumerateFiles(viewsRoot, "*.cs", SearchOption.AllDirectories))
        {
            yield return (Path.GetRelativePath(repositoryRoot, file), File.ReadAllText(file));
        }
    }

    private static int GetLineNumber(string content, int index)
    {
        var line = 1;
        for (var i = 0; i < index && i < content.Length; i++)
        {
            if (content[i] == '\n')
            {
                line++;
            }
        }

        return line;
    }

    private static string GetShortXamlTypeName(string xamlType)
    {
        var separatorIndex = xamlType.LastIndexOf(':');
        return separatorIndex >= 0
            ? xamlType[(separatorIndex + 1)..]
            : xamlType;
    }

    private static void AssertAccessibleBoundText(string xaml, string automationId, string bindingName)
    {
        AssertAccessibleTextId(xaml, automationId);
        Assert.Contains($"AutomationProperties.Name=\"{{Binding {bindingName}}}\"", xaml);
    }

    private static void AssertAccessibleTextId(string xaml, string automationId)
    {
        Assert.Contains($"AutomationProperties.AutomationId=\"{automationId}\"", xaml);
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
