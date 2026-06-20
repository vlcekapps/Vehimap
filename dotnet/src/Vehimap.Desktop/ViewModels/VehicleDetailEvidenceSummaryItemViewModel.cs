namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleDetailEvidenceSummaryItemViewModel(
    string Title,
    string Summary)
{
    public string AccessibleLabel => $"{Title}: {Summary}";

    public override string ToString() => AccessibleLabel;
}
