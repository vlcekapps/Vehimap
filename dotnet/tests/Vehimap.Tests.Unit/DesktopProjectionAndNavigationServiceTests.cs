// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Threading;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopProjectionAndNavigationServiceTests
{
    [Fact]
    public void Navigation_coordinator_routes_timeline_record_to_record_tab()
    {
        var coordinator = new DesktopNavigationCoordinator();
        var item = new VehicleTimelineItemViewModel(
            "record",
            "Doklad",
            "08/2099",
            "Asistence",
            "Platnost dokladu",
            string.Empty,
            "Octavia",
            "veh_1",
            "rec_2",
            true,
            string.Empty);

        var plan = coordinator.BuildForTimeline(item);

        Assert.Equal("veh_1", plan.VehicleId);
        Assert.Equal(DesktopTabIndexes.Record, plan.TabIndex);
        Assert.Equal(DesktopFocusTarget.RecordList, plan.FocusTarget);
        Assert.Equal(DesktopNavigationSelectionKind.Record, plan.SelectionKind);
        Assert.Equal("rec_2", plan.EntityId);
    }

    [Fact]
    public void Navigation_coordinator_routes_entity_reminder_to_reminder_tab()
    {
        var coordinator = new DesktopNavigationCoordinator();

        var plan = coordinator.BuildForEntity("veh_1", "Připomínka", "rem_1");

        Assert.Equal(DesktopTabIndexes.Reminder, plan.TabIndex);
        Assert.Equal(DesktopFocusTarget.ReminderList, plan.FocusTarget);
        Assert.Equal(DesktopNavigationSelectionKind.Reminder, plan.SelectionKind);
        Assert.Equal("rem_1", plan.EntityId);
    }

    [Fact]
    public void Projection_service_builds_managed_record_with_resolved_path_and_available_state()
    {
        var projectionService = new DesktopProjectionService();
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-projection-tests", Guid.NewGuid().ToString("N"));
        var dataRoot = new VehimapDataRoot(tempRoot, Path.Combine(tempRoot, "data"), true);
        Directory.CreateDirectory(Path.Combine(dataRoot.DataPath, "attachments", "veh_1"));
        var managedFile = Path.Combine(dataRoot.DataPath, "attachments", "veh_1", "pojisteni.pdf");
        File.WriteAllText(managedFile, "test");

        var dataSet = new VehimapDataSet
        {
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Pojištění", "", "05/2025", "05/2026", "2000", VehicleRecordAttachmentMode.Managed, @"attachments/veh_1/pojisteni.pdf", "Platný doklad")
            ]
        };

        var projection = projectionService.BuildRecords(
            dataRoot,
            dataSet,
            "veh_1",
            relativePath => Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));

        var item = Assert.Single(projection.Items);
        Assert.Equal("Spravovaná kopie", item.AttachmentMode);
        Assert.Equal("Soubor dostupný", item.AttachmentState);
        Assert.Equal(managedFile, item.ResolvedPath);
        Assert.True(item.FileExists);
        Assert.Contains("1 dokladů", projection.Summary);
    }

    [Fact]
    public void Projection_service_localizes_vehicle_list_detail_and_records()
    {
        var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US"));
        var projectionService = new DesktopProjectionService(localizer, CultureInfo.GetCultureInfo("en-US"));
        projectionService.ApplySupportedSettings(new DesktopSupportedSettingsSnapshot(
            30,
            15,
            30,
            1000,
            false,
            false,
            false,
            false,
            1,
            30,
            "en-US",
            "comma",
            "dot",
            "mi",
            "us_gal",
            "USD"));
        var timelineService = new LegacyTimelineService(localizer);
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-projection-localization-tests", Guid.NewGuid().ToString("N"));
        var dataRoot = new VehimapDataRoot(tempRoot, Path.Combine(tempRoot, "data"), true);
        Directory.CreateDirectory(Path.Combine(dataRoot.DataPath, "attachments", "veh_1"));
        var managedFile = Path.Combine(dataRoot.DataPath, "attachments", "veh_1", "insurance.pdf");
        File.WriteAllText(managedFile, "test");

        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Cars", "Family car", "Skoda 120L", "", "1988", "43", "", "08/2026", "05/2025", "")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "", "", "", "", "", "")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "01.04.2026", "Service", "10000", "1000", "")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "02.04.2026", "10050", "3.12", "350", true, "Gasoline", "", "Natural 95", "Shell")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "", "", "", "", "05/2026", "2000", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/insurance.pdf", "")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Call service", "10.04.2026", "", "", "")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("mnt_1", "veh_1", "Oil service", "1609", "", "", "10000", true, "")
            ]
        };

        var vehicleList = projectionService.BuildVehicleList(
            dataSet,
            dataSet.VehicleMetaEntries.ToDictionary(item => item.VehicleId, StringComparer.Ordinal),
            [],
            timelineService,
            new DesktopVehicleListFilters(string.Empty, MainWindowViewModel.AllVehicleCategoriesLabel, MainWindowViewModel.AllVehicleStatusFilterLabel, false),
            new DateOnly(2026, 4, 3));

        var vehicle = Assert.Single(vehicleList.Items);
        Assert.Equal("No license plate", vehicle.Plate);
        Assert.Contains("Green card missing", vehicle.StatusSummary, StringComparison.Ordinal);
        Assert.DoesNotContain("ZK chybí", vehicle.StatusSummary, StringComparison.Ordinal);
        Assert.Equal("Vehicle list: 1 vehicles.", vehicleList.Summary);

        var detail = projectionService.BuildVehicleDetail(
            dataSet,
            vehicle,
            dataSet.VehicleMetaEntries.Single(),
            dataRoot,
            relativePath => Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar)),
            new DateOnly(2026, 4, 3));

        Assert.Contains("State: Normal operation", detail.Overview, StringComparison.Ordinal);
        Assert.Contains("Related records: history 1, fuel 1, documents 1, reminders 1, maintenance plans 1, active 1.", detail.EvidenceSummary, StringComparison.Ordinal);
        Assert.Contains("History", detail.EvidenceSummaries.Select(item => item.Title));
        Assert.Contains("Fuel", detail.EvidenceSummaries.Select(item => item.Title));
        Assert.Contains("Documents", detail.EvidenceSummaries.Select(item => item.Title));
        Assert.Contains("Maintenance", detail.EvidenceSummaries.Select(item => item.Title));

        Assert.Equal("The selected vehicle has 1 history entries.", projectionService.BuildHistory(dataSet, "veh_1").Summary);
        Assert.Equal("The selected vehicle has 1 fuel entries.", projectionService.BuildFuel(dataSet, "veh_1").Summary);
        Assert.Equal("Full tank", projectionService.BuildFuel(dataSet, "veh_1").Items.Single().TankState);
        Assert.Equal("The selected vehicle has 1 reminders.", projectionService.BuildReminders(dataSet, "veh_1", new DateOnly(2026, 4, 3)).Summary);
        Assert.Equal("The selected vehicle has 1 maintenance plans.", projectionService.BuildMaintenance(dataSet, "veh_1", new DateOnly(2026, 4, 3)).Summary);

        var records = projectionService.BuildRecords(
            dataRoot,
            dataSet,
            "veh_1",
            relativePath => Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));

        var record = Assert.Single(records.Items);
        Assert.Equal("Document", record.RecordType);
        Assert.Equal("Untitled", record.Title);
        Assert.Equal("Managed copy", record.AttachmentMode);
        Assert.Equal("File available", record.AttachmentState);
        Assert.Equal("The selected vehicle has 1 documents. Select an entry to open the file or its folder.", records.Summary);
    }

    [Fact]
    public void Projection_service_filters_timeline_by_future_and_search_text()
    {
        var projectionService = new DesktopProjectionService();
        var timelineService = new LegacyTimelineService();
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Octavia", "Osobní vozidla", "Rodinné auto", "Škoda Octavia", "1AB2345", "2020", "110", "", "05/2027", "05/2025", "05/2026")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Objednat servis", "01.12.2099", "30", "Ročně", "Zavolat servisu"),
                new VehicleReminder("rem_2", "veh_1", "Starý termín", "01.01.2020", "30", "Ročně", "Historie")
            ]
        };

        var projection = projectionService.BuildTimeline(
            dataSet,
            timelineService,
            "veh_1",
            new DateOnly(2026, 4, 2),
            "Budoucí",
            "servis");

        var item = Assert.Single(projection.Items);
        Assert.Equal("rem_1", item.EntryId);
        Assert.Contains("Po filtru zobrazeno: 1", projection.Summary);
    }

    [Fact]
    public void Projection_service_builds_accessible_fuel_analysis_items()
    {
        var projectionService = new DesktopProjectionService();
        var analysis = new FuelAnalysisSummary(
            "veh_1",
            2,
            82m,
            4100m,
            50m,
            8.2m,
            new FuelConsumptionSegment(
                "segment_1",
                "fuel_1",
                "fuel_2",
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 1, 15),
                10000,
                10500,
                500,
                41m,
                2050m,
                8.2m,
                50m,
                4.1m),
            new FuelConsumptionSegment(
                "segment_1",
                "fuel_1",
                "fuel_2",
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 1, 15),
                10000,
                10500,
                500,
                41m,
                2050m,
                8.2m,
                50m,
                4.1m),
            "Spotřeba je spočítaná z 1 použitelného úseku.",
            [
                new FuelConsumptionSegment(
                    "segment_1",
                    "fuel_1",
                    "fuel_2",
                    new DateOnly(2026, 1, 1),
                    new DateOnly(2026, 1, 15),
                    10000,
                    10500,
                    500,
                    41m,
                    2050m,
                    8.2m,
                    50m,
                    4.1m)
            ],
            [
                new FuelGroupSummary("group_1", "fuel_2", "Shell", "Natural 95", "FuelSave", 2, 82m, 4100m, 50m, new DateOnly(2026, 1, 15))
            ],
            [
                new FuelAnalysisWarning("warn_1", "fuel_2", FuelAnalysisWarningSeverity.Info, "Kontrola", "Upozornění pro test.")
            ]);

        var originalCulture = Thread.CurrentThread.CurrentCulture;
        var originalUiCulture = Thread.CurrentThread.CurrentUICulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

        DesktopFuelAnalysisProjection projection;
        try
        {
            projection = projectionService.BuildFuelAnalysis(analysis);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUiCulture;
        }

        Assert.Contains("Průměrná spotřeba: 8,20 l/100 km", projection.Summary, StringComparison.Ordinal);
        var segment = Assert.Single(projection.ConsumptionSegments);
        Assert.Equal("fuel_2", segment.FuelEntryId);
        Assert.Contains("Úsek spotřeby", segment.AccessibleLabel, StringComparison.Ordinal);
        var group = Assert.Single(projection.GroupSummaries);
        Assert.Equal("Shell", group.Station);
        Assert.Contains("Natural 95", group.AccessibleLabel, StringComparison.Ordinal);
        var warning = Assert.Single(projection.Warnings);
        Assert.Equal("Info", warning.Severity);
        Assert.Contains("související tankování", warning.AccessibleLabel, StringComparison.Ordinal);
    }

    [Fact]
    public void Projection_service_formats_costs_with_selected_currency()
    {
        var projectionService = new DesktopProjectionService(
            new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US")),
            CultureInfo.GetCultureInfo("en-US"));
        projectionService.ApplySupportedSettings(new DesktopSupportedSettingsSnapshot(
            30,
            30,
            31,
            1000,
            false,
            false,
            false,
            false,
            1,
            30,
            "en-US",
            "comma",
            "dot",
            "mi",
            "us_gal",
            "USD"));
        var summary = new CostAnalysisSummary(
            "From 1/1/2026 to 12/31/2026",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            700m,
            150,
            4.6667m,
            0m,
            5.9167m,
            700m,
            -1.25m,
            1,
            0,
            0,
            [
                new VehicleCostBreakdown("veh_1", "Milena", "Cars", 350m, 150m, 200m, 700m, 150, 4.6667m, "Calculated")
            ]);

        var item = Assert.Single(projectionService.BuildDashboardCostVehicles(summary));

        Assert.Equal("$350.00", item.FuelCost);
        Assert.Equal("$700.00", item.TotalCost);
        Assert.Equal("93.2 mi", item.Distance);
        Assert.Equal("$7.51/mi", item.CostPerKm);
        Assert.Contains("cost per distance $7.51/mi", item.AccessibleLabel, StringComparison.Ordinal);
        Assert.Contains("$700.00", projectionService.BuildCostSummary(summary), StringComparison.Ordinal);
        Assert.Contains("-$2.01/mi", projectionService.BuildCostComparison(summary), StringComparison.Ordinal);
    }

    [Fact]
    public void Projection_service_formats_maintenance_distance_status_with_selected_unit()
    {
        var projectionService = new DesktopProjectionService(
            new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US")),
            CultureInfo.GetCultureInfo("en-US"));
        projectionService.ApplySupportedSettings(new DesktopSupportedSettingsSnapshot(
            30,
            15,
            30,
            1000,
            false,
            false,
            false,
            false,
            1,
            30,
            "en-US",
            "comma",
            "dot",
            "mi",
            "us_gal",
            "USD"));
        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Cars", "Family car", "Skoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "01.04.2026", "Service", "10000", "", "")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("mnt_1", "veh_1", "Oil service", "1609", "", "", "10000", true, "")
            ]
        };

        var projection = projectionService.BuildMaintenance(dataSet, "veh_1", new DateOnly(2026, 4, 2));

        var item = Assert.Single(projection.Items);
        Assert.Contains("1,000 mi", item.Status, StringComparison.Ordinal);
        Assert.DoesNotContain(" km", item.Status, StringComparison.Ordinal);
    }

    [Fact]
    public void Projection_service_localizes_fuel_analysis_summary_and_accessible_labels()
    {
        var projectionService = new DesktopProjectionService(
            new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US")),
            CultureInfo.GetCultureInfo("en-US"));
        projectionService.ApplySupportedSettings(new DesktopSupportedSettingsSnapshot(
            30,
            15,
            30,
            1000,
            false,
            false,
            false,
            false,
            1,
            30,
            "en-US",
            "comma",
            "dot",
            "mi",
            "us_gal",
            "USD"));
        var analysis = new FuelAnalysisSummary(
            "veh_1",
            2,
            82m,
            4100m,
            50m,
            8.2m,
            new FuelConsumptionSegment(
                "segment_1",
                "fuel_1",
                "fuel_2",
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 1, 15),
                10000,
                10500,
                500,
                41m,
                2050m,
                8.2m,
                50m,
                4.1m),
            null,
            "Consumption is calculated from 1 usable segment between full tanks.",
            [
                new FuelConsumptionSegment(
                    "segment_1",
                    "fuel_1",
                    "fuel_2",
                    new DateOnly(2026, 1, 1),
                    new DateOnly(2026, 1, 15),
                    10000,
                    10500,
                    500,
                    41m,
                    2050m,
                    8.2m,
                    50m,
                    4.1m)
            ],
            [
                new FuelGroupSummary("group_1", "fuel_2", "Shell", "Natural 95", "FuelSave", 2, 82m, 4100m, 50m, new DateOnly(2026, 1, 15))
            ],
            [
                new FuelAnalysisWarning("warn_1", "fuel_2", FuelAnalysisWarningSeverity.Warning, "Check", "Warning for test.")
            ]);

        var projection = projectionService.BuildFuelAnalysis(analysis);

        Assert.Contains("Refuel entries: 2", projection.Summary, StringComparison.Ordinal);
        Assert.Contains("Total fuel: 21.66 US gal", projection.Summary, StringComparison.Ordinal);
        Assert.Contains("Average price per fuel unit: $189.27/US gal", projection.Summary, StringComparison.Ordinal);
        Assert.Contains("Average consumption: 28.68 mpg", projection.Summary, StringComparison.Ordinal);
        Assert.Equal("310.7 mi", projection.ConsumptionSegments.Single().Distance);
        Assert.Equal("10.83 US gal", projection.ConsumptionSegments.Single().Liters);
        Assert.Equal("$6.60/mi", projection.ConsumptionSegments.Single().CostPerKm);
        Assert.Contains("Consumption segment", projection.ConsumptionSegments.Single().AccessibleLabel, StringComparison.Ordinal);
        Assert.Contains("fuel 21.66 US gal", projection.GroupSummaries.Single().AccessibleLabel, StringComparison.Ordinal);
        Assert.Equal("Warning", projection.Warnings.Single().Severity);
        Assert.Contains("related refuel entry", projection.Warnings.Single().AccessibleLabel, StringComparison.Ordinal);
    }

    [Fact]
    public void Projection_service_localizes_audit_severity_summary_and_accessible_labels()
    {
        var projectionService = new DesktopProjectionService(
            new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US")),
            CultureInfo.GetCultureInfo("en-US"));
        var audit = new[]
        {
            new AuditItem(
                AuditSeverity.Error,
                "Vehicle",
                "veh_1",
                "Milena",
                "Vozidlo",
                "veh_1",
                "Missing license plate",
                "The active vehicle has no license plate filled in."),
            new AuditItem(
                AuditSeverity.Warning,
                "Costs",
                "veh_1",
                "Milena",
                "Doklad",
                "rec_1",
                "Missing usable date",
                "The document has a price but no usable date for cost analysis.")
        };

        var items = projectionService.BuildAuditItems(audit);

        Assert.Equal("Error", items[0].Severity);
        Assert.Equal("Warning", items[1].Severity);
        Assert.Contains("Error, Milena, Missing license plate", items[0].AccessibleLabel, StringComparison.Ordinal);
        Assert.Equal(
            "There are 2 items to resolve: 1 errors and 1 warnings.",
            projectionService.BuildAuditSummary(audit));
        Assert.Equal(
            "Data audit has not found any issues that need action.",
            projectionService.BuildAuditSummary(Array.Empty<AuditItem>()));
    }

    [Fact]
    public void Projection_service_localizes_smart_advisor_priority_category_and_due_date()
    {
        var projectionService = new DesktopProjectionService(
            new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US")),
            CultureInfo.GetCultureInfo("en-US"));
        var summary = new SmartAdvisorSummary(
            2,
            1,
            1,
            0,
            "Smart advisor found 2 items: 1 critical, 1 warnings.",
            [
                new SmartAdvisorItem(
                    "advisor_1",
                    SmartAdvisorPriority.Critical,
                    SmartAdvisorCategory.Attachments,
                    "veh_1",
                    "Milena",
                    "Doklad",
                    "rec_1",
                    "Missing managed attachment",
                    "The document attachment file is not available.",
                    "Data audit: Attachment. The document attachment file is not available.",
                    "Open document",
                    new DateOnly(2026, 7, 2)),
                new SmartAdvisorItem(
                    "advisor_2",
                    SmartAdvisorPriority.Recommendation,
                    SmartAdvisorCategory.Costs,
                    "veh_1",
                    "Milena",
                    "Náklady",
                    "veh_1",
                    "Cost per distance is not available",
                    "The vehicle has costs.",
                    "Add odometers.",
                    "Open vehicle costs",
                    null)
            ]);

        var projection = projectionService.BuildSmartAdvisor(summary);

        Assert.Equal("Critical", projection.Items[0].Priority);
        Assert.Equal("Attachments", projection.Items[0].Category);
        Assert.Equal("7/2/2026", projection.Items[0].DueDate);
        Assert.Equal("Recommendation", projection.Items[1].Priority);
        Assert.Equal("Costs", projection.Items[1].Category);
        Assert.Equal("no due date", projection.Items[1].DueDate);
    }
}
