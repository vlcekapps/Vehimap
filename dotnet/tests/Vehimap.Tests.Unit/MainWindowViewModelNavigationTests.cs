using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
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

        viewModel.SelectedDashboardAuditItem = viewModel.AuditItems.First(item => item.EntityId == "rec_2");

        viewModel.OpenSelectedDashboardAuditItemCommand.Execute(null);

        Assert.Equal(6, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rec_2", viewModel.SelectedRecord?.Id);
        Assert.Equal(DesktopFocusTarget.RecordList, requestedFocus);
    }

    [Fact]
    public void Dashboard_timeline_command_opens_matching_reminder_and_requests_focus()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.SelectedDashboardTimelineItem = viewModel.DashboardUpcomingTimeline.First(item => item.Kind == "custom");

        viewModel.OpenSelectedDashboardTimelineItemCommand.Execute(null);

        Assert.Equal(3, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rem_1", viewModel.SelectedReminder?.Id);
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
    public void Focus_current_search_command_uses_active_workspace_or_vehicle_search()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Detail;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Timeline;
        viewModel.FocusCurrentSearchCommand.Execute(null);
        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Audit;
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
                DesktopFocusTarget.TimelineSearch,
                DesktopFocusTarget.AuditSearch,
                DesktopFocusTarget.GlobalSearchBox,
                DesktopFocusTarget.UpcomingOverviewSearch,
                DesktopFocusTarget.OverdueOverviewSearch
            ],
            requestedTargets);
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

        var selectedAudit = viewModel.DashboardAuditItems.First();
        var selectedCost = viewModel.CostVehicles.First();
        var selectedTimeline = viewModel.DashboardUpcomingTimeline.First();

        viewModel.SelectedDashboardAuditItem = selectedAudit;
        viewModel.SelectedDashboardCostVehicle = selectedCost;
        viewModel.SelectedDashboardTimelineItem = selectedTimeline;

        viewModel.DashboardWorkspace.RefreshDashboardCommand.Execute(null);

        Assert.Equal(selectedAudit.EntityId, viewModel.SelectedDashboardAuditItem?.EntityId);
        Assert.Equal(selectedCost.VehicleId, viewModel.SelectedDashboardCostVehicle?.VehicleId);
        Assert.Equal(selectedTimeline.EntryId, viewModel.SelectedDashboardTimelineItem?.EntryId);
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
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;
        viewModel.SelectedDashboardCostVehicle = viewModel.CostVehicles.Single(item => item.VehicleId == "veh_1");
        viewModel.SelectedDashboardTimelineItem = viewModel.DashboardUpcomingTimeline.First(item => item.Kind == "custom");

        var primaryHandled = await viewModel.HandleCurrentWorkspacePrimaryOpenShortcutAsync();

        Assert.True(primaryHandled);
        Assert.Equal(DesktopTabIndexes.Detail, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("veh_1", viewModel.SelectedVehicle?.Id);
        Assert.Contains(DesktopFocusTarget.VehicleList, requestedTargets);

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;

        var itemHandled = await viewModel.HandleCurrentWorkspaceItemOpenShortcutAsync();

        Assert.True(itemHandled);
        Assert.Equal(DesktopTabIndexes.Reminder, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rem_1", viewModel.SelectedReminder?.Id);
        Assert.Contains(DesktopFocusTarget.ReminderList, requestedTargets);

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Dashboard;
        viewModel.SelectedDashboardCostVehicle = viewModel.CostVehicles.Single(item => item.VehicleId == "veh_1");

        var editHandled = await viewModel.HandleCurrentWorkspaceEditShortcutAsync();

        Assert.True(editHandled);
        Assert.True(viewModel.IsEditingVehicle);
        Assert.Equal(DesktopTabIndexes.Detail, viewModel.SelectedVehicleTabIndex);
        Assert.Contains(DesktopFocusTarget.VehicleEditorName, requestedTargets);
    }

    [Fact]
    public async Task Contextual_primary_open_shortcut_opens_search_result_and_handles_empty_search_selection()
    {
        var viewModel = CreateViewModel();

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Search;
        viewModel.GlobalSearchText = "Asistence";
        viewModel.SelectedSearchResult = viewModel.GlobalSearchResults.Single(item => item.EntityId == "rec_2");

        var handled = await viewModel.HandleCurrentWorkspacePrimaryOpenShortcutAsync();

        Assert.True(handled);
        Assert.Equal(DesktopTabIndexes.Record, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rec_2", viewModel.SelectedRecord?.Id);

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Search;
        viewModel.SelectedSearchResult = null;

        handled = await viewModel.HandleCurrentWorkspacePrimaryOpenShortcutAsync();

        Assert.True(handled);
        Assert.Equal(DesktopTabIndexes.Search, viewModel.SelectedVehicleTabIndex);
    }

    [Fact]
    public async Task Contextual_item_open_shortcut_opens_timeline_item_and_ignores_non_context_tabs()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Timeline;
        viewModel.SelectedTimelineItem = viewModel.SelectedVehicleTimeline.First(item => item.Kind == "custom");

        var handled = await viewModel.HandleCurrentWorkspaceItemOpenShortcutAsync();

        Assert.True(handled);
        Assert.Equal(DesktopTabIndexes.Reminder, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rem_1", viewModel.SelectedReminder?.Id);
        Assert.Equal(DesktopFocusTarget.ReminderList, requestedFocus);

        handled = await viewModel.HandleCurrentWorkspaceItemOpenShortcutAsync();

        Assert.False(handled);
    }

    [Fact]
    public void Audit_search_filters_visible_audit_items_without_changing_dashboard_source()
    {
        var viewModel = CreateViewModel();

        viewModel.AuditWorkspace.AuditSearchText = "Doklad bez cesty";

        Assert.NotEmpty(viewModel.AuditWorkspace.VisibleAuditItems);
        Assert.All(viewModel.AuditWorkspace.VisibleAuditItems, item =>
            Assert.Contains("Doklad bez cesty", item.AccessibleLabel, StringComparison.CurrentCultureIgnoreCase));
        Assert.Equal("rec_2", viewModel.SelectedDashboardAuditItem?.EntityId);
        Assert.True(viewModel.DashboardWorkspace.AuditItems.Count <= viewModel.AuditItems.Count);
    }

    [Fact]
    public async Task Audit_primary_open_shortcut_opens_vehicle_detail()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Audit;
        viewModel.SelectedDashboardAuditItem = viewModel.AuditItems.First(item => item.EntityId == "rec_2");

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
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Audit;
        viewModel.SelectedDashboardAuditItem = viewModel.AuditItems.First(item => item.EntityId == "rec_2");

        var handled = await viewModel.HandleCurrentWorkspaceEditShortcutAsync();

        Assert.True(handled);
        Assert.Equal(DesktopTabIndexes.Record, viewModel.SelectedVehicleTabIndex);
        Assert.True(viewModel.IsEditingRecord);
        Assert.Equal("Asistence", viewModel.RecordEditorTitle);
        Assert.Equal(DesktopFocusTarget.RecordEditorTitle, requestedFocus);
    }

    [Fact]
    public void Contextual_create_shortcut_uses_active_evidence_workspace()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.History;

        var handled = viewModel.HandleCurrentWorkspaceCreateShortcut();

        Assert.True(handled);
        Assert.True(viewModel.IsEditingHistory);
        Assert.Equal(DesktopTabIndexes.History, viewModel.SelectedVehicleTabIndex);
        Assert.Equal(DesktopFocusTarget.HistoryEditorDate, requestedFocus);
    }

    [Fact]
    public void Contextual_edit_shortcut_uses_active_evidence_workspace()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Record;
        viewModel.SelectedRecord = viewModel.SelectedVehicleRecords.Single(item => item.Id == "rec_2");

        var handled = viewModel.HandleCurrentWorkspaceEditShortcut();

        Assert.True(handled);
        Assert.True(viewModel.IsEditingRecord);
        Assert.Equal("Asistence", viewModel.RecordEditorTitle);
        Assert.Equal(DesktopTabIndexes.Record, viewModel.SelectedVehicleTabIndex);
        Assert.Equal(DesktopFocusTarget.RecordEditorTitle, requestedFocus);
    }

    [Fact]
    public async Task Contextual_save_shortcut_saves_active_evidence_editor()
    {
        var viewModel = CreateViewModel();

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.History;
        Assert.True(viewModel.HandleCurrentWorkspaceCreateShortcut());
        viewModel.HistoryEditorDate = "01.02.2026";
        viewModel.HistoryEditorType = "Test";
        viewModel.HistoryEditorOdometer = "11111";
        viewModel.HistoryEditorCost = "123";
        viewModel.HistoryEditorNote = "Ulozeno zkratkou";

        var handled = await viewModel.HandleCurrentWorkspaceSaveShortcutAsync();

        Assert.True(handled);
        Assert.False(viewModel.IsEditingHistory);
        Assert.Contains(viewModel.SelectedVehicleHistory, item => item.Note == "Ulozeno zkratkou");
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
            viewModel.SelectedRecord = viewModel.SelectedVehicleRecords.Single(item => item.Id == "rec_1");

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
            viewModel.SelectedRecord = viewModel.SelectedVehicleRecords.Single(item => item.Id == "rec_1");

            Assert.True(viewModel.CopySelectedRecordPathCommand.CanExecute(null));

            await viewModel.CopySelectedRecordPathCommand.ExecuteAsync(null);

            Assert.Equal(attachmentPath, clipboard.LastCopiedText);
            Assert.Contains("zkopírována", viewModel.RecordEditorStatus, StringComparison.CurrentCulture);
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
        Assert.Equal("veh_1", viewModel.SelectedDashboardCostVehicle?.VehicleId);
        Assert.Contains("Palivo:", viewModel.CostWorkspace.SelectedCostVehicleDetail);
        Assert.Equal(DesktopFocusTarget.CostList, requestedFocus);
    }

    [Fact]
    public async Task Cost_workspace_shortcuts_focus_detail_open_vehicle_and_edit_vehicle()
    {
        var viewModel = CreateViewModel();
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Cost;
        viewModel.SelectedDashboardCostVehicle = viewModel.CostVehicles.Single(item => item.VehicleId == "veh_1");

        var itemHandled = await viewModel.HandleCurrentWorkspaceItemOpenShortcutAsync();

        Assert.True(itemHandled);
        Assert.Contains(DesktopFocusTarget.CostDetail, requestedTargets);

        var primaryHandled = await viewModel.HandleCurrentWorkspacePrimaryOpenShortcutAsync();

        Assert.True(primaryHandled);
        Assert.Equal(DesktopTabIndexes.Detail, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("veh_1", viewModel.SelectedVehicle?.Id);

        viewModel.SelectedVehicleTabIndex = DesktopTabIndexes.Cost;
        viewModel.SelectedDashboardCostVehicle = viewModel.CostVehicles.Single(item => item.VehicleId == "veh_1");

        var editHandled = await viewModel.HandleCurrentWorkspaceEditShortcutAsync();

        Assert.True(editHandled);
        Assert.True(viewModel.IsEditingVehicle);
        Assert.Equal(DesktopTabIndexes.Detail, viewModel.SelectedVehicleTabIndex);
        Assert.Contains(DesktopFocusTarget.VehicleEditorName, requestedTargets);
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
        Assert.Contains("Souhrn nákladů byl uložen", viewModel.CostExportStatus);
    }

    [Fact]
    public void Global_search_result_opens_matching_record_and_requests_focus()
    {
        var viewModel = CreateViewModel();
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.GlobalSearchText = "Asistence";
        viewModel.SelectedSearchResult = viewModel.GlobalSearchResults.Single(item => item.EntityId == "rec_2");

        viewModel.OpenSelectedSearchResultCommand.Execute(null);

        Assert.Equal(6, viewModel.SelectedVehicleTabIndex);
        Assert.Equal("rec_2", viewModel.SelectedRecord?.Id);
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
        Assert.Equal("Historie - Octavia", viewModel.HistoryWindowTitle);
        Assert.Equal("Tankování - Octavia", viewModel.FuelWindowTitle);
        Assert.Equal("Připomínky - Octavia", viewModel.ReminderWindowTitle);
        Assert.Equal("Údržba - Octavia", viewModel.MaintenanceWindowTitle);
        Assert.Equal("Doklady a přílohy - Octavia", viewModel.RecordWindowTitle);
        Assert.Equal("Detail - Octavia", viewModel.VehicleDetailWindowTitle);
        Assert.Equal("Časová osa - Octavia", viewModel.TimelineWindowTitle);
        Assert.Equal("Audit dat", viewModel.AuditWindowTitle);
        Assert.Equal("Náklady napříč vozidly", viewModel.CostWindowTitle);
        Assert.Equal("Dashboard", viewModel.DashboardWindowTitle);
        Assert.Equal("Globální hledání", viewModel.GlobalSearchWindowTitle);
        Assert.Equal("Blížící se termíny", viewModel.UpcomingOverviewWindowTitle);
        Assert.Equal("Propadlé termíny", viewModel.OverdueOverviewWindowTitle);

        viewModel.SelectedVehicle = null;

        Assert.False(viewModel.CanOpenVehicleDetailWindow);
        Assert.False(viewModel.CanOpenHistoryWindow);
        Assert.False(viewModel.CanOpenFuelWindow);
        Assert.False(viewModel.CanOpenReminderWindow);
        Assert.False(viewModel.CanOpenMaintenanceWindow);
        Assert.False(viewModel.CanOpenRecordWindow);
        Assert.Equal("Detail vozidla", viewModel.VehicleDetailWindowTitle);
        Assert.Equal("Historie vozidla", viewModel.HistoryWindowTitle);
        Assert.Equal("Tankování vozidla", viewModel.FuelWindowTitle);
        Assert.Equal("Připomínky vozidla", viewModel.ReminderWindowTitle);
        Assert.Equal("Plán údržby vozidla", viewModel.MaintenanceWindowTitle);
        Assert.Equal("Doklady a přílohy", viewModel.RecordWindowTitle);
        Assert.Equal("Časová osa vozidla", viewModel.TimelineWindowTitle);
    }

    private static MainWindowViewModel CreateViewModel(
        ITextFileSaveService? textFileSaveService = null,
        IFileLauncher? fileLauncher = null,
        IClipboardService? clipboardService = null,
        string? recordFilePath = null)
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
            new LegacyCalendarExportService(),
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

    private sealed class CapturingClipboardService : IClipboardService
    {
        public string LastCopiedText { get; private set; } = string.Empty;

        public Task SetTextAsync(string text, CancellationToken cancellationToken = default)
        {
            LastCopiedText = text;
            return Task.CompletedTask;
        }
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

    private sealed class StubFilePickerService : IFilePickerService
    {
        public Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }
}
