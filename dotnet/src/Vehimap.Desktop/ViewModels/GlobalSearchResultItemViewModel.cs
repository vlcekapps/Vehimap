namespace Vehimap.Desktop.ViewModels;

public sealed record GlobalSearchResultItemViewModel(
    string VehicleId,
    string EntityKind,
    string EntityId,
    string VehicleName,
    string SectionLabel,
    string Title,
    string Summary)
{
    public string AccessibleLabel =>
        $"{SectionLabel}, {Title}, vozidlo {VehicleName}, {Summary}".Trim().TrimEnd(',');

    public override string ToString() => AccessibleLabel;
}
