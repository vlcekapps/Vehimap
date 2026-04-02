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
            "Veterán | 1 položka k řešení");

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
        Assert.Contains("AutomationProperties.HelpText=\"Multiplatformní preview Vehimapu. Po otevření je fokus v seznamu vozidel vlevo.\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleLabel}\"", xaml);
        Assert.Contains("x:Name=\"DashboardTabButton\"", xaml);
        Assert.Contains("IsTabStop=\"{Binding IsDashboardTabSelected}\"", xaml);
        Assert.Contains("<RadioButton x:Name=\"DashboardTabButton\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Karta Dashboard\"", xaml);
    }
}
