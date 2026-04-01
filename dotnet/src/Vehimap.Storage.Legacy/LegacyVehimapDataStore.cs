using System.Text;
using Vehimap.Application.Abstractions;
using Vehimap.Domain.Models;

namespace Vehimap.Storage.Legacy;

public sealed class LegacyVehimapDataStore : ILegacyDataStore
{
    public async Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
    {
        await EnsureDataFilesAsync(dataRoot, cancellationToken).ConfigureAwait(false);

        var settings = LegacySectionSerialization.ParseSettings(await ReadTextAsync(GetPath(dataRoot, LegacySectionSerialization.SettingsFileName), cancellationToken).ConfigureAwait(false));
        var vehicles = LegacySectionSerialization.ParseVehicles(await ReadTextAsync(GetPath(dataRoot, LegacySectionSerialization.VehiclesFileName), cancellationToken).ConfigureAwait(false));
        var history = LegacySectionSerialization.ParseHistory(await ReadTextAsync(GetPath(dataRoot, LegacySectionSerialization.HistoryFileName), cancellationToken).ConfigureAwait(false));
        var fuel = LegacySectionSerialization.ParseFuel(await ReadTextAsync(GetPath(dataRoot, LegacySectionSerialization.FuelFileName), cancellationToken).ConfigureAwait(false));
        var records = LegacySectionSerialization.ParseRecords(await ReadTextAsync(GetPath(dataRoot, LegacySectionSerialization.RecordsFileName), cancellationToken).ConfigureAwait(false));
        var meta = LegacySectionSerialization.ParseVehicleMeta(await ReadTextAsync(GetPath(dataRoot, LegacySectionSerialization.MetaFileName), cancellationToken).ConfigureAwait(false));
        var reminders = LegacySectionSerialization.ParseReminders(await ReadTextAsync(GetPath(dataRoot, LegacySectionSerialization.RemindersFileName), cancellationToken).ConfigureAwait(false));
        var maintenance = LegacySectionSerialization.ParseMaintenancePlans(await ReadTextAsync(GetPath(dataRoot, LegacySectionSerialization.MaintenanceFileName), cancellationToken).ConfigureAwait(false));

        return new VehimapDataSet
        {
            Settings = settings,
            Vehicles = vehicles,
            HistoryEntries = history,
            FuelEntries = fuel,
            Records = records,
            VehicleMetaEntries = meta,
            Reminders = reminders,
            MaintenancePlans = maintenance
        };
    }

    public async Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
    {
        await EnsureDataFilesAsync(dataRoot, cancellationToken).ConfigureAwait(false);

        await WriteTextAsync(GetPath(dataRoot, LegacySectionSerialization.SettingsFileName), LegacySectionSerialization.SerializeSettings(dataSet.Settings), cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(GetPath(dataRoot, LegacySectionSerialization.VehiclesFileName), LegacySectionSerialization.SerializeVehicles(dataSet.Vehicles), cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(GetPath(dataRoot, LegacySectionSerialization.HistoryFileName), LegacySectionSerialization.SerializeHistory(dataSet.HistoryEntries), cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(GetPath(dataRoot, LegacySectionSerialization.FuelFileName), LegacySectionSerialization.SerializeFuel(dataSet.FuelEntries), cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(GetPath(dataRoot, LegacySectionSerialization.RecordsFileName), LegacySectionSerialization.SerializeRecords(dataSet.Records), cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(GetPath(dataRoot, LegacySectionSerialization.MetaFileName), LegacySectionSerialization.SerializeVehicleMeta(dataSet.VehicleMetaEntries), cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(GetPath(dataRoot, LegacySectionSerialization.RemindersFileName), LegacySectionSerialization.SerializeReminders(dataSet.Reminders), cancellationToken).ConfigureAwait(false);
        await WriteTextAsync(GetPath(dataRoot, LegacySectionSerialization.MaintenanceFileName), LegacySectionSerialization.SerializeMaintenancePlans(dataSet.MaintenancePlans), cancellationToken).ConfigureAwait(false);
    }

    internal static string GetPath(VehimapDataRoot dataRoot, string fileName) =>
        Path.Combine(dataRoot.DataPath, fileName);

    internal static string GetAttachmentsPath(VehimapDataRoot dataRoot) =>
        LegacySectionSerialization.GetAttachmentsRootPath(dataRoot.DataPath);

    internal static async Task<string> ReadTextAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        var text = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        return LegacySectionSerialization.NormalizeTextForStorage(text);
    }

    internal static async Task WriteTextAsync(string path, string content, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, content, new UTF8Encoding(true), cancellationToken).ConfigureAwait(false);
    }

    internal static async Task EnsureDataFilesAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(dataRoot.DataPath);

        await EnsureFileAsync(GetPath(dataRoot, LegacySectionSerialization.VehiclesFileName), $"{LegacySectionSerialization.VehiclesHeaderV4}\n", cancellationToken).ConfigureAwait(false);
        await EnsureFileAsync(GetPath(dataRoot, LegacySectionSerialization.HistoryFileName), $"{LegacySectionSerialization.HistoryHeaderV1}\n", cancellationToken).ConfigureAwait(false);
        await EnsureFileAsync(GetPath(dataRoot, LegacySectionSerialization.FuelFileName), $"{LegacySectionSerialization.FuelHeaderV1}\n", cancellationToken).ConfigureAwait(false);
        await EnsureFileAsync(GetPath(dataRoot, LegacySectionSerialization.RecordsFileName), $"{LegacySectionSerialization.RecordsHeaderV2}\n", cancellationToken).ConfigureAwait(false);
        await EnsureFileAsync(GetPath(dataRoot, LegacySectionSerialization.MetaFileName), $"{LegacySectionSerialization.MetaHeaderV2}\n", cancellationToken).ConfigureAwait(false);
        await EnsureFileAsync(GetPath(dataRoot, LegacySectionSerialization.RemindersFileName), $"{LegacySectionSerialization.RemindersHeaderV2}\n", cancellationToken).ConfigureAwait(false);
        await EnsureFileAsync(GetPath(dataRoot, LegacySectionSerialization.MaintenanceFileName), $"{LegacySectionSerialization.MaintenanceHeaderV1}\n", cancellationToken).ConfigureAwait(false);
        await EnsureFileAsync(GetPath(dataRoot, LegacySectionSerialization.SettingsFileName), string.Empty, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureFileAsync(string path, string content, CancellationToken cancellationToken)
    {
        if (File.Exists(path))
        {
            return;
        }

        await WriteTextAsync(path, content, cancellationToken).ConfigureAwait(false);
    }
}
