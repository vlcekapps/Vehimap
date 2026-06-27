using System.Text;
using Vehimap.Application.Abstractions;
using Vehimap.Domain.Models;

namespace Vehimap.Storage.Legacy;

public sealed class LegacyVehimapDataStore : ILegacyDataStore
{
    public async Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
    {
        await EnsureDataFilesAsync(dataRoot, cancellationToken).ConfigureAwait(false);

        var settings = await ReadAndParseAsync(dataRoot, LegacySectionSerialization.SettingsFileName, "nastavení", LegacySectionSerialization.ParseSettings, cancellationToken).ConfigureAwait(false);
        var vehicles = await ReadAndParseAsync(dataRoot, LegacySectionSerialization.VehiclesFileName, "vozidel", LegacySectionSerialization.ParseVehicles, cancellationToken).ConfigureAwait(false);
        var history = await ReadAndParseAsync(dataRoot, LegacySectionSerialization.HistoryFileName, "historie", LegacySectionSerialization.ParseHistory, cancellationToken).ConfigureAwait(false);
        var fuel = await ReadAndParseAsync(dataRoot, LegacySectionSerialization.FuelFileName, "tankování", LegacySectionSerialization.ParseFuel, cancellationToken).ConfigureAwait(false);
        var records = await ReadAndParseAsync(dataRoot, LegacySectionSerialization.RecordsFileName, "dokladů", LegacySectionSerialization.ParseRecords, cancellationToken).ConfigureAwait(false);
        var meta = await ReadAndParseAsync(dataRoot, LegacySectionSerialization.MetaFileName, "metadat vozidel", LegacySectionSerialization.ParseVehicleMeta, cancellationToken).ConfigureAwait(false);
        var reminders = await ReadAndParseAsync(dataRoot, LegacySectionSerialization.RemindersFileName, "připomínek", LegacySectionSerialization.ParseReminders, cancellationToken).ConfigureAwait(false);
        var maintenance = await ReadAndParseAsync(dataRoot, LegacySectionSerialization.MaintenanceFileName, "plánů údržby", LegacySectionSerialization.ParseMaintenancePlans, cancellationToken).ConfigureAwait(false);

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

    private static async Task<T> ReadAndParseAsync<T>(
        VehimapDataRoot dataRoot,
        string fileName,
        string sectionName,
        Func<string, T> parser,
        CancellationToken cancellationToken)
    {
        var path = GetPath(dataRoot, fileName);
        try
        {
            var content = await ReadTextAsync(path, cancellationToken).ConfigureAwait(false);
            return parser(content);
        }
        catch (FormatException ex)
        {
            throw BuildLoadException(fileName, path, sectionName, ex);
        }
    }

    private static LegacyDataLoadException BuildLoadException(string fileName, string path, string sectionName, Exception innerException)
    {
        var message =
            $"Soubor {sectionName} ({fileName}) se nepodařilo načíst.{Environment.NewLine}" +
            $"Zkontrolujte soubor: {path}{Environment.NewLine}" +
            $"Detail: {innerException.Message}";

        return new LegacyDataLoadException(fileName, path, message, innerException);
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
        await EnsureFileAsync(GetPath(dataRoot, LegacySectionSerialization.FuelFileName), $"{LegacySectionSerialization.FuelHeaderV2}\n", cancellationToken).ConfigureAwait(false);
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
