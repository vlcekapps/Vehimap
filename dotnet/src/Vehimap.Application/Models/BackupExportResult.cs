namespace Vehimap.Application.Models;

public sealed record BackupExportResult(
    string BackupPath,
    int IncludedManagedAttachmentCount,
    int MissingManagedAttachmentCount);
