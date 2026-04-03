using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopPrintableVehicleReportServiceTests
{
    [Fact]
    public void Build_html_should_include_title_counts_sections_and_sorted_rows()
    {
        var service = new DesktopPrintableVehicleReportService();
        var timelineService = new LegacyTimelineService();
        var generatedAt = new DateTime(2026, 4, 3, 9, 15, 0);
        var today = DateOnly.FromDateTime(generatedAt);

        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Božena", "Osobní vozidla", "Srazové", "Škoda 100", "", "1974", "35", "", "05/2026", "", "10/2026"),
                new Vehicle("veh_2", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "04/2026", "", "04/2026"),
                new Vehicle("veh_3", "Drobeček", "Autobusy", "Meziměsto", "Karosa C734.20", "4C3 3359", "1990", "152", "", "03/2027", "", "03/2027")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "Veterán", "Srazové", "Benzín", "Bez klimatizace", "Řetěz", "Manuální"),
                new VehicleMeta("veh_2", "Běžný provoz", "Rodinné", "Benzín", "Bez klimatizace", "Řetěz", "Manuální"),
                new VehicleMeta("veh_3", "Odstaveno", "Autobus", "Nafta", string.Empty, string.Empty, string.Empty)
            ]
        };
        dataSet.Settings.SetValue("notifications", "technical_reminder_days", "30");
        dataSet.Settings.SetValue("notifications", "green_card_reminder_days", "30");

        var metaByVehicleId = dataSet.VehicleMetaEntries.ToDictionary(item => item.VehicleId, StringComparer.Ordinal);
        var html = service.BuildHtml(dataSet, metaByVehicleId, timelineService, today, generatedAt);

        Assert.Contains("<title>Vehimap - Tiskový přehled vozidel</title>", html);
        Assert.Contains("Vytvořeno: 03.04.2026 09:15 | Celkem vozidel: 3", html);
        Assert.Equal(LegacyKnownValues.Categories.Length, CountOccurrences(html, "<h2>"));
        Assert.Contains("V této kategorii není žádné vozidlo.", html);
        Assert.Contains("TK: Do 27 dnů", html);
        Assert.Contains("ZK: Do 27 dnů", html);

        var milenaIndex = html.IndexOf("Milena", StringComparison.Ordinal);
        var bozenaIndex = html.IndexOf("Božena", StringComparison.Ordinal);
        Assert.True(milenaIndex >= 0 && bozenaIndex >= 0 && milenaIndex < bozenaIndex);
    }

    private static int CountOccurrences(string text, string needle)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count += 1;
            index += needle.Length;
        }

        return count;
    }
}
