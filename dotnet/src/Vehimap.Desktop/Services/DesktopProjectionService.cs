// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopProjectionService
{
    private readonly IAppLocalizer _localizer;
    private CultureInfo _formatCulture;
    private readonly IAppNumberFormatService _numberFormatService;
    private readonly IAppUnitFormatService _unitFormatService;
    private AppCulturePreferences _culturePreferences = new(AppCultureService.CzechLanguage, AppCultureService.NoSeparator, AppCultureService.CommaSeparator);
    private AppUnitPreferences _unitPreferences = new(AppUnitFormatService.Kilometers, AppUnitFormatService.Liters);
    private string _currency = AppCurrencyFormatService.CzechCrowns;

    public DesktopProjectionService()
        : this(
            new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage)),
            CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage))
    {
    }

    public DesktopProjectionService(IAppLocalizer localizer)
        : this(localizer, CultureInfo.CurrentCulture)
    {
    }

    public DesktopProjectionService(IAppLocalizer localizer, CultureInfo formatCulture)
        : this(localizer, formatCulture, new AppNumberFormatService())
    {
    }

    public DesktopProjectionService(
        IAppLocalizer localizer,
        CultureInfo formatCulture,
        IAppNumberFormatService numberFormatService,
        IAppUnitFormatService? unitFormatService = null)
    {
        _localizer = localizer;
        _formatCulture = formatCulture;
        _numberFormatService = numberFormatService;
        _unitFormatService = unitFormatService ?? new AppUnitFormatService(numberFormatService);
    }

    public void ApplySupportedSettings(DesktopSupportedSettingsSnapshot settings)
    {
        _culturePreferences = new AppCulturePreferences(
            settings.Language,
            settings.ThousandsSeparator,
            settings.DecimalSeparator);
        _formatCulture = new AppCultureService().ResolveCulture(settings.Language);
        _unitPreferences = new AppUnitPreferences(settings.DistanceUnit, settings.VolumeUnit);
        _currency = AppCurrencyFormatService.NormalizeCurrency(settings.Currency);
    }

    public DesktopListProjection<VehicleListItemViewModel> BuildVehicleList(
        VehimapDataSet dataSet,
        IReadOnlyDictionary<string, VehicleMeta> metaByVehicleId,
        IReadOnlyCollection<AuditItem> auditItems,
        ITimelineService timelineService,
        DesktopVehicleListFilters filters,
        DateOnly today)
    {
        var projectedVehicles = dataSet.Vehicles
            .Select(vehicle =>
            {
                var meta = metaByVehicleId.GetValueOrDefault(vehicle.Id);
                var timelineItems = timelineService
                    .BuildVehicleTimeline(dataSet, vehicle.Id, today)
                    .ToList();

                return new VehicleListItemViewModel(
                    vehicle.Id,
                    vehicle.Name,
                    vehicle.Category,
                    FormatValue(vehicle.Plate, L("Projection.Value.NoPlate")),
                    FormatValue(vehicle.MakeModel, L("Projection.Value.NoMakeModel")),
                    vehicle.VehicleNote,
                    vehicle.NextTk,
                    vehicle.GreenCardTo,
                    meta?.State ?? string.Empty,
                    meta?.Powertrain ?? string.Empty,
                    BuildVehicleStatusSummary(vehicle, meta, auditItems, timelineItems));
            })
            .ToList();

        var filteredVehicles = dataSet.Vehicles
            .Select((vehicle, index) => new
            {
                Vehicle = vehicle,
                Meta = metaByVehicleId.GetValueOrDefault(vehicle.Id),
                Projection = projectedVehicles[index],
                Timeline = timelineService.BuildVehicleTimeline(dataSet, vehicle.Id, today).ToList()
            })
            .Where(item => MatchesVehicleCategory(item.Vehicle, filters.SelectedCategory))
            .Where(item => MatchesVehicleSearch(item.Vehicle, item.Meta, item.Projection.StatusSummary, filters.SearchText))
            .Where(item => MatchesVehicleStatusFilter(item.Vehicle, item.Timeline, filters.StatusFilter))
            .Where(item => !filters.HideInactiveVehicles || !IsVehicleInactive(item.Meta))
            .OrderBy(item => item.Vehicle.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => item.Projection)
            .ToList();

        return new DesktopListProjection<VehicleListItemViewModel>(
            filteredVehicles,
            BuildVehicleListSummary(filteredVehicles.Count, dataSet.Vehicles.Count, filters));
    }

    public IReadOnlyList<AuditItemViewModel> BuildAuditItems(IReadOnlyList<AuditItem> auditItems) =>
        auditItems
            .Select(item => new AuditItemViewModel(
                item.VehicleId,
                item.EntityKind,
                item.EntityId,
                item.Severity switch
                {
                    AuditSeverity.Error => L("Audit.Severity.Error"),
                    AuditSeverity.Warning => L("Audit.Severity.Warning"),
                    _ => L("Audit.Severity.Info")
                },
                item.Category,
                item.VehicleName,
                item.Title,
                item.Message))
            .ToList();

    public IReadOnlyList<AuditItemViewModel> BuildDashboardAuditItems(IReadOnlyList<AuditItem> auditItems) =>
        BuildAuditItems(auditItems)
            .Take(8)
            .ToList();

    public DesktopSmartAdvisorProjection BuildSmartAdvisor(SmartAdvisorSummary summary) =>
        new(
            summary.Status,
            summary.Items
                .Select(item => new SmartAdvisorItemViewModel(
                    item.Id,
                    FormatSmartAdvisorPriority(item.Priority),
                    FormatSmartAdvisorCategory(item.Category),
                    FormatValue(item.VehicleName, L("Common.UnknownVehicle")),
                    item.VehicleId,
                    item.EntityKind,
                    item.EntityId,
                    item.Title,
                    item.Summary,
                    item.Detail,
                    item.ActionLabel,
                    item.DueDate.HasValue ? item.DueDate.Value.ToString("d", _formatCulture) : L("SmartAdvisor.Value.NoDueDate"),
                    (int)item.Priority))
                .ToList());

    public IReadOnlyList<CostVehicleItemViewModel> BuildDashboardCostVehicles(CostAnalysisSummary costSummary) =>
        costSummary.Vehicles
            .Where(item => item.TotalCost > 0m || !IsInactiveCostStatus(item.Status))
            .Select(row => new CostVehicleItemViewModel(
                row.VehicleId,
                row.VehicleName,
                row.Category,
                FormatMoney(row.FuelCost),
                FormatMoney(row.HistoryCost),
                FormatMoney(row.RecordCost),
                FormatMoney(row.TotalCost),
                FormatDistance(row.DistanceKm),
                FormatCostPerDistance(row.CostPerKm),
                row.Status,
                LF(
                    "CostItem.AccessibleLabel",
                    row.VehicleName,
                    row.Category,
                    FormatMoney(row.TotalCost),
                    FormatMoney(row.FuelCost),
                    FormatMoney(row.HistoryCost),
                    FormatMoney(row.RecordCost),
                    FormatDistance(row.DistanceKm),
                    FormatCostPerDistance(row.CostPerKm),
                    row.Status)))
            .ToList();

    public DesktopVehicleDetailProjection BuildVehicleDetail(
        VehimapDataSet dataSet,
        VehicleListItemViewModel? vehicle,
        VehicleMeta? meta = null,
        VehimapDataRoot? dataRoot = null,
        Func<string, string>? managedPathResolver = null,
        DateOnly? today = null)
    {
        if (vehicle is null)
        {
            return new DesktopVehicleDetailProjection(
                L("VehicleDetail.Projection.EmptyHeading"),
                L("VehicleDetail.Projection.EmptyOverview"),
                string.Empty,
                string.Empty,
                L("VehicleDetail.Projection.EmptyEvidence"),
                L("VehicleDetail.Projection.EmptyRecentHistory"),
                [],
                []);
        }

        var effectiveToday = today ?? DateOnly.FromDateTime(DateTime.Today);
        var state = string.IsNullOrWhiteSpace(vehicle.State) ? L("Projection.Value.NormalOperation") : vehicle.State;
        var tags = string.IsNullOrWhiteSpace(meta?.Tags) ? L("Common.EmptyValue") : meta.Tags;
        var note = string.IsNullOrWhiteSpace(vehicle.VehicleNote) ? L("Common.NoNote") : vehicle.VehicleNote;
        var powertrain = string.IsNullOrWhiteSpace(meta?.Powertrain) ? L("Common.EmptyValue") : meta.Powertrain;
        var climate = string.IsNullOrWhiteSpace(meta?.ClimateProfile) ? L("Common.EmptyValue") : meta.ClimateProfile;
        var timingDrive = string.IsNullOrWhiteSpace(meta?.TimingDrive) ? L("Common.EmptyValue") : meta.TimingDrive;
        var transmission = string.IsNullOrWhiteSpace(meta?.Transmission) ? L("Common.EmptyValue") : meta.Transmission;
        var currentOdometer = BuildCurrentOdometerLookup(dataSet).GetValueOrDefault(vehicle.Id);
        var recentHistory = BuildRecentVehicleHistory(dataSet, vehicle.Id);
        var evidenceSummaries = BuildVehicleEvidenceSummaryItems(
            dataSet,
            vehicle.Id,
            dataRoot,
            managedPathResolver,
            effectiveToday,
            currentOdometer);

        return new DesktopVehicleDetailProjection(
            vehicle.Name,
            LF(
                "VehicleDetail.Projection.Overview",
                vehicle.MakeModel,
                vehicle.Category,
                vehicle.Plate,
                state,
                tags,
                FormatCurrentOdometer(currentOdometer),
                note),
            LF(
                "VehicleDetail.Projection.Dates",
                FormatValue(vehicle.NextTk, L("Common.EmptyValue")),
                FormatValue(vehicle.GreenCardTo, L("Common.EmptyValue")),
                FormatValue(vehicle.StatusSummary, L("Projection.Value.NoWarning"))),
            LF("VehicleDetail.Projection.Profile", powertrain, climate, timingDrive, transmission),
            BuildVehicleEvidenceSummary(dataSet, vehicle.Id),
            recentHistory.Count == 0
                ? L("VehicleDetail.Projection.RecentHistoryEmpty")
                : LF("VehicleDetail.Projection.RecentHistoryCount", recentHistory.Count),
            evidenceSummaries,
            recentHistory);
    }

    public DesktopListProjection<VehicleHistoryItemViewModel> BuildHistory(VehimapDataSet dataSet, string vehicleId)
    {
        var items = dataSet.HistoryEntries
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryParseEventDate(item.EventDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Item.EventType, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new VehicleHistoryItemViewModel(
                item.Item.Id,
                FormatValue(item.Item.EventDate, L("Common.NoDate")),
                FormatValue(item.Item.EventType, L("Projection.Value.NoType")),
                FormatValue(item.Item.Odometer, L("Projection.Value.NoOdometer")),
                FormatValue(item.Item.Cost, L("Projection.Value.NoPrice")),
                FormatValue(item.Item.Note, L("Common.NoNote"))))
            .ToList();

        var summary = items.Count == 0
            ? L("History.Projection.Summary.Empty")
            : LF("History.Projection.Summary.Count", items.Count);

        return new DesktopListProjection<VehicleHistoryItemViewModel>(items, summary);
    }

    public DesktopListProjection<VehicleFuelItemViewModel> BuildFuel(VehimapDataSet dataSet, string vehicleId)
    {
        var items = dataSet.FuelEntries
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryParseEventDate(item.EntryDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Item.FuelType, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new VehicleFuelItemViewModel(
                item.Item.Id,
                FormatValue(item.Item.EntryDate, L("Common.NoDate")),
                FormatValue(item.Item.FuelType, L("Projection.Value.NoType")),
                FormatFuelVolume(item.Item.Liters),
                FormatCostValue(item.Item.TotalCost),
                FormatOdometerValue(item.Item.Odometer),
                item.Item.FullTank ? L("Fuel.Projection.FullTank") : L("Fuel.Projection.PartialFuel"),
                FormatValue(item.Item.FuelDetail, L("Projection.Value.NoFuelDetail")),
                FormatValue(item.Item.Station, L("Projection.Value.NoStation")),
                FormatValue(item.Item.Note, L("Common.NoNote"))))
            .ToList();

        var summary = items.Count == 0
            ? L("Fuel.Projection.Summary.Empty")
            : LF("Fuel.Projection.Summary.Count", items.Count);

        return new DesktopListProjection<VehicleFuelItemViewModel>(items, summary);
    }

    public DesktopFuelAnalysisProjection BuildFuelAnalysis(FuelAnalysisSummary analysis)
    {
        var summaryLines = new List<string>
        {
            LF("FuelAnalysis.Summary.Main", analysis.EntryCount, FormatFuelAnalysisVolume(analysis.TotalLiters), FormatFuelAnalysisMoney(analysis.TotalCost), FormatOptionalPricePerVolume(analysis.AveragePricePerLiter)),
            LF("FuelAnalysis.Summary.AverageConsumption", FormatOptionalConsumption(analysis.AverageConsumptionLitersPer100Km), analysis.Status)
        };

        if (analysis.BestConsumptionSegment is not null)
        {
            summaryLines.Add(LF("FuelAnalysis.Summary.BestSegment", FormatConsumptionSegmentPeriod(analysis.BestConsumptionSegment), FormatFuelAnalysisConsumption(analysis.BestConsumptionSegment.ConsumptionLitersPer100Km)));
        }

        if (analysis.WorstConsumptionSegment is not null
            && !string.Equals(analysis.WorstConsumptionSegment.Id, analysis.BestConsumptionSegment?.Id, StringComparison.Ordinal))
        {
            summaryLines.Add(LF("FuelAnalysis.Summary.WorstSegment", FormatConsumptionSegmentPeriod(analysis.WorstConsumptionSegment), FormatFuelAnalysisConsumption(analysis.WorstConsumptionSegment.ConsumptionLitersPer100Km)));
        }

        return new DesktopFuelAnalysisProjection(
            string.Join(Environment.NewLine, summaryLines),
            analysis.ConsumptionSegments
                .OrderByDescending(item => item.EndDate)
                .Select(item =>
                {
                    var period = FormatConsumptionSegmentPeriod(item);
                    var distance = FormatDistance(item.DistanceKm);
                    var liters = FormatFuelAnalysisVolume(item.Liters);
                    var consumption = FormatFuelAnalysisConsumption(item.ConsumptionLitersPer100Km);
                    var pricePerLiter = FormatOptionalPricePerVolume(item.PricePerLiter);
                    var costPerKm = item.CostPerKm.HasValue
                        ? FormatCostPerDistance(item.CostPerKm.Value)
                        : L("FuelAnalysis.Value.CostPerKmUnavailable");
                    return new FuelConsumptionSegmentItemViewModel(
                        item.Id,
                        item.EndFuelEntryId,
                        period,
                        distance,
                        liters,
                        consumption,
                        pricePerLiter,
                        costPerKm,
                        LF("FuelAnalysis.Accessible.Segment", period, distance, liters, consumption, pricePerLiter, costPerKm));
                })
                .ToList(),
            analysis.GroupSummaries
                .Select(item =>
                {
                    var fuel = BuildFuelGroupLabel(item.FuelType, item.FuelDetail);
                    var entryCount = LF("FuelAnalysis.Group.EntryCount", item.EntryCount);
                    var liters = FormatFuelAnalysisVolume(item.Liters);
                    var totalCost = FormatFuelAnalysisMoney(item.TotalCost);
                    var averagePrice = FormatOptionalPricePerVolume(item.AveragePricePerLiter);
                    var latestDate = item.LatestDate.HasValue
                        ? item.LatestDate.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
                        : L("FuelAnalysis.LatestDate.None");
                    return new FuelGroupSummaryItemViewModel(
                        item.Id,
                        item.LatestFuelEntryId,
                        item.Station,
                        fuel,
                        entryCount,
                        liters,
                        totalCost,
                        averagePrice,
                        latestDate,
                        LF("FuelAnalysis.Accessible.GroupSummary", item.Station, fuel, entryCount, liters, totalCost, averagePrice, latestDate));
                })
                .ToList(),
            analysis.Warnings
                .Select(item =>
                {
                    var fuelEntryId = item.FuelEntryId ?? string.Empty;
                    var severity = FormatFuelAnalysisWarningSeverity(item.Severity);
                    return new FuelAnalysisWarningItemViewModel(
                        item.Id,
                        fuelEntryId,
                        severity,
                        item.Title,
                        item.Description,
                        string.IsNullOrWhiteSpace(fuelEntryId)
                            ? LF("FuelAnalysis.Accessible.Warning", severity, item.Title, item.Description)
                            : LF("FuelAnalysis.Accessible.WarningWithAction", severity, item.Title, item.Description));
                })
                .ToList());
    }

    public DesktopListProjection<VehicleReminderItemViewModel> BuildReminders(VehimapDataSet dataSet, string vehicleId, DateOnly today)
    {
        var items = dataSet.Reminders
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = TryParseReminderDate(item.DueDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenBy(item => item.Date)
            .ThenBy(item => item.Item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new VehicleReminderItemViewModel(
                item.Item.Id,
                FormatValue(item.Item.Title, L("Projection.Value.NoTitle")),
                FormatValue(item.Item.DueDate, L("Projection.Value.NoDueDate")),
                BuildReminderStatus(item.Item, today),
                FormatReminderRepeatMode(item.Item.RepeatMode),
                FormatValue(item.Item.Note, L("Common.NoNote"))))
            .ToList();

        var summary = items.Count == 0
            ? L("Reminder.Projection.Summary.Empty")
            : LF("Reminder.Projection.Summary.Count", items.Count);

        return new DesktopListProjection<VehicleReminderItemViewModel>(items, summary);
    }

    public DesktopListProjection<VehicleMaintenanceItemViewModel> BuildMaintenance(VehimapDataSet dataSet, string vehicleId, DateOnly today)
    {
        var currentOdometerByVehicleId = BuildCurrentOdometerLookup(dataSet);
        var currentOdometer = currentOdometerByVehicleId.GetValueOrDefault(vehicleId);

        var items = dataSet.MaintenancePlans
            .Where(item => item.VehicleId == vehicleId)
            .OrderByDescending(item => item.IsActive)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new VehicleMaintenanceItemViewModel(
                item.Id,
                FormatValue(item.Title, L("Projection.Value.NoTitle")),
                BuildMaintenanceInterval(item),
                BuildMaintenanceLastService(item),
                BuildMaintenanceStatus(item, today, currentOdometer),
                FormatValue(item.Note, L("Common.NoNote"))))
            .ToList();

        var summary = items.Count == 0
            ? L("Maintenance.Projection.Summary.Empty")
            : LF("Maintenance.Projection.Summary.Count", items.Count);

        return new DesktopListProjection<VehicleMaintenanceItemViewModel>(items, summary);
    }

    public DesktopListProjection<VehicleTimelineItemViewModel> BuildTimeline(
        VehimapDataSet dataSet,
        ITimelineService timelineService,
        string vehicleId,
        DateOnly today,
        string selectedFilter,
        string? searchText)
    {
        var allItems = timelineService.BuildVehicleTimeline(dataSet, vehicleId, today).ToList();
        var filteredItems = allItems
            .Where(item => MatchesTimelineFilter(item, selectedFilter))
            .Where(item => MatchesTimelineSearch(item, searchText))
            .Select(item => new VehicleTimelineItemViewModel(
                item.Kind,
                item.KindLabel,
                item.DateText,
                item.Title,
                item.Detail,
                item.Status,
                item.VehicleName,
                item.VehicleId,
                item.EntryId,
                item.IsFuture,
                item.Note))
            .ToList();

        var futureCount = allItems.Count(item => item.IsFuture);
        var pastCount = allItems.Count - futureCount;
        var summary = allItems.Count == 0
            ? L("TimelineWorkspace.Summary.Empty")
            : filteredItems.Count == allItems.Count
                ? LF("TimelineWorkspace.Summary.All", allItems.Count, futureCount, pastCount)
                : LF("TimelineWorkspace.Summary.Filtered", allItems.Count, futureCount, pastCount, filteredItems.Count);

        return new DesktopListProjection<VehicleTimelineItemViewModel>(filteredItems, summary);
    }

    public DesktopListProjection<VehicleRecordItemViewModel> BuildRecords(
        VehimapDataRoot? dataRoot,
        VehimapDataSet dataSet,
        string vehicleId,
        Func<string, string> managedPathResolver)
    {
        var items = dataSet.Records
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => BuildVehicleRecordItem(dataRoot, item, managedPathResolver))
            .OrderBy(item => item.Validity, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var summary = items.Count == 0
            ? L("Record.Projection.Summary.Empty")
            : LF("Record.Projection.Summary.Count", items.Count);

        return new DesktopListProjection<VehicleRecordItemViewModel>(items, summary);
    }

    public DesktopListProjection<VehicleTimelineItemViewModel> BuildDashboardTimeline(
        VehimapDataSet dataSet,
        ITimelineService timelineService,
        DateOnly today)
    {
        var items = dataSet.Vehicles
            .SelectMany(vehicle => timelineService.BuildVehicleTimeline(dataSet, vehicle.Id, today))
            .Where(item => item.IsFuture)
            .OrderBy(item => item.Date)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.KindLabel, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Take(10)
            .Select(item => new VehicleTimelineItemViewModel(
                item.Kind,
                item.KindLabel,
                item.DateText,
                item.Title,
                item.Detail,
                item.Status,
                item.VehicleName,
                item.VehicleId,
                item.EntryId,
                item.IsFuture,
                item.Note))
            .ToList();

        var summary = items.Count == 0
            ? L("Overview.Summary.DashboardEmpty")
            : LF("Overview.Summary.DashboardWithItems", items.Count);

        return new DesktopListProjection<VehicleTimelineItemViewModel>(items, summary);
    }

    public string BuildAuditSummary(IReadOnlyCollection<AuditItem> audit)
    {
        if (audit.Count == 0)
        {
            return L("Audit.Summary.Empty");
        }

        var errorCount = audit.Count(item => item.Severity == AuditSeverity.Error);
        var warningCount = audit.Count(item => item.Severity == AuditSeverity.Warning);
        return LF("Audit.Summary.WithItems", audit.Count, errorCount, warningCount);
    }

    public string BuildCostSummary(CostAnalysisSummary summary)
    {
        return LF(
            "Cost.Summary",
            summary.PeriodLabel,
            FormatMoney(summary.TotalCost),
            FormatDistance(summary.DistanceKm),
            FormatCostPerDistance(summary.CostPerKm),
            summary.ActiveWithoutCostCount,
            summary.ActiveVehicleCount);
    }

    public string BuildCostComparison(CostAnalysisSummary summary)
    {
        return LF(
            "Cost.Comparison",
            FormatSignedMoney(summary.TotalCostDifference),
            FormatSignedCostPerDistance(summary.CostPerKmDifference),
            summary.CostPerKmUnavailableCount);
    }

    public string? GetRecordFolderPath(VehicleRecordItemViewModel? record)
    {
        if (record is null)
        {
            return null;
        }

        if (record.FileExists)
        {
            return Path.GetDirectoryName(record.ResolvedPath);
        }

        if (!string.IsNullOrWhiteSpace(record.ResolvedPath))
        {
            return Path.GetDirectoryName(record.ResolvedPath);
        }

        return null;
    }

    private VehicleRecordItemViewModel BuildVehicleRecordItem(
        VehimapDataRoot? dataRoot,
        VehicleRecord record,
        Func<string, string> managedPathResolver)
    {
        var resolvedPath = ResolveRecordPath(dataRoot, record, managedPathResolver);
        var fileExists = !string.IsNullOrWhiteSpace(resolvedPath) && File.Exists(resolvedPath);

        return new VehicleRecordItemViewModel(
            record.Id,
            FormatValue(record.RecordType, L("Projection.Value.Document")),
            FormatValue(record.Title, L("Projection.Value.NoTitle")),
            FormatValue(record.Provider, L("Projection.Value.NoProvider")),
            BuildRecordValidity(record),
            FormatCostValue(record.Price),
            record.AttachmentMode == VehicleRecordAttachmentMode.Managed
                ? L("Record.Projection.AttachmentMode.Managed")
                : L("Record.Projection.AttachmentMode.External"),
            BuildAttachmentState(record, resolvedPath, fileExists),
            record.FilePath,
            resolvedPath,
            fileExists,
            record.Note);
    }

    private static string ResolveRecordPath(
        VehimapDataRoot? dataRoot,
        VehicleRecord record,
        Func<string, string> managedPathResolver)
    {
        if (dataRoot is null || string.IsNullOrWhiteSpace(record.FilePath))
        {
            return string.Empty;
        }

        if (record.AttachmentMode == VehicleRecordAttachmentMode.Managed)
        {
            return managedPathResolver(record.FilePath);
        }

        return Path.IsPathRooted(record.FilePath)
            ? record.FilePath
            : Path.GetFullPath(Path.Combine(dataRoot.AppBasePath, record.FilePath));
    }

    private string BuildRecordValidity(VehicleRecord record)
    {
        var from = string.IsNullOrWhiteSpace(record.ValidFrom)
            ? L("Record.Projection.Validity.FromEmpty")
            : LF("Record.Projection.Validity.From", record.ValidFrom);
        var to = string.IsNullOrWhiteSpace(record.ValidTo)
            ? L("Record.Projection.Validity.ToEmpty")
            : LF("Record.Projection.Validity.To", record.ValidTo);
        return LF("Record.Projection.Validity.Range", from, to);
    }

    private string BuildAttachmentState(VehicleRecord record, string resolvedPath, bool fileExists)
    {
        if (string.IsNullOrWhiteSpace(record.FilePath))
        {
            return L("Record.Projection.AttachmentState.NoPath");
        }

        if (fileExists)
        {
            return L("Record.Projection.AttachmentState.Available");
        }

        return record.AttachmentMode == VehicleRecordAttachmentMode.Managed
            ? L("Record.Projection.AttachmentState.ManagedMissing")
            : string.IsNullOrWhiteSpace(resolvedPath)
                ? L("Record.Projection.AttachmentState.Unresolved")
                : L("Record.Projection.AttachmentState.ExternalMissing");
    }

    private static Dictionary<string, int?> BuildCurrentOdometerLookup(VehimapDataSet dataSet)
    {
        var result = new Dictionary<string, int?>(StringComparer.Ordinal);

        foreach (var vehicle in dataSet.Vehicles)
        {
            result[vehicle.Id] = null;
        }

        foreach (var item in dataSet.HistoryEntries)
        {
            if (!VehimapValueParser.TryParseOdometer(item.Odometer, out var odometer))
            {
                continue;
            }

            var current = result.GetValueOrDefault(item.VehicleId);
            if (!current.HasValue || odometer > current.Value)
            {
                result[item.VehicleId] = odometer;
            }
        }

        foreach (var item in dataSet.FuelEntries)
        {
            if (!VehimapValueParser.TryParseOdometer(item.Odometer, out var odometer))
            {
                continue;
            }

            var current = result.GetValueOrDefault(item.VehicleId);
            if (!current.HasValue || odometer > current.Value)
            {
                result[item.VehicleId] = odometer;
            }
        }

        return result;
    }

    private IReadOnlyList<VehicleHistoryItemViewModel> BuildRecentVehicleHistory(VehimapDataSet dataSet, string vehicleId) =>
        dataSet.HistoryEntries
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryParseEventDate(item.EventDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Item.EventType, StringComparer.CurrentCultureIgnoreCase)
            .Take(5)
            .Select(item => new VehicleHistoryItemViewModel(
                item.Item.Id,
                FormatValue(item.Item.EventDate, L("Common.NoDate")),
                FormatValue(item.Item.EventType, L("Projection.Value.NoType")),
                FormatValue(item.Item.Odometer, L("Projection.Value.NoOdometer")),
                FormatValue(item.Item.Cost, L("Projection.Value.NoPrice")),
                FormatValue(item.Item.Note, L("Common.NoNote"))))
            .ToList();

    private string BuildVehicleEvidenceSummary(VehimapDataSet dataSet, string vehicleId)
    {
        var historyCount = dataSet.HistoryEntries.Count(item => item.VehicleId == vehicleId);
        var fuelCount = dataSet.FuelEntries.Count(item => item.VehicleId == vehicleId);
        var recordCount = dataSet.Records.Count(item => item.VehicleId == vehicleId);
        var reminderCount = dataSet.Reminders.Count(item => item.VehicleId == vehicleId);
        var maintenanceCount = dataSet.MaintenancePlans.Count(item => item.VehicleId == vehicleId);
        var activeMaintenanceCount = dataSet.MaintenancePlans.Count(item => item.VehicleId == vehicleId && item.IsActive);

        return LF("VehicleDetail.Projection.EvidenceSummary", historyCount, fuelCount, recordCount, reminderCount, maintenanceCount, activeMaintenanceCount);
    }

    private IReadOnlyList<VehicleDetailEvidenceSummaryItemViewModel> BuildVehicleEvidenceSummaryItems(
        VehimapDataSet dataSet,
        string vehicleId,
        VehimapDataRoot? dataRoot,
        Func<string, string>? managedPathResolver,
        DateOnly today,
        int? currentOdometer)
    {
        return
        [
            new VehicleDetailEvidenceSummaryItemViewModel(L("VehicleDetail.Section.History"), BuildVehicleHistoryDetailSummary(dataSet, vehicleId)),
            new VehicleDetailEvidenceSummaryItemViewModel(L("VehicleDetail.Section.Fuel"), BuildVehicleFuelDetailSummary(dataSet, vehicleId)),
            new VehicleDetailEvidenceSummaryItemViewModel(L("VehicleDetail.Section.Reminders"), BuildVehicleReminderDetailSummary(dataSet, vehicleId, today)),
            new VehicleDetailEvidenceSummaryItemViewModel(L("VehicleDetail.Section.Records"), BuildVehicleRecordDetailSummary(dataRoot, dataSet, vehicleId, managedPathResolver)),
            new VehicleDetailEvidenceSummaryItemViewModel(L("VehicleDetail.Section.Maintenance"), BuildVehicleMaintenanceDetailSummary(dataSet, vehicleId, today, currentOdometer))
        ];
    }

    private string BuildVehicleHistoryDetailSummary(VehimapDataSet dataSet, string vehicleId)
    {
        var entries = dataSet.HistoryEntries
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryParseEventDate(item.EventDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Item.EventType, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        if (entries.Count == 0)
        {
            return L("VehicleDetail.Projection.History.Empty");
        }

        var latest = entries[0].Item;
        var summary = LF(
            "VehicleDetail.Projection.History.Latest",
            entries.Count,
            FormatValue(latest.EventType, L("Projection.Value.NoType")),
            FormatValue(latest.EventDate, L("Common.NoDate")));
        if (!string.IsNullOrWhiteSpace(latest.Odometer))
        {
            summary += " " + LF("VehicleDetail.Projection.History.Odometer", FormatOdometerValue(latest.Odometer));
        }

        return summary;
    }

    private string BuildVehicleFuelDetailSummary(VehimapDataSet dataSet, string vehicleId)
    {
        var entries = dataSet.FuelEntries
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryParseEventDate(item.EntryDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Item.FuelType, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => item.Item)
            .ToList();

        if (entries.Count == 0)
        {
            return L("VehicleDetail.Projection.Fuel.Empty");
        }

        var summary = LF("VehicleDetail.Projection.Fuel.CountAndOdometer", entries.Count, FormatOdometerValue(entries[0].Odometer));
        var latestFuelEntry = entries.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item.Liters) || !string.IsNullOrWhiteSpace(item.TotalCost));
        if (latestFuelEntry is not null)
        {
            var volume = string.IsNullOrWhiteSpace(latestFuelEntry.Liters)
                ? L("Projection.Value.NoFuelVolumeData")
                : FormatFuelVolume(latestFuelEntry.Liters);
            if (!string.IsNullOrWhiteSpace(latestFuelEntry.TotalCost))
            {
                summary += " " + LF(
                    "VehicleDetail.Projection.Fuel.LatestWithCost",
                    volume,
                    FormatCostValue(latestFuelEntry.TotalCost),
                    FormatValue(latestFuelEntry.EntryDate, L("Common.NoDate")));
            }
            else
            {
                summary += " " + LF(
                    "VehicleDetail.Projection.Fuel.Latest",
                    volume,
                    FormatValue(latestFuelEntry.EntryDate, L("Common.NoDate")));
            }
        }

        return summary;
    }

    private string BuildVehicleReminderDetailSummary(VehimapDataSet dataSet, string vehicleId, DateOnly today)
    {
        var entries = dataSet.Reminders
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = TryParseReminderDate(item.DueDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenBy(item => item.Date)
            .ThenBy(item => item.Item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => item.Item)
            .ToList();

        if (entries.Count == 0)
        {
            return L("VehicleDetail.Projection.Reminder.Empty");
        }

        var nearest = entries[0];
        var status = BuildReminderStatus(nearest, today);
        var details = new List<string>
        {
            FormatValue(nearest.DueDate, L("Projection.Value.NoDueDate"))
        };
        if (!string.IsNullOrWhiteSpace(status))
        {
            details.Add(status);
        }

        var repeatLabel = FormatReminderRepeatMode(nearest.RepeatMode);
        if (!string.Equals(repeatLabel, L("Reminder.Repeat.NoneLegacy"), StringComparison.CurrentCultureIgnoreCase)
            && !string.Equals(repeatLabel, L("Reminder.Repeat.None"), StringComparison.CurrentCultureIgnoreCase))
        {
            details.Add(repeatLabel);
        }

        return LF(
            "VehicleDetail.Projection.Reminder.Nearest",
            entries.Count,
            FormatValue(nearest.Title, L("Projection.Value.NoTitle")),
            string.Join(", ", details));
    }

    private string BuildVehicleRecordDetailSummary(
        VehimapDataRoot? dataRoot,
        VehimapDataSet dataSet,
        string vehicleId,
        Func<string, string>? managedPathResolver)
    {
        var entries = dataSet.Records
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryParseMonthYear(item.ValidTo, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenBy(item => item.Date)
            .ThenBy(item => item.Item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => item.Item)
            .ToList();

        if (entries.Count == 0)
        {
            return L("VehicleDetail.Projection.Record.Empty");
        }

        var missingPathCount = 0;
        var emptyPathCount = 0;
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.FilePath))
            {
                emptyPathCount++;
                continue;
            }

            var resolvedPath = ResolveRecordPath(dataRoot, entry, managedPathResolver ?? (_ => string.Empty));
            if (string.IsNullOrWhiteSpace(resolvedPath) || !File.Exists(resolvedPath))
            {
                missingPathCount++;
            }
        }

        var summary = LF("VehicleDetail.Projection.Record.Count", entries.Count);
        var nearestRecord = entries.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item.ValidTo));
        if (nearestRecord is not null)
        {
            var title = FormatValue(nearestRecord.Title, L("Projection.Value.NoTitle"));
            if (!string.IsNullOrWhiteSpace(nearestRecord.Provider))
            {
                title = LF("VehicleDetail.Projection.Record.TitleWithProvider", title, nearestRecord.Provider);
            }

            summary += " " + LF("VehicleDetail.Projection.Record.NearestValidity", title, nearestRecord.ValidTo);
        }
        else
        {
            summary += " " + L("VehicleDetail.Projection.Record.NoValidity");
        }

        if (missingPathCount > 0)
        {
            summary += " " + LF("VehicleDetail.Projection.Record.MissingAttachments", missingPathCount);
        }

        if (emptyPathCount > 0)
        {
            summary += " " + LF("VehicleDetail.Projection.Record.EmptyPaths", emptyPathCount);
        }

        return summary;
    }

    private string BuildVehicleMaintenanceDetailSummary(
        VehimapDataSet dataSet,
        string vehicleId,
        DateOnly today,
        int? currentOdometer)
    {
        var plans = dataSet.MaintenancePlans
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Plan = item,
                Status = BuildMaintenanceStatus(item, today, currentOdometer)
            })
            .OrderBy(item => BuildMaintenanceStatusPriority(item.Status))
            .ThenBy(item => item.Plan.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        if (plans.Count == 0)
        {
            return L("VehicleDetail.Projection.Maintenance.Empty");
        }

        var activeCount = plans.Count(item => item.Plan.IsActive);
        var pausedCount = plans.Count - activeCount;
        var summary = LF("VehicleDetail.Projection.Maintenance.Count", plans.Count, activeCount);
        if (pausedCount > 0)
        {
            summary += " " + LF("VehicleDetail.Projection.Maintenance.Paused", pausedCount);
        }

        if (activeCount == 0)
        {
            return summary + " " + L("VehicleDetail.Projection.Maintenance.AllPaused");
        }

        var nextPlan = plans.FirstOrDefault(item => item.Plan.IsActive);
        if (nextPlan is not null)
        {
            summary += " " + LF(
                "VehicleDetail.Projection.Maintenance.Nearest",
                FormatValue(nextPlan.Plan.Title, L("Projection.Value.NoTitle")),
                nextPlan.Status);
        }

        return summary;
    }

    private static int BuildMaintenanceStatusPriority(string status)
    {
        if (status.Contains("Po termínu", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("Po limitu", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("Overdue", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("Over the limit", StringComparison.CurrentCultureIgnoreCase))
        {
            return 0;
        }

        if (status.Contains("dnes", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("nyní", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("today", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("now", StringComparison.CurrentCultureIgnoreCase))
        {
            return 1;
        }

        if (status.StartsWith("Za ", StringComparison.CurrentCultureIgnoreCase)
            || status.StartsWith("In ", StringComparison.CurrentCultureIgnoreCase))
        {
            return 2;
        }

        if (status.Contains("Chybí", StringComparison.CurrentCultureIgnoreCase)
            || status.Contains("Missing", StringComparison.CurrentCultureIgnoreCase))
        {
            return 3;
        }

        return 4;
    }

    private string BuildReminderStatus(VehicleReminder reminder, DateOnly today)
    {
        if (!TryParseReminderDate(reminder.DueDate, out var dueDate))
        {
            return L("Reminder.Status.NoUsableDate");
        }

        var delta = dueDate.DayNumber - today.DayNumber;
        if (delta < 0)
        {
            return LF("Reminder.Status.Overdue", Math.Abs(delta));
        }

        if (delta == 0)
        {
            return L("Reminder.Status.Today");
        }

        return delta == 1 ? L("Reminder.Status.Tomorrow") : LF("Reminder.Status.InDays", delta);
    }

    private static bool TryParseReminderDate(string? text, out DateOnly value)
    {
        return VehimapValueParser.TryParseEventDate(text, out value)
            || VehimapValueParser.TryParseMonthYear(text, out value);
    }

    private string BuildMaintenanceInterval(MaintenancePlan plan)
    {
        var parts = new List<string>();
        if (TryParsePositiveInteger(plan.IntervalKm, out var intervalKm))
        {
            parts.Add(FormatDistance(intervalKm, decimalPlaces: 0));
        }

        if (TryParsePositiveInteger(plan.IntervalMonths, out var intervalMonths))
        {
            parts.Add(intervalMonths == 1
                ? L("Maintenance.Interval.OneMonth")
                : LF("Maintenance.Interval.Months", intervalMonths));
        }

        return parts.Count == 0 ? L("Maintenance.Interval.None") : string.Join(" / ", parts);
    }

    private string BuildMaintenanceLastService(MaintenancePlan plan)
    {
        var date = string.IsNullOrWhiteSpace(plan.LastServiceDate) ? L("Common.NoDate") : plan.LastServiceDate;
        return $"{date} | {FormatOdometerValue(plan.LastServiceOdometer)}";
    }

    private string BuildMaintenanceStatus(MaintenancePlan plan, DateOnly today, int? currentOdometer)
    {
        if (!plan.IsActive)
        {
            return L("Maintenance.Status.Inactive");
        }

        var parts = new List<string>();

        if (TryParsePositiveInteger(plan.IntervalMonths, out var intervalMonths))
        {
            if (TryParseReminderDate(plan.LastServiceDate, out var lastServiceDate))
            {
                var nextDate = lastServiceDate.AddMonths(intervalMonths);
                var delta = nextDate.DayNumber - today.DayNumber;
                if (delta < 0)
                {
                    parts.Add(LF("Maintenance.Status.Overdue", Math.Abs(delta)));
                }
                else if (delta == 0)
                {
                    parts.Add(L("Maintenance.Status.Today"));
                }
                else
                {
                    parts.Add(delta == 1 ? L("Maintenance.Status.InOneDay") : LF("Maintenance.Status.InDays", delta));
                }
            }
            else
            {
                parts.Add(L("Maintenance.Status.MissingLastServiceDate"));
            }
        }

        if (TryParsePositiveInteger(plan.IntervalKm, out var intervalKm))
        {
            if (VehimapValueParser.TryParseOdometer(plan.LastServiceOdometer, out var lastServiceOdometer) && currentOdometer.HasValue)
            {
                var remainingKm = (lastServiceOdometer + intervalKm) - currentOdometer.Value;
                if (remainingKm < 0)
                {
                    parts.Add(LF("Maintenance.Status.OverDistanceLimit", FormatDistance(Math.Abs(remainingKm), decimalPlaces: 0)));
                }
                else if (remainingKm == 0)
                {
                    parts.Add(L("Maintenance.Status.Now"));
                }
                else
                {
                    parts.Add(LF("Maintenance.Status.InDistance", FormatDistance(remainingKm, decimalPlaces: 0)));
                }
            }
            else
            {
                parts.Add(L("Maintenance.Status.MissingOdometer"));
            }
        }

        return parts.Count == 0 ? L("Maintenance.Status.NoActiveInterval") : string.Join(" | ", parts);
    }

    private static bool TryParsePositiveInteger(string? text, out int value)
    {
        value = 0;
        return int.TryParse((text ?? string.Empty).Trim(), out value) && value > 0;
    }

    private string FormatReminderRepeatMode(string? repeatMode) =>
        string.IsNullOrWhiteSpace(repeatMode) ? L("Reminder.Repeat.None") : repeatMode;

    private bool MatchesTimelineFilter(VehicleTimelineItem item, string selectedFilter)
    {
        return NormalizeTimelineFilterKey(selectedFilter) switch
        {
            "future" => item.IsFuture,
            "past" => !item.IsFuture,
            _ => true
        };
    }

    private string NormalizeTimelineFilterKey(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.Equals(normalized, "future", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, L("TimelineWorkspace.Filter.Future"), StringComparison.CurrentCultureIgnoreCase)
            || string.Equals(normalized, "Budoucí", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "Future", StringComparison.OrdinalIgnoreCase))
        {
            return "future";
        }

        if (string.Equals(normalized, "past", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, L("TimelineWorkspace.Filter.Past"), StringComparison.CurrentCultureIgnoreCase)
            || string.Equals(normalized, "Minulé", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "Past", StringComparison.OrdinalIgnoreCase))
        {
            return "past";
        }

        return "all";
    }

    private static bool MatchesTimelineSearch(VehicleTimelineItem item, string? searchText)
    {
        var needle = searchText?.Trim();
        if (string.IsNullOrWhiteSpace(needle))
        {
            return true;
        }

        var haystack = string.Join(' ', new[]
        {
            item.DateText,
            item.KindLabel,
            item.Title,
            item.Detail,
            item.Status,
            item.Note,
            item.VehicleName
        });

        return haystack.Contains(needle, StringComparison.CurrentCultureIgnoreCase);
    }

    private string BuildVehicleStatusSummary(
        Vehicle vehicle,
        VehicleMeta? meta,
        IReadOnlyCollection<AuditItem> audit,
        IReadOnlyList<VehicleTimelineItem> timelineItems)
    {
        var parts = new List<string>();
        var normalizedState = NormalizeVehicleState(meta?.State);
        if (!string.IsNullOrWhiteSpace(normalizedState) && !string.Equals(normalizedState, L("Projection.Value.NormalOperation"), StringComparison.CurrentCulture))
        {
            parts.Add(normalizedState);
        }

        if (TryGetTimelineAttention(timelineItems, "technical", out var technicalStatus))
        {
            parts.Add(LF("VehicleList.Status.Technical", technicalStatus));
        }

        if (TryGetTimelineAttention(timelineItems, "green", out var greenCardStatus))
        {
            parts.Add(LF("VehicleList.Status.GreenCard", greenCardStatus));
        }

        if (TryGetTimelineAttention(timelineItems, "custom", out var reminderStatus))
        {
            parts.Add(LF("VehicleList.Status.Reminder", reminderStatus));
        }

        if (TryGetTimelineAttention(timelineItems, "maintenance", out var maintenanceStatus))
        {
            parts.Add(LF("VehicleList.Status.Maintenance", maintenanceStatus));
        }

        if (string.IsNullOrWhiteSpace(vehicle.GreenCardTo))
        {
            parts.Add(L("VehicleList.Status.MissingGreenCard"));
        }

        var attentionCount = audit.Count(item => item.VehicleId == vehicle.Id);
        if (attentionCount > 0)
        {
            parts.Add(LF("VehicleList.Status.AttentionCount", attentionCount));
        }

        return parts.Count == 0 ? L("VehicleList.Status.Ok") : string.Join(" | ", parts);
    }

    private string FormatCostValue(string? value)
    {
        if (VehimapValueParser.TryParseMoney(value, out var parsed))
        {
            return FormatMoney(parsed);
        }

        return FormatValue(value, L("Projection.Value.NoPrice"));
    }

    private string FormatFuelVolume(string? value)
    {
        if (!VehimapValueParser.TryParseDecimalNumber(value, out var parsed))
        {
            return FormatValue(value, L("Projection.Value.NoQuantity"));
        }

        return FormatVolume(parsed);
    }

    private string FormatOdometerValue(string? value)
    {
        if (!VehimapValueParser.TryParseOdometer(value, out var parsed))
        {
            return FormatValue(value, L("Projection.Value.NoOdometer"));
        }

        return FormatDistance(parsed, decimalPlaces: 0);
    }

    private string FormatCurrentOdometer(int? value) =>
        value.HasValue ? FormatDistance(value.Value, decimalPlaces: 0) : L("Projection.Value.UnknownOdometer");

    private string FormatMoney(decimal value) =>
        _numberFormatService.FormatMoney(value, _culturePreferences, _currency);

    private string FormatDistance(int? value) =>
        value.HasValue
            ? FormatDistance((decimal)value.Value)
            : L("Cost.Value.Unavailable");

    private string FormatDistance(decimal kilometers, int decimalPlaces = 1) =>
        _unitFormatService.FormatDistanceFromKilometers(kilometers, _culturePreferences, _unitPreferences, decimalPlaces);

    private string FormatCostPerDistance(decimal? value) =>
        value.HasValue
            ? FormatCostPerDistance(value.Value)
            : L("Cost.Value.Unavailable");

    private string FormatCostPerDistance(decimal value)
    {
        var normalized = _unitFormatService.Normalize(_unitPreferences);
        var costPerDisplayedDistance = value * _unitFormatService.ConvertDistanceToKilometers(1m, normalized);
        return LF("Cost.Value.CostPerDistance", FormatMoney(costPerDisplayedDistance), DistanceUnitLabel(normalized));
    }

    private string FormatSignedMoney(decimal value)
    {
        return value >= 0m
            ? LF("Cost.Value.SignedMoney.Positive", FormatMoney(value))
            : LF("Cost.Value.SignedMoney.Negative", FormatMoney(Math.Abs(value)));
    }

    private string FormatSignedCostPerDistance(decimal? value)
    {
        if (!value.HasValue)
        {
            return L("Cost.Value.Unavailable");
        }

        var normalized = _unitFormatService.Normalize(_unitPreferences);
        var absoluteValue = Math.Abs(value.Value) * _unitFormatService.ConvertDistanceToKilometers(1m, normalized);
        return value.Value >= 0m
            ? LF("Cost.Value.SignedCostPerDistance.Positive", FormatMoney(absoluteValue), DistanceUnitLabel(normalized))
            : LF("Cost.Value.SignedCostPerDistance.Negative", FormatMoney(absoluteValue), DistanceUnitLabel(normalized));
    }

    private bool IsInactiveCostStatus(string? status) =>
        string.Equals(status, L("CostAnalysis.Status.Inactive"), StringComparison.CurrentCultureIgnoreCase)
        || string.Equals(status, "Neaktivní", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "Inactive", StringComparison.OrdinalIgnoreCase);

    private string FormatSmartAdvisorPriority(SmartAdvisorPriority priority) =>
        priority switch
        {
            SmartAdvisorPriority.Critical => L("SmartAdvisor.Priority.Critical"),
            SmartAdvisorPriority.Warning => L("SmartAdvisor.Priority.Warning"),
            SmartAdvisorPriority.Recommendation => L("SmartAdvisor.Priority.Recommendation"),
            _ => L("SmartAdvisor.Priority.Info")
        };

    private string FormatSmartAdvisorCategory(SmartAdvisorCategory category) =>
        category switch
        {
            SmartAdvisorCategory.Data => L("SmartAdvisor.Category.Data"),
            SmartAdvisorCategory.Deadlines => L("SmartAdvisor.Category.Deadlines"),
            SmartAdvisorCategory.Maintenance => L("SmartAdvisor.Category.Maintenance"),
            SmartAdvisorCategory.Fuel => L("SmartAdvisor.Category.Fuel"),
            SmartAdvisorCategory.Attachments => L("SmartAdvisor.Category.Attachments"),
            SmartAdvisorCategory.Costs => L("SmartAdvisor.Category.Costs"),
            _ => L("SmartAdvisor.Category.Other")
        };

    private string FormatOptionalConsumption(decimal? value) =>
        value.HasValue ? FormatFuelAnalysisConsumption(value.Value) : L("FuelAnalysis.Value.ConsumptionUnavailable");

    private string FormatOptionalPricePerVolume(decimal? value)
    {
        if (!value.HasValue)
        {
            return L("FuelAnalysis.Value.PricePerLiterUnavailable");
        }

        var normalized = _unitFormatService.Normalize(_unitPreferences);
        var pricePerDisplayedVolume = value.Value * _unitFormatService.ConvertVolumeToLiters(1m, normalized);
        return LF("FuelAnalysis.Value.PricePerVolume", FormatMoney(pricePerDisplayedVolume), VolumeUnitLabel(normalized));
    }

    private string FormatFuelAnalysisMoney(decimal value) => FormatMoney(value);

    private string FormatFuelAnalysisVolume(decimal value) => FormatVolume(value);

    private string FormatVolume(decimal liters)
    {
        var normalized = _unitFormatService.Normalize(_unitPreferences);
        var displayedVolume = _unitFormatService.ConvertVolumeFromLiters(liters, normalized);
        var decimalPlaces = displayedVolume == Math.Round(displayedVolume, 0) ? 0 : 2;
        return $"{FormatDecimal(displayedVolume, decimalPlaces)} {VolumeUnitLabel(normalized)}";
    }

    private string FormatFuelAnalysisConsumption(decimal litersPer100Km)
    {
        var normalized = _unitFormatService.Normalize(_unitPreferences);
        var distanceUnit = DistanceUnitLabel(normalized);
        var volumeUnit = VolumeUnitLabel(normalized);

        if (normalized.DistanceUnit == AppUnitFormatService.Miles
            && normalized.VolumeUnit is AppUnitFormatService.UsGallons or AppUnitFormatService.ImperialGallons)
        {
            var kilometersPerDisplayedVolume = _unitFormatService.ConvertVolumeToLiters(1m, normalized) * 100m / litersPer100Km;
            var milesPerDisplayedVolume = _unitFormatService.ConvertDistanceFromKilometers(kilometersPerDisplayedVolume, normalized);
            var mpgLabel = normalized.VolumeUnit == AppUnitFormatService.ImperialGallons ? "mpg (imp)" : "mpg";
            return $"{FormatDecimal(milesPerDisplayedVolume, 2)} {mpgLabel}";
        }

        var kilometersPer100DisplayedDistance = _unitFormatService.ConvertDistanceToKilometers(100m, normalized);
        var litersPer100DisplayedDistance = litersPer100Km * kilometersPer100DisplayedDistance / 100m;
        var volumePer100DisplayedDistance = _unitFormatService.ConvertVolumeFromLiters(litersPer100DisplayedDistance, normalized);
        return LF("FuelAnalysis.Value.ConsumptionPerDistance", FormatDecimal(volumePer100DisplayedDistance, 2), volumeUnit, distanceUnit);
    }

    private string FormatDecimal(decimal value, int decimalPlaces) =>
        _numberFormatService.FormatDecimal(value, _culturePreferences, decimalPlaces);

    private static string DistanceUnitLabel(AppUnitPreferences preferences) =>
        preferences.DistanceUnit == AppUnitFormatService.Miles ? "mi" : "km";

    private static string VolumeUnitLabel(AppUnitPreferences preferences) =>
        preferences.VolumeUnit switch
        {
            AppUnitFormatService.UsGallons => "US gal",
            AppUnitFormatService.ImperialGallons => "imp gal",
            _ => "l"
        };

    private string FormatConsumptionSegmentPeriod(FuelConsumptionSegment segment) =>
        LF(
            "FuelAnalysis.Value.SegmentPeriod",
            segment.StartDate.ToString("d", _formatCulture),
            segment.EndDate.ToString("d", _formatCulture));

    private string BuildFuelGroupLabel(string fuelType, string fuelDetail)
    {
        if (string.IsNullOrWhiteSpace(fuelDetail) || string.Equals(fuelDetail, L("FuelAnalysis.Group.UnknownFuelDetail"), StringComparison.CurrentCultureIgnoreCase))
        {
            return fuelType;
        }

        return $"{fuelType} | {fuelDetail}";
    }

    private string FormatFuelAnalysisWarningSeverity(FuelAnalysisWarningSeverity severity) =>
        severity switch
        {
            FuelAnalysisWarningSeverity.Error => L("FuelAnalysis.Severity.Error"),
            FuelAnalysisWarningSeverity.Warning => L("FuelAnalysis.Severity.Warning"),
            _ => L("FuelAnalysis.Severity.Info")
        };

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);

    private static string FormatValue(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;

    private string BuildVehicleListSummary(int visibleCount, int totalCount, DesktopVehicleListFilters filters)
    {
        if (totalCount == 0)
        {
            return L("VehicleList.Summary.Empty");
        }

        var filterParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(filters.SelectedCategory)
            && !string.Equals(filters.SelectedCategory, MainWindowViewModel.AllVehicleCategoriesLabel, StringComparison.Ordinal))
        {
            filterParts.Add(LF("VehicleList.Filter.Category", filters.SelectedCategory));
        }

        if (!string.IsNullOrWhiteSpace(filters.StatusFilter)
            && !string.Equals(filters.StatusFilter, MainWindowViewModel.AllVehicleStatusFilterLabel, StringComparison.Ordinal))
        {
            filterParts.Add(filters.StatusFilter);
        }

        if (filters.HideInactiveVehicles)
        {
            filterParts.Add(L("VehicleList.Filter.HideInactive"));
        }

        if (!string.IsNullOrWhiteSpace(filters.SearchText))
        {
            filterParts.Add(LF("VehicleList.Filter.Search", filters.SearchText.Trim()));
        }

        return filterParts.Count == 0
            ? LF("VehicleList.Summary.All", visibleCount)
            : LF("VehicleList.Summary.Filtered", visibleCount, totalCount, string.Join(" | ", filterParts));
    }

    private static bool MatchesVehicleCategory(Vehicle vehicle, string? selectedCategory)
    {
        return string.IsNullOrWhiteSpace(selectedCategory)
            || string.Equals(selectedCategory, MainWindowViewModel.AllVehicleCategoriesLabel, StringComparison.Ordinal)
            || string.Equals(vehicle.Category, selectedCategory, StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool MatchesVehicleSearch(Vehicle vehicle, VehicleMeta? meta, string statusSummary, string? searchText)
    {
        var needle = (searchText ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(needle))
        {
            return true;
        }

        var haystacks = new[]
        {
            vehicle.Name,
            vehicle.VehicleNote,
            vehicle.MakeModel,
            vehicle.Plate,
            vehicle.Category,
            vehicle.LastTk,
            vehicle.NextTk,
            vehicle.GreenCardFrom,
            vehicle.GreenCardTo,
            meta?.State,
            meta?.Tags,
            meta?.Powertrain,
            meta?.ClimateProfile,
            meta?.TimingDrive,
            meta?.Transmission,
            statusSummary
        };

        return haystacks.Any(haystack =>
            !string.IsNullOrWhiteSpace(haystack)
            && haystack.Contains(needle, StringComparison.CurrentCultureIgnoreCase));
    }

    private bool MatchesVehicleStatusFilter(Vehicle vehicle, IReadOnlyList<VehicleTimelineItem> timelineItems, string? statusFilter)
    {
        return statusFilter switch
        {
            MainWindowViewModel.AttentionVehicleStatusFilterLabel => HasVehicleAttention(timelineItems),
            MainWindowViewModel.OverdueVehicleStatusFilterLabel => HasVehicleOverdueTerm(timelineItems),
            MainWindowViewModel.MissingGreenVehicleStatusFilterLabel => string.IsNullOrWhiteSpace(vehicle.GreenCardTo),
            _ => true
        };
    }

    private bool HasVehicleAttention(IReadOnlyList<VehicleTimelineItem> timelineItems)
    {
        return timelineItems.Any(item =>
            IsVehicleStatusTimelineItem(item.Kind)
            && !string.IsNullOrWhiteSpace(item.Status)
            && !IsNoWarningStatus(item.Status));
    }

    private bool HasVehicleOverdueTerm(IReadOnlyList<VehicleTimelineItem> timelineItems)
    {
        return timelineItems.Any(item =>
            IsVehicleStatusTimelineItem(item.Kind)
            && !item.IsFuture
            && !string.IsNullOrWhiteSpace(item.Status)
            && IsOverdueStatus(item.Status));
    }

    private bool TryGetTimelineAttention(IReadOnlyList<VehicleTimelineItem> timelineItems, string kind, out string status)
    {
        status = timelineItems
            .Where(item => string.Equals(item.Kind, kind, StringComparison.Ordinal))
            .Select(item => item.Status)
            .FirstOrDefault(item =>
                !string.IsNullOrWhiteSpace(item)
                && !IsNoWarningStatus(item))
            ?? string.Empty;

        return !string.IsNullOrWhiteSpace(status);
    }

    private bool IsNoWarningStatus(string status) =>
        string.Equals(status, L("Projection.Value.NoWarning"), StringComparison.CurrentCultureIgnoreCase)
        || string.Equals(status, "Bez upozornění", StringComparison.CurrentCultureIgnoreCase)
        || string.Equals(status, "No warning", StringComparison.CurrentCultureIgnoreCase);

    private static bool IsOverdueStatus(string status) =>
        status.Contains("Po termínu", StringComparison.CurrentCultureIgnoreCase)
        || status.Contains("Overdue", StringComparison.CurrentCultureIgnoreCase);

    private static bool IsVehicleStatusTimelineItem(string kind) =>
        kind is "technical" or "green" or "custom" or "maintenance";

    private static bool IsVehicleInactive(VehicleMeta? meta)
    {
        var normalizedState = NormalizeVehicleState(meta?.State);
        return string.Equals(normalizedState, "Archiv", StringComparison.CurrentCultureIgnoreCase)
            || string.Equals(normalizedState, "Odstaveno", StringComparison.CurrentCultureIgnoreCase);
    }

    private static string NormalizeVehicleState(string? state) =>
        string.IsNullOrWhiteSpace(state) ? "Běžný provoz" : state.Trim();
}

internal sealed record DesktopVehicleDetailProjection(
    string Heading,
    string Overview,
    string Dates,
    string Profile,
    string EvidenceSummary,
    string RecentHistorySummary,
    IReadOnlyList<VehicleDetailEvidenceSummaryItemViewModel> EvidenceSummaries,
    IReadOnlyList<VehicleHistoryItemViewModel> RecentHistory);

internal sealed record DesktopListProjection<TItem>(
    IReadOnlyList<TItem> Items,
    string Summary);

internal sealed record DesktopFuelAnalysisProjection(
    string Summary,
    IReadOnlyList<FuelConsumptionSegmentItemViewModel> ConsumptionSegments,
    IReadOnlyList<FuelGroupSummaryItemViewModel> GroupSummaries,
    IReadOnlyList<FuelAnalysisWarningItemViewModel> Warnings);

internal sealed record DesktopVehicleListFilters(
    string SearchText,
    string SelectedCategory,
    string StatusFilter,
    bool HideInactiveVehicles);
