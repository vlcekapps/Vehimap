namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleFuelItemViewModel(
    string Date,
    string FuelType,
    string Liters,
    string TotalCost,
    string Odometer,
    string TankState,
    string Note);
