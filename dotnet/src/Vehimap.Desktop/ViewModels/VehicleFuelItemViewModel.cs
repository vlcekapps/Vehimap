namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleFuelItemViewModel(
    string Id,
    string Date,
    string FuelType,
    string Liters,
    string TotalCost,
    string Odometer,
    string TankState,
    string Note)
{
    public string AccessibleLabel =>
        $"{Date}, {FuelType}, {Liters}, cena {TotalCost}, tachometr {Odometer}, {TankState}, poznámka {Note}";

    public override string ToString() => AccessibleLabel;
}
