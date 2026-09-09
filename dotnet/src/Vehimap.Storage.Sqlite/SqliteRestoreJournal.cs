// SPDX-License-Identifier: GPL-3.0-or-later
using System.Security.Cryptography;
using System.Text.Json;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;

namespace Vehimap.Storage.Sqlite;

internal sealed record RestoreJournal(int Version, string BackupDirectory, bool HadDatabase,
    bool HadAttachments, string DatabaseHash, string AttachmentsHash, bool Committed);

internal sealed record RestoreJournalEnvelope(RestoreJournal Journal, string Sha256);

internal static class SqliteRestoreJournal
{
    internal const string FileName = ".restore-journal.json";

    public static bool Exists(VehimapDataRoot root) => File.Exists(Path.Combine(root.DataPath, FileName));

    public static void RecoverIfNeeded(VehimapDataRoot root)
    {
        using var lease = SqliteStorageLease.Enter(root);
        RecoverUnderLease(root);
    }

    public static RestoreJournal Prepare(VehimapDataRoot root, string backupPath, bool hadDatabase, bool hadAttachments)
    {
        var journal = new RestoreJournal(1, Path.GetFileName(backupPath), hadDatabase, hadAttachments,
            hadDatabase ? Hash(Path.Combine(backupPath, SqliteStoragePaths.DatabaseFileName)) : string.Empty,
            hadAttachments ? HashTree(Path.Combine(backupPath, SqliteStoragePaths.AttachmentsDirectoryName)) : string.Empty, false);
        Write(root, journal);
        return journal;
    }

    public static void Commit(VehimapDataRoot root, RestoreJournal journal) => Write(root, journal with { Committed = true });

    // Called only under the storage lease, before normal reads, writes or migration.
    // A missing/invalid safety copy fails closed; it never creates an empty dataset.
    public static void RecoverUnderLease(VehimapDataRoot root, Action<string>? checkpoint = null)
    {
        if (!Exists(root)) return;
        var journalPath = SafePath(root.DataPath, FileName);
        if (new FileInfo(journalPath).Length > 4096)
            throw new InvalidDataException("Restore journal is too large; preserve the data folder for recovery.");
        RestoreJournal journal;
        try
        {
            var envelope = JsonSerializer.Deserialize<RestoreJournalEnvelope>(File.ReadAllText(journalPath));
            journal = envelope?.Journal ?? throw new InvalidDataException("Restore journal is empty.");
            if (envelope!.Sha256 != JournalHash(journal))
                throw new InvalidDataException("Restore journal checksum does not match.");
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Restore journal is invalid; preserve the data folder for recovery.", ex);
        }
        if (journal.Version != 1 || !Guid.TryParseExact(journal.BackupDirectory, "N", out _))
            throw new InvalidDataException("Unsupported restore journal; preserve the data folder for recovery.");
        var backupPath = SafePath(root.DataPath, $"{SqliteStoragePaths.ImportBackupsDirectoryName}/{journal.BackupDirectory}");
        if (!Directory.Exists(backupPath))
            throw new InvalidDataException("Restore safety directory is missing.");
        if (!journal.Committed)
        {
            var databaseCopy = SafePath(backupPath, SqliteStoragePaths.DatabaseFileName);
            var attachmentCopy = SafePath(backupPath, SqliteStoragePaths.AttachmentsDirectoryName);
            if (journal.HadDatabase && (!File.Exists(databaseCopy) || Hash(databaseCopy) != journal.DatabaseHash))
                throw new InvalidDataException("Restore safety database is missing or changed.");
            if (journal.HadAttachments && (!Directory.Exists(attachmentCopy) || HashTree(attachmentCopy) != journal.AttachmentsHash))
                throw new InvalidDataException("Restore safety attachments are missing or changed.");

            // Copy before touching live paths. Never consume the safety copy; if this
            // recovery is killed too, the next start repeats the same rollback.
            var recoveredAttachments = SafePath(backupPath, $"recovery-{Guid.NewGuid():N}");
            if (journal.HadAttachments)
                CopyDirectory(attachmentCopy, recoveredAttachments, CancellationToken.None);
            if (journal.HadDatabase)
                SqliteDatabaseSnapshot.Replace(databaseCopy, SafePath(root.DataPath, SqliteStoragePaths.DatabaseFileName));
            else
                QuarantineNewDatabase(root, backupPath);
            checkpoint?.Invoke("rollback-database");

            var activeAttachments = SafePath(root.DataPath, SqliteStoragePaths.AttachmentsDirectoryName);
            if (Directory.Exists(activeAttachments))
                Directory.Move(activeAttachments, SafePath(backupPath, $"interrupted-attachments-{Guid.NewGuid():N}"));
            checkpoint?.Invoke("rollback-attachments-removed");
            if (journal.HadAttachments)
                Directory.Move(recoveredAttachments, activeAttachments);
            checkpoint?.Invoke("rollback-attachments-restored");
        }
        File.Delete(journalPath);
    }

    private static void QuarantineNewDatabase(VehimapDataRoot root, string backupPath)
    {
        var quarantine = SafePath(backupPath, $"interrupted-database-{Guid.NewGuid():N}");
        Directory.CreateDirectory(quarantine);
        foreach (var suffix in new[] { "", "-wal", "-shm", "-journal" })
        {
            var name = SqliteStoragePaths.DatabaseFileName + suffix;
            var source = SafePath(root.DataPath, name);
            if (File.Exists(source)) File.Move(source, Path.Combine(quarantine, name));
        }
    }

    public static void CopyDirectory(string source, string target, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(target);
        // Walk one level at a time, rejecting links before recursing into them.
        foreach (var entry in Directory.EnumerateFileSystemEntries(source))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var attributes = File.GetAttributes(entry);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Restore attachments cannot traverse symbolic links.");
            var destination = SafePath(target, Path.GetFileName(entry));
            if ((attributes & FileAttributes.Directory) != 0)
                CopyDirectory(entry, destination, cancellationToken);
            else
            {
                using var input = File.OpenRead(entry);
                using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                input.CopyTo(output);
                output.Flush(flushToDisk: true);
            }
        }
    }

    public static string SafePath(string root, string relative) =>
        ManagedAttachmentPathGuard.ResolveRelativePathInsideRoot(root, relative);

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static string HashTree(string path)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var entry in Directory.EnumerateFileSystemEntries(path).Order(StringComparer.Ordinal))
        {
            if ((File.GetAttributes(entry) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Restore safety attachments cannot contain symbolic links.");
            var directory = Directory.Exists(entry);
            var identity = JsonSerializer.Serialize(new[] { directory ? "directory" : "file", Path.GetFileName(entry), directory ? HashTree(entry) : Hash(entry) });
            hash.AppendData(System.Text.Encoding.UTF8.GetBytes(identity));
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static void Write(VehimapDataRoot root, RestoreJournal journal)
    {
        var path = SafePath(root.DataPath, FileName);
        var temp = SafePath(root.DataPath, $".restore-journal-{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            {
                JsonSerializer.Serialize(stream, new RestoreJournalEnvelope(journal, JournalHash(journal)));
                stream.Flush(flushToDisk: true);
            }
            File.Move(temp, path, overwrite: true);
        }
        finally { File.Delete(temp); }
    }

    private static string JournalHash(RestoreJournal journal) =>
        Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(journal)));
}
