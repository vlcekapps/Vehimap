using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class MainWindowViewModelNavigationTests
{
    [Fact]
    public void Dashboard_audit_command_opens_matching_record_tab_and_requests_focus()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.AuditWorkspace.SelectedDashboardAuditItem = viewModel.AuditWorkspace.AuditItems.First(item => item.EntityId == "rec_2");

        viewModel.OpenSelectedDashboardAuditItemCommand.Execute(null);

        Assert.Equal(6, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rec_2", viewModel.RecordWorkspace.SelectedRecord?.Id);
        Assert.Equal(DesktopFocusTarget.RecordList, requestedFocus);
    }

    [Fact]
    public void Dashboard_timeline_command_opens_matching_reminder_and_requests_focus()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.DashboardWorkspace.SelectedDashboardTimelineItem = viewModel.DashboardWorkspace.DashboardUpcomingTimeline.First(item => item.Kind == "custom");

        viewModel.OpenSelectedDashboardTimelineItemCommand.Execute(null);

        Assert.Equal(3, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rem_1", viewModel.ReminderWorkspace.SelectedReminder?.Id);
        Assert.Equal(DesktopFocusTarget.ReminderList, requestedFocus);
    }

    [Fact]
    public void Focus_timeline_search_command_switches_to_timeline_tab_and_requests_search_focus()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.FocusTimelineSearchCommand.Execute(null);

        Assert.Equal(5, viewModel.SelectedVehicleTabIndex);
        Assert.Equal(DesktopFocusTarget.TimelineSearch, requestedFocus);
    }

    [Fact]
    public void Workspace_navigation_commands_are_locked_while_an_editor_is_open()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.EditSelectedVehicleCommand.Execute(null);
        requestedTargets.Clear();

        Assert.True(viewModel.VehicleDetailWorkspace.IsEditingVehicle);
        Assert.True(viewModel.IsWorkspaceNavigationLocked);
        Assert.False(viewModel.CanUseWorkspaceNavigation);
        Assert.False(viewModel.CanOpenHistoryWindow);

        viewModel.FocusDashboardCommand.Execute(null);
        viewModel.SelectVehicleTabCommand.Execute(DesktopTabIndexes.History);
        viewModel.ShowDashboardFromTray();

        Assert.Equal(DesktopTabIndexes.Detail, viewModel.SelectedVehicleTabIndex);
        Assert.Contains("jinou kartu", viewModel.WorkspaceNavigationLockStatus, StringComparison.CurrentCulture);
        Assert.Contains("jinou kartu", viewModel.ShellStatus, StringComparison.CurrentCulture);
        Assert.NotEmpty(requestedTargets);
        Assert.All(requestedTargets, target => Assert.Equal(DesktopFocusTarget.VehicleEditorName, target));

        viewModel.CancelVehicleEditCommand.Execute(null);

        Assert.False(viewModel.IsWorkspaceNavigationLocked);
        Assert.True(viewModel.CanUseWorkspaceNavigation);
        Assert.True(viewModel.CanOpenHistoryWindow);
    }

    [Fact]
    public async Task Vehicle_detail_related_actions_switch_to_matching_workspaces_and_request_focus()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        Assert.True(viewModel.VehicleDetailWorkspace.OpenVehicleHistoryWorkspace());
        Assert.Equal(DesktopTabIndexes.History, viewModel.SelectedVehicleTabIndex);
        Assert.Equal(DesktopFocusTarget.HistoryList, requestedTargets.Last());

        Assert.True(viewModel.VehicleDetailWorkspace.OpenVehicleFuelWorkspace());
        Assert.Equal(DesktopTabIndexes.Fuel, viewModel.SelectedVehicleTabIndex);
        Assert.Equal(DesktopFocusTarget.FuelList, requestedTargets.Last());

        Assert.True(viewModel.VehicleDetailWorkspace.OpenVehicleReminderWorkspace());
        Assert.Equal(DesktopTabIndexes.Reminder, viewModel.SelectedVehicleTabIndex);
        Assert.Equal(DesktopFocusTarget.ReminderList, requestedTargets.Last());

        Assert.True(viewModel.VehicleDetailWorkspace.OpenVehicleMaintenanceWorkspace());
        Assert.Equal(DesktopTabIndexes.Maintenance, viewModel.SelectedVehicleTabIndex);
        Assert.Equal(DesktopFocusTarget.MaintenanceList, requestedTargets.Last());

        Assert.True(viewModel.VehicleDetailWorkspace.OpenVehicleRecordWorkspace());
        Assert.Equal(DesktopTabIndexes.Record, viewModel.SelectedVehicleTabIndex);
        Assert.Equal(DesktopFocusTarget.RecordList, requestedTargets.Last());

        Assert.True(viewModel.VehicleDetailWorkspace.OpenVehicleTimelineWorkspace());
        Assert.Equal(DesktopTabIndexes.Timeline, viewModel.SelectedVehicleTabIndex);
        Assert.Equal(DesktopFocusTarget.TimelineList, requestedTargets.Last());

        Assert.True(await viewModel.VehicleDetailWorkspace.OpenVehicleCostsWorkspaceAsync());
        Assert.Equal(DesktopTabIndexes.Cost, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("veh_1", viewModel.CostWorkspace.SelectedDashboardCostVehicle?.VehicleId);
        Assert.Equal(DesktopFocusTarget.CostList, requestedTargets.Last());
    }

    [Fact]
    public async Task Vehicle_detail_related_actions_are_locked_while_an_editor_is_open()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.EditSelectedVehicleCommand.Execute(null);
        requestedTargets.Clear();

        Assert.False(viewModel.VehicleDetailWorkspace.CanOpenVehicleRelatedWorkspace);
        Assert.False(viewModel.VehicleDetailWorkspace.OpenVehicleHistoryWorkspace());
        Assert.False(viewModel.VehicleDetailWorkspace.OpenVehicleFuelWorkspace());
        Assert.False(viewModel.VehicleDetailWorkspace.OpenVehicleReminderWorkspace());
        Assert.False(viewModel.VehicleDetailWorkspace.OpenVehicleMaintenanceWorkspace());
        Assert.False(viewModel.VehicleDetailWorkspace.OpenVehicleRecordWorkspace());
        Assert.False(viewModel.VehicleDetailWorkspace.OpenVehicleTimelineWorkspace());
        Assert.False(await viewModel.VehicleDetailWorkspace.OpenVehicleCostsWorkspaceAsync());
        Assert.Equal(DesktopTabIndexes.Detail, viewModel.SelectedVehicleTabIndex);
        Assert.Empty(requestedTargets);
    }

    [Fact]
    public void Focus_current_search_command_uses_active_workspace_or_vehicle_search()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Detail;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.History;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Fuel;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Reminder;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Maintenance;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Timeline;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Record;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Audit;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Cost;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Search;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.UpcomingOverview;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.OverdueOverview;
        viewModel.FocusCurrentSearchCommand.Execute(null);

        Assert.Equal(
            [
                DesktopFocusTarget.VehicleSearch,
                DesktopFocusTarget.HistorySearch,
                DesktopFocusTarget.FuelSearch,
                DesktopFocusTarget.ReminderSearch,
                DesktopFocusTarget.MaintenanceSearch,
                DesktopFocusTarget.TimelineSearch,
                DesktopFocusTarget.RecordSearch,
                DesktopFocusTarget.AuditSearch,
                DesktopFocusTarget.CostSearch,
                DesktopFocusTarget.GlobalSearchBox,
                DesktopFocusTarget.UpcomingOverviewSearch,
                DesktopFocusTarget.OverdueOverviewSearch
            ],
            requestedTargets);
    }

    [Fact]
    public void Evidence_workspace_search_filters_lists_and_clears_actions_when_empty()
    {
        var viewModel = CreateViewModel();

        viewModel.HistoryWorkspace.HistorySearchText = "oleje";
        Assert.Single(viewModel.HistoryWorkspace.VisibleHistoryItems);
        Assert.Equal("hist_1", viewModel.HistoryWorkspace.SelectedHistory?.Id);

        viewModel.FuelWorkspace.FuelSearchText = "benzín";
        Assert.Single(viewModel.FuelWorkspace.VisibleFuelItems);
        Assert.Equal("fuel_1", viewModel.FuelWorkspace.SelectedFuel?.Id);

        viewModel.ReminderWorkspace.ReminderSearchText = "servis";
        Assert.Single(viewModel.ReminderWorkspace.VisibleReminderItems);
        Assert.Equal("rem_1", viewModel.ReminderWorkspace.SelectedReminder?.Id);

        viewModel.MaintenanceWorkspace.MaintenanceSearchText = "olej";
        Assert.Single(viewModel.MaintenanceWorkspace.VisibleMaintenanceItems);
        Assert.Equal("mnt_1", viewModel.MaintenanceWorkspace.SelectedMaintenance?.Id);

        viewModel.RecordWorkspace.RecordSearchText = "Asistence";
        Assert.Single(viewModel.RecordWorkspace.VisibleRecordItems);
        Assert.Equal("rec_2", viewModel.RecordWorkspace.SelectedRecord?.Id);

        viewModel.RecordWorkspace.RecordSearchText = "nenajitelný dotaz";

        Assert.Empty(viewModel.RecordWorkspace.VisibleRecordItems);
        Assert.Null(viewModel.RecordWorkspace.SelectedRecord);
        Assert.False(viewModel.EditSelectedRecordCommand.CanExecute(null));
        Assert.False(viewModel.OpenSelectedRecordFileCommand.CanExecute(null));
    }

    [Fact]
    public void Evidence_workspace_clear_search_commands_restore_lists_and_focus_search_fields()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.HistoryWorkspace.HistorySearchText = "oleje";
        viewModel.FuelWorkspace.FuelSearchText = "benzín";
        viewModel.ReminderWorkspace.ReminderSearchText = "servis";
        viewModel.MaintenanceWorkspace.MaintenanceSearchText = "olej";
        viewModel.RecordWorkspace.RecordSearchText = "Asistence";

        Assert.True(viewModel.HistoryWorkspace.ClearHistorySearchCommand.CanExecute(null));
        Assert.True(viewModel.FuelWorkspace.ClearFuelSearchCommand.CanExecute(null));
        Assert.True(viewModel.ReminderWorkspace.ClearReminderSearchCommand.CanExecute(null));
        Assert.True(viewModel.MaintenanceWorkspace.ClearMaintenanceSearchCommand.CanExecute(null));
        Assert.True(viewModel.RecordWorkspace.ClearRecordSearchCommand.CanExecute(null));

        viewModel.HistoryWorkspace.ClearHistorySearchCommand.Execute(null);
        viewModel.FuelWorkspace.ClearFuelSearchCommand.Execute(null);
        viewModel.ReminderWorkspace.ClearReminderSearchCommand.Execute(null);
        viewModel.MaintenanceWorkspace.ClearMaintenanceSearchCommand.Execute(null);
        viewModel.RecordWorkspace.ClearRecordSearchCommand.Execute(null);

        Assert.Equal(string.Empty, viewModel.HistoryWorkspace.HistorySearchText);
        Assert.Equal(string.Empty, viewModel.FuelWorkspace.FuelSearchText);
        Assert.Equal(string.Empty, viewModel.ReminderWorkspace.ReminderSearchText);
        Assert.Equal(string.Empty, viewModel.MaintenanceWorkspace.MaintenanceSearchText);
        Assert.Equal(string.Empty, viewModel.RecordWorkspace.RecordSearchText);
        Assert.Equal(viewModel.HistoryWorkspace.SelectedVehicleHistory.Count, viewModel.HistoryWorkspace.VisibleHistoryItems.Count);
        Assert.Equal(viewModel.FuelWorkspace.SelectedVehicleFuel.Count, viewModel.FuelWorkspace.VisibleFuelItems.Count);
        Assert.Equal(viewModel.ReminderWorkspace.SelectedVehicleReminders.Count, viewModel.ReminderWorkspace.VisibleReminderItems.Count);
        Assert.Equal(viewModel.MaintenanceWorkspace.SelectedVehicleMaintenance.Count, viewModel.MaintenanceWorkspace.VisibleMaintenanceItems.Count);
        Assert.Equal(viewModel.RecordWorkspace.SelectedVehicleRecords.Count, viewModel.RecordWorkspace.VisibleRecordItems.Count);
        Assert.False(viewModel.HistoryWorkspace.ClearHistorySearchCommand.CanExecute(null));
        Assert.False(viewModel.FuelWorkspace.ClearFuelSearchCommand.CanExecute(null));
        Assert.False(viewModel.ReminderWorkspace.ClearReminderSearchCommand.CanExecute(null));
        Assert.False(viewModel.MaintenanceWorkspace.ClearMaintenanceSearchCommand.CanExecute(null));
        Assert.False(viewModel.RecordWorkspace.ClearRecordSearchCommand.CanExecute(null));
        Assert.Equal(
            [
                DesktopFocusTarget.HistorySearch,
                DesktopFocusTarget.FuelSearch,
                DesktopFocusTarget.ReminderSearch,
                DesktopFocusTarget.MaintenanceSearch,
                DesktopFocusTarget.RecordSearch
            ],
            requestedTargets);
    }

    [Fact]
    public void Cost_workspace_search_filters_vehicle_costs_and_clears_selected_actions_when_empty()
    {
        var viewModel = CreateViewModel();

        viewModel.CostWorkspace.CostSearchText = "Octavia";

        Assert.Single(viewModel.CostWorkspace.VisibleCostVehicles);
        Assert.Equal("veh_1", viewModel.CostWorkspace.SelectedDashboardCostVehicle?.VehicleId);
        Assert.True(viewModel.ExportSelectedVehicleCostDetailCommand.CanExecute(null));

        viewModel.CostWorkspace.CostSearchText = "nenajitelný dotaz";

        Assert.Empty(viewModel.CostWorkspace.VisibleCostVehicles);
        Assert.Null(viewModel.CostWorkspace.SelectedDashboardCostVehicle);
        Assert.False(viewModel.OpenSelectedDashboardCostVehicleCommand.CanExecute(null));
        Assert.False(viewModel.ExportSelectedVehicleCostDetailCommand.CanExecute(null));
    }

    [Fact]
    public void Evidence_workspace_sort_controls_reorder_lists_and_persist_preferences()
    {
        VehimapDataSet? dataSetRef = null;
        var viewModel = CreateViewModel(configureDataSet: dataSet =>
        {
            dataSetRef = dataSet;
            dataSet.HistoryEntries.Add(new VehicleHistoryEntry("hist_2", "veh_1", "05.01.2026", "Oprava", "9800", "900", ""));
            dataSet.FuelEntries.Add(new FuelEntry("fuel_2", "veh_1", "05.01.2026", "9800", "30", "200", true, "Benzín", ""));
            dataSet.Reminders.Add(new VehicleReminder("rem_2", "veh_1", "AAA kontrola", "01.01.2099", "30", "Ročně", ""));
            dataSet.MaintenancePlans.Add(new MaintenancePlan("mnt_2", "veh_1", "AAA filtr", "5000", "6", "05.01.2026", "9800", true, ""));
            dataSet.Records.Add(new VehicleRecord("rec_3", "veh_1", "Doklad", "Drahý doklad", "", "", "02/2027", "900", VehicleRecordAttachmentMode.External, "", ""));
        });

        viewModel.HistoryWorkspace.SelectedHistorySortOption = WorkspaceSortHelpers.CostSortLabel;
        viewModel.HistoryWorkspace.HistorySortDescending = true;
        viewModel.FuelWorkspace.SelectedFuelSortOption = WorkspaceSortHelpers.OdometerSortLabel;
        viewModel.FuelWorkspace.FuelSortDescending = false;
        viewModel.ReminderWorkspace.SelectedReminderSortOption = WorkspaceSortHelpers.TitleSortLabel;
        viewModel.MaintenanceWorkspace.SelectedMaintenanceSortOption = WorkspaceSortHelpers.IntervalSortLabel;
        viewModel.RecordWorkspace.SelectedRecordSortOption = WorkspaceSortHelpers.CostSortLabel;
        viewModel.RecordWorkspace.RecordSortDescending = true;

        Assert.Equal("hist_2", viewModel.HistoryWorkspace.VisibleHistoryItems.First().Id);
        Assert.Equal("fuel_2", viewModel.FuelWorkspace.VisibleFuelItems.First().Id);
        Assert.Equal("rem_2", viewModel.ReminderWorkspace.VisibleReminderItems.First().Id);
        Assert.Equal("mnt_2", viewModel.MaintenanceWorkspace.VisibleMaintenanceItems.First().Id);
        Assert.Equal("rec_3", viewModel.RecordWorkspace.VisibleRecordItems.First().Id);
        Assert.Equal(WorkspaceSortHelpers.CostSortLabel, dataSetRef!.Settings.GetValue("evidence_sort", "history_sort"));
        Assert.Equal("1", dataSetRef.Settings.GetValue("evidence_sort", "history_descending"));
        Assert.Equal(WorkspaceSortHelpers.OdometerSortLabel, dataSetRef.Settings.GetValue("evidence_sort", "fuel_sort"));
        Assert.Equal("0", dataSetRef.Settings.GetValue("evidence_sort", "fuel_descending"));
        Assert.Equal(WorkspaceSortHelpers.TitleSortLabel, dataSetRef.Settings.GetValue("evidence_sort", "reminder_sort"));
        Assert.Equal(WorkspaceSortHelpers.IntervalSortLabel, dataSetRef.Settings.GetValue("evidence_sort", "maintenance_sort"));
        Assert.Equal("0", dataSetRef.Settings.GetValue("evidence_sort", "maintenance_descending"));
        Assert.Equal(WorkspaceSortHelpers.CostSortLabel, dataSetRef.Settings.GetValue("evidence_sort", "record_sort"));
        Assert.Equal("1", dataSetRef.Settings.GetValue("evidence_sort", "record_descending"));
    }

    [Fact]
    public void Evidence_workspace_loads_saved_sort_preferences_before_initial_vehicle_selection()
    {
        var viewModel = CreateViewModel(configureDataSet: dataSet =>
        {
            dataSet.HistoryEntries.Add(new VehicleHistoryEntry("hist_2", "veh_1", "05.01.2026", "Oprava", "9800", "900", ""));
            dataSet.FuelEntries.Add(new FuelEntry("fuel_2", "veh_1", "05.01.2026", "9800", "30", "200", true, "Benzín", ""));
            dataSet.Reminders.Add(new VehicleReminder("rem_2", "veh_1", "AAA kontrola", "01.01.2099", "30", "Ročně", ""));
            dataSet.MaintenancePlans.Add(new MaintenancePlan("mnt_2", "veh_1", "AAA filtr", "5000", "6", "05.01.2026", "9800", true, ""));
            dataSet.Records.Add(new VehicleRecord("rec_3", "veh_1", "Doklad", "Drahý doklad", "", "", "02/2027", "900", VehicleRecordAttachmentMode.External, "", ""));
            dataSet.Settings.SetValue("evidence_sort", "history_sort", WorkspaceSortHelpers.CostSortLabel);
            dataSet.Settings.SetValue("evidence_sort", "history_descending", "1");
            dataSet.Settings.SetValue("evidence_sort", "fuel_sort", WorkspaceSortHelpers.OdometerSortLabel);
            dataSet.Settings.SetValue("evidence_sort", "fuel_descending", "0");
            dataSet.Settings.SetValue("evidence_sort", "reminder_sort", WorkspaceSortHelpers.TitleSortLabel);
            dataSet.Settings.SetValue("evidence_sort", "maintenance_sort", WorkspaceSortHelpers.IntervalSortLabel);
            dataSet.Settings.SetValue("evidence_sort", "maintenance_descending", "0");
            dataSet.Settings.SetValue("evidence_sort", "record_sort", WorkspaceSortHelpers.CostSortLabel);
            dataSet.Settings.SetValue("evidence_sort", "record_descending", "1");
        });

        Assert.Equal(WorkspaceSortHelpers.CostSortLabel, viewModel.HistoryWorkspace.SelectedHistorySortOption);
        Assert.Equal(WorkspaceSortHelpers.OdometerSortLabel, viewModel.FuelWorkspace.SelectedFuelSortOption);
        Assert.Equal(WorkspaceSortHelpers.TitleSortLabel, viewModel.ReminderWorkspace.SelectedReminderSortOption);
        Assert.Equal(WorkspaceSortHelpers.IntervalSortLabel, viewModel.MaintenanceWorkspace.SelectedMaintenanceSortOption);
        Assert.Equal(WorkspaceSortHelpers.CostSortLabel, viewModel.RecordWorkspace.SelectedRecordSortOption);
        Assert.Equal("hist_2", viewModel.HistoryWorkspace.VisibleHistoryItems.First().Id);
        Assert.Equal("fuel_2", viewModel.FuelWorkspace.VisibleFuelItems.First().Id);
        Assert.Equal("rem_2", viewModel.ReminderWorkspace.VisibleReminderItems.First().Id);
        Assert.Equal("mnt_2", viewModel.MaintenanceWorkspace.VisibleMaintenanceItems.First().Id);
        Assert.Equal("rec_3", viewModel.RecordWorkspace.VisibleRecordItems.First().Id);
    }

    [Fact]
    public void Audit_and_global_search_sort_controls_reorder_lists_and_persist_preferences()
    {
        VehimapDataSet? dataSetRef = null;
        var viewModel = CreateViewModel(configureDataSet: dataSet =>
        {
            dataSetRef = dataSet;
            dataSet.Vehicles.Add(new Vehicle("veh_2", "Božena", "Osobní vozidla", "Srazové auto", "Škoda 100", "", "1975", "35", "", "", "", ""));
            dataSet.VehicleMetaEntries.Add(new VehicleMeta("veh_2", "Aktivní", "", "Benzín", "", "Řemen", "Manuál"));
            dataSet.Records.Add(new VehicleRecord("rec_3", "veh_2", "Doklad", "Chybějící příloha", "", "", "02/2027", "", VehicleRecordAttachmentMode.External, "", "Prověřit"));
        });

        viewModel.AuditWorkspace.SelectedAuditSortOption = WorkspaceSortHelpers.VehicleSortLabel;
        viewModel.GlobalSearchWorkspace.GlobalSearchText = "Chybějící příloha";
        viewModel.GlobalSearchWorkspace.SelectedGlobalSearchSortOption = WorkspaceSortHelpers.VehicleSortLabel;

        Assert.Equal("Božena", viewModel.AuditWorkspace.VisibleAuditItems.First().VehicleName);
        Assert.Equal("Božena", viewModel.GlobalSearchWorkspace.GlobalSearchResults.First().VehicleName);
        Assert.Equal(WorkspaceSortHelpers.VehicleSortLabel, dataSetRef!.Settings.GetValue("workspace_sort", "audit_sort"));
        Assert.Equal("0", dataSetRef.Settings.GetValue("workspace_sort", "audit_descending"));
        Assert.Equal(WorkspaceSortHelpers.VehicleSortLabel, dataSetRef.Settings.GetValue("workspace_sort", "global_search_sort"));
        Assert.Equal("0", dataSetRef.Settings.GetValue("workspace_sort", "global_search_descending"));
    }

    [Fact]
    public void Audit_and_global_search_load_saved_sort_preferences()
    {
        var viewModel = CreateViewModel(configureDataSet: dataSet =>
        {
            dataSet.Vehicles.Add(new Vehicle("veh_2", "Božena", "Osobní vozidla", "Srazové auto", "Škoda 100", "", "1975", "35", "", "", "", ""));
            dataSet.VehicleMetaEntries.Add(new VehicleMeta("veh_2", "Aktivní", "", "Benzín", "", "Řemen", "Manuál"));
            dataSet.Records.Add(new VehicleRecord("rec_3", "veh_2", "Doklad", "Chybějící příloha", "", "", "02/2027", "", VehicleRecordAttachmentMode.External, "", "Prověřit"));
            dataSet.Settings.SetValue("workspace_sort", "audit_sort", WorkspaceSortHelpers.VehicleSortLabel);
            dataSet.Settings.SetValue("workspace_sort", "audit_descending", "0");
            dataSet.Settings.SetValue("workspace_sort", "global_search_sort", WorkspaceSortHelpers.VehicleSortLabel);
            dataSet.Settings.SetValue("workspace_sort", "global_search_descending", "0");
        });

        viewModel.GlobalSearchWorkspace.GlobalSearchText = "Chybějící příloha";

        Assert.Equal(WorkspaceSortHelpers.VehicleSortLabel, viewModel.AuditWorkspace.SelectedAuditSortOption);
        Assert.Equal(WorkspaceSortHelpers.VehicleSortLabel, viewModel.GlobalSearchWorkspace.SelectedGlobalSearchSortOption);
        Assert.Equal("Božena", viewModel.AuditWorkspace.VisibleAuditItems.First().VehicleName);
        Assert.Equal("Božena", viewModel.GlobalSearchWorkspace.GlobalSearchResults.First().VehicleName);
    }

    [Fact]
    public void Overview_workspace_clear_search_commands_restore_lists_and_focus_search_fields()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;
        var initialTimelineCount = viewModel.TimelineWorkspace.SelectedVehicleTimeline.Count;
        var initialAuditCount = viewModel.AuditWorkspace.VisibleAuditItems.Count;
        var initialCostCount = viewModel.CostWorkspace.VisibleCostVehicles.Count;

        viewModel.TimelineWorkspace.TimelineSearchText = "Asistence";
        viewModel.GlobalSearchWorkspace.GlobalSearchText = "Asistence";
        viewModel.AuditWorkspace.AuditSearchText = "Doklad bez cesty";
        viewModel.CostWorkspace.CostSearchText = "Octavia";

        Assert.True(viewModel.TimelineWorkspace.ClearTimelineSearchCommand.CanExecute(null));
        Assert.True(viewModel.GlobalSearchWorkspace.ClearGlobalSearchCommand.CanExecute(null));
        Assert.True(viewModel.AuditWorkspace.ClearAuditSearchCommand.CanExecute(null));
        Assert.True(viewModel.CostWorkspace.ClearCostSearchCommand.CanExecute(null));

        viewModel.TimelineWorkspace.ClearTimelineSearchCommand.Execute(null);
        viewModel.GlobalSearchWorkspace.ClearGlobalSearchCommand.Execute(null);
        viewModel.AuditWorkspace.ClearAuditSearchCommand.Execute(null);
        viewModel.CostWorkspace.ClearCostSearchCommand.Execute(null);

        Assert.Equal(string.Empty, viewModel.TimelineWorkspace.TimelineSearchText);
        Assert.Equal(string.Empty, viewModel.GlobalSearchWorkspace.GlobalSearchText);
        Assert.Equal(string.Empty, viewModel.AuditWorkspace.AuditSearchText);
        Assert.Equal(string.Empty, viewModel.CostWorkspace.CostSearchText);
        Assert.Equal(initialTimelineCount, viewModel.TimelineWorkspace.SelectedVehicleTimeline.Count);
        Assert.Empty(viewModel.GlobalSearchWorkspace.GlobalSearchResults);
        Assert.Equal(initialAuditCount, viewModel.AuditWorkspace.VisibleAuditItems.Count);
        Assert.Equal(initialCostCount, viewModel.CostWorkspace.VisibleCostVehicles.Count);
        Assert.False(viewModel.TimelineWorkspace.ClearTimelineSearchCommand.CanExecute(null));
        Assert.False(viewModel.GlobalSearchWorkspace.ClearGlobalSearchCommand.CanExecute(null));
        Assert.False(viewModel.AuditWorkspace.ClearAuditSearchCommand.CanExecute(null));
        Assert.False(viewModel.CostWorkspace.ClearCostSearchCommand.CanExecute(null));
        Assert.Equal(
            [
                DesktopFocusTarget.TimelineSearch,
                DesktopFocusTarget.GlobalSearchBox,
                DesktopFocusTarget.AuditSearch,
                DesktopFocusTarget.CostSearch
            ],
            requestedTargets);
    }

    [Fact]
    public void Cost_workspace_refresh_recomputes_summary_preserves_filter_and_notifies_status()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        var costStatusNotified = false;
        viewModel.FocusRequested += target => requestedFocus = target;
        viewModel.CostWorkspace.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "CostExportStatus")
            {
                costStatusNotified = true;
            }
        };

        viewModel.CostWorkspace.CostSearchText = "Octavia";
        var selectedCostVehicle = Assert.Single(viewModel.CostWorkspace.VisibleCostVehicles);
        viewModel.CostWorkspace.SelectedDashboardCostVehicle = selectedCostVehicle;

        viewModel.CostWorkspace.RefreshCostCommand.Execute(null);

        Assert.Equal(selectedCostVehicle.VehicleId, viewModel.CostWorkspace.SelectedDashboardCostVehicle?.VehicleId);
        Assert.Single(viewModel.CostWorkspace.VisibleCostVehicles);
        Assert.Equal(DesktopFocusTarget.CostList, requestedFocus);
        Assert.Equal("Nákladový přehled byl obnoven.", viewModel.CostWorkspace.CostExportStatus);
        Assert.Contains("Nákladový přehled byl obnoven", viewModel.ShellStatus, StringComparison.CurrentCulture);
        Assert.True(costStatusNotified);
    }

    [Fact]
    public void Cost_workspace_custom_period_recomputes_summary_and_persists_dates()
    {
        VehimapDataSet? dataSetRef = null;
        var viewModel = CreateViewModel(configureDataSet: dataSet =>
        {
            dataSetRef = dataSet;
            dataSet.HistoryEntries.Add(new VehicleHistoryEntry("hist_2", "veh_1", "10.02.2026", "Servis", "10200", "100", ""));
            dataSet.HistoryEntries.Add(new VehicleHistoryEntry("hist_3", "veh_1", "20.02.2026", "Servis", "10400", "200", ""));
        });

        viewModel.CostWorkspace.CostPeriodStartText = "01.02.2026";
        viewModel.CostWorkspace.CostPeriodEndText = "28.02.2026";
        viewModel.CostWorkspace.ApplyCostPeriodCommand.Execute(null);

        Assert.Equal("Vlastní období", viewModel.CostWorkspace.SelectedCostPeriodPreset);
        Assert.Equal("01.02.2026", viewModel.CostWorkspace.CostPeriodStartText);
        Assert.Equal("28.02.2026", viewModel.CostWorkspace.CostPeriodEndText);
        Assert.Contains("Od 01.02.2026 do 28.02.2026", viewModel.CostWorkspace.CostSummary, StringComparison.CurrentCulture);
        Assert.Equal("custom", dataSetRef!.Settings.GetValue("costs", "period_preset"));
        Assert.Equal("2026-02-01", dataSetRef.Settings.GetValue("costs", "period_start"));
        Assert.Equal("2026-02-28", dataSetRef.Settings.GetValue("costs", "period_end"));
        Assert.Contains("Období nákladů bylo použito", viewModel.CostWorkspace.CostPeriodStatus, StringComparison.CurrentCulture);
    }

    [Fact]
    public void Cost_workspace_loads_saved_custom_period_preferences()
    {
        var viewModel = CreateViewModel(configureDataSet: dataSet =>
        {
            dataSet.Settings.SetValue("costs", "period_preset", "Vlastní období");
            dataSet.Settings.SetValue("costs", "period_start", "2026-02-01");
            dataSet.Settings.SetValue("costs", "period_end", "2026-02-28");
        });

        Assert.Equal("Vlastní období", viewModel.CostWorkspace.SelectedCostPeriodPreset);
        Assert.Equal("01.02.2026", viewModel.CostWorkspace.CostPeriodStartText);
        Assert.Equal("28.02.2026", viewModel.CostWorkspace.CostPeriodEndText);
        Assert.Contains("Od 01.02.2026 do 28.02.2026", viewModel.CostWorkspace.CostSummary, StringComparison.CurrentCulture);
    }

    [Fact]
    public void Focus_dashboard_command_switches_to_dashboard_tab_and_requests_dashboard_focus()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.FocusDashboardCommand.Execute(null);

        Assert.Equal(DesktopTabIndexes.Dashboard, viewModel.SelectedVehicleTabIndex);
        Assert.Equal(DesktopFocusTarget.DashboardAuditList, requestedFocus);
    }

    [Fact]
    public void Refresh_dashboard_command_preserves_panel_selection_and_requests_dashboard_focus()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        var selectedAudit = viewModel.DashboardWorkspace.AuditItems.First();
        var selectedCost = viewModel.CostWorkspace.CostVehicles.First();
        var selectedTimeline = viewModel.DashboardWorkspace.DashboardUpcomingTimeline.First();

        viewModel.AuditWorkspace.SelectedDashboardAuditItem = selectedAudit;
        viewModel.CostWorkspace.SelectedDashboardCostVehicle = selectedCost;
        viewModel.DashboardWorkspace.SelectedDashboardTimelineItem = selectedTimeline;

        viewModel.DashboardWorkspace.RefreshDashboardCommand.Execute(null);

        Assert.Equal(selectedAudit.EntityId, viewModel.AuditWorkspace.SelectedDashboardAuditItem?.EntityId);
        Assert.Equal(selectedCost.VehicleId, viewModel.CostWorkspace.SelectedDashboardCostVehicle?.VehicleId);
        Assert.Equal(selectedTimeline.EntryId, viewModel.DashboardWorkspace.SelectedDashboardTimelineItem?.EntryId);
        Assert.Equal(DesktopFocusTarget.DashboardAuditList, requestedFocus);
        Assert.Contains("Dashboard byl obnoven", viewModel.ShellStatus, StringComparison.CurrentCulture);
    }

    [Fact]
    public void Dashboard_workspace_navigation_commands_open_search_and_overviews()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;

        viewModel.DashboardWorkspace.FocusGlobalSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;
        viewModel.DashboardWorkspace.FocusUpcomingOverviewCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;
        viewModel.DashboardWorkspace.FocusOverdueOverviewCommand.Execute(null);

        Assert.Equal(
            [
                DesktopFocusTarget.GlobalSearchBox,
                DesktopFocusTarget.UpcomingOverviewSearch,
                DesktopFocusTarget.OverdueOverviewSearch
            ],
            requestedTargets);
        Assert.Equal(DesktopTabIndexes.OverdueOverview, viewModel.SelectedVehicleTabIndex);
    }

    [Fact]
    public async Task Dashboard_contextual_shortcuts_open_term_vehicle_and_edit_vehicle()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        var editorRequests = new List<VehicleEditorDialogRequest>();
        viewModel.FocusRequested += requestedTargets.Add;
        viewModel.VehicleEditorDialogRequested += (_, request) => editorRequests.Add(request);

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;
        viewModel.CostWorkspace.SelectedDashboardCostVehicle = viewModel.CostWorkspace.CostVehicles.Single(item => item.VehicleId == "veh_1");
        viewModel.DashboardWorkspace.SelectedDashboardTimelineItem = viewModel.DashboardWorkspace.DashboardUpcomingTimeline.First(item => item.Kind == "custom");

        var primaryHandled = await viewModel.HandleCurrentWorkspacePrimaryOpenShortcutAsync();

        Assert.True(primaryHandled);
        Assert.Equal(DesktopTabIndexes.Detail, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("veh_1", viewModel.SelectedVehicle?.Id);
        Assert.Contains(DesktopFocusTarget.VehicleList, requestedTargets);

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;

        var itemHandled = await viewModel.HandleCurrentWorkspaceItemOpenShortcutAsync();

        Assert.True(itemHandled);
        Assert.Equal(DesktopTabIndexes.Reminder, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rem_1", viewModel.ReminderWorkspace.SelectedReminder?.Id);
        Assert.Contains(DesktopFocusTarget.ReminderList, requestedTargets);

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;
        viewModel.CostWorkspace.SelectedDashboardCostVehicle = viewModel.CostWorkspace.CostVehicles.Single(item => item.VehicleId == "veh_1");

        var editHandled = await viewModel.HandleCurrentWorkspaceEditShortcutAsync();

        Assert.True(editHandled);
        Assert.True(viewModel.VehicleDetailWorkspace.IsEditingVehicle);
        Assert.Equal(DesktopTabIndexes.Dashboard, viewModel.SelectedVehicleTabIndex);
        var editorRequest = Assert.Single(editorRequests);
        Assert.Equal(DesktopFocusTarget.DashboardCostList, editorRequest.ReturnFocusTarget);
        Assert.DoesNotContain(DesktopFocusTarget.VehicleEditorName, requestedTargets);
    }

    [Fact]
    public async Task Dashboard_action_buttons_open_history_costs_and_prepare_maintenance_completion()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;
        var completionRequested = false;
        viewModel.DashboardWorkspace.DashboardMaintenanceCompletionRequested += (_, _) => completionRequested = true;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;
        viewModel.DashboardWorkspace.SelectedDashboardTimelineItem = viewModel.DashboardWorkspace.DashboardUpcomingTimeline.First(item => item.Kind == "custom");

        Assert.False(viewModel.DashboardWorkspace.CompleteSelectedDashboardMaintenanceCommand.CanExecute(null));

        await viewModel.OpenSelectedDashboardVehicleHistoryCommand.ExecuteAsync(null);

        Assert.Equal(DesktopTabIndexes.History, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("hist_1", viewModel.HistoryWorkspace.SelectedHistory?.Id);
        Assert.Contains(DesktopFocusTarget.HistoryList, requestedTargets);

        await viewModel.OpenDashboardCostOverviewCommand.ExecuteAsync(null);

        Assert.Equal(DesktopTabIndexes.Cost, viewModel.SelectedVehicleTabIndex);
        Assert.Contains(DesktopFocusTarget.CostList, requestedTargets);

        await viewModel.OpenSelectedDashboardVehicleCostsCommand.ExecuteAsync(null);

        Assert.Equal(DesktopTabIndexes.Cost, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("veh_1", viewModel.CostWorkspace.SelectedDashboardCostVehicle?.VehicleId);
        Assert.Contains(DesktopFocusTarget.CostList, requestedTargets);

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;
        viewModel.DashboardWorkspace.SelectedDashboardTimelineItem = viewModel.DashboardWorkspace.DashboardUpcomingTimeline.First(item => item.Kind == "maintenance");

        Assert.True(viewModel.DashboardWorkspace.CompleteSelectedDashboardMaintenanceCommand.CanExecute(null));

        await viewModel.DashboardWorkspace.CompleteSelectedDashboardMaintenanceCommand.ExecuteAsync(null);

        Assert.True(completionRequested);
        Assert.Equal(DesktopTabIndexes.Maintenance, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("mnt_1", viewModel.MaintenanceWorkspace.SelectedMaintenance?.Id);
        Assert.NotNull(viewModel.DashboardWorkspace.BuildDashboardMaintenanceCompletionDialogViewModel());
        Assert.Contains(DesktopFocusTarget.MaintenanceList, requestedTargets);
    }

    [Fact]
    public async Task Contextual_primary_open_shortcut_opens_search_result_and_handles_empty_search_selection()
    {
        var viewModel = CreateViewModel();

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Search;
        viewModel.GlobalSearchWorkspace.GlobalSearchText = "Asistence";
        viewModel.GlobalSearchWorkspace.SelectedSearchResult = viewModel.GlobalSearchWorkspace.GlobalSearchResults.Single(item => item.EntityId == "rec_2");

        var handled = await viewModel.HandleCurrentWorkspacePrimaryOpenShortcutAsync();

        Assert.True(handled);
        Assert.Equal(DesktopTabIndexes.Record, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rec_2", viewModel.RecordWorkspace.SelectedRecord?.Id);

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Search;
        viewModel.GlobalSearchWorkspace.SelectedSearchResult = null;

        handled = await viewModel.HandleCurrentWorkspacePrimaryOpenShortcutAsync();

        Assert.True(handled);
        Assert.Equal(DesktopTabIndexes.Search, viewModel.SelectedVehicleTabIndex);
    }

    [Fact]
    public void Global_search_refresh_preserves_selection_and_requests_list_focus()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.GlobalSearchWorkspace.GlobalSearchText = "Asistence";
        var selectedResult = viewModel.GlobalSearchWorkspace.GlobalSearchResults.Single(item => item.EntityId == "rec_2");
        viewModel.GlobalSearchWorkspace.SelectedSearchResult = selectedResult;

        viewModel.GlobalSearchWorkspace.RefreshGlobalSearchCommand.Execute(null);

        Assert.Equal(selectedResult.EntityId, viewModel.GlobalSearchWorkspace.SelectedSearchResult?.EntityId);
        Assert.Equal(DesktopFocusTarget.GlobalSearchList, requestedFocus);
        Assert.Contains("Globální hledání bylo obnoveno", viewModel.ShellStatus, StringComparison.CurrentCulture);
    }

    [Fact]
    public async Task Contextual_item_open_shortcut_opens_timeline_item_and_ignores_non_context_tabs()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Timeline;
        viewModel.TimelineWorkspace.SelectedTimelineItem = viewModel.TimelineWorkspace.SelectedVehicleTimeline.First(item => item.Kind == "custom");

        var handled = await viewModel.HandleCurrentWorkspaceItemOpenShortcutAsync();

        Assert.True(handled);
        Assert.Equal(DesktopTabIndexes.Reminder, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rem_1", viewModel.ReminderWorkspace.SelectedReminder?.Id);
        Assert.Equal(DesktopFocusTarget.ReminderList, requestedFocus);

        handled = await viewModel.HandleCurrentWorkspaceItemOpenShortcutAsync();

        Assert.False(handled);
    }

    [Fact]
    public void Timeline_refresh_preserves_selection_and_requests_list_focus()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        var selectedTimelineItem = viewModel.TimelineWorkspace.SelectedVehicleTimeline.First(item => item.Kind == "custom");
        viewModel.TimelineWorkspace.SelectedTimelineItem = selectedTimelineItem;

        viewModel.TimelineWorkspace.RefreshTimelineCommand.Execute(null);

        Assert.Equal(selectedTimelineItem.EntryId, viewModel.TimelineWorkspace.SelectedTimelineItem?.EntryId);
        Assert.Equal(DesktopFocusTarget.TimelineList, requestedFocus);
        Assert.Contains("Časová osa byla obnovena", viewModel.ShellStatus, StringComparison.CurrentCulture);
    }

    [Fact]
    public void Timeline_filter_preference_is_restored_from_settings()
    {
        var viewModel = CreateViewModel(configureDataSet: dataSet =>
            dataSet.Settings.SetValue("timeline", "filter", "Budoucí"));

        Assert.Equal("Budoucí", viewModel.TimelineWorkspace.SelectedTimelineFilter);
        Assert.NotEmpty(viewModel.TimelineWorkspace.SelectedVehicleTimeline);
        Assert.All(viewModel.TimelineWorkspace.SelectedVehicleTimeline, item => Assert.True(item.IsFuture));
    }

    [Fact]
    public void Timeline_filter_changes_are_saved_to_settings()
    {
        VehimapDataSet? dataSetRef = null;
        var viewModel = CreateViewModel(configureDataSet: dataSet => dataSetRef = dataSet);

        viewModel.TimelineWorkspace.SelectedTimelineFilter = "Minulé";

        Assert.Equal("past", dataSetRef?.Settings.GetValue("timeline", "filter", string.Empty));
        Assert.NotEmpty(viewModel.TimelineWorkspace.SelectedVehicleTimeline);
        Assert.All(viewModel.TimelineWorkspace.SelectedVehicleTimeline, item => Assert.False(item.IsFuture));
    }

    [Fact]
    public void Unknown_timeline_filter_preference_falls_back_to_all_items()
    {
        VehimapDataSet? dataSetRef = null;
        var viewModel = CreateViewModel(configureDataSet: dataSet =>
        {
            dataSet.Settings.SetValue("timeline", "filter", "Neznámý filtr");
            dataSetRef = dataSet;
        });

        Assert.Equal("Vše", viewModel.TimelineWorkspace.SelectedTimelineFilter);
        Assert.Contains(viewModel.TimelineWorkspace.SelectedVehicleTimeline, item => item.IsFuture);
        Assert.Contains(viewModel.TimelineWorkspace.SelectedVehicleTimeline, item => !item.IsFuture);

        viewModel.TimelineWorkspace.SelectedTimelineFilter = "Neznámý filtr";

        Assert.Equal("Vše", viewModel.TimelineWorkspace.SelectedTimelineFilter);
        Assert.Equal("all", dataSetRef?.Settings.GetValue("timeline", "filter", string.Empty));
    }

    [Fact]
    public void Audit_search_filters_visible_audit_items_without_changing_dashboard_source()
    {
        var viewModel = CreateViewModel();

        viewModel.AuditWorkspace.AuditSearchText = "Doklad bez cesty";

        Assert.NotEmpty(viewModel.AuditWorkspace.VisibleAuditItems);
        Assert.All(viewModel.AuditWorkspace.VisibleAuditItems, item =>
            Assert.Contains("Doklad bez cesty", item.AccessibleLabel, StringComparison.CurrentCultureIgnoreCase));
        Assert.Equal("rec_2", viewModel.AuditWorkspace.SelectedDashboardAuditItem?.EntityId);
        Assert.True(viewModel.DashboardWorkspace.AuditItems.Count <= viewModel.AuditWorkspace.AuditItems.Count);
    }

    [Fact]
    public void Audit_refresh_preserves_filtered_selection_and_requests_list_focus()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.AuditWorkspace.AuditSearchText = "Doklad bez cesty";
        var selectedAuditItem = Assert.Single(viewModel.AuditWorkspace.VisibleAuditItems);
        viewModel.AuditWorkspace.SelectedDashboardAuditItem = selectedAuditItem;

        viewModel.AuditWorkspace.RefreshAuditCommand.Execute(null);

        Assert.Equal(selectedAuditItem.EntityId, viewModel.AuditWorkspace.SelectedDashboardAuditItem?.EntityId);
        Assert.Equal(DesktopFocusTarget.AuditList, requestedFocus);
        Assert.Contains("Audit dat byl obnoven", viewModel.ShellStatus, StringComparison.CurrentCulture);
        Assert.Contains("Hledání", viewModel.AuditWorkspace.AuditSummary, StringComparison.CurrentCulture);
    }

    [Fact]
    public async Task Audit_primary_open_shortcut_opens_vehicle_detail()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Audit;
        viewModel.AuditWorkspace.SelectedDashboardAuditItem = viewModel.AuditWorkspace.AuditItems.First(item => item.EntityId == "rec_2");

        var handled = await viewModel.HandleCurrentWorkspacePrimaryOpenShortcutAsync();

        Assert.True(handled);
        Assert.Equal(DesktopTabIndexes.Detail, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("veh_1", viewModel.SelectedVehicle?.Id);
        Assert.Equal(DesktopFocusTarget.VehicleList, requestedFocus);
    }

    [Fact]
    public async Task Audit_edit_shortcut_opens_nearest_relevant_editor()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        WorkspaceEditorDialogRequest? dialogRequest = null;
        viewModel.FocusRequested += target => requestedFocus = target;
        viewModel.WorkspaceEditorDialogRequested += (_, request) => dialogRequest = request;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Audit;
        viewModel.AuditWorkspace.SelectedDashboardAuditItem = viewModel.AuditWorkspace.AuditItems.First(item => item.EntityId == "rec_2");

        var handled = await viewModel.HandleCurrentWorkspaceEditShortcutAsync();

        Assert.True(handled);
        Assert.Equal(DesktopTabIndexes.Record, viewModel.SelectedVehicleTabIndex);
        Assert.True(viewModel.RecordWorkspace.IsEditingRecord);
        Assert.Equal("Asistence", viewModel.RecordWorkspace.RecordEditorTitle);
        Assert.Equal(DesktopFocusTarget.RecordList, requestedFocus);
        Assert.NotNull(dialogRequest);
        Assert.Equal(WorkspaceEditorKind.Record, dialogRequest.Kind);
        Assert.Equal(DesktopFocusTarget.AuditList, dialogRequest.ReturnFocusTarget);
    }

    [Fact]
    public void Contextual_create_shortcut_uses_active_evidence_workspace()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        WorkspaceEditorDialogRequest? dialogRequest = null;
        viewModel.FocusRequested += target => requestedFocus = target;
        viewModel.WorkspaceEditorDialogRequested += (_, request) => dialogRequest = request;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.History;

        var handled = viewModel.HandleCurrentWorkspaceCreateShortcut();

        Assert.True(handled);
        Assert.True(viewModel.HistoryWorkspace.IsEditingHistory);
        Assert.Equal(DesktopTabIndexes.History, viewModel.SelectedVehicleTabIndex);
        Assert.Null(requestedFocus);
        Assert.NotNull(dialogRequest);
        Assert.Equal(WorkspaceEditorKind.History, dialogRequest.Kind);
        Assert.Equal(DesktopFocusTarget.HistoryList, dialogRequest.ReturnFocusTarget);
    }

    [Fact]
    public void Contextual_edit_shortcut_uses_active_evidence_workspace()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        WorkspaceEditorDialogRequest? dialogRequest = null;
        viewModel.FocusRequested += target => requestedFocus = target;
        viewModel.WorkspaceEditorDialogRequested += (_, request) => dialogRequest = request;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Record;
        viewModel.RecordWorkspace.SelectedRecord = viewModel.RecordWorkspace.SelectedVehicleRecords.Single(item => item.Id == "rec_2");

        var handled = viewModel.HandleCurrentWorkspaceEditShortcut();

        Assert.True(handled);
        Assert.True(viewModel.RecordWorkspace.IsEditingRecord);
        Assert.Equal("Asistence", viewModel.RecordWorkspace.RecordEditorTitle);
        Assert.Equal(DesktopTabIndexes.Record, viewModel.SelectedVehicleTabIndex);
        Assert.Null(requestedFocus);
        Assert.NotNull(dialogRequest);
        Assert.Equal(WorkspaceEditorKind.Record, dialogRequest.Kind);
        Assert.Equal(DesktopFocusTarget.RecordList, dialogRequest.ReturnFocusTarget);
    }

    [Fact]
    public async Task Contextual_save_shortcut_saves_active_evidence_editor()
    {
        var viewModel = CreateViewModel();

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.History;
        Assert.True(viewModel.HandleCurrentWorkspaceCreateShortcut());
        viewModel.HistoryWorkspace.HistoryEditorDate = "01.02.2026";
        viewModel.HistoryWorkspace.HistoryEditorType = "Test";
        viewModel.HistoryWorkspace.HistoryEditorOdometer = "11111";
        viewModel.HistoryWorkspace.HistoryEditorCost = "123";
        viewModel.HistoryWorkspace.HistoryEditorNote = "Ulozeno zkratkou";

        var handled = await viewModel.HandleCurrentWorkspaceSaveShortcutAsync();

        Assert.True(handled);
        Assert.False(viewModel.HistoryWorkspace.IsEditingHistory);
        Assert.Contains(viewModel.HistoryWorkspace.SelectedVehicleHistory, item => item.Note == "Ulozeno zkratkou");
        Assert.Equal(DesktopTabIndexes.History, viewModel.SelectedVehicleTabIndex);
    }

    [Fact]
    public async Task Contextual_primary_open_shortcut_opens_record_attachment_in_record_tab()
    {
        var attachmentPath = Path.GetTempFileName();
        var fileLauncher = new CapturingFileLauncher();

        try
        {
            var viewModel = CreateViewModel(fileLauncher: fileLauncher, recordFilePath: attachmentPath);

            viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Record;
            viewModel.RecordWorkspace.SelectedRecord = viewModel.RecordWorkspace.SelectedVehicleRecords.Single(item => item.Id == "rec_1");

            var handled = await viewModel.HandleCurrentWorkspacePrimaryOpenShortcutAsync();

            Assert.True(handled);
            Assert.Equal(attachmentPath, fileLauncher.LastOpenedPath);
        }
        finally
        {
            File.Delete(attachmentPath);
        }
    }

    [Fact]
    public async Task Copy_selected_record_path_command_copies_resolved_attachment_path()
    {
        var attachmentPath = Path.GetTempFileName();
        var clipboard = new CapturingClipboardService();

        try
        {
            var viewModel = CreateViewModel(clipboardService: clipboard, recordFilePath: attachmentPath);

            viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Record;
            viewModel.RecordWorkspace.SelectedRecord = viewModel.RecordWorkspace.SelectedVehicleRecords.Single(item => item.Id == "rec_1");

            Assert.True(viewModel.CopySelectedRecordPathCommand.CanExecute(null));

            await viewModel.CopySelectedRecordPathCommand.ExecuteAsync(null);

            Assert.Equal(attachmentPath, clipboard.LastCopiedText);
            Assert.Contains("zkopírována", viewModel.RecordWorkspace.RecordEditorStatus, StringComparison.CurrentCulture);
            Assert.Equal(viewModel.RecordWorkspace.RecordEditorStatus, viewModel.ShellStatus);
        }
        finally
        {
            File.Delete(attachmentPath);
        }
    }

    [Fact]
    public async Task Open_selected_record_file_failure_reports_status_without_crashing_shell()
    {
        var attachmentPath = Path.GetTempFileName();
        var fileLauncher = new FailingFileLauncher(new InvalidOperationException("Soubor nejde otevřít."));

        try
        {
            var viewModel = CreateViewModel(fileLauncher: fileLauncher, recordFilePath: attachmentPath);

            viewModel.RecordWorkspace.SelectedRecord = viewModel.RecordWorkspace.SelectedVehicleRecords.Single(item => item.Id == "rec_1");

            await viewModel.OpenSelectedRecordFileCommand.ExecuteAsync(null);

            Assert.Contains("Přílohu dokladu se nepodařilo otevřít", viewModel.RecordWorkspace.RecordEditorStatus);
            Assert.Contains("Soubor nejde otevřít", viewModel.RecordWorkspace.RecordEditorStatus);
            Assert.Equal(viewModel.RecordWorkspace.RecordEditorStatus, viewModel.ShellStatus);
        }
        finally
        {
            File.Delete(attachmentPath);
        }
    }

    [Fact]
    public async Task Open_selected_record_folder_failure_reports_status_without_crashing_shell()
    {
        var attachmentPath = Path.GetTempFileName();
        var fileLauncher = new FailingFileLauncher(new InvalidOperationException("Složka nejde otevřít."), failFolders: true);

        try
        {
            var viewModel = CreateViewModel(fileLauncher: fileLauncher, recordFilePath: attachmentPath);

            viewModel.RecordWorkspace.SelectedRecord = viewModel.RecordWorkspace.SelectedVehicleRecords.Single(item => item.Id == "rec_1");

            await viewModel.OpenSelectedRecordFolderCommand.ExecuteAsync(null);

            Assert.Contains("Složku přílohy dokladu se nepodařilo otevřít", viewModel.RecordWorkspace.RecordEditorStatus);
            Assert.Contains("Složka nejde otevřít", viewModel.RecordWorkspace.RecordEditorStatus);
            Assert.Equal(viewModel.RecordWorkspace.RecordEditorStatus, viewModel.ShellStatus);
        }
        finally
        {
            File.Delete(attachmentPath);
        }
    }

    [Fact]
    public async Task Copy_selected_record_path_failure_reports_status_without_crashing_shell()
    {
        var attachmentPath = Path.GetTempFileName();
        var clipboard = new FailingClipboardService(new InvalidOperationException("Schránka není dostupná."));

        try
        {
            var viewModel = CreateViewModel(clipboardService: clipboard, recordFilePath: attachmentPath);

            viewModel.RecordWorkspace.SelectedRecord = viewModel.RecordWorkspace.SelectedVehicleRecords.Single(item => item.Id == "rec_1");

            await viewModel.CopySelectedRecordPathCommand.ExecuteAsync(null);

            Assert.Contains("Cestu dokladu se nepodařilo zkopírovat", viewModel.RecordWorkspace.RecordEditorStatus);
            Assert.Contains("Schránka není dostupná", viewModel.RecordWorkspace.RecordEditorStatus);
            Assert.Equal(viewModel.RecordWorkspace.RecordEditorStatus, viewModel.ShellStatus);
        }
        finally
        {
            File.Delete(attachmentPath);
        }
    }

    [Fact]
    public async Task Selected_vehicle_cost_command_opens_cost_tab_and_selects_current_vehicle()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.OpenSelectedVehicleCostsCommand.ExecuteAsync(null);

        Assert.Equal(DesktopTabIndexes.Cost, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("veh_1", viewModel.CostWorkspace.SelectedDashboardCostVehicle?.VehicleId);
        Assert.Contains("Palivo:", viewModel.CostWorkspace.SelectedCostVehicleDetail);
        Assert.Equal(DesktopFocusTarget.CostList, requestedFocus);
    }

    [Fact]
    public async Task Cost_workspace_shortcuts_focus_detail_open_vehicle_and_edit_vehicle()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        var editorRequests = new List<VehicleEditorDialogRequest>();
        viewModel.FocusRequested += requestedTargets.Add;
        viewModel.VehicleEditorDialogRequested += (_, request) => editorRequests.Add(request);

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Cost;
        viewModel.CostWorkspace.SelectedDashboardCostVehicle = viewModel.CostWorkspace.CostVehicles.Single(item => item.VehicleId == "veh_1");

        var itemHandled = await viewModel.HandleCurrentWorkspaceItemOpenShortcutAsync();

        Assert.True(itemHandled);
        Assert.Contains(DesktopFocusTarget.CostDetail, requestedTargets);

        var primaryHandled = await viewModel.HandleCurrentWorkspacePrimaryOpenShortcutAsync();

        Assert.True(primaryHandled);
        Assert.Equal(DesktopTabIndexes.Detail, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("veh_1", viewModel.SelectedVehicle?.Id);

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Cost;
        viewModel.CostWorkspace.SelectedDashboardCostVehicle = viewModel.CostWorkspace.CostVehicles.Single(item => item.VehicleId == "veh_1");

        var editHandled = await viewModel.HandleCurrentWorkspaceEditShortcutAsync();

        Assert.True(editHandled);
        Assert.True(viewModel.VehicleDetailWorkspace.IsEditingVehicle);
        Assert.Equal(DesktopTabIndexes.Cost, viewModel.SelectedVehicleTabIndex);
        var editorRequest = Assert.Single(editorRequests);
        Assert.Equal(DesktopFocusTarget.CostList, editorRequest.ReturnFocusTarget);
        Assert.DoesNotContain(DesktopFocusTarget.VehicleEditorName, requestedTargets);
    }

    [Fact]
    public async Task Cost_export_command_saves_fleet_summary_tsv()
    {
        var saveService = new CapturingTextFileSaveService(@"C:\exports\naklady.tsv");
        var viewModel = CreateViewModel(saveService);

        await viewModel.ExportFleetCostSummaryCommand.ExecuteAsync(null);

        Assert.Equal("Export souhrnu nákladů", saveService.LastTitle);
        Assert.Equal("TSV soubor", saveService.LastFileTypeName);
        Assert.Equal("tsv", saveService.LastDefaultExtension);
        Assert.Contains("Octavia", saveService.LastContent);
        Assert.Contains("Palivo", saveService.LastContent);
        Assert.Contains("Souhrn nákladů byl uložen", viewModel.CostWorkspace.CostExportStatus);
        Assert.Equal(viewModel.CostWorkspace.CostExportStatus, viewModel.ShellStatus);
    }

    [Fact]
    public async Task Cost_detail_export_command_saves_selected_vehicle_tsv()
    {
        var saveService = new CapturingTextFileSaveService(@"C:\exports\octavia-naklady.tsv");
        var viewModel = CreateViewModel(saveService);

        viewModel.CostWorkspace.SelectedDashboardCostVehicle = viewModel.CostWorkspace.CostVehicles.Single(item => item.VehicleId == "veh_1");

        await viewModel.ExportSelectedVehicleCostDetailCommand.ExecuteAsync(null);

        Assert.Equal("Export detailu nákladů", saveService.LastTitle);
        Assert.Equal("TSV soubor", saveService.LastFileTypeName);
        Assert.Equal("tsv", saveService.LastDefaultExtension);
        Assert.Contains("Octavia", saveService.LastContent);
        Assert.Contains("Tankování", saveService.LastContent);
        Assert.Contains("Detail nákladů byl uložen", viewModel.CostWorkspace.CostExportStatus);
        Assert.Equal(viewModel.CostWorkspace.CostExportStatus, viewModel.ShellStatus);
    }

    [Fact]
    public async Task Cost_export_save_failure_reports_status_without_crashing_shell()
    {
        var viewModel = CreateViewModel(new FailingTextFileSaveService(new IOException("Cílový soubor nelze zapsat.")));

        await viewModel.ExportFleetCostSummaryCommand.ExecuteAsync(null);

        Assert.Contains("Export souhrnu nákladů se nepodařil", viewModel.CostWorkspace.CostExportStatus);
        Assert.Contains("Cílový soubor nelze zapsat", viewModel.CostWorkspace.CostExportStatus);
        Assert.Equal(viewModel.CostWorkspace.CostExportStatus, viewModel.ShellStatus);
    }

    [Fact]
    public async Task Cost_html_report_open_failure_reports_saved_path_and_error()
    {
        var saveService = new CapturingTextFileSaveService(@"C:\exports\octavia-naklady.html");
        var fileLauncher = new FailingFileLauncher(new InvalidOperationException("Prohlížeč není dostupný."));
        var viewModel = CreateViewModel(saveService, fileLauncher);

        viewModel.CostWorkspace.SelectedDashboardCostVehicle = viewModel.CostWorkspace.CostVehicles.Single(item => item.VehicleId == "veh_1");

        await viewModel.ExportSelectedVehicleCostReportCommand.ExecuteAsync(null);

        Assert.Equal("Export HTML sestavy nákladů", saveService.LastTitle);
        Assert.Equal("HTML soubor", saveService.LastFileTypeName);
        Assert.Equal("html", saveService.LastDefaultExtension);
        Assert.Contains("<!DOCTYPE html>", saveService.LastContent);
        Assert.Contains(@"C:\exports\octavia-naklady.html", viewModel.CostWorkspace.CostExportStatus);
        Assert.Contains("nepodařilo se ji otevřít", viewModel.CostWorkspace.CostExportStatus);
        Assert.Contains("Prohlížeč není dostupný", viewModel.CostWorkspace.CostExportStatus);
        Assert.Equal(viewModel.CostWorkspace.CostExportStatus, viewModel.ShellStatus);
    }

    [Fact]
    public async Task Calendar_export_command_saves_ics_and_updates_shell_status()
    {
        var saveService = new CapturingTextFileSaveService(@"C:\exports\terminy.ics");
        var viewModel = CreateViewModel(saveService);

        await viewModel.ExportCalendarCommand.ExecuteAsync(null);

        Assert.Equal("Export termínů do kalendáře", saveService.LastTitle);
        Assert.Contains("BEGIN:VCALENDAR", saveService.LastContent);
        Assert.Contains("Kalendář uložen", viewModel.TimelineWorkspace.ExportStatus);
        Assert.Contains(@"C:\exports\terminy.ics", viewModel.TimelineWorkspace.ExportStatus);
        Assert.Equal(viewModel.TimelineWorkspace.ExportStatus, viewModel.ShellStatus);
    }

    [Fact]
    public async Task Calendar_export_save_failure_reports_status_without_crashing_shell()
    {
        var viewModel = CreateViewModel(new FailingTextFileSaveService(new IOException("Cílovou složku nelze zapsat.")));

        await viewModel.ExportCalendarCommand.ExecuteAsync(null);

        Assert.Contains("Export kalendáře se nepodařil", viewModel.TimelineWorkspace.ExportStatus);
        Assert.Contains("Cílovou složku nelze zapsat", viewModel.TimelineWorkspace.ExportStatus);
        Assert.Equal(viewModel.TimelineWorkspace.ExportStatus, viewModel.ShellStatus);
    }

    [Fact]
    public async Task Calendar_export_build_failure_reports_status_without_crashing_shell()
    {
        var viewModel = CreateViewModel(calendarExportService: new FailingCalendarExportService("Generátor ICS selhal."));

        await viewModel.ExportCalendarCommand.ExecuteAsync(null);

        Assert.Contains("Export kalendáře se nepodařil", viewModel.TimelineWorkspace.ExportStatus);
        Assert.Contains("Generátor ICS selhal", viewModel.TimelineWorkspace.ExportStatus);
        Assert.Equal(viewModel.TimelineWorkspace.ExportStatus, viewModel.ShellStatus);
    }

    [Fact]
    public void Global_search_result_opens_matching_record_and_requests_focus()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.GlobalSearchWorkspace.GlobalSearchText = "Asistence";
        viewModel.GlobalSearchWorkspace.SelectedSearchResult = viewModel.GlobalSearchWorkspace.GlobalSearchResults.Single(item => item.EntityId == "rec_2");

        viewModel.OpenSelectedSearchResultCommand.Execute(null);

        Assert.Equal(6, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rec_2", viewModel.RecordWorkspace.SelectedRecord?.Id);
        Assert.Equal(DesktopFocusTarget.RecordList, requestedFocus);
    }

    [Fact]
    public void Window_titles_and_open_flags_follow_selected_vehicle()
    {
        var viewModel = CreateViewModel();

        Assert.True(viewModel.CanOpenReminderWindow);
        Assert.True(viewModel.CanOpenRecordWindow);
        Assert.True(viewModel.CanOpenHistoryWindow);
        Assert.True(viewModel.CanOpenFuelWindow);
        Assert.True(viewModel.CanOpenMaintenanceWindow);
        Assert.True(viewModel.CanOpenVehicleDetailWindow);
        Assert.Equal("Historie - Octavia", viewModel.HistoryWorkspace.WindowTitle);
        Assert.Equal("Tankování - Octavia", viewModel.FuelWorkspace.WindowTitle);
        Assert.Equal("Připomínky - Octavia", viewModel.ReminderWorkspace.WindowTitle);
        Assert.Equal("Údržba - Octavia", viewModel.MaintenanceWorkspace.WindowTitle);
        Assert.Equal("Doklady a přílohy - Octavia", viewModel.RecordWorkspace.WindowTitle);
        Assert.Equal("Detail - Octavia", viewModel.VehicleDetailWorkspace.WindowTitle);
        Assert.Equal("Časová osa - Octavia", viewModel.TimelineWorkspace.WindowTitle);
        Assert.Equal("Audit dat", viewModel.AuditWorkspace.WindowTitle);
        Assert.Equal("Náklady napříč vozidly", viewModel.CostWorkspace.WindowTitle);
        Assert.Equal("Dashboard", viewModel.DashboardWorkspace.WindowTitle);
        Assert.Equal("Globální hledání", viewModel.GlobalSearchWorkspace.WindowTitle);
        Assert.Equal("Blížící se termíny", viewModel.UpcomingOverviewWorkspace.WindowTitle);
        Assert.Equal("Propadlé termíny", viewModel.OverdueOverviewWorkspace.WindowTitle);

        viewModel.SelectedVehicle = null;

        Assert.False(viewModel.CanOpenVehicleDetailWindow);
        Assert.False(viewModel.CanOpenHistoryWindow);
        Assert.False(viewModel.CanOpenFuelWindow);
        Assert.False(viewModel.CanOpenReminderWindow);
        Assert.False(viewModel.CanOpenMaintenanceWindow);
        Assert.False(viewModel.CanOpenRecordWindow);
        Assert.Equal("Detail vozidla", viewModel.VehicleDetailWorkspace.WindowTitle);
        Assert.Equal("Historie vozidla", viewModel.HistoryWorkspace.WindowTitle);
        Assert.Equal("Tankování vozidla", viewModel.FuelWorkspace.WindowTitle);
        Assert.Equal("Připomínky vozidla", viewModel.ReminderWorkspace.WindowTitle);
        Assert.Equal("Plán údržby vozidla", viewModel.MaintenanceWorkspace.WindowTitle);
        Assert.Equal("Doklady a přílohy", viewModel.RecordWorkspace.WindowTitle);
        Assert.Equal("Časová osa vozidla", viewModel.TimelineWorkspace.WindowTitle);
    }

    private static MainWindowViewModel CreateViewModel(
        ITextFileSaveService? textFileSaveService = null,
        IFileLauncher? fileLauncher = null,
        IClipboardService? clipboardService = null,
        string? recordFilePath = null,
        ICalendarExportService? calendarExportService = null,
        Action<VehimapDataSet>? configureDataSet = null)
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
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
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Chybějící příloha", "", "", "03/2027", "200", VehicleRecordAttachmentMode.External, recordFilePath ?? @"C:\missing\policy.pdf", "Prověřit"),
                new VehicleRecord("rec_2", "veh_1", "Asistence", "Asistence", "", "", "08/2099", "", VehicleRecordAttachmentMode.External, "", "")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Objednat servis", "01.12.2099", "30", "Ročně", "Zavolat servisu")
            ],
            MaintenancePlans =
            [
                new MaintenancePlan("mnt_1", "veh_1", "Motorový olej", "15000", "12", "10.01.2026", "10000", true, "Každý rok")
            ]
        };

        configureDataSet?.Invoke(dataSet);

        var bootstrapper = new LegacyVehimapBootstrapper(
            new StubDataRootLocator(dataRoot),
            new StubLegacyDataStore(dataSet));

        return new MainWindowViewModel(
            new StubLegacyDataStore(dataSet),
            bootstrapper,
            new ManagedAttachmentPathService(),
            fileLauncher ?? new StubFileLauncher(),
            new StubFilePickerService(),
            new LegacyGlobalSearchService(new ManagedAttachmentPathService()),
            new LegacyTimelineService(),
            calendarExportService ?? new LegacyCalendarExportService(),
            textFileSaveService ?? new StubTextFileSaveService(),
            clipboardService: clipboardService);
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

    private sealed class StubLegacyDataStore : ILegacyDataStore
    {
        private readonly VehimapDataSet _dataSet;

        public StubLegacyDataStore(VehimapDataSet dataSet)
        {
            _dataSet = dataSet;
        }

        public Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
            => Task.FromResult(_dataSet);

        public Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StubFileLauncher : IFileLauncher
    {
        public Task OpenAsync(string path, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task OpenFolderAsync(string path, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class CapturingFileLauncher : IFileLauncher
    {
        public string LastOpenedPath { get; private set; } = string.Empty;

        public Task OpenAsync(string path, CancellationToken cancellationToken = default)
        {
            LastOpenedPath = path;
            return Task.CompletedTask;
        }

        public Task OpenFolderAsync(string path, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FailingFileLauncher : IFileLauncher
    {
        private readonly Exception _exception;
        private readonly bool _failFolders;

        public FailingFileLauncher(Exception exception, bool failFolders = false)
        {
            _exception = exception;
            _failFolders = failFolders;
        }

        public Task OpenAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromException(_exception);

        public Task OpenFolderAsync(string path, CancellationToken cancellationToken = default)
            => _failFolders ? Task.FromException(_exception) : Task.CompletedTask;
    }

    private sealed class CapturingClipboardService : IClipboardService
    {
        public string LastCopiedText { get; private set; } = string.Empty;

        public Task SetTextAsync(string text, CancellationToken cancellationToken = default)
        {
            LastCopiedText = text;
            return Task.CompletedTask;
        }
    }

    private sealed class FailingClipboardService : IClipboardService
    {
        private readonly Exception _exception;

        public FailingClipboardService(Exception exception)
        {
            _exception = exception;
        }

        public Task SetTextAsync(string text, CancellationToken cancellationToken = default)
            => Task.FromException(_exception);
    }

    private sealed class StubTextFileSaveService : ITextFileSaveService
    {
        public Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class CapturingTextFileSaveService : ITextFileSaveService
    {
        private readonly string _path;

        public CapturingTextFileSaveService(string path)
        {
            _path = path;
        }

        public string LastTitle { get; private set; } = string.Empty;
        public string LastFileTypeName { get; private set; } = string.Empty;
        public string LastDefaultExtension { get; private set; } = string.Empty;
        public string LastContent { get; private set; } = string.Empty;

        public Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default)
        {
            LastTitle = title;
            LastContent = content;
            return Task.FromResult<string?>(_path);
        }

        public Task<string?> SaveTextAsync(
            string title,
            string suggestedFileName,
            string content,
            string fileTypeName,
            string defaultExtension,
            IReadOnlyList<string> patterns,
            CancellationToken cancellationToken = default)
        {
            LastTitle = title;
            LastFileTypeName = fileTypeName;
            LastDefaultExtension = defaultExtension;
            LastContent = content;
            return Task.FromResult<string?>(_path);
        }
    }

    private sealed class FailingTextFileSaveService : ITextFileSaveService
    {
        private readonly Exception _exception;

        public FailingTextFileSaveService(Exception exception)
        {
            _exception = exception;
        }

        public Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default)
            => Task.FromException<string?>(_exception);
    }

    private sealed class FailingCalendarExportService : ICalendarExportService
    {
        private readonly string _message;

        public FailingCalendarExportService(string message)
        {
            _message = message;
        }

        public CalendarExportResult BuildUpcomingCalendar(VehimapDataSet dataSet, DateOnly today, DateTimeOffset generatedAtUtc)
            => throw new InvalidOperationException(_message);
    }

    private sealed class StubFilePickerService : IFilePickerService
    {
        public Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }
}
