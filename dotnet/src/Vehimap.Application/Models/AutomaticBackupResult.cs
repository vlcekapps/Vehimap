namespace Vehimap.Application.Models;

public sealed record AutomaticBackupResult(
    bool Created,
    bool IsError,
    string BackupPath,
    string Message);
