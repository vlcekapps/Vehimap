namespace Vehimap.Application.Models;

public sealed record CalendarExportResult(
    IReadOnlyList<CalendarExportItem> Items,
    int SkippedMaintenanceCount,
    string IcsContent);
