using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class LegacySmartAdvisorService : ISmartAdvisorService
{
    private readonly ITimelineService _timelineService;
    private readonly IFuelAnalysisService _fuelAnalysisService;

    public LegacySmartAdvisorService(ITimelineService timelineService, IFuelAnalysisService fuelAnalysisService)
    {
        _timelineService = timelineService;
        _fuelAnalysisService = fuelAnalysisService;
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

    private static void AddAuditItems(ICollection<SmartAdvisorItem> items, IReadOnlyList<AuditItem> auditItems)
    {
        foreach (var audit in auditItems)
        {
            var category = MapAuditCategory(audit);
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
                ValueOrFallback(audit.VehicleName, "Neznámé vozidlo"),
                audit.EntityKind,
                audit.EntityId,
                audit.Title,
                audit.Message,
                $"Audit dat: {audit.Category}. {audit.Message}",
                BuildActionLabel(audit.EntityKind),
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
                    $"{timeline.KindLabel}: {timeline.Title}",
                    $"{timeline.Status}. {timeline.DateText}.",
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
                    "Tankování",
                    warning.FuelEntryId ?? string.Empty,
                    warning.Title,
                    warning.Description,
                    "Tankovací analýza upozorňuje na položku, kterou se vyplatí zkontrolovat.",
                    string.IsNullOrWhiteSpace(warning.FuelEntryId) ? "Otevřít tankování" : "Otevřít související tankování",
                    null));
            }
        }
    }

    private static void AddCostItems(ICollection<SmartAdvisorItem> items, CostAnalysisSummary? costSummary)
    {
        if (costSummary is null)
        {
            return;
        }

        foreach (var vehicle in costSummary.Vehicles.Where(item => item.TotalCost > 0m && !item.CostPerKm.HasValue))
        {
            items.Add(new SmartAdvisorItem(
                $"cost-{vehicle.VehicleId}-cost-per-km-unavailable",
                SmartAdvisorPriority.Recommendation,
                SmartAdvisorCategory.Costs,
                vehicle.VehicleId,
                ValueOrFallback(vehicle.VehicleName, "Neznámé vozidlo"),
                "Náklady",
                vehicle.VehicleId,
                "Cena na kilometr není dostupná",
                "Vozidlo má náklady, ale chybí použitelný rozdíl tachometru pro výpočet ceny na kilometr.",
                "Doplňte použitelné tachometry v historii nebo tankování, aby šlo spočítat cenu na kilometr.",
                "Otevřít náklady vozidla",
                null));
        }
    }

    private static SmartAdvisorCategory MapAuditCategory(AuditItem item)
    {
        if (string.Equals(item.Category, "Příloha", StringComparison.CurrentCultureIgnoreCase))
        {
            return SmartAdvisorCategory.Attachments;
        }

        if (string.Equals(item.Category, "Údržba", StringComparison.CurrentCultureIgnoreCase))
        {
            return SmartAdvisorCategory.Maintenance;
        }

        if (string.Equals(item.Category, "Náklady", StringComparison.CurrentCultureIgnoreCase))
        {
            return SmartAdvisorCategory.Costs;
        }

        if (string.Equals(item.EntityKind, "Tankování", StringComparison.CurrentCultureIgnoreCase))
        {
            return SmartAdvisorCategory.Fuel;
        }

        if (string.Equals(item.Category, "Technická kontrola", StringComparison.CurrentCultureIgnoreCase)
            || string.Equals(item.Category, "Zelená karta", StringComparison.CurrentCultureIgnoreCase))
        {
            return SmartAdvisorCategory.Deadlines;
        }

        return SmartAdvisorCategory.Data;
    }

    private static bool IsAdvisorTimelineKind(string kind) =>
        kind is "technical" or "green" or "custom" or "maintenance" or "record";

    private static SmartAdvisorPriority BuildTimelinePriority(string status)
    {
        if (status.Contains("Po termínu", StringComparison.CurrentCultureIgnoreCase))
        {
            return SmartAdvisorPriority.Critical;
        }

        if (status.Contains("Dnes", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("Do ", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("Po limitu", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("Chybí", StringComparison.CurrentCultureIgnoreCase))
        {
            return SmartAdvisorPriority.Warning;
        }

        return SmartAdvisorPriority.Info;
    }

    private static string BuildTimelineEntityKind(string kind) =>
        kind switch
        {
            "custom" => "Připomínka",
            "maintenance" => "Údržba",
            "record" => "Doklad",
            _ => "Vozidlo"
        };

    private static string BuildTimelineActionLabel(string kind) =>
        kind switch
        {
            "custom" => "Otevřít připomínku",
            "maintenance" => "Otevřít údržbu",
            "record" => "Otevřít doklad",
            _ => "Otevřít vozidlo"
        };

    private static string BuildActionLabel(string entityKind) =>
        entityKind switch
        {
            "Historie" => "Otevřít historii",
            "Tankování" => "Otevřít tankování",
            "Doklad" => "Otevřít doklad",
            "Údržba" => "Otevřít údržbu",
            "Připomínka" => "Otevřít připomínku",
            "Náklady" => "Otevřít náklady",
            _ => "Otevřít vozidlo"
        };

    private static string BuildStatus(int totalCount, int criticalCount, int warningCount, int recommendationCount)
    {
        if (totalCount == 0)
        {
            return "Chytrý poradce nenašel nic naléhavého. Data vypadají z pohledu známých pravidel v pořádku.";
        }

        var parts = new List<string>();
        if (criticalCount > 0)
        {
            parts.Add($"{criticalCount} naléhavých");
        }

        if (warningCount > 0)
        {
            parts.Add($"{warningCount} upozornění");
        }

        if (recommendationCount > 0)
        {
            parts.Add($"{recommendationCount} doporučení");
        }

        return $"Chytrý poradce našel {totalCount} položek: {string.Join(", ", parts)}.";
    }

    private static string ValueOrFallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
