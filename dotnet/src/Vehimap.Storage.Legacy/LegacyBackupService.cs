using System.Text;
using Vehimap.Application.Abstractions;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Storage.Legacy;

public sealed class LegacyBackupService : IBackupService
{
    public async Task ExportAsync(string backupPath, VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
    {
        var attachments = await CollectManagedAttachmentsAsync(dataRoot, dataSet, cancellationToken).ConfigureAwait(false);
        var payload = new LegacyBackupPayload(
            LegacySectionSerialization.SerializeSettings(dataSet.Settings),
            LegacySectionSerialization.SerializeVehicles(dataSet.Vehicles),
            LegacySectionSerialization.SerializeHistory(dataSet.HistoryEntries),
            LegacySectionSerialization.SerializeFuel(dataSet.FuelEntries),
            LegacySectionSerialization.SerializeRecords(dataSet.Records),
            LegacySectionSerialization.SerializeVehicleMeta(dataSet.VehicleMetaEntries),
            LegacySectionSerialization.SerializeReminders(dataSet.Reminders),
            LegacySectionSerialization.SerializeMaintenancePlans(dataSet.MaintenancePlans),
            LegacySectionSerialization.SerializeAttachmentsSection(attachments));

        var content = LegacyBackupSerialization.Build(payload);
        var directory = Path.GetDirectoryName(backupPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(backupPath, content, new UTF8Encoding(true), cancellationToken).ConfigureAwait(false);
    }

    public async Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        var content = await File.ReadAllTextAsync(backupPath, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        var payload = LegacyBackupSerialization.Parse(content);

        var data = new VehimapDataSet
        {
            Settings = LegacySectionSerialization.ParseSettings(payload.SettingsContent),
            Vehicles = LegacySectionSerialization.ParseVehicles(payload.VehiclesContent),
            HistoryEntries = LegacySectionSerialization.ParseHistory(payload.HistoryContent),
            FuelEntries = LegacySectionSerialization.ParseFuel(payload.FuelContent),
            Records = LegacySectionSerialization.ParseRecords(payload.RecordsContent),
            VehicleMetaEntries = LegacySectionSerialization.ParseVehicleMeta(payload.MetaContent),
            Reminders = LegacySectionSerialization.ParseReminders(payload.RemindersContent),
            MaintenancePlans = LegacySectionSerialization.ParseMaintenancePlans(payload.MaintenanceContent)
        };

        var attachments = LegacySectionSerialization.ParseAttachmentsSection(payload.AttachmentsContent);
        return new VehimapBackupBundle(data, attachments);
    }

    public async Task RestoreAsync(VehimapDataRoot dataRoot, VehimapBackupBundle backupBundle, CancellationToken cancellationToken = default)
    {
        var dataStore = new LegacyVehimapDataStore();
        await dataStore.SaveAsync(dataRoot, backupBundle.Data, cancellationToken).ConfigureAwait(false);

        var attachmentsRoot = LegacyVehimapDataStore.GetAttachmentsPath(dataRoot);
        if (Directory.Exists(attachmentsRoot))
        {
            Directory.Delete(attachmentsRoot, true);
        }

        foreach (var attachment in backupBundle.Attachments)
        {
            var targetPath = LegacySectionSerialization.ResolveManagedAttachmentPath(dataRoot.DataPath, attachment.RelativePath);
            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(targetPath, attachment.Content, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<List<ManagedAttachment>> CollectManagedAttachmentsAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items = new List<ManagedAttachment>();

        foreach (var record in dataSet.Records.Where(record => record.AttachmentMode == VehicleRecordAttachmentMode.Managed))
        {
            var relativePath = LegacySectionSerialization.NormalizeAttachmentRelativePath(record.FilePath);
            if (string.IsNullOrWhiteSpace(relativePath) || !seen.Add(relativePath))
            {
                continue;
            }

            var absolutePath = LegacySectionSerialization.ResolveManagedAttachmentPath(dataRoot.DataPath, relativePath);
            if (!File.Exists(absolutePath))
            {
                continue;
            }

            var content = await File.ReadAllBytesAsync(absolutePath, cancellationToken).ConfigureAwait(false);
            items.Add(new ManagedAttachment(relativePath, content));
        }

        return items;
    }
}
