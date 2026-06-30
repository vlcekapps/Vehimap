namespace Vehimap.Desktop.ViewModels;

public sealed record FuelConsumptionSegmentItemViewModel(
    string Id,
    string FuelEntryId,
    string Period,
    string Distance,
    string Liters,
    string Consumption,
    string PricePerLiter,
    string CostPerKm,
    string AccessibleLabel)
{
    public override string ToString() => AccessibleLabel;
}

public sealed record FuelGroupSummaryItemViewModel(
    string Id,
    string FuelEntryId,
    string Station,
    string Fuel,
    string EntryCount,
    string Liters,
    string TotalCost,
    string AveragePricePerLiter,
    string LatestDate,
    string AccessibleLabel)
{
    public override string ToString() => AccessibleLabel;
}

public sealed record FuelAnalysisWarningItemViewModel(
    string Id,
    string FuelEntryId,
    string Severity,
    string Title,
    string Description,
    string AccessibleLabel)
{
    public override string ToString() => AccessibleLabel;
}
