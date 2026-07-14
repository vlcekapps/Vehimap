// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
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
