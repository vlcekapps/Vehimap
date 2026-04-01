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
}
