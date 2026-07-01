// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Net;
using System.Text;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopCostExportService
{
    private readonly bool _usesManagedLocalizer;
    private readonly IAppNumberFormatService _numberFormatService;
    private readonly IAppUnitFormatService _unitFormatService;
    private IAppLocalizer _localizer;
    private CultureInfo _formatCulture = CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage);
    private AppCulturePreferences _culturePreferences = new(AppCultureService.CzechLanguage, AppCultureService.NoSeparator, AppCultureService.CommaSeparator);
    private AppUnitPreferences _unitPreferences = new(AppUnitFormatService.Kilometers, AppUnitFormatService.Liters);
    private string _currency = AppCurrencyFormatService.CzechCrowns;

    public DesktopCostExportService(
        IAppLocalizer? localizer = null,
        IAppNumberFormatService? numberFormatService = null,
        IAppUnitFormatService? unitFormatService = null)
    {
        _usesManagedLocalizer = localizer is null;
        _localizer = localizer ?? new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));
        _numberFormatService = numberFormatService ?? new AppNumberFormatService();
        _unitFormatService = unitFormatService ?? new AppUnitFormatService(_numberFormatService);
    }

    public void ApplySupportedSettings(DesktopSupportedSettingsSnapshot settings)
    {
        var cultureService = new AppCultureService();
        _formatCulture = cultureService.ResolveCulture(settings.Language);
        if (_usesManagedLocalizer)
        {
            _localizer = new ResourceAppLocalizer(_formatCulture);
        }

        _culturePreferences = new AppCulturePreferences(
            settings.Language,
            settings.ThousandsSeparator,
            settings.DecimalSeparator);
        _unitPreferences = new AppUnitPreferences(settings.DistanceUnit, settings.VolumeUnit);
        _currency = AppCurrencyFormatService.NormalizeCurrency(settings.Currency);
    }

    public string BuildFleetSummaryTsv(CostAnalysisSummary summary)
    {
        var lines = new List<string>
        {
            Tsv([
                L("CostExport.Column.Vehicle"),
                L("CostExport.Column.Category"),
                L("CostExport.Column.Fuel"),
                L("CostExport.Column.History"),
                L("CostExport.Column.Records"),
                L("CostExport.Column.Total"),
                L("CostExport.Column.Distance"),
                L("CostExport.Column.CostPerDistance"),
                L("CostExport.Column.Status")
            ])
        };

        foreach (var row in summary.Vehicles)
        {
            lines.Add(Tsv([
                row.VehicleName,
                FormatCategory(row.Category),
                Money(row.FuelCost),
                Money(row.HistoryCost),
                Money(row.RecordCost),
                Money(row.TotalCost),
                row.DistanceKm.HasValue ? FormatDistance(row.DistanceKm.Value, decimalPlaces: 1) : string.Empty,
                row.CostPerKm.HasValue ? FormatCostPerDistance(row.CostPerKm.Value) : string.Empty,
                row.Status
            ]));
        }

        return string.Join('\n', lines);
    }

    public string BuildVehicleDetailTsv(VehimapDataSet dataSet, string vehicleId, DateOnly periodStart, DateOnly periodEnd)
    {
        var vehicleName = GetVehicleName(dataSet, vehicleId);
        var periodLabel = BuildPeriodLabel(periodStart, periodEnd);
        var entries = BuildVehicleCostEntries(dataSet, vehicleId, periodStart, periodEnd);
        var lines = new List<string>
        {
            Tsv([
                L("CostExport.Column.Vehicle"),
                L("CostExport.Column.Period"),
                L("CostExport.Column.Date"),
                L("CostExport.Column.Group"),
                L("CostExport.Column.Title"),
                L("CostExport.Column.Amount"),
                L("CostExport.Column.ExtraInfo"),
                L("CostExport.Column.Note")
            ])
        };

        if (entries.Count == 0)
        {
            lines.Add(Tsv([vehicleName, periodLabel, string.Empty, L("CostExport.EmptyItems"), string.Empty, string.Empty, string.Empty, string.Empty]));
            return string.Join('\n', lines);
        }

        foreach (var entry in entries)
        {
            lines.Add(Tsv([
                vehicleName,
                periodLabel,
                entry.DateText,
                entry.Group,
                entry.Title,
                Money(entry.Amount),
                entry.ExtraInfo,
                entry.Note
            ]));
        }

        return string.Join('\n', lines);
    }

    public string BuildVehicleReportHtml(VehimapDataSet dataSet, CostAnalysisSummary summary, string vehicleId, DateTime generatedAt)
    {
        var vehicleName = GetVehicleName(dataSet, vehicleId);
        var periodLabel = BuildPeriodLabel(summary.PeriodStart, summary.PeriodEnd);
        var row = summary.Vehicles.FirstOrDefault(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal));
        var entries = BuildVehicleCostEntries(dataSet, vehicleId, summary.PeriodStart, summary.PeriodEnd);

        var builder = new StringBuilder();
        builder.AppendLine($"<!DOCTYPE html><html lang=\"{L("CostExport.HtmlLanguage")}\"><head><meta charset=\"utf-8\">");
        builder.AppendLine($"<title>{Html(L("CostExport.ReportTitle"))}</title>");
        builder.AppendLine("<style>");
        builder.AppendLine("body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#1d2329;background:#f7f7f4;}");
        builder.AppendLine("h1,h2{margin:0 0 12px}.meta,.summary{margin:0 0 18px}.card{background:#fff;border:1px solid #d8ddd4;border-radius:8px;padding:16px;margin:0 0 18px}");
        builder.AppendLine("table{width:100%;border-collapse:collapse;background:#fff}th,td{border:1px solid #cfd5cb;padding:8px 10px;text-align:left;vertical-align:top}th{background:#edf2e8}.empty{font-style:italic;color:#666}");
        builder.AppendLine("</style></head><body>");
        builder.AppendLine($"<h1>{Html(L("CostExport.ReportHeading"))}</h1>");
        builder.AppendLine($"<p class=\"meta\">{Html(LF("CostExport.ReportMeta", vehicleName, periodLabel, generatedAt.ToString("g", _formatCulture)))}</p>");

        builder.AppendLine($"<div class=\"card\"><h2>{Html(L("CostExport.ReportSummaryHeading"))}</h2>");
        if (row is null)
        {
            builder.AppendLine($"<p class=\"empty\">{Html(L("CostExport.ReportSummaryUnavailable"))}</p>");
        }
        else
        {
            var distance = row.DistanceKm.HasValue ? FormatDistance(row.DistanceKm.Value, decimalPlaces: 1) : L("Cost.Value.Unavailable");
            var costPerDistance = row.CostPerKm.HasValue ? FormatCostPerDistance(row.CostPerKm.Value) : L("Cost.Value.Unavailable");
            builder.AppendLine($"<p class=\"summary\">{Html(LF("CostExport.ReportSummaryLine", Money(row.TotalCost), distance, costPerDistance, row.Status))}</p>");
            builder.AppendLine($"<table><thead><tr><th>{Html(L("CostExport.Column.Group"))}</th><th>{Html(L("CostExport.Column.Amount"))}</th></tr></thead><tbody>");
            builder.AppendLine($"<tr><td>{Html(L("CostExport.EntryGroup.Fuel"))}</td><td>{Html(Money(row.FuelCost))}</td></tr>");
            builder.AppendLine($"<tr><td>{Html(L("CostExport.EntryGroup.History"))}</td><td>{Html(Money(row.HistoryCost))}</td></tr>");
            builder.AppendLine($"<tr><td>{Html(L("CostExport.EntryGroup.Records"))}</td><td>{Html(Money(row.RecordCost))}</td></tr>");
            builder.AppendLine($"<tr><td><strong>{Html(L("CostExport.Column.Total"))}</strong></td><td><strong>{Html(Money(row.TotalCost))}</strong></td></tr>");
            builder.AppendLine("</tbody></table>");
        }

        builder.AppendLine("</div>");
        builder.AppendLine($"<div class=\"card\"><h2>{Html(L("CostExport.ReportDetailHeading"))}</h2>");
        if (entries.Count == 0)
        {
            builder.AppendLine($"<p class=\"empty\">{Html(L("CostExport.ReportDetailEmpty"))}</p>");
        }
        else
        {
            builder.AppendLine($"<table><thead><tr><th>{Html(L("CostExport.Column.Date"))}</th><th>{Html(L("CostExport.Column.Group"))}</th><th>{Html(L("CostExport.Column.Title"))}</th><th>{Html(L("CostExport.Column.Amount"))}</th><th>{Html(L("CostExport.Column.ExtraInfo"))}</th><th>{Html(L("CostExport.Column.Note"))}</th></tr></thead><tbody>");
            foreach (var entry in entries)
            {
                builder.AppendLine("<tr>");
                builder.AppendLine($"<td>{Html(entry.DateText)}</td>");
                builder.AppendLine($"<td>{Html(entry.Group)}</td>");
                builder.AppendLine($"<td>{Html(entry.Title)}</td>");
                builder.AppendLine($"<td>{Html(Money(entry.Amount))}</td>");
                builder.AppendLine($"<td>{Html(entry.ExtraInfo)}</td>");
                builder.AppendLine($"<td>{Html(entry.Note)}</td>");
                builder.AppendLine("</tr>");
            }

            builder.AppendLine("</tbody></table>");
        }

        builder.AppendLine("</div></body></html>");
        return builder.ToString();
    }

    public string BuildFleetSummaryFileName(CostAnalysisSummary summary) =>
        $"vehimap-naklady-souhrn-{summary.PeriodStart:yyyy-MM-dd}-{summary.PeriodEnd:yyyy-MM-dd}.tsv";

    public string BuildVehicleDetailFileName(VehimapDataSet dataSet, string vehicleId, DateOnly periodStart, DateOnly periodEnd) =>
        $"{SafeFileName(GetVehicleName(dataSet, vehicleId))}-naklady-detail-{periodStart:yyyy-MM-dd}-{periodEnd:yyyy-MM-dd}.tsv";

    public string BuildVehicleReportFileName(VehimapDataSet dataSet, string vehicleId, DateOnly periodStart, DateOnly periodEnd) =>
        $"{SafeFileName(GetVehicleName(dataSet, vehicleId))}-naklady-sestava-{periodStart:yyyy-MM-dd}-{periodEnd:yyyy-MM-dd}.html";

    private List<CostExportEntry> BuildVehicleCostEntries(VehimapDataSet dataSet, string vehicleId, DateOnly periodStart, DateOnly periodEnd)
    {
        var entries = new List<CostExportEntry>();

        foreach (var entry in dataSet.FuelEntries)
        {
            if (!string.Equals(entry.VehicleId, vehicleId, StringComparison.Ordinal)
                || !TryGetDatedMoney(entry.EntryDate, entry.TotalCost, periodStart, periodEnd, out var date, out var amount))
            {
                continue;
            }

            var extra = JoinNonEmpty([
                TextPart(L("CostExport.Extra.Fuel"), FormatFuelType(entry.FuelType)),
                TextPart(L("CostExport.Extra.Volume"), FormatFuelVolume(entry.Liters)),
                TextPart(L("CostExport.Extra.Odometer"), FormatOdometer(entry.Odometer)),
                entry.FullTank ? L("CostExport.Extra.FullTank") : string.Empty
            ]);
            entries.Add(new CostExportEntry(date, entry.EntryDate, L("CostExport.EntryGroup.Fuel"), string.IsNullOrWhiteSpace(entry.FuelType) ? L("CostExport.Entry.FuelFallback") : FormatFuelType(entry.FuelType), amount, extra, entry.Note));
        }

        foreach (var entry in dataSet.HistoryEntries)
        {
            if (!string.Equals(entry.VehicleId, vehicleId, StringComparison.Ordinal)
                || !TryGetDatedMoney(entry.EventDate, entry.Cost, periodStart, periodEnd, out var date, out var amount))
            {
                continue;
            }

            entries.Add(new CostExportEntry(
                date,
                entry.EventDate,
                L("CostExport.EntryGroup.History"),
                string.IsNullOrWhiteSpace(entry.EventType) ? L("CostExport.Entry.HistoryFallback") : entry.EventType,
                amount,
                TextPart(L("CostExport.Extra.Odometer"), FormatOdometer(entry.Odometer)),
                entry.Note));
        }

        foreach (var entry in dataSet.Records)
        {
            if (!string.Equals(entry.VehicleId, vehicleId, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(entry.Price)
                || !VehimapValueParser.TryResolveRecordDate(entry, out var date)
                || date < periodStart
                || date > periodEnd
                || !VehimapValueParser.TryParseMoney(entry.Price, out var amount))
            {
                continue;
            }

            var dateText = string.IsNullOrWhiteSpace(entry.ValidTo) ? entry.ValidFrom : entry.ValidTo;
            var extra = JoinNonEmpty([
                TextPart(L("CostExport.Extra.RecordType"), FormatRecordType(entry.RecordType)),
                TextPart(L("CostExport.Extra.Provider"), entry.Provider),
                TextPart(L("CostExport.Extra.Attachment"), entry.AttachmentMode == VehicleRecordAttachmentMode.Managed ? Path.GetFileName(entry.FilePath) : entry.FilePath)
            ]);
            entries.Add(new CostExportEntry(
                date,
                dateText,
                L("CostExport.EntryGroup.Records"),
                string.IsNullOrWhiteSpace(entry.Title) ? L("CostExport.Entry.RecordFallback") : entry.Title,
                amount,
                extra,
                entry.Note));
        }

        return entries
            .OrderByDescending(item => item.Date)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static bool TryGetDatedMoney(string dateText, string moneyText, DateOnly periodStart, DateOnly periodEnd, out DateOnly date, out decimal amount)
    {
        amount = 0m;
        return VehimapValueParser.TryParseEventDate(dateText, out date)
            && date >= periodStart
            && date <= periodEnd
            && VehimapValueParser.TryParseMoney(moneyText, out amount);
    }

    private string BuildPeriodLabel(DateOnly periodStart, DateOnly periodEnd) =>
        LF("CostAnalysis.PeriodLabel", periodStart.ToString("d", _formatCulture), periodEnd.ToString("d", _formatCulture));

    private string GetVehicleName(VehimapDataSet dataSet, string vehicleId) =>
        dataSet.Vehicles.FirstOrDefault(item => string.Equals(item.Id, vehicleId, StringComparison.Ordinal))?.Name
        ?? L("CostExport.Value.VehicleFallback");

    private string Money(decimal value) =>
        _numberFormatService.FormatMoney(value, _culturePreferences, _currency);

    private string FormatDistance(decimal kilometers, int decimalPlaces) =>
        _unitFormatService.FormatDistanceFromKilometers(kilometers, _culturePreferences, _unitPreferences, decimalPlaces);

    private string FormatCategory(string? value) =>
        LegacyKnownValueDisplayService.FormatCategory(value, _localizer);

    private string FormatFuelType(string? value) =>
        LegacyKnownValueDisplayService.FormatFuelType(value, _localizer);

    private string FormatRecordType(string? value) =>
        LegacyKnownValueDisplayService.FormatRecordType(value, _localizer);

    private string FormatCostPerDistance(decimal costPerKm)
    {
        var normalized = _unitFormatService.Normalize(_unitPreferences);
        var kilometersPerDisplayedUnit = _unitFormatService.ConvertDistanceToKilometers(1m, normalized);
        return LF("CostExport.Value.CostPerDistance", Money(costPerKm * kilometersPerDisplayedUnit), DistanceUnitLabel(normalized));
    }

    private string FormatFuelVolume(string? value)
    {
        return VehimapValueParser.TryParseDecimalNumber(value, out var liters)
            ? _unitFormatService.FormatVolumeFromLiters(liters, _culturePreferences, _unitPreferences, decimalPlaces: 2)
            : (value ?? string.Empty).Trim();
    }

    private string FormatOdometer(string? value)
    {
        return VehimapValueParser.TryParseOdometer(value, out var kilometers)
            ? FormatDistance(kilometers, decimalPlaces: 0)
            : (value ?? string.Empty).Trim();
    }

    private static string DistanceUnitLabel(AppUnitPreferences preferences) =>
        string.Equals(preferences.DistanceUnit, AppUnitFormatService.Miles, StringComparison.Ordinal)
            ? "mi"
            : "km";

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);

    private static string Tsv(IEnumerable<string> fields) =>
        string.Join('\t', fields.Select(field => (field ?? string.Empty).Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ')));

    private static string TextPart(string label, string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : $"{label}: {value.Trim()}";

    private static string JoinNonEmpty(IEnumerable<string> values) =>
        string.Join("; ", values.Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string SafeFileName(string value)
    {
        var safe = string.IsNullOrWhiteSpace(value) ? "vozidlo" : value.Trim();
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            safe = safe.Replace(invalidChar, '_');
        }

        safe = string.Join("_", safe.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(safe) ? "vozidlo" : safe.Trim('_', '.');
    }

    private static string Html(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private sealed record CostExportEntry(
        DateOnly Date,
        string DateText,
        string Group,
        string Title,
        decimal Amount,
        string ExtraInfo,
        string Note);
}
