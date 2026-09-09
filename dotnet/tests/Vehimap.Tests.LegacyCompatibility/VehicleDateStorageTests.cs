// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;
using Vehimap.Storage.Sqlite;
using Xunit;

namespace Vehimap.Tests.LegacyCompatibility;

public sealed partial class SqliteStorageCompatibilityTests
{
    [Fact]
    public async Task Vehicle_dates_preserve_precision_across_migration_save_restart_backup_and_package()
    {
        var temp = CreateTempRoot("vehimap-vehicle-dates");
        try
        {
            var root = CreateDataRoot(Path.Combine(temp, "source"));
            var legacy = new LegacyVehimapDataStore();
            var sqlite = new SqliteVehimapDataStore();
            var original = new Vehicle("v", "User vehicle", "Osobní vozidla", "User note", "User model", "TEST", "", "", "07/2025", "07/2027", "07/2026", "07/2027");
            await legacy.SaveAsync(root, new VehimapDataSet { Vehicles = [original] });
            await new SqliteDataMigrationService(legacy, sqlite).MigrateIfNeededAsync(root);
            var data = await sqlite.LoadAsync(root);
            Assert.Equal(original, Assert.Single(data.Vehicles));
            var updated = original with { NextTk = "03.07.2027", GreenCardFrom = "04.07.2026", GreenCardTo = "03.07.2027" };
            data.Vehicles[0] = updated;
            await sqlite.SaveAsync(root, data);
            Assert.Equal(updated, Assert.Single((await new SqliteVehimapDataStore().LoadAsync(root)).Vehicles));
            AssertLiveLegacyFilesAbsent(root);

            var backup = new SqliteBackupService();
            var archive = Path.Combine(temp, "dates.vehimapbak");
            await backup.ExportAsync(archive, root, data);
            var restoreRoot = CreateDataRoot(Path.Combine(temp, "restore"));
            await backup.RestoreAsync(restoreRoot, await backup.ImportAsync(archive));
            Assert.Equal(updated, Assert.Single((await sqlite.LoadAsync(restoreRoot)).Vehicles));
            AssertLiveLegacyFilesAbsent(restoreRoot);

            var package = new VehiclePackageService();
            var packagePath = Path.Combine(temp, "dates.vehimapvehicle");
            await package.ExportVehicleAsync(packagePath, root, data, "v");
            var packageRoot = CreateDataRoot(Path.Combine(temp, "package"));
            var imported = await package.ImportVehicleAsync(packagePath, packageRoot, new VehimapDataSet());
            await sqlite.SaveAsync(packageRoot, imported.DataSet);
            var vehicle = Assert.Single((await sqlite.LoadAsync(packageRoot)).Vehicles);
            Assert.Equal(updated with { Id = vehicle.Id }, vehicle);
        }
        finally
        {
            DeleteTempRoot(temp);
        }
    }
}
