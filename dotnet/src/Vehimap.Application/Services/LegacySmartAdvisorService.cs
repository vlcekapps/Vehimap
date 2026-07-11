// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacySmartAdvisorService : ISmartAdvisorService
{
    private const string EntityVehicle = ApplicationEntityKinds.Vehicle;
    private const string EntityHistory = ApplicationEntityKinds.History;
    private const string EntityFuel = ApplicationEntityKinds.Fuel;
    private const string EntityRecord = ApplicationEntityKinds.Record;
    private const string EntityMaintenance = ApplicationEntityKinds.Maintenance;
    private const string EntityReminder = ApplicationEntityKinds.Reminder;
    private const string EntityCosts = ApplicationEntityKinds.Costs;
    private readonly ITimelineService _timelineService;
    private readonly IFuelAnalysisService _fuelAnalysisService;
    private readonly IAppLocalizer _localizer;
    private readonly IAppPluralizationService _pluralizationService;

    public LegacySmartAdvisorService(
        ITimelineService timelineService,
        IFuelAnalysisService fuelAnalysisService,
        IAppLocalizer? localizer = null,
        IAppPluralizationService? pluralizationService = null)
    {
        _timelineService = timelineService;
        _fuelAnalysisService = fuelAnalysisService;
        _localizer = localizer ?? new ResourceAppLocalizer();
        _pluralizationService = pluralizationService ?? new AppPluralizationService();
    }

    public SmartAdvisorSummary BuildSmartAdvisor(
        VehimapDataSet dataSet,
        IReadOnlyList<AuditItem> auditItems,
        CostAnalysisSummary? costSummary,
        DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(dataSet);
        ArgumentNullException.ThrowIfNull(auditItems);

        var items = new List<SmartAdvisorItem>();
        AddAuditItems(items, auditItems);
        AddTimelineItems(items, dataSet, today);
        AddFuelItems(items, dataSet);
        AddCostItems(items, costSummary);

        var orderedItems = items
            .GroupBy(item => item.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderByDescending(item => item.Priority)
            .ThenBy(item => item.DueDate ?? DateOnly.MaxValue)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var criticalCount = orderedItems.Count(item => item.Priority == SmartAdvisorPriority.Critical);
        var warningCount = orderedItems.Count(item => item.Priority == SmartAdvisorPriority.Warning);
        var recommendationCount = orderedItems.Count(item => item.Priority == SmartAdvisorPriority.Recommendation);

        return new SmartAdvisorSummary(
            orderedItems.Count,
            criticalCount,
            warningCount,
            recommendationCount,
            BuildStatus(orderedItems.Count, criticalCount, warningCount, recommendationCount),
            orderedItems);
    }

    private void AddAuditItems(ICollection<SmartAdvisorItem> items, IReadOnlyList<AuditItem> auditItems)
    {
        foreach (var audit in auditItems)
        {
            var category = MapAuditCategory(audit);
            var entityKind = ApplicationEntityKinds.Normalize(audit.EntityKind);
            var priority = audit.Severity switch
            {
                AuditSeverity.Error => SmartAdvisorPriority.Critical,
                AuditSeverity.Warning => SmartAdvisorPriority.Warning,
                _ => SmartAdvisorPriority.Info
            };

            items.Add(new SmartAdvisorItem(
                $"audit-{audit.VehicleId}-{audit.EntityKind}-{audit.EntityId}-{audit.Title}",
                priority,
                category,
                audit.VehicleId,
                ValueOrFallback(audit.VehicleName, L("Common.UnknownVehicle")),
                entityKind,
                audit.EntityId,
                audit.Title,
                audit.Message,
                LF("SmartAdvisor.Detail.Audit", audit.Category, audit.Message),
                BuildActionLabel(entityKind),
                null));
        }
    }

    private void AddTimelineItems(ICollection<SmartAdvisorItem> items, VehimapDataSet dataSet, DateOnly today)
    {
        foreach (var vehicle in dataSet.Vehicles)
        {
            foreach (var timeline in _timelineService.BuildVehicleTimeline(dataSet, vehicle.Id, today))
            {
                if (!IsAdvisorTimelineKind(timeline.Kind) || string.IsNullOrWhiteSpace(timeline.Status))
                {
                    continue;
                }

                var priority = BuildTimelinePriority(timeline.Status);
                if (priority == SmartAdvisorPriority.Info)
                {
                    continue;
                }

                items.Add(new SmartAdvisorItem(
                    $"timeline-{timeline.VehicleId}-{timeline.Kind}-{timeline.EntryId}-{timeline.Date:yyyyMMdd}",
                    priority,
                    timeline.Kind == "maintenance" ? SmartAdvisorCategory.Maintenance : SmartAdvisorCategory.Deadlines,
                    timeline.VehicleId,
                    ValueOrFallback(timeline.VehicleName, vehicle.Name),
                    BuildTimelineEntityKind(timeline.Kind),
                    timeline.EntryId,
                    LF("SmartAdvisor.Title.Timeline", timeline.KindLabel, timeline.Title),
                    LF("SmartAdvisor.Summary.Timeline", timeline.Status, timeline.DateText),
                    ValueOrFallback(timeline.Detail, timeline.Title),
                    BuildTimelineActionLabel(timeline.Kind),
                    timeline.Date));
            }
        }
    }

    private void AddFuelItems(ICollection<SmartAdvisorItem> items, VehimapDataSet dataSet)
    {
        var vehiclesById = dataSet.Vehicles.ToDictionary(item => item.Id, StringComparer.Ordinal);
        foreach (var vehicle in dataSet.Vehicles)
        {
            var analysis = _fuelAnalysisService.BuildVehicleFuelAnalysis(dataSet, vehicle.Id);
            foreach (var warning in analysis.Warnings)
            {
                var priority = warning.Severity switch
                {
                    FuelAnalysisWarningSeverity.Error => SmartAdvisorPriority.Critical,
                    FuelAnalysisWarningSeverity.Warning => SmartAdvisorPriority.Warning,
                    _ => SmartAdvisorPriority.Recommendation
                };

                items.Add(new SmartAdvisorItem(
                    $"fuel-{vehicle.Id}-{warning.Id}",
                    priority,
                    SmartAdvisorCategory.Fuel,
                    vehicle.Id,
                    ValueOrFallback(vehiclesById.GetValueOrDefault(vehicle.Id)?.Name, vehicle.Name),
                    EntityFuel,
                    warning.FuelEntryId ?? string.Empty,
                    warning.Title,
                    warning.Description,
                    L("SmartAdvisor.Detail.FuelAnalysis"),
                    string.IsNullOrWhiteSpace(warning.FuelEntryId)
                        ? L("SmartAdvisor.Action.OpenFuel")
                        : L("SmartAdvisor.Action.OpenRelatedFuel"),
                    null));
            }
        }
    }

    private void AddCostItems(ICollection<SmartAdvisorItem> items, CostAnalysisSummary? costSummary)
    {
        if (costSummary is null)
        {
            return;
        }

        foreach (var vehicle in costSummary.Vehicles.Where(item => item.TotalCost > 0m && !item.CostPerKm.HasValue))
        {
            items.Add(new SmartAdvisorItem(
                $"cost-{vehicle.VehicleId}-cost-per-distance-unavailable",
                SmartAdvisorPriority.Recommendation,
                SmartAdvisorCategory.Costs,
                vehicle.VehicleId,
                ValueOrFallback(vehicle.VehicleName, L("Common.UnknownVehicle")),
                EntityCosts,
                vehicle.VehicleId,
                L("SmartAdvisor.Title.CostPerDistanceUnavailable"),
                L("SmartAdvisor.Summary.CostPerDistanceUnavailable"),
                L("SmartAdvisor.Detail.CostPerDistanceUnavailable"),
                L("SmartAdvisor.Action.OpenVehicleCosts"),
                null));
        }
    }

    private SmartAdvisorCategory MapAuditCategory(AuditItem item)
    {
        if (LocalizedResourceValueMatcher.Matches(_localizer, item.Category, "Audit.Category.Attachment"))
        {
            return SmartAdvisorCategory.Attachments;
        }

        if (LocalizedResourceValueMatcher.Matches(_localizer, item.Category, "Audit.Category.Maintenance"))
        {
            return SmartAdvisorCategory.Maintenance;
        }

        if (LocalizedResourceValueMatcher.Matches(_localizer, item.Category, "Audit.Category.Costs"))
        {
            return SmartAdvisorCategory.Costs;
        }

        if (string.Equals(ApplicationEntityKinds.Normalize(item.EntityKind), EntityFuel, StringComparison.Ordinal))
        {
            return SmartAdvisorCategory.Fuel;
        }

        if (LocalizedResourceValueMatcher.Matches(_localizer, item.Category, "Audit.Category.TechnicalInspection")
            || LocalizedResourceValueMatcher.Matches(_localizer, item.Category, "Audit.Category.GreenCard"))
        {
            return SmartAdvisorCategory.Deadlines;
        }

        return SmartAdvisorCategory.Data;
    }

    private static bool IsAdvisorTimelineKind(string kind) =>
        kind is "technical" or "green" or "custom" or "maintenance" or "record";

    private SmartAdvisorPriority BuildTimelinePriority(string status)
    {
        if (ContainsLocalizedStatus(status, "Timeline.Status.Overdue"))
        {
            return SmartAdvisorPriority.Critical;
        }

        if (ContainsLocalizedStatus(
            status,
            "Timeline.Status.Today",
            "Timeline.Status.DaysLeft",
            "Timeline.Status.WithinDistance",
            "Timeline.Status.OverDistanceLimit",
            "Timeline.Status.MissingCurrentOdometer",
            "Timeline.Status.MissingOdometerForCalculation"))
        {
            return SmartAdvisorPriority.Warning;
        }

        return SmartAdvisorPriority.Info;
    }

    private static string BuildTimelineEntityKind(string kind) =>
        kind switch
        {
            "custom" => EntityReminder,
            "maintenance" => EntityMaintenance,
            "record" => EntityRecord,
            _ => EntityVehicle
        };

    private string BuildTimelineActionLabel(string kind) =>
        kind switch
        {
            "custom" => L("SmartAdvisor.Action.OpenReminder"),
            "maintenance" => L("SmartAdvisor.Action.OpenMaintenance"),
            "record" => L("SmartAdvisor.Action.OpenRecord"),
            _ => L("SmartAdvisor.Action.OpenVehicle")
        };

    private string BuildActionLabel(string entityKind) =>
        entityKind switch
        {
            EntityHistory => L("SmartAdvisor.Action.OpenHistory"),
            EntityFuel => L("SmartAdvisor.Action.OpenFuel"),
            EntityRecord => L("SmartAdvisor.Action.OpenRecord"),
            EntityMaintenance => L("SmartAdvisor.Action.OpenMaintenance"),
            EntityReminder => L("SmartAdvisor.Action.OpenReminder"),
            EntityCosts => L("SmartAdvisor.Action.OpenCosts"),
            _ => L("SmartAdvisor.Action.OpenVehicle")
        };

    private string BuildStatus(int totalCount, int criticalCount, int warningCount, int recommendationCount)
    {
        if (totalCount == 0)
        {
            return L("SmartAdvisor.Status.Empty");
        }

        var parts = new List<string>();
        if (criticalCount > 0)
        {
            parts.Add(LP("SmartAdvisor.Status.Part.Critical", criticalCount, criticalCount));
        }

        if (warningCount > 0)
        {
            parts.Add(LP("SmartAdvisor.Status.Part.Warning", warningCount, warningCount));
        }

        if (recommendationCount > 0)
        {
            parts.Add(LP("SmartAdvisor.Status.Part.Recommendation", recommendationCount, recommendationCount));
        }

        return LP("SmartAdvisor.Status.WithItems", totalCount, totalCount, string.Join(", ", parts));
    }

    private static string ValueOrFallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static bool ContainsAny(string value, params string[] expectedValues)
    {
        foreach (var expectedValue in expectedValues)
        {
            if (value.Contains(expectedValue, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private bool ContainsLocalizedStatus(string value, params string[] resourceKeys)
    {
        foreach (var resourceKey in resourceKeys)
        {
            var needle = GetLocalizedStatusNeedle(resourceKey);
            if (!string.IsNullOrWhiteSpace(needle)
                && value.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private string GetLocalizedStatusNeedle(string resourceKey)
    {
        var value = L(resourceKey);
        var placeholderIndex = value.IndexOf("{0}", StringComparison.Ordinal);
        return placeholderIndex >= 0 ? value[..placeholderIndex] : value;
    }

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);

    private string LP(string keyPrefix, int count, params object?[] args) =>
        _pluralizationService.Format(_localizer, keyPrefix, count, args);
}
