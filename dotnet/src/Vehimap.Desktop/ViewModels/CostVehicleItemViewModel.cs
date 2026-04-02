namespace Vehimap.Desktop.ViewModels;

public sealed record CostVehicleItemViewModel(
    string VehicleId,
    string VehicleName,
    string Category,
    string TotalCost,
    string Distance,
    string CostPerKm,
    string Status);
