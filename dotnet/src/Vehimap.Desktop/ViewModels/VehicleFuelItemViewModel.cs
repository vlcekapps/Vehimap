namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleFuelItemViewModel(
    string Id,
    string Date,
    string FuelType,
    string Liters,
    string TotalCost,
    string Odometer,
    string TankState,
    string Note);
