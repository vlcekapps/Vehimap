// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public enum RepairAction { Create, Edit, Reschedule, Complete, CannotRepair }

public sealed class RepairValidationException(string resourceKey, string message) : InvalidOperationException(message)
{
    public string ResourceKey { get; } = resourceKey;
}

public sealed record RepairChange(
    RepairAction Action, string VehicleId, string? RepairId, string Title, string Description,
    string ReportedDate, string PlannedDate, int ReminderDays, string Reason,
    string CompletedDate, string Odometer, string Cost, string ExistingHistoryId = "");

public sealed class VehicleRepairService(IAppLocalizer localizer)
{
    public static void ValidateReferences(VehimapDataSet data)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var histories = new HashSet<string>(StringComparer.Ordinal);
        foreach (var repair in data.Repairs)
        {
            if (repair is null || string.IsNullOrWhiteSpace(repair.Id) || !ids.Add(repair.Id)
                || !data.Vehicles.Any(v => v.Id == repair.VehicleId)
                || string.IsNullOrWhiteSpace(repair.Title) || repair.Description is null || repair.Resolution is null
                || repair.State is not (VehicleRepair.Waiting or VehicleRepair.Repaired or VehicleRepair.Unrepairable)
                || repair.ReminderDays is < 0 or > 3650
                || !VehimapValueParser.TryParseEventDate(repair.ReportedDate, out var discovered)
                || repair.PlannedDate is null || repair.PlannedDate.Length > 0 && (!VehimapValueParser.TryParseEventDate(repair.PlannedDate, out var due) || due < discovered)
                || repair.ScheduleChanges is null || repair.ScheduleChanges.Any(c => c is null || c.Reason is null || c.NewDate is null || c.PreviousDate is null)
                || repair.HistoryEntryId is null
                || repair.State == VehicleRepair.Repaired && (!histories.Add(repair.HistoryEntryId)
                    || !data.HistoryEntries.Any(h => h.Id == repair.HistoryEntryId && h.VehicleId == repair.VehicleId))
                || repair.State != VehicleRepair.Repaired && repair.HistoryEntryId.Length != 0)
                throw new InvalidDataException("Invalid repair data or broken repair/history reference.");
        }
    }

    public VehicleRepair Apply(VehimapDataSet data, RepairChange change, DateTimeOffset now)
    {
        void Require(bool condition, string key)
        {
            if (!condition) throw new RepairValidationException(key, localizer.GetString(key));
        }
        Require(data.Vehicles.Any(v => v.Id == change.VehicleId), "Repairs.Error.Vehicle");
        Require(Enum.IsDefined(change.Action), "Repairs.Error.Missing");
        Require(change.Action != RepairAction.Create || change.RepairId is null, "Repairs.Error.Missing");
        var original = data.Repairs.FirstOrDefault(r => r.Id == change.RepairId && r.VehicleId == change.VehicleId);
        Require(change.Action == RepairAction.Create || original is not null, "Repairs.Error.Missing");
        Require(original is null || original.State == VehicleRepair.Waiting, "Repairs.Error.Closed");
        var updated = original;
        VehicleHistoryEntry? history = null;
        if (change.Action is RepairAction.Create or RepairAction.Edit)
        {
            Require(!string.IsNullOrWhiteSpace(change.Title), "Repairs.Error.Title");
            Require(VehimapValueParser.TryParseEventDate(change.ReportedDate, out var reported), "Repairs.Error.Date");
            Require(change.ReminderDays is >= 0 and <= 3650, "Repairs.Error.Days");
            Require(string.IsNullOrWhiteSpace(change.PlannedDate)
                || VehimapValueParser.TryParseEventDate(change.PlannedDate, out var planned) && planned >= reported, "Repairs.Error.PlannedDate");
            updated = new VehicleRepair(original?.Id ?? Guid.NewGuid().ToString("N"), change.VehicleId,
                change.Title.Trim(), change.Description.Trim(), VehimapValueParser.FormatCanonicalEventDate(reported),
                string.IsNullOrWhiteSpace(change.PlannedDate) ? "" : CanonicalDate(change.PlannedDate), change.ReminderDays,
                VehicleRepair.Waiting, "", "", original?.ScheduleChanges ?? []);
        }
        else if (change.Action == RepairAction.Reschedule)
        {
            Require(VehimapValueParser.TryParseEventDate(change.PlannedDate, out var planned)
                && VehimapValueParser.TryParseEventDate(original!.ReportedDate, out var reported) && planned >= reported, "Repairs.Error.PlannedDate");
            updated = original! with { PlannedDate = CanonicalDate(change.PlannedDate) };
        }
        else if (change.Action == RepairAction.CannotRepair)
        {
            Require(!string.IsNullOrWhiteSpace(change.Reason), "Repairs.Error.Reason");
            updated = original! with { State = VehicleRepair.Unrepairable, Resolution = change.Reason.Trim() };
        }
        else if (change.Action == RepairAction.Complete)
        {
            if (!string.IsNullOrEmpty(change.ExistingHistoryId))
            {
                history = data.HistoryEntries.FirstOrDefault(h => h.Id == change.ExistingHistoryId && h.VehicleId == change.VehicleId);
                Require(history is not null && !data.Repairs.Any(r => r.HistoryEntryId == history.Id), "Repairs.Error.History");
            }
            else
            {
                Require(VehimapValueParser.TryParseEventDate(change.CompletedDate, out var completed)
                    && VehimapValueParser.TryParseEventDate(original!.ReportedDate, out var reported) && completed >= reported, "Repairs.Error.Date");
                Require(!string.IsNullOrWhiteSpace(change.Reason), "Repairs.Error.Work");
                Require(string.IsNullOrEmpty(change.Odometer) || VehimapValueParser.TryParseOdometer(change.Odometer, out _), "Repairs.Error.Odometer");
                Require(string.IsNullOrEmpty(change.Cost) || VehimapValueParser.TryParseMoney(change.Cost, out var cost) && cost >= 0, "Repairs.Error.Cost");
                history = new VehicleHistoryEntry(Guid.NewGuid().ToString("N"), change.VehicleId,
                    VehimapValueParser.FormatCanonicalEventDate(completed), change.Reason.Trim(), change.Odometer, change.Cost, original!.Description);
            }
            Require(VehimapValueParser.TryParseEventDate(history!.EventDate, out var historyDate)
                && VehimapValueParser.TryParseEventDate(original!.ReportedDate, out var start) && historyDate >= start, "Repairs.Error.Date");
            updated = original! with { State = VehicleRepair.Repaired, HistoryEntryId = history.Id, Resolution = history.EventType };
        }
        Require(updated is not null, "Repairs.Error.Missing");
        if (original is not null && original.PlannedDate != updated!.PlannedDate)
            updated = updated with { ScheduleChanges = [.. original.ScheduleChanges,
                new RepairScheduleChange(now.ToString("O"), original.PlannedDate, updated.PlannedDate, change.Reason.Trim())] };
        // Only mutate the candidate dataset after every validation has succeeded.
        if (history is not null && string.IsNullOrEmpty(change.ExistingHistoryId)) data.HistoryEntries.Add(history);
        if (original is null) data.Repairs.Add(updated!);
        else data.Repairs[data.Repairs.IndexOf(original)] = updated!;
        return updated!;
    }

    public IReadOnlyList<AuditItem> BuildAudit(VehimapDataSet data, DateOnly today)
    {
        var result = new List<AuditItem>();
        foreach (var repair in data.Repairs)
        {
            string? key = null;
            if (repair.State == VehicleRepair.Unrepairable) key = "Repairs.Audit.Unrepairable";
            else if (repair.State == VehicleRepair.Waiting)
            {
                if (string.IsNullOrWhiteSpace(repair.PlannedDate)) key = "Repairs.Audit.Unscheduled";
                else if (VehimapValueParser.TryParseEventDate(repair.PlannedDate, out var due))
                {
                    if (due < today) key = "Repairs.Audit.Overdue";
                    else if (due.DayNumber - today.DayNumber <= repair.ReminderDays) key = "Repairs.Audit.Upcoming";
                }
            }
            if (key is null) continue;
            result.Add(new AuditItem(AuditSeverity.Warning, localizer.GetString("Repairs.Title"), repair.VehicleId,
                data.Vehicles.FirstOrDefault(v => v.Id == repair.VehicleId)?.Name ?? "", ApplicationEntityKinds.Repair,
                repair.Id, localizer.GetString(key), repair.Title));
        }
        return result;
    }

    private static string CanonicalDate(string value)
    {
        VehimapValueParser.TryParseEventDate(value, out var date);
        return VehimapValueParser.FormatCanonicalEventDate(date);
    }
}
