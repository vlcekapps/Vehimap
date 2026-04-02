namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleMaintenanceItemViewModel(
    string Id,
    string Title,
    string Interval,
    string LastService,
    string Status,
    string Note)
{
    public string AccessibleLabel =>
        $"{Title}, interval {Interval}, poslední servis {LastService}, stav {Status}, poznámka {Note}";

    public override string ToString() => AccessibleLabel;
}
