// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;

namespace Vehimap.Application.Abstractions;

public interface ICalendarExportService
{
    CalendarExportResult BuildUpcomingCalendar(Vehimap.Domain.Models.VehimapDataSet dataSet, DateOnly today, DateTimeOffset generatedAtUtc);
}
