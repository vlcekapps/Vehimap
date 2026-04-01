using Vehimap.Application.Abstractions;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacyCostAnalysisService : ICostAnalysisService
{
    public CostAnalysisSummary BuildYearToDateSummary(VehimapDataSet dataSet, DateOnly today)
    {
        var currentStart = new DateOnly(today.Year, 1, 1);
        var currentEnd = today;
        var duration = currentEnd.DayNumber - currentStart.DayNumber;
        var previousStart = currentStart.AddYears(-1);
        var previousEnd = previousStart.AddDays(duration);

        var current = BuildPeriodSummary(dataSet, currentStart, currentEnd);
        var previous = BuildPeriodSummary(dataSet, previousStart, previousEnd);

        return new CostAnalysisSummary(
            $"Od {currentStart:dd.MM.yyyy} do {currentEnd:dd.MM.yyyy}",
            currentStart,
            currentEnd,
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

    private static CostAnalysisSummary BuildPeriodSummary(VehimapDataSet dataSet, DateOnly periodStart, DateOnly periodEnd)
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
            $"Od {periodStart:dd.MM.yyyy} do {periodEnd:dd.MM.yyyy}",
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

    private static VehicleCostBreakdown BuildVehicleBreakdown(VehicleCostAccumulator row)
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
            status = "Bez nákladů";
        }
        else if (hasRegression)
        {
            status = "Nekonzistentní tachometr";
        }
        else if (orderedSamples.Count < 2)
        {
            status = total > 0m ? "Chybí km v období" : "Bez km v období";
        }
        else
        {
            var first = orderedSamples.First().Odometer;
            var last = orderedSamples.Last().Odometer;
            distanceKm = Math.Max(0, last - first);
            if (distanceKm > 0)
            {
                costPerKm = total / distanceKm.Value;
                status = "V pořádku";
            }
            else
            {
                status = total > 0m ? "Nulový nájezd" : "Bez pohybu";
            }
        }

        if (row.IsInactive && total <= 0m)
        {
            status = "Neaktivní";
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
