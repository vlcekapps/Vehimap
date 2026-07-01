using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LegacyGlobalSearchServiceTests
{
    private static readonly VehimapDataRoot DataRoot = new(@"C:\vehimap-test", @"C:\vehimap-test\data", true);

    [Fact]
    public void Search_should_find_vehicle_and_record_matches()
    {
        var service = new LegacyGlobalSearchService(new StubAttachmentService());
        var dataSet = CreateDataSet();

        var results = service.Search(DataRoot, dataSet, "Božena");

        Assert.Contains(results, item => item.EntityKind == "Vozidlo" && item.VehicleId == "veh_1");
        Assert.Contains(results, item => item.EntityKind == "Doklad" && item.EntityId == "rec_1");
    }

    [Fact]
    public void Search_should_find_vehicle_meta_fields()
    {
        var service = new LegacyGlobalSearchService(new StubAttachmentService());
        var dataSet = CreateDataSet();

        var tagResults = service.Search(DataRoot, dataSet, "garáž");
        var timingResults = service.Search(DataRoot, dataSet, "Řemen");

        Assert.Contains(tagResults, item => item.EntityKind == "Vozidlo" && item.VehicleId == "veh_1");
        Assert.Contains(timingResults, item => item.EntityKind == "Vozidlo" && item.VehicleId == "veh_1");
    }

    [Fact]
    public void Search_should_find_related_entries_by_vehicle_identity()
    {
        var service = new LegacyGlobalSearchService(new StubAttachmentService());
        var dataSet = CreateDataSet();

        var results = service.Search(DataRoot, dataSet, "Škoda 100");

        Assert.Equal("Vozidlo", results[0].EntityKind);
        Assert.Contains(results, item => item.EntityKind == "Vozidlo" && item.VehicleId == "veh_1");
        Assert.Contains(results, item => item.EntityKind == "Historie" && item.EntityId == "hist_1");
        Assert.Contains(results, item => item.EntityKind == "Tankování" && item.EntityId == "fuel_1");
        Assert.Contains(results, item => item.EntityKind == "Doklad" && item.EntityId == "rec_1");
        Assert.Contains(results, item => item.EntityKind == "Připomínka" && item.EntityId == "rem_1");
        Assert.Contains(results, item => item.EntityKind == "Údržba" && item.EntityId == "mnt_1");
    }

    [Fact]
    public void Search_should_find_timeline_status_texts_without_matching_unrelated_entries()
    {
        var service = new LegacyGlobalSearchService(new StubAttachmentService());
        var dataSet = CreateDataSet();

        var results = service.Search(DataRoot, dataSet, "Po termínu");

        Assert.Contains(results, item => item.EntityKind == "Vozidlo" && item.VehicleId == "veh_1");
        Assert.Contains(results, item => item.EntityKind == "Připomínka" && item.EntityId == "rem_1");
        Assert.DoesNotContain(results, item => item.EntityKind == "Historie" && item.EntityId == "hist_1");
        Assert.DoesNotContain(results, item => item.EntityKind == "Tankování" && item.EntityId == "fuel_1");
    }

    [Fact]
    public void Search_should_ignore_localized_neutral_timeline_status()
    {
        var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US"));
        var service = new LegacyGlobalSearchService(new StubAttachmentService(), new LegacyTimelineService(localizer), localizer);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Cars", "", "Skoda 120", "", "1980", "37", "", "", "", "")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("mnt_1", "veh_1", "Oil service", "", "12", today.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "", true, "")
            ]
        };

        var results = service.Search(DataRoot, dataSet, "No alert");

        Assert.Empty(results);
    }

    [Fact]
    public void Search_should_localize_visible_result_texts_without_changing_navigation_entity_kinds()
    {
        var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US"));
        var service = new LegacyGlobalSearchService(new StubAttachmentService(), new LegacyTimelineService(localizer), localizer);
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
        var dataSet = CreateDataSet();

        var fuelResults = service.Search(DataRoot, dataSet, "FuelSave");
        var fuelResult = Assert.Single(fuelResults.Where(item => item.EntityId == "fuel_1"));
        Assert.Equal("Tankování", fuelResult.EntityKind);
        Assert.Equal("Fuel", fuelResult.SectionLabel);
        Assert.StartsWith("Fuel -", fuelResult.Title, StringComparison.Ordinal);
        Assert.Contains("Full tank", fuelResult.Summary, StringComparison.Ordinal);
        Assert.Contains("$1,200.00", fuelResult.Summary, StringComparison.Ordinal);

        var recordResults = service.Search(DataRoot, dataSet, "asistence.pdf");
        var recordResult = Assert.Single(recordResults.Where(item => item.EntityId == "rec_2"));
        Assert.Equal("Doklad", recordResult.EntityKind);
        Assert.Equal("Documents", recordResult.SectionLabel);
        Assert.Contains("Managed copy", recordResult.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void Search_should_find_managed_attachment_by_file_name()
    {
        var service = new LegacyGlobalSearchService(new StubAttachmentService());
        var dataSet = CreateDataSet();

        var results = service.Search(DataRoot, dataSet, "asistence.pdf");

        var recordResult = Assert.Single(results);
        Assert.Equal("Doklad", recordResult.EntityKind);
        Assert.Equal("rec_2", recordResult.EntityId);
    }

    [Fact]
    public void Search_should_find_fuel_detail_and_station()
    {
        var service = new LegacyGlobalSearchService(new StubAttachmentService());
        var dataSet = CreateDataSet();

        var detailResults = service.Search(DataRoot, dataSet, "FuelSave");
        var stationResults = service.Search(DataRoot, dataSet, "Station 42");

        Assert.Contains(detailResults, item => item.EntityKind == "Tankování" && item.EntityId == "fuel_1");
        Assert.Contains(stationResults, item => item.EntityKind == "Tankování" && item.EntityId == "fuel_1");
    }

    private static VehimapDataSet CreateDataSet() =>
        new()
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Božena", "Osobní vozidla", "Srazové", "Škoda 100", "", "1974", "34", "", "09/2026", "10/2025", "10/2026")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "Veterán", "sraz, garáž", "Benzín", "Bez klimatizace", "Řemen", "Manuální")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "01.04.2026", "Servis", "12345", "1500", "Kontrola brzd")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "02.04.2026", "12400", "32", "1200", true, "Benzín", "Plná nádrž před srazem", "Shell FuelSave Natural 95", "Shell Station 42")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Božena pojistka", "Kooperativa", "10/2025", "10/2026", "1200", VehicleRecordAttachmentMode.External, @"C:\docs\bozena-ruceni.pdf", ""),
                new VehicleRecord("rec_2", "veh_1", "Asistence", "Asistenční karta", "", "", "08/2027", "", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/asistence.pdf", "")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Objednat veteránský sraz", "10.05.2026", "14", "Neopakovat", "Připravit dokumenty")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("mnt_1", "veh_1", "Výměna oleje", "10000", "12", "01.04.2026", "12345", true, "Minerální olej")
            ]
        };

    private sealed class StubAttachmentService : IFileAttachmentService
    {
        public string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath) =>
            Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
