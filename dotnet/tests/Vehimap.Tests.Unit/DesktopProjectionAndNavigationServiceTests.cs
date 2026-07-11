// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Threading;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;
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

    [Theory]
    [InlineData(DesktopEntityKinds.Reminder)]
    [InlineData("Připomínka")]
    public void Navigation_coordinator_routes_entity_reminder_to_reminder_tab(string entityKind)
    {
        var coordinator = new DesktopNavigationCoordinator();

        var plan = coordinator.BuildForEntity("veh_1", entityKind, "rem_1");

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
        DesktopLocalization.Configure(new AppCulturePreferences("en-US", "comma", "dot"));
        try
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
                new Vehicle("veh_1", "Testovací vozidlo", "Osobní vozidla", "Rodinné auto z garáže", "Škoda 120L", "", "1988", "43", "", "08/2026", "05/2025", "")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "Veterán", "veterán; rodina", "Benzín", "Má klimatizaci", "Řemen", "Manuální")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "01.04.2026", "Service", "10000", "1000", "")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "02.04.2026", "10050", "3.12", "350", true, "Benzin", "", "Natural 95", "Shell")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "", "", "", "05/2026", "2000", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/insurance.pdf", "")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Call service", "10.04.2026", "", "Každý rok", "")
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
        Assert.Equal("Passenger vehicles", vehicle.Category);
        Assert.Equal("Veteran", vehicle.State);
        Assert.Equal("Gasoline", vehicle.Powertrain);
        Assert.Equal("No license plate", vehicle.Plate);
        Assert.Contains("license plate", vehicle.AccessibleLabel, StringComparison.Ordinal);
        Assert.Contains("state Veteran", vehicle.AccessibleLabel, StringComparison.Ordinal);
        Assert.DoesNotContain("SPZ", vehicle.AccessibleLabel, StringComparison.Ordinal);
        Assert.DoesNotContain("stav", vehicle.AccessibleLabel, StringComparison.Ordinal);
        Assert.Contains("Testovací vozidlo", vehicle.AccessibleLabel, StringComparison.Ordinal);
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

        Assert.Equal("Testovací vozidlo", detail.Heading);
        Assert.Contains("Škoda 120L | Passenger vehicles | No license plate", detail.Overview, StringComparison.Ordinal);
        Assert.Contains("State: Veteran", detail.Overview, StringComparison.Ordinal);
        Assert.Contains("Tags: veterán; rodina", detail.Overview, StringComparison.Ordinal);
        Assert.Contains("Note: Rodinné auto z garáže", detail.Overview, StringComparison.Ordinal);
        Assert.Contains("Next technical inspection: 08/2026", detail.Dates, StringComparison.Ordinal);
        Assert.Contains("Green card until: not filled", detail.Dates, StringComparison.Ordinal);
        Assert.Contains("Status summary:", detail.Dates, StringComparison.Ordinal);
        Assert.Contains("Green card missing", detail.Dates, StringComparison.Ordinal);
        Assert.DoesNotContain("Příští TK", detail.Dates, StringComparison.Ordinal);
        Assert.DoesNotContain("Zelená karta", detail.Dates, StringComparison.Ordinal);
        Assert.Contains("Powertrain: Gasoline", detail.Profile, StringComparison.Ordinal);
        Assert.Contains("Climate: Has air conditioning", detail.Profile, StringComparison.Ordinal);
        Assert.Contains("Timing drive: Belt", detail.Profile, StringComparison.Ordinal);
        Assert.Contains("Transmission: Manual", detail.Profile, StringComparison.Ordinal);
        Assert.Contains("Related records: history 1, fuel 1, documents 1, reminders 1, maintenance plans 1, active 1.", detail.EvidenceSummary, StringComparison.Ordinal);
        Assert.Contains("History", detail.EvidenceSummaries.Select(item => item.Title));
        Assert.Contains("Fuel", detail.EvidenceSummaries.Select(item => item.Title));
        Assert.Contains("Documents", detail.EvidenceSummaries.Select(item => item.Title));
        Assert.Contains("Maintenance", detail.EvidenceSummaries.Select(item => item.Title));

        Assert.Equal("The selected vehicle has 1 history entries.", projectionService.BuildHistory(dataSet, "veh_1").Summary);
        Assert.Equal("The selected vehicle has 1 fuel entries.", projectionService.BuildFuel(dataSet, "veh_1").Summary);
        var fuel = Assert.Single(projectionService.BuildFuel(dataSet, "veh_1").Items);
        Assert.Equal("Gasoline", fuel.FuelType);
        Assert.Equal("Full tank", fuel.TankState);
        Assert.Equal("The selected vehicle has 1 reminders.", projectionService.BuildReminders(dataSet, "veh_1", new DateOnly(2026, 4, 3)).Summary);
        Assert.Equal("Every year", projectionService.BuildReminders(dataSet, "veh_1", new DateOnly(2026, 4, 3)).Items.Single().RepeatMode);
        Assert.Equal("The selected vehicle has 1 maintenance plans.", projectionService.BuildMaintenance(dataSet, "veh_1", new DateOnly(2026, 4, 3)).Summary);

        var records = projectionService.BuildRecords(
            dataRoot,
            dataSet,
            "veh_1",
            relativePath => Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));

        var record = Assert.Single(records.Items);
        Assert.Equal("Liability insurance", record.RecordType);
        Assert.Equal("Untitled", record.Title);
        Assert.Equal("Managed copy", record.AttachmentMode);
        Assert.Equal("File available", record.AttachmentState);
        Assert.Equal("The selected vehicle has 1 documents. Select an entry to open the file or its folder.", records.Summary);
        }
        finally
        {
            TestCultureInitializer.ResetToCzech();
        }
    }

    [Fact]
    public void Accessible_item_labels_follow_active_language_and_keep_user_data_raw()
    {
        try
        {
            DesktopLocalization.Configure(new AppCulturePreferences("en-US", "comma", "dot"));

            var vehicle = new VehicleListItemViewModel(
                "veh_1",
                "Testovací vozidlo",
                "Passenger vehicles",
                "No license plate",
                "Škoda 120L",
                "Rodinné auto",
                "08/2026",
                "05/2026",
                "Veteran",
                "Gasoline",
                "Green card missing");
            Assert.Contains("license plate No license plate", vehicle.AccessibleLabel, StringComparison.Ordinal);
            Assert.Contains("state Veteran", vehicle.AccessibleLabel, StringComparison.Ordinal);
            Assert.DoesNotContain("SPZ", vehicle.AccessibleLabel, StringComparison.Ordinal);
            Assert.DoesNotContain("stav", vehicle.AccessibleLabel, StringComparison.Ordinal);
            Assert.Contains("Testovací vozidlo", vehicle.AccessibleLabel, StringComparison.Ordinal);

            var timeline = new VehicleTimelineItemViewModel(
                "technical",
                "Technical inspection",
                "08/2026",
                "Next technical inspection",
                "Škoda 120L",
                "Upcoming",
                "Testovací vozidlo",
                "veh_1",
                "veh_1",
                true,
                string.Empty);
            Assert.Contains("status Upcoming", timeline.AccessibleLabel, StringComparison.Ordinal);
            Assert.DoesNotContain("stav", timeline.AccessibleLabel, StringComparison.Ordinal);

            var advisor = new SmartAdvisorItemViewModel(
                "advisor_1",
                "Critical",
                "Attachments",
                "Testovací vozidlo",
                "veh_1",
                "Doklad",
                "rec_1",
                "Missing attachment",
                "Attachment is not available.",
                "Open the document record.",
                "Open document",
                "no due date",
                1);
            Assert.Contains("Action: Open document", advisor.AccessibleLabel, StringComparison.Ordinal);
            Assert.DoesNotContain("Akce:", advisor.AccessibleLabel, StringComparison.Ordinal);

            var search = new GlobalSearchResultItemViewModel(
                "veh_1",
                "vehicle",
                "veh_1",
                "Testovací vozidlo",
                "Documents",
                "Liability insurance",
                "Valid until 05/2026");
            Assert.Contains("vehicle Testovací vozidlo", search.AccessibleLabel, StringComparison.Ordinal);
            Assert.DoesNotContain("vozidlo Testovací vozidlo", search.AccessibleLabel, StringComparison.Ordinal);

            DesktopLocalization.Configure(new AppCulturePreferences("cs-CZ", "none", "comma"));

            Assert.Contains("SPZ Bez SPZ", new VehicleListItemViewModel(
                "veh_1",
                "Testovací vozidlo",
                "Osobní vozidla",
                "Bez SPZ",
                "Škoda 120L",
                "Rodinné auto",
                "08/2026",
                "05/2026",
                "Veterán",
                "Benzín",
                "ZK chybí").AccessibleLabel, StringComparison.Ordinal);
            Assert.Contains("stav Nadcházející", new VehicleTimelineItemViewModel(
                "technical",
                "Technická kontrola",
                "08/2026",
                "Příští TK",
                "Škoda 120L",
                "Nadcházející",
                "Testovací vozidlo",
                "veh_1",
                "veh_1",
                true,
                string.Empty).AccessibleLabel, StringComparison.Ordinal);
            Assert.Contains("Akce: Otevřít doklad", new SmartAdvisorItemViewModel(
                "advisor_1",
                "Kritická",
                "Přílohy",
                "Testovací vozidlo",
                "veh_1",
                "Doklad",
                "rec_1",
                "Chybí příloha",
                "Soubor přílohy není dostupný.",
                "Otevřete evidenci dokladů.",
                "Otevřít doklad",
                "bez termínu",
                1).AccessibleLabel, StringComparison.Ordinal);
            Assert.Contains("vozidlo Testovací vozidlo", new GlobalSearchResultItemViewModel(
                "veh_1",
                "vehicle",
                "veh_1",
                "Testovací vozidlo",
                "Doklady",
                "Povinné ručení",
                "Platné do 05/2026").AccessibleLabel, StringComparison.Ordinal);
        }
        finally
        {
            TestCultureInitializer.ResetToCzech();
        }
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
        Assert.Equal("$7.51/mi", item.CostPerDistance);
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
    public void Projection_service_formats_full_dates_for_selected_ui_language()
    {
        var dataSet = new VehimapDataSet
        {
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "30.04.2026", "Service", "10000", "100", "")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "30.04.2026", "10000", "40", "100", true, "Gasoline", "")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Service", "30.04.2026", "7", "Do not repeat", "")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("mnt_1", "veh_1", "Service", "", "12", "30.04.2026", "10000", true, "")
            ]
        };

        var english = new DesktopProjectionService(
            new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US")),
            CultureInfo.GetCultureInfo("en-US"));
        english.ApplySupportedSettings(CreateSettings("en-US", "comma", "dot", "mi", "us_gal", "USD"));

        var czech = new DesktopProjectionService(
            new ResourceAppLocalizer(CultureInfo.GetCultureInfo("cs-CZ")),
            CultureInfo.GetCultureInfo("cs-CZ"));
        czech.ApplySupportedSettings(CreateSettings("cs-CZ", "none", "comma", "km", "l", "CZK"));

        Assert.Equal("4/30/2026", english.BuildHistory(dataSet, "veh_1").Items.Single().Date);
        Assert.Equal("4/30/2026", english.BuildFuel(dataSet, "veh_1").Items.Single().Date);
        Assert.Equal("4/30/2026", english.BuildReminders(dataSet, "veh_1", new DateOnly(2026, 4, 1)).Items.Single().DueDate);
        Assert.StartsWith("4/30/2026", english.BuildMaintenance(dataSet, "veh_1", new DateOnly(2026, 4, 1)).Items.Single().LastService, StringComparison.Ordinal);

        Assert.Equal("30.04.2026", czech.BuildHistory(dataSet, "veh_1").Items.Single().Date);
        Assert.Equal("30.04.2026", czech.BuildFuel(dataSet, "veh_1").Items.Single().Date);
        Assert.Equal("30.04.2026", czech.BuildReminders(dataSet, "veh_1", new DateOnly(2026, 4, 1)).Items.Single().DueDate);
        Assert.StartsWith("30.04.2026", czech.BuildMaintenance(dataSet, "veh_1", new DateOnly(2026, 4, 1)).Items.Single().LastService, StringComparison.Ordinal);
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
        Assert.Equal("10.83 US gal", projection.ConsumptionSegments.Single().FuelAmount);
        Assert.Equal("$6.60/mi", projection.ConsumptionSegments.Single().CostPerDistance);
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
        DesktopLocalization.Configure(new AppCulturePreferences("en-US", "comma", "dot"));
        try
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
        Assert.Contains("Action: Open document", projection.Items[0].AccessibleLabel, StringComparison.Ordinal);
        Assert.DoesNotContain("Akce:", projection.Items[0].AccessibleLabel, StringComparison.Ordinal);
        Assert.Equal("Recommendation", projection.Items[1].Priority);
        Assert.Equal("Costs", projection.Items[1].Category);
        Assert.Equal("no due date", projection.Items[1].DueDate);
        }
        finally
        {
            TestCultureInitializer.ResetToCzech();
        }
    }

    [Fact]
    public void English_ui_conformance_gate_localizes_core_outputs_over_czech_legacy_data()
    {
        DesktopLocalization.Configure(new AppCulturePreferences("en-US", "comma", "dot"));
        try
        {
            var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo("en-US"));
            var settings = new DesktopSupportedSettingsSnapshot(
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
                "USD");
            var projectionService = new DesktopProjectionService(localizer, CultureInfo.GetCultureInfo("en-US"));
            projectionService.ApplySupportedSettings(settings);
            var timelineService = new LegacyTimelineService(localizer);
            timelineService.ApplySupportedSettings(settings);
            var fuelAnalysisService = new LegacyFuelAnalysisService(localizer);
            fuelAnalysisService.ApplySupportedSettings(settings);
            var serviceBookService = new LegacyServiceBookService(localizer);
            serviceBookService.ApplySupportedSettings(settings);
            var costService = new LegacyCostAnalysisService(localizer);
            var attachmentService = new ProjectionAttachmentService();
            var auditService = new LegacyAuditService(attachmentService, localizer);
            var globalSearchService = new LegacyGlobalSearchService(attachmentService, timelineService, localizer);
            globalSearchService.ApplySupportedSettings(settings);
            var smartAdvisorService = new LegacySmartAdvisorService(timelineService, fuelAnalysisService, localizer);

            var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-i18n-gate-" + Guid.NewGuid().ToString("N"));
            var dataRoot = new VehimapDataRoot(tempRoot, Path.Combine(tempRoot, "data"), true);
            Directory.CreateDirectory(Path.Combine(dataRoot.DataPath, "attachments", "veh_1"));
            File.WriteAllText(Path.Combine(dataRoot.DataPath, "attachments", "veh_1", "service.pdf"), "test");
            var dataSet = new VehimapDataSet
            {
                Settings = new VehimapSettings(),
                Vehicles =
                [
                    new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto z garáže", "Škoda 120L", "", "1988", "43", "", "08/2026", "", "06/2026")
                ],
                VehicleMetaEntries =
                [
                    new VehicleMeta("veh_1", "Veterán", "veterán; rodina", "Benzín", "Má klimatizaci", "Řemen", "Manuální")
                ],
                HistoryEntries =
                [
                    new VehicleHistoryEntry("hist_1", "veh_1", "01.04.2026", "Servis garáže", "10000", "2500", "Olej a filtry")
                ],
                FuelEntries =
                [
                    new FuelEntry("fuel_1", "veh_1", "02.04.2026", "10100", "3.12", "350", true, "Benzin", "Poznámka řidiče", "Natural 95", "Shell")
                ],
                Records =
                [
                    new VehicleRecord("rec_1", "veh_1", "Servisní dokument", "Faktura z garáže", "Autoservis", "04/2026", "04/2026", "4000", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/service.pdf", "Práce mechanika")
                ],
                Reminders =
                [
                    new VehicleReminder("rem_1", "veh_1", "Zavolat servis", "10.04.2026", "30", "Každý rok", "Zeptat se na brzdy")
                ],
                MaintenancePlans =
                [
                    new MaintenancePlan("mnt_1", "veh_1", "Motorový olej", "1609", "12", "01.04.2025", "9000", true, "Syntetika")
                ]
            };
            dataSet.Settings.SetValue("app", "technical_reminder_days", "31");
            dataSet.Settings.SetValue("app", "green_card_reminder_days", "31");
            dataSet.Settings.SetValue("app", "maintenance_reminder_days", "31");
            dataSet.Settings.SetValue("app", "maintenance_reminder_km", "1000");

            var today = new DateOnly(2026, 4, 3);
            var meta = dataSet.VehicleMetaEntries.ToDictionary(item => item.VehicleId, StringComparer.Ordinal);
            var audit = auditService.BuildAudit(dataRoot, dataSet);
            var vehicleList = projectionService.BuildVehicleList(
                dataSet,
                meta,
                audit,
                timelineService,
                new DesktopVehicleListFilters(string.Empty, MainWindowViewModel.VehicleCategoryAllFilterKey, MainWindowViewModel.VehicleStatusAllFilterKey, false),
                today);
            var vehicle = Assert.Single(vehicleList.Items);
            var detail = projectionService.BuildVehicleDetail(
                dataSet,
                vehicle,
                dataSet.VehicleMetaEntries.Single(),
                dataRoot,
                relativePath => Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar)),
                today);
            var history = projectionService.BuildHistory(dataSet, "veh_1");
            var fuel = projectionService.BuildFuel(dataSet, "veh_1");
            var reminders = projectionService.BuildReminders(dataSet, "veh_1", today);
            var maintenance = projectionService.BuildMaintenance(dataSet, "veh_1", today);
            var records = projectionService.BuildRecords(
                dataRoot,
                dataSet,
                "veh_1",
                relativePath => Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            var timeline = projectionService.BuildTimeline(dataSet, timelineService, "veh_1", today, TimelineFilterOptions.AllKey, null);
            var dashboard = projectionService.BuildDashboardTimeline(dataSet, timelineService, today);
            var costSummary = costService.BuildPeriodSummary(dataSet, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));
            var serviceBook = serviceBookService.BuildVehicleServiceBook(dataSet, "veh_1", today);
            var search = globalSearchService.Search(dataRoot, dataSet, "Milena");
            var fuelAnalysis = projectionService.BuildFuelAnalysis(fuelAnalysisService.BuildVehicleFuelAnalysis(dataSet, "veh_1"));
            var advisor = projectionService.BuildSmartAdvisor(smartAdvisorService.BuildSmartAdvisor(dataSet, audit, costSummary, today));

            Assert.Equal("Passenger vehicles", vehicle.Category);
            Assert.Equal("Veteran", vehicle.State);
            Assert.Equal("Gasoline", vehicle.Powertrain);
            Assert.Contains("No license plate", vehicle.AccessibleLabel, StringComparison.Ordinal);
            Assert.Contains("Next technical inspection", detail.Dates, StringComparison.Ordinal);
            Assert.Contains("Green card", detail.Dates, StringComparison.Ordinal);
            Assert.Contains("Powertrain: Gasoline", detail.Profile, StringComparison.Ordinal);
            Assert.Contains("History", detail.EvidenceSummaries.Select(item => item.Title));
            Assert.Contains("Fuel", detail.EvidenceSummaries.Select(item => item.Title));
            Assert.Equal("The selected vehicle has 1 history entries.", history.Summary);
            Assert.Equal("Gasoline", fuel.Items.Single().FuelType);
            Assert.Equal("Full tank", fuel.Items.Single().TankState);
            Assert.Equal("Every year", reminders.Items.Single().RepeatMode);
            Assert.Contains("mi", maintenance.Items.Single().Interval, StringComparison.Ordinal);
            Assert.Equal("Service document", records.Items.Single().RecordType);
            Assert.Equal("Managed copy", records.Items.Single().AttachmentMode);
            Assert.Equal("File available", records.Items.Single().AttachmentState);
            Assert.Contains(timeline.Items, item => item.Title.Contains("Green card end", StringComparison.Ordinal));
            Assert.Contains(dashboard.Items, item => item.Title.Contains("Green card end", StringComparison.Ordinal));
            Assert.Contains("Warning", projectionService.BuildAuditItems(audit).Select(item => item.Severity));
            Assert.Contains("$", projectionService.BuildCostSummary(costSummary), StringComparison.Ordinal);
            Assert.Equal("Passenger vehicles", serviceBook.VehicleCategory);
            Assert.Equal("Service document", serviceBook.Records.Single().RecordType);
            Assert.Contains("mi", serviceBook.CurrentOdometer, StringComparison.Ordinal);
            Assert.Contains(search, item => item.SectionLabel == "Vehicle");
            Assert.Contains("US gal", fuelAnalysis.Summary, StringComparison.Ordinal);
            Assert.Contains(advisor.Items, item => item.Category is "Deadlines" or "Data");

            var userVisibleSystemTexts = new[]
            {
                vehicle.Category,
                vehicle.StatusSummary,
                vehicle.AccessibleLabel,
                detail.Overview,
                detail.Dates,
                detail.Profile,
                detail.EvidenceSummary,
                string.Join(" | ", detail.EvidenceSummaries.Select(item => $"{item.Title}: {item.Summary}")),
                history.Summary,
                fuel.Summary,
                fuel.Items.Single().AccessibleLabel,
                reminders.Summary,
                reminders.Items.Single().AccessibleLabel,
                maintenance.Summary,
                maintenance.Items.Single().AccessibleLabel,
                records.Summary,
                records.Items.Single().AccessibleLabel,
                timeline.Summary,
                string.Join(" | ", timeline.Items.Select(item => item.AccessibleLabel)),
                dashboard.Summary,
                string.Join(" | ", dashboard.Items.Select(item => item.AccessibleLabel)),
                projectionService.BuildAuditSummary(audit),
                string.Join(" | ", projectionService.BuildAuditItems(audit).Select(item => item.AccessibleLabel)),
                projectionService.BuildCostSummary(costSummary),
                projectionService.BuildCostComparison(costSummary),
                serviceBook.Status,
                serviceBook.VehicleCategory,
                serviceBook.CurrentOdometer,
                string.Join(" | ", serviceBook.HistoryEntries.Select(item => $"{item.Odometer} {item.Cost}")),
                string.Join(" | ", serviceBook.MaintenancePlans.Select(item => $"{item.Interval} {item.Status}")),
                string.Join(" | ", serviceBook.Records.Select(item => $"{item.RecordType} {item.AttachmentMode} {item.Price}")),
                string.Join(" | ", search.Select(item => $"{item.SectionLabel}: {item.Summary}")),
                fuelAnalysis.Summary,
                string.Join(" | ", fuelAnalysis.ConsumptionSegments.Select(item => item.AccessibleLabel)),
                advisor.Summary,
                string.Join(" | ", advisor.Items.Select(item => item.AccessibleLabel))
            };

            AssertNoCzechSystemText(userVisibleSystemTexts);
        }
        finally
        {
            TestCultureInitializer.ResetToCzech();
        }
    }

    private static DesktopSupportedSettingsSnapshot CreateSettings(
        string language,
        string thousandsSeparator,
        string decimalSeparator,
        string distanceUnit,
        string volumeUnit,
        string currency) =>
        new(
            30,
            30,
            30,
            1000,
            false,
            false,
            false,
            false,
            1,
            30,
            language,
            thousandsSeparator,
            decimalSeparator,
            distanceUnit,
            volumeUnit,
            currency);

    private static void AssertNoCzechSystemText(IEnumerable<string?> values)
    {
        var forbidden = new[]
        {
            "Osobní vozidla",
            "Bez SPZ",
            "stav ",
            "Příští TK",
            "Zelená karta",
            "ZK chybí",
            "Benzín",
            "Má klimatizaci",
            "Řemen",
            "Manuální",
            "Spravovaná kopie",
            "Externí cesta",
            "Soubor dostupný",
            "Plná nádrž",
            "Částečné tankování",
            "Povinné ručení",
            "Servisní dokument",
            "Každý rok",
            "Technická kontrola",
            "Po termínu",
            "Nadcházející",
            "Bez upozornění",
            "tachometr ",
            "cena ",
            "dokladů",
            "připomínek",
            "servisní plány"
        };

        foreach (var value in values.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            foreach (var text in forbidden)
            {
                Assert.DoesNotContain(text, value, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private sealed class ProjectionAttachmentService : IFileAttachmentService
    {
        public string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath) =>
            Path.Combine(dataRoot.DataPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
