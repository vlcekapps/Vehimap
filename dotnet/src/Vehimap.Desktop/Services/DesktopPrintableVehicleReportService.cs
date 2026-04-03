using System.Globalization;
using System.Net;
using System.Text;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopPrintableVehicleReportService
{
    public string BuildHtml(
        VehimapDataSet dataSet,
        IReadOnlyDictionary<string, VehicleMeta> metaByVehicleId,
        ITimelineService timelineService,
        DateOnly today,
        DateTime generatedAt)
    {
        var sections = GetPrintableCategories(dataSet)
            .Select(category => BuildCategorySection(category, dataSet, metaByVehicleId, timelineService, today))
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine("<!DOCTYPE html>");
        builder.AppendLine("<html lang=\"cs\">");
        builder.AppendLine("<head>");
        builder.AppendLine("  <meta charset=\"utf-8\">");
        builder.AppendLine("  <title>Vehimap - Tiskový přehled vozidel</title>");
        builder.AppendLine("  <style>");
        builder.AppendLine("    body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#111;background:#fff;}");
        builder.AppendLine("    h1{margin:0 0 8px 0;font-size:28px;}");
        builder.AppendLine("    h2{margin:28px 0 10px 0;font-size:20px;border-bottom:1px solid #bbb;padding-bottom:4px;}");
        builder.AppendLine("    p.meta{margin:0 0 18px 0;color:#444;}");
        builder.AppendLine("    table{width:100%;border-collapse:collapse;margin-bottom:18px;}");
        builder.AppendLine("    th,td{border:1px solid #b9b9b9;padding:6px 8px;vertical-align:top;text-align:left;font-size:13px;}");
        builder.AppendLine("    th{background:#efefef;font-weight:600;}");
        builder.AppendLine("    p.empty{margin:8px 0 18px 0;color:#555;}");
        builder.AppendLine("    @media print{body{margin:12mm;}a{text-decoration:none;color:inherit;}}");
        builder.AppendLine("  </style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine("  <h1>Vehimap - Tiskový přehled vozidel</h1>");
        builder.AppendLine($"  <p class=\"meta\">Vytvořeno: {Html(generatedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture))} | Celkem vozidel: {dataSet.Vehicles.Count}</p>");
        foreach (var section in sections)
        {
            builder.Append(section);
        }

        builder.AppendLine("</body>");
        builder.AppendLine("</html>");
        return builder.ToString();
    }

    private static IReadOnlyList<string> GetPrintableCategories(VehimapDataSet dataSet)
    {
        var categories = new List<string>(LegacyKnownValues.Categories);
        var known = new HashSet<string>(LegacyKnownValues.Categories, StringComparer.CurrentCultureIgnoreCase);
        var extras = dataSet.Vehicles
            .Select(vehicle => vehicle.Category?.Trim() ?? string.Empty)
            .Where(category => !string.IsNullOrWhiteSpace(category) && !known.Contains(category))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(category => category, StringComparer.CurrentCultureIgnoreCase);

        categories.AddRange(extras);
        return categories;
    }

    private static string BuildCategorySection(
        string category,
        VehimapDataSet dataSet,
        IReadOnlyDictionary<string, VehicleMeta> metaByVehicleId,
        ITimelineService timelineService,
        DateOnly today)
    {
        var vehicles = dataSet.Vehicles
            .Where(vehicle => string.Equals(vehicle.Category, category, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(vehicle => BuildDueSortKey(vehicle.NextTk))
            .ThenBy(vehicle => vehicle.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine($"  <h2>{Html(category)} ({vehicles.Count})</h2>");
        if (vehicles.Count == 0)
        {
            builder.AppendLine("  <p class=\"empty\">V této kategorii není žádné vozidlo.</p>");
            return builder.ToString();
        }

        builder.AppendLine("  <table>");
        builder.AppendLine("    <thead><tr><th>Název</th><th>Poznámka</th><th>Značka / model</th><th>SPZ</th><th>Rok výroby</th><th>Výkon</th><th>Stav vozidla</th><th>Štítky</th><th>Poslední TK</th><th>Příští TK</th><th>Zelená karta do</th><th>Stav</th></tr></thead>");
        builder.AppendLine("    <tbody>");
        foreach (var vehicle in vehicles)
        {
            metaByVehicleId.TryGetValue(vehicle.Id, out var meta);
            var timeline = timelineService.BuildVehicleTimeline(dataSet, vehicle.Id, today);
            builder.AppendLine("      <tr>");
            builder.AppendLine($"        <td>{Html(vehicle.Name)}</td>");
            builder.AppendLine($"        <td>{Html(vehicle.VehicleNote)}</td>");
            builder.AppendLine($"        <td>{Html(vehicle.MakeModel)}</td>");
            builder.AppendLine($"        <td>{Html(vehicle.Plate)}</td>");
            builder.AppendLine($"        <td>{Html(vehicle.Year)}</td>");
            builder.AppendLine($"        <td>{Html(vehicle.Power)}</td>");
            builder.AppendLine($"        <td>{Html(meta?.State ?? string.Empty)}</td>");
            builder.AppendLine($"        <td>{Html(meta?.Tags ?? string.Empty)}</td>");
            builder.AppendLine($"        <td>{Html(vehicle.LastTk)}</td>");
            builder.AppendLine($"        <td>{Html(vehicle.NextTk)}</td>");
            builder.AppendLine($"        <td>{Html(vehicle.GreenCardTo)}</td>");
            builder.AppendLine($"        <td>{Html(BuildPrintableStatusText(timeline))}</td>");
            builder.AppendLine("      </tr>");
        }

        builder.AppendLine("    </tbody>");
        builder.AppendLine("  </table>");
        return builder.ToString();
    }

    private static string BuildPrintableStatusText(IReadOnlyList<VehicleTimelineItem> timelineItems)
    {
        var parts = new List<string>();
        AddTimelineStatusPart(parts, timelineItems, "technical", "TK");
        AddTimelineStatusPart(parts, timelineItems, "green", "ZK");
        AddTimelineStatusPart(parts, timelineItems, "custom", "Připomínka");
        AddTimelineStatusPart(parts, timelineItems, "maintenance", "Údržba");
        return string.Join(" | ", parts);
    }

    private static void AddTimelineStatusPart(List<string> parts, IReadOnlyList<VehicleTimelineItem> timelineItems, string kind, string label)
    {
        var relevantStatus = timelineItems
            .Where(item => string.Equals(item.Kind, kind, StringComparison.Ordinal))
            .Select(item => item.Status)
            .Where(status => !string.IsNullOrWhiteSpace(status) && !string.Equals(status, "Bez upozornění", StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(GetStatusPriority)
            .ThenBy(status => status, StringComparer.CurrentCultureIgnoreCase)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(relevantStatus))
        {
            parts.Add($"{label}: {relevantStatus}");
        }
    }

    private static int GetStatusPriority(string status)
    {
        if (status.Contains("Po termínu", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("Po limitu", StringComparison.CurrentCultureIgnoreCase))
        {
            return 0;
        }

        if (string.Equals(status, "Dnes", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("Servis dnes", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("Servis nyní", StringComparison.CurrentCultureIgnoreCase))
        {
            return 1;
        }

        if (status.StartsWith("Do ", StringComparison.CurrentCultureIgnoreCase)
            || status.StartsWith("Za ", StringComparison.CurrentCultureIgnoreCase))
        {
            return 2;
        }

        return 3;
    }

    private static int BuildDueSortKey(string? dueText)
    {
        if (!VehimapValueParser.TryParseMonthYear(dueText, out var monthDate))
        {
            return int.MaxValue;
        }

        var dueDate = new DateOnly(monthDate.Year, monthDate.Month, DateTime.DaysInMonth(monthDate.Year, monthDate.Month));
        return dueDate.DayNumber;
    }

    private static string Html(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
