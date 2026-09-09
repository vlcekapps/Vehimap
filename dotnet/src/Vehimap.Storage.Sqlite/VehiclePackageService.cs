// SPDX-License-Identifier: GPL-3.0-or-later
using System.IO.Compression;
using System.Text.Json;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Storage.Sqlite;

public sealed class VehiclePackageService : IVehiclePackageService
{
    private const string ManifestFileName = "manifest.json";
    private const string DataFileName = "vehicle.json";
    private const string AttachmentFallbackFileName = "attachment.bin";
    private const string PackageFormat = "vehimap.vehicle-package";
    private const int PackageVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IAppLocalizer _localizer;

    public VehiclePackageService(IAppLocalizer? localizer = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer();
    }

    public async Task<VehiclePackageExportResult> ExportVehicleAsync(
        string packagePath,
        VehimapDataRoot dataRoot,
        VehimapDataSet dataSet,
        string vehicleId,
        CancellationToken cancellationToken = default)
    {
        var vehicle = dataSet.Vehicles.FirstOrDefault(item => string.Equals(item.Id, vehicleId, StringComparison.Ordinal))
            ?? throw new InvalidOperationException(L("VehiclePackage.Error.SelectedVehicleMissing"));

        var packageData = CreatePackageData(dataSet, vehicleId);
        var tempDirectory = CreateTemporaryDirectory("vehimap-vehicle-package");
        try
        {
            var attachmentResult = await CopyPackageAttachmentsAsync(
                    dataRoot,
                    packageData.Records,
                    Path.Combine(tempDirectory, SqliteStoragePaths.AttachmentsDirectoryName),
                    cancellationToken)
                .ConfigureAwait(false);

            var manifest = new VehiclePackageManifest(
                PackageFormat,
                PackageVersion,
                vehicle.Id,
                vehicle.Name,
                DateTime.UtcNow);

            await WriteJsonAsync(Path.Combine(tempDirectory, ManifestFileName), manifest, cancellationToken).ConfigureAwait(false);
            await WriteJsonAsync(Path.Combine(tempDirectory, DataFileName), packageData, cancellationToken).ConfigureAwait(false);

            var targetDirectory = Path.GetDirectoryName(packagePath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            AtomicArchiveWriter.CreateFromDirectory(tempDirectory, packagePath, cancellationToken);
            return new VehiclePackageExportResult(packagePath, vehicle.Id, vehicle.Name, attachmentResult.IncludedCount, attachmentResult.MissingCount);
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    public async Task<VehiclePackageImportResult> ImportVehicleAsync(
        string packagePath,
        VehimapDataRoot dataRoot,
        VehimapDataSet currentDataSet,
        CancellationToken cancellationToken = default)
    {
        var tempDirectory = CreateTemporaryDirectory("vehimap-vehicle-package-import");
        try
        {
            ZipFile.ExtractToDirectory(packagePath, tempDirectory);
            var manifest = await ReadJsonAsync<VehiclePackageManifest>(Path.Combine(tempDirectory, ManifestFileName), cancellationToken).ConfigureAwait(false)
                ?? throw new FormatException(L("VehiclePackage.Error.InvalidManifest"));
            if (!string.Equals(manifest.Format, PackageFormat, StringComparison.Ordinal) || manifest.Version != PackageVersion)
            {
                throw new FormatException(L("VehiclePackage.Error.UnsupportedFormat"));
            }

            var packageData = await ReadJsonAsync<VehiclePackageData>(Path.Combine(tempDirectory, DataFileName), cancellationToken).ConfigureAwait(false)
                ?? throw new FormatException(L("VehiclePackage.Error.MissingVehicleData"));
            if (packageData.Vehicles is not { Count: 1 } || packageData.Vehicles[0] is null)
            {
                throw new FormatException(L("VehiclePackage.Error.ExactlyOneVehicleRequired"));
            }

            var sourceVehicle = packageData.Vehicles[0];
            ValidatePackageData(packageData, manifest.VehicleId);
            var importedVehicleId = ResolveImportedVehicleId(currentDataSet, sourceVehicle.Id);
            var remapped = RemapPackageData(packageData, currentDataSet, sourceVehicle.Id, importedVehicleId);
            var restoredAttachments = RestorePackageAttachments(
                tempDirectory,
                dataRoot,
                remapped.Records,
                importedVehicleId,
                cancellationToken);

            var mergedData = MergeDataSets(currentDataSet, remapped);
            var importedVehicle = mergedData.Vehicles.First(item => string.Equals(item.Id, importedVehicleId, StringComparison.Ordinal));
            return new VehiclePackageImportResult(mergedData, importedVehicle.Id, importedVehicle.Name, restoredAttachments);
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    private void ValidatePackageData(VehiclePackageData data, string manifestVehicleId)
    {
        var id = data.Vehicles[0].Id;
        if (string.IsNullOrWhiteSpace(id) || id != manifestVehicleId
            || data.VehicleMetaEntries is null || data.HistoryEntries is null || data.FuelEntries is null
            || data.Records is null || data.Reminders is null || data.MaintenancePlans is null
            || data.VehicleMetaEntries.Count > 1
            || data.VehicleMetaEntries.Any(item => item is null || item.VehicleId != id)
            || data.HistoryEntries.Any(item => item is null || item.VehicleId != id || string.IsNullOrWhiteSpace(item.Id))
            || data.FuelEntries.Any(item => item is null || item.VehicleId != id || string.IsNullOrWhiteSpace(item.Id))
            || data.Records.Any(item => item is null || item.VehicleId != id || string.IsNullOrWhiteSpace(item.Id) || !Enum.IsDefined(item.AttachmentMode))
            || data.Reminders.Any(item => item is null || item.VehicleId != id || string.IsNullOrWhiteSpace(item.Id))
            || data.MaintenancePlans.Any(item => item is null || item.VehicleId != id || string.IsNullOrWhiteSpace(item.Id)))
        {
            throw new FormatException(L("VehiclePackage.Error.MissingVehicleData"));
        }
    }

    private static VehiclePackageData CreatePackageData(VehimapDataSet dataSet, string vehicleId) =>
        new(
            dataSet.Vehicles.Where(item => string.Equals(item.Id, vehicleId, StringComparison.Ordinal)).ToList(),
            dataSet.VehicleMetaEntries.Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal)).ToList(),
            dataSet.HistoryEntries.Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal)).ToList(),
            dataSet.FuelEntries.Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal)).ToList(),
            dataSet.Records.Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal)).ToList(),
            dataSet.Reminders.Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal)).ToList(),
            dataSet.MaintenancePlans.Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal)).ToList());

    private static VehiclePackageData RemapPackageData(
        VehiclePackageData packageData,
        VehimapDataSet currentDataSet,
        string sourceVehicleId,
        string importedVehicleId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var sequence = 0;

        string ResolveId(string sourceId, HashSet<string> existingIds, string prefix)
        {
            if (!existingIds.Contains(sourceId))
            {
                existingIds.Add(sourceId);
                return sourceId;
            }

            string candidate;
            do
            {
                sequence++;
                candidate = $"{prefix}_{timestamp}_{sequence}";
            }
            while (!existingIds.Add(candidate));

            return candidate;
        }

        var existingHistoryIds = currentDataSet.HistoryEntries.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var existingFuelIds = currentDataSet.FuelEntries.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var existingRecordIds = currentDataSet.Records.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var existingReminderIds = currentDataSet.Reminders.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var existingMaintenanceIds = currentDataSet.MaintenancePlans.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);

        return new VehiclePackageData(
            packageData.Vehicles
                .Select(item => item with { Id = importedVehicleId })
                .ToList(),
            packageData.VehicleMetaEntries
                .Select(item => item with { VehicleId = importedVehicleId })
                .ToList(),
            packageData.HistoryEntries
                .Select(item => item with
                {
                    Id = ResolveId(item.Id, existingHistoryIds, "hist"),
                    VehicleId = importedVehicleId
                })
                .ToList(),
            packageData.FuelEntries
                .Select(item => item with
                {
                    Id = ResolveId(item.Id, existingFuelIds, "fuel"),
                    VehicleId = importedVehicleId
                })
                .ToList(),
            packageData.Records
                .Select(item => item with
                {
                    Id = ResolveId(item.Id, existingRecordIds, "record"),
                    VehicleId = importedVehicleId
                })
                .ToList(),
            packageData.Reminders
                .Select(item => item with
                {
                    Id = ResolveId(item.Id, existingReminderIds, "rem"),
                    VehicleId = importedVehicleId
                })
                .ToList(),
            packageData.MaintenancePlans
                .Select(item => item with
                {
                    Id = ResolveId(item.Id, existingMaintenanceIds, "maint"),
                    VehicleId = importedVehicleId
                })
                .ToList());
    }

    private static VehimapDataSet MergeDataSets(VehimapDataSet currentDataSet, VehiclePackageData packageData) =>
        new()
        {
            Settings = CloneSettings(currentDataSet.Settings),
            Vehicles = [.. currentDataSet.Vehicles, .. packageData.Vehicles],
            VehicleMetaEntries = [.. currentDataSet.VehicleMetaEntries, .. packageData.VehicleMetaEntries],
            HistoryEntries = [.. currentDataSet.HistoryEntries, .. packageData.HistoryEntries],
            FuelEntries = [.. currentDataSet.FuelEntries, .. packageData.FuelEntries],
            Records = [.. currentDataSet.Records, .. packageData.Records],
            Reminders = [.. currentDataSet.Reminders, .. packageData.Reminders],
            MaintenancePlans = [.. currentDataSet.MaintenancePlans, .. packageData.MaintenancePlans]
        };

    private static VehimapSettings CloneSettings(VehimapSettings source)
    {
        var settings = new VehimapSettings();
        foreach (var (section, values) in source.Sections)
        {
            foreach (var (key, value) in values)
            {
                settings.SetValue(section, key, value);
            }
        }

        return settings;
    }

    private static string ResolveImportedVehicleId(VehimapDataSet currentDataSet, string sourceVehicleId)
    {
        if (!currentDataSet.Vehicles.Any(item => string.Equals(item.Id, sourceVehicleId, StringComparison.Ordinal)))
        {
            return sourceVehicleId;
        }

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        for (var suffix = 1; ; suffix++)
        {
            var candidate = $"veh_{timestamp}_{suffix}";
            if (!currentDataSet.Vehicles.Any(item => string.Equals(item.Id, candidate, StringComparison.Ordinal)))
            {
                return candidate;
            }
        }
    }

    private string BuildImportedAttachmentRelativePath(string vehicleId, string sourceRelativePath)
    {
        var normalized = SqliteStoragePaths.NormalizeAttachmentRelativePath(sourceRelativePath);
        var fileName = Path.GetFileName(normalized.Replace('/', Path.DirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = AttachmentFallbackFileName;
        }

        return $"{SqliteStoragePaths.AttachmentsDirectoryName}/{vehicleId}/{fileName}";
    }

    private int RestorePackageAttachments(
        string packageDirectory,
        VehimapDataRoot dataRoot,
        List<VehicleRecord> records,
        string importedVehicleId,
        CancellationToken cancellationToken)
    {
        var restoredCount = 0;
        var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var originalToTarget = new Dictionary<string, string>(comparer);
        var reservedTargets = new HashSet<string>(comparer);
        var plannedCopies = new Dictionary<string, string>(comparer);
        foreach (var record in records.Where(record => record.AttachmentMode == VehicleRecordAttachmentMode.Managed).ToList())
        {
            cancellationToken.ThrowIfCancellationRequested();
            string expectedSourceRelativePath;
            try
            {
                expectedSourceRelativePath = SqliteStoragePaths.NormalizeAttachmentRelativePath(record.FilePath);
            }
            catch (InvalidDataException ex)
            {
                throw new FormatException(L("VehiclePackage.Error.InvalidAttachmentPath"), ex);
            }

            var packageSourcePath = ManagedAttachmentPathGuard.ResolveManagedAttachmentPath(packageDirectory, expectedSourceRelativePath);
            if (string.IsNullOrEmpty(expectedSourceRelativePath))
            {
                continue;
            }

            if (!originalToTarget.TryGetValue(expectedSourceRelativePath, out var targetRelativePath))
            {
                targetRelativePath = ResolveUniqueAttachmentRelativePath(dataRoot, importedVehicleId, expectedSourceRelativePath, reservedTargets);
                originalToTarget[expectedSourceRelativePath] = targetRelativePath;
            }

            var targetPath = SqliteStoragePaths.ResolveManagedAttachmentPath(dataRoot, targetRelativePath);
            if (File.Exists(packageSourcePath))
                plannedCopies[targetPath] = packageSourcePath;

            var index = records.IndexOf(record);
            // Missing source files also receive a new, unoccupied path. Otherwise an
            // existing destination attachment could silently become the imported one.
            records[index] = record with { FilePath = targetRelativePath };
        }

        var createdFiles = new List<string>();
        try
        {
            foreach (var (targetPath, sourcePath) in plannedCopies)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                using var source = File.OpenRead(sourcePath);
                using var destination = new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write);
                createdFiles.Add(targetPath);
                source.CopyTo(destination);
                restoredCount++;
            }
        }
        catch
        {
            foreach (var path in createdFiles)
                File.Delete(path);
            throw;
        }
        return restoredCount;
    }

    private string ResolveUniqueAttachmentRelativePath(VehimapDataRoot dataRoot, string vehicleId, string sourceRelativePath, HashSet<string> reservedTargets)
    {
        var baseRelativePath = BuildImportedAttachmentRelativePath(vehicleId, sourceRelativePath);
        if (!Path.Exists(SqliteStoragePaths.ResolveManagedAttachmentPath(dataRoot, baseRelativePath)) && reservedTargets.Add(baseRelativePath))
        {
            return baseRelativePath;
        }

        var normalized = SqliteStoragePaths.NormalizeAttachmentRelativePath(baseRelativePath);
        var folder = Path.GetDirectoryName(normalized.Replace('/', Path.DirectorySeparatorChar))?.Replace('\\', '/') ?? SqliteStoragePaths.AttachmentsDirectoryName;
        var fileName = Path.GetFileNameWithoutExtension(normalized);
        var extension = Path.GetExtension(normalized);

        for (var suffix = 2; ; suffix++)
        {
            var candidate = $"{folder}/{fileName}-{suffix}{extension}";
            if (!Path.Exists(SqliteStoragePaths.ResolveManagedAttachmentPath(dataRoot, candidate)) && reservedTargets.Add(candidate))
            {
                return candidate;
            }
        }
    }

    private static async Task<AttachmentCopyResult> CopyPackageAttachmentsAsync(
        VehimapDataRoot dataRoot,
        IEnumerable<VehicleRecord> records,
        string targetAttachmentsRoot,
        CancellationToken cancellationToken)
    {
        var includedCount = 0;
        var missingCount = 0;
        var seen = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var record in records.Where(record => record.AttachmentMode == VehicleRecordAttachmentMode.Managed))
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

            var targetPath = ManagedAttachmentPathGuard.ResolveRelativePathInsideRoot(
                targetAttachmentsRoot,
                StripAttachmentsPrefix(relativePath));
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

    private static string StripAttachmentsPrefix(string relativePath)
    {
        var normalized = SqliteStoragePaths.NormalizeAttachmentRelativePath(relativePath);
        var prefix = $"{SqliteStoragePaths.AttachmentsDirectoryName}/";
        return normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? normalized[prefix.Length..]
            : normalized;
    }

    private static async Task WriteJsonAsync<T>(string path, T value, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value, JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<T?> ReadJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
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

    private string L(string key) => _localizer.GetString(key);

    private sealed record VehiclePackageManifest(
        string Format,
        int Version,
        string VehicleId,
        string VehicleName,
        DateTime CreatedUtc);

    private sealed record VehiclePackageData(
        List<Vehicle> Vehicles,
        List<VehicleMeta> VehicleMetaEntries,
        List<VehicleHistoryEntry> HistoryEntries,
        List<FuelEntry> FuelEntries,
        List<VehicleRecord> Records,
        List<VehicleReminder> Reminders,
        List<MaintenancePlan> MaintenancePlans);

    private sealed record AttachmentCopyResult(int IncludedCount, int MissingCount);
}
