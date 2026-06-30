using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacyCostAnalysisService : ICostAnalysisService
{
    private readonly IAppLocalizer _localizer;

    public LegacyCostAnalysisService(IAppLocalizer? localizer = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));
    }

    public CostAnalysisSummary BuildYearToDateSummary(VehimapDataSet dataSet, DateOnly today)
    {
        var currentStart = new DateOnly(today.Year, 1, 1);
        return BuildPeriodSummary(dataSet, currentStart, today);
    }

    public CostAnalysisSummary BuildPeriodSummary(VehimapDataSet dataSet, DateOnly periodStart, DateOnly periodEnd)
    {
        if (periodEnd < periodStart)
        {
            (periodStart, periodEnd) = (periodEnd, periodStart);
        }

        var duration = periodEnd.DayNumber - periodStart.DayNumber;
        var previousStart = periodStart.AddYears(-1);
        var previousEnd = previousStart.AddDays(duration);

        var current = BuildPeriodTotals(dataSet, periodStart, periodEnd);
        var previous = BuildPeriodTotals(dataSet, previousStart, previousEnd);

        return new CostAnalysisSummary(
            BuildPeriodLabel(periodStart, periodEnd),
            periodStart,
            periodEnd,
            current.TotalCost,
            current.DistanceKm,
            current.CostPerKm,
            previous.TotalCost,
            previous.CostPerKm,
            current.TotalCost - previous.TotalCost,
            current.CostPerKm.HasValue && previous.CostPerKm.HasValue ? current.CostPerKm.Value - previous.CostPerKm.Value : null,
            current.ActiveVehicleCount,
            current.ActiveWithoutCostCount,
            current.CostPerKmUnavailableCount,
            current.Vehicles);
    }

    private CostAnalysisSummary BuildPeriodTotals(VehimapDataSet dataSet, DateOnly periodStart, DateOnly periodEnd)
    {
        var metaByVehicleId = dataSet.VehicleMetaEntries
            .GroupBy(item => item.VehicleId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        var vehicleRows = dataSet.Vehicles.ToDictionary(
            vehicle => vehicle.Id,
            vehicle => new VehicleCostAccumulator(vehicle, IsVehicleInactive(vehicle, metaByVehicleId)),
            StringComparer.Ordinal);

        foreach (var entry in dataSet.FuelEntries)
        {
            if (!vehicleRows.TryGetValue(entry.VehicleId, out var row))
            {
                continue;
            }

            if (VehimapValueParser.TryParseEventDate(entry.EntryDate, out var date)
                && VehimapValueParser.TryParseOdometer(entry.Odometer, out var odometer)
                && date >= periodStart
                && date <= periodEnd)
            {
                row.OdometerSamples.Add(new OdometerPeriodSample(date, odometer));
            }

            if (!string.IsNullOrWhiteSpace(entry.TotalCost)
                && VehimapValueParser.TryParseEventDate(entry.EntryDate, out date)
                && date >= periodStart
                && date <= periodEnd
                && VehimapValueParser.TryParseMoney(entry.TotalCost, out var amount))
            {
                row.FuelCost += amount;
            }
        }

        foreach (var entry in dataSet.HistoryEntries)
        {
            if (!vehicleRows.TryGetValue(entry.VehicleId, out var row))
            {
                continue;
            }

            if (VehimapValueParser.TryParseEventDate(entry.EventDate, out var date)
                && VehimapValueParser.TryParseOdometer(entry.Odometer, out var odometer)
                && date >= periodStart
                && date <= periodEnd)
            {
                row.OdometerSamples.Add(new OdometerPeriodSample(date, odometer));
            }

            if (!string.IsNullOrWhiteSpace(entry.Cost)
                && VehimapValueParser.TryParseEventDate(entry.EventDate, out date)
                && date >= periodStart
                && date <= periodEnd
                && VehimapValueParser.TryParseMoney(entry.Cost, out var amount))
            {
                row.HistoryCost += amount;
            }
        }

        foreach (var entry in dataSet.Records)
        {
            if (!vehicleRows.TryGetValue(entry.VehicleId, out var row))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(entry.Price)
                && VehimapValueParser.TryResolveRecordDate(entry, out var date)
                && date >= periodStart
                && date <= periodEnd
                && VehimapValueParser.TryParseMoney(entry.Price, out var amount))
            {
                row.RecordCost += amount;
            }
        }

        var rows = vehicleRows.Values
            .Select(BuildVehicleBreakdown)
            .OrderByDescending(item => item.TotalCost)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var totalCost = rows.Sum(item => item.TotalCost);
        var distanceValues = rows.Where(item => item.DistanceKm.HasValue).Select(item => item.DistanceKm!.Value).ToList();
        int? totalDistance = distanceValues.Count > 0 ? distanceValues.Sum() : null;
        decimal? totalCostPerKm = totalDistance is > 0 ? totalCost / totalDistance.Value : null;
        var activeVehicles = vehicleRows.Values.Count(item => !item.IsInactive);
        var activeWithoutCost = vehicleRows.Values.Count(item => !item.IsInactive && item.TotalCost <= 0m);
        var unavailableCostPerKm = rows.Count(item => item.TotalCost > 0m && !item.CostPerKm.HasValue);

        return new CostAnalysisSummary(
            BuildPeriodLabel(periodStart, periodEnd),
            periodStart,
            periodEnd,
            totalCost,
            totalDistance,
            totalCostPerKm,
            0m,
            null,
            0m,
            null,
            activeVehicles,
            activeWithoutCost,
            unavailableCostPerKm,
            rows);
    }

    private VehicleCostBreakdown BuildVehicleBreakdown(VehicleCostAccumulator row)
    {
        var total = row.TotalCost;
        var orderedSamples = row.OdometerSamples
            .OrderBy(item => item.Date)
            .ThenBy(item => item.Odometer)
            .ToList();

        var hasRegression = false;
        for (var index = 1; index < orderedSamples.Count; index++)
        {
            if (orderedSamples[index].Odometer < orderedSamples[index - 1].Odometer)
            {
                hasRegression = true;
                break;
            }
        }

        int? distanceKm = null;
        decimal? costPerKm = null;
        string status;

        if (total <= 0m && !row.IsInactive)
        {
            status = L("CostAnalysis.Status.NoCost");
        }
        else if (hasRegression)
        {
            status = L("CostAnalysis.Status.OdometerRegression");
        }
        else if (orderedSamples.Count < 2)
        {
            status = total > 0m
                ? L("CostAnalysis.Status.MissingDistanceWithCost")
                : L("CostAnalysis.Status.NoDistance");
        }
        else
        {
            var first = orderedSamples.First().Odometer;
            var last = orderedSamples.Last().Odometer;
            distanceKm = Math.Max(0, last - first);
            if (distanceKm > 0)
            {
                costPerKm = total / distanceKm.Value;
                status = L("CostAnalysis.Status.Ok");
            }
            else
            {
                status = total > 0m
                    ? L("CostAnalysis.Status.ZeroDistanceWithCost")
                    : L("CostAnalysis.Status.NoMovement");
            }
        }

        if (row.IsInactive && total <= 0m)
        {
            status = L("CostAnalysis.Status.Inactive");
        }

        return new VehicleCostBreakdown(
            row.Vehicle.Id,
            row.Vehicle.Name,
            row.Vehicle.Category,
            row.FuelCost,
            row.HistoryCost,
            row.RecordCost,
            total,
            distanceKm,
            costPerKm,
            status);
    }

    private string BuildPeriodLabel(DateOnly start, DateOnly end) =>
        LF("CostAnalysis.PeriodLabel", FormatPeriodDate(start), FormatPeriodDate(end));

    private static string FormatPeriodDate(DateOnly date) =>
        date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);

    private static bool IsVehicleInactive(Vehicle vehicle, IReadOnlyDictionary<string, VehicleMeta> metaByVehicleId)
    {
        var state = metaByVehicleId.TryGetValue(vehicle.Id, out var meta)
            ? (meta.State ?? string.Empty).Trim()
            : string.Empty;

        return state.Equals("Archiv", StringComparison.OrdinalIgnoreCase)
               || state.Equals("Odstaveno", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class VehicleCostAccumulator
    {
        public VehicleCostAccumulator(Vehicle vehicle, bool isInactive)
        {
            Vehicle = vehicle;
            IsInactive = isInactive;
        }

        public Vehicle Vehicle { get; }

        public bool IsInactive { get; }

        public decimal FuelCost { get; set; }

        public decimal HistoryCost { get; set; }

        public decimal RecordCost { get; set; }

        public decimal TotalCost => FuelCost + HistoryCost + RecordCost;

        public List<OdometerPeriodSample> OdometerSamples { get; } = [];
    }

    private sealed record OdometerPeriodSample(DateOnly Date, int Odometer);
}
