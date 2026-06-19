namespace Vehimap.Desktop.ViewModels;

public sealed record CostVehicleItemViewModel(
    string VehicleId,
    string VehicleName,
    string Category,
    string FuelCost,
    string HistoryCost,
    string RecordCost,
    string TotalCost,
    string Distance,
    string CostPerKm,
    string Status)
{
    public string AccessibleLabel =>
        $"{VehicleName}, {Category}, náklady {TotalCost}, palivo {FuelCost}, historie {HistoryCost}, doklady {RecordCost}, ujeto {Distance}, cena za kilometr {CostPerKm}, stav {Status}";

    public override string ToString() => AccessibleLabel;
}
