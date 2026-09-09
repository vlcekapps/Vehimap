// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;
using Vehimap.Storage.Sqlite;

namespace Vehimap.Tests.LegacyCompatibility;

internal static class RestoreProcessWorker
{
    public static async Task<int> Main(string[] args)
    {
        // This executable is only a fault-injection fixture, never shipped with Vehimap.
        if (args.Length != 3 || args[0] is not ("restore" or "recover")) return 2;
        var rootPath = Path.GetFullPath(args[1]);
        if (!rootPath.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase)
            || !File.Exists(Path.Combine(rootPath, "restore-process-fixture"))) return 3;
        var root = new VehimapDataRoot(rootPath, Path.Combine(rootPath, "data"), true);
        void Checkpoint(string phase)
        {
            if (phase != args[2]) return;
            var marker = Path.Combine(rootPath, "checkpoint");
            File.WriteAllText(marker + ".tmp", phase);
            File.Move(marker + ".tmp", marker);
            Thread.Sleep(Timeout.Infinite);
        }
        if (args[0] == "restore")
        {
            await new SqliteBackupService(new SqliteVehimapDataStore(), new LegacyBackupService(), Checkpoint)
                .RestoreAsync(root, new VehimapBackupBundle(Data("Incoming"), [new("attachments/record.txt", [2, 3, 4])]));
        }
        else
        {
            using var lease = SqliteStorageLease.Enter(root);
            SqliteRestoreJournal.RecoverUnderLease(root, Checkpoint);
        }
        return 0;
    }

    internal static VehimapDataSet Data(string name) => new()
    {
        Vehicles = [new("v1", name, "Osobní vozidla", "", "Test", "", "", "", "", "", "", "")],
        Records = [new("r1", "v1", "Doklad", "Original user title", "", "", "", "",
            Vehimap.Domain.Enums.VehicleRecordAttachmentMode.Managed, "attachments/record.txt", "")]
    };
}
