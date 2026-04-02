namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleReminderItemViewModel(
    string Id,
    string Title,
    string DueDate,
    string Status,
    string RepeatMode,
    string Note)
{
    public string AccessibleLabel =>
        $"{Title}, termín {DueDate}, stav {Status}, opakování {RepeatMode}, poznámka {Note}";

    public override string ToString() => AccessibleLabel;
}
