using Vehimap.Domain.Models;

namespace Vehimap.Application.Abstractions;

public interface IBackupService
{
    Task ExportAsync(string backupPath, VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default);
    Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default);
    Task RestoreAsync(VehimapDataRoot dataRoot, VehimapBackupBundle backupBundle, CancellationToken cancellationToken = default);
}
