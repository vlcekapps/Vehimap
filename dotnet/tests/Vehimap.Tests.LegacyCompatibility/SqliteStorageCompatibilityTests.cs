using Vehimap.Application.Abstractions;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;
using Vehimap.Storage.Sqlite;
using Xunit;

namespace Vehimap.Tests.LegacyCompatibility;

public sealed class SqliteStorageCompatibilityTests
{
    private static readonly string[] LegacyFileNames =
    [
        "vehicles.tsv",
        "history.tsv",
        "fuel.tsv",
        "records.tsv",
        "vehicle_meta.tsv",
        "reminders.tsv",
        "maintenance.tsv",
        "settings.ini"
    ];

    [Fact]
    public async Task Sqlite_roundtrip_preserves_all_domain_entities_and_czech_text()
    {
        var tempRoot = CreateTempRoot("vehimap-sqlite-roundtrip");
        var dataRoot = CreateDataRoot(tempRoot);
        var store = new SqliteVehimapDataStore();
        var dataSet = BuildSampleDataSet();

        try
        {
            await store.SaveAsync(dataRoot, dataSet);
            var loaded = await store.LoadAsync(dataRoot);

            Assert.True(File.Exists(Path.Combine(dataRoot.DataPath, "vehimap.db")));
            Assert.Equal("Žluťoučký kůň\nmultiline", loaded.Vehicles[0].VehicleNote);
            Assert.Equal("Shell V-Power 100", loaded.FuelEntries[0].FuelDetail);
            Assert.Equal("Benzina Praha", loaded.FuelEntries[0].Station);
            Assert.Equal(VehicleRecordAttachmentMode.Managed, loaded.Records[0].AttachmentMode);
            Assert.Equal("attachments/veh_1/faktura.pdf", loaded.Records[0].FilePath);
            Assert.Equal("45", loaded.Settings.GetValue("reminders", "technical_reminder_days"));
            Assert.Single(loaded.MaintenancePlans);
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [Fact]
    public async Task Migration_from_legacy_files_creates_backup_and_sqlite_database()
    {
        var tempRoot = CreateTempRoot("vehimap-sqlite-migration");
        var dataRoot = CreateDataRoot(tempRoot);
        var legacyStore = new LegacyVehimapDataStore();
        var sqliteStore = new SqliteVehimapDataStore();
        var migrationService = new SqliteDataMigrationService(legacyStore, sqliteStore);

        try
        {
            await legacyStore.SaveAsync(dataRoot, BuildSampleDataSet());
            Directory.CreateDirectory(Path.Combine(dataRoot.DataPath, "attachments", "veh_1"));
            await File.WriteAllBytesAsync(Path.Combine(dataRoot.DataPath, "attachments", "veh_1", "faktura.pdf"), [1, 2, 3]);

            var result = await migrationService.MigrateIfNeededAsync(dataRoot);
            var loaded = await sqliteStore.LoadAsync(dataRoot);

            Assert.True(result.Migrated);
            Assert.NotNull(result.PreMigrationBackupPath);
            Assert.True(Directory.Exists(result.PreMigrationBackupPath));
            Assert.True(File.Exists(Path.Combine(result.PreMigrationBackupPath!, "vehicles.tsv")));
            Assert.True(File.Exists(Path.Combine(result.PreMigrationBackupPath!, "removed-from-data-root", "vehicles.tsv")));
            Assert.True(File.Exists(Path.Combine(result.PreMigrationBackupPath!, "attachments", "veh_1", "faktura.pdf")));
            Assert.True(File.Exists(Path.Combine(dataRoot.DataPath, "vehimap.db")));
            Assert.True(File.Exists(Path.Combine(dataRoot.DataPath, "attachments", "veh_1", "faktura.pdf")));
            foreach (var fileName in LegacyFileNames)
            {
                Assert.False(File.Exists(Path.Combine(dataRoot.DataPath, fileName)));
            }

            Assert.Equal("Božena", loaded.Vehicles[0].Name);
            Assert.Equal("Technická kontrola", loaded.Reminders[0].Title);

            var secondRun = await migrationService.MigrateIfNeededAsync(dataRoot);
            Assert.False(secondRun.Migrated);
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [Fact]
    public async Task Migration_cleanup_moves_remaining_legacy_files_without_reimporting_them()
    {
        var tempRoot = CreateTempRoot("vehimap-sqlite-cleanup");
        var dataRoot = CreateDataRoot(tempRoot);
        var legacyStore = new LegacyVehimapDataStore();
        var sqliteStore = new SqliteVehimapDataStore();
        var migrationService = new SqliteDataMigrationService(legacyStore, sqliteStore);

        try
        {
            await sqliteStore.SaveAsync(dataRoot, BuildSampleDataSet("SQLite Božena"));
            await legacyStore.SaveAsync(dataRoot, BuildSampleDataSet("Legacy Božena"));
            Directory.CreateDirectory(Path.Combine(dataRoot.DataPath, "attachments", "veh_1"));
            await File.WriteAllBytesAsync(Path.Combine(dataRoot.DataPath, "attachments", "veh_1", "faktura.pdf"), [1, 2, 3]);

            var result = await migrationService.MigrateIfNeededAsync(dataRoot);
            var loaded = await sqliteStore.LoadAsync(dataRoot);

            Assert.False(result.Migrated);
            Assert.NotNull(result.PreMigrationBackupPath);
            Assert.True(File.Exists(Path.Combine(dataRoot.DataPath, "vehimap.db")));
            Assert.True(File.Exists(Path.Combine(dataRoot.DataPath, "attachments", "veh_1", "faktura.pdf")));
            Assert.True(File.Exists(Path.Combine(result.PreMigrationBackupPath!, "removed-from-data-root", "vehicles.tsv")));
            foreach (var fileName in LegacyFileNames)
            {
                Assert.False(File.Exists(Path.Combine(dataRoot.DataPath, fileName)));
            }

            Assert.Equal("SQLite Božena", loaded.Vehicles[0].Name);
            Assert.Equal(result.PreMigrationBackupPath, loaded.Settings.GetValue("migration", "pre_migration_backup_path"));
            Assert.NotEmpty(loaded.Settings.GetValue("migration", "legacy_cleanup_utc"));
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [Fact]
    public async Task Fixture_legacy_fleet_migrates_to_sqlite_and_archives_runtime_legacy_files()
    {
        var tempRoot = CreateTempRoot("vehimap-sqlite-fixture-migration");
        var dataRoot = CreateDataRoot(tempRoot);
        var legacyStore = new LegacyVehimapDataStore();
        var sqliteStore = new SqliteVehimapDataStore();
        var migrationService = new SqliteDataMigrationService(legacyStore, sqliteStore);

        try
        {
            CopyFixtureDataTo(dataRoot.DataPath);

            var result = await migrationService.MigrateIfNeededAsync(dataRoot);
            var loaded = await sqliteStore.LoadAsync(dataRoot);

            Assert.True(result.Migrated);
            Assert.NotNull(result.PreMigrationBackupPath);
            AssertFixtureFleetLoaded(loaded);
            AssertLegacyFilesArchived(dataRoot, result.PreMigrationBackupPath!);
            Assert.True(File.Exists(Path.Combine(dataRoot.DataPath, "attachments", "veh_fixture_1", "pojisteni.pdf")));
            Assert.True(File.Exists(Path.Combine(dataRoot.DataPath, "attachments", "veh_fixture_2", "tk.pdf")));
            Assert.True(File.Exists(Path.Combine(result.PreMigrationBackupPath!, "attachments", "veh_fixture_1", "servis-faktura.pdf")));

            var secondRun = await migrationService.MigrateIfNeededAsync(dataRoot);
            var reloaded = await sqliteStore.LoadAsync(dataRoot);

            Assert.False(secondRun.Migrated);
            AssertFixtureFleetLoaded(reloaded);
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [Fact]
    public async Task Fixture_storage_gate_roundtrips_backups_and_vehicle_package()
    {
        var tempRoot = CreateTempRoot("vehimap-sqlite-fixture-gate");
        var sourceRoot = CreateDataRoot(Path.Combine(tempRoot, "source"));
        var sqliteRestoreRoot = CreateDataRoot(Path.Combine(tempRoot, "sqlite-restore"));
        var legacyBackupSourceRoot = CreateDataRoot(Path.Combine(tempRoot, "legacy-backup-source"));
        var legacyRestoreRoot = CreateDataRoot(Path.Combine(tempRoot, "legacy-restore"));
        var packageTargetRoot = CreateDataRoot(Path.Combine(tempRoot, "package-target"));
        var legacyStore = new LegacyVehimapDataStore();
        var sqliteStore = new SqliteVehimapDataStore();
        var migrationService = new SqliteDataMigrationService(legacyStore, sqliteStore);
        var sqliteBackupService = new SqliteBackupService();
        var legacyBackupService = new LegacyBackupService();
        var packageService = new VehiclePackageService();

        try
        {
            CopyFixtureDataTo(sourceRoot.DataPath);
            await migrationService.MigrateIfNeededAsync(sourceRoot);
            var migrated = await sqliteStore.LoadAsync(sourceRoot);

            var sqliteBackupPath = Path.Combine(tempRoot, "fixture-v7.vehimapbak");
            var sqliteExport = await sqliteBackupService.ExportAsync(sqliteBackupPath, sourceRoot, migrated);
            var sqliteImported = await sqliteBackupService.ImportAsync(sqliteBackupPath);
            var sqliteRestore = await sqliteBackupService.RestoreAsync(sqliteRestoreRoot, sqliteImported);
            var sqliteRestored = await sqliteStore.LoadAsync(sqliteRestoreRoot);

            Assert.Equal(3, sqliteExport.IncludedManagedAttachmentCount);
            Assert.Equal(1, sqliteExport.MissingManagedAttachmentCount);
            Assert.Equal(3, sqliteRestore.RestoredAttachmentCount);
            AssertFixtureFleetLoaded(sqliteRestored);
            Assert.True(File.Exists(Path.Combine(sqliteRestoreRoot.DataPath, "attachments", "veh_fixture_1", "pojisteni.pdf")));

            CopyFixtureDataTo(legacyBackupSourceRoot.DataPath);
            var legacyData = await legacyStore.LoadAsync(legacyBackupSourceRoot);
            var legacyBackupPath = Path.Combine(tempRoot, "fixture-legacy.vehimapbak");
            var legacyExport = await legacyBackupService.ExportAsync(legacyBackupPath, legacyBackupSourceRoot, legacyData);
            var legacyImported = await sqliteBackupService.ImportAsync(legacyBackupPath);
            var legacyRestore = await sqliteBackupService.RestoreAsync(legacyRestoreRoot, legacyImported);
            var legacyRestored = await sqliteStore.LoadAsync(legacyRestoreRoot);

            Assert.Equal(3, legacyExport.IncludedManagedAttachmentCount);
            Assert.Equal(1, legacyExport.MissingManagedAttachmentCount);
            Assert.Equal(3, legacyRestore.RestoredAttachmentCount);
            Assert.True(File.Exists(Path.Combine(legacyRestoreRoot.DataPath, "vehimap.db")));
            AssertFixtureFleetLoaded(legacyRestored);

            var packagePath = Path.Combine(tempRoot, "zofka.vehimapvehicle");
            var packageTargetData = new VehimapDataSet
            {
                Vehicles =
                [
                    new Vehicle("veh_fixture_1", "Kolizní vozidlo", "Osobní vozidla", "", "Test", "", "", "", "", "", "", "")
                ]
            };

            var packageExport = await packageService.ExportVehicleAsync(packagePath, sourceRoot, migrated, "veh_fixture_1");
            var packageImport = await packageService.ImportVehicleAsync(packagePath, packageTargetRoot, packageTargetData);
            var importedVehicle = packageImport.DataSet.Vehicles.Single(item => item.Name == "Žofka");
            var importedManagedRecords = packageImport.DataSet.Records
                .Where(item => item.VehicleId == importedVehicle.Id && item.AttachmentMode == VehicleRecordAttachmentMode.Managed)
                .ToList();

            Assert.Equal(2, packageExport.IncludedAttachmentCount);
            Assert.Equal(0, packageExport.MissingAttachmentCount);
            Assert.NotEqual("veh_fixture_1", importedVehicle.Id);
            Assert.Equal(2, packageImport.RestoredAttachmentCount);
            Assert.Equal(2, importedManagedRecords.Count);
            Assert.All(importedManagedRecords, item => Assert.StartsWith($"attachments/{importedVehicle.Id}/", item.FilePath, StringComparison.Ordinal));
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [Fact]
    public async Task Runtime_save_after_migration_writes_only_sqlite_and_keeps_live_legacy_files_absent()
    {
        var tempRoot = CreateTempRoot("vehimap-sqlite-runtime-write");
        var dataRoot = CreateDataRoot(tempRoot);
        var legacyStore = new LegacyVehimapDataStore();
        var sqliteStore = new SqliteVehimapDataStore();
        var migrationService = new SqliteDataMigrationService(legacyStore, sqliteStore);

        try
        {
            CopyFixtureDataTo(dataRoot.DataPath);
            await migrationService.MigrateIfNeededAsync(dataRoot);
            var migrated = await sqliteStore.LoadAsync(dataRoot);

            migrated.Settings.SetValue("app", "show_dashboard_on_launch", "1");
            migrated.Vehicles[0] = migrated.Vehicles[0] with { Name = "SQLite runtime zápis" };
            migrated.FuelEntries.Add(new FuelEntry(
                "fuel_runtime",
                migrated.Vehicles[0].Id,
                "29.06.2026",
                "98765",
                "44.4",
                "1999",
                true,
                "Natural 95",
                "Runtime save smoke",
                "V-Power",
                "Shell Test"));
            migrated.Records[0] = migrated.Records[0] with { Title = "Doklad uložený přes SQLite" };

            await sqliteStore.SaveAsync(dataRoot, migrated);
            var reloaded = await sqliteStore.LoadAsync(dataRoot);

            Assert.True(File.Exists(Path.Combine(dataRoot.DataPath, "vehimap.db")));
            Assert.Equal("1", reloaded.Settings.GetValue("app", "show_dashboard_on_launch"));
            Assert.Equal("SQLite runtime zápis", reloaded.Vehicles[0].Name);
            Assert.Contains(reloaded.FuelEntries, item => item.Id == "fuel_runtime" && item.Station == "Shell Test");
            Assert.Equal("Doklad uložený přes SQLite", reloaded.Records[0].Title);
            AssertLiveLegacyFilesAbsent(dataRoot);
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [Fact]
    public async Task Sqlite_backup_roundtrip_restores_database_and_managed_attachments()
    {
        var tempRoot = CreateTempRoot("vehimap-sqlite-backup");
        var dataRoot = CreateDataRoot(Path.Combine(tempRoot, "source"));
        var restoreRoot = CreateDataRoot(Path.Combine(tempRoot, "restore"));
        var store = new SqliteVehimapDataStore();
        var backupService = new SqliteBackupService();
        var dataSet = BuildSampleDataSet();

        try
        {
            await store.SaveAsync(dataRoot, dataSet);
            Directory.CreateDirectory(Path.Combine(dataRoot.DataPath, "attachments", "veh_1"));
            await File.WriteAllBytesAsync(Path.Combine(dataRoot.DataPath, "attachments", "veh_1", "faktura.pdf"), [10, 20, 30]);

            var backupPath = Path.Combine(tempRoot, "vehimap-v7.vehimapbak");
            var exportResult = await backupService.ExportAsync(backupPath, dataRoot, dataSet);
            var imported = await backupService.ImportAsync(backupPath);
            var restoreResult = await backupService.RestoreAsync(restoreRoot, imported);
            var restored = await store.LoadAsync(restoreRoot);
            var restoredAttachment = await File.ReadAllBytesAsync(Path.Combine(restoreRoot.DataPath, "attachments", "veh_1", "faktura.pdf"));

            Assert.Equal(1, exportResult.IncludedManagedAttachmentCount);
            Assert.Equal(0, exportResult.MissingManagedAttachmentCount);
            Assert.Single(imported.Attachments);
            Assert.NotNull(restoreResult.PreRestoreBackupPath);
            Assert.Equal(1, restoreResult.RestoredAttachmentCount);
            Assert.Equal("Božena", restored.Vehicles[0].Name);
            Assert.Equal([10, 20, 30], restoredAttachment);
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [Fact]
    public async Task Sqlite_backup_service_imports_legacy_backup_and_restores_into_sqlite()
    {
        var tempRoot = CreateTempRoot("vehimap-sqlite-legacy-backup");
        var legacyRoot = CreateDataRoot(Path.Combine(tempRoot, "legacy"));
        var sqliteRoot = CreateDataRoot(Path.Combine(tempRoot, "sqlite"));
        var legacyBackup = new LegacyBackupService();
        var sqliteBackup = new SqliteBackupService();
        var sqliteStore = new SqliteVehimapDataStore();
        var dataSet = BuildSampleDataSet();

        try
        {
            Directory.CreateDirectory(Path.Combine(legacyRoot.DataPath, "attachments", "veh_1"));
            await File.WriteAllBytesAsync(Path.Combine(legacyRoot.DataPath, "attachments", "veh_1", "faktura.pdf"), [4, 5, 6]);
            var backupPath = Path.Combine(tempRoot, "legacy.vehimapbak");

            await legacyBackup.ExportAsync(backupPath, legacyRoot, dataSet);
            var imported = await sqliteBackup.ImportAsync(backupPath);
            await sqliteBackup.RestoreAsync(sqliteRoot, imported);
            var loaded = await sqliteStore.LoadAsync(sqliteRoot);

            Assert.Equal("Božena", loaded.Vehicles[0].Name);
            Assert.True(File.Exists(Path.Combine(sqliteRoot.DataPath, "vehimap.db")));
            Assert.True(File.Exists(Path.Combine(sqliteRoot.DataPath, "attachments", "veh_1", "faktura.pdf")));
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [Fact]
    public async Task Vehicle_package_export_import_remaps_vehicle_and_restores_attachment()
    {
        var tempRoot = CreateTempRoot("vehimap-vehicle-package");
        var sourceRoot = CreateDataRoot(Path.Combine(tempRoot, "source"));
        var targetRoot = CreateDataRoot(Path.Combine(tempRoot, "target"));
        var service = new VehiclePackageService();
        var sourceDataSet = BuildSampleDataSet();
        var targetDataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Už existuje", "Osobní vozidla", "", "Škoda 105", "", "", "", "", "", "", "")
            ]
        };

        try
        {
            Directory.CreateDirectory(Path.Combine(sourceRoot.DataPath, "attachments", "veh_1"));
            await File.WriteAllBytesAsync(Path.Combine(sourceRoot.DataPath, "attachments", "veh_1", "faktura.pdf"), [7, 8, 9]);
            var packagePath = Path.Combine(tempRoot, "bozena.vehimapvehicle");

            var exportResult = await service.ExportVehicleAsync(packagePath, sourceRoot, sourceDataSet, "veh_1");
            var importResult = await service.ImportVehicleAsync(packagePath, targetRoot, targetDataSet);

            var importedVehicle = importResult.DataSet.Vehicles.Single(item => item.Name == "Božena");
            var importedRecord = importResult.DataSet.Records.Single(item =>
                item.VehicleId == importedVehicle.Id
                && item.AttachmentMode == VehicleRecordAttachmentMode.Managed);
            var restoredAttachmentPath = Path.Combine(targetRoot.DataPath, importedRecord.FilePath.Replace('/', Path.DirectorySeparatorChar));
            var restoredAttachment = await File.ReadAllBytesAsync(restoredAttachmentPath);

            Assert.Equal(1, exportResult.IncludedAttachmentCount);
            Assert.Equal(0, exportResult.MissingAttachmentCount);
            Assert.NotEqual("veh_1", importedVehicle.Id);
            Assert.StartsWith($"attachments/{importedVehicle.Id}/", importedRecord.FilePath, StringComparison.Ordinal);
            Assert.Equal([7, 8, 9], restoredAttachment);
            Assert.Equal(1, importResult.RestoredAttachmentCount);
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    private static VehimapDataSet BuildSampleDataSet(string vehicleName = "Božena")
    {
        var settings = new VehimapSettings();
        settings.SetValue("reminders", "technical_reminder_days", "45");

        return new VehimapDataSet
        {
            Settings = settings,
            Vehicles =
            [
                new Vehicle("veh_1", vehicleName, "Osobní vozidla", "Žluťoučký kůň\nmultiline", "Škoda 100", "", "1972", "35 kW", "05/2024", "05/2026", "05/2025", "05/2026")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "Veterán", "srazové", "benzín", "garáž", "řetěz", "manuál")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "01.05.2026", "Servis", "12345", "2500", "Výměna oleje")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "02.05.2026", "12400", "32.5", "1490", true, "Benzín", "Bez problémů", "Shell V-Power 100", "Benzina Praha")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Faktura", "Servisní faktura", "Servis Praha", "01.05.2026", "01.05.2027", "2500", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/faktura.pdf", "Daňový doklad"),
                new VehicleRecord("rec_2", "veh_1", "Doklad", "Externí doklad", "", "", "", "", VehicleRecordAttachmentMode.External, @"C:\docs\externi.pdf", "")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Technická kontrola", "05/2026", "30", "Bez opakování", "Připomenout")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("maint_1", "veh_1", "Pravidelný servis", "10000", "12", "01.05.2026", "12345", true, "Olej a filtry")
            ]
        };
    }

    private static VehimapDataRoot CreateDataRoot(string rootPath) =>
        new(rootPath, Path.Combine(rootPath, "data"), true);

    private static void AssertFixtureFleetLoaded(VehimapDataSet dataSet)
    {
        Assert.Equal(3, dataSet.Vehicles.Count);
        Assert.Equal(5, dataSet.HistoryEntries.Count);
        Assert.Equal(5, dataSet.FuelEntries.Count);
        Assert.Equal(5, dataSet.Records.Count);
        Assert.Equal(3, dataSet.VehicleMetaEntries.Count);
        Assert.Equal(4, dataSet.Reminders.Count);
        Assert.Equal(4, dataSet.MaintenancePlans.Count);

        var vehicle = dataSet.Vehicles.Single(item => item.Id == "veh_fixture_1");
        Assert.Equal("Žofka", vehicle.Name);
        Assert.Equal("Rodinné auto\nanonymizovaná poznámka", vehicle.VehicleNote);
        Assert.Equal("45", dataSet.Settings.GetValue("reminders", "technical_reminder_days"));
        Assert.Equal("1", dataSet.Settings.GetValue("dashboard", "show_dashboard_on_launch"));
        Assert.Contains(dataSet.FuelEntries, item => item.Station == "Shell Brno" && item.FuelDetail == "Natural 98");
        Assert.Equal(4, dataSet.Records.Count(item => item.AttachmentMode == VehicleRecordAttachmentMode.Managed));
        Assert.Equal(1, dataSet.Records.Count(item => item.AttachmentMode == VehicleRecordAttachmentMode.External));
        Assert.Contains(dataSet.MaintenancePlans, item => item.Title == "Pravidelný servis" && item.VehicleId == "veh_fixture_1");
    }

    private static void AssertLegacyFilesArchived(VehimapDataRoot dataRoot, string backupPath)
    {
        foreach (var fileName in LegacyFileNames)
        {
            Assert.False(File.Exists(Path.Combine(dataRoot.DataPath, fileName)));
            Assert.True(File.Exists(Path.Combine(backupPath, fileName)));
            Assert.True(File.Exists(Path.Combine(backupPath, "removed-from-data-root", fileName)));
        }
    }

    private static void AssertLiveLegacyFilesAbsent(VehimapDataRoot dataRoot)
    {
        foreach (var fileName in LegacyFileNames)
        {
            Assert.False(File.Exists(Path.Combine(dataRoot.DataPath, fileName)));
        }
    }

    private static void CopyFixtureDataTo(string targetDataPath)
    {
        var fixtureDataPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "legacy-1.0.2-fleet", "data");
        if (!Directory.Exists(fixtureDataPath))
        {
            throw new DirectoryNotFoundException($"Fixture data nebyla nalezena: {fixtureDataPath}");
        }

        CopyDirectory(fixtureDataPath, targetDataPath);
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);
        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(targetDirectory, relativePath));
        }

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            var targetPath = Path.Combine(targetDirectory, relativePath);
            var parent = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                Directory.CreateDirectory(parent);
            }

            File.Copy(file, targetPath, overwrite: true);
        }
    }

    private static string CreateTempRoot(string prefix)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{prefix}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteTempRoot(string tempRoot)
    {
        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
