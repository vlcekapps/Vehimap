namespace Vehimap.Desktop.ViewModels;

public sealed record FuelConsumptionSegmentItemViewModel(
    string Id,
    string FuelEntryId,
    string Period,
    string Distance,
    string Liters,
    string Consumption,
    string PricePerLiter,
    string CostPerKm)
{
    public string AccessibleLabel =>
        $"Úsek spotřeby {Period}, vzdálenost {Distance}, natankováno {Liters}, spotřeba {Consumption}, cena za litr {PricePerLiter}, cena za kilometr {CostPerKm}";

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
    string LatestDate)
{
    public string AccessibleLabel =>
        $"{Station}, {Fuel}, {EntryCount}, litry {Liters}, náklady {TotalCost}, průměrná cena za litr {AveragePricePerLiter}, poslední tankování {LatestDate}";

    public override string ToString() => AccessibleLabel;
}

public sealed record FuelAnalysisWarningItemViewModel(
    string Id,
    string FuelEntryId,
    string Severity,
    string Title,
    string Description)
{
    public string AccessibleLabel =>
        string.IsNullOrWhiteSpace(FuelEntryId)
            ? $"{Severity}: {Title}. {Description}"
            : $"{Severity}: {Title}. {Description}. Enter nebo tlačítko otevře související tankování.";

    public override string ToString() => AccessibleLabel;
}
