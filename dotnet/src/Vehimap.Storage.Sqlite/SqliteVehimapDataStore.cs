// SPDX-License-Identifier: GPL-3.0-or-later
using Microsoft.Data.Sqlite;
using Vehimap.Application.Abstractions;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Storage.Sqlite;

public sealed class SqliteVehimapDataStore : IVehimapDataStore
{
    public async Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(dataRoot.DataPath);
        await using var connection = new SqliteConnection(BuildConnectionString(dataRoot));
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnsureSchemaAsync(connection, cancellationToken).ConfigureAwait(false);

        return new VehimapDataSet
        {
            Settings = await LoadSettingsAsync(connection, cancellationToken).ConfigureAwait(false),
            Vehicles = await LoadVehiclesAsync(connection, cancellationToken).ConfigureAwait(false),
            HistoryEntries = await LoadHistoryAsync(connection, cancellationToken).ConfigureAwait(false),
            FuelEntries = await LoadFuelAsync(connection, cancellationToken).ConfigureAwait(false),
            Records = await LoadRecordsAsync(connection, cancellationToken).ConfigureAwait(false),
            VehicleMetaEntries = await LoadVehicleMetaAsync(connection, cancellationToken).ConfigureAwait(false),
            Reminders = await LoadRemindersAsync(connection, cancellationToken).ConfigureAwait(false),
            MaintenancePlans = await LoadMaintenancePlansAsync(connection, cancellationToken).ConfigureAwait(false)
        };
    }

    public async Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(dataRoot.DataPath);
        await using var connection = new SqliteConnection(BuildConnectionString(dataRoot));
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await EnsureSchemaAsync(connection, cancellationToken).ConfigureAwait(false);

        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            foreach (var table in new[]
                     {
                         "settings",
                         "vehicles",
                         "vehicle_meta",
                         "history_entries",
                         "fuel_entries",
                         "records",
                         "reminders",
                         "maintenance_plans"
                     })
            {
                await ExecuteAsync(connection, transaction, $"DELETE FROM {table};", cancellationToken).ConfigureAwait(false);
            }

            foreach (var (section, values) in dataSet.Settings.Sections)
            {
                foreach (var (key, value) in values)
                {
                    await ExecuteAsync(
                            connection,
                            transaction,
                            "INSERT INTO settings(section_name, setting_key, setting_value) VALUES ($section, $key, $value);",
                            cancellationToken,
                            ("$section", section),
                            ("$key", key),
                            ("$value", value))
                        .ConfigureAwait(false);
                }
            }

            foreach (var vehicle in dataSet.Vehicles)
            {
                await ExecuteAsync(
                        connection,
                        transaction,
                        """
                        INSERT INTO vehicles(id, name, category, vehicle_note, make_model, plate, year, power, last_tk, next_tk, green_card_from, green_card_to)
                        VALUES ($id, $name, $category, $vehicle_note, $make_model, $plate, $year, $power, $last_tk, $next_tk, $green_card_from, $green_card_to);
                        """,
                        cancellationToken,
                        ("$id", vehicle.Id),
                        ("$name", vehicle.Name),
                        ("$category", vehicle.Category),
                        ("$vehicle_note", vehicle.VehicleNote),
                        ("$make_model", vehicle.MakeModel),
                        ("$plate", vehicle.Plate),
                        ("$year", vehicle.Year),
                        ("$power", vehicle.Power),
                        ("$last_tk", vehicle.LastTk),
                        ("$next_tk", vehicle.NextTk),
                        ("$green_card_from", vehicle.GreenCardFrom),
                        ("$green_card_to", vehicle.GreenCardTo))
                    .ConfigureAwait(false);
            }

            foreach (var meta in dataSet.VehicleMetaEntries)
            {
                await ExecuteAsync(
                        connection,
                        transaction,
                        """
                        INSERT INTO vehicle_meta(vehicle_id, state, tags, powertrain, climate_profile, timing_drive, transmission)
                        VALUES ($vehicle_id, $state, $tags, $powertrain, $climate_profile, $timing_drive, $transmission);
                        """,
                        cancellationToken,
                        ("$vehicle_id", meta.VehicleId),
                        ("$state", meta.State),
                        ("$tags", meta.Tags),
                        ("$powertrain", meta.Powertrain),
                        ("$climate_profile", meta.ClimateProfile),
                        ("$timing_drive", meta.TimingDrive),
                        ("$transmission", meta.Transmission))
                    .ConfigureAwait(false);
            }

            foreach (var item in dataSet.HistoryEntries)
            {
                await ExecuteAsync(
                        connection,
                        transaction,
                        """
                        INSERT INTO history_entries(id, vehicle_id, event_date, event_type, odometer, cost, note)
                        VALUES ($id, $vehicle_id, $event_date, $event_type, $odometer, $cost, $note);
                        """,
                        cancellationToken,
                        ("$id", item.Id),
                        ("$vehicle_id", item.VehicleId),
                        ("$event_date", item.EventDate),
                        ("$event_type", item.EventType),
                        ("$odometer", item.Odometer),
                        ("$cost", item.Cost),
                        ("$note", item.Note))
                    .ConfigureAwait(false);
            }

            foreach (var item in dataSet.FuelEntries)
            {
                await ExecuteAsync(
                        connection,
                        transaction,
                        """
                        INSERT INTO fuel_entries(id, vehicle_id, entry_date, odometer, liters, total_cost, full_tank, fuel_type, fuel_detail, station, note)
                        VALUES ($id, $vehicle_id, $entry_date, $odometer, $liters, $total_cost, $full_tank, $fuel_type, $fuel_detail, $station, $note);
                        """,
                        cancellationToken,
                        ("$id", item.Id),
                        ("$vehicle_id", item.VehicleId),
                        ("$entry_date", item.EntryDate),
                        ("$odometer", item.Odometer),
                        ("$liters", item.Liters),
                        ("$total_cost", item.TotalCost),
                        ("$full_tank", item.FullTank ? 1 : 0),
                        ("$fuel_type", item.FuelType),
                        ("$fuel_detail", item.FuelDetail),
                        ("$station", item.Station),
                        ("$note", item.Note))
                    .ConfigureAwait(false);
            }

            foreach (var item in dataSet.Records)
            {
                await ExecuteAsync(
                        connection,
                        transaction,
                        """
                        INSERT INTO records(id, vehicle_id, record_type, title, provider, valid_from, valid_to, price, attachment_mode, file_path, note)
                        VALUES ($id, $vehicle_id, $record_type, $title, $provider, $valid_from, $valid_to, $price, $attachment_mode, $file_path, $note);
                        """,
                        cancellationToken,
                        ("$id", item.Id),
                        ("$vehicle_id", item.VehicleId),
                        ("$record_type", item.RecordType),
                        ("$title", item.Title),
                        ("$provider", item.Provider),
                        ("$valid_from", item.ValidFrom),
                        ("$valid_to", item.ValidTo),
                        ("$price", item.Price),
                        ("$attachment_mode", item.AttachmentMode == VehicleRecordAttachmentMode.Managed ? "managed" : "external"),
                        ("$file_path", item.FilePath),
                        ("$note", item.Note))
                    .ConfigureAwait(false);
            }

            foreach (var item in dataSet.Reminders)
            {
                await ExecuteAsync(
                        connection,
                        transaction,
                        """
                        INSERT INTO reminders(id, vehicle_id, title, due_date, reminder_days, repeat_mode, note)
                        VALUES ($id, $vehicle_id, $title, $due_date, $reminder_days, $repeat_mode, $note);
                        """,
                        cancellationToken,
                        ("$id", item.Id),
                        ("$vehicle_id", item.VehicleId),
                        ("$title", item.Title),
                        ("$due_date", item.DueDate),
                        ("$reminder_days", item.ReminderDays),
                        ("$repeat_mode", item.RepeatMode),
                        ("$note", item.Note))
                    .ConfigureAwait(false);
            }

            foreach (var item in dataSet.MaintenancePlans)
            {
                await ExecuteAsync(
                        connection,
                        transaction,
                        """
                        INSERT INTO maintenance_plans(id, vehicle_id, title, interval_km, interval_months, last_service_date, last_service_odometer, is_active, note)
                        VALUES ($id, $vehicle_id, $title, $interval_km, $interval_months, $last_service_date, $last_service_odometer, $is_active, $note);
                        """,
                        cancellationToken,
                        ("$id", item.Id),
                        ("$vehicle_id", item.VehicleId),
                        ("$title", item.Title),
                        ("$interval_km", item.IntervalKm),
                        ("$interval_months", item.IntervalMonths),
                        ("$last_service_date", item.LastServiceDate),
                        ("$last_service_odometer", item.LastServiceOdometer),
                        ("$is_active", item.IsActive ? 1 : 0),
                        ("$note", item.Note))
                    .ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public static string GetDatabasePath(VehimapDataRoot dataRoot) =>
        SqliteStoragePaths.GetDatabasePath(dataRoot);

    private static string BuildConnectionString(VehimapDataRoot dataRoot)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = SqliteStoragePaths.GetDatabasePath(dataRoot),
            ForeignKeys = false,
            Pooling = false
        };

        return builder.ToString();
    }

    private static async Task EnsureSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var schema = """
        PRAGMA journal_mode=WAL;

        CREATE TABLE IF NOT EXISTS schema_migrations(
            id TEXT PRIMARY KEY,
            applied_utc TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS settings(
            section_name TEXT NOT NULL,
            setting_key TEXT NOT NULL,
            setting_value TEXT NOT NULL,
            PRIMARY KEY(section_name, setting_key)
        );

        CREATE TABLE IF NOT EXISTS vehicles(
            id TEXT PRIMARY KEY,
            name TEXT NOT NULL,
            category TEXT NOT NULL,
            vehicle_note TEXT NOT NULL,
            make_model TEXT NOT NULL,
            plate TEXT NOT NULL,
            year TEXT NOT NULL,
            power TEXT NOT NULL,
            last_tk TEXT NOT NULL,
            next_tk TEXT NOT NULL,
            green_card_from TEXT NOT NULL,
            green_card_to TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS vehicle_meta(
            vehicle_id TEXT PRIMARY KEY,
            state TEXT NOT NULL,
            tags TEXT NOT NULL,
            powertrain TEXT NOT NULL,
            climate_profile TEXT NOT NULL,
            timing_drive TEXT NOT NULL,
            transmission TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS history_entries(
            id TEXT PRIMARY KEY,
            vehicle_id TEXT NOT NULL,
            event_date TEXT NOT NULL,
            event_type TEXT NOT NULL,
            odometer TEXT NOT NULL,
            cost TEXT NOT NULL,
            note TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS fuel_entries(
            id TEXT PRIMARY KEY,
            vehicle_id TEXT NOT NULL,
            entry_date TEXT NOT NULL,
            odometer TEXT NOT NULL,
            liters TEXT NOT NULL,
            total_cost TEXT NOT NULL,
            full_tank INTEGER NOT NULL,
            fuel_type TEXT NOT NULL,
            fuel_detail TEXT NOT NULL,
            station TEXT NOT NULL,
            note TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS records(
            id TEXT PRIMARY KEY,
            vehicle_id TEXT NOT NULL,
            record_type TEXT NOT NULL,
            title TEXT NOT NULL,
            provider TEXT NOT NULL,
            valid_from TEXT NOT NULL,
            valid_to TEXT NOT NULL,
            price TEXT NOT NULL,
            attachment_mode TEXT NOT NULL,
            file_path TEXT NOT NULL,
            note TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS reminders(
            id TEXT PRIMARY KEY,
            vehicle_id TEXT NOT NULL,
            title TEXT NOT NULL,
            due_date TEXT NOT NULL,
            reminder_days TEXT NOT NULL,
            repeat_mode TEXT NOT NULL,
            note TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS maintenance_plans(
            id TEXT PRIMARY KEY,
            vehicle_id TEXT NOT NULL,
            title TEXT NOT NULL,
            interval_km TEXT NOT NULL,
            interval_months TEXT NOT NULL,
            last_service_date TEXT NOT NULL,
            last_service_odometer TEXT NOT NULL,
            is_active INTEGER NOT NULL,
            note TEXT NOT NULL
        );

        INSERT OR IGNORE INTO schema_migrations(id, applied_utc)
        VALUES ('2.0-initial', strftime('%Y-%m-%dT%H:%M:%fZ', 'now'));
        """;

        await ExecuteAsync(connection, null, schema, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<VehimapSettings> LoadSettingsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var settings = new VehimapSettings();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT section_name, setting_key, setting_value FROM settings ORDER BY section_name, setting_key;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            settings.SetValue(reader.GetString(0), reader.GetString(1), reader.GetString(2));
        }

        return settings;
    }

    private static async Task<List<Vehicle>> LoadVehiclesAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var items = new List<Vehicle>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, category, vehicle_note, make_model, plate, year, power, last_tk, next_tk, green_card_from, green_card_to FROM vehicles ORDER BY rowid;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new Vehicle(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                reader.GetString(7),
                reader.GetString(8),
                reader.GetString(9),
                reader.GetString(10),
                reader.GetString(11)));
        }

        return items;
    }

    private static async Task<List<VehicleMeta>> LoadVehicleMetaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var items = new List<VehicleMeta>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT vehicle_id, state, tags, powertrain, climate_profile, timing_drive, transmission FROM vehicle_meta ORDER BY rowid;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new VehicleMeta(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6)));
        }

        return items;
    }

    private static async Task<List<VehicleHistoryEntry>> LoadHistoryAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var items = new List<VehicleHistoryEntry>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, vehicle_id, event_date, event_type, odometer, cost, note FROM history_entries ORDER BY rowid;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new VehicleHistoryEntry(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6)));
        }

        return items;
    }

    private static async Task<List<FuelEntry>> LoadFuelAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var items = new List<FuelEntry>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, vehicle_id, entry_date, odometer, liters, total_cost, full_tank, fuel_type, note, fuel_detail, station FROM fuel_entries ORDER BY rowid;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new FuelEntry(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetInt32(6) != 0,
                reader.GetString(7),
                reader.GetString(8),
                reader.GetString(9),
                reader.GetString(10)));
        }

        return items;
    }

    private static async Task<List<VehicleRecord>> LoadRecordsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var items = new List<VehicleRecord>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, vehicle_id, record_type, title, provider, valid_from, valid_to, price, attachment_mode, file_path, note FROM records ORDER BY rowid;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new VehicleRecord(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                reader.GetString(7),
                string.Equals(reader.GetString(8), "managed", StringComparison.OrdinalIgnoreCase)
                    ? VehicleRecordAttachmentMode.Managed
                    : VehicleRecordAttachmentMode.External,
                reader.GetString(9),
                reader.GetString(10)));
        }

        return items;
    }

    private static async Task<List<VehicleReminder>> LoadRemindersAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var items = new List<VehicleReminder>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, vehicle_id, title, due_date, reminder_days, repeat_mode, note FROM reminders ORDER BY rowid;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new VehicleReminder(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6)));
        }

        return items;
    }

    private static async Task<List<MaintenancePlan>> LoadMaintenancePlansAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var items = new List<MaintenancePlan>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, vehicle_id, title, interval_km, interval_months, last_service_date, last_service_odometer, is_active, note FROM maintenance_plans ORDER BY rowid;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new MaintenancePlan(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                reader.GetInt32(7) != 0,
                reader.GetString(8)));
        }

        return items;
    }

    private static async Task ExecuteAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string commandText,
        CancellationToken cancellationToken,
        params (string Name, object Value)[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
