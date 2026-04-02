using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopProjectionAndNavigationServiceTests
{
    [Fact]
    public void Navigation_coordinator_routes_timeline_record_to_record_tab()
    {
        var coordinator = new DesktopNavigationCoordinator();
        var item = new VehicleTimelineItemViewModel(
            "record",
            "Doklad",
            "08/2099",
            "Asistence",
            "Platnost dokladu",
            string.Empty,
            "Octavia",
            "veh_1",
            "rec_2",
            true,
            string.Empty);

        var plan = coordinator.BuildForTimeline(item);

        Assert.Equal("veh_1", plan.VehicleId);
        Assert.Equal(DesktopTabIndexes.Record, plan.TabIndex);
        Assert.Equal(DesktopFocusTarget.RecordList, plan.FocusTarget);
        Assert.Equal(DesktopNavigationSelectionKind.Record, plan.SelectionKind);
        Assert.Equal("rec_2", plan.EntityId);
    }

    [Fact]
    public void Navigation_coordinator_routes_entity_reminder_to_reminder_tab()
    {
        var coordinator = new DesktopNavigationCoordinator();

        var plan = coordinator.BuildForEntity("veh_1", "Připomínka", "rem_1");

        Assert.Equal(DesktopTabIndexes.Reminder, plan.TabIndex);
        Assert.Equal(DesktopFocusTarget.ReminderList, plan.FocusTarget);
        Assert.Equal(DesktopNavigationSelectionKind.Reminder, plan.SelectionKind);
        Assert.Equal("rem_1", plan.EntityId);
    }

    [Fact]
    public void Projection_service_builds_managed_record_with_resolved_path_and_available_state()
    {
        var projectionService = new DesktopProjectionService();
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-projection-tests", Guid.NewGuid().ToString("N"));
        var dataRoot = new VehimapDataRoot(tempRoot, Path.Combine(tempRoot, "data"), true);
        Directory.CreateDirectory(Path.Combine(dataRoot.DataPath, "attachments", "veh_1"));
        var managedFile = Path.Combine(dataRoot.DataPath, "attachments", "veh_1", "pojisteni.pdf");
        File.WriteAllText(managedFile, "test");

        var dataSet = new VehimapDataSet
        {
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Pojištění", "", "05/2025", "05/2026", "2000", VehicleRecordAttachmentMode.Managed, @"attachments/veh_1/pojisteni.pdf", "Platný doklad")
            ]
        };

        var projection = projectionService.BuildRecords(
            dataRoot,
            dataSet,
            "veh_1",
            relativePath => Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));

        var item = Assert.Single(projection.Items);
        Assert.Equal("Spravovaná kopie", item.AttachmentMode);
        Assert.Equal("Soubor dostupný", item.AttachmentState);
        Assert.Equal(managedFile, item.ResolvedPath);
        Assert.True(item.FileExists);
        Assert.Contains("1 dokladů", projection.Summary);
    }

    [Fact]
    public void Projection_service_filters_timeline_by_future_and_search_text()
    {
        var projectionService = new DesktopProjectionService();
        var timelineService = new LegacyTimelineService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Octavia", "Osobní vozidla", "Rodinné auto", "Škoda Octavia", "1AB2345", "2020", "110", "", "05/2027", "05/2025", "05/2026")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Objednat servis", "01.12.2099", "30", "Ročně", "Zavolat servisu"),
                new VehicleReminder("rem_2", "veh_1", "Starý termín", "01.01.2020", "30", "Ročně", "Historie")
            ]
        };

        var projection = projectionService.BuildTimeline(
            dataSet,
            timelineService,
            "veh_1",
            new DateOnly(2026, 4, 2),
            "Budoucí",
            "servis");

        var item = Assert.Single(projection.Items);
        Assert.Equal("rem_1", item.EntryId);
        Assert.Contains("Po filtru zobrazeno: 1", projection.Summary);
    }
}
