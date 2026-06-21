namespace Vehimap.Application.Models;

public sealed record BackupRestoreResult(
    string? PreRestoreBackupPath,
    int RestoredAttachmentCount);
