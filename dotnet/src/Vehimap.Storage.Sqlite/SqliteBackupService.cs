// SPDX-License-Identifier: GPL-3.0-or-later
using System.Text;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;

namespace Vehimap.Storage.Sqlite;

public sealed class SqliteBackupService : IBackupService
{
    private const string ManifestFileName = "manifest.ini";
    private const string ManifestHeader = "# Vehimap backup v7";

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

    private readonly IVehimapDataStore _dataStore;
    private readonly IBackupService _legacyBackupService;
    private readonly Action<string>? _restoreCheckpoint;

    public SqliteBackupService()
        : this(new SqliteVehimapDataStore(), CreateLegacyBackupService(null))
    {
    }

    public SqliteBackupService(IAppLocalizer localizer)
        : this(new SqliteVehimapDataStore(), CreateLegacyBackupService(localizer))
    {
    }

    public SqliteBackupService(IVehimapDataStore dataStore, IBackupService legacyBackupService)
        : this(dataStore, legacyBackupService, null)
    {
    }

    internal SqliteBackupService(IVehimapDataStore dataStore, IBackupService legacyBackupService, Action<string>? restoreCheckpoint)
    {
        _dataStore = dataStore;
        _legacyBackupService = legacyBackupService;
        _restoreCheckpoint = restoreCheckpoint;
    }

    private static IBackupService CreateLegacyBackupService(IAppLocalizer? localizer) =>
        new LegacyBackupService(localizer);

    public async Task<BackupExportResult> ExportAsync(
        string backupPath,
        VehimapDataRoot dataRoot,
        VehimapDataSet dataSet,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var lease = SqliteStorageLease.EnterStable(dataRoot);
        var tempDirectory = CreateTemporaryDirectory("vehimap-backup");
        try
        {
            var tempRoot = new VehimapDataRoot(tempDirectory, tempDirectory, true);
            await _dataStore.SaveAsync(tempRoot, dataSet, cancellationToken).ConfigureAwait(false);

            var attachments = await CopyReferencedManagedAttachmentsAsync(
                    dataRoot,
                    dataSet,
                    Path.Combine(tempDirectory, SqliteStoragePaths.AttachmentsDirectoryName),
                    cancellationToken)
                .ConfigureAwait(false);

            await File.WriteAllTextAsync(
                    Path.Combine(tempDirectory, ManifestFileName),
                    BuildManifest(),
                    new UTF8Encoding(false),
                    cancellationToken)
                .ConfigureAwait(false);

            var targetDirectory = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            AtomicArchiveWriter.CreateFromDirectory(tempDirectory, backupPath, cancellationToken);
            return new BackupExportResult(backupPath, attachments.IncludedCount, attachments.MissingCount);
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    public async Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (SafeDataArchive.HasLegacyBackupHeader(backupPath))
            return await _legacyBackupService.ImportAsync(backupPath, cancellationToken).ConfigureAwait(false);
        var tempDirectory = CreateTemporaryDirectory("vehimap-backup-import");
        try
        {
            await SafeDataArchive.ExtractAsync(backupPath, tempDirectory, cancellationToken).ConfigureAwait(false);

            if (!IsSqliteBackup(tempDirectory))
            {
                throw new InvalidDataException("Archive is not a supported SQLite backup.");
            }

            var tempRoot = new VehimapDataRoot(tempDirectory, tempDirectory, true);
            var data = await _dataStore.LoadAsync(tempRoot, cancellationToken).ConfigureAwait(false);
            var attachments = await ReadAttachmentsAsync(tempRoot, cancellationToken).ConfigureAwait(false);
            return new VehimapBackupBundle(data, attachments);
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    public async Task<BackupRestoreResult> RestoreAsync(
        VehimapDataRoot dataRoot,
        VehimapBackupBundle backupBundle,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(dataRoot.DataPath);
        var stagingPath = Path.Combine(dataRoot.DataPath, $".restore-{Guid.NewGuid():N}");
        var stagingRoot = new VehimapDataRoot(stagingPath, stagingPath, true);
        Directory.CreateDirectory(SqliteStoragePaths.GetAttachmentsPath(stagingRoot));
        try
        {
            var paths = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            foreach (var attachment in backupBundle.Attachments)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var targetPath = SqliteStoragePaths.ResolveManagedAttachmentPath(stagingRoot, attachment.RelativePath);
                if (string.IsNullOrEmpty(targetPath) || !paths.Add(targetPath))
                    throw new InvalidDataException("Backup contains an empty or duplicate attachment path.");
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                await File.WriteAllBytesAsync(targetPath, attachment.Content, cancellationToken).ConfigureAwait(false);
            }
            // Validate the incoming database before touching any live file, including
            // malformed rows that would cause the real SQLite transaction to fail.
            await _dataStore.SaveAsync(stagingRoot, backupBundle.Data, cancellationToken).ConfigureAwait(false);
            var verified = await _dataStore.LoadAsync(stagingRoot, cancellationToken).ConfigureAwait(false);
            SqliteDataMigrationService.VerifyRoundTrip(backupBundle.Data, verified);
            using var lease = SqliteStorageLease.Enter(dataRoot);
            SqliteRestoreJournal.RecoverUnderLease(dataRoot);
            var preRestoreBackupPath = BackupCurrentDataBeforeRestore(dataRoot, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var attachmentsRoot = SqliteRestoreJournal.SafePath(dataRoot.DataPath, SqliteStoragePaths.AttachmentsDirectoryName);
            var previousAttachments = Path.Combine(preRestoreBackupPath, "replaced-attachments");
            var hadAttachments = Directory.Exists(attachmentsRoot);
            var journal = SqliteRestoreJournal.Prepare(dataRoot, preRestoreBackupPath,
                File.Exists(SqliteStoragePaths.GetDatabasePath(dataRoot)), hadAttachments);
            try
            {
                _restoreCheckpoint?.Invoke("prepared");
                if (hadAttachments)
                    Directory.Move(attachmentsRoot, previousAttachments);
                _restoreCheckpoint?.Invoke("attachments-removed");
                Directory.Move(SqliteStoragePaths.GetAttachmentsPath(stagingRoot), attachmentsRoot);
                _restoreCheckpoint?.Invoke("attachments-installed");
                cancellationToken.ThrowIfCancellationRequested();
                SqliteDatabaseSnapshot.Replace(SqliteStoragePaths.GetDatabasePath(stagingRoot),
                    SqliteRestoreJournal.SafePath(dataRoot.DataPath, SqliteStoragePaths.DatabaseFileName));
                _restoreCheckpoint?.Invoke("database-installed");
                cancellationToken.ThrowIfCancellationRequested();
                SqliteRestoreJournal.Commit(dataRoot, journal);
                _restoreCheckpoint?.Invoke("committed");
            }
            catch
            {
                // A failed rollback keeps the journal and all safety files, so normal
                // storage access fails closed or retries recovery on the next start.
                SqliteRestoreJournal.RecoverUnderLease(dataRoot);
                throw;
            }
            SqliteRestoreJournal.RecoverUnderLease(dataRoot);
            return new BackupRestoreResult(preRestoreBackupPath, paths.Count);
        }
        finally
        {
            if (!SqliteRestoreJournal.Exists(dataRoot)) TryDeleteDirectory(stagingPath);
        }
    }

    private static string BuildManifest()
    {
        var builder = new StringBuilder();
        builder.AppendLine(ManifestHeader);
        builder.AppendLine("storage=sqlite");
        builder.AppendLine("schema=2.0");
        builder.AppendLine("database=vehimap.db");
        builder.AppendLine("attachments=attachments");
        builder.AppendLine($"created_utc={DateTime.UtcNow:O}");
        return builder.ToString();
    }

    private static bool IsSqliteBackup(string directory)
    {
        var manifestPath = Path.Combine(directory, ManifestFileName);
        var databasePath = Path.Combine(directory, SqliteStoragePaths.DatabaseFileName);
        if (!File.Exists(manifestPath) || !File.Exists(databasePath))
        {
            return false;
        }

        var firstLine = File.ReadLines(manifestPath, Encoding.UTF8).FirstOrDefault() ?? string.Empty;
        return string.Equals(firstLine.Trim(), ManifestHeader, StringComparison.Ordinal);
    }

    private static async Task<IReadOnlyList<ManagedAttachment>> ReadAttachmentsAsync(
        VehimapDataRoot tempRoot,
        CancellationToken cancellationToken)
    {
        var attachmentsRoot = SqliteStoragePaths.GetAttachmentsPath(tempRoot);
        if (!Directory.Exists(attachmentsRoot))
        {
            return [];
        }

        var attachments = new List<ManagedAttachment>();
        foreach (var file in Directory.GetFiles(attachmentsRoot, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativeToAttachments = Path.GetRelativePath(attachmentsRoot, file).Replace('\\', '/');
            var relativePath = $"{SqliteStoragePaths.AttachmentsDirectoryName}/{relativeToAttachments}";
            var content = await File.ReadAllBytesAsync(file, cancellationToken).ConfigureAwait(false);
            attachments.Add(new ManagedAttachment(relativePath, content));
        }

        return attachments;
    }

    private static async Task<AttachmentCopyResult> CopyReferencedManagedAttachmentsAsync(
        VehimapDataRoot dataRoot,
        VehimapDataSet dataSet,
        string targetAttachmentsRoot,
        CancellationToken cancellationToken)
    {
        var seen = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var includedCount = 0;
        var missingCount = 0;

        foreach (var record in dataSet.Records.Where(record => record.AttachmentMode == VehicleRecordAttachmentMode.Managed))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = SqliteStoragePaths.NormalizeAttachmentRelativePath(record.FilePath);
            if (string.IsNullOrWhiteSpace(relativePath) || !seen.Add(relativePath))
            {
                continue;
            }

            var sourcePath = SqliteStoragePaths.ResolveManagedAttachmentPath(dataRoot, relativePath);
            if (!File.Exists(sourcePath))
            {
                missingCount++;
                continue;
            }

            var relativeInAttachments = StripAttachmentsPrefix(relativePath);
            var targetPath = ManagedAttachmentPathGuard.ResolveRelativePathInsideRoot(targetAttachmentsRoot, relativeInAttachments);
            var targetParent = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetParent))
            {
                Directory.CreateDirectory(targetParent);
            }

            File.Copy(sourcePath, targetPath, overwrite: true);
            includedCount++;
        }

        await Task.CompletedTask.ConfigureAwait(false);
        return new AttachmentCopyResult(includedCount, missingCount);
    }

    private static string BackupCurrentDataBeforeRestore(VehimapDataRoot dataRoot, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(dataRoot.DataPath);

        var backupRoot = SqliteRestoreJournal.SafePath(dataRoot.DataPath, SqliteStoragePaths.ImportBackupsDirectoryName);
        Directory.CreateDirectory(backupRoot);
        var backupPath = Path.Combine(backupRoot, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(backupPath);

        var databasePath = SqliteRestoreJournal.SafePath(dataRoot.DataPath, SqliteStoragePaths.DatabaseFileName);
        if (File.Exists(databasePath))
        {
            SqliteDatabaseSnapshot.Copy(databasePath, Path.Combine(backupPath, SqliteStoragePaths.DatabaseFileName));
        }

        foreach (var legacyFileName in LegacyFileNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourcePath = Path.Combine(dataRoot.DataPath, legacyFileName);
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, Path.Combine(backupPath, legacyFileName), overwrite: true);
            }
        }

        var attachmentsRoot = SqliteRestoreJournal.SafePath(dataRoot.DataPath, SqliteStoragePaths.AttachmentsDirectoryName);
        if (Directory.Exists(attachmentsRoot))
        {
            SqliteRestoreJournal.CopyDirectory(attachmentsRoot, Path.Combine(backupPath, SqliteStoragePaths.AttachmentsDirectoryName), cancellationToken);
        }

        return backupPath;
    }

    private static string StripAttachmentsPrefix(string relativePath)
    {
        var normalized = SqliteStoragePaths.NormalizeAttachmentRelativePath(relativePath);
        var prefix = $"{SqliteStoragePaths.AttachmentsDirectoryName}/";
        return normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? normalized[prefix.Length..]
            : normalized;
    }

    private static string CreateTemporaryDirectory(string prefix)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{prefix}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch
        {
        }
    }

    private sealed record AttachmentCopyResult(int IncludedCount, int MissingCount);
}
