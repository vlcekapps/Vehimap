using Vehimap.Application.Abstractions;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacyAuditService : IAuditService
{
    private readonly IFileAttachmentService _attachmentService;

    public LegacyAuditService(IFileAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
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
                    "Vozidlo",
                    vehicle,
                    "Chybí SPZ",
                    "Aktivní vozidlo nemá vyplněnou registrační značku."));
            }

            if (string.IsNullOrWhiteSpace(vehicle.NextTk))
            {
                items.Add(CreateVehicleAudit(
                    AuditSeverity.Warning,
                    "Technická kontrola",
                    vehicle,
                    "Chybí příští TK",
                    "Aktivní vozidlo nemá vyplněný termín příští technické kontroly."));
            }

            if (HasInvalidGreenCardRange(vehicle))
            {
                items.Add(CreateVehicleAudit(
                    AuditSeverity.Error,
                    "Zelená karta",
                    vehicle,
                    "Neplatný rozsah zelené karty",
                    "Rozsah zelené karty je neplatný, protože pole Platné od je později než pole Platné do."));
            }
        }

        foreach (var record in dataSet.Records)
        {
            var vehicle = vehiclesById.GetValueOrDefault(record.VehicleId);
            var vehicleName = vehicle?.Name ?? "Neznámé vozidlo";
            var resolvedPath = ResolveRecordPath(dataRoot, record);

            if (record.AttachmentMode == VehicleRecordAttachmentMode.External && string.IsNullOrWhiteSpace(record.FilePath))
            {
                items.Add(new AuditItem(
                    AuditSeverity.Warning,
                    "Příloha",
                    record.VehicleId,
                    vehicleName,
                    "Doklad",
                    record.Id,
                    "Doklad bez cesty",
                    "Doklad nemá vyplněnou cestu k příloze."));
            }
            else if (!string.IsNullOrWhiteSpace(record.FilePath) && !File.Exists(resolvedPath))
            {
                items.Add(new AuditItem(
                    AuditSeverity.Warning,
                    "Příloha",
                    record.VehicleId,
                    vehicleName,
                    "Doklad",
                    record.Id,
                    record.AttachmentMode == VehicleRecordAttachmentMode.Managed ? "Chybí spravovaná příloha" : "Chybí externí příloha",
                    "U dokladu není dostupný soubor přílohy v očekávaném umístění."));
            }

            if (VehimapValueParser.TryParseMonthYear(record.ValidFrom, out var validFrom)
                && VehimapValueParser.TryParseMonthYear(record.ValidTo, out var validTo)
                && validFrom > validTo)
            {
                items.Add(new AuditItem(
                    AuditSeverity.Error,
                    "Doklad",
                    record.VehicleId,
                    vehicleName,
                    "Doklad",
                    record.Id,
                    "Neplatný rozsah platnosti",
                    "Datum platnosti od je později než datum platnosti do."));
            }

            if (!string.IsNullOrWhiteSpace(record.Price))
            {
                if (!VehimapValueParser.TryParseMoney(record.Price, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        "Náklady",
                        record.VehicleId,
                        vehicleName,
                        "Doklad",
                        record.Id,
                        "Neplatná částka",
                        "Doklad obsahuje cenu, kterou se nepodařilo převést na číslo."));
                }
                else if (!VehimapValueParser.TryResolveRecordDate(record, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        "Náklady",
                        record.VehicleId,
                        vehicleName,
                        "Doklad",
                        record.Id,
                        "Chybí použitelné datum",
                        "Doklad má cenu, ale nemá použitelné datum pro zařazení do nákladů."));
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

    private static void AddHistoryAuditItems(List<AuditItem> items, IReadOnlyDictionary<string, Vehicle> vehiclesById, IEnumerable<VehicleHistoryEntry> historyEntries)
    {
        foreach (var entry in historyEntries)
        {
            var vehicle = vehiclesById.GetValueOrDefault(entry.VehicleId);
            var vehicleName = vehicle?.Name ?? "Neznámé vozidlo";

            if (!string.IsNullOrWhiteSpace(entry.Cost))
            {
                if (!VehimapValueParser.TryParseMoney(entry.Cost, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        "Náklady",
                        entry.VehicleId,
                        vehicleName,
                        "Historie",
                        entry.Id,
                        "Neplatná částka",
                        "Historická událost obsahuje částku, kterou se nepodařilo převést na číslo."));
                }
                else if (!VehimapValueParser.TryParseEventDate(entry.EventDate, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        "Náklady",
                        entry.VehicleId,
                        vehicleName,
                        "Historie",
                        entry.Id,
                        "Chybí použitelné datum",
                        "Historická událost má cenu, ale nemá použitelné datum."));
                }
            }
        }

        AddOdometerRegressionAuditItems(
            items,
            vehiclesById,
            historyEntries.Select(entry => new OdometerSample(entry.VehicleId, entry.Id, "Historie", entry.EventDate, entry.Odometer)),
            "Historie");
    }

    private static void AddFuelAuditItems(List<AuditItem> items, IReadOnlyDictionary<string, Vehicle> vehiclesById, IEnumerable<FuelEntry> fuelEntries)
    {
        foreach (var entry in fuelEntries)
        {
            var vehicle = vehiclesById.GetValueOrDefault(entry.VehicleId);
            var vehicleName = vehicle?.Name ?? "Neznámé vozidlo";

            if (!string.IsNullOrWhiteSpace(entry.TotalCost))
            {
                if (!VehimapValueParser.TryParseMoney(entry.TotalCost, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        "Náklady",
                        entry.VehicleId,
                        vehicleName,
                        "Tankování",
                        entry.Id,
                        "Neplatná částka",
                        "Tankování obsahuje částku, kterou se nepodařilo převést na číslo."));
                }
                else if (!VehimapValueParser.TryParseEventDate(entry.EntryDate, out _))
                {
                    items.Add(new AuditItem(
                        AuditSeverity.Warning,
                        "Náklady",
                        entry.VehicleId,
                        vehicleName,
                        "Tankování",
                        entry.Id,
                        "Chybí použitelné datum",
                        "Tankování má cenu, ale nemá použitelné datum."));
                }
            }
        }

        AddOdometerRegressionAuditItems(
            items,
            vehiclesById,
            fuelEntries.Select(entry => new OdometerSample(entry.VehicleId, entry.Id, "Tankování", entry.EntryDate, entry.Odometer)),
            "Tankování");
    }

    private static void AddMaintenanceAuditItems(
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
            var vehicleName = vehicle?.Name ?? "Neznámé vozidlo";
            items.Add(new AuditItem(
                AuditSeverity.Warning,
                "Údržba",
                plan.VehicleId,
                vehicleName,
                "Údržba",
                plan.Id,
                "Chybí použitelný tachometr",
                "Plán údržby používá kilometrový interval, ale u vozidla není k dispozici použitelný aktuální tachometr."));
        }
    }

    private static Dictionary<string, int> BuildCurrentOdometerLookup(IEnumerable<VehicleHistoryEntry> historyEntries, IEnumerable<FuelEntry> fuelEntries)
    {
        var latestByVehicle = new Dictionary<string, (DateOnly Date, int Odometer)>(StringComparer.Ordinal);

        foreach (var sample in historyEntries.Select(entry => new OdometerSample(entry.VehicleId, entry.Id, "Historie", entry.EventDate, entry.Odometer))
                     .Concat(fuelEntries.Select(entry => new OdometerSample(entry.VehicleId, entry.Id, "Tankování", entry.EntryDate, entry.Odometer))))
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

    private static void AddOdometerRegressionAuditItems(
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
                var vehicleName = vehicle?.Name ?? "Neznámé vozidlo";
                items.Add(new AuditItem(
                    AuditSeverity.Error,
                    category,
                    group.Key,
                    vehicleName,
                    current.EntityKind,
                    current.EntityId,
                    "Klesající tachometr",
                    $"Hodnota tachometru {current.Odometer} je nižší než dříve zaznamenaná hodnota {previous.Odometer}."));
            }
        }
    }

    private static AuditItem CreateVehicleAudit(AuditSeverity severity, string category, Vehicle vehicle, string title, string message)
    {
        return new AuditItem(severity, category, vehicle.Id, vehicle.Name, "Vozidlo", vehicle.Id, title, message);
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

    private sealed record OdometerSample(
        string VehicleId,
        string EntityId,
        string EntityKind,
        string DateText,
        string OdometerText);
}
