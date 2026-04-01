using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;
using Xunit;

namespace Vehimap.Tests.LegacyCompatibility;

public sealed class LegacyDataStoreCompatibilityTests
{
    [Fact]
    public async Task Save_and_load_roundtrip_preserves_records_v2_and_managed_attachments()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-compat-" + Guid.NewGuid());
        var dataRoot = new Vehimap.Application.Abstractions.VehimapDataRoot(tempRoot, Path.Combine(tempRoot, "data"), true);
        var store = new LegacyVehimapDataStore();

        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Octavia", "Osobní vozidla", "Rodinné auto", "Škoda Octavia", "1AB2345", "2020", "110", "05/2024", "05/2026", "05/2025", "05/2026")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Kooperativa", "Kooperativa", "05/2025", "05/2026", "5500", VehicleRecordAttachmentMode.External, @"C:\docs\zelena-karta.pdf", "externí"),
                new VehicleRecord("rec_2", "veh_1", "Doklad", "TP", "MDČR", "05/2025", "05/2028", "", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/tp.pdf", "spravovaná kopie")
            ]
        };

        try
        {
            await store.SaveAsync(dataRoot, dataSet);
            var loaded = await store.LoadAsync(dataRoot);

            Assert.Single(loaded.Vehicles);
            Assert.Equal(2, loaded.Records.Count);
            Assert.Equal(VehicleRecordAttachmentMode.External, loaded.Records[0].AttachmentMode);
            Assert.Equal(VehicleRecordAttachmentMode.Managed, loaded.Records[1].AttachmentMode);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public async Task Backup_roundtrip_restores_dataset_and_attachments()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-backup-" + Guid.NewGuid());
        var appRoot = Path.Combine(tempRoot, "app");
        var dataRoot = new Vehimap.Application.Abstractions.VehimapDataRoot(appRoot, Path.Combine(appRoot, "data"), true);
        var store = new LegacyVehimapDataStore();
        var backupService = new LegacyBackupService();

        Directory.CreateDirectory(Path.Combine(dataRoot.DataPath, "attachments", "veh_1"));
        await File.WriteAllBytesAsync(Path.Combine(dataRoot.DataPath, "attachments", "veh_1", "tp.pdf"), [1, 2, 3, 4]);

        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Octavia", "Osobní vozidla", "", "Škoda Octavia", "1AB2345", "2020", "110", "", "05/2026", "", "")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Doklad", "TP", "MDČR", "", "", "", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/tp.pdf", "")
            ]
        };

        var backupPath = Path.Combine(tempRoot, "vehimap.vehimapbak");
        var restoreRoot = new Vehimap.Application.Abstractions.VehimapDataRoot(Path.Combine(tempRoot, "restore"), Path.Combine(tempRoot, "restore", "data"), true);

        try
        {
            await backupService.ExportAsync(backupPath, dataRoot, dataSet);
            var imported = await backupService.ImportAsync(backupPath);
            await backupService.RestoreAsync(restoreRoot, imported);
            var restored = await store.LoadAsync(restoreRoot);

            Assert.Single(imported.Attachments);
            Assert.Single(restored.Records);
            Assert.True(File.Exists(Path.Combine(restoreRoot.DataPath, "attachments", "veh_1", "tp.pdf")));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }
}
