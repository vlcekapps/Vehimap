using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class MainWindowViewModelOverviewTests
{
    [Fact]
    public void Load_builds_upcoming_and_overdue_overview_lists_from_timeline_data()
    {
        var viewModel = CreateViewModel(BuildOverviewDataSet());

        Assert.Equal(2, viewModel.UpcomingOverviewItems.Count);
        Assert.Equal(2, viewModel.OverdueOverviewItems.Count);
        Assert.Contains(viewModel.UpcomingOverviewItems, item => item.Kind == "technical");
        Assert.Contains(viewModel.UpcomingOverviewItems, item => item.Kind == "custom");
        Assert.Contains(viewModel.OverdueOverviewItems, item => item.Kind == "green");
        Assert.Contains(viewModel.OverdueOverviewItems, item => item.Kind == "record");
        Assert.DoesNotContain(viewModel.UpcomingOverviewItems, item => item.Kind is "history" or "fuel");
    }

    [Fact]
    public void Focus_overview_commands_select_correct_tab_and_request_search_focus()
    {
        var requestedFocus = DesktopFocusTarget.VehicleList;
        var viewModel = CreateViewModel(BuildOverviewDataSet());
        viewModel.FocusRequested += target => requestedFocus = target;

        viewModel.FocusUpcomingOverviewCommand.Execute(null);
        Assert.True(viewModel.IsUpcomingOverviewTabSelected);
        Assert.Equal(DesktopFocusTarget.UpcomingOverviewSearch, requestedFocus);

        viewModel.FocusOverdueOverviewCommand.Execute(null);
        Assert.True(viewModel.IsOverdueOverviewTabSelected);
        Assert.Equal(DesktopFocusTarget.OverdueOverviewSearch, requestedFocus);
    }

    [Fact]
    public void Opening_upcoming_overview_item_navigates_to_matching_vehicle_workflow()
    {
        var viewModel = CreateViewModel(BuildOverviewDataSet());
        viewModel.SelectedUpcomingOverviewItem = viewModel.UpcomingOverviewItems.First(item => item.Kind == "custom");

        viewModel.OpenSelectedUpcomingOverviewItemCommand.Execute(null);

        Assert.True(viewModel.IsReminderTabSelected);
        Assert.Equal("Přezutí", viewModel.SelectedReminder?.Title);
    }

    [Fact]
    public void Opening_overdue_overview_vehicle_navigates_back_to_vehicle_detail()
    {
        var viewModel = CreateViewModel(BuildOverviewDataSet());
        viewModel.SelectedOverdueOverviewItem = viewModel.OverdueOverviewItems.First(item => item.Kind == "green");

        viewModel.OpenSelectedOverdueOverviewVehicleCommand.Execute(null);

        Assert.True(viewModel.IsDetailTabSelected);
        Assert.Equal("Milena", viewModel.SelectedVehicle?.Name);
    }

    private static MainWindowViewModel CreateViewModel(VehimapDataSet dataSet)
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataStore = new StubLegacyDataStore(dataSet);
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
            new StubTextFileSaveService(),
            new StubBackupService(),
            new StubFileDialogService(),
            new DesktopSupportedSettingsService(),
            new StubBuildInfoProvider());
    }

    private static VehimapDataSet BuildOverviewDataSet()
    {
        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "05/2026", "01/2026", "03/2026")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", "10.03.2026", "Servis", "12000", "100", "Kontrola")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", "28.03.2026", "12450", "40", "300", true, "Natural 95", "")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Přezutí", "10.04.2026", "7", "ročně", "Objednat pneuservis")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Doklad", "Povinné ručení", "", "", "03/2026", "2000", VehicleRecordAttachmentMode.External, "", "")
            ]
        };

        dataSet.Settings.SetValue("notifications", "technical_reminder_days", "31");
        dataSet.Settings.SetValue("notifications", "green_card_reminder_days", "31");
        dataSet.Settings.SetValue("notifications", "maintenance_reminder_days", "31");
        dataSet.Settings.SetValue("notifications", "maintenance_reminder_km", "1000");
        return dataSet;
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
        public StubLegacyDataStore(VehimapDataSet dataSet)
        {
            CurrentDataSet = dataSet;
        }

        public VehimapDataSet CurrentDataSet { get; set; }

        public Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
            => Task.FromResult(CurrentDataSet);

        public Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
        {
            CurrentDataSet = dataSet;
            return Task.CompletedTask;
        }
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

    private sealed class StubFileDialogService : IFileDialogService
    {
        public Task<string?> PickOpenFileAsync(string title, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);

        public Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class StubBuildInfoProvider : IAppBuildInfoProvider
    {
        public AppBuildInfo GetCurrent() => new(
            "Vehimap",
            "1.0.2",
            "1.0.2.0",
            "vývojový Avalonia shell",
            @"C:\vehimap\Vehimap.Desktop.exe",
            "Windows",
            ".NET 10",
            "https://example.com/latest.ini",
            "https://example.com/release",
            @"C:\vehimap\Vehimap.Updater.exe",
            false);
    }

    private sealed class StubBackupService : IBackupService
    {
        public Task ExportAsync(string backupPath, VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default) =>
            Task.FromResult(new VehimapBackupBundle(new VehimapDataSet(), []));

        public Task RestoreAsync(VehimapDataRoot dataRoot, VehimapBackupBundle backupBundle, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
