namespace Vehimap.Application;

public sealed record CostAnalysisSummary(
    decimal TotalCost,
    int? DistanceKm,
    decimal? CostPerKm);
