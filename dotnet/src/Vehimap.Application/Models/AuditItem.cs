namespace Vehimap.Application;

public sealed record AuditItem(
    string Severity,
    string Category,
    string VehicleId,
    string Message);
