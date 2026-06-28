using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LegacyServiceBookServiceTests
{
    [Fact]
    public void Build_vehicle_service_book_includes_history_maintenance_and_service_records()
    {
        var service = new LegacyServiceBookService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "", "06/2026")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "01.01.2026", "Servis", "100000", "2500", "Olej"),
                new VehicleHistoryEntry("hist_2", "veh_1", "15.02.2026", "Oprava", "101000", "1500 Kč", "Brzdy")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "01.03.2026", "102000", "35", "1600", true, "Natural 95", "")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("mnt_1", "veh_1", "Motorový olej", "15000", "12", "01.01.2026", "100000", true, "Syntetika")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Servisní dokument", "Faktura servis", "Autoservis", "02/2026", "02/2026", "4000", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/faktura.pdf", "Práce"),
                new VehicleRecord("rec_2", "veh_1", "Povinné ručení", "Pojistka", "Pojišťovna", "01/2026", "01/2027", "2000", VehicleRecordAttachmentMode.External, "", "")
            ]
        };

        var summary = service.BuildVehicleServiceBook(dataSet, "veh_1", new DateOnly(2026, 3, 1));

        Assert.Equal("Milena", summary.VehicleName);
        Assert.Equal("102000 km", summary.CurrentOdometer);
        Assert.Equal(4000m, summary.TotalHistoryCost);
        Assert.Equal(["hist_2", "hist_1"], summary.HistoryEntries.Select(item => item.Id));
        Assert.Single(summary.MaintenancePlans);
        Assert.Contains("Za 13000 km", summary.MaintenancePlans[0].Status);
        var record = Assert.Single(summary.Records);
        Assert.Equal("rec_1", record.Id);
        Assert.Equal("Spravovaná kopie", record.AttachmentMode);
        Assert.DoesNotContain(summary.Records, item => item.Id == "rec_2");
        Assert.Contains("Záznamy historie: 2", summary.Status);
    }

    [Fact]
    public void Build_vehicle_service_book_explains_empty_vehicle()
    {
        var service = new LegacyServiceBookService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Božena", "Osobní vozidla", "", "Škoda 100", "", "", "", "", "", "", "")
            ]
        };

        var summary = service.BuildVehicleServiceBook(dataSet, "veh_1", new DateOnly(2026, 3, 1));

        Assert.Empty(summary.HistoryEntries);
        Assert.Empty(summary.MaintenancePlans);
        Assert.Empty(summary.Records);
        Assert.Contains("zatím nemá žádné položky", summary.Status);
        Assert.Equal("neznámý", summary.CurrentOdometer);
    }
}
