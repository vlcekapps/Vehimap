using Microsoft.Data.Sqlite;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;

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
                "Datová složka neexistuje.",
                [$"Datová složka nebyla nalezena: {dataPath}"],
                databasePath,
                dataPath);
        }

        if (!File.Exists(databasePath))
        {
            return BuildReport(
                DataStoreHealthStatus.Error,
                "Databáze datové sady 2.0 nebyla nalezena.",
                [$"Soubor databáze nebyl nalezen: {databasePath}"],
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
            details.Add($"Databázi se nepodařilo otevřít nebo zkontrolovat: {ex.Message}");
        }

        var summary = status switch
        {
            DataStoreHealthStatus.Healthy => "Datová sada 2.0 je v pořádku.",
            DataStoreHealthStatus.Warning => "Datová sada 2.0 je použitelná, ale vyžaduje pozornost.",
            _ => "Datová sada 2.0 má problém. Data nebyla automaticky opravena ani smazána."
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

    private static void CheckDataPathWritable(string dataPath, List<string> details, ref DataStoreHealthStatus status)
    {
        var probePath = Path.Combine(dataPath, $".vehimap-health-{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(probePath, "Vehimap health check");
            File.Delete(probePath);
            details.Add("Datová složka je zapisovatelná.");
        }
        catch (Exception ex)
        {
            status = DataStoreHealthStatus.Error;
            details.Add($"Datová složka není zapisovatelná: {ex.Message}");
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

    private static void CheckLegacyFiles(VehimapDataRoot dataRoot, List<string> details, ref DataStoreHealthStatus status)
    {
        var remaining = LegacyFileNames
            .Where(fileName => File.Exists(Path.Combine(dataRoot.DataPath, fileName)))
            .ToArray();

        if (remaining.Length == 0)
        {
            details.Add("V živé datové složce nejsou legacy TSV/INI soubory.");
            return;
        }

        status = MaxStatus(status, DataStoreHealthStatus.Warning);
        details.Add($"V živé datové složce zůstaly legacy TSV/INI soubory: {string.Join(", ", remaining)}.");
    }

    private static async Task<DataStoreHealthStatus> CheckQuickCheckAsync(
        SqliteConnection connection,
        List<string> details,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA quick_check;";
        var result = Convert.ToString(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        if (string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
        {
            details.Add("SQLite quick_check je v pořádku.");
            return DataStoreHealthStatus.Healthy;
        }

        details.Add($"SQLite quick_check vrátil problém: {result ?? "bez detailu"}.");
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

    private static void CheckExpectedTables(HashSet<string> tables, List<string> details, ref DataStoreHealthStatus status)
    {
        var missing = ExpectedTables
            .Where(table => !tables.Contains(table))
            .ToArray();

        if (missing.Length == 0)
        {
            details.Add("SQLite obsahuje všechny očekávané tabulky datové sady 2.0.");
            return;
        }

        status = DataStoreHealthStatus.Error;
        details.Add($"SQLite databázi chybí očekávané tabulky: {string.Join(", ", missing)}.");
    }

    private static async Task<DataStoreHealthStatus> CheckSchemaMigrationMarkerAsync(
        SqliteConnection connection,
        List<string> details,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM schema_migrations WHERE id = '2.0-initial';";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        if (count > 0)
        {
            details.Add("Schema marker 2.0-initial je přítomný.");
            return DataStoreHealthStatus.Healthy;
        }

        details.Add("SQLite databázi chybí schema marker 2.0-initial.");
        return DataStoreHealthStatus.Error;
    }

    private static async Task<DataStoreHealthStatus> CheckAttachmentsAsync(
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
            details.Add($"Složka spravovaných příloh existuje: {attachmentsPath}");
            return DataStoreHealthStatus.Healthy;
        }

        if (managedCount > 0)
        {
            details.Add($"Složka spravovaných příloh chybí, ale databáze odkazuje na {managedCount} spravovaných příloh: {attachmentsPath}");
            return DataStoreHealthStatus.Warning;
        }

        details.Add("Složka spravovaných příloh zatím neexistuje; vytvoří se při první spravované příloze.");
        return DataStoreHealthStatus.Healthy;
    }

    private static async Task<string?> TryReadPreMigrationBackupPathAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT setting_value FROM settings WHERE section_name = 'migration' AND setting_key = 'pre_migration_backup_path' LIMIT 1;";
        var value = Convert.ToString(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DataStoreHealthReport BuildReport(
        DataStoreHealthStatus status,
        string summary,
        IReadOnlyList<string> details,
        string databasePath,
        string dataPath,
        string? preMigrationBackupPath = null) =>
        new(
            status,
            summary,
            details.Count == 0 ? ["Kontrola nevrátila žádné další detaily."] : details,
            databasePath,
            dataPath,
            string.IsNullOrWhiteSpace(preMigrationBackupPath) ? null : preMigrationBackupPath);

    private static DataStoreHealthStatus MaxStatus(DataStoreHealthStatus current, DataStoreHealthStatus candidate) =>
        (int)candidate > (int)current ? candidate : current;
}
