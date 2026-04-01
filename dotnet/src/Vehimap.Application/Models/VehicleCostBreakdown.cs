namespace Vehimap.Application;

public sealed record VehicleCostBreakdown(
    string VehicleId,
    string VehicleName,
    string Category,
    decimal FuelCost,
    decimal HistoryCost,
    decimal RecordCost,
    decimal TotalCost,
    int? DistanceKm,
    decimal? CostPerKm,
    string Status);
