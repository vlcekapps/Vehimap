using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacyAuditService : IAuditService
{
    private const string EntityVehicle = "Vozidlo";
    private const string EntityHistory = "Historie";
    private const string EntityFuel = "Tankov\u00E1n\u00ED";
    private const string EntityRecord = "Doklad";
    private const string EntityMaintenance = "\u00DAdr\u017Eba";

    private readonly IFileAttachmentService _attachmentService;
    private readonly IAppLocalizer _localizer;

    public LegacyAuditService(IFileAttachmentService attachmentService, IAppLocalizer? localizer = null)
    {
        _attachmentService = attachmentService;
        _localizer = localizer ?? new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));
    }

    public IReadOnlyList<AuditItem> BuildAudit(VehimapDataRoot dataRoot, VehimapDataSet dataSet)
    {
        var items = new List<AuditItem>();
        var vehiclesById = dataSet.Vehicles.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var metaByVehicleId = dataSet.VehicleMetaEntries
            .GroupBy(item => item.VehicleId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var vehicle in dataSet.Vehicles)
        {
            if (IsVehicleInactive(vehicle, metaByVehicleId))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(vehicle.Plate))
            {
                items.Add(CreateVehicleAudit(
                    AuditSeverity.Warning,
                    L("Audit.Category.Vehicle"),
                    vehicle,
                    L("Audit.Title.MissingPlate"),
                    L("Audit.Message.MissingPlate")));
            }

            if (string.IsNullOrWhiteSpace(vehicle.NextTk))
            {
                items.Add(CreateVehicleAudit(
                    AuditSeverity.Warning,
                    L("Audit.Category.TechnicalInspection"),
                    vehicle,
                    L("Audit.Title.MissingNextTechnicalInspection"),
                    L("Audit.Message.MissingNextTechnicalInspection")));
            }

            if (HasInvalidGreenCardRange(vehicle))
            {
                items.Add(CreateVehicleAudit(
                    AuditSeverity.Error,
                    L("Audit.Category.GreenCard"),
                    vehicle,
                    L("Audit.Title.InvalidGreenCardRange"),
                    L("Audit.Message.InvalidGreenCardRange")));
            }
        }

        foreach (var record in dataSet.Records)
        {
            var vehicle = vehiclesById.GetValueOrDefault(record.VehicleId);
            var vehicleName = vehicle?.Name ?? L("Common.UnknownVehicle");
            var resolvedPath = ResolveRecordPath(dataRoot, record);

            if (record.AttachmentMode == VehicleRecordAttachmentMode.External && string.IsNullOrWhiteSpace(record.FilePath))
            {
                items.Add(new AuditItem(
                    AuditSeverity.Warning,
                    L("Audit.Category.Attachment"),
                    record.VehicleId,
                    vehicleName,
                    EntityRecord,
                    record.Id,
                    L("Audit.Title.RecordWithoutPath"),
                    L("Audit.Message.RecordWithoutPath")));
            }
            else if (!string.IsNullOrWhiteSpace(record.FilePath) && !File.Exists(resolvedPath))
            {
                items.Add(new AuditItem(
                    AuditSeverity.Warning,
                    L("Audit.Category.Attachment"),
                    record.VehicleId,
                    vehicleName,
                    EntityRecord,
                    record.Id,
                    record.AttachmentMode == VehicleRecordAttachmentMode.Managed
                        ? L("Audit.Title.MissingManagedAttachment")
                        : L("Audit.Title.MissingExternalAttachment"),
                    L("Audit.Message.MissingAttachment")));
            }

            if (VehimapValueParser.TryParseMonthYear(record.ValidFrom, out var validFrom)
                && VehimapValueParser.TryParseMonthYear(record.ValidTo, out var validTo)
                && validFrom > validTo)
            {
                items.Add(new AuditItem(
                    AuditSeverity.Error,
                    L("Audit.Category.Document"),
                    record.VehicleId,
                    vehicleName,
                    EntityRecord,
                    record.Id,
                    L("Audit.Title.InvalidValidityRange"),
                    L("Audit.Message.InvalidValidityRange")));
            }

            if (!string.IsNullOrWhiteSpace(record.Price))
            {
                if (!VehimapValueParser.TryParseMoney(record.Price, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        L("Audit.Category.Costs"),
                        record.VehicleId,
                        vehicleName,
                        EntityRecord,
                        record.Id,
                        L("Audit.Title.InvalidAmount"),
                        L("Audit.Message.RecordInvalidAmount")));
                }
                else if (!VehimapValueParser.TryResolveRecordDate(record, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        L("Audit.Category.Costs"),
                        record.VehicleId,
                        vehicleName,
                        EntityRecord,
                        record.Id,
                        L("Audit.Title.MissingUsableDate"),
                        L("Audit.Message.RecordMissingUsableDate")));
                }
            }
        }

        AddHistoryAuditItems(items, vehiclesById, dataSet.HistoryEntries);
        AddFuelAuditItems(items, vehiclesById, dataSet.FuelEntries);
        AddMaintenanceAuditItems(items, vehiclesById, dataSet.MaintenancePlans, dataSet.HistoryEntries, dataSet.FuelEntries);

        return items
            .OrderByDescending(item => item.Severity)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Category, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private void AddHistoryAuditItems(List<AuditItem> items, IReadOnlyDictionary<string, Vehicle> vehiclesById, IEnumerable<VehicleHistoryEntry> historyEntries)
    {
        foreach (var entry in historyEntries)
        {
            var vehicle = vehiclesById.GetValueOrDefault(entry.VehicleId);
            var vehicleName = vehicle?.Name ?? L("Common.UnknownVehicle");

            if (!string.IsNullOrWhiteSpace(entry.Cost))
            {
                if (!VehimapValueParser.TryParseMoney(entry.Cost, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        L("Audit.Category.Costs"),
                        entry.VehicleId,
                        vehicleName,
                        EntityHistory,
                        entry.Id,
                        L("Audit.Title.InvalidAmount"),
                        L("Audit.Message.HistoryInvalidAmount")));
                }
                else if (!VehimapValueParser.TryParseEventDate(entry.EventDate, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        L("Audit.Category.Costs"),
                        entry.VehicleId,
                        vehicleName,
                        EntityHistory,
                        entry.Id,
                        L("Audit.Title.MissingUsableDate"),
                        L("Audit.Message.HistoryMissingUsableDate")));
                }
            }
        }

        AddOdometerRegressionAuditItems(
            items,
            vehiclesById,
            historyEntries.Select(entry => new OdometerSample(entry.VehicleId, entry.Id, EntityHistory, entry.EventDate, entry.Odometer)),
            L("Audit.Category.History"));
    }

    private void AddFuelAuditItems(List<AuditItem> items, IReadOnlyDictionary<string, Vehicle> vehiclesById, IEnumerable<FuelEntry> fuelEntries)
    {
        foreach (var entry in fuelEntries)
        {
            var vehicle = vehiclesById.GetValueOrDefault(entry.VehicleId);
            var vehicleName = vehicle?.Name ?? L("Common.UnknownVehicle");

            if (!string.IsNullOrWhiteSpace(entry.TotalCost))
            {
                if (!VehimapValueParser.TryParseMoney(entry.TotalCost, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        L("Audit.Category.Costs"),
                        entry.VehicleId,
                        vehicleName,
                        EntityFuel,
                        entry.Id,
                        L("Audit.Title.InvalidAmount"),
                        L("Audit.Message.FuelInvalidAmount")));
                }
                else if (!VehimapValueParser.TryParseEventDate(entry.EntryDate, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        L("Audit.Category.Costs"),
                        entry.VehicleId,
                        vehicleName,
                        EntityFuel,
                        entry.Id,
                        L("Audit.Title.MissingUsableDate"),
                        L("Audit.Message.FuelMissingUsableDate")));
                }
            }
        }

        AddOdometerRegressionAuditItems(
            items,
            vehiclesById,
            fuelEntries.Select(entry => new OdometerSample(entry.VehicleId, entry.Id, EntityFuel, entry.EntryDate, entry.Odometer)),
            L("Audit.Category.Fuel"));
    }

    private void AddMaintenanceAuditItems(
        List<AuditItem> items,
        IReadOnlyDictionary<string, Vehicle> vehiclesById,
        IEnumerable<MaintenancePlan> maintenancePlans,
        IEnumerable<VehicleHistoryEntry> historyEntries,
        IEnumerable<FuelEntry> fuelEntries)
    {
        var currentOdometers = BuildCurrentOdometerLookup(historyEntries, fuelEntries);

        foreach (var plan in maintenancePlans)
        {
            if (!plan.IsActive || !VehimapValueParser.TryParseOdometer(plan.IntervalKm, out _))
            {
                continue;
            }

            if (currentOdometers.ContainsKey(plan.VehicleId))
            {
                continue;
            }

            var vehicle = vehiclesById.GetValueOrDefault(plan.VehicleId);
            var vehicleName = vehicle?.Name ?? L("Common.UnknownVehicle");
            items.Add(new AuditItem(
                AuditSeverity.Warning,
                L("Audit.Category.Maintenance"),
                plan.VehicleId,
                vehicleName,
                EntityMaintenance,
                plan.Id,
                L("Audit.Title.MissingUsableOdometer"),
                L("Audit.Message.MaintenanceMissingUsableOdometer")));
        }
    }

    private static Dictionary<string, int> BuildCurrentOdometerLookup(IEnumerable<VehicleHistoryEntry> historyEntries, IEnumerable<FuelEntry> fuelEntries)
    {
        var latestByVehicle = new Dictionary<string, (DateOnly Date, int Odometer)>(StringComparer.Ordinal);

        foreach (var sample in historyEntries.Select(entry => new OdometerSample(entry.VehicleId, entry.Id, EntityHistory, entry.EventDate, entry.Odometer))
                     .Concat(fuelEntries.Select(entry => new OdometerSample(entry.VehicleId, entry.Id, EntityFuel, entry.EntryDate, entry.Odometer))))
        {
            if (!VehimapValueParser.TryParseEventDate(sample.DateText, out var sampleDate)
                || !VehimapValueParser.TryParseOdometer(sample.OdometerText, out var odometer))
            {
                continue;
            }

            if (!latestByVehicle.TryGetValue(sample.VehicleId, out var current)
                || sampleDate > current.Date
                || (sampleDate == current.Date && odometer > current.Odometer))
            {
                latestByVehicle[sample.VehicleId] = (sampleDate, odometer);
            }
        }

        return latestByVehicle.ToDictionary(item => item.Key, item => item.Value.Odometer, StringComparer.Ordinal);
    }

    private void AddOdometerRegressionAuditItems(
        List<AuditItem> items,
        IReadOnlyDictionary<string, Vehicle> vehiclesById,
        IEnumerable<OdometerSample> samples,
        string category)
    {
        foreach (var group in samples.GroupBy(item => item.VehicleId, StringComparer.Ordinal))
        {
            var ordered = group
                .Select(item =>
                {
                    var hasDate = VehimapValueParser.TryParseEventDate(item.DateText, out var sampleDate);
                    var hasOdometer = VehimapValueParser.TryParseOdometer(item.OdometerText, out var odometer);
                    return new
                    {
                        item.EntityId,
                        item.VehicleId,
                        item.EntityKind,
                        HasDate = hasDate,
                        Date = sampleDate,
                        HasOdometer = hasOdometer,
                        Odometer = odometer
                    };
                })
                .Where(item => item.HasDate && item.HasOdometer)
                .OrderBy(item => item.Date)
                .ThenBy(item => item.Odometer)
                .ToList();

            for (var index = 1; index < ordered.Count; index++)
            {
                var previous = ordered[index - 1];
                var current = ordered[index];
                if (current.Odometer >= previous.Odometer)
                {
                    continue;
                }

                var vehicle = vehiclesById.GetValueOrDefault(group.Key);
                var vehicleName = vehicle?.Name ?? L("Common.UnknownVehicle");
                items.Add(new AuditItem(
                    AuditSeverity.Error,
                    category,
                    group.Key,
                    vehicleName,
                    current.EntityKind,
                    current.EntityId,
                    L("Audit.Title.OdometerRegression"),
                    LF("Audit.Message.OdometerRegression", current.Odometer, previous.Odometer)));
            }
        }
    }

    private static AuditItem CreateVehicleAudit(AuditSeverity severity, string category, Vehicle vehicle, string title, string message)
    {
        return new AuditItem(severity, category, vehicle.Id, vehicle.Name, EntityVehicle, vehicle.Id, title, message);
    }

    private static bool HasInvalidGreenCardRange(Vehicle vehicle)
    {
        return VehimapValueParser.TryParseMonthYear(vehicle.GreenCardFrom, out var from)
               && VehimapValueParser.TryParseMonthYear(vehicle.GreenCardTo, out var to)
               && from > to;
    }

    private static bool IsVehicleInactive(Vehicle vehicle, IReadOnlyDictionary<string, VehicleMeta> metaByVehicleId)
    {
        var state = metaByVehicleId.TryGetValue(vehicle.Id, out var meta)
            ? (meta.State ?? string.Empty).Trim()
            : string.Empty;

        return state.Equals("Archiv", StringComparison.OrdinalIgnoreCase)
               || state.Equals("Odstaveno", StringComparison.OrdinalIgnoreCase);
    }

    private string ResolveRecordPath(VehimapDataRoot dataRoot, VehicleRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.FilePath))
        {
            return string.Empty;
        }

        if (record.AttachmentMode == VehicleRecordAttachmentMode.Managed)
        {
            return _attachmentService.ResolveManagedAttachmentPath(dataRoot, record.FilePath);
        }

        return Path.IsPathRooted(record.FilePath)
            ? record.FilePath
            : Path.GetFullPath(Path.Combine(dataRoot.AppBasePath, record.FilePath));
    }

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);

    private sealed record OdometerSample(
        string VehicleId,
        string EntityId,
        string EntityKind,
        string DateText,
        string OdometerText);
}
