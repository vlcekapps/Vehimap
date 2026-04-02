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
        Assert.Equal("Historie - Octavia", viewModel.HistoryWindowTitle);
        Assert.Equal("Tankování - Octavia", viewModel.FuelWindowTitle);
        Assert.Equal("Připomínky - Octavia", viewModel.ReminderWindowTitle);
        Assert.Equal("Údržba - Octavia", viewModel.MaintenanceWindowTitle);
        Assert.Equal("Doklady a přílohy - Octavia", viewModel.RecordWindowTitle);

        viewModel.SelectedVehicle = null;

        Assert.False(viewModel.CanOpenHistoryWindow);
        Assert.False(viewModel.CanOpenFuelWindow);
        Assert.False(viewModel.CanOpenReminderWindow);
        Assert.False(viewModel.CanOpenMaintenanceWindow);
        Assert.False(viewModel.CanOpenRecordWindow);
        Assert.Equal("Historie vozidla", viewModel.HistoryWindowTitle);
        Assert.Equal("Tankování vozidla", viewModel.FuelWindowTitle);
        Assert.Equal("Připomínky vozidla", viewModel.ReminderWindowTitle);
        Assert.Equal("Plán údržby vozidla", viewModel.MaintenanceWindowTitle);
        Assert.Equal("Doklady a přílohy", viewModel.RecordWindowTitle);
    }

    private static MainWindowViewModel CreateViewModel()
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
                new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Chybějící příloha", "", "", "03/2027", "200", VehicleRecordAttachmentMode.External, @"C:\missing\policy.pdf", "Prověřit"),
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
            new StubFileLauncher(),
            new StubFilePickerService(),
            new LegacyGlobalSearchService(new ManagedAttachmentPathService()),
            new LegacyTimelineService(),
            new LegacyCalendarExportService(),
            new StubTextFileSaveService());
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

    private sealed class StubTextFileSaveService : ITextFileSaveService
    {
        public Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class StubFilePickerService : IFilePickerService
    {
        public Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }
}
