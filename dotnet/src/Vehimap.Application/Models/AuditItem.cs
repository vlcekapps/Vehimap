namespace Vehimap.Application;

public sealed record AuditItem(
    AuditSeverity Severity,
    string Category,
    string VehicleId,
    string VehicleName,
    string EntityKind,
    string EntityId,
    string Title,
    string Message);
