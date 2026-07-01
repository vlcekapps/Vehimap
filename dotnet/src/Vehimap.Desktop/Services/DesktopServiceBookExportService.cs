// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Net;
using System.Text;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopServiceBookExportService
{
    private readonly IAppLocalizer _localizer;
    private readonly IAppNumberFormatService _numberFormatService;
    private AppCulturePreferences _culturePreferences = new(AppCultureService.CzechLanguage, AppCultureService.NoSeparator, AppCultureService.CommaSeparator);
    private string _currency = AppCurrencyFormatService.CzechCrowns;

    public DesktopServiceBookExportService(IAppLocalizer? localizer = null, IAppNumberFormatService? numberFormatService = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));
        _numberFormatService = numberFormatService ?? new AppNumberFormatService();
    }

    public void ApplySupportedSettings(DesktopSupportedSettingsSnapshot settings)
    {
        _culturePreferences = new AppCulturePreferences(
            settings.Language,
            settings.ThousandsSeparator,
            settings.DecimalSeparator);
        _currency = AppCurrencyFormatService.NormalizeCurrency(settings.Currency);
    }

    public string BuildFileName(ServiceBookSummary summary, DateTime generatedAt) =>
        $"{SafeFileName(summary.VehicleName)}-{L("ServiceBook.FileName.Suffix")}-{generatedAt:yyyy-MM-dd}.html";

    public string BuildHtml(
        ServiceBookSummary summary,
        IReadOnlyList<ServiceBookItemViewModel> historyItems,
        IReadOnlyList<ServiceBookItemViewModel> maintenanceItems,
        IReadOnlyList<ServiceBookItemViewModel> recordItems,
        DateTime generatedAt)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!DOCTYPE html>");
        builder.AppendLine($"<html lang=\"{L("ServiceBook.Export.HtmlLanguage")}\">");
        builder.AppendLine("<head>");
        builder.AppendLine("  <meta charset=\"utf-8\">");
        builder.AppendLine($"  <title>{L("ServiceBook.Export.Title")}</title>");
        builder.AppendLine("  <style>");
        builder.AppendLine("    body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#1d2329;background:#f7f7f4;}");
        builder.AppendLine("    h1{margin:0 0 8px 0;font-size:28px;}h2{margin:22px 0 10px 0;font-size:20px;}");
        builder.AppendLine("    .meta,.summary{margin:0 0 14px 0;color:#3b4248;}.card{background:#fff;border:1px solid #d8ddd4;border-radius:8px;padding:16px;margin:0 0 18px;}");
        builder.AppendLine("    table{width:100%;border-collapse:collapse;background:#fff;}th,td{border:1px solid #cfd5cb;padding:8px 10px;text-align:left;vertical-align:top;font-size:13px;}th{background:#edf2e8;}.empty{font-style:italic;color:#666;}");
        builder.AppendLine("    @media print{body{margin:12mm;background:#fff}.card{border:0;padding:0}a{text-decoration:none;color:inherit;}}");
        builder.AppendLine("  </style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine($"  <h1>{L("ServiceBook.Export.Heading")}</h1>");
        builder.AppendLine($"  <p class=\"meta\">{Html(LF("ServiceBook.Export.VehicleMeta", summary.VehicleName, summary.VehicleMakeModel, summary.VehicleCategory, summary.VehiclePlate))}</p>");
        builder.AppendLine($"  <p class=\"meta\">{Html(LF("ServiceBook.Export.OdometerMeta", summary.CurrentOdometer, generatedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)))}</p>");
        builder.AppendLine($"  <p class=\"summary\">{Html(LF("ServiceBook.Export.Summary", summary.Status, FormatMoney(summary.TotalHistoryCost)))}</p>");
        AppendSection(builder, L("ServiceBook.Export.HistorySection"), historyItems);
        AppendSection(builder, L("ServiceBook.Export.MaintenanceSection"), maintenanceItems);
        AppendSection(builder, L("ServiceBook.Export.RecordsSection"), recordItems);
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");
        return builder.ToString();
    }

    private void AppendSection(StringBuilder builder, string title, IReadOnlyList<ServiceBookItemViewModel> items)
    {
        builder.AppendLine("  <div class=\"card\">");
        builder.AppendLine($"    <h2>{title}</h2>");
        if (items.Count == 0)
        {
            builder.AppendLine($"    <p class=\"empty\">{L("ServiceBook.Export.EmptySection")}</p>");
            builder.AppendLine("  </div>");
            return;
        }

        builder.AppendLine("    <table>");
        builder.AppendLine(
            $"      <thead><tr><th>{L("ServiceBook.Export.Column.Primary")}</th><th>{L("ServiceBook.Export.Column.Secondary")}</th><th>{L("ServiceBook.Export.Column.Detail")}</th><th>{L("ServiceBook.Export.Column.Status")}</th></tr></thead>");
        builder.AppendLine("      <tbody>");
        foreach (var item in items)
        {
            builder.AppendLine("        <tr>");
            builder.AppendLine($"          <td>{Html(item.Primary)}</td>");
            builder.AppendLine($"          <td>{Html(item.Secondary)}</td>");
            builder.AppendLine($"          <td>{Html(item.Detail)}</td>");
            builder.AppendLine($"          <td>{Html(item.Status)}</td>");
            builder.AppendLine("        </tr>");
        }

        builder.AppendLine("      </tbody>");
        builder.AppendLine("    </table>");
        builder.AppendLine("  </div>");
    }

    private string SafeFileName(string value)
    {
        var safe = string.IsNullOrWhiteSpace(value) ? L("ServiceBook.FileName.VehicleFallback") : value.Trim();
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            safe = safe.Replace(invalidChar, '_');
        }

        safe = string.Join("_", safe.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(safe) ? L("ServiceBook.FileName.VehicleFallback") : safe.Trim('_', '.');
    }

    private static string Html(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private string FormatMoney(decimal value) =>
        _numberFormatService.FormatMoney(value, _culturePreferences, _currency);

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);
}
