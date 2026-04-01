namespace Vehimap.Desktop.ViewModels;

public sealed record AuditItemViewModel(
    string Severity,
    string Category,
    string VehicleName,
    string Title,
    string Message);
