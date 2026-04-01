namespace Vehimap.Application;

public sealed record CostAnalysisSummary(
    string PeriodLabel,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal TotalCost,
    int? DistanceKm,
    decimal? CostPerKm,
    decimal PreviousTotalCost,
    decimal? PreviousCostPerKm,
    decimal TotalCostDifference,
    decimal? CostPerKmDifference,
    int ActiveVehicleCount,
    int ActiveWithoutCostCount,
    int CostPerKmUnavailableCount,
    IReadOnlyList<VehicleCostBreakdown> Vehicles);
