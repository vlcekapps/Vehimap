// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record FuelAnalysisSummary(
    string VehicleId,
    int EntryCount,
    decimal TotalLiters,
    decimal TotalCost,
    decimal? AveragePricePerLiter,
    decimal? AverageConsumptionLitersPer100Km,
    FuelConsumptionSegment? BestConsumptionSegment,
    FuelConsumptionSegment? WorstConsumptionSegment,
    string Status,
    IReadOnlyList<FuelConsumptionSegment> ConsumptionSegments,
    IReadOnlyList<FuelGroupSummary> GroupSummaries,
    IReadOnlyList<FuelAnalysisWarning> Warnings);

public sealed record FuelConsumptionSegment(
    string Id,
    string StartFuelEntryId,
    string EndFuelEntryId,
    DateOnly StartDate,
    DateOnly EndDate,
    int StartOdometer,
    int EndOdometer,
    int DistanceKm,
    decimal Liters,
    decimal TotalCost,
    decimal ConsumptionLitersPer100Km,
    decimal? PricePerLiter,
    decimal? CostPerKm);

public sealed record FuelGroupSummary(
    string Id,
    string LatestFuelEntryId,
    string Station,
    string FuelType,
    string FuelDetail,
    int EntryCount,
    decimal Liters,
    decimal TotalCost,
    decimal? AveragePricePerLiter,
    DateOnly? LatestDate);

public sealed record FuelAnalysisWarning(
    string Id,
    string? FuelEntryId,
    FuelAnalysisWarningSeverity Severity,
    string Title,
    string Description);

public enum FuelAnalysisWarningSeverity
{
    Info,
    Warning,
    Error
}
