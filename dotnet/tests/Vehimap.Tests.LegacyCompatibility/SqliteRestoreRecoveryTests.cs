// SPDX-License-Identifier: GPL-3.0-or-later
using System.Diagnostics;
using System.Text.Json;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;
using Vehimap.Storage.Sqlite;
using Xunit;

namespace Vehimap.Tests.LegacyCompatibility;

public sealed partial class SqliteStorageCompatibilityTests
{
    public static IEnumerable<object[]> RestoreCrashPoints =>
        from phase in new[] { "prepared", "attachments-removed", "attachments-installed", "database-installed", "committed" }
        from existing in new[] { false, true }
        from attachments in new[] { false, true }
        select new object[] { phase, existing, attachments };

    [Theory]
    [MemberData(nameof(RestoreCrashPoints))]
    public async Task Killed_restore_recovers_a_consistent_database_and_attachments_before_migration(string phase, bool existing, bool attachments)
    {
        var directory = await PrepareProcessFixture(existing, attachments);
        var root = CreateDataRoot(directory);
        try
        {
            await KillAtCheckpoint(directory, "restore", phase);
            Assert.True(SqliteRestoreJournal.Exists(root));
            Assert.Equal(DataStoreHealthStatus.Error, (await new SqliteDataStoreHealthService().CheckAsync(root)).Status);
            var store = new SqliteVehimapDataStore();
            var migration = await new SqliteDataMigrationService(new LegacyVehimapDataStore(), store).MigrateIfNeededAsync(root);
            Assert.False(migration.Migrated);
            var data = await store.LoadAsync(root);
            var committed = phase == "committed";
            if (existing || committed)
            {
                SqliteDataMigrationService.VerifyRoundTrip(RestoreProcessWorker.Data(committed ? "Incoming" : "Original"), data);
            }
            else
            {
                Assert.Empty(data.Vehicles);
            }
            if (attachments || committed)
                Assert.Equal(committed ? new byte[] { 2, 3, 4 } : new byte[] { 1 },
                    await File.ReadAllBytesAsync(Path.Combine(root.DataPath, "attachments", "record.txt")));
            else
                Assert.False(Directory.Exists(Path.Combine(root.DataPath, "attachments")));
            Assert.False(SqliteRestoreJournal.Exists(root));
            AssertLiveLegacyFilesAbsent(root);
            // A second start must not roll back a successfully recovered dataset again.
            SqliteDataMigrationService.VerifyRoundTrip(data, await store.LoadAsync(root));
            if (attachments)
            {
                var backup = Assert.Single(Directory.GetDirectories(Path.Combine(root.DataPath, "import-backups")));
                Assert.Equal(new byte[] { 1 }, await File.ReadAllBytesAsync(Path.Combine(backup, "attachments", "record.txt")));
            }
        }
        finally { DeleteTempRoot(directory); }
    }

    [Theory]
    [InlineData("rollback-database")]
    [InlineData("rollback-attachments-removed")]
    [InlineData("rollback-attachments-restored")]
    public async Task Recovery_can_itself_be_killed_and_retried_without_consuming_safety_copies(string phase)
    {
        var directory = await PrepareProcessFixture(true);
        var root = CreateDataRoot(directory);
        try
        {
            await KillAtCheckpoint(directory, "restore", "database-installed");
            await KillAtCheckpoint(directory, "recover", phase);
            Assert.True(SqliteRestoreJournal.Exists(root));
            var loaded = await new SqliteVehimapDataStore().LoadAsync(root);
            Assert.Equal("Original", Assert.Single(loaded.Vehicles).Name);
            Assert.Equal(new byte[] { 1 }, await File.ReadAllBytesAsync(Path.Combine(root.DataPath, "attachments", "record.txt")));
            Assert.False(SqliteRestoreJournal.Exists(root));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Theory]
    [InlineData("database")]
    [InlineData("attachment")]
    [InlineData("journal")]
    public async Task Damaged_recovery_evidence_blocks_load_and_save_without_touching_live_data(string damage)
    {
        var directory = await PrepareProcessFixture(true);
        var root = CreateDataRoot(directory);
        try
        {
            await KillAtCheckpoint(directory, "restore", "database-installed");
            var backup = Assert.Single(Directory.GetDirectories(Path.Combine(root.DataPath, "import-backups")));
            var damaged = damage switch
            {
                "database" => Path.Combine(backup, "vehimap.db"),
                "attachment" => Path.Combine(backup, "attachments", "record.txt"),
                _ => Path.Combine(root.DataPath, SqliteRestoreJournal.FileName)
            };
            await File.WriteAllTextAsync(damaged, "damaged evidence");
            var before = await File.ReadAllBytesAsync(Path.Combine(root.DataPath, "vehimap.db"));
            var store = new SqliteVehimapDataStore();
            await Assert.ThrowsAsync<InvalidDataException>(() => store.LoadAsync(root));
            await Assert.ThrowsAsync<InvalidDataException>(() => store.SaveAsync(root, new VehimapDataSet()));
            Assert.Equal(before, await File.ReadAllBytesAsync(Path.Combine(root.DataPath, "vehimap.db")));
            Assert.Equal(new byte[] { 2, 3, 4 }, await File.ReadAllBytesAsync(Path.Combine(root.DataPath, "attachments", "record.txt")));
            Assert.True(SqliteRestoreJournal.Exists(root));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Save_after_interrupted_restore_requires_reload_instead_of_committing_stale_state()
    {
        var directory = await PrepareProcessFixture(true);
        var root = CreateDataRoot(directory);
        try
        {
            await KillAtCheckpoint(directory, "restore", "database-installed");
            var store = new SqliteVehimapDataStore();
            await Assert.ThrowsAsync<IOException>(() => store.SaveAsync(root, new VehimapDataSet()));
            Assert.Equal("Original", Assert.Single((await store.LoadAsync(root)).Vehicles).Name);
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Restore_lease_blocks_a_second_writer_and_does_not_trigger_rollback_in_the_running_operation()
    {
        var directory = await PrepareProcessFixture(true);
        var root = CreateDataRoot(directory);
        try
        {
            using var lease = SqliteStorageLease.Enter(root);
            await Assert.ThrowsAsync<IOException>(() => new SqliteVehimapDataStore().LoadAsync(root));
            await Assert.ThrowsAsync<IOException>(() => new SqliteVehimapDataStore().SaveAsync(root, new VehimapDataSet()));
            await Assert.ThrowsAsync<IOException>(() => new SqliteBackupService().ExportAsync(Path.Combine(directory, "out.vehimapbak"), root, new VehimapDataSet()));
            await Assert.ThrowsAsync<IOException>(() => new VehiclePackageService().ExportVehicleAsync(Path.Combine(directory, "out.vehimapvehicle"), root, RestoreProcessWorker.Data("Original"), "v1"));
            await Assert.ThrowsAsync<IOException>(() => new VehiclePackageService().ImportVehicleAsync(Path.Combine(directory, "absent.vehimapvehicle"), root, new VehimapDataSet()));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Recovery_journal_cannot_redirect_to_an_arbitrary_path()
    {
        var directory = await PrepareProcessFixture(true);
        var root = CreateDataRoot(directory);
        try
        {
            await KillAtCheckpoint(directory, "restore", "prepared");
            var path = Path.Combine(root.DataPath, SqliteRestoreJournal.FileName);
            var envelope = JsonSerializer.Deserialize<RestoreJournalEnvelope>(await File.ReadAllTextAsync(path))!;
            var journal = envelope.Journal with { BackupDirectory = "../../outside" };
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(journal)));
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new RestoreJournalEnvelope(journal, hash)));
            await Assert.ThrowsAsync<InvalidDataException>(() => new SqliteVehimapDataStore().LoadAsync(root));
            Assert.True(File.Exists(path));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Changed_journal_commit_flag_does_not_bypass_recovery_validation()
    {
        var directory = await PrepareProcessFixture(true);
        var root = CreateDataRoot(directory);
        try
        {
            await KillAtCheckpoint(directory, "restore", "attachments-installed");
            var path = Path.Combine(root.DataPath, SqliteRestoreJournal.FileName);
            var envelope = JsonSerializer.Deserialize<RestoreJournalEnvelope>(await File.ReadAllTextAsync(path))!;
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(envelope with { Journal = envelope.Journal with { Committed = true } }));
            await Assert.ThrowsAsync<InvalidDataException>(() => new SqliteVehimapDataStore().LoadAsync(root));
            Assert.True(File.Exists(path));
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Restore_refuses_linked_existing_attachments_before_publishing_a_journal()
    {
        var directory = await PrepareProcessFixture(true);
        var root = CreateDataRoot(directory);
        try
        {
            var outside = Path.Combine(directory, "outside");
            Directory.CreateDirectory(outside);
            await File.WriteAllTextAsync(Path.Combine(outside, "keep.txt"), "untouched");
            var link = Path.Combine(root.DataPath, "attachments", "link");
            Directory.CreateSymbolicLink(link, outside);
            try
            {
                await Assert.ThrowsAsync<InvalidDataException>(() => new SqliteBackupService().RestoreAsync(root,
                    new VehimapBackupBundle(RestoreProcessWorker.Data("Incoming"), [])));
                Assert.False(SqliteRestoreJournal.Exists(root));
                Assert.Equal("Original", Assert.Single((await new SqliteVehimapDataStore().LoadAsync(root)).Vehicles).Name);
                Assert.Equal("untouched", await File.ReadAllTextAsync(Path.Combine(outside, "keep.txt")));
            }
            finally { Directory.Delete(link); }
        }
        finally { DeleteTempRoot(directory); }
    }

    [Fact]
    public async Task Restore_verifies_staged_rows_before_any_live_change()
    {
        var directory = await PrepareProcessFixture(true);
        var root = CreateDataRoot(directory);
        try
        {
            await Assert.ThrowsAsync<InvalidDataException>(() => new SqliteBackupService(new IncompleteStore(), new LegacyBackupService())
                .RestoreAsync(root, new VehimapBackupBundle(RestoreProcessWorker.Data("Incoming"), [])));
            Assert.False(SqliteRestoreJournal.Exists(root));
            Assert.Equal("Original", Assert.Single((await new SqliteVehimapDataStore().LoadAsync(root)).Vehicles).Name);
        }
        finally { DeleteTempRoot(directory); }
    }

    private static async Task<string> PrepareProcessFixture(bool existing, bool? attachments = null)
    {
        var directory = CreateTempRoot("vehimap-restore-process");
        await File.WriteAllTextAsync(Path.Combine(directory, "restore-process-fixture"), "synthetic test only");
        var root = CreateDataRoot(directory);
        if (existing)
        {
            await new SqliteVehimapDataStore().SaveAsync(root, RestoreProcessWorker.Data("Original"));
        }
        if (attachments ?? existing)
        {
            Directory.CreateDirectory(Path.Combine(root.DataPath, "attachments"));
            await File.WriteAllBytesAsync(Path.Combine(root.DataPath, "attachments", "record.txt"), [1]);
        }
        return directory;
    }

    private static async Task KillAtCheckpoint(string directory, string operation, string phase)
    {
        var marker = Path.Combine(directory, "checkpoint");
        File.Delete(marker);
        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardError = true, RedirectStandardOutput = true
        };
        start.ArgumentList.Add(typeof(RestoreProcessWorker).Assembly.Location);
        start.ArgumentList.Add(operation);
        start.ArgumentList.Add(directory);
        start.ArgumentList.Add(phase);
        using var process = Process.Start(start)!;
        var error = process.StandardError.ReadToEndAsync();
        var output = process.StandardOutput.ReadToEndAsync();
        try
        {
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (!process.HasExited && !File.Exists(marker) && DateTime.UtcNow < deadline)
                await Task.Delay(25);
            Assert.False(process.HasExited, process.HasExited ? await error : "");
            Assert.True(File.Exists(marker), $"Restore worker did not reach {phase}.");
            Assert.Equal(phase, await File.ReadAllTextAsync(marker));
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15));
            Assert.NotEqual(0, process.ExitCode);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
            await error;
            await output;
        }
    }
}
