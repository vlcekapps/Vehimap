using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacyFuelAnalysisService : IFuelAnalysisService
{
    private const decimal ConsumptionOutlierHighMultiplier = 1.4m;
    private const decimal ConsumptionOutlierLowMultiplier = 0.6m;
    private const decimal PriceOutlierHighMultiplier = 1.3m;
    private const decimal PriceOutlierLowMultiplier = 0.7m;

    private readonly IAppLocalizer _localizer;

    public LegacyFuelAnalysisService()
        : this(new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage)))
    {
    }

    public LegacyFuelAnalysisService(IAppLocalizer localizer)
    {
        _localizer = localizer;
    }

    public FuelAnalysisSummary BuildVehicleFuelAnalysis(VehimapDataSet dataSet, string vehicleId)
    {
        ArgumentNullException.ThrowIfNull(dataSet);

        var entries = dataSet.FuelEntries
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal))
            .ToList();
        var warnings = new List<FuelAnalysisWarning>();
        var parsedEntries = entries
            .Select(item => ParseEntry(item, warnings))
            .ToList();

        AddOdometerRegressionWarnings(parsedEntries, warnings);
        var segments = BuildConsumptionSegments(parsedEntries, warnings);
        AddAvailabilityWarning(entries.Count, parsedEntries, segments, warnings);
        AddConsumptionOutlierWarnings(segments, warnings);
        AddPriceOutlierWarnings(parsedEntries, warnings);

        var totalLiters = parsedEntries
            .Where(item => item.Liters.HasValue)
            .Sum(item => item.Liters!.Value);
        var totalCost = parsedEntries
            .Where(item => item.TotalCost.HasValue)
            .Sum(item => item.TotalCost!.Value);
        decimal? averagePrice = totalLiters > 0m && totalCost > 0m
            ? totalCost / totalLiters
            : null;
        var totalSegmentDistance = segments.Sum(item => item.DistanceKm);
        var totalSegmentLiters = segments.Sum(item => item.Liters);
        decimal? averageConsumption = totalSegmentDistance > 0
            ? totalSegmentLiters / totalSegmentDistance * 100m
            : null;
        var bestSegment = segments.OrderBy(item => item.ConsumptionLitersPer100Km).FirstOrDefault();
        var worstSegment = segments.OrderByDescending(item => item.ConsumptionLitersPer100Km).FirstOrDefault();

        return new FuelAnalysisSummary(
            vehicleId,
            entries.Count,
            totalLiters,
            totalCost,
            averagePrice,
            averageConsumption,
            bestSegment,
            worstSegment,
            BuildStatus(entries.Count, segments.Count),
            segments,
            BuildGroupSummaries(parsedEntries),
            warnings
                .OrderByDescending(item => item.Severity)
                .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(item => item.Id, StringComparer.Ordinal)
                .ToList());
    }

    private ParsedFuelEntry ParseEntry(FuelEntry entry, ICollection<FuelAnalysisWarning> warnings)
    {
        DateOnly? date = null;
        if (VehimapValueParser.TryParseEventDate(entry.EntryDate, out var parsedDate))
        {
            date = parsedDate;
        }

        int? odometer = null;
        if (VehimapValueParser.TryParseOdometer(entry.Odometer, out var parsedOdometer))
        {
            odometer = parsedOdometer;
        }
        else if (!string.IsNullOrWhiteSpace(entry.Odometer))
        {
            warnings.Add(new FuelAnalysisWarning(
                $"fuel-analysis-odometer-{entry.Id}",
                entry.Id,
                FuelAnalysisWarningSeverity.Warning,
                L("FuelAnalysis.Warning.OdometerInvalid.Title"),
                LF("FuelAnalysis.Warning.OdometerInvalid.Description", FormatEntryDate(entry))));
        }

        decimal? liters = null;
        if (VehimapValueParser.TryParseDecimalNumber(entry.Liters, out var parsedLiters) && parsedLiters > 0m)
        {
            liters = parsedLiters;
        }
        else if (!string.IsNullOrWhiteSpace(entry.Liters))
        {
            warnings.Add(new FuelAnalysisWarning(
                $"fuel-analysis-liters-{entry.Id}",
                entry.Id,
                FuelAnalysisWarningSeverity.Warning,
                L("FuelAnalysis.Warning.LitersInvalid.Title"),
                LF("FuelAnalysis.Warning.LitersInvalid.Description", FormatEntryDate(entry))));
        }

        decimal? totalCost = null;
        if (VehimapValueParser.TryParseMoney(entry.TotalCost, out var parsedTotalCost) && parsedTotalCost >= 0m)
        {
            totalCost = parsedTotalCost;
        }
        else if (!string.IsNullOrWhiteSpace(entry.TotalCost))
        {
            warnings.Add(new FuelAnalysisWarning(
                $"fuel-analysis-cost-{entry.Id}",
                entry.Id,
                FuelAnalysisWarningSeverity.Warning,
                L("FuelAnalysis.Warning.CostInvalid.Title"),
                LF("FuelAnalysis.Warning.CostInvalid.Description", FormatEntryDate(entry))));
        }

        return new ParsedFuelEntry(entry, date, odometer, liters, totalCost);
    }

    private void AddOdometerRegressionWarnings(
        IReadOnlyList<ParsedFuelEntry> parsedEntries,
        ICollection<FuelAnalysisWarning> warnings)
    {
        ParsedFuelEntry? previous = null;
        foreach (var current in parsedEntries
                     .Where(item => item.Date.HasValue && item.Odometer.HasValue)
                     .OrderBy(item => item.Date!.Value)
                     .ThenBy(item => item.Odometer!.Value)
                     .ThenBy(item => item.Entry.Id, StringComparer.Ordinal))
        {
            if (previous is not null && current.Odometer!.Value < previous.Odometer!.Value)
            {
                warnings.Add(new FuelAnalysisWarning(
                    $"fuel-analysis-odometer-regression-{current.Entry.Id}",
                    current.Entry.Id,
                    FuelAnalysisWarningSeverity.Warning,
                    L("FuelAnalysis.Warning.OdometerRegression.Title"),
                    LF("FuelAnalysis.Warning.OdometerRegression.Description", FormatEntryDate(current.Entry), current.Odometer.Value, previous.Odometer.Value)));
            }

            previous = current;
        }
    }

    private IReadOnlyList<FuelConsumptionSegment> BuildConsumptionSegments(
        IReadOnlyList<ParsedFuelEntry> parsedEntries,
        ICollection<FuelAnalysisWarning> warnings)
    {
        var segments = new List<FuelConsumptionSegment>();
        var orderedEntries = parsedEntries
            .Where(item => item.Date.HasValue && item.Odometer.HasValue)
            .OrderBy(item => item.Date!.Value)
            .ThenBy(item => item.Odometer!.Value)
            .ThenBy(item => item.Entry.Id, StringComparer.Ordinal)
            .ToList();

        ParsedFuelEntry? previousSample = null;
        ParsedFuelEntry? lastFullTank = null;
        decimal windowLiters = 0m;
        decimal windowCost = 0m;
        var windowHasMissingLiters = false;
        var windowHasOdometerRegression = false;

        foreach (var sample in orderedEntries)
        {
            if (previousSample is not null && sample.Odometer!.Value < previousSample.Odometer!.Value)
            {
                windowHasOdometerRegression = true;
            }

            previousSample = sample;

            if (lastFullTank is null)
            {
                if (sample.Entry.FullTank)
                {
                    lastFullTank = sample;
                    windowLiters = 0m;
                    windowCost = 0m;
                    windowHasMissingLiters = false;
                    windowHasOdometerRegression = false;
                }

                continue;
            }

            if (sample.Liters.HasValue)
            {
                windowLiters += sample.Liters.Value;
            }
            else
            {
                windowHasMissingLiters = true;
            }

            if (sample.TotalCost.HasValue)
            {
                windowCost += sample.TotalCost.Value;
            }

            if (!sample.Entry.FullTank)
            {
                continue;
            }

            var distance = sample.Odometer!.Value - lastFullTank.Odometer!.Value;
            if (distance > 0 && windowLiters > 0m && !windowHasMissingLiters && !windowHasOdometerRegression)
            {
                decimal? pricePerLiter = windowCost > 0m ? windowCost / windowLiters : null;
                segments.Add(new FuelConsumptionSegment(
                    $"fuel-segment-{lastFullTank.Entry.Id}-{sample.Entry.Id}",
                    lastFullTank.Entry.Id,
                    sample.Entry.Id,
                    lastFullTank.Date!.Value,
                    sample.Date!.Value,
                    lastFullTank.Odometer.Value,
                    sample.Odometer.Value,
                    distance,
                    windowLiters,
                    windowCost,
                    windowLiters / distance * 100m,
                    pricePerLiter,
                    windowCost > 0m ? windowCost / distance : null));
            }
            else if (distance <= 0)
            {
                warnings.Add(new FuelAnalysisWarning(
                    $"fuel-analysis-segment-distance-{sample.Entry.Id}",
                    sample.Entry.Id,
                    FuelAnalysisWarningSeverity.Warning,
                    L("FuelAnalysis.Warning.SegmentUnavailable.Title"),
                    LF("FuelAnalysis.Warning.SegmentUnavailable.Description", FormatEntryDate(sample.Entry))));
            }

            lastFullTank = sample;
            windowLiters = 0m;
            windowCost = 0m;
            windowHasMissingLiters = false;
            windowHasOdometerRegression = false;
        }

        return segments;
    }

    private void AddAvailabilityWarning(
        int entryCount,
        IReadOnlyList<ParsedFuelEntry> parsedEntries,
        IReadOnlyList<FuelConsumptionSegment> segments,
        ICollection<FuelAnalysisWarning> warnings)
    {
        if (entryCount == 0 || segments.Count > 0)
        {
            return;
        }

        var fullTankWithOdometerCount = parsedEntries.Count(item => item.Entry.FullTank && item.Date.HasValue && item.Odometer.HasValue);
        var description = fullTankWithOdometerCount < 2
            ? L("FuelAnalysis.Warning.ConsumptionUnavailable.Description.FullTanks")
            : L("FuelAnalysis.Warning.ConsumptionUnavailable.Description.InvalidSegments");

        warnings.Add(new FuelAnalysisWarning(
            "fuel-analysis-consumption-unavailable",
            null,
            FuelAnalysisWarningSeverity.Info,
            L("FuelAnalysis.Warning.ConsumptionUnavailable.Title"),
            description));
    }

    private void AddConsumptionOutlierWarnings(
        IReadOnlyList<FuelConsumptionSegment> segments,
        ICollection<FuelAnalysisWarning> warnings)
    {
        if (segments.Count < 3)
        {
            return;
        }

        var average = segments.Sum(item => item.Liters) / segments.Sum(item => item.DistanceKm) * 100m;
        foreach (var segment in segments)
        {
            if (segment.ConsumptionLitersPer100Km > average * ConsumptionOutlierHighMultiplier)
            {
                warnings.Add(new FuelAnalysisWarning(
                    $"fuel-analysis-consumption-high-{segment.EndFuelEntryId}",
                    segment.EndFuelEntryId,
                    FuelAnalysisWarningSeverity.Info,
                    L("FuelAnalysis.Warning.ConsumptionHigh.Title"),
                    LF("FuelAnalysis.Warning.ConsumptionHigh.Description", segment.EndDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture))));
            }
            else if (segment.ConsumptionLitersPer100Km < average * ConsumptionOutlierLowMultiplier)
            {
                warnings.Add(new FuelAnalysisWarning(
                    $"fuel-analysis-consumption-low-{segment.EndFuelEntryId}",
                    segment.EndFuelEntryId,
                    FuelAnalysisWarningSeverity.Info,
                    L("FuelAnalysis.Warning.ConsumptionLow.Title"),
                    LF("FuelAnalysis.Warning.ConsumptionLow.Description", segment.EndDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture))));
            }
        }
    }

    private void AddPriceOutlierWarnings(
        IReadOnlyList<ParsedFuelEntry> parsedEntries,
        ICollection<FuelAnalysisWarning> warnings)
    {
        var priceSamples = parsedEntries
            .Where(item => item.PricePerLiter.HasValue)
            .ToList();
        if (priceSamples.Count < 5)
        {
            return;
        }

        var totalLiters = priceSamples.Sum(item => item.Liters!.Value);
        var totalCost = priceSamples.Sum(item => item.TotalCost!.Value);
        if (totalLiters <= 0m || totalCost <= 0m)
        {
            return;
        }

        var average = totalCost / totalLiters;
        foreach (var sample in priceSamples)
        {
            if (sample.PricePerLiter!.Value > average * PriceOutlierHighMultiplier)
            {
                warnings.Add(new FuelAnalysisWarning(
                    $"fuel-analysis-price-high-{sample.Entry.Id}",
                    sample.Entry.Id,
                    FuelAnalysisWarningSeverity.Info,
                    L("FuelAnalysis.Warning.PriceHigh.Title"),
                    LF("FuelAnalysis.Warning.PriceHigh.Description", FormatEntryDate(sample.Entry))));
            }
            else if (sample.PricePerLiter.Value < average * PriceOutlierLowMultiplier)
            {
                warnings.Add(new FuelAnalysisWarning(
                    $"fuel-analysis-price-low-{sample.Entry.Id}",
                    sample.Entry.Id,
                    FuelAnalysisWarningSeverity.Info,
                    L("FuelAnalysis.Warning.PriceLow.Title"),
                    LF("FuelAnalysis.Warning.PriceLow.Description", FormatEntryDate(sample.Entry))));
            }
        }
    }

    private IReadOnlyList<FuelGroupSummary> BuildGroupSummaries(IReadOnlyList<ParsedFuelEntry> parsedEntries)
    {
        return parsedEntries
            .GroupBy(item => new
            {
                Station = NormalizeGroupValue(item.Entry.Station, L("FuelAnalysis.Group.UnknownStation")),
                FuelType = NormalizeGroupValue(item.Entry.FuelType, L("FuelAnalysis.Group.UnknownFuelType")),
                FuelDetail = NormalizeGroupValue(item.Entry.FuelDetail, L("FuelAnalysis.Group.UnknownFuelDetail"))
            })
            .Select(group =>
            {
                var totalLiters = group.Where(item => item.Liters.HasValue).Sum(item => item.Liters!.Value);
                var totalCost = group.Where(item => item.TotalCost.HasValue).Sum(item => item.TotalCost!.Value);
                var latest = group
                    .OrderByDescending(item => item.Date.HasValue)
                    .ThenByDescending(item => item.Date)
                    .ThenByDescending(item => item.Odometer.HasValue)
                    .ThenByDescending(item => item.Odometer)
                    .ThenBy(item => item.Entry.Id, StringComparer.Ordinal)
                    .First();

                return new FuelGroupSummary(
                    $"fuel-group-{group.Key.Station}-{group.Key.FuelType}-{group.Key.FuelDetail}",
                    latest.Entry.Id,
                    group.Key.Station,
                    group.Key.FuelType,
                    group.Key.FuelDetail,
                    group.Count(),
                    totalLiters,
                    totalCost,
                    totalLiters > 0m && totalCost > 0m ? totalCost / totalLiters : null,
                    latest.Date);
            })
            .OrderByDescending(item => item.TotalCost)
            .ThenByDescending(item => item.EntryCount)
            .ThenBy(item => item.Station, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.FuelType, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private string BuildStatus(int entryCount, int segmentCount)
    {
        if (entryCount == 0)
        {
            return L("FuelAnalysis.Status.NoEntries");
        }

        if (segmentCount == 0)
        {
            return L("FuelAnalysis.Status.Unavailable");
        }

        return segmentCount == 1
            ? L("FuelAnalysis.Status.OneSegment")
            : LF("FuelAnalysis.Status.ManySegments", segmentCount);
    }

    private static string NormalizeGroupValue(string? value, string fallback)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? fallback : normalized;
    }

    private string FormatEntryDate(FuelEntry entry) =>
        string.IsNullOrWhiteSpace(entry.EntryDate) ? L("Common.NoDate") : entry.EntryDate;

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);

    private sealed record ParsedFuelEntry(
        FuelEntry Entry,
        DateOnly? Date,
        int? Odometer,
        decimal? Liters,
        decimal? TotalCost)
    {
        public decimal? PricePerLiter =>
            Liters is > 0m && TotalCost is > 0m
                ? TotalCost.Value / Liters.Value
                : null;
    }
}
