// SPDX-License-Identifier: GPL-3.0-or-later
using Microsoft.Data.Sqlite;

namespace Vehimap.Storage.Sqlite;

internal static class SqliteDatabaseSnapshot
{
    public static void Replace(string sourcePath, string destinationPath)
    {
        // Restore through SQLite, not a file swap: live WAL connections must not
        // become paired with the journal of a different database.
        using var source = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = sourcePath, Mode = SqliteOpenMode.ReadOnly, Pooling = false
        }.ToString());
        using var destination = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = destinationPath, Pooling = false
        }.ToString());
        source.Open();
        destination.Open();
        using var command = destination.CreateCommand();
        command.CommandText = "PRAGMA synchronous=FULL;";
        command.ExecuteNonQuery();
        source.BackupDatabase(destination);
    }

    public static void Copy(string sourcePath, string destinationPath)
    {
        var temporaryPath = destinationPath + $".{Guid.NewGuid():N}.tmp";
        try
        {
            // SQLite's backup API includes committed WAL pages, unlike File.Copy(db).
            using (var source = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = sourcePath, Mode = SqliteOpenMode.ReadOnly, Pooling = false
            }.ToString()))
            using (var destination = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = temporaryPath, Pooling = false
            }.ToString()))
            {
                source.Open();
                destination.Open();
                source.BackupDatabase(destination);
                using var command = destination.CreateCommand();
                command.CommandText = "PRAGMA journal_mode=DELETE;";
                command.ExecuteNonQuery();
            }
            using (var stream = new FileStream(temporaryPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                stream.Flush(flushToDisk: true);
            File.Move(temporaryPath, destinationPath, overwrite: false);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }
}
