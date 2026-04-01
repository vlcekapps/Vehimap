namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleMaintenanceItemViewModel(
    string Id,
    string Title,
    string Interval,
    string LastService,
    string Status,
    string Note);
