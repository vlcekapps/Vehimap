using Vehimap.Application.Abstractions;
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
    public void Search_should_find_managed_attachment_by_file_name()
    {
        var service = new LegacyGlobalSearchService(new StubAttachmentService());
        var dataSet = CreateDataSet();

        var results = service.Search(DataRoot, dataSet, "asistence.pdf");

        var recordResult = Assert.Single(results);
        Assert.Equal("Doklad", recordResult.EntityKind);
        Assert.Equal("rec_2", recordResult.EntityId);
    }

    private static VehimapDataSet CreateDataSet() =>
        new()
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Božena", "Osobní vozidla", "Srazové", "Škoda 100", "", "1974", "34", "", "09/2026", "10/2025", "10/2026")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Božena pojistka", "Kooperativa", "10/2025", "10/2026", "1200", VehicleRecordAttachmentMode.External, @"C:\docs\bozena-ruceni.pdf", ""),
                new VehicleRecord("rec_2", "veh_1", "Asistence", "Asistenční karta", "", "", "08/2027", "", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/asistence.pdf", "")
            ]
        };

    private sealed class StubAttachmentService : IFileAttachmentService
    {
        public string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath) =>
            Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
