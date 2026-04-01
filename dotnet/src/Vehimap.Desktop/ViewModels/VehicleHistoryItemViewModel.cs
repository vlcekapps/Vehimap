namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleHistoryItemViewModel(
    string Date,
    string EventType,
    string Odometer,
    string Cost,
    string Note);
