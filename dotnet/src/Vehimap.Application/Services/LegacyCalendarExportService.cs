// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Text;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacyCalendarExportService : ICalendarExportService
{
    private const int IcsFirstLineByteLimit = 75;
    private const int IcsContinuationLineTextByteLimit = 74;

    private readonly ITimelineService _timelineService;
    private readonly IAppLocalizer _localizer;

    public LegacyCalendarExportService()
        : this(new LegacyTimelineService(), null)
    {
    }

    public LegacyCalendarExportService(ITimelineService timelineService, IAppLocalizer? localizer = null)
    {
        _timelineService = timelineService;
        _localizer = localizer ?? new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));
    }

    public CalendarExportResult BuildUpcomingCalendar(VehimapDataSet dataSet, DateOnly today, DateTimeOffset generatedAtUtc)
    {
        var items = new List<CalendarExportItem>();

        foreach (var vehicle in dataSet.Vehicles)
        {
            foreach (var entry in _timelineService.BuildVehicleTimeline(dataSet, vehicle.Id, today))
            {
                if (!entry.IsFuture || !IsCalendarExportKind(entry.Kind))
                {
                    continue;
                }

                items.Add(new CalendarExportItem(
                    entry.Kind,
                    entry.KindLabel,
                    entry.VehicleId,
                    entry.VehicleName,
                    entry.Date,
                    LF("CalendarExport.Summary", entry.KindLabel, entry.VehicleName),
                    BuildDescription(entry),
                    BuildUid(entry)));
            }
        }

        items = items
            .OrderBy(item => item.Date)
            .ThenBy(item => item.Summary, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Uid, StringComparer.Ordinal)
            .ToList();

        var skippedMaintenanceCount = dataSet.MaintenancePlans
            .Where(item => item.IsActive)
            .Count(item => !LegacyTimelineService.TryBuildMaintenanceSchedule(
                item,
                null,
                today,
                LegacyTimelineService.GetMaintenanceReminderDays(dataSet.Settings),
                LegacyTimelineService.GetMaintenanceReminderKm(dataSet.Settings),
                out _,
                out _,
                out _));

        return new CalendarExportResult(
            items,
            skippedMaintenanceCount,
            BuildIcsContent(items, generatedAtUtc));
    }

    private static bool IsCalendarExportKind(string kind) =>
        kind is "technical" or "green" or "custom" or "record" or "maintenance";

    private string BuildDescription(VehicleTimelineItem entry)
    {
        var lines = new List<string>
        {
            LF("CalendarExport.Description.Vehicle", entry.VehicleName),
            LF("CalendarExport.Description.Kind", entry.KindLabel),
            LF("CalendarExport.Description.Item", entry.Title),
            LF("CalendarExport.Description.DueDate", entry.DateText)
        };

        if (!string.IsNullOrWhiteSpace(entry.VehiclePlate))
        {
            lines.Add(LF("CalendarExport.Description.Plate", entry.VehiclePlate));
        }

        if (!string.IsNullOrWhiteSpace(entry.Detail))
        {
            lines.Add(LF("CalendarExport.Description.Detail", entry.Detail));
        }

        if (!string.IsNullOrWhiteSpace(entry.Status))
        {
            lines.Add(LF("CalendarExport.Description.Status", entry.Status));
        }

        if (!string.IsNullOrWhiteSpace(entry.Note))
        {
            lines.Add(LF("CalendarExport.Description.Note", entry.Note));
        }

        return string.Join('\n', lines);
    }

    private static string BuildUid(VehicleTimelineItem entry)
    {
        var stableId = string.IsNullOrWhiteSpace(entry.EntryId)
            ? SanitizeUidPart($"{entry.Date:yyyyMMdd}-{entry.Title}")
            : SanitizeUidPart(entry.EntryId);

        return $"vehimap-{entry.Kind}-{entry.VehicleId}-{stableId}@vlcekapps";
    }

    private static string SanitizeUidPart(string text)
    {
        var buffer = new StringBuilder();
        foreach (var ch in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                buffer.Append(ch);
            }
            else if (buffer.Length == 0 || buffer[^1] != '-')
            {
                buffer.Append('-');
            }
        }

        return buffer.ToString().Trim('-') is { Length: > 0 } value ? value : "item";
    }

    private static string BuildIcsContent(IReadOnlyList<CalendarExportItem> items, DateTimeOffset generatedAtUtc)
    {
        var lines = new List<string>
        {
            "BEGIN:VCALENDAR",
            "VERSION:2.0",
            "PRODID:-//vlcekapps//Vehimap//CS",
            "CALSCALE:GREGORIAN",
            "METHOD:PUBLISH"
        };

        var dtStamp = generatedAtUtc.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);

        foreach (var item in items)
        {
            lines.Add("BEGIN:VEVENT");
            lines.Add($"UID:{EscapeIcsText(item.Uid)}");
            lines.Add($"DTSTAMP:{dtStamp}");
            lines.Add($"DTSTART;VALUE=DATE:{item.Date:yyyyMMdd}");
            lines.Add($"DTEND;VALUE=DATE:{item.Date.AddDays(1):yyyyMMdd}");
            lines.Add($"SUMMARY:{EscapeIcsText(item.Summary)}");
            lines.Add($"DESCRIPTION:{EscapeIcsText(item.Description)}");
            lines.Add("END:VEVENT");
        }

        lines.Add("END:VCALENDAR");
        return string.Join("\r\n", lines.Select(FoldIcsLine)) + "\r\n";
    }

    private static string EscapeIcsText(string text)
    {
        return text
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace(";", "\\;", StringComparison.Ordinal)
            .Replace(",", "\\,", StringComparison.Ordinal)
            .Replace("\r\n", "\\n", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\n", StringComparison.Ordinal);
    }

    private static string FoldIcsLine(string line)
    {
        if (Encoding.UTF8.GetByteCount(line) <= IcsFirstLineByteLimit)
        {
            return line;
        }

        var builder = new StringBuilder();
        var currentLineTextBytes = 0;
        var textByteLimit = IcsFirstLineByteLimit;

        foreach (var rune in line.EnumerateRunes())
        {
            var runeBytes = rune.Utf8SequenceLength;
            if (currentLineTextBytes > 0 && currentLineTextBytes + runeBytes > textByteLimit)
            {
                builder.Append("\r\n ");
                currentLineTextBytes = 0;
                textByteLimit = IcsContinuationLineTextByteLimit;
            }

            builder.Append(rune);
            currentLineTextBytes += runeBytes;
        }

        return builder.ToString();
    }

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);
}
