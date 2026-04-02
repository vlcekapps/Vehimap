namespace Vehimap.Desktop.ViewModels;

public sealed record CostVehicleItemViewModel(
    string VehicleId,
    string VehicleName,
    string Category,
    string TotalCost,
    string Distance,
    string CostPerKm,
    string Status)
{
    public string AccessibleLabel =>
        $"{VehicleName}, {Category}, náklady {TotalCost}, ujeto {Distance}, cena za kilometr {CostPerKm}, stav {Status}";

    public override string ToString() => AccessibleLabel;
}
