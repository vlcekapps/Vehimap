using System.Globalization;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacyTimelineService : ITimelineService
{
    private readonly IAppLocalizer _localizer;

    public LegacyTimelineService()
        : this(CreateDefaultLocalizer())
    {
    }

    public LegacyTimelineService(IAppLocalizer localizer)
    {
        _localizer = localizer;
    }

    public IReadOnlyList<VehicleTimelineItem> BuildVehicleTimeline(VehimapDataSet dataSet, string vehicleId, DateOnly today)
    {
        var vehicle = dataSet.Vehicles.FirstOrDefault(item => item.Id == vehicleId);
        if (vehicle is null)
        {
            return [];
        }

        var items = new List<VehicleTimelineItem>();
        var currentOdometerLookup = BuildCurrentOdometerLookup(dataSet);

        foreach (var entry in dataSet.HistoryEntries.Where(item => item.VehicleId == vehicleId))
        {
            if (!VehimapValueParser.TryParseEventDate(entry.EventDate, out var date))
            {
                continue;
            }

            items.Add(new VehicleTimelineItem(
                "history",
                L("Timeline.Kind.History"),
                vehicle.Id,
                vehicle.Name,
                vehicle.Plate,
                vehicle.MakeModel,
                date,
                FormatEventDate(date),
                ValueOrFallback(entry.EventType, L("Timeline.Kind.History")),
                JoinParts(FormatOdometerText(entry.Odometer), entry.Note),
                FormatCostStatus(entry.Cost),
                entry.Id,
                entry.Note,
                date >= today));
        }

        foreach (var entry in dataSet.FuelEntries.Where(item => item.VehicleId == vehicleId))
        {
            if (!VehimapValueParser.TryParseEventDate(entry.EntryDate, out var date))
            {
                continue;
            }

            items.Add(new VehicleTimelineItem(
                "fuel",
                L("Timeline.Kind.Fuel"),
                vehicle.Id,
                vehicle.Name,
                vehicle.Plate,
                vehicle.MakeModel,
                date,
                FormatEventDate(date),
                L("Timeline.Title.Fuel"),
                JoinParts(FormatOdometerText(entry.Odometer), FormatFuelLiters(entry.Liters), entry.FuelType, entry.FuelDetail, entry.Station, entry.Note),
                FormatCostStatus(entry.TotalCost),
                entry.Id,
                entry.Note,
                date >= today));
        }

        if (TryParseDueDate(vehicle.NextTk, out var technicalDate))
        {
            items.Add(new VehicleTimelineItem(
                "technical",
                L("Timeline.Kind.TechnicalInspection"),
                vehicle.Id,
                vehicle.Name,
                vehicle.Plate,
                vehicle.MakeModel,
                technicalDate,
                vehicle.NextTk,
                L("Timeline.Title.NextTechnicalInspection"),
                BuildVehicleDetail(vehicle),
                BuildExpirationStatusText(technicalDate, today, GetReminderDays(dataSet.Settings, "technical_reminder_days", 31)),
                string.Empty,
                string.Empty,
                technicalDate >= today));
        }

        if (TryParseDueDate(vehicle.GreenCardTo, out var greenDate))
        {
            items.Add(new VehicleTimelineItem(
                "green",
                L("Timeline.Kind.GreenCard"),
                vehicle.Id,
                vehicle.Name,
                vehicle.Plate,
                vehicle.MakeModel,
                greenDate,
                vehicle.GreenCardTo,
                L("Timeline.Title.GreenCardEnd"),
                BuildVehicleDetail(vehicle),
                BuildExpirationStatusText(greenDate, today, GetReminderDays(dataSet.Settings, "green_card_reminder_days", 31)),
                string.Empty,
                string.Empty,
                greenDate >= today));
        }

        foreach (var reminder in dataSet.Reminders.Where(item => item.VehicleId == vehicleId))
        {
            if (!VehimapValueParser.TryParseEventDate(reminder.DueDate, out var dueDate))
            {
                continue;
            }

            items.Add(new VehicleTimelineItem(
                "custom",
                L("Timeline.Kind.Reminder"),
                vehicle.Id,
                vehicle.Name,
                vehicle.Plate,
                vehicle.MakeModel,
                dueDate,
                FormatEventDate(dueDate),
                ValueOrFallback(reminder.Title, L("Timeline.Kind.Reminder")),
                JoinParts(FormatReminderRepeatMode(reminder.RepeatMode), reminder.Note),
                BuildExpirationStatusText(dueDate, today, GetReminderDaysFromReminder(reminder)),
                reminder.Id,
                reminder.Note,
                dueDate >= today));
        }

        foreach (var record in dataSet.Records.Where(item => item.VehicleId == vehicleId))
        {
            if (!TryParseDueDate(record.ValidTo, out var dueDate))
            {
                continue;
            }

            items.Add(new VehicleTimelineItem(
                "record",
                L("Timeline.Kind.Record"),
                vehicle.Id,
                vehicle.Name,
                vehicle.Plate,
                vehicle.MakeModel,
                dueDate,
                record.ValidTo,
                LF("Timeline.Title.Record", ValueOrFallback(record.RecordType, L("Timeline.Kind.Record")), ValueOrFallback(record.Title, L("Timeline.Value.Untitled"))),
                JoinParts(record.Provider, record.Note),
                BuildExpirationStatusText(dueDate, today, 30),
                record.Id,
                record.Note,
                dueDate >= today));
        }

        foreach (var plan in dataSet.MaintenancePlans.Where(item => item.VehicleId == vehicleId && item.IsActive))
        {
            if (!TryBuildMaintenanceScheduleLocalized(plan, currentOdometerLookup.GetValueOrDefault(vehicleId), today, GetMaintenanceReminderDays(dataSet.Settings), GetMaintenanceReminderKm(dataSet.Settings), out var dueDate, out var nextServiceText, out var statusText))
            {
                continue;
            }

            items.Add(new VehicleTimelineItem(
                "maintenance",
                L("Timeline.Kind.Maintenance"),
                vehicle.Id,
                vehicle.Name,
                vehicle.Plate,
                vehicle.MakeModel,
                dueDate,
                FormatEventDate(dueDate),
                ValueOrFallback(plan.Title, L("Timeline.Value.ServiceTask")),
                nextServiceText,
                statusText,
                plan.Id,
                plan.Note,
                dueDate >= today));
        }

        return items
            .OrderBy(item => item.IsFuture ? 0 : 1)
            .ThenBy(item => item.IsFuture ? item.Date.DayNumber : -item.Date.DayNumber)
            .ThenBy(item => item.KindLabel, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    internal static bool TryBuildMaintenanceSchedule(
        MaintenancePlan plan,
        int? currentOdometer,
        DateOnly today,
        int reminderDays,
        int reminderKm,
        out DateOnly dueDate,
        out string nextServiceText,
        out string statusText) =>
        TryBuildMaintenanceScheduleCore(
            plan,
            currentOdometer,
            today,
            reminderDays,
            reminderKm,
            CreateDefaultLocalizer(),
            out dueDate,
            out nextServiceText,
            out statusText);

    private bool TryBuildMaintenanceScheduleLocalized(
        MaintenancePlan plan,
        int? currentOdometer,
        DateOnly today,
        int reminderDays,
        int reminderKm,
        out DateOnly dueDate,
        out string nextServiceText,
        out string statusText) =>
        TryBuildMaintenanceScheduleCore(
            plan,
            currentOdometer,
            today,
            reminderDays,
            reminderKm,
            _localizer,
            out dueDate,
            out nextServiceText,
            out statusText);

    private static bool TryBuildMaintenanceScheduleCore(
        MaintenancePlan plan,
        int? currentOdometer,
        DateOnly today,
        int reminderDays,
        int reminderKm,
        IAppLocalizer localizer,
        out DateOnly dueDate,
        out string nextServiceText,
        out string statusText)
    {
        dueDate = default;
        nextServiceText = string.Empty;
        statusText = string.Empty;

        if (!plan.IsActive || !TryParsePositiveInteger(plan.IntervalMonths, out var intervalMonths) || !VehimapValueParser.TryParseEventDate(plan.LastServiceDate, out var lastServiceDate))
        {
            return false;
        }

        dueDate = lastServiceDate.AddMonths(intervalMonths);

        string? nextOdometerText = null;
        string? odometerStatus = null;
        if (TryParsePositiveInteger(plan.IntervalKm, out var intervalKm))
        {
            if (VehimapValueParser.TryParseOdometer(plan.LastServiceOdometer, out var lastServiceOdometer))
            {
                var nextOdometer = lastServiceOdometer + intervalKm;
                nextOdometerText = LF(localizer, "Timeline.Value.OdometerKm", nextOdometer);
                if (currentOdometer.HasValue)
                {
                    var remainingKm = nextOdometer - currentOdometer.Value;
                    if (remainingKm < 0)
                    {
                        odometerStatus = LF(localizer, "Timeline.Status.OverDistanceLimitKm", Math.Abs(remainingKm));
                    }
                    else if (remainingKm <= reminderKm)
                    {
                        odometerStatus = LF(localizer, "Timeline.Status.WithinDistanceKm", remainingKm);
                    }
                }
                else
                {
                    odometerStatus = L(localizer, "Timeline.Status.MissingCurrentOdometer");
                }
            }
            else
            {
                odometerStatus = L(localizer, "Timeline.Status.MissingOdometerForCalculation");
            }
        }

        nextServiceText = nextOdometerText is null
            ? FormatEventDate(dueDate)
            : LF(localizer, "Timeline.Detail.DateAndOdometer", FormatEventDate(dueDate), nextOdometerText);

        var dateStatus = BuildExpirationStatusCore(dueDate, today, reminderDays, localizer);
        statusText = JoinParts(dateStatus, odometerStatus);
        if (string.IsNullOrWhiteSpace(statusText))
        {
            statusText = L(localizer, "Timeline.Status.NoAlert");
        }

        return true;
    }

    internal static int GetReminderDays(VehimapSettings settings, string key, int defaultValue)
    {
        var raw = settings.GetValue("notifications", key, defaultValue.ToString(CultureInfo.InvariantCulture));
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value >= 0 && value <= 3650
            ? value
            : defaultValue;
    }

    internal static int GetMaintenanceReminderDays(VehimapSettings settings) => GetReminderDays(settings, "maintenance_reminder_days", 31);

    internal static int GetMaintenanceReminderKm(VehimapSettings settings)
    {
        var raw = settings.GetValue("notifications", "maintenance_reminder_km", "1000");
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value is > 0 and <= 999999
            ? value
            : 1000;
    }

    internal static bool TryParseDueDate(string? text, out DateOnly date)
    {
        if (!VehimapValueParser.TryParseMonthYear(text, out date))
        {
            return false;
        }

        date = new DateOnly(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        return true;
    }

    internal static string BuildExpirationStatus(DateOnly dueDate, DateOnly today, int reminderDays) =>
        BuildExpirationStatusCore(dueDate, today, reminderDays, CreateDefaultLocalizer());

    private string BuildExpirationStatusText(DateOnly dueDate, DateOnly today, int reminderDays) =>
        BuildExpirationStatusCore(dueDate, today, reminderDays, _localizer);

    private static string BuildExpirationStatusCore(DateOnly dueDate, DateOnly today, int reminderDays, IAppLocalizer localizer)
    {
        if (dueDate < today)
        {
            return L(localizer, "Timeline.Status.Overdue");
        }

        var cutoff = today.AddDays(reminderDays);
        if (dueDate <= cutoff)
        {
            var daysLeft = dueDate.DayNumber - today.DayNumber;
            return daysLeft < 1
                ? L(localizer, "Timeline.Status.Today")
                : LF(localizer, "Timeline.Status.DaysLeft", daysLeft);
        }

        return string.Empty;
    }

    private static int GetReminderDaysFromReminder(VehicleReminder reminder)
    {
        return int.TryParse(reminder.ReminderDays, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value >= 0
            ? value
            : 30;
    }

    private static Dictionary<string, int?> BuildCurrentOdometerLookup(VehimapDataSet dataSet)
    {
        var result = new Dictionary<string, int?>(StringComparer.Ordinal);

        foreach (var vehicle in dataSet.Vehicles)
        {
            result[vehicle.Id] = null;
        }

        foreach (var entry in dataSet.HistoryEntries)
        {
            if (!VehimapValueParser.TryParseOdometer(entry.Odometer, out var odometer))
            {
                continue;
            }

            var current = result.GetValueOrDefault(entry.VehicleId);
            if (!current.HasValue || odometer > current.Value)
            {
                result[entry.VehicleId] = odometer;
            }
        }

        foreach (var entry in dataSet.FuelEntries)
        {
            if (!VehimapValueParser.TryParseOdometer(entry.Odometer, out var odometer))
            {
                continue;
            }

            var current = result.GetValueOrDefault(entry.VehicleId);
            if (!current.HasValue || odometer > current.Value)
            {
                result[entry.VehicleId] = odometer;
            }
        }

        return result;
    }

    private static bool TryParsePositiveInteger(string? text, out int value)
    {
        return int.TryParse((text ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && value > 0;
    }

    private static string BuildVehicleDetail(Vehicle vehicle) => JoinParts(vehicle.Plate, vehicle.MakeModel);

    private static string FormatEventDate(DateOnly date) => date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    private string FormatCostStatus(string? value)
    {
        if (VehimapValueParser.TryParseMoney(value, out var money))
        {
            return LF("Timeline.Value.Cost", money.ToString("0.00", CultureInfo.InvariantCulture));
        }

        return string.IsNullOrWhiteSpace(value) ? L("Timeline.Value.NoCost") : value;
    }

    private string FormatFuelLiters(string? liters)
    {
        if (string.IsNullOrWhiteSpace(liters))
        {
            return string.Empty;
        }

        return liters.Contains('l', StringComparison.OrdinalIgnoreCase) ? liters : LF("Timeline.Value.Liters", liters.Trim());
    }

    private string FormatOdometerText(string? odometer)
    {
        if (!VehimapValueParser.TryParseOdometer(odometer, out var parsed))
        {
            return string.Empty;
        }

        return LF("Timeline.Value.OdometerKm", parsed);
    }

    private string FormatReminderRepeatMode(string? repeatMode)
    {
        return string.IsNullOrWhiteSpace(repeatMode) ? L("Timeline.Value.NoRepeat") : repeatMode;
    }

    private static string JoinParts(params string?[] parts)
    {
        return string.Join(" | ", parts.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part!.Trim()));
    }

    private static string ValueOrFallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static IAppLocalizer CreateDefaultLocalizer() =>
        new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);

    private static string L(IAppLocalizer localizer, string key) => localizer.GetString(key);

    private static string LF(IAppLocalizer localizer, string key, params object?[] args) => localizer.Format(key, args);
}
