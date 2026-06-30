using System.Globalization;
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

    private readonly IAppLocalizer _localizer;

    public LegacyServiceBookService(IAppLocalizer? localizer = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));
    }

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

        var vehicleName = FormatValue(vehicle?.Name, L("ServiceBook.Value.UnknownVehicle"));
        var status = history.Count == 0 && maintenance.Count == 0 && records.Count == 0
            ? L("ServiceBook.Summary.Empty")
            : LF("ServiceBook.Summary.Counts", history.Count, maintenance.Count, records.Count);

        return new ServiceBookSummary(
            vehicleId,
            vehicleName,
            FormatValue(vehicle?.Category, L("ServiceBook.Value.NoCategory")),
            FormatValue(vehicle?.MakeModel, L("ServiceBook.Value.NoMakeModel")),
            FormatValue(vehicle?.Plate, L("ServiceBook.Value.NoPlate")),
            currentOdometer.HasValue ? LF("ServiceBook.Value.OdometerKm", currentOdometer.Value) : L("ServiceBook.Value.Unknown"),
            totalHistoryCost,
            status,
            history,
            maintenance,
            records);
    }

    private IReadOnlyList<ServiceBookHistoryEntry> BuildHistoryEntries(VehimapDataSet dataSet, string vehicleId)
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
                FormatValue(item.Entry.EventDate, L("ServiceBook.Value.NoDate")),
                item.Date,
                FormatValue(item.Entry.EventType, L("ServiceBook.Section.History")),
                FormatOdometer(item.Entry.Odometer),
                FormatMoneyText(item.Entry.Cost),
                item.Cost,
                FormatValue(item.Entry.Note, L("ServiceBook.Value.NoNote"))))
            .ToList();
    }

    private IReadOnlyList<ServiceBookMaintenanceEntry> BuildMaintenanceEntries(
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
                FormatValue(item.Title, L("ServiceBook.Value.Untitled")),
                BuildMaintenanceInterval(item),
                BuildMaintenanceLastService(item),
                BuildMaintenanceStatus(item, today, currentOdometer),
                item.IsActive,
                FormatValue(item.Note, L("ServiceBook.Value.NoNote"))))
            .ToList();
    }

    private IReadOnlyList<ServiceBookRecordEntry> BuildRecordEntries(VehimapDataSet dataSet, string vehicleId)
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
                FormatValue(item.Entry.RecordType, L("ServiceBook.Value.Record")),
                FormatValue(item.Entry.Title, L("ServiceBook.Value.Untitled")),
                FormatValue(item.Entry.Provider, L("ServiceBook.Value.NoProvider")),
                BuildRecordValidity(item.Entry),
                FormatMoneyText(item.Entry.Price),
                item.Entry.AttachmentMode == VehicleRecordAttachmentMode.Managed
                    ? L("ServiceBook.Value.ManagedAttachment")
                    : L("ServiceBook.Value.ExternalAttachment"),
                FormatValue(item.Entry.FilePath, L("ServiceBook.Value.NoStoredPath")),
                FormatValue(item.Entry.Note, L("ServiceBook.Value.NoNote"))))
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

    private string BuildRecordValidity(VehicleRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.ValidFrom) && string.IsNullOrWhiteSpace(record.ValidTo))
        {
            return L("ServiceBook.Value.NoValidity");
        }

        if (string.IsNullOrWhiteSpace(record.ValidFrom))
        {
            return LF("ServiceBook.Value.ValidTo", record.ValidTo);
        }

        if (string.IsNullOrWhiteSpace(record.ValidTo))
        {
            return LF("ServiceBook.Value.ValidFrom", record.ValidFrom);
        }

        return LF("ServiceBook.Value.ValidRange", record.ValidFrom, record.ValidTo);
    }

    private string BuildMaintenanceInterval(MaintenancePlan plan)
    {
        var parts = new List<string>();
        if (TryParsePositiveInteger(plan.IntervalKm, out var intervalKm))
        {
            parts.Add(LF("ServiceBook.Value.OdometerKm", intervalKm));
        }

        if (TryParsePositiveInteger(plan.IntervalMonths, out var intervalMonths))
        {
            parts.Add(intervalMonths == 1
                ? L("ServiceBook.Value.MonthSingular")
                : LF("ServiceBook.Value.MonthPlural", intervalMonths));
        }

        return parts.Count == 0 ? L("ServiceBook.Value.NoInterval") : string.Join(" / ", parts);
    }

    private string BuildMaintenanceLastService(MaintenancePlan plan)
    {
        var date = FormatValue(plan.LastServiceDate, L("ServiceBook.Value.NoDate"));
        return LF("ServiceBook.Value.LastService", date, FormatOdometer(plan.LastServiceOdometer));
    }

    private string BuildMaintenanceStatus(MaintenancePlan plan, DateOnly today, int? currentOdometer)
    {
        if (!plan.IsActive)
        {
            return L("ServiceBook.Value.Inactive");
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
                    < 0 => LF("ServiceBook.Value.OverdueDays", Math.Abs(delta)),
                    0 => L("ServiceBook.Value.ServiceToday"),
                    1 => L("ServiceBook.Value.InOneDay"),
                    _ => LF("ServiceBook.Value.InDays", delta)
                });
            }
            else
            {
                parts.Add(L("ServiceBook.Value.MissingLastServiceDate"));
            }
        }

        if (TryParsePositiveInteger(plan.IntervalKm, out var intervalKm))
        {
            if (VehimapValueParser.TryParseOdometer(plan.LastServiceOdometer, out var lastServiceOdometer) && currentOdometer.HasValue)
            {
                var remainingKm = (lastServiceOdometer + intervalKm) - currentOdometer.Value;
                parts.Add(remainingKm switch
                {
                    < 0 => LF("ServiceBook.Value.OverDistanceLimitKm", Math.Abs(remainingKm)),
                    0 => L("ServiceBook.Value.ServiceNow"),
                    _ => LF("ServiceBook.Value.InKm", remainingKm)
                });
            }
            else
            {
                parts.Add(L("ServiceBook.Value.MissingOdometerForCalculation"));
            }
        }

        return parts.Count == 0 ? L("ServiceBook.Value.NoActiveInterval") : string.Join(" | ", parts);
    }

    private static bool TryParsePositiveInteger(string? text, out int value)
    {
        value = 0;
        return int.TryParse((text ?? string.Empty).Trim(), out value) && value > 0;
    }

    private string FormatOdometer(string? value)
    {
        if (VehimapValueParser.TryParseOdometer(value, out var parsed))
        {
            return LF("ServiceBook.Value.OdometerKm", parsed);
        }

        return FormatValue(value, L("ServiceBook.Value.NoOdometer"));
    }

    private string FormatMoneyText(string? value)
    {
        if (VehimapValueParser.TryParseMoney(value, out var parsed))
        {
            return LF("ServiceBook.Value.Money", parsed.ToString("0.00", CultureInfo.InvariantCulture));
        }

        return FormatValue(value, L("ServiceBook.Value.NoPrice"));
    }

    private static string FormatValue(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);
}
