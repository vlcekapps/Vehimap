// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Net;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopPrintableVehicleReportServiceTests
{
    [Fact]
    public void Build_file_name_should_be_stable_and_html()
    {
        var service = new DesktopPrintableVehicleReportService();

        var fileName = service.BuildFileName(new DateTime(2026, 4, 3, 9, 15, 0));

        Assert.Equal("vehimap-tiskovy-prehled-2026-04-03.html", fileName);
    }

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
        var decodedHtml = WebUtility.HtmlDecode(html);

        Assert.Contains("<title>Vehimap - Tiskový přehled vozidel</title>", decodedHtml);
        Assert.Contains("<html lang=\"cs\">", html);
        Assert.Contains("Vytvořeno:", decodedHtml);
        Assert.Contains("Celkem vozidel: 3", decodedHtml);
        Assert.Contains("<th>Název</th>", decodedHtml);
        Assert.Contains("<th>Zelená karta do</th>", decodedHtml);
        Assert.Equal(LegacyKnownValues.Categories.Length, CountOccurrences(html, "<h2>"));
        Assert.Contains("V této kategorii není žádné vozidlo.", decodedHtml);
        Assert.Contains("TK: Do 27 dnů", decodedHtml);
        Assert.Contains("ZK: Do 27 dnů", decodedHtml);

        var milenaIndex = decodedHtml.IndexOf("Milena", StringComparison.Ordinal);
        var bozenaIndex = decodedHtml.IndexOf("Božena", StringComparison.Ordinal);
        Assert.True(milenaIndex >= 0 && bozenaIndex >= 0 && milenaIndex < bozenaIndex);
    }

    [Fact]
    public void Build_html_should_use_english_resources_when_requested()
    {
        var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US"));
        var service = new DesktopPrintableVehicleReportService(localizer);
        var timelineService = new LegacyTimelineService(localizer);
        var generatedAt = new DateTime(2026, 4, 3, 9, 15, 0);
        var today = DateOnly.FromDateTime(generatedAt);
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Family car", "Skoda 120L", "1AB2345", "1988", "43", "", "04/2026", "", "04/2026")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "In service", "Family", "Petrol", "", "", "")
            ]
        };
        dataSet.Settings.SetValue("notifications", "technical_reminder_days", "30");
        dataSet.Settings.SetValue("notifications", "green_card_reminder_days", "30");
        var metaByVehicleId = dataSet.VehicleMetaEntries.ToDictionary(item => item.VehicleId, StringComparer.Ordinal);

        var html = service.BuildHtml(dataSet, metaByVehicleId, timelineService, today, generatedAt);
        var decodedHtml = WebUtility.HtmlDecode(html);

        Assert.Equal("vehimap-printable-overview-2026-04-03.html", service.BuildFileName(generatedAt));
        Assert.Contains("<html lang=\"en\">", html);
        Assert.Contains("<title>Vehimap - Printable vehicle overview</title>", decodedHtml);
        Assert.Contains("Generated:", decodedHtml);
        Assert.Contains("Vehicles total: 1", decodedHtml);
        Assert.Contains("<th>Name</th>", decodedHtml);
        Assert.Contains("<th>Green card valid to</th>", decodedHtml);
        Assert.Contains("There is no vehicle in this category.", decodedHtml);
        Assert.Contains("Inspection: In 27 days", decodedHtml);
        Assert.Contains("Green card: In 27 days", decodedHtml);
        Assert.DoesNotContain("Tiskový přehled", decodedHtml);
        Assert.DoesNotContain("Vytvořeno:", decodedHtml);
        Assert.DoesNotContain("TK: Do 27 dnů", decodedHtml);
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
