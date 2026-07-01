// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacyGlobalSearchService : IGlobalSearchService
{
    private const int NoSearchMatchRank = 1_000_000;
    private const string EntityVehicle = "Vozidlo";
    private const string EntityHistory = "Historie";
    private const string EntityFuel = "Tankov\u00E1n\u00ED";
    private const string EntityRecord = "Doklad";
    private const string EntityReminder = "P\u0159ipom\u00EDnka";
    private const string EntityMaintenance = "\u00DAdr\u017Eba";
    private const string NeutralTimelineStatusCs = "Bez upozorn\u011Bn\u00ED";
    private const string NeutralTimelineStatusEn = "No alert";

    private readonly IFileAttachmentService _attachmentService;
    private readonly ITimelineService _timelineService;
    private readonly IAppLocalizer _localizer;
    private readonly IAppNumberFormatService _numberFormatService;
    private AppCulturePreferences _culturePreferences = new(AppCultureService.CzechLanguage, AppCultureService.NoSeparator, AppCultureService.CommaSeparator);
    private string _currency = AppCurrencyFormatService.CzechCrowns;

    public LegacyGlobalSearchService(IFileAttachmentService attachmentService)
        : this(attachmentService, new LegacyTimelineService(), null)
    {
    }

    public LegacyGlobalSearchService(IFileAttachmentService attachmentService, ITimelineService timelineService, IAppLocalizer? localizer = null, IAppNumberFormatService? numberFormatService = null)
    {
        _attachmentService = attachmentService;
        _timelineService = timelineService;
        _localizer = localizer ?? CreateDefaultLocalizer();
        _numberFormatService = numberFormatService ?? new AppNumberFormatService();
    }

    public void ApplySupportedSettings(DesktopSupportedSettingsSnapshot settings)
    {
        _culturePreferences = new AppCulturePreferences(
            settings.Language,
            settings.ThousandsSeparator,
            settings.DecimalSeparator);
        _currency = AppCurrencyFormatService.NormalizeCurrency(settings.Currency);
        if (_timelineService is LegacyTimelineService legacyTimelineService)
        {
            legacyTimelineService.ApplySupportedSettings(settings);
        }
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
            var title = ValueOrFallback(vehicle.Name, L("GlobalSearch.Value.UntitledVehicle"));
            var summary = JoinParts(
                ValueOrFallback(vehicle.MakeModel, L("GlobalSearch.Value.NoMakeModel")),
                ValueOrFallback(vehicle.Category, L("GlobalSearch.Value.NoCategory")),
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
                    EntityVehicle,
                    vehicle.Id,
                    L("GlobalSearch.Entity.Vehicle"),
                    title,
                    summary,
                    rank));
            }
        }

        foreach (var entry in dataSet.HistoryEntries)
        {
            var vehicle = vehiclesById.GetValueOrDefault(entry.VehicleId);
            var vehicleName = vehicle?.Name ?? L("Common.UnknownVehicle");
            var meta = vehicle is null ? null : metaByVehicleId.GetValueOrDefault(vehicle.Id);
            var timeline = GetVehicleTimeline(timelineByVehicleId, entry.VehicleId);
            var title = ValueOrFallback(entry.EventType, L("GlobalSearch.Entity.History"));
            var summary = JoinParts(
                ValueOrFallback(entry.EventDate, L("Common.NoDate")),
                FormatOdometer(entry.Odometer),
                FormatMoneyValue(entry.Cost),
                ValueOrFallback(entry.Note, string.Empty));
            var searchTexts = BuildSearchTexts(
                BuildVehicleSearchTexts(vehicle, meta, timeline),
                EntityHistory,
                L("GlobalSearch.Entity.History"),
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
                    EntityHistory,
                    entry.Id,
                    L("GlobalSearch.Entity.History"),
                    title,
                    summary,
                    rank));
            }
        }

        foreach (var entry in dataSet.FuelEntries)
        {
            var vehicle = vehiclesById.GetValueOrDefault(entry.VehicleId);
            var vehicleName = vehicle?.Name ?? L("Common.UnknownVehicle");
            var meta = vehicle is null ? null : metaByVehicleId.GetValueOrDefault(vehicle.Id);
            var timeline = GetVehicleTimeline(timelineByVehicleId, entry.VehicleId);
            var title = BuildFuelTitle(entry);
            var summary = JoinParts(
                ValueOrFallback(entry.EntryDate, L("Common.NoDate")),
                ValueOrFallback(entry.FuelType, L("GlobalSearch.Value.NoFuelType")),
                FormatFuelLiters(entry.Liters),
                FormatOdometer(entry.Odometer),
                FormatMoneyValue(entry.TotalCost),
                entry.FullTank ? L("GlobalSearch.Value.FullTank") : string.Empty,
                ValueOrFallback(entry.FuelDetail, string.Empty),
                ValueOrFallback(entry.Station, string.Empty),
                ValueOrFallback(entry.Note, string.Empty));
            var searchTexts = BuildSearchTexts(
                BuildVehicleSearchTexts(vehicle, meta, timeline),
                EntityFuel,
                L("GlobalSearch.Entity.Fuel"),
                entry.EntryDate,
                entry.Odometer,
                entry.Liters,
                entry.TotalCost,
                entry.FullTank ? L("GlobalSearch.Value.Yes") : L("GlobalSearch.Value.No"),
                entry.FullTank ? L("GlobalSearch.Value.FullTank") : string.Empty,
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
                    EntityFuel,
                    entry.Id,
                    L("GlobalSearch.Entity.Fuel"),
                    title,
                    summary,
                    rank));
            }
        }

        foreach (var record in dataSet.Records)
        {
            var vehicle = vehiclesById.GetValueOrDefault(record.VehicleId);
            var vehicleName = vehicle?.Name ?? L("Common.UnknownVehicle");
            var meta = vehicle is null ? null : metaByVehicleId.GetValueOrDefault(vehicle.Id);
            var timeline = GetVehicleTimeline(timelineByVehicleId, record.VehicleId);
            var timelineStatus = FindTimelineStatus(timeline, "record", record.Id);
            var title = ValueOrFallback(record.Title, L("GlobalSearch.Entity.Record"));
            var resolvedPath = ResolveRecordPath(dataRoot, record);
            var summary = JoinParts(
                ValueOrFallback(record.RecordType, L("GlobalSearch.Entity.Record")),
                ValueOrFallback(record.Provider, string.Empty),
                BuildValidity(record.ValidFrom, record.ValidTo),
                BuildAttachmentLabel(record, resolvedPath),
                timelineStatus,
                ValueOrFallback(record.Note, string.Empty));
            var searchTexts = BuildSearchTexts(
                BuildVehicleSearchTexts(vehicle, meta, timeline),
                EntityRecord,
                L("GlobalSearch.Entity.Record"),
                L("GlobalSearch.Entity.Records"),
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
                    EntityRecord,
                    record.Id,
                    L("GlobalSearch.Entity.Records"),
                    title,
                    summary,
                    rank));
            }
        }

        foreach (var reminder in dataSet.Reminders)
        {
            var vehicle = vehiclesById.GetValueOrDefault(reminder.VehicleId);
            var vehicleName = vehicle?.Name ?? L("Common.UnknownVehicle");
            var meta = vehicle is null ? null : metaByVehicleId.GetValueOrDefault(vehicle.Id);
            var timeline = GetVehicleTimeline(timelineByVehicleId, reminder.VehicleId);
            var timelineStatus = FindTimelineStatus(timeline, "custom", reminder.Id);
            var title = ValueOrFallback(reminder.Title, L("GlobalSearch.Entity.Reminder"));
            var summary = JoinParts(
                ValueOrFallback(reminder.DueDate, L("GlobalSearch.Value.NoDueDate")),
                ValueOrFallback(reminder.RepeatMode, string.Empty),
                timelineStatus,
                ValueOrFallback(reminder.Note, string.Empty));
            var searchTexts = BuildSearchTexts(
                BuildVehicleSearchTexts(vehicle, meta, timeline),
                EntityReminder,
                L("GlobalSearch.Entity.Reminder"),
                L("GlobalSearch.Entity.Reminders"),
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
                    EntityReminder,
                    reminder.Id,
                    L("GlobalSearch.Entity.Reminders"),
                    title,
                    summary,
                    rank));
            }
        }

        foreach (var plan in dataSet.MaintenancePlans)
        {
            var vehicle = vehiclesById.GetValueOrDefault(plan.VehicleId);
            var vehicleName = vehicle?.Name ?? L("Common.UnknownVehicle");
            var meta = vehicle is null ? null : metaByVehicleId.GetValueOrDefault(vehicle.Id);
            var timeline = GetVehicleTimeline(timelineByVehicleId, plan.VehicleId);
            var timelineItem = FindTimelineItem(timeline, "maintenance", plan.Id);
            var timelineStatus = FormatSearchableTimelineStatus(timelineItem?.Status);
            var title = ValueOrFallback(plan.Title, L("GlobalSearch.Value.ServiceTask"));
            var summary = JoinParts(
                BuildMaintenanceInterval(plan),
                ValueOrFallback(plan.LastServiceDate, string.Empty),
                FormatOdometer(plan.LastServiceOdometer),
                timelineItem?.Detail,
                timelineStatus,
                plan.IsActive ? L("GlobalSearch.Value.Active") : L("GlobalSearch.Value.Inactive"),
                ValueOrFallback(plan.Note, string.Empty));
            var searchTexts = BuildSearchTexts(
                BuildVehicleSearchTexts(vehicle, meta, timeline),
                EntityMaintenance,
                L("GlobalSearch.Entity.Maintenance"),
                plan.Title,
                plan.IntervalKm,
                plan.IntervalMonths,
                plan.LastServiceDate,
                plan.LastServiceOdometer,
                timelineItem?.Detail,
                timelineStatus,
                plan.IsActive ? L("GlobalSearch.Value.Active") : L("GlobalSearch.Value.Inactive"),
                plan.Note);

            var rank = ComputeRank(searchTexts, needle);
            if (rank < NoSearchMatchRank)
            {
                results.Add(new GlobalSearchResult(
                    plan.VehicleId,
                    vehicleName,
                    EntityMaintenance,
                    plan.Id,
                    L("GlobalSearch.Entity.Maintenance"),
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

    private IReadOnlyList<string?> BuildVehicleSearchTexts(
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
            EntityVehicle,
            L("GlobalSearch.Entity.Vehicle"),
            includeTimelineStatus ? statusText : string.Empty,
            .. (includeTimelineStatus
                ? timelineItems
                    .Where(IsVehicleStatusTimelineItem)
                    .SelectMany(item => new string?[] { item.KindLabel, item.Title, item.Detail, FormatSearchableTimelineStatus(item.Status) })
                : [])
        ];
    }

    private static VehicleTimelineItem? FindTimelineItem(IReadOnlyList<VehicleTimelineItem> timelineItems, string kind, string entryId) =>
        timelineItems.FirstOrDefault(item =>
            string.Equals(item.Kind, kind, StringComparison.Ordinal)
            && string.Equals(item.EntryId, entryId, StringComparison.Ordinal));

    private static string FindTimelineStatus(IReadOnlyList<VehicleTimelineItem> timelineItems, string kind, string entryId) =>
        FormatSearchableTimelineStatus(FindTimelineItem(timelineItems, kind, entryId)?.Status);

    private static string FormatSearchableTimelineStatus(string? status) =>
        IsAttentionStatus(status) ? status ?? string.Empty : string.Empty;

    private string BuildVehicleAttentionStatusText(IReadOnlyList<VehicleTimelineItem> timelineItems)
    {
        var parts = new List<string>();
        AddTimelineStatusPart(parts, timelineItems, "technical", L("GlobalSearch.Attention.TechnicalInspection"));
        AddTimelineStatusPart(parts, timelineItems, "green", L("GlobalSearch.Attention.GreenCard"));
        AddTimelineStatusPart(parts, timelineItems, "custom", L("GlobalSearch.Entity.Reminder"));
        AddTimelineStatusPart(parts, timelineItems, "maintenance", L("GlobalSearch.Entity.Maintenance"));
        return string.Join(" | ", parts);
    }

    private void AddTimelineStatusPart(List<string> parts, IReadOnlyList<VehicleTimelineItem> timelineItems, string kind, string label)
    {
        var status = timelineItems
            .Where(item => string.Equals(item.Kind, kind, StringComparison.Ordinal))
            .Select(item => item.Status)
            .FirstOrDefault(IsAttentionStatus);

        if (!string.IsNullOrWhiteSpace(status))
        {
            parts.Add(LF("GlobalSearch.Attention.StatusPart", label, status));
        }
    }

    private static bool IsVehicleStatusTimelineItem(VehicleTimelineItem item) =>
        item.Kind is "technical" or "green" or "custom" or "maintenance";

    private static bool IsAttentionStatus(string? status) =>
        !string.IsNullOrWhiteSpace(status)
        && !string.Equals(status, NeutralTimelineStatusCs, StringComparison.CurrentCultureIgnoreCase)
        && !string.Equals(status, NeutralTimelineStatusEn, StringComparison.CurrentCultureIgnoreCase);

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
            EntityVehicle => 1,
            EntityReminder => 2,
            EntityRecord => 3,
            EntityHistory => 4,
            EntityFuel => 5,
            EntityMaintenance => 6,
            _ => 99
        };

    private static string ValueOrFallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string JoinParts(params string?[] parts) =>
        string.Join(" | ", parts.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part!.Trim()));

    private string FormatPlate(string? plate) =>
        LF("GlobalSearch.Value.Plate", string.IsNullOrWhiteSpace(plate) ? L("GlobalSearch.Value.NoPlate") : plate.Trim());

    private string FormatOdometer(string? value) =>
        VehimapValueParser.TryParseOdometer(value, out var parsed) ? LF("GlobalSearch.Value.OdometerKm", parsed) : ValueOrFallback(value, string.Empty);

    private string FormatMoneyValue(string? value) =>
        VehimapValueParser.TryParseMoney(value, out var parsed)
            ? LF("GlobalSearch.Value.Money", _numberFormatService.FormatMoney(parsed, _culturePreferences, _currency))
            : ValueOrFallback(value, string.Empty);

    private string FormatFuelLiters(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Contains('l', StringComparison.OrdinalIgnoreCase) ? value : LF("GlobalSearch.Value.Liters", value.Trim());
    }

    private string BuildFuelTitle(FuelEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.Liters) || !string.IsNullOrWhiteSpace(entry.TotalCost))
        {
            var fuelLabel = JoinParts(entry.FuelType, entry.FuelDetail);
            return string.IsNullOrWhiteSpace(fuelLabel)
                ? L("GlobalSearch.Entity.Fuel")
                : LF("GlobalSearch.Title.FuelWithLabel", fuelLabel);
        }

        return L("GlobalSearch.Title.OdometerState");
    }

    private string BuildValidity(string? validFrom, string? validTo)
    {
        if (string.IsNullOrWhiteSpace(validFrom) && string.IsNullOrWhiteSpace(validTo))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(validFrom))
        {
            return LF("GlobalSearch.Validity.To", validTo);
        }

        if (string.IsNullOrWhiteSpace(validTo))
        {
            return LF("GlobalSearch.Validity.From", validFrom);
        }

        return LF("GlobalSearch.Validity.Range", validFrom, validTo);
    }

    private string BuildAttachmentLabel(VehicleRecord record, string resolvedPath)
    {
        var mode = BuildAttachmentModeLabel(record);
        var fileName = !string.IsNullOrWhiteSpace(resolvedPath) ? Path.GetFileName(resolvedPath) : Path.GetFileName(record.FilePath);
        return JoinParts(mode, fileName);
    }

    private string BuildAttachmentModeLabel(VehicleRecord record) =>
        record.AttachmentMode == VehicleRecordAttachmentMode.Managed ? L("GlobalSearch.Attachment.Managed") : L("GlobalSearch.Attachment.External");

    private string BuildMaintenanceInterval(MaintenancePlan plan)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(plan.IntervalKm))
        {
            parts.Add(LF("GlobalSearch.Value.OdometerKm", plan.IntervalKm.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(plan.IntervalMonths))
        {
            parts.Add(LF("GlobalSearch.Value.Months", plan.IntervalMonths.Trim()));
        }

        return string.Join(" / ", parts);
    }

    private static IAppLocalizer CreateDefaultLocalizer() =>
        new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);
}
