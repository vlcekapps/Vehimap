using System.Text;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LegacyCalendarExportServiceTests
{
    [Fact]
    public void BuildUpcomingCalendar_creates_ics_content_and_counts_skipped_maintenance_without_due_date()
    {
        var today = new DateOnly(2026, 4, 1);
        var generatedAt = new DateTimeOffset(2026, 4, 1, 12, 30, 0, TimeSpan.Zero);
        var service = new LegacyCalendarExportService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Octavia", "Osobní vozidla", "", "Škoda Octavia", "1AB2345", "2020", "110", "", "05/2026", "", ""),
                new Vehicle("veh_2", "Transit", "Nákladní vozidla", "", "Ford Transit", "2AB2345", "2019", "125", "", "", "", "")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Přezutí", "10.04.2026", "14", "", "")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("plan_1", "veh_1", "Olej", "", "12", "15.04.2025", "10000", true, ""),
                new MaintenancePlan("plan_2", "veh_2", "Filtr", "15000", "", "", "", true, "")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Doklad", "Pojištění", "", "", "04/2026", "200", VehicleRecordAttachmentMode.External, "", "")
            ]
        };

        var result = service.BuildUpcomingCalendar(dataSet, today, generatedAt);

        Assert.NotEmpty(result.Items);
        Assert.Contains(result.Items, item => item.Kind == "technical");
        Assert.Contains(result.Items, item => item.Kind == "custom");
        Assert.Contains(result.Items, item => item.Kind == "record");
        Assert.Equal(1, result.SkippedMaintenanceCount);
        Assert.Contains("BEGIN:VCALENDAR", result.IcsContent);
        Assert.Contains("SUMMARY:Vehimap - Technická kontrola - Octavia", result.IcsContent);
        Assert.Contains("UID:vehimap-technical-veh_1-", result.IcsContent);
    }

    [Fact]
    public void BuildUpcomingCalendar_folds_long_ics_lines_without_losing_escaped_description()
    {
        var today = new DateOnly(2026, 4, 1);
        var generatedAt = new DateTimeOffset(2026, 4, 1, 12, 30, 0, TimeSpan.Zero);
        var service = new LegacyCalendarExportService();
        var longNote = "velmi dlouh\u00e1 pozn\u00e1mka ke kontrole: kapaliny; sv\u011btla, pneumatiky\n"
            + "a dal\u0161\u00ed bezpe\u010dnostn\u00ed prvky p\u0159ed cestou p\u0159es n\u011bkolik zem\u00ed s del\u0161\u00edm popisem pro kalend\u00e1\u0159.";
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Dlouha Octavia", "Osobni vozidla", "", "Skoda Octavia Combi", "1AB2345", "2020", "110", "", "", "", "")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Velmi dlouha kontrola pred cestou", "10.04.2026", "14", "", longNote)
            ]
        };

        var result = service.BuildUpcomingCalendar(dataSet, today, generatedAt);
        var unfolded = result.IcsContent.Replace("\r\n ", string.Empty, StringComparison.Ordinal);

        Assert.Contains("\r\n ", result.IcsContent);
        Assert.All(
            result.IcsContent.Split("\r\n", StringSplitOptions.RemoveEmptyEntries),
            line => Assert.True(Encoding.UTF8.GetByteCount(line) <= 75, $"ICS line is too long: {line}"));
        Assert.Contains("Pozn\u00e1mka: velmi dlouh\u00e1 pozn\u00e1mka", unfolded);
        Assert.Contains("kapaliny\\; sv\u011btla\\, pneumatiky\\na dal\u0161\u00ed", unfolded);
    }
}
