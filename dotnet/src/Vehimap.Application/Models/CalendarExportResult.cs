// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record CalendarExportResult(
    IReadOnlyList<CalendarExportItem> Items,
    int SkippedMaintenanceCount,
    string IcsContent);
