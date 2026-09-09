// SPDX-License-Identifier: GPL-3.0-or-later
using System.IO.Compression;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;
using Vehimap.Storage.Sqlite;
using Xunit;

namespace Vehimap.Tests.LegacyCompatibility;

public sealed partial class SqliteStorageCompatibilityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Package_import_failure_after_copy_rolls_back_database_and_removes_only_new_files(bool cancel)
    {
        var directory = CreateTempRoot("vehimap-package-commit-failure");
        var source = CreateDataRoot(Path.Combine(directory, "source"));
        var target = CreateDataRoot(Path.Combine(directory, "target"));
        var store = new SqliteVehimapDataStore();
        var current = BuildSampleDataSet("Original");
        var beforeCommitReached = false;
        using var cancellation = new CancellationTokenSource();
        try
        {
            var sourceAttachment = Path.Combine(source.DataPath, "attachments", "veh_1", "faktura.pdf");
            Directory.CreateDirectory(Path.GetDirectoryName(sourceAttachment)!);
            await File.WriteAllBytesAsync(sourceAttachment, [7, 8, 9]);
            var originalAttachment = Path.Combine(target.DataPath, "attachments", "veh_1", "faktura.pdf");
            Directory.CreateDirectory(Path.GetDirectoryName(originalAttachment)!);
            await File.WriteAllBytesAsync(originalAttachment, [1, 2, 3]);
            await store.SaveAsync(target, current);
            var package = Path.Combine(directory, "incoming.vehimapvehicle");
            await new VehiclePackageService().ExportVehicleAsync(package, source, BuildSampleDataSet("Incoming"), "veh_1");
            using (var connection = OpenDatabase(target))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TRIGGER reject_import BEFORE INSERT ON vehicles WHEN NEW.name = 'Incoming' BEGIN SELECT RAISE(ABORT, 'Injected import failure'); END;";
                command.ExecuteNonQuery();
            }
            var service = new VehiclePackageService(null, () =>
            {
                beforeCommitReached = true;
                Assert.Equal(2, Directory.GetFiles(Path.Combine(target.DataPath, "attachments"), "*", SearchOption.AllDirectories).Length);
                Assert.Throws<IOException>(() => SqliteStorageLease.EnterStable(target));
                if (cancel) cancellation.Cancel();
            });

            var exception = await Record.ExceptionAsync(() => service.ImportVehicleAsync(package, target, current, cancellation.Token));
            if (cancel) Assert.IsAssignableFrom<OperationCanceledException>(exception);
            else Assert.IsType<Microsoft.Data.Sqlite.SqliteException>(exception);
            Assert.True(beforeCommitReached);
            Assert.Equal("Original", Assert.Single(current.Vehicles).Name);
            Assert.Equal("Original", Assert.Single((await store.LoadAsync(target)).Vehicles).Name);
            Assert.Equal(originalAttachment, Assert.Single(Directory.GetFiles(Path.Combine(target.DataPath, "attachments"), "*", SearchOption.AllDirectories)));
            Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(originalAttachment));

            using (var connection = OpenDatabase(target))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DROP TRIGGER reject_import;";
                command.ExecuteNonQuery();
            }
            var retry = await new VehiclePackageService().ImportVehicleAsync(package, target, current);
            var loaded = await store.LoadAsync(target);
            Assert.Equal(2, loaded.Vehicles.Count);
            Assert.Contains(loaded.Vehicles, v => v.Id == retry.ImportedVehicleId && v.Name == "Incoming");
            SqliteDataMigrationService.VerifyRoundTrip(retry.DataSet, loaded);
            Assert.Single(current.Vehicles);
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Oversized_zip_entries_are_rejected_by_backup_and_package_without_legacy_fallback()
    {
        var directory = CreateTempRoot("vehimap-import-size");
        var root = CreateDataRoot(Path.Combine(directory, "data"));
        try
        {
            var archive = Path.Combine(directory, "invalid.zip");
            using (var zip = ZipFile.Open(archive, ZipArchiveMode.Create))
            using (var stream = zip.CreateEntry("vehicle.json").Open())
            {
                var buffer = new byte[64 * 1024];
                for (var i = 0; i <= DataArchiveLimits.Default.JsonBytes / buffer.Length; i++)
                    await stream.WriteAsync(buffer);
            }
            await Assert.ThrowsAsync<DataArchiveLimitException>(() => new SqliteBackupService().ImportAsync(archive));
            await Assert.ThrowsAsync<DataArchiveLimitException>(() => new VehiclePackageService().ImportVehicleAsync(archive, root, new()));
            Assert.False(File.Exists(Path.Combine(root.DataPath, "vehimap.db")));
            Assert.False(Directory.Exists(Path.Combine(root.DataPath, "attachments")));
            await File.WriteAllTextAsync(archive, "not a ZIP or a legacy backup");
            await Assert.ThrowsAsync<InvalidDataException>(() => new SqliteBackupService().ImportAsync(archive));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Oversized_legacy_text_is_rejected_before_parsing_or_base64_allocation()
    {
        var directory = CreateTempRoot("vehimap-legacy-size");
        try
        {
            var path = Path.Combine(directory, "large.vehimapbak");
            await File.WriteAllTextAsync(path, "# Vehimap backup v6\n");
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Write))
                stream.SetLength(DataArchiveLimits.LegacyTextBytes + 1);
            var exception = await Assert.ThrowsAsync<LegacyBackupException>(() => new SqliteBackupService().ImportAsync(path));
            Assert.IsType<DataArchiveLimitException>(exception.GetBaseException());
            Assert.Equal(DataArchiveLimits.LegacyTextBytes + 1, new FileInfo(path).Length);
        }
        finally { DeleteTempRoot(directory); }
    }
}
