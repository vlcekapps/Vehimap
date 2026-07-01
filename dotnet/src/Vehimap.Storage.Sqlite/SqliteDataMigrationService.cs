// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Storage.Sqlite;

public sealed class SqliteDataMigrationService : IDataMigrationService
{
    private const string RemovedLegacyFilesDirectoryName = "removed-from-data-root";

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

    private readonly ILegacyDataStore _legacyDataStore;
    private readonly IVehimapDataStore _targetDataStore;
    private readonly IAppLocalizer _localizer;

    public SqliteDataMigrationService(ILegacyDataStore legacyDataStore, IVehimapDataStore targetDataStore, IAppLocalizer? localizer = null)
    {
        _legacyDataStore = legacyDataStore;
        _targetDataStore = targetDataStore;
        _localizer = localizer ?? new ResourceAppLocalizer(System.Globalization.CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));
    }

    public async Task<DataMigrationResult> MigrateIfNeededAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(dataRoot.DataPath);

        var databasePath = SqliteStoragePaths.GetDatabasePath(dataRoot);
        if (File.Exists(databasePath))
        {
            var remainingLegacyFiles = GetLiveLegacyFiles(dataRoot);
            if (remainingLegacyFiles.Count > 0)
            {
                var cleanupBackupPath = CreateMigrationBackupDirectory(dataRoot);
                var sqliteData = await _targetDataStore.LoadAsync(dataRoot, cancellationToken).ConfigureAwait(false);
                ArchiveLiveLegacyFiles(dataRoot, cleanupBackupPath, remainingLegacyFiles, cancellationToken);

                sqliteData.Settings.SetValue("migration", "legacy_cleanup_utc", DateTime.UtcNow.ToString("O"));
                sqliteData.Settings.SetValue("migration", "pre_migration_backup_path", cleanupBackupPath);
                sqliteData.Settings.SetValue("migration", "legacy_cleanup_file_count", remainingLegacyFiles.Count.ToString());
                await _targetDataStore.SaveAsync(dataRoot, sqliteData, cancellationToken).ConfigureAwait(false);

                return new DataMigrationResult(
                    false,
                    cleanupBackupPath,
                    LF("DataMigration.LegacyCleanupCompleted", cleanupBackupPath));
            }

            return NotNeeded();
        }

        var legacyFiles = GetLiveLegacyFiles(dataRoot);
        if (legacyFiles.Count == 0)
        {
            return NotNeeded();
        }

        var backupPath = BackupLegacyData(dataRoot, cancellationToken);
        var legacyData = await _legacyDataStore.LoadAsync(dataRoot, cancellationToken).ConfigureAwait(false);
        legacyData.Settings.SetValue("migration", "storage_version", "2.0");
        legacyData.Settings.SetValue("migration", "migrated_utc", DateTime.UtcNow.ToString("O"));
        legacyData.Settings.SetValue("migration", "pre_migration_backup_path", backupPath);
        await _targetDataStore.SaveAsync(dataRoot, legacyData, cancellationToken).ConfigureAwait(false);
        await _targetDataStore.LoadAsync(dataRoot, cancellationToken).ConfigureAwait(false);
        ArchiveLiveLegacyFiles(dataRoot, backupPath, legacyFiles, cancellationToken);

        return new DataMigrationResult(
            true,
            backupPath,
            LF("DataMigration.LegacyMigrationCompleted", backupPath));
    }

    private static IReadOnlyList<string> GetLiveLegacyFiles(VehimapDataRoot dataRoot) =>
        LegacyFileNames
            .Where(fileName => File.Exists(Path.Combine(dataRoot.DataPath, fileName)))
            .ToArray();

    private static string BackupLegacyData(VehimapDataRoot dataRoot, CancellationToken cancellationToken)
    {
        var backupPath = CreateMigrationBackupDirectory(dataRoot);
        foreach (var fileName in LegacyFileNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourcePath = Path.Combine(dataRoot.DataPath, fileName);
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, Path.Combine(backupPath, fileName), overwrite: true);
            }
        }

        var attachmentsRoot = SqliteStoragePaths.GetAttachmentsPath(dataRoot);
        if (Directory.Exists(attachmentsRoot))
        {
            CopyDirectory(attachmentsRoot, Path.Combine(backupPath, SqliteStoragePaths.AttachmentsDirectoryName), cancellationToken);
        }

        return backupPath;
    }

    private static string CreateMigrationBackupDirectory(VehimapDataRoot dataRoot)
    {
        var backupRoot = Path.Combine(dataRoot.DataPath, SqliteStoragePaths.MigrationBackupsDirectoryName);
        Directory.CreateDirectory(backupRoot);

        var baseName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var backupPath = Path.Combine(backupRoot, baseName);
        for (var suffix = 2; Directory.Exists(backupPath); suffix++)
        {
            backupPath = Path.Combine(backupRoot, $"{baseName}-{suffix}");
        }

        Directory.CreateDirectory(backupPath);
        return backupPath;
    }

    private static void ArchiveLiveLegacyFiles(
        VehimapDataRoot dataRoot,
        string backupPath,
        IReadOnlyList<string> legacyFiles,
        CancellationToken cancellationToken)
    {
        if (legacyFiles.Count == 0)
        {
            return;
        }

        var archivePath = Path.Combine(backupPath, RemovedLegacyFilesDirectoryName);
        Directory.CreateDirectory(archivePath);
        foreach (var fileName in legacyFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourcePath = Path.Combine(dataRoot.DataPath, fileName);
            if (!File.Exists(sourcePath))
            {
                continue;
            }

            File.Move(sourcePath, Path.Combine(archivePath, fileName));
        }
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
            var parent = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                Directory.CreateDirectory(parent);
            }

            File.Copy(file, targetPath, overwrite: true);
        }
    }

    private DataMigrationResult NotNeeded() =>
        new(false, null, L("DataMigration.NotNeeded"));

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);
}
