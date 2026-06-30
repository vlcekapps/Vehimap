namespace Vehimap.Application.Models;

public sealed record DataStoreHealthReport(
    DataStoreHealthStatus Status,
    string Summary,
    IReadOnlyList<string> Details,
    string DatabasePath,
    string DataPath,
    string? PreMigrationBackupPath = null)
{
    public bool HasWarningsOrErrors => Status != DataStoreHealthStatus.Healthy;
}
