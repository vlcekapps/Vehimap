// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Domain.Models;

public sealed record RepairScheduleChange(string ChangedAt, string PreviousDate, string NewDate, string Reason);

public sealed record VehicleRepair(
    string Id,
    string VehicleId,
    string Title,
    string Description,
    string ReportedDate,
    string PlannedDate,
    int ReminderDays,
    string State,
    string Resolution,
    string HistoryEntryId,
    RepairScheduleChange[] ScheduleChanges)
{
    public const string Waiting = "waiting";
    public const string Repaired = "repaired";
    public const string Unrepairable = "unrepairable";
}
