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

        Assert.Contains("Title=\"Nastavení\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"TechnicalReminderDaysBox\"", settingsXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CancelSettingsButton\"", settingsXaml);
        Assert.Contains("Title=\"O programu\"", aboutXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"ReleaseNotesButton\"", aboutXaml);
        Assert.Contains("Title=\"Kontrola aktualizací\"", updateXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"UpdateCloseButton\"", updateXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseHistoryWindowButton\"", historyXaml);
        Assert.Contains("AutomationProperties.AutomationId=\"CloseDashboardWindowButton\"", dashboardXaml);
    }
}
