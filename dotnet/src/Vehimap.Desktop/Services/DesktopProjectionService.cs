using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopProjectionService
{
    public IReadOnlyList<VehicleListItemViewModel> BuildVehicleList(
        VehimapDataSet dataSet,
        IReadOnlyDictionary<string, VehicleMeta> metaByVehicleId,
        IReadOnlyCollection<AuditItem> auditItems)
    {
        return dataSet.Vehicles
            .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(vehicle =>
            {
                var meta = metaByVehicleId.GetValueOrDefault(vehicle.Id);
                return new VehicleListItemViewModel(
                    vehicle.Id,
                    vehicle.Name,
                    vehicle.Category,
                    FormatValue(vehicle.Plate, "Bez SPZ"),
                    FormatValue(vehicle.MakeModel, "Bez značky / modelu"),
                    vehicle.VehicleNote,
                    vehicle.NextTk,
                    vehicle.GreenCardTo,
                    meta?.State ?? string.Empty,
                    meta?.Powertrain ?? string.Empty,
                    BuildVehicleStatusSummary(vehicle, meta, auditItems));
            })
            .ToList();
    }

    public IReadOnlyList<AuditItemViewModel> BuildDashboardAuditItems(IReadOnlyList<AuditItem> auditItems) =>
        auditItems
            .Take(8)
            .Select(item => new AuditItemViewModel(
                item.VehicleId,
                item.EntityKind,
                item.EntityId,
                item.Severity switch
                {
                    AuditSeverity.Error => "Chyba",
                    AuditSeverity.Warning => "Upozornění",
                    _ => "Info"
                },
                item.Category,
                item.VehicleName,
                item.Title,
                item.Message))
            .ToList();

    public IReadOnlyList<CostVehicleItemViewModel> BuildDashboardCostVehicles(CostAnalysisSummary costSummary) =>
        costSummary.Vehicles
            .Where(item => item.TotalCost > 0m || item.Status != "Neaktivní")
            .Take(8)
            .Select(row => new CostVehicleItemViewModel(
                row.VehicleId,
                row.VehicleName,
                row.Category,
                FormatMoney(row.TotalCost),
                row.DistanceKm.HasValue ? $"{row.DistanceKm.Value} km" : "nedostupné",
                row.CostPerKm.HasValue ? $"{row.CostPerKm.Value:0.00} Kč/km" : "nedostupné",
                row.Status))
            .ToList();

    public DesktopVehicleDetailProjection BuildVehicleDetail(VehicleListItemViewModel? vehicle)
    {
        if (vehicle is null)
        {
            return new DesktopVehicleDetailProjection(
                "Nevybrané vozidlo",
                "Vyberte vozidlo vlevo a zobrazí se jeho základní souhrn.",
                string.Empty,
                string.Empty);
        }

        var state = string.IsNullOrWhiteSpace(vehicle.State) ? "Běžný provoz" : vehicle.State;
        var note = string.IsNullOrWhiteSpace(vehicle.VehicleNote) ? "Bez poznámky" : vehicle.VehicleNote;
        var powertrain = string.IsNullOrWhiteSpace(vehicle.Powertrain) ? "Servisní profil zatím nevyplněn" : vehicle.Powertrain;

        return new DesktopVehicleDetailProjection(
            vehicle.Name,
            $"{vehicle.MakeModel} | {vehicle.Category} | {vehicle.Plate}\nStav: {state}\nPoznámka: {note}",
            $"Příští TK: {FormatValue(vehicle.NextTk, "nevyplněno")}\nZelená karta do: {FormatValue(vehicle.GreenCardTo, "nevyplněno")}\nSouhrnný stav: {FormatValue(vehicle.StatusSummary, "bez upozornění")}",
            $"Pohon a servisní profil: {powertrain}");
    }

    public DesktopListProjection<VehicleHistoryItemViewModel> BuildHistory(VehimapDataSet dataSet, string vehicleId)
    {
        var items = dataSet.HistoryEntries
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryParseEventDate(item.EventDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Item.EventType, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new VehicleHistoryItemViewModel(
                item.Item.Id,
                FormatValue(item.Item.EventDate, "bez data"),
                FormatValue(item.Item.EventType, "bez typu"),
                FormatValue(item.Item.Odometer, "bez tachometru"),
                FormatValue(item.Item.Cost, "bez ceny"),
                FormatValue(item.Item.Note, "bez poznámky")))
            .ToList();

        var summary = items.Count == 0
            ? "Vybrané vozidlo zatím nemá žádné záznamy v historii."
            : $"Vybrané vozidlo má {items.Count} historických záznamů.";

        return new DesktopListProjection<VehicleHistoryItemViewModel>(items, summary);
    }

    public DesktopListProjection<VehicleFuelItemViewModel> BuildFuel(VehimapDataSet dataSet, string vehicleId)
    {
        var items = dataSet.FuelEntries
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryParseEventDate(item.EntryDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Item.FuelType, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new VehicleFuelItemViewModel(
                item.Item.Id,
                FormatValue(item.Item.EntryDate, "bez data"),
                FormatValue(item.Item.FuelType, "bez typu"),
                FormatFuelLiters(item.Item.Liters),
                FormatCostValue(item.Item.TotalCost),
                FormatOdometerValue(item.Item.Odometer),
                item.Item.FullTank ? "Plná nádrž" : "Částečné tankování",
                FormatValue(item.Item.Note, "bez poznámky")))
            .ToList();

        var summary = items.Count == 0
            ? "Vybrané vozidlo zatím nemá žádné záznamy tankování."
            : $"Vybrané vozidlo má {items.Count} záznamů tankování.";

        return new DesktopListProjection<VehicleFuelItemViewModel>(items, summary);
    }

    public DesktopListProjection<VehicleReminderItemViewModel> BuildReminders(VehimapDataSet dataSet, string vehicleId, DateOnly today)
    {
        var items = dataSet.Reminders
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = TryParseReminderDate(item.DueDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenBy(item => item.Date)
            .ThenBy(item => item.Item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new VehicleReminderItemViewModel(
                item.Item.Id,
                FormatValue(item.Item.Title, "Bez názvu"),
                FormatValue(item.Item.DueDate, "bez termínu"),
                BuildReminderStatus(item.Item, today),
                FormatReminderRepeatMode(item.Item.RepeatMode),
                FormatValue(item.Item.Note, "bez poznámky")))
            .ToList();

        var summary = items.Count == 0
            ? "Vybrané vozidlo zatím nemá žádné připomínky."
            : $"Vybrané vozidlo má {items.Count} připomínek.";

        return new DesktopListProjection<VehicleReminderItemViewModel>(items, summary);
    }

    public DesktopListProjection<VehicleMaintenanceItemViewModel> BuildMaintenance(VehimapDataSet dataSet, string vehicleId, DateOnly today)
    {
        var currentOdometerByVehicleId = BuildCurrentOdometerLookup(dataSet);
        var currentOdometer = currentOdometerByVehicleId.GetValueOrDefault(vehicleId);

        var items = dataSet.MaintenancePlans
            .Where(item => item.VehicleId == vehicleId)
            .OrderByDescending(item => item.IsActive)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new VehicleMaintenanceItemViewModel(
                item.Id,
                FormatValue(item.Title, "Bez názvu"),
                BuildMaintenanceInterval(item),
                BuildMaintenanceLastService(item),
                BuildMaintenanceStatus(item, today, currentOdometer),
                FormatValue(item.Note, "bez poznámky")))
            .ToList();

        var summary = items.Count == 0
            ? "Vybrané vozidlo zatím nemá žádné servisní plány."
            : $"Vybrané vozidlo má {items.Count} servisních plánů.";

        return new DesktopListProjection<VehicleMaintenanceItemViewModel>(items, summary);
    }

    public DesktopListProjection<VehicleTimelineItemViewModel> BuildTimeline(
        VehimapDataSet dataSet,
        ITimelineService timelineService,
        string vehicleId,
        DateOnly today,
        string selectedFilter,
        string? searchText)
    {
        var allItems = timelineService.BuildVehicleTimeline(dataSet, vehicleId, today).ToList();
        var filteredItems = allItems
            .Where(item => MatchesTimelineFilter(item, selectedFilter))
            .Where(item => MatchesTimelineSearch(item, searchText))
            .Select(item => new VehicleTimelineItemViewModel(
                item.Kind,
                item.KindLabel,
                item.DateText,
                item.Title,
                item.Detail,
                item.Status,
                item.VehicleName,
                item.VehicleId,
                item.EntryId,
                item.IsFuture,
                item.Note))
            .ToList();

        var futureCount = allItems.Count(item => item.IsFuture);
        var pastCount = allItems.Count - futureCount;
        var summary = allItems.Count == 0
            ? "Pro toto vozidlo zatím nejsou žádné časové položky s datem."
            : filteredItems.Count == allItems.Count
                ? $"Celkem položek: {allItems.Count}. Budoucí: {futureCount}. Minulé: {pastCount}."
                : $"Celkem položek: {allItems.Count}. Budoucí: {futureCount}. Minulé: {pastCount}. Po filtru zobrazeno: {filteredItems.Count}.";

        return new DesktopListProjection<VehicleTimelineItemViewModel>(filteredItems, summary);
    }

    public DesktopListProjection<VehicleRecordItemViewModel> BuildRecords(
        VehimapDataRoot? dataRoot,
        VehimapDataSet dataSet,
        string vehicleId,
        Func<string, string> managedPathResolver)
    {
        var items = dataSet.Records
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => BuildVehicleRecordItem(dataRoot, item, managedPathResolver))
            .OrderBy(item => item.Validity, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var summary = items.Count == 0
            ? "Vybrané vozidlo zatím nemá žádné doklady."
            : $"Vybrané vozidlo má {items.Count} dokladů. Vyberte záznam a můžete otevřít soubor nebo jeho složku.";

        return new DesktopListProjection<VehicleRecordItemViewModel>(items, summary);
    }

    public DesktopListProjection<VehicleTimelineItemViewModel> BuildDashboardTimeline(
        VehimapDataSet dataSet,
        ITimelineService timelineService,
        DateOnly today)
    {
        var items = dataSet.Vehicles
            .SelectMany(vehicle => timelineService.BuildVehicleTimeline(dataSet, vehicle.Id, today))
            .Where(item => item.IsFuture)
            .OrderBy(item => item.Date)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.KindLabel, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Take(10)
            .Select(item => new VehicleTimelineItemViewModel(
                item.Kind,
                item.KindLabel,
                item.DateText,
                item.Title,
                item.Detail,
                item.Status,
                item.VehicleName,
                item.VehicleId,
                item.EntryId,
                item.IsFuture,
                item.Note))
            .ToList();

        var summary = items.Count == 0
            ? "V dostupných legacy datech zatím nejsou žádné budoucí termíny s konkrétním datem."
            : $"Napříč všemi vozidly je nejbližších {items.Count} budoucích termínů připravených k otevření.";

        return new DesktopListProjection<VehicleTimelineItemViewModel>(items, summary);
    }

    public string BuildAuditSummary(IReadOnlyCollection<AuditItem> audit)
    {
        if (audit.Count == 0)
        {
            return "Audit zatím nenašel žádné problémy, které by potřebovaly zásah.";
        }

        var errorCount = audit.Count(item => item.Severity == AuditSeverity.Error);
        var warningCount = audit.Count(item => item.Severity == AuditSeverity.Warning);
        return $"K řešení je {audit.Count} položek: {errorCount} chyb a {warningCount} upozornění.";
    }

    public string BuildCostSummary(CostAnalysisSummary summary)
    {
        var costPerKmText = summary.CostPerKm.HasValue ? $"{summary.CostPerKm.Value:0.00} Kč/km" : "nedostupné";
        var distanceText = summary.DistanceKm.HasValue ? $"{summary.DistanceKm.Value} km" : "nedostupné";
        return $"{summary.PeriodLabel}\nCelkem: {FormatMoney(summary.TotalCost)} | Ujeto: {distanceText} | Cena / km: {costPerKmText}\nBez číselného nákladu: {summary.ActiveWithoutCostCount} z {summary.ActiveVehicleCount} aktivních vozidel.";
    }

    public string BuildCostComparison(CostAnalysisSummary summary)
    {
        var totalDelta = summary.TotalCostDifference >= 0m
            ? $"+{summary.TotalCostDifference:0.00} Kč"
            : $"{summary.TotalCostDifference:0.00} Kč";

        var costDelta = summary.CostPerKmDifference.HasValue
            ? (summary.CostPerKmDifference.Value >= 0m
                ? $"+{summary.CostPerKmDifference.Value:0.00} Kč/km"
                : $"{summary.CostPerKmDifference.Value:0.00} Kč/km")
            : "nedostupné";

        return $"Proti stejně dlouhému období loni: náklady {totalDelta}, cena / km {costDelta}. U {summary.CostPerKmUnavailableCount} vozidel s náklady zatím chybí spolehlivý výpočet ceny za kilometr.";
    }

    public string? GetRecordFolderPath(VehicleRecordItemViewModel? record)
    {
        if (record is null)
        {
            return null;
        }

        if (record.FileExists)
        {
            return Path.GetDirectoryName(record.ResolvedPath);
        }

        if (!string.IsNullOrWhiteSpace(record.ResolvedPath))
        {
            return Path.GetDirectoryName(record.ResolvedPath);
        }

        return null;
    }

    private VehicleRecordItemViewModel BuildVehicleRecordItem(
        VehimapDataRoot? dataRoot,
        VehicleRecord record,
        Func<string, string> managedPathResolver)
    {
        var resolvedPath = ResolveRecordPath(dataRoot, record, managedPathResolver);
        var fileExists = !string.IsNullOrWhiteSpace(resolvedPath) && File.Exists(resolvedPath);

        return new VehicleRecordItemViewModel(
            record.Id,
            FormatValue(record.RecordType, "Doklad"),
            FormatValue(record.Title, "Bez názvu"),
            FormatValue(record.Provider, "Bez poskytovatele"),
            BuildRecordValidity(record),
            FormatCostValue(record.Price),
            record.AttachmentMode == VehicleRecordAttachmentMode.Managed ? "Spravovaná kopie" : "Externí cesta",
            BuildAttachmentState(record, resolvedPath, fileExists),
            record.FilePath,
            resolvedPath,
            fileExists,
            record.Note);
    }

    private static string ResolveRecordPath(
        VehimapDataRoot? dataRoot,
        VehicleRecord record,
        Func<string, string> managedPathResolver)
    {
        if (dataRoot is null || string.IsNullOrWhiteSpace(record.FilePath))
        {
            return string.Empty;
        }

        if (record.AttachmentMode == VehicleRecordAttachmentMode.Managed)
        {
            return managedPathResolver(record.FilePath);
        }

        return Path.IsPathRooted(record.FilePath)
            ? record.FilePath
            : Path.GetFullPath(Path.Combine(dataRoot.AppBasePath, record.FilePath));
    }

    private static string BuildRecordValidity(VehicleRecord record)
    {
        var from = string.IsNullOrWhiteSpace(record.ValidFrom) ? "od nevyplněno" : $"od {record.ValidFrom}";
        var to = string.IsNullOrWhiteSpace(record.ValidTo) ? "do nevyplněno" : $"do {record.ValidTo}";
        return $"{from} | {to}";
    }

    private static string BuildAttachmentState(VehicleRecord record, string resolvedPath, bool fileExists)
    {
        if (string.IsNullOrWhiteSpace(record.FilePath))
        {
            return "Bez cesty";
        }

        if (fileExists)
        {
            return "Soubor dostupný";
        }

        return record.AttachmentMode == VehicleRecordAttachmentMode.Managed
            ? "Chybí spravovaná příloha"
            : string.IsNullOrWhiteSpace(resolvedPath) ? "Cesta nevyřešena" : "Chybí externí příloha";
    }

    private static Dictionary<string, int?> BuildCurrentOdometerLookup(VehimapDataSet dataSet)
    {
        var result = new Dictionary<string, int?>(StringComparer.Ordinal);

        foreach (var vehicle in dataSet.Vehicles)
        {
            result[vehicle.Id] = null;
        }

        foreach (var item in dataSet.HistoryEntries)
        {
            if (!VehimapValueParser.TryParseOdometer(item.Odometer, out var odometer))
            {
                continue;
            }

            var current = result.GetValueOrDefault(item.VehicleId);
            if (!current.HasValue || odometer > current.Value)
            {
                result[item.VehicleId] = odometer;
            }
        }

        foreach (var item in dataSet.FuelEntries)
        {
            if (!VehimapValueParser.TryParseOdometer(item.Odometer, out var odometer))
            {
                continue;
            }

            var current = result.GetValueOrDefault(item.VehicleId);
            if (!current.HasValue || odometer > current.Value)
            {
                result[item.VehicleId] = odometer;
            }
        }

        return result;
    }

    private static string BuildReminderStatus(VehicleReminder reminder, DateOnly today)
    {
        if (!TryParseReminderDate(reminder.DueDate, out var dueDate))
        {
            return "Bez použitelného termínu";
        }

        var delta = dueDate.DayNumber - today.DayNumber;
        if (delta < 0)
        {
            return $"Po termínu o {Math.Abs(delta)} dnů";
        }

        if (delta == 0)
        {
            return "Dnes";
        }

        return delta == 1 ? "Zítra" : $"Za {delta} dnů";
    }

    private static bool TryParseReminderDate(string? text, out DateOnly value)
    {
        return VehimapValueParser.TryParseEventDate(text, out value)
            || VehimapValueParser.TryParseMonthYear(text, out value);
    }

    private static string BuildMaintenanceInterval(MaintenancePlan plan)
    {
        var parts = new List<string>();
        if (TryParsePositiveInteger(plan.IntervalKm, out var intervalKm))
        {
            parts.Add($"{intervalKm} km");
        }

        if (TryParsePositiveInteger(plan.IntervalMonths, out var intervalMonths))
        {
            parts.Add(intervalMonths == 1 ? "1 měsíc" : $"{intervalMonths} měsíců");
        }

        return parts.Count == 0 ? "Bez intervalu" : string.Join(" / ", parts);
    }

    private static string BuildMaintenanceLastService(MaintenancePlan plan)
    {
        var date = string.IsNullOrWhiteSpace(plan.LastServiceDate) ? "bez data" : plan.LastServiceDate;
        return $"{date} | {FormatOdometerValue(plan.LastServiceOdometer)}";
    }

    private static string BuildMaintenanceStatus(MaintenancePlan plan, DateOnly today, int? currentOdometer)
    {
        if (!plan.IsActive)
        {
            return "Neaktivní";
        }

        var parts = new List<string>();

        if (TryParsePositiveInteger(plan.IntervalMonths, out var intervalMonths))
        {
            if (TryParseReminderDate(plan.LastServiceDate, out var lastServiceDate))
            {
                var nextDate = lastServiceDate.AddMonths(intervalMonths);
                var delta = nextDate.DayNumber - today.DayNumber;
                if (delta < 0)
                {
                    parts.Add($"Po termínu o {Math.Abs(delta)} dnů");
                }
                else if (delta == 0)
                {
                    parts.Add("Servis dnes");
                }
                else
                {
                    parts.Add(delta == 1 ? "Za 1 den" : $"Za {delta} dnů");
                }
            }
            else
            {
                parts.Add("Chybí datum posledního servisu");
            }
        }

        if (TryParsePositiveInteger(plan.IntervalKm, out var intervalKm))
        {
            if (VehimapValueParser.TryParseOdometer(plan.LastServiceOdometer, out var lastServiceOdometer) && currentOdometer.HasValue)
            {
                var remainingKm = (lastServiceOdometer + intervalKm) - currentOdometer.Value;
                if (remainingKm < 0)
                {
                    parts.Add($"Po limitu o {Math.Abs(remainingKm)} km");
                }
                else if (remainingKm == 0)
                {
                    parts.Add("Servis nyní");
                }
                else
                {
                    parts.Add($"Za {remainingKm} km");
                }
            }
            else
            {
                parts.Add("Chybí tachometr pro výpočet");
            }
        }

        return parts.Count == 0 ? "Bez aktivního intervalu" : string.Join(" | ", parts);
    }

    private static bool TryParsePositiveInteger(string? text, out int value)
    {
        value = 0;
        return int.TryParse((text ?? string.Empty).Trim(), out value) && value > 0;
    }

    private static string FormatReminderRepeatMode(string? repeatMode) =>
        string.IsNullOrWhiteSpace(repeatMode) ? "bez opakování" : repeatMode;

    private static bool MatchesTimelineFilter(VehicleTimelineItem item, string selectedFilter)
    {
        return selectedFilter switch
        {
            "Budoucí" => item.IsFuture,
            "Minulé" => !item.IsFuture,
            _ => true
        };
    }

    private static bool MatchesTimelineSearch(VehicleTimelineItem item, string? searchText)
    {
        var needle = searchText?.Trim();
        if (string.IsNullOrWhiteSpace(needle))
        {
            return true;
        }

        var haystack = string.Join(' ', new[]
        {
            item.DateText,
            item.KindLabel,
            item.Title,
            item.Detail,
            item.Status,
            item.Note,
            item.VehicleName
        });

        return haystack.Contains(needle, StringComparison.CurrentCultureIgnoreCase);
    }

    private static string BuildVehicleStatusSummary(Vehicle vehicle, VehicleMeta? meta, IReadOnlyCollection<AuditItem> audit)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(meta?.State))
        {
            parts.Add(meta.State);
        }

        var attentionCount = audit.Count(item => item.VehicleId == vehicle.Id);
        if (attentionCount > 0)
        {
            parts.Add($"{attentionCount} položek k řešení");
        }

        return parts.Count == 0 ? "V pořádku" : string.Join(" | ", parts);
    }

    private static string FormatCostValue(string? value)
    {
        if (VehimapValueParser.TryParseMoney(value, out var parsed))
        {
            return FormatMoney(parsed);
        }

        return FormatValue(value, "bez ceny");
    }

    private static string FormatFuelLiters(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "bez množství";
        }

        return value.Contains('l', StringComparison.OrdinalIgnoreCase)
            ? value
            : $"{value} l";
    }

    private static string FormatOdometerValue(string? value)
    {
        if (!VehimapValueParser.TryParseOdometer(value, out var parsed))
        {
            return FormatValue(value, "bez tachometru");
        }

        return $"{parsed} km";
    }

    private static string FormatMoney(decimal value) => $"{value:0.00} Kč";

    private static string FormatValue(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}

internal sealed record DesktopVehicleDetailProjection(
    string Heading,
    string Overview,
    string Dates,
    string Profile);

internal sealed record DesktopListProjection<TItem>(
    IReadOnlyList<TItem> Items,
    string Summary);
