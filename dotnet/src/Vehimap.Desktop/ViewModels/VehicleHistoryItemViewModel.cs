namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleHistoryItemViewModel(
    string Id,
    string Date,
    string EventType,
    string Odometer,
    string Cost,
    string Note)
{
    public string AccessibleLabel =>
        $"{Date}, {EventType}, tachometr {Odometer}, cena {Cost}, poznámka {Note}";

    public override string ToString() => AccessibleLabel;
}
