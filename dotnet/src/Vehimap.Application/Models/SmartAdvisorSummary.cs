// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Models;

public sealed record SmartAdvisorSummary(
    int TotalCount,
    int CriticalCount,
    int WarningCount,
    int RecommendationCount,
    string Status,
    IReadOnlyList<SmartAdvisorItem> Items);

public sealed record SmartAdvisorItem(
    string Id,
    SmartAdvisorPriority Priority,
    SmartAdvisorCategory Category,
    string VehicleId,
    string VehicleName,
    string EntityKind,
    string EntityId,
    string Title,
    string Summary,
    string Detail,
    string ActionLabel,
    DateOnly? DueDate);

public enum SmartAdvisorPriority
{
    Info = 0,
    Recommendation = 1,
    Warning = 2,
    Critical = 3
}

public enum SmartAdvisorCategory
{
    Data,
    Deadlines,
    Maintenance,
    Fuel,
    Attachments,
    Costs
}
