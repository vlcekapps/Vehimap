using System.Globalization;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LegacyTimelineServiceTests
{
    [Fact]
    public void BuildVehicleTimeline_combines_multiple_sources_and_sorts_future_before_past()
    {
        var today = new DateOnly(2026, 4, 1);
        var service = new LegacyTimelineService();
        var dataSet = new VehimapDataSet
        {
            Settings =
            {
                Sections =
                {
                    ["notifications"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["technical_reminder_days"] = "31",
                        ["green_card_reminder_days"] = "31",
                        ["maintenance_reminder_days"] = "31",
                        ["maintenance_reminder_km"] = "1000"
                    }
                }
            },
            Vehicles =
            [
                new Vehicle("veh_1", "Octavia", "Osobní vozidla", "", "Škoda Octavia", "1AB2345", "2020", "110", "", "05/2026", "", "06/2026")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "10.03.2026", "Servis", "12000", "100", "Kontrola")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "28.03.2026", "12450", "40", "300", true, "Diesel", "", "Shell FuelSave Diesel", "Shell Station 42")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Přezutí", "05.04.2026", "14", "ročně", "Objednat pneuservis")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Doklad", "Pojištění", "", "", "04/2026", "200", VehicleRecordAttachmentMode.External, "", "")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("plan_1", "veh_1", "Olej", "15000", "12", "15.04.2025", "10000", true, "Roční výměna")
            ]
        };

        var timeline = service.BuildVehicleTimeline(dataSet, "veh_1", today);

        Assert.Contains(timeline, item => item.Kind == "technical");
        Assert.Contains(timeline, item => item.Kind == "green");
        Assert.Contains(timeline, item => item.Kind == "custom");
        Assert.Contains(timeline, item => item.Kind == "record");
        Assert.Contains(timeline, item => item.Kind == "maintenance");
        Assert.Contains(timeline, item => item.Kind == "fuel");
        Assert.Contains(timeline, item => item.Kind == "history");
        Assert.Contains("Shell FuelSave Diesel", timeline.First(item => item.Kind == "fuel").Detail, StringComparison.Ordinal);
        Assert.Contains("Shell Station 42", timeline.First(item => item.Kind == "fuel").Detail, StringComparison.Ordinal);

        Assert.True(timeline[0].IsFuture);
        Assert.True(timeline.TakeWhile(item => item.IsFuture).SequenceEqual(timeline.Where(item => item.IsFuture).OrderBy(item => item.Date)));
        Assert.Equal("05.04.2026", timeline.First(item => item.Kind == "custom").DateText);
        Assert.Equal("Do 14 dnů", timeline.First(item => item.Kind == "maintenance").Status);
    }

    [Fact]
    public void BuildVehicleTimeline_uses_localized_domain_messages()
    {
        var today = new DateOnly(2026, 4, 1);
        var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US"));
        var service = new LegacyTimelineService(localizer);
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
        var dataSet = new VehimapDataSet
        {
            Settings =
            {
                Sections =
                {
                    ["notifications"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["technical_reminder_days"] = "31",
                        ["maintenance_reminder_days"] = "31",
                        ["maintenance_reminder_km"] = "1000"
                    }
                }
            },
            Vehicles =
            [
                new Vehicle("veh_1", "Octavia", "Cars", "", "Skoda Octavia", "1AB2345", "2020", "110", "", "03/2026", "", "")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "10.03.2026", "", "12000", "100", "")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "28.03.2026", "12450", "40", "300", true, "Diesel", "")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "", "", "", "", "04/2026", "", VehicleRecordAttachmentMode.External, "", "")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("plan_1", "veh_1", "", "13000", "12", "15.04.2025", "10000", true, "")
            ]
        };

        var timeline = service.BuildVehicleTimeline(dataSet, "veh_1", today);

        var technical = timeline.First(item => item.Kind == "technical");
        Assert.Equal("Technical inspection", technical.KindLabel);
        Assert.Equal("Next technical inspection", technical.Title);
        Assert.Equal("Overdue", technical.Status);

        var maintenance = timeline.First(item => item.Kind == "maintenance");
        Assert.Equal("Maintenance plan", maintenance.KindLabel);
        Assert.Equal("Service task", maintenance.Title);
        Assert.Equal("15.04.2026 | 23000 km", maintenance.Detail);
        Assert.Equal("In 14 days", maintenance.Status);

        var record = timeline.First(item => item.Kind == "record");
        Assert.Equal("Document", record.KindLabel);
        Assert.Equal("Document: Untitled", record.Title);

        var history = timeline.First(item => item.Kind == "history");
        Assert.Equal("History", history.KindLabel);
        Assert.Equal("History", history.Title);

        var fuel = timeline.First(item => item.Kind == "fuel");
        Assert.Equal("Fuel", fuel.KindLabel);
        Assert.Equal("Fuel", fuel.Title);
        Assert.Equal("$300.00", fuel.Status);
    }
}
