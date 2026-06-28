using Vehimap.Application;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LegacySmartAdvisorServiceTests
{
    [Fact]
    public void BuildSmartAdvisor_returns_empty_state_for_clean_empty_data()
    {
        var service = CreateService();

        var summary = service.BuildSmartAdvisor(new VehimapDataSet(), [], null, new DateOnly(2026, 6, 15));

        Assert.Empty(summary.Items);
        Assert.Equal(0, summary.TotalCount);
        Assert.Contains("nenašel nic naléhavého", summary.Status);
    }

    [Fact]
    public void BuildSmartAdvisor_projects_audit_errors_as_critical_recommendations()
    {
        var service = CreateService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            {
                CreateVehicle("veh_1", "Milena")
            }
        };
        var auditItems = new[]
        {
            new AuditItem(
                AuditSeverity.Error,
                "Příloha",
                "veh_1",
                "Milena",
                "Doklad",
                "rec_1",
                "Chybí spravovaná příloha",
                "U dokladu není dostupný soubor.")
        };

        var summary = service.BuildSmartAdvisor(dataSet, auditItems, null, new DateOnly(2026, 6, 15));

        var item = Assert.Single(summary.Items);
        Assert.Equal(SmartAdvisorPriority.Critical, item.Priority);
        Assert.Equal(SmartAdvisorCategory.Attachments, item.Category);
        Assert.Equal("Doklad", item.EntityKind);
        Assert.Equal("rec_1", item.EntityId);
    }

    [Fact]
    public void BuildSmartAdvisor_reports_overdue_and_upcoming_timeline_items()
    {
        var service = CreateService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            {
                CreateVehicle("veh_1", "Božena", nextTk: "05/2026"),
                CreateVehicle("veh_2", "Drobeček", nextTk: "06/2026")
            }
        };

        var summary = service.BuildSmartAdvisor(dataSet, [], null, new DateOnly(2026, 6, 15));

        Assert.Contains(summary.Items, item =>
            item.VehicleId == "veh_1"
            && item.Priority == SmartAdvisorPriority.Critical
            && item.Title.Contains("Technická kontrola", StringComparison.Ordinal));
        Assert.Contains(summary.Items, item =>
            item.VehicleId == "veh_2"
            && item.Priority == SmartAdvisorPriority.Warning
            && item.Title.Contains("Technická kontrola", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildSmartAdvisor_reports_fuel_analysis_warnings()
    {
        var service = CreateService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            {
                CreateVehicle("veh_1", "Milena")
            },
            FuelEntries =
            {
                new FuelEntry("fuel_1", "veh_1", "01.06.2026", "120000", "abc", "1000", true, "Benzín", "")
            }
        };

        var summary = service.BuildSmartAdvisor(dataSet, [], null, new DateOnly(2026, 6, 15));

        Assert.Contains(summary.Items, item =>
            item.Category == SmartAdvisorCategory.Fuel
            && item.EntityKind == "Tankování"
            && item.EntityId == "fuel_1"
            && item.Title.Contains("Množství", StringComparison.CurrentCultureIgnoreCase));
    }

    [Fact]
    public void BuildSmartAdvisor_reports_cost_per_km_unavailable_when_vehicle_has_costs()
    {
        var service = CreateService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            {
                CreateVehicle("veh_1", "Milena")
            }
        };
        var costs = new CostAnalysisSummary(
            "Test",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            1200m,
            null,
            null,
            0m,
            null,
            1200m,
            null,
            1,
            0,
            1,
            [
                new VehicleCostBreakdown("veh_1", "Milena", "Osobní vozidla", 1200m, 0m, 0m, 1200m, null, null, "Aktivní")
            ]);

        var summary = service.BuildSmartAdvisor(dataSet, [], costs, new DateOnly(2026, 6, 15));

        var item = Assert.Single(summary.Items);
        Assert.Equal(SmartAdvisorCategory.Costs, item.Category);
        Assert.Equal(SmartAdvisorPriority.Recommendation, item.Priority);
        Assert.Equal("Náklady", item.EntityKind);
    }

    private static LegacySmartAdvisorService CreateService() =>
        new(new LegacyTimelineService(), new LegacyFuelAnalysisService());

    private static Vehicle CreateVehicle(string id, string name, string nextTk = "", string greenCardTo = "") =>
        new(
            id,
            name,
            "Osobní vozidla",
            "",
            "Škoda 120",
            "1A2 3456",
            "1980",
            "37",
            "",
            nextTk,
            "",
            greenCardTo);
}
