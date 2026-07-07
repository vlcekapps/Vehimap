// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Desktop.ViewModels;

public sealed record FuelConsumptionSegmentItemViewModel(
    string Id,
    string FuelEntryId,
    string Period,
    string Distance,
    string FuelAmount,
    string Consumption,
    string PricePerVolume,
    string CostPerDistance,
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
    string FuelAmount,
    string TotalCost,
    string AveragePricePerVolume,
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
