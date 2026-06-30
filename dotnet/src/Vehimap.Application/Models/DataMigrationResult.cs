namespace Vehimap.Application.Models;

public sealed record DataMigrationResult(
    bool Migrated,
    string? PreMigrationBackupPath,
    string Message);
