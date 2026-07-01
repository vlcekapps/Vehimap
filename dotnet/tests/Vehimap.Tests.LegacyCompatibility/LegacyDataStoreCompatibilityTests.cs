// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;
using Xunit;

namespace Vehimap.Tests.LegacyCompatibility;

public sealed class LegacyDataStoreCompatibilityTests
{
    [Fact]
    public async Task Load_reports_file_name_path_and_parser_detail_for_malformed_legacy_file()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-malformed-" + Guid.NewGuid());
        var dataRoot = new Vehimap.Application.Abstractions.VehimapDataRoot(tempRoot, Path.Combine(tempRoot, "data"), true);
        var store = new LegacyVehimapDataStore();
        var vehiclesPath = Path.Combine(dataRoot.DataPath, "vehicles.tsv");

        try
        {
            Directory.CreateDirectory(dataRoot.DataPath);
            await File.WriteAllTextAsync(vehiclesPath, "# Vehimap data v4\njen-jedno-pole\n");

            var exception = await Assert.ThrowsAsync<LegacyDataLoadException>(() => store.LoadAsync(dataRoot));

            Assert.Equal("vehicles.tsv", exception.FileName);
            Assert.Equal(vehiclesPath, exception.FilePath);
            Assert.Contains("vehicles.tsv", exception.Message, StringComparison.Ordinal);
            Assert.Contains(vehiclesPath, exception.Message, StringComparison.Ordinal);
            Assert.Contains("Řádek vozidel", exception.Message, StringComparison.Ordinal);
            Assert.IsType<FormatException>(exception.InnerException);
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
    public async Task Load_maps_fuel_v1_note_and_save_roundtrip_writes_fuel_v2_detail_and_station()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-fuel-compat-" + Guid.NewGuid());
        var dataRoot = new Vehimap.Application.Abstractions.VehimapDataRoot(tempRoot, Path.Combine(tempRoot, "data"), true);
        var store = new LegacyVehimapDataStore();
        var fuelPath = Path.Combine(dataRoot.DataPath, "fuel.tsv");

        try
        {
            Directory.CreateDirectory(dataRoot.DataPath);
            await File.WriteAllTextAsync(
                fuelPath,
                "# Vehimap fuel v1\nfuel_1\tveh_1\t20.10.2026\t123789\t38.5\t1890\t1\tBenzín\tPůvodní poznámka\n");

            var loadedV1 = await store.LoadAsync(dataRoot);
            var legacyFuel = Assert.Single(loadedV1.FuelEntries);

            Assert.Equal("Původní poznámka", legacyFuel.Note);
            Assert.Equal(string.Empty, legacyFuel.FuelDetail);
            Assert.Equal(string.Empty, legacyFuel.Station);

            loadedV1.FuelEntries.Clear();
            loadedV1.FuelEntries.Add(new FuelEntry(
                "fuel_2",
                "veh_1",
                "21.10.2026",
                "123900",
                "40",
                "1999",
                true,
                "Benzín",
                "Nová poznámka",
                "Natural 98 V-Power",
                "Shell Brno Vídeňská"));

            await store.SaveAsync(dataRoot, loadedV1);
            var savedFuelContent = await File.ReadAllTextAsync(fuelPath);
            var loadedV2 = await store.LoadAsync(dataRoot);
            var savedFuel = Assert.Single(loadedV2.FuelEntries);

            Assert.StartsWith("# Vehimap fuel v2", savedFuelContent, StringComparison.Ordinal);
            Assert.Contains("Natural 98 V-Power", savedFuelContent, StringComparison.Ordinal);
            Assert.Contains("Shell Brno Vídeňská", savedFuelContent, StringComparison.Ordinal);
            Assert.Equal("Nová poznámka", savedFuel.Note);
            Assert.Equal("Natural 98 V-Power", savedFuel.FuelDetail);
            Assert.Equal("Shell Brno Vídeňská", savedFuel.Station);
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
                new VehicleRecord("rec_1", "veh_1", "Doklad", "TP", "MDČR", "", "", "", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/tp.pdf", ""),
                new VehicleRecord("rec_2", "veh_1", "Doklad", "Chybějící příloha", "", "", "", "", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/chybi.pdf", "")
            ]
        };

        var backupPath = Path.Combine(tempRoot, "vehimap.vehimapbak");
        var restoreRoot = new Vehimap.Application.Abstractions.VehimapDataRoot(Path.Combine(tempRoot, "restore"), Path.Combine(tempRoot, "restore", "data"), true);

        try
        {
            var exportResult = await backupService.ExportAsync(backupPath, dataRoot, dataSet);
            var imported = await backupService.ImportAsync(backupPath);
            var restoreResult = await backupService.RestoreAsync(restoreRoot, imported);
            var restored = await store.LoadAsync(restoreRoot);

            Assert.Equal(backupPath, exportResult.BackupPath);
            Assert.Equal(1, exportResult.IncludedManagedAttachmentCount);
            Assert.Equal(1, exportResult.MissingManagedAttachmentCount);
            Assert.Single(imported.Attachments);
            Assert.Null(restoreResult.PreRestoreBackupPath);
            Assert.Equal(1, restoreResult.RestoredAttachmentCount);
            Assert.Equal(2, restored.Records.Count);
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

    [Fact]
    public async Task Import_backup_reports_path_for_missing_backup_file()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-missing-backup-" + Guid.NewGuid());
        var backupPath = Path.Combine(tempRoot, "missing.vehimapbak");
        var backupService = new LegacyBackupService();

        try
        {
            var exception = await Assert.ThrowsAsync<LegacyBackupException>(() => backupService.ImportAsync(backupPath));

            Assert.Equal(Path.GetFullPath(backupPath), exception.BackupPath);
            Assert.Contains(Path.GetFullPath(backupPath), exception.Message, StringComparison.Ordinal);
            Assert.Contains("Zálohu se nepodařilo načíst", exception.Message, StringComparison.Ordinal);
            Assert.IsAssignableFrom<IOException>(exception.InnerException);
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
    public async Task Import_backup_reports_path_and_parser_detail_for_invalid_header()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-invalid-backup-" + Guid.NewGuid());
        var backupPath = Path.Combine(tempRoot, "broken.vehimapbak");
        var backupService = new LegacyBackupService();

        try
        {
            Directory.CreateDirectory(tempRoot);
            await File.WriteAllTextAsync(backupPath, "# Neni Vehimap backup\nsettings_length=0\nvehicles_length=0\n\n");

            var exception = await Assert.ThrowsAsync<LegacyBackupException>(() => backupService.ImportAsync(backupPath));

            Assert.Equal(Path.GetFullPath(backupPath), exception.BackupPath);
            Assert.Contains(Path.GetFullPath(backupPath), exception.Message, StringComparison.Ordinal);
            Assert.Contains("formátu zálohy Vehimap", exception.Message, StringComparison.Ordinal);
            Assert.IsType<FormatException>(exception.InnerException);
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
    public async Task Import_backup_uses_configured_localizer_for_wrapper_and_parser_errors()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-invalid-backup-en-" + Guid.NewGuid());
        var backupPath = Path.Combine(tempRoot, "broken.vehimapbak");
        var english = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));
        var backupService = new LegacyBackupService(english);

        try
        {
            Directory.CreateDirectory(tempRoot);
            await File.WriteAllTextAsync(backupPath, "# Not a Vehimap backup\nsettings_length=0\nvehicles_length=0\n\n");

            var exception = await Assert.ThrowsAsync<LegacyBackupException>(() => backupService.ImportAsync(backupPath));

            Assert.Equal(Path.GetFullPath(backupPath), exception.BackupPath);
            Assert.Contains("Backup could not be loaded", exception.Message, StringComparison.Ordinal);
            Assert.Contains("The file is not a Vehimap backup", exception.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("Zálohu", exception.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("Soubor není", exception.Message, StringComparison.Ordinal);
            Assert.IsType<FormatException>(exception.InnerException);
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
    public async Task Import_backup_reports_attachment_line_for_invalid_base64()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-invalid-attachment-backup-" + Guid.NewGuid());
        var backupPath = Path.Combine(tempRoot, "broken-attachment.vehimapbak");
        var backupService = new LegacyBackupService();
        var attachments = "# Vehimap attachments v1\nattachments/veh_1/tp.pdf\t%%%neni-base64%%%\n";

        try
        {
            Directory.CreateDirectory(tempRoot);
            await File.WriteAllTextAsync(backupPath, BuildBackupContent(attachments));

            var exception = await Assert.ThrowsAsync<LegacyBackupException>(() => backupService.ImportAsync(backupPath));

            Assert.Equal(Path.GetFullPath(backupPath), exception.BackupPath);
            Assert.Contains(Path.GetFullPath(backupPath), exception.Message, StringComparison.Ordinal);
            Assert.Contains("Řádek příloh 2", exception.Message, StringComparison.Ordinal);
            Assert.Contains("neplatný obsah souboru", exception.Message, StringComparison.Ordinal);
            Assert.IsType<FormatException>(exception.InnerException);
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
    public async Task Import_backup_uses_configured_localizer_for_attachment_parser_errors()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-invalid-attachment-backup-en-" + Guid.NewGuid());
        var backupPath = Path.Combine(tempRoot, "broken-attachment.vehimapbak");
        var english = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));
        var backupService = new LegacyBackupService(english);
        var attachments = "# Vehimap attachments v1\nattachments/veh_1/tp.pdf\t%%%invalid-base64%%%\n";

        try
        {
            Directory.CreateDirectory(tempRoot);
            await File.WriteAllTextAsync(backupPath, BuildBackupContent(attachments));

            var exception = await Assert.ThrowsAsync<LegacyBackupException>(() => backupService.ImportAsync(backupPath));

            Assert.Equal(Path.GetFullPath(backupPath), exception.BackupPath);
            Assert.Contains("The attachments row 2 contains invalid file content.", exception.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("Řádek příloh", exception.Message, StringComparison.Ordinal);
            Assert.IsType<FormatException>(exception.InnerException);
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
    public async Task Import_backup_rejects_unsafe_managed_attachment_path()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-unsafe-attachment-path-" + Guid.NewGuid());
        var backupPath = Path.Combine(tempRoot, "unsafe-attachment.vehimapbak");
        var english = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.EnglishLanguage));
        var backupService = new LegacyBackupService(english);
        var attachments = "# Vehimap attachments v1\n../outside.txt\tAQID\n";
        var outsidePath = Path.Combine(tempRoot, "outside.txt");

        try
        {
            Directory.CreateDirectory(tempRoot);
            await File.WriteAllTextAsync(backupPath, BuildBackupContent(attachments));

            var exception = await Assert.ThrowsAsync<LegacyBackupException>(() => backupService.ImportAsync(backupPath));

            Assert.Equal(Path.GetFullPath(backupPath), exception.BackupPath);
            Assert.Contains("unsafe managed attachment path", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(outsidePath));
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
    public async Task Restore_backup_rejects_unsafe_managed_attachment_path_without_writing_outside_data()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-unsafe-restore-attachment-" + Guid.NewGuid());
        var dataRoot = new Vehimap.Application.Abstractions.VehimapDataRoot(tempRoot, Path.Combine(tempRoot, "data"), true);
        var backupService = new LegacyBackupService();
        var outsidePath = Path.GetFullPath(Path.Combine(dataRoot.DataPath, "..", "outside.txt"));

        try
        {
            var bundle = new VehimapBackupBundle(
                new VehimapDataSet(),
                [new ManagedAttachment("../outside.txt", [1, 2, 3])]);

            await Assert.ThrowsAsync<InvalidDataException>(() => backupService.RestoreAsync(dataRoot, bundle));

            Assert.False(File.Exists(outsidePath));
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
    public async Task Restore_creates_import_backup_with_current_files_and_attachments()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-import-backup-" + Guid.NewGuid());
        var dataRoot = new Vehimap.Application.Abstractions.VehimapDataRoot(tempRoot, Path.Combine(tempRoot, "data"), true);
        var store = new LegacyVehimapDataStore();
        var backupService = new LegacyBackupService();

        var currentDataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_old", "Puvodni", "Osobni vozidla", "", "Skoda 100", "ABC1234", "1975", "35", "", "05/2025", "", "")
            ],
            Records =
            [
                new VehicleRecord("rec_old", "veh_old", "Doklad", "Stary TP", "", "", "", "", VehicleRecordAttachmentMode.Managed, "attachments/veh_old/old.pdf", "")
            ]
        };

        var importedDataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_new", "Importovane", "Osobni vozidla", "", "Skoda 120", "DEF5678", "1985", "40", "", "06/2026", "", "")
            ],
            Records =
            [
                new VehicleRecord("rec_new", "veh_new", "Doklad", "Novy TP", "", "", "", "", VehicleRecordAttachmentMode.Managed, "attachments/veh_new/imported.pdf", "")
            ]
        };

        try
        {
            await store.SaveAsync(dataRoot, currentDataSet);
            Directory.CreateDirectory(Path.Combine(dataRoot.DataPath, "attachments", "veh_old"));
            await File.WriteAllBytesAsync(Path.Combine(dataRoot.DataPath, "attachments", "veh_old", "old.pdf"), [10, 11]);

            var bundle = new VehimapBackupBundle(
                importedDataSet,
                [new ManagedAttachment("attachments/veh_new/imported.pdf", [20, 21])]);

            var restoreResult = await backupService.RestoreAsync(dataRoot, bundle);
            var restored = await store.LoadAsync(dataRoot);

            var importBackupRoot = Path.Combine(dataRoot.DataPath, "import-backups");
            var importBackupDirectory = Assert.Single(Directory.GetDirectories(importBackupRoot));
            var backedUpVehicles = await File.ReadAllTextAsync(Path.Combine(importBackupDirectory, "vehicles.tsv"));
            var backedUpAttachment = await File.ReadAllBytesAsync(Path.Combine(importBackupDirectory, "attachments", "veh_old", "old.pdf"));
            var restoredAttachment = await File.ReadAllBytesAsync(Path.Combine(dataRoot.DataPath, "attachments", "veh_new", "imported.pdf"));

            Assert.Contains("Puvodni", backedUpVehicles, StringComparison.Ordinal);
            Assert.Equal(importBackupDirectory, restoreResult.PreRestoreBackupPath);
            Assert.Equal(1, restoreResult.RestoredAttachmentCount);
            Assert.Equal([10, 11], backedUpAttachment);
            Assert.Single(restored.Vehicles);
            Assert.Equal("Importovane", restored.Vehicles[0].Name);
            Assert.False(File.Exists(Path.Combine(dataRoot.DataPath, "attachments", "veh_old", "old.pdf")));
            Assert.Equal([20, 21], restoredAttachment);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    private static string BuildBackupContent(string attachmentsContent)
    {
        const string settings = "[app]\n";
        const string vehicles = "# Vehimap data v4\n";
        const string history = "# Vehimap history v1\n";
        const string fuel = "# Vehimap fuel v1\n";
        const string records = "# Vehimap records v2\n";
        const string meta = "# Vehimap meta v2\n";
        const string reminders = "# Vehimap reminders v1\n";
        const string maintenance = "# Vehimap maintenance v1\n";

        var header = string.Join('\n',
            "# Vehimap backup v6",
            $"settings_length={settings.Length}",
            $"vehicles_length={vehicles.Length}",
            $"history_length={history.Length}",
            $"fuel_length={fuel.Length}",
            $"records_length={records.Length}",
            $"meta_length={meta.Length}",
            $"reminders_length={reminders.Length}",
            $"maintenance_length={maintenance.Length}",
            $"attachments_length={attachmentsContent.Length}");

        return $"{header}\n\n{settings}{vehicles}{history}{fuel}{records}{meta}{reminders}{maintenance}{attachmentsContent}";
    }
}
