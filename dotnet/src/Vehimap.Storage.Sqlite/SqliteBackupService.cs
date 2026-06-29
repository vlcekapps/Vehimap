using System.IO.Compression;
using System.Text;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
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

    public SqliteBackupService()
        : this(new SqliteVehimapDataStore(), new LegacyBackupService())
    {
    }

    public SqliteBackupService(IVehimapDataStore dataStore, IBackupService legacyBackupService)
    {
        _dataStore = dataStore;
        _legacyBackupService = legacyBackupService;
    }

    public async Task<BackupExportResult> ExportAsync(
        string backupPath,
        VehimapDataRoot dataRoot,
        VehimapDataSet dataSet,
        CancellationToken cancellationToken = default)
    {
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

            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            ZipFile.CreateFromDirectory(tempDirectory, backupPath, CompressionLevel.Optimal, includeBaseDirectory: false);
            return new BackupExportResult(backupPath, attachments.IncludedCount, attachments.MissingCount);
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    public async Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        var tempDirectory = CreateTemporaryDirectory("vehimap-backup-import");
        try
        {
            try
            {
                ZipFile.ExtractToDirectory(backupPath, tempDirectory);
            }
            catch (InvalidDataException)
            {
                TryDeleteDirectory(tempDirectory);
                return await _legacyBackupService.ImportAsync(backupPath, cancellationToken).ConfigureAwait(false);
            }

            if (!IsSqliteBackup(tempDirectory))
            {
                TryDeleteDirectory(tempDirectory);
                return await _legacyBackupService.ImportAsync(backupPath, cancellationToken).ConfigureAwait(false);
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
        var preRestoreBackupPath = BackupCurrentDataBeforeRestore(dataRoot, cancellationToken);

        await _dataStore.SaveAsync(dataRoot, backupBundle.Data, cancellationToken).ConfigureAwait(false);
        var attachmentsRoot = SqliteStoragePaths.GetAttachmentsPath(dataRoot);
        if (Directory.Exists(attachmentsRoot))
        {
            Directory.Delete(attachmentsRoot, recursive: true);
        }

        var restoredAttachmentCount = 0;
        foreach (var attachment in backupBundle.Attachments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetPath = SqliteStoragePaths.ResolveManagedAttachmentPath(dataRoot, attachment.RelativePath);
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                continue;
            }

            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(targetPath, attachment.Content, cancellationToken).ConfigureAwait(false);
            restoredAttachmentCount++;
        }

        return new BackupRestoreResult(preRestoreBackupPath, restoredAttachmentCount);
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
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
            var targetPath = Path.Combine(targetAttachmentsRoot, relativeInAttachments.Replace('/', Path.DirectorySeparatorChar));
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

        var backupRoot = Path.Combine(dataRoot.DataPath, SqliteStoragePaths.ImportBackupsDirectoryName);
        Directory.CreateDirectory(backupRoot);

        var baseName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var backupPath = Path.Combine(backupRoot, baseName);
        for (var suffix = 2; Directory.Exists(backupPath); suffix++)
        {
            backupPath = Path.Combine(backupRoot, $"{baseName}-{suffix}");
        }

        Directory.CreateDirectory(backupPath);

        var databasePath = SqliteStoragePaths.GetDatabasePath(dataRoot);
        if (File.Exists(databasePath))
        {
            File.Copy(databasePath, Path.Combine(backupPath, SqliteStoragePaths.DatabaseFileName), overwrite: true);
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

        var attachmentsRoot = SqliteStoragePaths.GetAttachmentsPath(dataRoot);
        if (Directory.Exists(attachmentsRoot))
        {
            CopyDirectory(attachmentsRoot, Path.Combine(backupPath, SqliteStoragePaths.AttachmentsDirectoryName), cancellationToken);
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

    private static void CopyDirectory(string sourceDirectory, string targetDirectory, CancellationToken cancellationToken)
    {
        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(targetDirectory, relativePath));
        }

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            var targetPath = Path.Combine(targetDirectory, relativePath);
            var targetParent = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetParent))
            {
                Directory.CreateDirectory(targetParent);
            }

            File.Copy(file, targetPath, overwrite: true);
        }
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
