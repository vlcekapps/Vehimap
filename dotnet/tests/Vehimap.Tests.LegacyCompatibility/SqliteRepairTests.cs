// SPDX-License-Identifier: GPL-3.0-or-later
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Vehimap.Storage.Sqlite;
using Xunit;

namespace Vehimap.Tests.LegacyCompatibility;

public sealed partial class SqliteStorageCompatibilityTests
{
    [Fact]
    public async Task Repairs_upgrade_old_schema_with_backup_and_survive_backup_and_colliding_package_import()
    {
        var temp = CreateTempRoot("vehimap-repair-roundtrip");
        try
        {
            var root = CreateDataRoot(temp); var store = new SqliteVehimapDataStore();
            var data = BuildSampleDataSet();
            await store.SaveAsync(root, data);
            using (var connection = new SqliteConnection($"Data Source={SqliteVehimapDataStore.GetDatabasePath(root)};Pooling=False"))
            {
                connection.Open(); using var command = connection.CreateCommand();
                command.CommandText = "DROP TABLE vehicle_repairs; DELETE FROM schema_migrations WHERE id = '2.0-repairs';";
                command.ExecuteNonQuery();
            }
            var old = await store.LoadAsync(root);
            Assert.Empty(old.Repairs);
            var service = new VehicleRepairService(new ResourceAppLocalizer());
            var change = new RepairChange(RepairAction.Create, "veh_1", null, "Závada klimatizace", "Poznámka\ns češtinou", "01.09.2026", "20.09.2026", 7, "", "", "", "");
            var repair = service.Apply(old, change, DateTimeOffset.UtcNow);
            repair = service.Apply(old, change with { Action = RepairAction.Reschedule, RepairId = repair.Id, PlannedDate = "30.09.2026", Reason = "Nedostupný díl" }, DateTimeOffset.UtcNow);
            repair = service.Apply(old, change with { Action = RepairAction.Complete, RepairId = repair.Id, CompletedDate = "22.09.2026", Reason = "Výměna dílu", Cost = "200.5" }, DateTimeOffset.UtcNow);
            await store.SaveAsync(root, old);
            Assert.Single(Directory.GetFiles(Path.Combine(root.DataPath, "schema-backups"), "*.db"));
            var loaded = await store.LoadAsync(root);
            Assert.Equal(JsonSerializer.Serialize(old.Repairs), JsonSerializer.Serialize(loaded.Repairs));
            await store.SaveAsync(root, loaded);
            Assert.Single(Directory.GetFiles(Path.Combine(root.DataPath, "schema-backups"), "*.db"));
            var backup = new SqliteBackupService();
            var path = Path.Combine(temp, "repairs.vehimapbak");
            await backup.ExportAsync(path, root, loaded);
            var bundle = await backup.ImportAsync(path);
            var restoredRoot = CreateDataRoot(Path.Combine(temp, "restored"));
            await backup.RestoreAsync(restoredRoot, bundle);
            Assert.Equal(JsonSerializer.Serialize(loaded.Repairs), JsonSerializer.Serialize((await store.LoadAsync(restoredRoot)).Repairs));
            var package = new VehiclePackageService(); var packagePath = Path.Combine(temp, "vehicle.vehimapvehicle");
            await package.ExportVehicleAsync(packagePath, root, loaded, "veh_1");
            var imported = await package.ImportVehicleAsync(packagePath, root, loaded);
            var copy = imported.DataSet.Repairs.Single(r => r.VehicleId != "veh_1");
            Assert.NotEqual(repair.Id, copy.Id); Assert.NotEqual(repair.HistoryEntryId, copy.HistoryEntryId);
            Assert.Equal("Výměna dílu", imported.DataSet.HistoryEntries.Single(h => h.Id == copy.HistoryEntryId && h.VehicleId == copy.VehicleId).EventType);
            Assert.Single(copy.ScheduleChanges);
            await store.SaveAsync(root, imported.DataSet);
            Assert.Equal(2, (await store.LoadAsync(root)).Repairs.Count);
            Assert.All(LegacyFileNames, name => Assert.False(File.Exists(Path.Combine(root.DataPath, name))));
        }
        finally { DeleteTempRoot(temp); }
    }

    [Fact]
    public async Task Repair_and_history_transaction_rolls_back_on_write_failure()
    {
        var temp = CreateTempRoot("vehimap-repair-rollback");
        try
        {
            var root = CreateDataRoot(temp); var store = new SqliteVehimapDataStore(); var data = BuildSampleDataSet();
            var service = new VehicleRepairService(new ResourceAppLocalizer());
            var change = new RepairChange(RepairAction.Create, "veh_1", null, "Závada", "", "01.09.2026", "", 7, "Oprava", "02.09.2026", "", "10");
            var repair = service.Apply(data, change, DateTimeOffset.UtcNow); await store.SaveAsync(root, data);
            using (var connection = new SqliteConnection($"Data Source={SqliteVehimapDataStore.GetDatabasePath(root)};Pooling=False"))
            {
                connection.Open(); using var command = connection.CreateCommand();
                command.CommandText = "CREATE TRIGGER reject_repair BEFORE INSERT ON vehicle_repairs BEGIN SELECT RAISE(ABORT, 'test failure'); END;";
                command.ExecuteNonQuery();
            }
            var before = await store.LoadAsync(root);
            service.Apply(data, change with { Action = RepairAction.Complete, RepairId = repair.Id }, DateTimeOffset.UtcNow);
            await Assert.ThrowsAsync<SqliteException>(() => store.SaveAsync(root, data));
            var after = await store.LoadAsync(root);
            Assert.Equal(VehicleRepair.Waiting, Assert.Single(after.Repairs).State);
            Assert.Equal(before.HistoryEntries, after.HistoryEntries);
        }
        finally { DeleteTempRoot(temp); }
    }
}
