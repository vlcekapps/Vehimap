// SPDX-License-Identifier: GPL-3.0-or-later
using System.Text;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Storage.Legacy;

public sealed class LegacyBackupService : IBackupService
{
    private readonly IAppLocalizer _localizer;

    public LegacyBackupService(IAppLocalizer? localizer = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer();
    }

    public async Task<BackupExportResult> ExportAsync(string backupPath, VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
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
            LegacySectionSerialization.SerializeAttachmentsSection(attachments.Items));

        var content = LegacyBackupSerialization.Build(payload);
        var directory = Path.GetDirectoryName(backupPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(backupPath, content, new UTF8Encoding(true), cancellationToken).ConfigureAwait(false);
        return new BackupExportResult(backupPath, attachments.Items.Count, attachments.MissingCount);
    }

    public async Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var content = await File.ReadAllTextAsync(backupPath, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            var payload = LegacyBackupSerialization.Parse(content, _localizer);

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
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw CreateImportException(backupPath, ex);
        }
    }

    public async Task<BackupRestoreResult> RestoreAsync(VehimapDataRoot dataRoot, VehimapBackupBundle backupBundle, CancellationToken cancellationToken = default)
    {
        var preRestoreBackupPath = await BackupCurrentFilesBeforeRestoreAsync(dataRoot, cancellationToken).ConfigureAwait(false);

        var dataStore = new LegacyVehimapDataStore(_localizer);
        await dataStore.SaveAsync(dataRoot, backupBundle.Data, cancellationToken).ConfigureAwait(false);

        var attachmentsRoot = LegacyVehimapDataStore.GetAttachmentsPath(dataRoot);
        if (Directory.Exists(attachmentsRoot))
        {
            Directory.Delete(attachmentsRoot, true);
        }

        var restoredAttachmentCount = 0;
        foreach (var attachment in backupBundle.Attachments)
        {
            var targetPath = LegacySectionSerialization.ResolveManagedAttachmentPath(dataRoot.DataPath, attachment.RelativePath);
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

    private static Task<string?> BackupCurrentFilesBeforeRestoreAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(dataRoot.DataPath))
        {
            return Task.FromResult<string?>(null);
        }

        var backupDirectory = CreateImportBackupDirectory(dataRoot);
        foreach (var fileName in LegacyDataFileNames)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sourcePath = LegacyVehimapDataStore.GetPath(dataRoot, fileName);
            if (!File.Exists(sourcePath))
            {
                continue;
            }

            File.Copy(sourcePath, Path.Combine(backupDirectory, fileName), overwrite: true);
        }

        var attachmentsRoot = LegacyVehimapDataStore.GetAttachmentsPath(dataRoot);
        if (Directory.Exists(attachmentsRoot))
        {
            CopyDirectory(attachmentsRoot, Path.Combine(backupDirectory, LegacySectionSerialization.AttachmentsDirectoryName), cancellationToken);
        }

        return Task.FromResult<string?>(backupDirectory);
    }

    private static string CreateImportBackupDirectory(VehimapDataRoot dataRoot)
    {
        var backupRoot = Path.Combine(dataRoot.DataPath, "import-backups");
        Directory.CreateDirectory(backupRoot);

        var baseName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var candidate = Path.Combine(backupRoot, baseName);
        for (var suffix = 2; Directory.Exists(candidate); suffix++)
        {
            candidate = Path.Combine(backupRoot, $"{baseName}-{suffix}");
        }

        Directory.CreateDirectory(candidate);
        return candidate;
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

    private static async Task<ManagedAttachmentCollection> CollectManagedAttachmentsAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items = new List<ManagedAttachment>();
        var missingCount = 0;

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
                missingCount++;
                continue;
            }

            var content = await File.ReadAllBytesAsync(absolutePath, cancellationToken).ConfigureAwait(false);
            items.Add(new ManagedAttachment(relativePath, content));
        }

        return new ManagedAttachmentCollection(items, missingCount);
    }

    private LegacyBackupException CreateImportException(string backupPath, Exception exception)
    {
        var resolvedPath = ResolveBackupPathForMessage(backupPath);
        var detail = string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message;

        return new LegacyBackupException(
            resolvedPath,
            _localizer.Format("LegacyBackup.ImportFailed", resolvedPath, detail),
            exception);
    }

    private string ResolveBackupPathForMessage(string backupPath)
    {
        if (string.IsNullOrWhiteSpace(backupPath))
        {
            return _localizer.GetString("LegacyBackup.PathMissing");
        }

        try
        {
            return Path.GetFullPath(backupPath);
        }
        catch
        {
            return backupPath;
        }
    }

    private static readonly string[] LegacyDataFileNames =
    [
        LegacySectionSerialization.VehiclesFileName,
        LegacySectionSerialization.HistoryFileName,
        LegacySectionSerialization.FuelFileName,
        LegacySectionSerialization.RecordsFileName,
        LegacySectionSerialization.MetaFileName,
        LegacySectionSerialization.RemindersFileName,
        LegacySectionSerialization.MaintenanceFileName,
        LegacySectionSerialization.SettingsFileName
    ];

    private sealed record ManagedAttachmentCollection(
        List<ManagedAttachment> Items,
        int MissingCount);
}
