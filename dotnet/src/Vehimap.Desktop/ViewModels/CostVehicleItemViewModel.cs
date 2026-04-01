namespace Vehimap.Desktop.ViewModels;

public sealed record CostVehicleItemViewModel(
    string VehicleName,
    string Category,
    string TotalCost,
    string Distance,
    string CostPerKm,
    string Status);
