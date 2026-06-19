using System.Globalization;
using System.Net;
using System.Text;
using Vehimap.Application;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopCostExportService
{
    public string BuildFleetSummaryTsv(CostAnalysisSummary summary)
    {
        var lines = new List<string>
        {
            Tsv(["Vozidlo", "Kategorie", "Palivo", "Historie", "Doklady", "Celkem", "Ujeto", "Cena / km", "Stav"])
        };

        foreach (var row in summary.Vehicles)
        {
            lines.Add(Tsv([
                row.VehicleName,
                row.Category,
                MoneyForTsv(row.FuelCost),
                MoneyForTsv(row.HistoryCost),
                MoneyForTsv(row.RecordCost),
                MoneyForTsv(row.TotalCost),
                row.DistanceKm?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                row.CostPerKm.HasValue ? MoneyForTsv(row.CostPerKm.Value) : string.Empty,
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
            Tsv(["Vozidlo", "Období", "Datum", "Skupina", "Název", "Částka", "Doplňující údaje", "Poznámka"])
        };

        if (entries.Count == 0)
        {
            lines.Add(Tsv([vehicleName, periodLabel, string.Empty, "Bez položek", string.Empty, string.Empty, string.Empty, string.Empty]));
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
                MoneyForTsv(entry.Amount),
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
        builder.AppendLine("<!DOCTYPE html><html lang=\"cs\"><head><meta charset=\"utf-8\">");
        builder.AppendLine("<title>Vehimap - Náklady vozidla</title>");
        builder.AppendLine("<style>");
        builder.AppendLine("body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#1d2329;background:#f7f7f4;}");
        builder.AppendLine("h1,h2{margin:0 0 12px}.meta,.summary{margin:0 0 18px}.card{background:#fff;border:1px solid #d8ddd4;border-radius:8px;padding:16px;margin:0 0 18px}");
        builder.AppendLine("table{width:100%;border-collapse:collapse;background:#fff}th,td{border:1px solid #cfd5cb;padding:8px 10px;text-align:left;vertical-align:top}th{background:#edf2e8}.empty{font-style:italic;color:#666}");
        builder.AppendLine("</style></head><body>");
        builder.AppendLine("<h1>Náklady vozidla</h1>");
        builder.AppendLine($"<p class=\"meta\">Vozidlo: {Html(vehicleName)} | Období: {Html(periodLabel)} | Vytvořeno: {Html(generatedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture))}</p>");

        builder.AppendLine("<div class=\"card\"><h2>Souhrn období</h2>");
        if (row is null)
        {
            builder.AppendLine("<p class=\"empty\">Pro vybrané vozidlo není k dispozici nákladový souhrn.</p>");
        }
        else
        {
            builder.AppendLine($"<p class=\"summary\">Celkem nákladů: {Html(Money(row.TotalCost))}. Ujeto: {Html(row.DistanceKm.HasValue ? $"{row.DistanceKm.Value} km" : "nedostupné")}. Cena / km: {Html(row.CostPerKm.HasValue ? $"{row.CostPerKm.Value:0.00} Kč/km" : "nedostupné")}. Stav: {Html(row.Status)}.</p>");
            builder.AppendLine("<table><thead><tr><th>Skupina</th><th>Částka</th></tr></thead><tbody>");
            builder.AppendLine($"<tr><td>Tankování</td><td>{Html(Money(row.FuelCost))}</td></tr>");
            builder.AppendLine($"<tr><td>Historie a servis</td><td>{Html(Money(row.HistoryCost))}</td></tr>");
            builder.AppendLine($"<tr><td>Doklady a pojištění</td><td>{Html(Money(row.RecordCost))}</td></tr>");
            builder.AppendLine($"<tr><td><strong>Celkem</strong></td><td><strong>{Html(Money(row.TotalCost))}</strong></td></tr>");
            builder.AppendLine("</tbody></table>");
        }

        builder.AppendLine("</div>");
        builder.AppendLine("<div class=\"card\"><h2>Detail položek období</h2>");
        if (entries.Count == 0)
        {
            builder.AppendLine("<p class=\"empty\">Ve zvoleném období nejsou žádné položky s vyplněnou číselnou částkou.</p>");
        }
        else
        {
            builder.AppendLine("<table><thead><tr><th>Datum</th><th>Skupina</th><th>Název</th><th>Částka</th><th>Doplňující údaje</th><th>Poznámka</th></tr></thead><tbody>");
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

    private static List<CostExportEntry> BuildVehicleCostEntries(VehimapDataSet dataSet, string vehicleId, DateOnly periodStart, DateOnly periodEnd)
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
                TextPart("Palivo", entry.FuelType),
                TextPart("Litry", entry.Liters),
                TextPart("Tachometr", entry.Odometer),
                entry.FullTank ? "Plná nádrž" : string.Empty
            ]);
            entries.Add(new CostExportEntry(date, entry.EntryDate, "Tankování", string.IsNullOrWhiteSpace(entry.FuelType) ? "Tankování" : entry.FuelType, amount, extra, entry.Note));
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
                "Historie a servis",
                string.IsNullOrWhiteSpace(entry.EventType) ? "Historie" : entry.EventType,
                amount,
                TextPart("Tachometr", entry.Odometer),
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
                TextPart("Druh", entry.RecordType),
                TextPart("Poskytovatel", entry.Provider),
                TextPart("Příloha", entry.AttachmentMode == VehicleRecordAttachmentMode.Managed ? Path.GetFileName(entry.FilePath) : entry.FilePath)
            ]);
            entries.Add(new CostExportEntry(
                date,
                dateText,
                "Doklady a pojištění",
                string.IsNullOrWhiteSpace(entry.Title) ? "Doklad" : entry.Title,
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

    private static string BuildPeriodLabel(DateOnly periodStart, DateOnly periodEnd) =>
        $"Od {periodStart:dd.MM.yyyy} do {periodEnd:dd.MM.yyyy}";

    private static string GetVehicleName(VehimapDataSet dataSet, string vehicleId) =>
        dataSet.Vehicles.FirstOrDefault(item => string.Equals(item.Id, vehicleId, StringComparison.Ordinal))?.Name
        ?? "vozidlo";

    private static string Money(decimal value) => $"{value:0.00} Kč";

    private static string MoneyForTsv(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');

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
