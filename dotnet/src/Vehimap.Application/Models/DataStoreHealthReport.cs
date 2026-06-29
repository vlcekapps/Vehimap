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

    public string DiagnosticText => string.Join(
        Environment.NewLine,
        new[]
        {
            "Vehimap - kontrola datové sady 2.0",
            $"Stav: {Status}",
            $"Souhrn: {Summary}",
            $"Databáze: {DatabasePath}",
            $"Datová složka: {DataPath}",
            $"Předmigrační záloha: {PreMigrationBackupPath ?? string.Empty}",
            "Detaily:"
        }.Concat(Details.Select(item => $"- {item}")));
}
