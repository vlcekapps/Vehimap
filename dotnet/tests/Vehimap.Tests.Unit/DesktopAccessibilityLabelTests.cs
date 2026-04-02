using Vehimap.Desktop.ViewModels;
using Xunit;

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
        Assert.Contains("režim Spravovaná kopie", item.AccessibleLabel);
        Assert.Contains("stav přílohy Příloha je dostupná", item.AccessibleLabel);
        Assert.DoesNotContain(nameof(VehicleRecordItemViewModel), item.AccessibleLabel);
    }

    [Fact]
    public void Main_window_xaml_should_define_accessible_shell_metadata()
    {
        var xamlPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "Vehimap.Desktop",
            "Views",
            "MainWindow.axaml"));

        var xaml = File.ReadAllText(xamlPath);

        Assert.Contains("AutomationProperties.Name=\"Vehimap Desktop Preview\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Seznam načtených vozidel\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleLabel}\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SettingsButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"AboutButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpdateCheckButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleListBox\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenHistoryWindowButton\"", xaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenDashboardWindowButton\"", xaml);
        Assert.Contains("x:Name=\"DashboardTabButton\"", xaml);
        Assert.Contains("IsTabStop=\"{Binding IsDashboardTabSelected}\"", xaml);
        Assert.Contains("<RadioButton x:Name=\"DashboardTabButton\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Karta Dashboard\"", xaml);
        Assert.Contains("Click=\"OnSettingsClick\"", xaml);
        Assert.Contains("Click=\"OnAboutClick\"", xaml);
        Assert.Contains("Click=\"OnUpdateCheckClick\"", xaml);
    }

    [Fact]
    public void Dialog_xaml_files_should_define_expected_titles_and_automation_ids()
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

        var settingsXaml = File.ReadAllText(Path.Combine(desktopRoot, "SettingsWindow.axaml"));
        var aboutXaml = File.ReadAllText(Path.Combine(desktopRoot, "AboutWindow.axaml"));
        var updateXaml = File.ReadAllText(Path.Combine(desktopRoot, "UpdateCheckWindow.axaml"));
        var historyXaml = File.ReadAllText(Path.Combine(desktopRoot, "HistoryWindow.axaml"));
        var dashboardXaml = File.ReadAllText(Path.Combine(desktopRoot, "DashboardWindow.axaml"));
        var bundleXaml = File.ReadAllText(Path.Combine(desktopRoot, "VehicleStarterBundleWindow.axaml"));

        Assert.Contains("Title=\"Nastavení\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TechnicalReminderDaysBox\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CancelSettingsButton\"", settingsXaml);
        Assert.Contains("Title=\"O programu\"", aboutXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReleaseNotesButton\"", aboutXaml);
        Assert.Contains("Title=\"Kontrola aktualizací\"", updateXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpdateCloseButton\"", updateXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseHistoryWindowButton\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseDashboardWindowButton\"", dashboardXaml);
        Assert.Contains("Title=\"Balíček pro vozidlo\"", bundleXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"BundleItemsListBox\"", bundleXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ApplyBundleButton\"", bundleXaml);
    }

    [Fact]
    public void Editor_fields_should_define_explicit_accessibility_names()
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

        var settingsXaml = File.ReadAllText(Path.Combine(desktopRoot, "SettingsWindow.axaml"));
        var fuelXaml = File.ReadAllText(Path.Combine(desktopRoot, "Workspaces", "FuelWorkspaceView.axaml"));
        var historyXaml = File.ReadAllText(Path.Combine(desktopRoot, "Workspaces", "HistoryWorkspaceView.axaml"));
        var maintenanceXaml = File.ReadAllText(Path.Combine(desktopRoot, "Workspaces", "MaintenanceWorkspaceView.axaml"));
        var recordXaml = File.ReadAllText(Path.Combine(desktopRoot, "Workspaces", "RecordWorkspaceView.axaml"));
        var reminderXaml = File.ReadAllText(Path.Combine(desktopRoot, "Workspaces", "ReminderWorkspaceView.axaml"));
        var timelineXaml = File.ReadAllText(Path.Combine(desktopRoot, "Workspaces", "TimelineWorkspaceView.axaml"));
        var globalSearchXaml = File.ReadAllText(Path.Combine(desktopRoot, "Workspaces", "GlobalSearchWorkspaceView.axaml"));
        var upcomingOverviewXaml = File.ReadAllText(Path.Combine(desktopRoot, "Workspaces", "UpcomingOverviewWorkspaceView.axaml"));
        var overdueOverviewXaml = File.ReadAllText(Path.Combine(desktopRoot, "Workspaces", "OverdueOverviewWorkspaceView.axaml"));
        var costXaml = File.ReadAllText(Path.Combine(desktopRoot, "Workspaces", "CostWorkspaceView.axaml"));
        var vehicleDetailXaml = File.ReadAllText(Path.Combine(desktopRoot, "Workspaces", "VehicleDetailWorkspaceView.axaml"));

        Assert.Contains("AutomationProperties.Name=\"Upozornění na technickou kontrolu ve dnech\"", settingsXaml);
        Assert.Contains("AutomationProperties.Name=\"Upozornění na zelenou kartu ve dnech\"", settingsXaml);
        Assert.Contains("AutomationProperties.Name=\"Datum tankování\"", fuelXaml);
        Assert.Contains("AutomationProperties.Name=\"Typ paliva\"", fuelXaml);
        Assert.Contains("AutomationProperties.Name=\"Datum historického záznamu\"", historyXaml);
        Assert.Contains("AutomationProperties.Name=\"Typ historické události\"", historyXaml);
        Assert.Contains("AutomationProperties.Name=\"Název servisního úkonu\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.Name=\"Interval údržby v kilometrech\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.Name=\"Typ dokladu\"", recordXaml);
        Assert.Contains("AutomationProperties.Name=\"Režim přílohy dokladu\"", recordXaml);
        Assert.Contains("AutomationProperties.Name=\"Název připomínky\"", reminderXaml);
        Assert.Contains("AutomationProperties.Name=\"Termín připomínky\"", reminderXaml);
        Assert.Contains("AutomationProperties.Name=\"Název vozidla\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.Name=\"Kategorie vozidla\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.Name=\"Klimatizace vozidla\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.Name=\"Typ rozvodů vozidla\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.Name=\"Převodovka vozidla\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CreateReminderButton\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReminderEditorTitleBox\"", reminderXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CreateRecordButton\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"RecordEditorTitleBox\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"RecordAttachmentModeComboBox\"", recordXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CreateHistoryButton\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"HistoryEditorDateBox\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CreateFuelButton\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"FuelEditorDateBox\"", fuelXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CreateMaintenanceButton\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"MaintenanceEditorTitleBox\"", maintenanceXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TimelineSearchBox\"", timelineXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TimelineOpenButton\"", timelineXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"GlobalSearchTextBox\"", globalSearchXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"SearchOpenButton\"", globalSearchXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewSearchBox\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpcomingOverviewOpenButton\"", upcomingOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverdueOverviewSearchBox\"", overdueOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OverdueOverviewOpenButton\"", overdueOverviewXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CostListBox\"", costXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CreateVehicleButton\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorNameBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"OpenVehicleStarterBundleButton\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorClimateProfileBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorTimingDriveBox\"", vehicleDetailXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"VehicleEditorTransmissionBox\"", vehicleDetailXaml);
        Assert.Contains("x:Name=\"CancelVehicleButton\"", vehicleDetailXaml);
        Assert.Contains("ItemsSource=\"{Binding VehicleCategoryOptions}\"", vehicleDetailXaml);
        Assert.Contains("ItemsSource=\"{Binding VehicleStateOptions}\"", vehicleDetailXaml);
        Assert.Contains("<ComboBox x:Name=\"VehicleEditorCategoryBox\"", vehicleDetailXaml);
    }
}
