namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleHistoryItemViewModel(
    string Id,
    string Date,
    string EventType,
    string Odometer,
    string Cost,
    string Note);
