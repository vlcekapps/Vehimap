namespace Vehimap.Application.Models;

public sealed record DataMigrationResult(
    bool Migrated,
    string? PreMigrationBackupPath,
    string Message)
{
    public static DataMigrationResult NotNeeded { get; } =
        new(false, null, "Datová sada 2.0 je připravena.");
}
