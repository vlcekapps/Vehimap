namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleTimelineItemViewModel(
    string Kind,
    string KindLabel,
    string Date,
    string Title,
    string Detail,
    string Status,
    string VehicleName,
    string VehicleId,
    string EntryId,
    bool IsFuture,
    string Note)
{
    public string AccessibleLabel =>
        $"{VehicleName}, {Date}, {KindLabel}, {Title}, stav {Status}, detail {Detail}";

    public override string ToString() => AccessibleLabel;
}
