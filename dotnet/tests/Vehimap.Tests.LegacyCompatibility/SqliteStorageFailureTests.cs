// SPDX-License-Identifier: GPL-3.0-or-later
using Microsoft.Data.Sqlite;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;
using Vehimap.Storage.Sqlite;
using Xunit;

namespace Vehimap.Tests.LegacyCompatibility;

public sealed partial class SqliteStorageCompatibilityTests
{
    [Fact]
    public async Task Failed_migration_does_not_publish_an_empty_database_and_can_be_retried()
    {
        var directory = CreateTempRoot("vehimap-migration-failure");
        var root = CreateDataRoot(directory);
        var legacy = new LegacyVehimapDataStore();
        var sqlite = new SqliteVehimapDataStore();
        try
        {
            await legacy.SaveAsync(root, BuildSampleDataSet());
            var migration = new SqliteDataMigrationService(legacy, new InterruptedStore());
            await Assert.ThrowsAsync<IOException>(() => migration.MigrateIfNeededAsync(root));

            Assert.False(File.Exists(Path.Combine(root.DataPath, "vehimap.db")));
            Assert.True(File.Exists(Path.Combine(root.DataPath, "vehicles.tsv")));
            Assert.NotEmpty(Directory.GetFiles(Path.Combine(root.DataPath, "migration-backups"), "vehicles.tsv", SearchOption.AllDirectories));
            Assert.True((await new SqliteDataMigrationService(legacy, sqlite).MigrateIfNeededAsync(root)).Migrated);
            Assert.Equal("Božena", Assert.Single((await sqlite.LoadAsync(root)).Vehicles).Name);
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Migration_verifies_all_saved_entities_before_archiving_legacy_files()
    {
        var directory = CreateTempRoot("vehimap-migration-verification");
        var root = CreateDataRoot(directory);
        var legacy = new LegacyVehimapDataStore();
        try
        {
            await legacy.SaveAsync(root, BuildSampleDataSet());
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                new SqliteDataMigrationService(legacy, new IncompleteStore()).MigrateIfNeededAsync(root));
            Assert.False(File.Exists(Path.Combine(root.DataPath, "vehimap.db")));
            Assert.True(File.Exists(Path.Combine(root.DataPath, "fuel.tsv")));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Theory]
    [InlineData("DROP TABLE fuel_entries;")]
    [InlineData("DELETE FROM schema_migrations;")]
    [InlineData("INSERT INTO schema_migrations VALUES ('9.0-future', '2026-01-01');")]
    public async Task Existing_invalid_schema_is_rejected_without_automatic_repair(string damage)
    {
        var directory = CreateTempRoot("vehimap-invalid-schema");
        var root = CreateDataRoot(directory);
        var store = new SqliteVehimapDataStore();
        try
        {
            await store.SaveAsync(root, BuildSampleDataSet());
            using (var connection = OpenDatabase(root))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = damage;
                command.ExecuteNonQuery();
            }
            var before = await File.ReadAllBytesAsync(Path.Combine(root.DataPath, "vehimap.db"));
            await Assert.ThrowsAsync<InvalidDataException>(() => store.LoadAsync(root));
            await Assert.ThrowsAsync<InvalidDataException>(() => store.SaveAsync(root, new VehimapDataSet()));
            var health = await new SqliteDataStoreHealthService().CheckAsync(root);
            Assert.Equal(Vehimap.Application.Models.DataStoreHealthStatus.Error, health.Status);
            Assert.Equal(before, await File.ReadAllBytesAsync(Path.Combine(root.DataPath, "vehimap.db")));
        }
        finally { DeleteTempRoot(directory); }
    }

    private static SqliteConnection OpenDatabase(VehimapDataRoot root)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(root.DataPath, "vehimap.db"), Pooling = false
        }.ToString());
        connection.Open();
        return connection;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Restore_rejects_invalid_or_duplicate_attachment_paths_before_changing_live_data(bool duplicate)
    {
        var directory = CreateTempRoot("vehimap-restore-invalid");
        var root = CreateDataRoot(directory);
        var store = new SqliteVehimapDataStore();
        try
        {
            await store.SaveAsync(root, BuildSampleDataSet("Original"));
            var attachment = Path.Combine(root.DataPath, "attachments", "veh_1", "original.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(attachment)!);
            await File.WriteAllTextAsync(attachment, "original attachment");
            var bundle = new VehimapBackupBundle(BuildSampleDataSet("Incoming"),
                [new("attachments/veh_1/incoming.txt", [1]), new(duplicate ? "attachments/veh_1/incoming.txt" : "../outside.txt", [2])]);
            await Assert.ThrowsAsync<InvalidDataException>(() => new SqliteBackupService().RestoreAsync(root, bundle));
            Assert.Equal("Original", Assert.Single((await store.LoadAsync(root)).Vehicles).Name);
            Assert.Equal("original attachment", await File.ReadAllTextAsync(attachment));
            Assert.False(File.Exists(Path.Combine(root.DataPath, "attachments", "veh_1", "incoming.txt")));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Restore_database_failure_rolls_back_replaced_attachments()
    {
        var directory = CreateTempRoot("vehimap-restore-rollback");
        var root = CreateDataRoot(directory);
        var store = new SqliteVehimapDataStore();
        try
        {
            await store.SaveAsync(root, BuildSampleDataSet("Original"));
            var attachment = Path.Combine(root.DataPath, "attachments", "original.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(attachment)!);
            await File.WriteAllTextAsync(attachment, "original");
            var backup = new SqliteBackupService(store, new LegacyBackupService(), phase =>
            {
                if (phase == "database-installed") throw new OperationCanceledException("Injected interruption after live commit.");
            });
            await Assert.ThrowsAsync<OperationCanceledException>(() => backup.RestoreAsync(root,
                new VehimapBackupBundle(BuildSampleDataSet("Incoming"), [new("attachments/new.txt", [1])])));
            Assert.Equal("Original", Assert.Single((await store.LoadAsync(root)).Vehicles).Name);
            Assert.Equal("original", await File.ReadAllTextAsync(attachment));
            Assert.False(File.Exists(Path.Combine(root.DataPath, "attachments", "new.txt")));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Pre_restore_backup_includes_committed_WAL_data()
    {
        var directory = CreateTempRoot("vehimap-restore-wal");
        var root = CreateDataRoot(directory);
        var store = new SqliteVehimapDataStore();
        try
        {
            await store.SaveAsync(root, BuildSampleDataSet());
            using var keeper = OpenDatabase(root);
            using var command = keeper.CreateCommand();
            command.CommandText = "PRAGMA wal_autocheckpoint=0; UPDATE vehicles SET name='Committed WAL';";
            command.ExecuteNonQuery();
            Assert.True(new FileInfo(Path.Combine(root.DataPath, "vehimap.db-wal")).Length > 0);
            var restored = await new SqliteBackupService().RestoreAsync(root, new VehimapBackupBundle(new VehimapDataSet(), []));
            var backupRoot = new VehimapDataRoot(restored.PreRestoreBackupPath!, restored.PreRestoreBackupPath!, true);
            Assert.Equal("Committed WAL", Assert.Single((await store.LoadAsync(backupRoot)).Vehicles).Name);
            Assert.Empty((await store.LoadAsync(root)).Vehicles);
            Assert.True(Directory.Exists(Path.Combine(root.DataPath, "attachments")));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Failed_or_cancelled_archive_export_preserves_existing_file()
    {
        var directory = CreateTempRoot("vehimap-atomic-export");
        try
        {
            var target = Path.Combine(directory, "existing.vehimapbak");
            await File.WriteAllTextAsync(target, "original backup");
            Assert.Throws<DirectoryNotFoundException>(() => AtomicArchiveWriter.CreateFromDirectory(Path.Combine(directory, "absent"), target));
            Assert.Equal("original backup", await File.ReadAllTextAsync(target));
            Assert.Throws<OperationCanceledException>(() => AtomicArchiveWriter.CreateFromDirectory(directory, target, new CancellationToken(true)));
            Assert.Equal("original backup", await File.ReadAllTextAsync(target));
            Assert.Single(Directory.GetFiles(directory));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Package_missing_attachment_never_links_to_an_existing_destination_file()
    {
        var directory = CreateTempRoot("vehimap-package-missing");
        var source = CreateDataRoot(Path.Combine(directory, "source"));
        var target = CreateDataRoot(Path.Combine(directory, "target"));
        var service = new VehiclePackageService();
        try
        {
            var existingPath = Path.Combine(target.DataPath, "attachments", "veh_1", "faktura.pdf");
            Directory.CreateDirectory(Path.GetDirectoryName(existingPath)!);
            await File.WriteAllTextAsync(existingPath, "unrelated destination file");
            var package = Path.Combine(directory, "missing.vehimapvehicle");
            await service.ExportVehicleAsync(package, source, BuildSampleDataSet(), "veh_1");
            var imported = await service.ImportVehicleAsync(package, target, new VehimapDataSet());
            var record = imported.DataSet.Records.Single(item => item.Id == "rec_1");
            Assert.NotEqual("attachments/veh_1/faktura.pdf", record.FilePath);
            Assert.False(File.Exists(Path.Combine(target.DataPath, record.FilePath)));
            Assert.Equal("unrelated destination file", await File.ReadAllTextAsync(existingPath));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Theory]
    [InlineData("path")]
    [InlineData("owner")]
    [InlineData("manifest")]
    public async Task Package_validates_all_records_before_copying_any_attachment(string damage)
    {
        var directory = CreateTempRoot("vehimap-package-validation");
        var root = CreateDataRoot(directory);
        try
        {
            var data = BuildSampleDataSet();
            data.Records.Add(data.Records[0] with
            {
                Id = "invalid", VehicleId = damage == "owner" ? "another_vehicle" : "veh_1",
                FilePath = damage == "path" ? "../outside.txt" : "attachments/veh_1/other.pdf"
            });
            var manifest = SerializePackageJson(new { format = "vehimap.vehicle-package", version = 1,
                vehicleId = damage == "manifest" ? "another_vehicle" : "veh_1", vehicleName = "Test", createdUtc = DateTime.UtcNow });
            var package = Path.Combine(directory, "invalid.vehimapvehicle");
            await CreateZipAsync(package, ("manifest.json", manifest), ("vehicle.json", SerializePackageJson(data)),
                ("attachments/veh_1/faktura.pdf", "first valid attachment"));
            await Assert.ThrowsAsync<FormatException>(() => new VehiclePackageService().ImportVehicleAsync(package, root, new VehimapDataSet()));
            Assert.False(Directory.Exists(Path.Combine(root.DataPath, "attachments")));
        }
        finally { DeleteTempRoot(directory); }
    }

    private sealed class InterruptedStore : IVehimapDataStore
    {
        public Task<VehimapDataSet> LoadAsync(VehimapDataRoot root, CancellationToken cancellationToken = default) =>
            new SqliteVehimapDataStore().LoadAsync(root, cancellationToken);

        public async Task SaveAsync(VehimapDataRoot root, VehimapDataSet data, CancellationToken cancellationToken = default)
        {
            await new SqliteVehimapDataStore().SaveAsync(root, new VehimapDataSet(), cancellationToken);
            throw new IOException("Injected interruption after database initialization.");
        }
    }

    private sealed class IncompleteStore : IVehimapDataStore
    {
        public Task<VehimapDataSet> LoadAsync(VehimapDataRoot root, CancellationToken cancellationToken = default) =>
            new SqliteVehimapDataStore().LoadAsync(root, cancellationToken);

        public Task SaveAsync(VehimapDataRoot root, VehimapDataSet data, CancellationToken cancellationToken = default) =>
            new SqliteVehimapDataStore().SaveAsync(root, new VehimapDataSet { Settings = data.Settings, Vehicles = data.Vehicles }, cancellationToken);
    }
}
