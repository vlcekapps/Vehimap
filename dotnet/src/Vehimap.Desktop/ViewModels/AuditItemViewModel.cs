namespace Vehimap.Desktop.ViewModels;

public sealed record AuditItemViewModel(
    string VehicleId,
    string EntityKind,
    string EntityId,
    string Severity,
    string Category,
    string VehicleName,
    string Title,
    string Message);
