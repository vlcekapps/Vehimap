// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application;
using Vehimap.Application.Models;
using Vehimap.Desktop.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopCostExportServiceTests
{
    [Fact]
    public void Build_fleet_summary_tsv_exports_cost_breakdown()
    {
        var service = new DesktopCostExportService();
        var summary = BuildSummary();

        var content = service.BuildFleetSummaryTsv(summary);

        Assert.Contains("Vozidlo\tKategorie\tPalivo\tHistorie\tDoklady\tCelkem\tUjeto\tCena / vzdálenost\tStav", content);
        Assert.Contains("Milena\tOsobní vozidla\t350,00 Kč\t150,00 Kč\t200,00 Kč\t700,00 Kč\t150,0 km\t4,67 Kč/km\tV pořádku", content);
    }

    [Fact]
    public void Build_vehicle_detail_tsv_exports_period_items()
    {
        var service = new DesktopCostExportService();
        var dataSet = BuildDataSet();

        var content = service.BuildVehicleDetailTsv(dataSet, "veh_1", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        Assert.Contains("Tankování", content);
        Assert.Contains("Historie a servis", content);
        Assert.Contains("Doklady a pojištění", content);
        Assert.Contains("Natural 95", content);
        Assert.Contains("Povinné ručení", content);
        Assert.Contains("Množství: 42,00 l", content);
        Assert.Contains("Tachometr: 10150 km", content);
    }

    [Fact]
    public void Build_vehicle_report_html_escapes_values_and_contains_detail_rows()
    {
        var service = new DesktopCostExportService();
        var dataSet = BuildDataSet();
        dataSet.Vehicles[0] = dataSet.Vehicles[0] with { Name = "Milena <test>" };

        var html = service.BuildVehicleReportHtml(dataSet, BuildSummary(), "veh_1", new DateTime(2026, 6, 19, 20, 0, 0));

        Assert.Contains("Milena &lt;test&gt;", html);
        Assert.Contains("Souhrn obdob&#237;", html);
        Assert.Contains("<table><thead><tr><th>", html);
        Assert.Contains("Natural 95", html);
        Assert.Contains("Povinn", html);
    }

    [Fact]
    public void Build_vehicle_report_html_formats_money_with_selected_currency()
    {
        var service = new DesktopCostExportService();
        service.ApplySupportedSettings(new DesktopSupportedSettingsSnapshot(
            30,
            30,
            31,
            1000,
            false,
            false,
            false,
            false,
            1,
            30,
            "en-US",
            "comma",
            "dot",
            "mi",
            "us_gal",
            "USD"));

        var html = service.BuildVehicleReportHtml(BuildDataSet(), BuildSummary(), "veh_1", new DateTime(2026, 6, 19, 20, 0, 0));

        Assert.Contains("$700.00", html);
        Assert.Contains("93.2 mi", html);
        Assert.Contains("$7.51/mi", html);
        Assert.DoesNotContain("Kč", html);
    }

    [Fact]
    public void Build_tsv_exports_use_selected_language_currency_and_units()
    {
        var service = new DesktopCostExportService();
        service.ApplySupportedSettings(new DesktopSupportedSettingsSnapshot(
            30,
            30,
            31,
            1000,
            false,
            false,
            false,
            false,
            1,
            30,
            "en-US",
            "comma",
            "dot",
            "mi",
            "us_gal",
            "USD"));

        var fleet = service.BuildFleetSummaryTsv(BuildSummary());
        var detail = service.BuildVehicleDetailTsv(BuildDataSet(), "veh_1", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        Assert.Contains("Vehicle\tCategory\tFuel\tHistory\tDocuments\tTotal\tDistance\tCost / distance\tStatus", fleet);
        Assert.Contains("$700.00", fleet);
        Assert.Contains("93.2 mi", fleet);
        Assert.Contains("$7.51/mi", fleet);
        Assert.Contains("Fuel\t", detail);
        Assert.Contains("Volume: 11.10 US gal", detail);
        Assert.Contains("Odometer: 6,307 mi", detail);
        Assert.DoesNotContain("Kč", fleet);
        Assert.DoesNotContain("Kč", detail);
    }

    private static CostAnalysisSummary BuildSummary()
    {
        return new CostAnalysisSummary(
            "Od 01.01.2026 do 31.12.2026",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            700m,
            150,
            4.6667m,
            0m,
            null,
            700m,
            null,
            1,
            0,
            0,
            [
                new VehicleCostBreakdown("veh_1", "Milena", "Osobní vozidla", 350m, 150m, 200m, 700m, 150, 4.6667m, "V pořádku")
            ]);
    }

    private static VehimapDataSet BuildDataSet()
    {
        return new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "", "06/2026")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "15.01.2026", "10150", "42", "350", true, "Natural 95", "Plná")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "10.01.2026", "Servis", "10000", "150", "Olej")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Povinné ručení", "Kooperativa", "", "03/2026", "200", VehicleRecordAttachmentMode.External, @"C:\doklady\pojistka.pdf", "Smlouva")
            ]
        };
    }
}
