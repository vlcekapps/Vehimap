// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;

namespace Vehimap.Storage.Sqlite;

internal static class SqliteStorageLease
{
    // Keep the file after closing: deleting a lock file can create two independent
    // locks on Unix. All Vehimap SQLite readers/writers cooperate with this lease.
    public static FileStream Enter(VehimapDataRoot root)
    {
        Directory.CreateDirectory(root.DataPath);
        var path = ManagedAttachmentPathGuard.ResolveRelativePathInsideRoot(root.DataPath, ".storage.lock");
        return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }

    public static FileStream EnterStable(VehimapDataRoot root)
    {
        var lease = Enter(root);
        if (!SqliteRestoreJournal.Exists(root)) return lease;
        lease.Dispose();
        throw new IOException("Backup recovery is pending; reload the dataset before importing or exporting attachments.");
    }
}
