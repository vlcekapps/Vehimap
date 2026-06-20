using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class WorkspaceCompositionTests
{
    [Fact]
    public void Main_window_initializes_shared_workspace_viewmodels()
    {
        var viewModel = CreateViewModel();

        Assert.NotNull(viewModel.VehicleDetailWorkspace);
        Assert.NotNull(viewModel.HistoryWorkspace);
        Assert.NotNull(viewModel.FuelWorkspace);
        Assert.NotNull(viewModel.ReminderWorkspace);
        Assert.NotNull(viewModel.MaintenanceWorkspace);
        Assert.NotNull(viewModel.TimelineWorkspace);
        Assert.NotNull(viewModel.RecordWorkspace);
        Assert.NotNull(viewModel.AuditWorkspace);
        Assert.NotNull(viewModel.CostWorkspace);
        Assert.NotNull(viewModel.DashboardWorkspace);
        Assert.NotNull(viewModel.GlobalSearchWorkspace);
        Assert.NotNull(viewModel.UpcomingOverviewWorkspace);
        Assert.NotNull(viewModel.OverdueOverviewWorkspace);
        Assert.Equal("Detail - Octavia", viewModel.VehicleDetailWorkspace.WindowTitle);
        Assert.Equal("Historie - Octavia", viewModel.HistoryWorkspace.WindowTitle);
        Assert.Equal("Tankování - Octavia", viewModel.FuelWorkspace.WindowTitle);
        Assert.Equal("Připomínky - Octavia", viewModel.ReminderWorkspace.WindowTitle);
        Assert.Equal("Údržba - Octavia", viewModel.MaintenanceWorkspace.WindowTitle);
        Assert.Equal("Doklady a přílohy - Octavia", viewModel.RecordWorkspace.WindowTitle);
        Assert.Equal("Časová osa - Octavia", viewModel.TimelineWorkspace.WindowTitle);
        Assert.Equal("Audit dat", viewModel.AuditWorkspace.WindowTitle);
        Assert.Equal("Náklady napříč vozidly", viewModel.CostWorkspace.WindowTitle);
        Assert.Equal("Dashboard", viewModel.DashboardWorkspace.WindowTitle);
        Assert.Equal("Globální hledání", viewModel.GlobalSearchWorkspace.WindowTitle);
        Assert.Equal("Blížící se termíny", viewModel.UpcomingOverviewWorkspace.WindowTitle);
        Assert.Equal("Propadlé termíny", viewModel.OverdueOverviewWorkspace.WindowTitle);
    }

    [Fact]
    public void Reminder_workspace_owns_selection_and_editing_state()
    {
        var viewModel = CreateViewModel();
        var reminder = Assert.Single(viewModel.ReminderWorkspace.SelectedVehicleReminders);

        viewModel.ReminderWorkspace.SelectedReminder = reminder;

        Assert.Same(reminder, viewModel.ReminderWorkspace.SelectedReminder);

        viewModel.ReminderWorkspace.EditSelectedReminderCommand.Execute(null);
        viewModel.ReminderWorkspace.ReminderEditorTitle = "Upravená připomínka";

        Assert.True(viewModel.ReminderWorkspace.IsEditingReminder);
        Assert.Equal("Upravená připomínka", viewModel.ReminderWorkspace.ReminderEditorTitle);
    }

    [Fact]
    public void Record_workspace_owns_selection_and_editing_state()
    {
        var viewModel = CreateViewModel();
        var record = viewModel.RecordWorkspace.SelectedVehicleRecords.First();

        viewModel.RecordWorkspace.SelectedRecord = record;

        Assert.Same(record, viewModel.RecordWorkspace.SelectedRecord);

        viewModel.RecordWorkspace.EditSelectedRecordCommand.Execute(null);
        viewModel.RecordWorkspace.RecordEditorTitle = "Upravený doklad";

        Assert.True(viewModel.RecordWorkspace.IsEditingRecord);
        Assert.Equal("Upravený doklad", viewModel.RecordWorkspace.RecordEditorTitle);
    }

    [Fact]
    public void Timeline_and_search_workspaces_own_state_and_collections()
    {
        var viewModel = CreateViewModel();

        viewModel.TimelineWorkspace.TimelineSearchText = "technická";
        viewModel.GlobalSearchWorkspace.GlobalSearchText = "Octavia";

        Assert.Equal("technická", viewModel.TimelineWorkspace.TimelineSearchText);
        Assert.Equal("Octavia", viewModel.GlobalSearchWorkspace.GlobalSearchText);
        Assert.NotEmpty(viewModel.TimelineWorkspace.SelectedVehicleTimeline);
        Assert.NotEmpty(viewModel.GlobalSearchWorkspace.GlobalSearchResults);
    }

    [Fact]
    public void Overview_workspaces_own_loaded_collections()
    {
        var viewModel = CreateViewModel();

        Assert.NotNull(viewModel.UpcomingOverviewWorkspace.UpcomingOverviewItems);
        Assert.NotNull(viewModel.OverdueOverviewWorkspace.OverdueOverviewItems);
    }

    [Fact]
    public void Audit_cost_and_dashboard_workspaces_own_loaded_collections()
    {
        var viewModel = CreateViewModel();

        Assert.NotNull(viewModel.AuditWorkspace.AuditItems);
        Assert.NotNull(viewModel.DashboardWorkspace.AuditItems);
        Assert.NotNull(viewModel.CostWorkspace.CostVehicles);
        Assert.Same(viewModel.CostWorkspace.CostVehicles, viewModel.DashboardWorkspace.CostVehicles);
        Assert.NotNull(viewModel.DashboardWorkspace.DashboardUpcomingTimeline);
    }

    [Fact]
    public void Workspace_option_lists_are_exposed_by_their_own_workspaces()
    {
        var viewModel = CreateViewModel();

        Assert.Contains("Vlastní období", viewModel.CostWorkspace.CostPeriodPresets);
        Assert.Contains("Datové nedostatky", viewModel.UpcomingOverviewWorkspace.OverviewFilters);
        Assert.DoesNotContain("Datové nedostatky", viewModel.OverdueOverviewWorkspace.OverviewFilters);
        Assert.Contains("Kabinový filtr", viewModel.MaintenanceWorkspace.MaintenanceTemplateOptions);
        Assert.Contains("Spravovaná kopie", viewModel.RecordWorkspace.RecordAttachmentModes);
    }

    [Fact]
    public void Dashboard_workspace_reads_shared_audit_cost_and_timeline_state()
    {
        var viewModel = CreateViewModel();
        var auditItem = new AuditItemViewModel("veh_1", "Doklad", "rec_1", "Vážné", "Doklady", "Octavia", "Doklad bez cesty", "Doplňte cestu.");
        var costVehicle = new CostVehicleItemViewModel("veh_1", "Octavia", "Osobní vozidla", "1 000 Kč", "0 Kč", "0 Kč", "1 000 Kč", "100 km", "10 Kč/km", "Vypočteno");
        var timelineItem = new VehicleTimelineItemViewModel("custom", "Připomínka", "01.12.2099", "Objednat servis", "Zavolat servisu", "Budoucí", "Octavia", "veh_1", "rem_1", true, string.Empty);

        viewModel.AuditWorkspace.SetAuditSummary("Audit sdílený přes workspace.");
        viewModel.CostWorkspace.CostSummary = "Náklady sdílené přes workspace.";
        viewModel.CostWorkspace.CostComparison = "Srovnání sdílené přes workspace.";
        viewModel.AuditWorkspace.SelectedDashboardAuditItem = auditItem;
        viewModel.CostWorkspace.SelectedDashboardCostVehicle = costVehicle;
        viewModel.DashboardWorkspace.SelectedDashboardTimelineItem = timelineItem;

        Assert.Equal("Audit sdílený přes workspace.", viewModel.DashboardWorkspace.AuditSummary);
        Assert.Equal("Náklady sdílené přes workspace.", viewModel.DashboardWorkspace.CostSummary);
        Assert.Equal("Srovnání sdílené přes workspace.", viewModel.DashboardWorkspace.CostComparison);
        Assert.Same(auditItem, viewModel.DashboardWorkspace.SelectedDashboardAuditItem);
        Assert.Same(costVehicle, viewModel.DashboardWorkspace.SelectedDashboardCostVehicle);
        Assert.Same(timelineItem, viewModel.DashboardWorkspace.SelectedDashboardTimelineItem);
    }

    [Fact]
    public void Evidence_workspaces_own_their_summary_state()
    {
        var viewModel = CreateViewModel();

        viewModel.HistoryWorkspace.HistorySummary = "Historie má vlastní souhrn.";
        viewModel.FuelWorkspace.FuelSummary = "Tankování má vlastní souhrn.";
        viewModel.ReminderWorkspace.ReminderSummary = "Připomínky mají vlastní souhrn.";
        viewModel.MaintenanceWorkspace.MaintenanceSummary = "Údržba má vlastní souhrn.";
        viewModel.RecordWorkspace.RecordSummary = "Doklady mají vlastní souhrn.";

        Assert.Equal("Historie má vlastní souhrn.", viewModel.HistoryWorkspace.HistorySummary);
        Assert.Equal("Tankování má vlastní souhrn.", viewModel.FuelWorkspace.FuelSummary);
        Assert.Equal("Připomínky mají vlastní souhrn.", viewModel.ReminderWorkspace.ReminderSummary);
        Assert.Equal("Údržba má vlastní souhrn.", viewModel.MaintenanceWorkspace.MaintenanceSummary);
        Assert.Equal("Doklady mají vlastní souhrn.", viewModel.RecordWorkspace.RecordSummary);
    }

    [Fact]
    public void Evidence_workspaces_own_their_loaded_collections()
    {
        var viewModel = CreateViewModel();

        Assert.NotEmpty(viewModel.HistoryWorkspace.SelectedVehicleHistory);
        Assert.NotEmpty(viewModel.FuelWorkspace.SelectedVehicleFuel);
        Assert.NotEmpty(viewModel.ReminderWorkspace.SelectedVehicleReminders);
        Assert.NotEmpty(viewModel.MaintenanceWorkspace.SelectedVehicleMaintenance);
        Assert.NotEmpty(viewModel.RecordWorkspace.SelectedVehicleRecords);
        Assert.Equal(
            viewModel.HistoryWorkspace.SelectedVehicleHistory.Count,
            viewModel.HistoryWorkspace.VisibleHistoryItems.Count);
        Assert.Equal(
            viewModel.FuelWorkspace.SelectedVehicleFuel.Count,
            viewModel.FuelWorkspace.VisibleFuelItems.Count);
        Assert.Equal(
            viewModel.ReminderWorkspace.SelectedVehicleReminders.Count,
            viewModel.ReminderWorkspace.VisibleReminderItems.Count);
        Assert.Equal(
            viewModel.MaintenanceWorkspace.SelectedVehicleMaintenance.Count,
            viewModel.MaintenanceWorkspace.VisibleMaintenanceItems.Count);
        Assert.Equal(
            viewModel.RecordWorkspace.SelectedVehicleRecords.Count,
            viewModel.RecordWorkspace.VisibleRecordItems.Count);
    }

    [Fact]
    public void Vehicle_detail_workspace_owns_display_summary_state()
    {
        var viewModel = CreateViewModel();

        viewModel.VehicleDetailWorkspace.SelectedVehicleHeading = "Detail má vlastní nadpis.";
        viewModel.VehicleDetailWorkspace.SelectedVehicleOverview = "Detail má vlastní přehled.";
        viewModel.VehicleDetailWorkspace.SelectedVehicleDates = "Detail má vlastní termíny.";
        viewModel.VehicleDetailWorkspace.SelectedVehicleProfile = "Detail má vlastní servisní profil.";
        viewModel.VehicleDetailWorkspace.SelectedVehicleEvidenceSummary = "Detail má vlastní souhrn evidencí.";
        viewModel.VehicleDetailWorkspace.SelectedVehicleRecentHistorySummary = "Detail má vlastní souhrn posledních událostí.";
        viewModel.VehicleDetailWorkspace.EvidenceSummaryItems.Clear();
        viewModel.VehicleDetailWorkspace.EvidenceSummaryItems.Add(new VehicleDetailEvidenceSummaryItemViewModel("Doklady", "Detail má vlastní souhrn dokladů."));
        viewModel.VehicleDetailWorkspace.RecentHistoryItems.Clear();
        viewModel.VehicleDetailWorkspace.RecentHistoryItems.Add(new VehicleHistoryItemViewModel("hist_1", "01.05.2026", "Servis", "12000 km", "1500 Kč", "Olej"));

        Assert.Equal("Detail má vlastní nadpis.", viewModel.VehicleDetailWorkspace.SelectedVehicleHeading);
        Assert.Equal("Detail má vlastní přehled.", viewModel.VehicleDetailWorkspace.SelectedVehicleOverview);
        Assert.Equal("Detail má vlastní termíny.", viewModel.VehicleDetailWorkspace.SelectedVehicleDates);
        Assert.Equal("Detail má vlastní servisní profil.", viewModel.VehicleDetailWorkspace.SelectedVehicleProfile);
        Assert.Equal("Detail má vlastní souhrn evidencí.", viewModel.VehicleDetailWorkspace.SelectedVehicleEvidenceSummary);
        Assert.Equal("Detail má vlastní souhrn posledních událostí.", viewModel.VehicleDetailWorkspace.SelectedVehicleRecentHistorySummary);
        Assert.Equal("Doklady", Assert.Single(viewModel.VehicleDetailWorkspace.EvidenceSummaryItems).Title);
        Assert.Equal("Servis", Assert.Single(viewModel.VehicleDetailWorkspace.RecentHistoryItems).EventType);
    }

    [Fact]
    public void Vehicle_detail_workspace_owns_vehicle_editor_form_state()
    {
        var viewModel = CreateViewModel();

        Assert.False(viewModel.VehicleDetailWorkspace.IsEditingVehicle);
        Assert.True(viewModel.VehicleDetailWorkspace.IsVehicleDetailVisible);
        Assert.Equal("Detail vozidla", viewModel.VehicleDetailWorkspace.VehiclePanelHeading);

        viewModel.CreateVehicleCommand.Execute(null);

        Assert.True(viewModel.VehicleDetailWorkspace.IsEditingVehicle);
        Assert.False(viewModel.VehicleDetailWorkspace.IsVehicleDetailVisible);
        Assert.Equal("Nové vozidlo", viewModel.VehicleDetailWorkspace.VehiclePanelHeading);

        viewModel.VehicleDetailWorkspace.VehicleEditorStatus = "Editor má vlastní stav.";
        viewModel.VehicleDetailWorkspace.VehicleEditorName = "Božena";
        viewModel.VehicleDetailWorkspace.VehicleEditorCategory = "Osobní vozidla";
        viewModel.VehicleDetailWorkspace.VehicleEditorNote = "Srazové";
        viewModel.VehicleDetailWorkspace.VehicleEditorMakeModel = "Škoda 100";
        viewModel.VehicleDetailWorkspace.VehicleEditorPlate = "2AB3456";
        viewModel.VehicleDetailWorkspace.VehicleEditorYear = "1973";
        viewModel.VehicleDetailWorkspace.VehicleEditorPower = "35";
        viewModel.VehicleDetailWorkspace.VehicleEditorLastTk = "05/2025";
        viewModel.VehicleDetailWorkspace.VehicleEditorNextTk = "05/2027";
        viewModel.VehicleDetailWorkspace.VehicleEditorGreenCardFrom = "05/2025";
        viewModel.VehicleDetailWorkspace.VehicleEditorGreenCardTo = "05/2026";
        viewModel.VehicleDetailWorkspace.VehicleEditorState = "Veterán";
        viewModel.VehicleDetailWorkspace.VehicleEditorTags = "srazové, veterán";
        viewModel.VehicleDetailWorkspace.VehicleEditorPowertrain = "Benzín";
        viewModel.VehicleDetailWorkspace.VehicleEditorClimateProfile = "Má klimatizaci";
        viewModel.VehicleDetailWorkspace.VehicleEditorTimingDrive = "Řemen";
        viewModel.VehicleDetailWorkspace.VehicleEditorTransmission = "Manuální";

        Assert.Equal("Editor má vlastní stav.", viewModel.VehicleDetailWorkspace.VehicleEditorStatus);
        Assert.Equal("Božena", viewModel.VehicleDetailWorkspace.VehicleEditorName);
        Assert.Equal("Osobní vozidla", viewModel.VehicleDetailWorkspace.VehicleEditorCategory);
        Assert.Equal("Srazové", viewModel.VehicleDetailWorkspace.VehicleEditorNote);
        Assert.Equal("Škoda 100", viewModel.VehicleDetailWorkspace.VehicleEditorMakeModel);
        Assert.Equal("2AB3456", viewModel.VehicleDetailWorkspace.VehicleEditorPlate);
        Assert.Equal("1973", viewModel.VehicleDetailWorkspace.VehicleEditorYear);
        Assert.Equal("35", viewModel.VehicleDetailWorkspace.VehicleEditorPower);
        Assert.Equal("05/2025", viewModel.VehicleDetailWorkspace.VehicleEditorLastTk);
        Assert.Equal("05/2027", viewModel.VehicleDetailWorkspace.VehicleEditorNextTk);
        Assert.Equal("05/2025", viewModel.VehicleDetailWorkspace.VehicleEditorGreenCardFrom);
        Assert.Equal("05/2026", viewModel.VehicleDetailWorkspace.VehicleEditorGreenCardTo);
        Assert.Equal("Veterán", viewModel.VehicleDetailWorkspace.VehicleEditorState);
        Assert.Equal("srazové, veterán", viewModel.VehicleDetailWorkspace.VehicleEditorTags);
        Assert.Equal("Benzín", viewModel.VehicleDetailWorkspace.VehicleEditorPowertrain);
        Assert.Equal("Má klimatizaci", viewModel.VehicleDetailWorkspace.VehicleEditorClimateProfile);
        Assert.Equal("Řemen", viewModel.VehicleDetailWorkspace.VehicleEditorTimingDrive);
        Assert.Equal("Manuální", viewModel.VehicleDetailWorkspace.VehicleEditorTransmission);

        viewModel.CancelVehicleEditCommand.Execute(null);

        Assert.False(viewModel.VehicleDetailWorkspace.IsEditingVehicle);
        Assert.True(viewModel.VehicleDetailWorkspace.IsVehicleDetailVisible);
        Assert.Equal("Detail vozidla", viewModel.VehicleDetailWorkspace.VehiclePanelHeading);
    }

    [Fact]
    public void Shared_workspace_state_should_not_be_reexposed_as_root_proxy_properties()
    {
        var rootType = typeof(MainWindowViewModel);
        var removedProxyProperties = new[]
        {
            "TimelineSummary",
            "TimelineSearchText",
            "SelectedTimelineFilter",
            "SelectedTimelineItem",
            "SelectedTimelineDetail",
            "ExportStatus",
            "TimelineFilters",
            "GlobalSearchSummary",
            "GlobalSearchText",
            "SelectedSearchResult",
            "SelectedSearchResultDetail",
            "UpcomingOverviewSearchText",
            "OverdueOverviewSearchText",
            "SelectedUpcomingOverviewFilter",
            "IncludeMissingGreenCardsInUpcomingOverview",
            "IncludeDataIssuesInUpcomingOverview",
            "SelectedOverdueOverviewFilter",
            "UpcomingOverviewSummary",
            "OverdueOverviewSummary",
            "SelectedUpcomingOverviewDetail",
            "SelectedOverdueOverviewDetail",
            "SelectedUpcomingOverviewItem",
            "SelectedOverdueOverviewItem",
            "OverviewFilters",
            "UpcomingOverviewFilters",
            "AuditSummary",
            "SelectedDashboardAuditItem",
            "CostSummary",
            "CostComparison",
            "SelectedDashboardCostVehicle",
            "CostExportStatus",
            "CostPeriodPresets",
            "DashboardTimelineSummary",
            "SelectedDashboardTimelineDetail",
            "SelectedDashboardTimelineItem",
            "AuditItems",
            "DashboardAuditItems",
            "CostVehicles",
            "DashboardUpcomingTimeline",
            "HistorySummary",
            "FuelSummary",
            "ReminderSummary",
            "MaintenanceSummary",
            "RecordSummary",
            "MaintenanceTemplateOptions",
            "RecordAttachmentModes",
            "VehicleDetailWindowTitle",
            "HistoryWindowTitle",
            "FuelWindowTitle",
            "ReminderWindowTitle",
            "MaintenanceWindowTitle",
            "RecordWindowTitle",
            "TimelineWindowTitle",
            "AuditWindowTitle",
            "CostWindowTitle",
            "DashboardWindowTitle",
            "GlobalSearchWindowTitle",
            "UpcomingOverviewWindowTitle",
            "OverdueOverviewWindowTitle",
            "CanCreateHistory",
            "CanEditSelectedHistory",
            "CanDeleteSelectedHistory",
            "CanSaveHistory",
            "CanCancelHistoryEdit",
            "CanCreateFuel",
            "CanEditSelectedFuel",
            "CanDeleteSelectedFuel",
            "CanSaveFuel",
            "CanCancelFuelEdit",
            "CanCreateReminder",
            "CanEditSelectedReminder",
            "CanDeleteSelectedReminder",
            "CanAdvanceSelectedReminder",
            "CanSaveReminder",
            "CanCancelReminderEdit",
            "CanCreateMaintenance",
            "CanEditSelectedMaintenance",
            "CanDeleteSelectedMaintenance",
            "CanCompleteSelectedMaintenance",
            "CanOpenMaintenanceRecommendations",
            "CanSaveMaintenance",
            "CanCancelMaintenanceEdit",
            "CanCreateRecord",
            "CanEditSelectedRecord",
            "CanDeleteSelectedRecord",
            "CanSaveRecord",
            "CanCancelRecordEdit",
            "CanBrowseRecordAttachment",
            "CanMoveSelectedRecordToManaged",
            "CanOpenSelectedRecordFile",
            "CanOpenSelectedRecordFolder",
            "CanCopySelectedRecordPath",
            "CanExportFleetCostSummary",
            "CanExportSelectedVehicleCost",
            "SelectedRecord",
            "SelectedRecordDetail",
            "IsEditingRecord",
            "RecordPanelHeading",
            "RecordEditorStatus",
            "RecordEditorRecordType",
            "RecordEditorTitle",
            "RecordEditorProvider",
            "RecordEditorValidFrom",
            "RecordEditorValidTo",
            "RecordEditorPrice",
            "SelectedRecordEditorAttachmentMode",
            "RecordEditorPathInput",
            "RecordEditorStoredPath",
            "RecordEditorResolvedPath",
            "RecordEditorAvailability",
            "RecordEditorNote",
            "IsRecordDetailVisible",
            "IsRecordEditorManaged",
            "RecordEditorPathInputLabel",
            "RecordEditorPathInputHelp",
            "SelectedHistory",
            "SelectedHistoryDetail",
            "IsEditingHistory",
            "HistoryPanelHeading",
            "HistoryEditorStatus",
            "HistoryEditorDate",
            "HistoryEditorType",
            "HistoryEditorOdometer",
            "HistoryEditorCost",
            "HistoryEditorNote",
            "IsHistoryDetailVisible",
            "SelectedFuel",
            "SelectedFuelDetail",
            "IsEditingFuel",
            "FuelPanelHeading",
            "FuelEditorStatus",
            "FuelEditorDate",
            "FuelEditorFuelType",
            "FuelEditorLiters",
            "FuelEditorTotalCost",
            "FuelEditorOdometer",
            "FuelEditorFullTank",
            "FuelEditorNote",
            "IsFuelDetailVisible",
            "SelectedReminder",
            "SelectedReminderDetail",
            "IsEditingReminder",
            "ReminderPanelHeading",
            "ReminderEditorStatus",
            "ReminderEditorTitle",
            "ReminderEditorDueDate",
            "ReminderEditorDays",
            "ReminderEditorRepeatMode",
            "ReminderEditorNote",
            "IsReminderDetailVisible",
            "SelectedMaintenance",
            "SelectedMaintenanceDetail",
            "IsEditingMaintenance",
            "MaintenancePanelHeading",
            "MaintenanceEditorStatus",
            "SelectedMaintenanceTemplate",
            "MaintenanceEditorTitle",
            "MaintenanceEditorIntervalKm",
            "MaintenanceEditorIntervalMonths",
            "MaintenanceEditorLastServiceDate",
            "MaintenanceEditorLastServiceOdometer",
            "MaintenanceEditorIsActive",
            "MaintenanceEditorNote",
            "IsMaintenanceDetailVisible",
            "SelectedVehicleHistory",
            "SelectedVehicleFuel",
            "SelectedVehicleReminders",
            "SelectedVehicleMaintenance",
            "SelectedVehicleRecords",
            "SelectedVehicleTimeline",
            "GlobalSearchResults",
            "UpcomingOverviewItems",
            "OverdueOverviewItems",
            "SelectedVehicleHeading",
            "SelectedVehicleOverview",
            "SelectedVehicleDates",
            "SelectedVehicleProfile",
            "VehicleEditorStatus",
            "VehicleEditorName",
            "VehicleEditorCategory",
            "VehicleEditorNote",
            "VehicleEditorMakeModel",
            "VehicleEditorPlate",
            "VehicleEditorYear",
            "VehicleEditorPower",
            "VehicleEditorLastTk",
            "VehicleEditorNextTk",
            "VehicleEditorGreenCardFrom",
            "VehicleEditorGreenCardTo",
            "VehicleEditorState",
            "VehicleEditorTags",
            "VehicleEditorPowertrain",
            "VehicleEditorClimateProfile",
            "VehicleEditorTimingDrive",
            "VehicleEditorTransmission",
            "IsEditingVehicle",
            "IsVehicleDetailVisible",
            "VehiclePanelHeading"
        };

        foreach (var propertyName in removedProxyProperties)
        {
            Assert.Null(rootType.GetProperty(propertyName));
        }
    }

    [Fact]
    public void Searchable_workspace_focus_commands_request_matching_search_fields()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.TimelineWorkspace.FocusSearchCommand.Execute(null);
        viewModel.GlobalSearchWorkspace.FocusSearchCommand.Execute(null);
        viewModel.UpcomingOverviewWorkspace.FocusSearchCommand.Execute(null);
        viewModel.OverdueOverviewWorkspace.FocusSearchCommand.Execute(null);

        Assert.Equal(
            [
                DesktopFocusTarget.TimelineSearch,
                DesktopFocusTarget.GlobalSearchBox,
                DesktopFocusTarget.UpcomingOverviewSearch,
                DesktopFocusTarget.OverdueOverviewSearch
            ],
            requestedTargets);
    }

    [Theory]
    [InlineData("HistoryWorkspaceView")]
    [InlineData("TimelineWorkspaceView")]
    [InlineData("ReminderWorkspaceView")]
    [InlineData("RecordWorkspaceView")]
    [InlineData("GlobalSearchWorkspaceView")]
    [InlineData("UpcomingOverviewWorkspaceView")]
    [InlineData("OverdueOverviewWorkspaceView")]
    [InlineData("CostWorkspaceView")]
    [InlineData("DashboardWorkspaceView")]
    public void Main_window_hosts_shared_workspace_controls(string workspaceViewName)
    {
        var root = FindRepositoryRoot();
        var mainWindowXaml = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", "MainWindow.axaml"));

        Assert.Contains($"workspaces:{workspaceViewName}", mainWindowXaml, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("TimelineWindow", "TimelineWorkspaceView", "TimelineWorkspaceHost")]
    [InlineData("CostWindow", "CostWorkspaceView", "CostWorkspaceHost")]
    [InlineData("GlobalSearchWindow", "GlobalSearchWorkspaceView", "GlobalSearchWorkspaceHost")]
    [InlineData("UpcomingOverviewWindow", "UpcomingOverviewWorkspaceView", "UpcomingOverviewWorkspaceHost")]
    [InlineData("OverdueOverviewWindow", "OverdueOverviewWorkspaceView", "OverdueOverviewWorkspaceHost")]
    public void Overview_windows_host_shared_workspace_controls(string windowName, string workspaceViewName, string workspaceHostName)
    {
        var root = FindRepositoryRoot();
        var windowXaml = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", $"{windowName}.axaml"));
        var codeBehind = File.ReadAllText(Path.Combine(root, "dotnet", "src", "Vehimap.Desktop", "Views", $"{windowName}.axaml.cs"));

        Assert.Contains($"workspaces:{workspaceViewName}", windowXaml, StringComparison.Ordinal);
        Assert.Contains($"RegisterWorkspaceLifecycle(this, \"{workspaceHostName}\"", codeBehind, StringComparison.Ordinal);
    }

    private static MainWindowViewModel CreateViewModel()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-workspace-tests", Guid.NewGuid().ToString("N"));
        var dataRoot = new VehimapDataRoot(tempRoot, Path.Combine(tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Octavia", "Osobní vozidla", "Rodinné auto", "Škoda Octavia", "1AB2345", "2020", "110", "", "05/2027", "05/2025", "05/2026")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "Aktivní", "", "Benzín", "Klimatizace", "Řemen", "Manuál")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "10.01.2026", "Servis", "10000", "150", "Výměna oleje")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "15.01.2026", "10150", "42", "350", true, "Benzín", "Plná")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Objednat servis", "01.12.2099", "30", "Ročně", "Zavolat servisu")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Doklad", "Asistence", "", "", "08/2099", "", VehicleRecordAttachmentMode.External, "", "")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("mnt_1", "veh_1", "Motorový olej", "15000", "12", "10.01.2026", "10000", true, "Každý rok")
            ]
        };

        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var bootstrapper = new LegacyVehimapBootstrapper(new StubDataRootLocator(dataRoot), dataStore);

        return new MainWindowViewModel(
            dataStore,
            bootstrapper,
            new ManagedAttachmentPathService(),
            new StubFileLauncher(),
            new StubFilePickerService(),
            new LegacyGlobalSearchService(new ManagedAttachmentPathService()),
            new LegacyTimelineService(),
            new LegacyCalendarExportService(),
            new StubTextFileSaveService());
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "dotnet"))
                && File.Exists(Path.Combine(directory.FullName, "src", "VERSION")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private sealed class MutableStubLegacyDataStore : ILegacyDataStore
    {
        public MutableStubLegacyDataStore(VehimapDataSet dataSet)
        {
            CurrentDataSet = dataSet;
        }

        public VehimapDataSet CurrentDataSet { get; private set; }

        public Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
            => Task.FromResult(CurrentDataSet);

        public Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
        {
            CurrentDataSet = dataSet;
            return Task.CompletedTask;
        }
    }

    private sealed class StubDataRootLocator : IDataRootLocator
    {
        private readonly VehimapDataRoot _dataRoot;

        public StubDataRootLocator(VehimapDataRoot dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public VehimapDataRoot Resolve(string appBasePath) => _dataRoot;
    }

    private sealed class StubFileLauncher : IFileLauncher
    {
        public Task OpenAsync(string path, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task OpenFolderAsync(string path, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubFilePickerService : IFilePickerService
    {
        public Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class StubTextFileSaveService : ITextFileSaveService
    {
        public Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }
}
