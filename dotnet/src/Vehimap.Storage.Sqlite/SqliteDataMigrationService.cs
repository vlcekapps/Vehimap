using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

namespace Vehimap.Storage.Sqlite;

public sealed class SqliteDataMigrationService : IDataMigrationService
{
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

    public SqliteDataMigrationService(ILegacyDataStore legacyDataStore, IVehimapDataStore targetDataStore)
    {
        _legacyDataStore = legacyDataStore;
        _targetDataStore = targetDataStore;
    }

    public async Task<DataMigrationResult> MigrateIfNeededAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
    {
        var databasePath = SqliteStoragePaths.GetDatabasePath(dataRoot);
        if (File.Exists(databasePath))
        {
            return DataMigrationResult.NotNeeded;
        }

        Directory.CreateDirectory(dataRoot.DataPath);
        if (!HasLegacyData(dataRoot))
        {
            return DataMigrationResult.NotNeeded;
        }

        var backupPath = BackupLegacyData(dataRoot, cancellationToken);
        var legacyData = await _legacyDataStore.LoadAsync(dataRoot, cancellationToken).ConfigureAwait(false);
        legacyData.Settings.SetValue("migration", "storage_version", "2.0");
        legacyData.Settings.SetValue("migration", "migrated_utc", DateTime.UtcNow.ToString("O"));
        legacyData.Settings.SetValue("migration", "pre_migration_backup_path", backupPath);
        await _targetDataStore.SaveAsync(dataRoot, legacyData, cancellationToken).ConfigureAwait(false);

        return new DataMigrationResult(
            true,
            backupPath,
            $"Data byla automaticky migrována do datové sady 2.0. Původní soubory byly odloženy do {backupPath}.");
    }

    private static bool HasLegacyData(VehimapDataRoot dataRoot) =>
        LegacyFileNames.Any(fileName => File.Exists(Path.Combine(dataRoot.DataPath, fileName)));

    private static string BackupLegacyData(VehimapDataRoot dataRoot, CancellationToken cancellationToken)
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
}
