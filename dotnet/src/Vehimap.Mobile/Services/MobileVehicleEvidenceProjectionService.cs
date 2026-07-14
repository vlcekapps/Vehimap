// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Mobile.ViewModels;

namespace Vehimap.Mobile.Services;

internal sealed class MobileVehicleEvidenceProjectionService
{
    private readonly IAppNumberFormatService _numberFormatService;
    private readonly IAppUnitFormatService _unitFormatService;
    private readonly IAppDateFormatService _dateFormatService;
    private readonly IAppPluralizationService _pluralizationService;

    public MobileVehicleEvidenceProjectionService(
        IAppNumberFormatService? numberFormatService = null,
        IAppUnitFormatService? unitFormatService = null,
        IAppDateFormatService? dateFormatService = null,
        IAppPluralizationService? pluralizationService = null)
    {
        _numberFormatService = numberFormatService ?? new AppNumberFormatService();
        _unitFormatService = unitFormatService ?? new AppUnitFormatService(_numberFormatService);
        _dateFormatService = dateFormatService ?? new AppDateFormatService();
        _pluralizationService = pluralizationService ?? new AppPluralizationService();
    }

    public MobileVehicleEvidenceProjection BuildHistory(
        VehimapDataSet dataSet,
        string vehicleId,
        string vehicleName,
        DesktopSupportedSettingsSnapshot settings,
        IAppLocalizer localizer)
    {
        var culture = CreateCulturePreferences(settings);
        var units = CreateUnitPreferences(settings);
        var itemType = localizer.GetString("HistoryWorkspace.ItemType");
        var items = dataSet.HistoryEntries
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.OrdinalIgnoreCase))
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryParseEventDate(item.EventDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Item.EventType, StringComparer.CurrentCultureIgnoreCase)
            .Select(item =>
            {
                var date = item.HasDate
                    ? _dateFormatService.FormatDate(item.Date, culture)
                    : ValueOrFallback(item.Item.EventDate, localizer.GetString("Common.NoDate"));
                var eventType = ValueOrFallback(item.Item.EventType, localizer.GetString("Projection.Value.NoType"));
                var odometer = FormatOdometer(item.Item.Odometer, culture, units, localizer);
                var cost = FormatMoney(item.Item.Cost, culture, settings.Currency, localizer);
                var note = ValueOrFallback(item.Item.Note, localizer.GetString("Common.NoNote"));
                return new MobileVehicleEvidenceItemViewModel(
                    item.Item.Id,
                    "MobileHistoryItem",
                    eventType,
                    date,
                    string.Join(" | ", odometer, cost),
                    localizer.Format("HistoryItem.AccessibleLabel", date, eventType, odometer, cost, note),
                    itemType,
                    string.Empty,
                    BuildHistoryDetailLines(item.Item.Id, date, eventType, odometer, cost, note, localizer));
            })
            .ToArray();

        return new MobileVehicleEvidenceProjection(
            MobileVehicleEvidenceKind.History,
            localizer.Format("Mobile.History.Heading", vehicleName),
            FormatCount(localizer, culture, "History.Projection.Summary.Count", "History.Projection.Summary.Empty", items.Length),
            localizer.GetString("HistoryWorkspace.ListName"),
            localizer.GetString("HistoryWorkspace.ItemType"),
            localizer.GetString("Mobile.History.DetailHeading"),
            items);
    }

    public MobileVehicleEvidenceProjection BuildFuel(
        VehimapDataSet dataSet,
        string vehicleId,
        string vehicleName,
        DesktopSupportedSettingsSnapshot settings,
        IAppLocalizer localizer)
    {
        var culture = CreateCulturePreferences(settings);
        var units = CreateUnitPreferences(settings);
        var itemType = localizer.GetString("FuelWorkspace.ItemType");
        var items = dataSet.FuelEntries
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.OrdinalIgnoreCase))
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryParseEventDate(item.EntryDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Item.FuelType, StringComparer.CurrentCultureIgnoreCase)
            .Select(item =>
            {
                var date = item.HasDate
                    ? _dateFormatService.FormatDate(item.Date, culture)
                    : ValueOrFallback(item.Item.EntryDate, localizer.GetString("Common.NoDate"));
                var fuelType = ValueOrFallback(
                    LegacyKnownValueDisplayService.FormatFuelType(item.Item.FuelType, localizer),
                    localizer.GetString("Projection.Value.NoType"));
                var fuelDetail = ValueOrFallback(item.Item.FuelDetail, localizer.GetString("Projection.Value.NoFuelDetail"));
                var station = ValueOrFallback(item.Item.Station, localizer.GetString("Projection.Value.NoStation"));
                var volume = FormatVolume(item.Item.Liters, culture, units, localizer);
                var totalCost = FormatMoney(item.Item.TotalCost, culture, settings.Currency, localizer);
                var odometer = FormatOdometer(item.Item.Odometer, culture, units, localizer);
                var tankState = localizer.GetString(item.Item.FullTank
                    ? "Fuel.Projection.FullTank"
                    : "Fuel.Projection.PartialFuel");
                var note = ValueOrFallback(item.Item.Note, localizer.GetString("Common.NoNote"));
                return new MobileVehicleEvidenceItemViewModel(
                    item.Item.Id,
                    "MobileFuelItem",
                    fuelType,
                    date,
                    string.Join(" | ", volume, totalCost, tankState),
                    localizer.Format(
                        "FuelItem.AccessibleLabel",
                        date,
                        fuelType,
                        fuelDetail,
                        station,
                        volume,
                        totalCost,
                        odometer,
                        tankState,
                        note),
                    itemType,
                    tankState,
                    BuildFuelDetailLines(
                        item.Item.Id,
                        date,
                        fuelType,
                        fuelDetail,
                        station,
                        volume,
                        totalCost,
                        odometer,
                        tankState,
                        note,
                        localizer));
            })
            .ToArray();

        return new MobileVehicleEvidenceProjection(
            MobileVehicleEvidenceKind.Fuel,
            localizer.Format("Mobile.Fuel.Heading", vehicleName),
            FormatCount(localizer, culture, "Fuel.Projection.Summary.Count", "Fuel.Projection.Summary.Empty", items.Length),
            localizer.GetString("FuelWorkspace.ListName"),
            localizer.GetString("FuelWorkspace.ItemType"),
            localizer.GetString("Mobile.Fuel.DetailHeading"),
            items);
    }

    public MobileVehicleEvidenceProjection BuildRecords(
        VehimapDataRoot dataRoot,
        VehimapDataSet dataSet,
        string vehicleId,
        string vehicleName,
        DesktopSupportedSettingsSnapshot settings,
        IAppLocalizer localizer,
        Func<string, string> managedPathResolver)
    {
        var culture = CreateCulturePreferences(settings);
        var itemType = localizer.GetString("RecordWorkspace.ItemType");
        var items = dataSet.Records
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.OrdinalIgnoreCase))
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryResolveRecordDate(item, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenBy(item => item.Date)
            .ThenBy(item => item.Item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item =>
            {
                var title = ValueOrFallback(item.Item.Title, localizer.GetString("Projection.Value.NoTitle"));
                var recordType = ValueOrFallback(
                    LegacyKnownValueDisplayService.FormatRecordType(item.Item.RecordType, localizer),
                    localizer.GetString("Projection.Value.Document"));
                var provider = ValueOrFallback(item.Item.Provider, localizer.GetString("Projection.Value.NoProvider"));
                var validity = BuildRecordValidity(item.Item, culture, localizer);
                var price = FormatMoney(item.Item.Price, culture, settings.Currency, localizer);
                var attachmentMode = localizer.GetString(item.Item.AttachmentMode == VehicleRecordAttachmentMode.Managed
                    ? "Record.Projection.AttachmentMode.Managed"
                    : "Record.Projection.AttachmentMode.External");
                var resolvedPath = ResolveRecordPath(dataRoot, item.Item, managedPathResolver);
                var attachmentState = BuildAttachmentState(item.Item, resolvedPath, localizer);
                var note = ValueOrFallback(item.Item.Note, localizer.GetString("Common.NoNote"));
                var providerPart = string.IsNullOrWhiteSpace(item.Item.Provider)
                    ? string.Empty
                    : localizer.Format("RecordItem.ProviderPart", item.Item.Provider.Trim());
                var notePart = string.IsNullOrWhiteSpace(item.Item.Note)
                    ? string.Empty
                    : localizer.Format("RecordItem.NotePart", item.Item.Note.Trim());

                return new MobileVehicleEvidenceItemViewModel(
                    item.Item.Id,
                    "MobileRecordItem",
                    title,
                    string.Join(" | ", recordType, provider),
                    string.Join(" | ", validity, price, attachmentState),
                    localizer.Format(
                        "RecordItem.AccessibleLabel",
                        title,
                        recordType,
                        providerPart,
                        validity,
                        attachmentMode,
                        attachmentState,
                        notePart),
                    itemType,
                    attachmentState,
                    BuildRecordDetailLines(
                        item.Item.Id,
                        title,
                        recordType,
                        provider,
                        validity,
                        price,
                        attachmentMode,
                        attachmentState,
                        note,
                        localizer));
            })
            .ToArray();

        return new MobileVehicleEvidenceProjection(
            MobileVehicleEvidenceKind.Records,
            localizer.Format("Mobile.Records.Heading", vehicleName),
            FormatCount(localizer, culture, "Record.Projection.Summary.Count", "Record.Projection.Summary.Empty", items.Length),
            localizer.GetString("RecordWorkspace.ListName"),
            itemType,
            localizer.GetString("Mobile.Records.DetailHeading"),
            items);
    }

    public MobileVehicleEvidenceProjection BuildReminders(
        VehimapDataSet dataSet,
        string vehicleId,
        string vehicleName,
        DesktopSupportedSettingsSnapshot settings,
        IAppLocalizer localizer,
        DateOnly today)
    {
        var culture = CreateCulturePreferences(settings);
        var itemType = localizer.GetString("ReminderWorkspace.ItemType");
        var items = dataSet.Reminders
            .Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.OrdinalIgnoreCase))
            .Select(item => new
            {
                Item = item,
                HasDate = TryParseReminderDate(item.DueDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenBy(item => item.Date)
            .ThenBy(item => item.Item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item =>
            {
                var title = ValueOrFallback(item.Item.Title, localizer.GetString("Projection.Value.NoTitle"));
                var dueDate = item.HasDate
                    ? FormatReminderDueDate(item.Item.DueDate, item.Date, culture)
                    : ValueOrFallback(item.Item.DueDate, localizer.GetString("Projection.Value.NoDueDate"));
                var status = BuildReminderStatus(item.Item, today, culture, localizer);
                var repeatMode = ValueOrFallback(
                    LegacyKnownValueDisplayService.FormatReminderRepeatMode(item.Item.RepeatMode, localizer),
                    localizer.GetString("Common.EmptyValue"));
                var reminderDays = BuildReminderDays(item.Item.ReminderDays, culture, localizer);
                var note = ValueOrFallback(item.Item.Note, localizer.GetString("Common.NoNote"));

                return new MobileVehicleEvidenceItemViewModel(
                    item.Item.Id,
                    "MobileReminderItem",
                    title,
                    dueDate,
                    string.Join(" | ", status, repeatMode),
                    localizer.Format("ReminderItem.AccessibleLabel", title, dueDate, status, repeatMode, note),
                    itemType,
                    status,
                    BuildReminderDetailLines(
                        item.Item.Id,
                        title,
                        dueDate,
                        status,
                        repeatMode,
                        reminderDays,
                        note,
                        localizer));
            })
            .ToArray();

        return new MobileVehicleEvidenceProjection(
            MobileVehicleEvidenceKind.Reminders,
            localizer.Format("Mobile.Reminders.Heading", vehicleName),
            FormatCount(localizer, culture, "Reminder.Projection.Summary.Count", "Reminder.Projection.Summary.Empty", items.Length),
            localizer.GetString("ReminderWorkspace.ListName"),
            itemType,
            localizer.GetString("Mobile.Reminders.DetailHeading"),
            items);
    }

    private static IReadOnlyList<MobileEvidenceDetailLineViewModel> BuildHistoryDetailLines(
        string id,
        string date,
        string eventType,
        string odometer,
        string cost,
        string note,
        IAppLocalizer localizer) =>
        [
            DetailLine(id, "Date", localizer.Format("HistoryWorkspace.Detail.Date", date)),
            DetailLine(id, "EventType", localizer.Format("HistoryWorkspace.Detail.EventType", eventType)),
            DetailLine(id, "Odometer", localizer.Format("HistoryWorkspace.Detail.Odometer", odometer)),
            DetailLine(id, "Cost", localizer.Format("HistoryWorkspace.Detail.Cost", cost)),
            DetailLine(id, "Note", localizer.Format("HistoryWorkspace.Detail.Note", note))
        ];

    private static IReadOnlyList<MobileEvidenceDetailLineViewModel> BuildFuelDetailLines(
        string id,
        string date,
        string fuelType,
        string fuelDetail,
        string station,
        string volume,
        string totalCost,
        string odometer,
        string tankState,
        string note,
        IAppLocalizer localizer) =>
        [
            DetailLine(id, "Date", localizer.Format("FuelWorkspace.Detail.Date", date)),
            DetailLine(id, "Fuel", localizer.Format("FuelWorkspace.Detail.Fuel", fuelType)),
            DetailLine(id, "FuelDetail", localizer.Format("FuelWorkspace.Detail.FuelDetail", fuelDetail)),
            DetailLine(id, "Station", localizer.Format("FuelWorkspace.Detail.Station", station)),
            DetailLine(id, "Volume", localizer.Format("FuelWorkspace.Detail.Volume", volume)),
            DetailLine(id, "TotalCost", localizer.Format("FuelWorkspace.Detail.TotalCost", totalCost)),
            DetailLine(id, "Odometer", localizer.Format("FuelWorkspace.Detail.Odometer", odometer)),
            DetailLine(id, "TankState", localizer.Format("FuelWorkspace.Detail.TankState", tankState)),
            DetailLine(id, "Note", localizer.Format("FuelWorkspace.Detail.Note", note))
        ];

    private static IReadOnlyList<MobileEvidenceDetailLineViewModel> BuildRecordDetailLines(
        string id,
        string title,
        string recordType,
        string provider,
        string validity,
        string price,
        string attachmentMode,
        string attachmentState,
        string note,
        IAppLocalizer localizer) =>
        [
            DetailLine(id, "Title", localizer.Format("Mobile.Records.Detail.Title", title)),
            DetailLine(id, "Type", localizer.Format("RecordWorkspace.Detail.Type", recordType)),
            DetailLine(id, "Provider", localizer.Format("Mobile.Records.Detail.Provider", provider)),
            DetailLine(id, "Validity", localizer.Format("RecordWorkspace.Detail.Validity", validity)),
            DetailLine(id, "Price", localizer.Format("RecordWorkspace.Detail.Price", price)),
            DetailLine(id, "AttachmentMode", localizer.Format("RecordWorkspace.Detail.AttachmentMode", attachmentMode)),
            DetailLine(id, "AttachmentState", localizer.Format("RecordWorkspace.Detail.AttachmentState", attachmentState)),
            DetailLine(id, "Note", localizer.Format("RecordWorkspace.Detail.Note", note))
        ];

    private static IReadOnlyList<MobileEvidenceDetailLineViewModel> BuildReminderDetailLines(
        string id,
        string title,
        string dueDate,
        string status,
        string repeatMode,
        string reminderDays,
        string note,
        IAppLocalizer localizer) =>
        [
            DetailLine(id, "Title", localizer.Format("ReminderWorkspace.Detail.Title", title)),
            DetailLine(id, "DueDate", localizer.Format("ReminderWorkspace.Detail.DueDate", dueDate)),
            DetailLine(id, "Status", localizer.Format("ReminderWorkspace.Detail.Status", status)),
            DetailLine(id, "Repeat", localizer.Format("ReminderWorkspace.Detail.Repeat", repeatMode)),
            DetailLine(id, "ReminderDays", localizer.Format("Mobile.Reminders.Detail.ReminderDays", reminderDays)),
            DetailLine(id, "Note", localizer.Format("ReminderWorkspace.Detail.Note", note))
        ];

    private static MobileEvidenceDetailLineViewModel DetailLine(string id, string suffix, string text) =>
        new($"MobileEvidenceDetail_{SanitizeAutomationId(id)}_{suffix}", text);

    private string FormatCount(
        IAppLocalizer localizer,
        AppCulturePreferences culture,
        string countKey,
        string emptyKey,
        int count) =>
        count == 0
            ? localizer.GetString(emptyKey)
            : _pluralizationService.Format(localizer, culture, countKey, count, count);

    private string FormatOdometer(
        string? value,
        AppCulturePreferences culture,
        AppUnitPreferences units,
        IAppLocalizer localizer) =>
        VehimapValueParser.TryParseOdometer(value, out var parsed)
            ? _unitFormatService.FormatDistanceFromKilometers(parsed, culture, units, decimalPlaces: 0)
            : ValueOrFallback(value, localizer.GetString("Projection.Value.NoOdometer"));

    private string FormatVolume(
        string? value,
        AppCulturePreferences culture,
        AppUnitPreferences units,
        IAppLocalizer localizer) =>
        VehimapValueParser.TryParseDecimalNumber(value, out var parsed)
            ? _unitFormatService.FormatVolumeFromLiters(parsed, culture, units)
            : ValueOrFallback(value, localizer.GetString("Projection.Value.NoQuantity"));

    private string FormatMoney(
        string? value,
        AppCulturePreferences culture,
        string currency,
        IAppLocalizer localizer) =>
        VehimapValueParser.TryParseMoney(value, out var parsed)
            ? _numberFormatService.FormatMoney(parsed, culture, AppCurrencyFormatService.NormalizeCurrency(currency))
            : ValueOrFallback(value, localizer.GetString("Projection.Value.NoPrice"));

    private string BuildRecordValidity(
        VehicleRecord record,
        AppCulturePreferences culture,
        IAppLocalizer localizer)
    {
        var fromValue = FormatRecordDate(record.ValidFrom, culture);
        var toValue = FormatRecordDate(record.ValidTo, culture);
        var from = string.IsNullOrWhiteSpace(fromValue)
            ? localizer.GetString("Record.Projection.Validity.FromEmpty")
            : localizer.Format("Record.Projection.Validity.From", fromValue);
        var to = string.IsNullOrWhiteSpace(toValue)
            ? localizer.GetString("Record.Projection.Validity.ToEmpty")
            : localizer.Format("Record.Projection.Validity.To", toValue);
        return localizer.Format("Record.Projection.Validity.Range", from, to);
    }

    private string FormatRecordDate(string? value, AppCulturePreferences culture)
    {
        var normalized = (value ?? string.Empty).Trim();
        return VehimapValueParser.TryParseEventDate(normalized, out var parsed)
            ? _dateFormatService.FormatDate(parsed, culture)
            : normalized;
    }

    private string FormatReminderDueDate(string source, DateOnly parsed, AppCulturePreferences culture) =>
        VehimapValueParser.TryParseEventDate(source, out _)
            ? _dateFormatService.FormatDate(parsed, culture)
            : source.Trim();

    private string BuildReminderStatus(
        VehicleReminder reminder,
        DateOnly today,
        AppCulturePreferences culture,
        IAppLocalizer localizer)
    {
        if (!TryParseReminderDate(reminder.DueDate, out var dueDate))
        {
            return localizer.GetString("Reminder.Status.NoUsableDate");
        }

        var delta = dueDate.DayNumber - today.DayNumber;
        if (delta < 0)
        {
            var overdueDays = Math.Abs(delta);
            return _pluralizationService.Format(localizer, culture, "Reminder.Status.Overdue", overdueDays, overdueDays);
        }

        if (delta == 0)
        {
            return localizer.GetString("Reminder.Status.Today");
        }

        return delta == 1
            ? localizer.GetString("Reminder.Status.Tomorrow")
            : _pluralizationService.Format(localizer, culture, "Reminder.Status.InDays", delta, delta);
    }

    private string BuildReminderDays(
        string? value,
        AppCulturePreferences culture,
        IAppLocalizer localizer)
    {
        if (!int.TryParse((value ?? string.Empty).Trim(), out var days) || days < 0)
        {
            return localizer.GetString("Common.EmptyValue");
        }

        return _pluralizationService.Format(localizer, culture, "Mobile.Reminders.ReminderDays", days, days);
    }

    private static string ResolveRecordPath(
        VehimapDataRoot dataRoot,
        VehicleRecord record,
        Func<string, string> managedPathResolver)
    {
        if (string.IsNullOrWhiteSpace(record.FilePath))
        {
            return string.Empty;
        }

        try
        {
            if (record.AttachmentMode == VehicleRecordAttachmentMode.Managed)
            {
                return managedPathResolver(record.FilePath);
            }

            return Path.IsPathRooted(record.FilePath)
                ? record.FilePath
                : Path.GetFullPath(Path.Combine(dataRoot.AppBasePath, record.FilePath));
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or NotSupportedException)
        {
            return string.Empty;
        }
    }

    private static string BuildAttachmentState(
        VehicleRecord record,
        string resolvedPath,
        IAppLocalizer localizer)
    {
        if (string.IsNullOrWhiteSpace(record.FilePath))
        {
            return localizer.GetString("Record.Projection.AttachmentState.NoPath");
        }

        if (!string.IsNullOrWhiteSpace(resolvedPath) && File.Exists(resolvedPath))
        {
            return localizer.GetString("Record.Projection.AttachmentState.Available");
        }

        return record.AttachmentMode == VehicleRecordAttachmentMode.Managed
            ? localizer.GetString("Record.Projection.AttachmentState.ManagedMissing")
            : string.IsNullOrWhiteSpace(resolvedPath)
                ? localizer.GetString("Record.Projection.AttachmentState.Unresolved")
                : localizer.GetString("Record.Projection.AttachmentState.ExternalMissing");
    }

    private static bool TryParseReminderDate(string? value, out DateOnly date) =>
        VehimapValueParser.TryParseEventDate(value, out date)
        || VehimapValueParser.TryParseMonthYear(value, out date);

    private static AppCulturePreferences CreateCulturePreferences(DesktopSupportedSettingsSnapshot settings) =>
        new(settings.Language, settings.ThousandsSeparator, settings.DecimalSeparator);

    private static AppUnitPreferences CreateUnitPreferences(DesktopSupportedSettingsSnapshot settings) =>
        new(settings.DistanceUnit, settings.VolumeUnit);

    private static string ValueOrFallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string SanitizeAutomationId(string value)
    {
        var characters = value.Where(character => char.IsLetterOrDigit(character) || character == '_').ToArray();
        return characters.Length == 0 ? "Unknown" : new string(characters);
    }
}

internal sealed record MobileVehicleEvidenceProjection(
    MobileVehicleEvidenceKind Kind,
    string Heading,
    string Summary,
    string ListName,
    string ItemType,
    string DetailHeading,
    IReadOnlyList<MobileVehicleEvidenceItemViewModel> Items);
