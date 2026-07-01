// SPDX-License-Identifier: GPL-3.0-or-later
using Microsoft.Data.Sqlite;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Storage.Sqlite;

public sealed class SqliteDataStoreHealthService : IDataStoreHealthService
{
    private static readonly string[] ExpectedTables =
    [
        "schema_migrations",
        "settings",
        "vehicles",
        "vehicle_meta",
        "history_entries",
        "fuel_entries",
        "records",
        "reminders",
        "maintenance_plans"
    ];

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

    private readonly IAppLocalizer _localizer;

    public SqliteDataStoreHealthService(IAppLocalizer? localizer = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer(System.Globalization.CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));
    }

    public async Task<DataStoreHealthReport> CheckAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
    {
        var details = new List<string>();
        var status = DataStoreHealthStatus.Healthy;
        var databasePath = SqliteStoragePaths.GetDatabasePath(dataRoot);
        var dataPath = dataRoot.DataPath;
        string? preMigrationBackupPath = null;

        if (!Directory.Exists(dataPath))
        {
            return BuildReport(
                DataStoreHealthStatus.Error,
                L("DataStoreHealth.Report.DataPathMissingSummary"),
                [LF("DataStoreHealth.Report.DataPathMissingDetail", dataPath)],
                databasePath,
                dataPath);
        }

        if (!File.Exists(databasePath))
        {
            return BuildReport(
                DataStoreHealthStatus.Error,
                L("DataStoreHealth.Report.DatabaseMissingSummary"),
                [LF("DataStoreHealth.Report.DatabaseMissingDetail", databasePath)],
                databasePath,
                dataPath);
        }

        CheckDataPathWritable(dataPath, details, ref status);
        CheckLegacyFiles(dataRoot, details, ref status);

        try
        {
            await using var connection = new SqliteConnection(BuildConnectionString(databasePath));
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            status = MaxStatus(
                status,
                await CheckQuickCheckAsync(connection, details, cancellationToken).ConfigureAwait(false));
            var tables = await LoadTableNamesAsync(connection, cancellationToken).ConfigureAwait(false);
            CheckExpectedTables(tables, details, ref status);

            if (ExpectedTables.All(table => tables.Contains(table)))
            {
                status = MaxStatus(
                    status,
                    await CheckSchemaMigrationMarkerAsync(connection, details, cancellationToken).ConfigureAwait(false));
                status = MaxStatus(
                    status,
                    await CheckAttachmentsAsync(dataRoot, connection, details, cancellationToken).ConfigureAwait(false));
                preMigrationBackupPath = await TryReadPreMigrationBackupPathAsync(connection, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            status = DataStoreHealthStatus.Error;
            details.Add(LF("DataStoreHealth.Report.DatabaseCheckFailed", ex.Message));
        }

        var summary = status switch
        {
            DataStoreHealthStatus.Healthy => L("DataStoreHealth.Report.SummaryHealthy"),
            DataStoreHealthStatus.Warning => L("DataStoreHealth.Report.SummaryWarning"),
            _ => L("DataStoreHealth.Report.SummaryError")
        };

        return BuildReport(status, summary, details, databasePath, dataPath, preMigrationBackupPath);
    }

    private static string BuildConnectionString(string databasePath)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = false,
            Pooling = false
        };

        return builder.ToString();
    }

    private void CheckDataPathWritable(string dataPath, List<string> details, ref DataStoreHealthStatus status)
    {
        var probePath = Path.Combine(dataPath, $".vehimap-health-{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(probePath, "Vehimap health check");
            File.Delete(probePath);
            details.Add(L("DataStoreHealth.Report.DataPathWritable"));
        }
        catch (Exception ex)
        {
            status = DataStoreHealthStatus.Error;
            details.Add(LF("DataStoreHealth.Report.DataPathNotWritable", ex.Message));
            try
            {
                if (File.Exists(probePath))
                {
                    File.Delete(probePath);
                }
            }
            catch
            {
            }
        }
    }

    private void CheckLegacyFiles(VehimapDataRoot dataRoot, List<string> details, ref DataStoreHealthStatus status)
    {
        var remaining = LegacyFileNames
            .Where(fileName => File.Exists(Path.Combine(dataRoot.DataPath, fileName)))
            .ToArray();

        if (remaining.Length == 0)
        {
            details.Add(L("DataStoreHealth.Report.NoLiveLegacyFiles"));
            return;
        }

        status = MaxStatus(status, DataStoreHealthStatus.Warning);
        details.Add(LF("DataStoreHealth.Report.LiveLegacyFilesPresent", string.Join(", ", remaining)));
    }

    private async Task<DataStoreHealthStatus> CheckQuickCheckAsync(
        SqliteConnection connection,
        List<string> details,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA quick_check;";
        var result = Convert.ToString(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        if (string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
        {
            details.Add(L("DataStoreHealth.Report.QuickCheckOk"));
            return DataStoreHealthStatus.Healthy;
        }

        details.Add(LF("DataStoreHealth.Report.QuickCheckProblem", result ?? L("DataStoreHealth.Report.NoDetail")));
        return DataStoreHealthStatus.Error;
    }

    private static async Task<HashSet<string>> LoadTableNamesAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var tables = new HashSet<string>(StringComparer.Ordinal);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table';";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    private void CheckExpectedTables(HashSet<string> tables, List<string> details, ref DataStoreHealthStatus status)
    {
        var missing = ExpectedTables
            .Where(table => !tables.Contains(table))
            .ToArray();

        if (missing.Length == 0)
        {
            details.Add(L("DataStoreHealth.Report.ExpectedTablesPresent"));
            return;
        }

        status = DataStoreHealthStatus.Error;
        details.Add(LF("DataStoreHealth.Report.ExpectedTablesMissing", string.Join(", ", missing)));
    }

    private async Task<DataStoreHealthStatus> CheckSchemaMigrationMarkerAsync(
        SqliteConnection connection,
        List<string> details,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM schema_migrations WHERE id = '2.0-initial';";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        if (count > 0)
        {
            details.Add(L("DataStoreHealth.Report.SchemaMarkerPresent"));
            return DataStoreHealthStatus.Healthy;
        }

        details.Add(L("DataStoreHealth.Report.SchemaMarkerMissing"));
        return DataStoreHealthStatus.Error;
    }

    private async Task<DataStoreHealthStatus> CheckAttachmentsAsync(
        VehimapDataRoot dataRoot,
        SqliteConnection connection,
        List<string> details,
        CancellationToken cancellationToken)
    {
        var attachmentsPath = SqliteStoragePaths.GetAttachmentsPath(dataRoot);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM records WHERE attachment_mode = 'managed' AND TRIM(file_path) <> '';";
        var managedCount = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        if (Directory.Exists(attachmentsPath))
        {
            details.Add(LF("DataStoreHealth.Report.AttachmentsFolderExists", attachmentsPath));
            return DataStoreHealthStatus.Healthy;
        }

        if (managedCount > 0)
        {
            details.Add(LF("DataStoreHealth.Report.AttachmentsFolderMissingWithManaged", managedCount, attachmentsPath));
            return DataStoreHealthStatus.Warning;
        }

        details.Add(L("DataStoreHealth.Report.AttachmentsFolderMissingNoManaged"));
        return DataStoreHealthStatus.Healthy;
    }

    private static async Task<string?> TryReadPreMigrationBackupPathAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT setting_value FROM settings WHERE section_name = 'migration' AND setting_key = 'pre_migration_backup_path' LIMIT 1;";
        var value = Convert.ToString(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private DataStoreHealthReport BuildReport(
        DataStoreHealthStatus status,
        string summary,
        IReadOnlyList<string> details,
        string databasePath,
        string dataPath,
        string? preMigrationBackupPath = null) =>
        new(
            status,
            summary,
            details.Count == 0 ? [L("DataStoreHealth.Report.NoDetailsReturned")] : details,
            databasePath,
            dataPath,
            string.IsNullOrWhiteSpace(preMigrationBackupPath) ? null : preMigrationBackupPath);

    private static DataStoreHealthStatus MaxStatus(DataStoreHealthStatus current, DataStoreHealthStatus candidate) =>
        (int)candidate > (int)current ? candidate : current;

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);
}
