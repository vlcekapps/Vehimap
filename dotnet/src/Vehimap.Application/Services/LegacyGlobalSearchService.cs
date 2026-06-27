using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacyGlobalSearchService : IGlobalSearchService
{
    private const int NoSearchMatchRank = 1_000_000;

    private readonly IFileAttachmentService _attachmentService;
    private readonly ITimelineService _timelineService;

    public LegacyGlobalSearchService(IFileAttachmentService attachmentService)
        : this(attachmentService, new LegacyTimelineService())
    {
    }

    public LegacyGlobalSearchService(IFileAttachmentService attachmentService, ITimelineService timelineService)
    {
        _attachmentService = attachmentService;
        _timelineService = timelineService;
    }

    public IReadOnlyList<GlobalSearchResult> Search(VehimapDataRoot dataRoot, VehimapDataSet dataSet, string query)
    {
        var needle = query?.Trim();
        if (string.IsNullOrWhiteSpace(needle))
        {
            return [];
        }

        var metaByVehicleId = dataSet.VehicleMetaEntries
            .GroupBy(item => item.VehicleId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var vehiclesById = dataSet.Vehicles.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var timelineByVehicleId = dataSet.Vehicles.ToDictionary(
            item => item.Id,
            item => _timelineService.BuildVehicleTimeline(dataSet, item.Id, today),
            StringComparer.Ordinal);
        var results = new List<GlobalSearchResult>();

        foreach (var vehicle in dataSet.Vehicles)
        {
            var meta = metaByVehicleId.GetValueOrDefault(vehicle.Id);
            var timeline = timelineByVehicleId.GetValueOrDefault(vehicle.Id) ?? [];
            var statusText = BuildVehicleAttentionStatusText(timeline);
            var title = ValueOrFallback(vehicle.Name, "Bez názvu vozidla");
            var summary = JoinParts(
                ValueOrFallback(vehicle.MakeModel, "Bez značky / modelu"),
                ValueOrFallback(vehicle.Category, "Bez kategorie"),
                FormatPlate(vehicle.Plate),
                ValueOrFallback(vehicle.VehicleNote, string.Empty),
                ValueOrFallback(meta?.State, string.Empty),
                ValueOrFallback(meta?.Tags, string.Empty),
                ValueOrFallback(meta?.Powertrain, string.Empty),
                ValueOrFallback(meta?.ClimateProfile, string.Empty),
                ValueOrFallback(meta?.TimingDrive, string.Empty),
                ValueOrFallback(meta?.Transmission, string.Empty),
                statusText);
            var searchTexts = BuildVehicleSearchTexts(vehicle, meta, timeline, includeTimelineStatus: true);

            var rank = ComputeRank(searchTexts, needle);
            if (rank < NoSearchMatchRank)
            {
                results.Add(new GlobalSearchResult(
                    vehicle.Id,
                    title,
                    "Vozidlo",
                    vehicle.Id,
                    "Vozidlo",
                    title,
                    summary,
                    rank));
            }
        }

        foreach (var entry in dataSet.HistoryEntries)
        {
            var vehicle = vehiclesById.GetValueOrDefault(entry.VehicleId);
            var vehicleName = vehicle?.Name ?? "Neznámé vozidlo";
            var meta = vehicle is null ? null : metaByVehicleId.GetValueOrDefault(vehicle.Id);
            var timeline = GetVehicleTimeline(timelineByVehicleId, entry.VehicleId);
            var title = ValueOrFallback(entry.EventType, "Historie");
            var summary = JoinParts(
                ValueOrFallback(entry.EventDate, "bez data"),
                FormatOdometer(entry.Odometer),
                FormatMoneyValue(entry.Cost),
                ValueOrFallback(entry.Note, string.Empty));
            var searchTexts = BuildSearchTexts(
                BuildVehicleSearchTexts(vehicle, meta, timeline),
                entry.EventDate,
                entry.EventType,
                entry.Odometer,
                entry.Cost,
                entry.Note);

            var rank = ComputeRank(searchTexts, needle);
            if (rank < NoSearchMatchRank)
            {
                results.Add(new GlobalSearchResult(
                    entry.VehicleId,
                    vehicleName,
                    "Historie",
                    entry.Id,
                    "Historie",
                    title,
                    summary,
                    rank));
            }
        }

        foreach (var entry in dataSet.FuelEntries)
        {
            var vehicle = vehiclesById.GetValueOrDefault(entry.VehicleId);
            var vehicleName = vehicle?.Name ?? "Neznámé vozidlo";
            var meta = vehicle is null ? null : metaByVehicleId.GetValueOrDefault(vehicle.Id);
            var timeline = GetVehicleTimeline(timelineByVehicleId, entry.VehicleId);
            var title = BuildFuelTitle(entry);
            var summary = JoinParts(
                ValueOrFallback(entry.EntryDate, "bez data"),
                ValueOrFallback(entry.FuelType, "bez typu"),
                FormatFuelLiters(entry.Liters),
                FormatOdometer(entry.Odometer),
                FormatMoneyValue(entry.TotalCost),
                entry.FullTank ? "Plná nádrž" : string.Empty,
                ValueOrFallback(entry.FuelDetail, string.Empty),
                ValueOrFallback(entry.Station, string.Empty),
                ValueOrFallback(entry.Note, string.Empty));
            var searchTexts = BuildSearchTexts(
                BuildVehicleSearchTexts(vehicle, meta, timeline),
                entry.EntryDate,
                entry.Odometer,
                entry.Liters,
                entry.TotalCost,
                entry.FullTank ? "ano" : "ne",
                entry.FullTank ? "Plná nádrž" : string.Empty,
                entry.FuelType,
                entry.FuelDetail,
                entry.Station,
                entry.Note);

            var rank = ComputeRank(searchTexts, needle);
            if (rank < NoSearchMatchRank)
            {
                results.Add(new GlobalSearchResult(
                    entry.VehicleId,
                    vehicleName,
                    "Tankování",
                    entry.Id,
                    "Tankování",
                    title,
                    summary,
                    rank));
            }
        }

        foreach (var record in dataSet.Records)
        {
            var vehicle = vehiclesById.GetValueOrDefault(record.VehicleId);
            var vehicleName = vehicle?.Name ?? "Neznámé vozidlo";
            var meta = vehicle is null ? null : metaByVehicleId.GetValueOrDefault(vehicle.Id);
            var timeline = GetVehicleTimeline(timelineByVehicleId, record.VehicleId);
            var timelineStatus = FindTimelineStatus(timeline, "record", record.Id);
            var title = ValueOrFallback(record.Title, "Doklad");
            var resolvedPath = ResolveRecordPath(dataRoot, record);
            var summary = JoinParts(
                ValueOrFallback(record.RecordType, "Doklad"),
                ValueOrFallback(record.Provider, string.Empty),
                BuildValidity(record.ValidFrom, record.ValidTo),
                BuildAttachmentLabel(record, resolvedPath),
                timelineStatus,
                ValueOrFallback(record.Note, string.Empty));
            var searchTexts = BuildSearchTexts(
                BuildVehicleSearchTexts(vehicle, meta, timeline),
                record.RecordType,
                record.Title,
                record.Provider,
                record.ValidFrom,
                record.ValidTo,
                record.Price,
                record.FilePath,
                resolvedPath,
                Path.GetFileName(resolvedPath),
                BuildAttachmentModeLabel(record),
                timelineStatus,
                record.Note);

            var rank = ComputeRank(searchTexts, needle);
            if (rank < NoSearchMatchRank)
            {
                results.Add(new GlobalSearchResult(
                    record.VehicleId,
                    vehicleName,
                    "Doklad",
                    record.Id,
                    "Doklady",
                    title,
                    summary,
                    rank));
            }
        }

        foreach (var reminder in dataSet.Reminders)
        {
            var vehicle = vehiclesById.GetValueOrDefault(reminder.VehicleId);
            var vehicleName = vehicle?.Name ?? "Neznámé vozidlo";
            var meta = vehicle is null ? null : metaByVehicleId.GetValueOrDefault(vehicle.Id);
            var timeline = GetVehicleTimeline(timelineByVehicleId, reminder.VehicleId);
            var timelineStatus = FindTimelineStatus(timeline, "custom", reminder.Id);
            var title = ValueOrFallback(reminder.Title, "Připomínka");
            var summary = JoinParts(
                ValueOrFallback(reminder.DueDate, "bez termínu"),
                ValueOrFallback(reminder.RepeatMode, string.Empty),
                timelineStatus,
                ValueOrFallback(reminder.Note, string.Empty));
            var searchTexts = BuildSearchTexts(
                BuildVehicleSearchTexts(vehicle, meta, timeline),
                reminder.Title,
                reminder.DueDate,
                reminder.ReminderDays,
                reminder.RepeatMode,
                timelineStatus,
                reminder.Note);

            var rank = ComputeRank(searchTexts, needle);
            if (rank < NoSearchMatchRank)
            {
                results.Add(new GlobalSearchResult(
                    reminder.VehicleId,
                    vehicleName,
                    "Připomínka",
                    reminder.Id,
                    "Připomínky",
                    title,
                    summary,
                    rank));
            }
        }

        foreach (var plan in dataSet.MaintenancePlans)
        {
            var vehicle = vehiclesById.GetValueOrDefault(plan.VehicleId);
            var vehicleName = vehicle?.Name ?? "Neznámé vozidlo";
            var meta = vehicle is null ? null : metaByVehicleId.GetValueOrDefault(vehicle.Id);
            var timeline = GetVehicleTimeline(timelineByVehicleId, plan.VehicleId);
            var timelineItem = FindTimelineItem(timeline, "maintenance", plan.Id);
            var title = ValueOrFallback(plan.Title, "Servisní úkon");
            var summary = JoinParts(
                BuildMaintenanceInterval(plan),
                ValueOrFallback(plan.LastServiceDate, string.Empty),
                FormatOdometer(plan.LastServiceOdometer),
                timelineItem?.Detail,
                timelineItem?.Status,
                plan.IsActive ? "Aktivní" : "Neaktivní",
                ValueOrFallback(plan.Note, string.Empty));
            var searchTexts = BuildSearchTexts(
                BuildVehicleSearchTexts(vehicle, meta, timeline),
                plan.Title,
                plan.IntervalKm,
                plan.IntervalMonths,
                plan.LastServiceDate,
                plan.LastServiceOdometer,
                timelineItem?.Detail,
                timelineItem?.Status,
                plan.IsActive ? "Aktivní" : "Neaktivní",
                plan.Note);

            var rank = ComputeRank(searchTexts, needle);
            if (rank < NoSearchMatchRank)
            {
                results.Add(new GlobalSearchResult(
                    plan.VehicleId,
                    vehicleName,
                    "Údržba",
                    plan.Id,
                    "Údržba",
                    title,
                    summary,
                    rank));
            }
        }

        return results
            .OrderBy(item => item.Rank)
            .ThenBy(item => GetEntityKindPriority(item.EntityKind))
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.SectionLabel, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Take(100)
            .ToList();
    }

    private string ResolveRecordPath(VehimapDataRoot dataRoot, VehicleRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.FilePath))
        {
            return string.Empty;
        }

        return record.AttachmentMode == VehicleRecordAttachmentMode.Managed
            ? _attachmentService.ResolveManagedAttachmentPath(dataRoot, record.FilePath)
            : record.FilePath;
    }

    private static IReadOnlyList<VehicleTimelineItem> GetVehicleTimeline(
        IReadOnlyDictionary<string, IReadOnlyList<VehicleTimelineItem>> timelineByVehicleId,
        string vehicleId) =>
        timelineByVehicleId.GetValueOrDefault(vehicleId) ?? [];

    private static IReadOnlyList<string?> BuildVehicleSearchTexts(
        Vehicle? vehicle,
        VehicleMeta? meta,
        IReadOnlyList<VehicleTimelineItem> timelineItems,
        bool includeTimelineStatus = false)
    {
        if (vehicle is null)
        {
            return [];
        }

        var statusText = includeTimelineStatus ? BuildVehicleAttentionStatusText(timelineItems) : string.Empty;
        return
        [
            vehicle.Name,
            vehicle.VehicleNote,
            vehicle.MakeModel,
            vehicle.Plate,
            vehicle.Year,
            vehicle.Power,
            vehicle.Category,
            meta?.State,
            meta?.Tags,
            meta?.Powertrain,
            meta?.ClimateProfile,
            meta?.TimingDrive,
            meta?.Transmission,
            vehicle.LastTk,
            vehicle.NextTk,
            vehicle.GreenCardFrom,
            vehicle.GreenCardTo,
            includeTimelineStatus ? statusText : string.Empty,
            .. (includeTimelineStatus
                ? timelineItems
                    .Where(IsVehicleStatusTimelineItem)
                    .SelectMany(item => new string?[] { item.KindLabel, item.Title, item.Detail, item.Status })
                : [])
        ];
    }

    private static VehicleTimelineItem? FindTimelineItem(IReadOnlyList<VehicleTimelineItem> timelineItems, string kind, string entryId) =>
        timelineItems.FirstOrDefault(item =>
            string.Equals(item.Kind, kind, StringComparison.Ordinal)
            && string.Equals(item.EntryId, entryId, StringComparison.Ordinal));

    private static string FindTimelineStatus(IReadOnlyList<VehicleTimelineItem> timelineItems, string kind, string entryId) =>
        FindTimelineItem(timelineItems, kind, entryId)?.Status ?? string.Empty;

    private static string BuildVehicleAttentionStatusText(IReadOnlyList<VehicleTimelineItem> timelineItems)
    {
        var parts = new List<string>();
        AddTimelineStatusPart(parts, timelineItems, "technical", "TK");
        AddTimelineStatusPart(parts, timelineItems, "green", "ZK");
        AddTimelineStatusPart(parts, timelineItems, "custom", "Připomínka");
        AddTimelineStatusPart(parts, timelineItems, "maintenance", "Údržba");
        return string.Join(" | ", parts);
    }

    private static void AddTimelineStatusPart(List<string> parts, IReadOnlyList<VehicleTimelineItem> timelineItems, string kind, string label)
    {
        var status = timelineItems
            .Where(item => string.Equals(item.Kind, kind, StringComparison.Ordinal))
            .Select(item => item.Status)
            .FirstOrDefault(IsAttentionStatus);

        if (!string.IsNullOrWhiteSpace(status))
        {
            parts.Add($"{label}: {status}");
        }
    }

    private static bool IsVehicleStatusTimelineItem(VehicleTimelineItem item) =>
        item.Kind is "technical" or "green" or "custom" or "maintenance";

    private static bool IsAttentionStatus(string? status) =>
        !string.IsNullOrWhiteSpace(status)
        && !string.Equals(status, "Bez upozornění", StringComparison.CurrentCultureIgnoreCase);

    private static IReadOnlyList<string?> BuildSearchTexts(IReadOnlyList<string?> vehicleSearchTexts, params string?[] entrySearchTexts)
    {
        if (vehicleSearchTexts.Count == 0)
        {
            return entrySearchTexts;
        }

        return vehicleSearchTexts.Concat(entrySearchTexts).ToArray();
    }

    private static int ComputeRank(IEnumerable<string?> searchTexts, string needle)
    {
        if (string.IsNullOrWhiteSpace(needle))
        {
            return NoSearchMatchRank;
        }

        var bestRank = NoSearchMatchRank;
        foreach (var text in searchTexts)
        {
            var haystack = text?.Trim();
            if (string.IsNullOrWhiteSpace(haystack))
            {
                continue;
            }

            if (haystack.Equals(needle, StringComparison.CurrentCultureIgnoreCase))
            {
                return 0;
            }

            var position = haystack.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase);
            if (position < 0)
            {
                continue;
            }

            var rank = position == 0
                ? 100 + haystack.Length
                : 1000 + position + haystack.Length;
            if (rank < bestRank)
            {
                bestRank = rank;
            }
        }

        return bestRank;
    }

    private static int GetEntityKindPriority(string entityKind) =>
        entityKind switch
        {
            "Vozidlo" => 1,
            "Připomínka" => 2,
            "Doklad" => 3,
            "Historie" => 4,
            "Tankování" => 5,
            "Údržba" => 6,
            _ => 99
        };

    private static string ValueOrFallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string JoinParts(params string?[] parts) =>
        string.Join(" | ", parts.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part!.Trim()));

    private static string FormatPlate(string? plate) =>
        string.IsNullOrWhiteSpace(plate) ? "SPZ Bez SPZ" : $"SPZ {plate.Trim()}";

    private static string FormatOdometer(string? value) =>
        VehimapValueParser.TryParseOdometer(value, out var parsed) ? $"{parsed} km" : ValueOrFallback(value, string.Empty);

    private static string FormatMoneyValue(string? value) =>
        VehimapValueParser.TryParseMoney(value, out var parsed) ? $"{parsed:0.00} Kč" : ValueOrFallback(value, string.Empty);

    private static string FormatFuelLiters(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Contains('l', StringComparison.OrdinalIgnoreCase) ? value : $"{value} l";
    }

    private static string BuildFuelTitle(FuelEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.Liters) || !string.IsNullOrWhiteSpace(entry.TotalCost))
        {
            var fuelLabel = JoinParts(entry.FuelType, entry.FuelDetail);
            return string.IsNullOrWhiteSpace(fuelLabel)
                ? "Tankování"
                : $"Tankování - {fuelLabel}";
        }

        return "Stav tachometru";
    }

    private static string BuildValidity(string? validFrom, string? validTo)
    {
        if (string.IsNullOrWhiteSpace(validFrom) && string.IsNullOrWhiteSpace(validTo))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(validFrom))
        {
            return $"do {validTo}";
        }

        if (string.IsNullOrWhiteSpace(validTo))
        {
            return $"od {validFrom}";
        }

        return $"{validFrom} až {validTo}";
    }

    private static string BuildAttachmentLabel(VehicleRecord record, string resolvedPath)
    {
        var mode = BuildAttachmentModeLabel(record);
        var fileName = !string.IsNullOrWhiteSpace(resolvedPath) ? Path.GetFileName(resolvedPath) : Path.GetFileName(record.FilePath);
        return JoinParts(mode, fileName);
    }

    private static string BuildAttachmentModeLabel(VehicleRecord record) =>
        record.AttachmentMode == VehicleRecordAttachmentMode.Managed ? "Spravovaná kopie" : "Externí cesta";

    private static string BuildMaintenanceInterval(MaintenancePlan plan)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(plan.IntervalKm))
        {
            parts.Add($"{plan.IntervalKm} km");
        }

        if (!string.IsNullOrWhiteSpace(plan.IntervalMonths))
        {
            parts.Add($"{plan.IntervalMonths} měsíců");
        }

        return string.Join(" / ", parts);
    }
}
