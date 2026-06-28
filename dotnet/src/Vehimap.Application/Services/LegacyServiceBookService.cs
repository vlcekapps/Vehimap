using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacyServiceBookService : IServiceBookService
{
    private static readonly string[] ServiceRecordKeywords =
    [
        "servis",
        "údržba",
        "udrzba",
        "oprava",
        "faktura",
        "účtenka",
        "uctenka",
        "doklad o servisu"
    ];

    public ServiceBookSummary BuildVehicleServiceBook(VehimapDataSet dataSet, string vehicleId, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(dataSet);

        var vehicle = dataSet.Vehicles.FirstOrDefault(item => string.Equals(item.Id, vehicleId, StringComparison.Ordinal));
        var currentOdometer = BuildCurrentOdometer(dataSet, vehicleId);
        var history = BuildHistoryEntries(dataSet, vehicleId);
        var maintenance = BuildMaintenanceEntries(dataSet, vehicleId, today, currentOdometer);
        var records = BuildRecordEntries(dataSet, vehicleId);
        var totalHistoryCost = history
            .Where(item => item.ParsedCost.HasValue)
            .Sum(item => item.ParsedCost!.Value);

        var vehicleName = FormatValue(vehicle?.Name, "Neznámé vozidlo");
        var status = history.Count == 0 && maintenance.Count == 0 && records.Count == 0
            ? "Servisní knížka zatím nemá žádné položky. Historii, údržbu nebo servisní doklady doplníte v běžných evidencích vozidla."
            : $"Záznamy historie: {history.Count}. Servisní plány: {maintenance.Count}. Servisní doklady: {records.Count}.";

        return new ServiceBookSummary(
            vehicleId,
            vehicleName,
            FormatValue(vehicle?.Category, "bez kategorie"),
            FormatValue(vehicle?.MakeModel, "bez značky / modelu"),
            FormatValue(vehicle?.Plate, "bez SPZ"),
            currentOdometer.HasValue ? $"{currentOdometer.Value} km" : "neznámý",
            totalHistoryCost,
            status,
            history,
            maintenance,
            records);
    }

    private static IReadOnlyList<ServiceBookHistoryEntry> BuildHistoryEntries(VehimapDataSet dataSet, string vehicleId)
    {
        return dataSet.HistoryEntries
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal))
            .Select(item =>
            {
                var hasDate = VehimapValueParser.TryParseEventDate(item.EventDate, out var parsedDate);
                var hasCost = VehimapValueParser.TryParseMoney(item.Cost, out var parsedCost);
                return new
                {
                    Entry = item,
                    Date = hasDate ? parsedDate : (DateOnly?)null,
                    Cost = hasCost ? parsedCost : (decimal?)null
                };
            })
            .OrderByDescending(item => item.Date.HasValue)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Entry.EventType, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new ServiceBookHistoryEntry(
                item.Entry.Id,
                FormatValue(item.Entry.EventDate, "bez data"),
                item.Date,
                FormatValue(item.Entry.EventType, "Historie"),
                FormatOdometer(item.Entry.Odometer),
                FormatMoneyText(item.Entry.Cost),
                item.Cost,
                FormatValue(item.Entry.Note, "bez poznámky")))
            .ToList();
    }

    private static IReadOnlyList<ServiceBookMaintenanceEntry> BuildMaintenanceEntries(
        VehimapDataSet dataSet,
        string vehicleId,
        DateOnly today,
        int? currentOdometer)
    {
        return dataSet.MaintenancePlans
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal))
            .OrderByDescending(item => item.IsActive)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new ServiceBookMaintenanceEntry(
                item.Id,
                FormatValue(item.Title, "Bez názvu"),
                BuildMaintenanceInterval(item),
                BuildMaintenanceLastService(item),
                BuildMaintenanceStatus(item, today, currentOdometer),
                item.IsActive,
                FormatValue(item.Note, "bez poznámky")))
            .ToList();
    }

    private static IReadOnlyList<ServiceBookRecordEntry> BuildRecordEntries(VehimapDataSet dataSet, string vehicleId)
    {
        return dataSet.Records
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal))
            .Where(IsServiceRecord)
            .Select(item => new
            {
                Entry = item,
                HasDate = VehimapValueParser.TryResolveRecordDate(item, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Entry.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new ServiceBookRecordEntry(
                item.Entry.Id,
                FormatValue(item.Entry.RecordType, "Doklad"),
                FormatValue(item.Entry.Title, "Bez názvu"),
                FormatValue(item.Entry.Provider, "bez poskytovatele"),
                BuildRecordValidity(item.Entry),
                FormatMoneyText(item.Entry.Price),
                item.Entry.AttachmentMode == VehicleRecordAttachmentMode.Managed ? "Spravovaná kopie" : "Externí cesta",
                FormatValue(item.Entry.FilePath, "bez uložené cesty"),
                FormatValue(item.Entry.Note, "bez poznámky")))
            .ToList();
    }

    private static int? BuildCurrentOdometer(VehimapDataSet dataSet, string vehicleId)
    {
        var samples = new List<int>();
        samples.AddRange(dataSet.HistoryEntries
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal))
            .Select(item => VehimapValueParser.TryParseOdometer(item.Odometer, out var odometer) ? odometer : (int?)null)
            .Where(item => item.HasValue)
            .Select(item => item!.Value));
        samples.AddRange(dataSet.FuelEntries
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal))
            .Select(item => VehimapValueParser.TryParseOdometer(item.Odometer, out var odometer) ? odometer : (int?)null)
            .Where(item => item.HasValue)
            .Select(item => item!.Value));

        return samples.Count == 0 ? null : samples.Max();
    }

    private static bool IsServiceRecord(VehicleRecord record)
    {
        var haystack = $"{record.RecordType} {record.Title}";
        return ServiceRecordKeywords.Any(keyword =>
            haystack.Contains(keyword, StringComparison.CurrentCultureIgnoreCase));
    }

    private static string BuildRecordValidity(VehicleRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.ValidFrom) && string.IsNullOrWhiteSpace(record.ValidTo))
        {
            return "bez platnosti";
        }

        if (string.IsNullOrWhiteSpace(record.ValidFrom))
        {
            return $"do {record.ValidTo}";
        }

        if (string.IsNullOrWhiteSpace(record.ValidTo))
        {
            return $"od {record.ValidFrom}";
        }

        return $"{record.ValidFrom} až {record.ValidTo}";
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

        return parts.Count == 0 ? "bez intervalu" : string.Join(" / ", parts);
    }

    private static string BuildMaintenanceLastService(MaintenancePlan plan)
    {
        var date = FormatValue(plan.LastServiceDate, "bez data");
        return $"{date} | {FormatOdometer(plan.LastServiceOdometer)}";
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
            if (VehimapValueParser.TryParseEventDate(plan.LastServiceDate, out var lastServiceDate))
            {
                var nextDate = lastServiceDate.AddMonths(intervalMonths);
                var delta = nextDate.DayNumber - today.DayNumber;
                parts.Add(delta switch
                {
                    < 0 => $"Po termínu o {Math.Abs(delta)} dnů",
                    0 => "Servis dnes",
                    1 => "Za 1 den",
                    _ => $"Za {delta} dnů"
                });
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
                parts.Add(remainingKm switch
                {
                    < 0 => $"Po limitu o {Math.Abs(remainingKm)} km",
                    0 => "Servis nyní",
                    _ => $"Za {remainingKm} km"
                });
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

    private static string FormatOdometer(string? value)
    {
        if (VehimapValueParser.TryParseOdometer(value, out var parsed))
        {
            return $"{parsed} km";
        }

        return FormatValue(value, "bez tachometru");
    }

    private static string FormatMoneyText(string? value)
    {
        if (VehimapValueParser.TryParseMoney(value, out var parsed))
        {
            return $"{parsed:0.00} Kč";
        }

        return FormatValue(value, "bez ceny");
    }

    private static string FormatValue(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
