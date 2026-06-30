using System.Globalization;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LegacyCostAnalysisServiceTests
{
    [Fact]
    public void BuildYearToDateSummary_calculates_costs_distance_and_cost_per_km()
    {
        var today = new DateOnly(2026, 4, 1);
        var service = new LegacyCostAnalysisService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Octavia", "Osobní vozidla", "", "Škoda Octavia", "1AB2345", "2020", "110", "", "05/2027", "", "")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "10.01.2026", "10000", "40", "300", true, "Diesel", "")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "20.02.2026", "Servis", "10400", "100", "")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Doklad", "Pojištění", "", "", "03/2026", "200", VehicleRecordAttachmentMode.External, "", "")
            ]
        };

        var summary = service.BuildYearToDateSummary(dataSet, today);

        Assert.Equal(600m, summary.TotalCost);
        Assert.Equal(400, summary.DistanceKm);
        Assert.Equal(1.5m, summary.CostPerKm);
        Assert.Equal(1, summary.ActiveVehicleCount);
        Assert.Single(summary.Vehicles);
        Assert.Equal("V pořádku", summary.Vehicles[0].Status);
    }

    [Fact]
    public void BuildYearToDateSummary_marks_cost_per_km_unavailable_for_regression()
    {
        var today = new DateOnly(2026, 4, 1);
        var service = new LegacyCostAnalysisService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Passat", "Osobní vozidla", "", "Volkswagen Passat", "1AB2345", "2021", "110", "", "05/2027", "", "")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "10.01.2026", "15000", "40", "300", true, "Nafta", ""),
                new FuelEntry("fuel_2", "veh_1", "15.02.2026", "14900", "42", "350", true, "Nafta", "")
            ]
        };

        var summary = service.BuildYearToDateSummary(dataSet, today);

        Assert.Null(summary.CostPerKm);
        Assert.Equal(1, summary.CostPerKmUnavailableCount);
        Assert.Single(summary.Vehicles);
        Assert.Equal("Nekonzistentní tachometr", summary.Vehicles[0].Status);
        Assert.Null(summary.Vehicles[0].CostPerKm);
    }

    [Fact]
    public void BuildPeriodSummary_filters_requested_range_and_compares_same_period_last_year()
    {
        var service = new LegacyCostAnalysisService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Octavia", "Osobní vozidla", "", "Škoda Octavia", "1AB2345", "2020", "110", "", "05/2027", "", "")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "10.02.2026", "Servis", "10000", "100", ""),
                new VehicleHistoryEntry("hist_2", "veh_1", "20.02.2026", "Servis", "10100", "200", ""),
                new VehicleHistoryEntry("hist_3", "veh_1", "10.03.2026", "Servis", "10200", "900", ""),
                new VehicleHistoryEntry("hist_4", "veh_1", "10.02.2025", "Servis", "9000", "50", ""),
                new VehicleHistoryEntry("hist_5", "veh_1", "20.02.2025", "Servis", "9050", "50", "")
            ]
        };

        var summary = service.BuildPeriodSummary(dataSet, new DateOnly(2026, 2, 28), new DateOnly(2026, 2, 1));

        Assert.Equal(new DateOnly(2026, 2, 1), summary.PeriodStart);
        Assert.Equal(new DateOnly(2026, 2, 28), summary.PeriodEnd);
        Assert.Equal(300m, summary.TotalCost);
        Assert.Equal(100, summary.DistanceKm);
        Assert.Equal(3m, summary.CostPerKm);
        Assert.Equal(100m, summary.PreviousTotalCost);
        Assert.Equal(200m, summary.TotalCostDifference);
        Assert.Single(summary.Vehicles);
    }

    [Fact]
    public void BuildPeriodSummary_uses_supplied_localizer_for_period_and_statuses()
    {
        var service = new LegacyCostAnalysisService(new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US")));
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Octavia", "Passenger cars", "", "Skoda Octavia", "1AB2345", "2020", "110", "", "05/2027", "", "")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "10.02.2026", "Service", "10000", "100", ""),
                new VehicleHistoryEntry("hist_2", "veh_1", "20.02.2026", "Service", "10100", "200", "")
            ]
        };

        var summary = service.BuildPeriodSummary(dataSet, new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28));

        Assert.Equal("From 01.02.2026 to 28.02.2026", summary.PeriodLabel);
        Assert.Single(summary.Vehicles);
        Assert.Equal("OK", summary.Vehicles[0].Status);
    }
}
