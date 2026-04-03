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

public sealed class MainWindowViewModelVehicleListAndQuickActionsTests
{
    [Fact]
    public void Projection_service_filters_vehicle_list_by_category_search_and_inactive_state()
    {
        var projectionService = new DesktopProjectionService();
        var timelineService = new LegacyTimelineService();
        var dataSet = new VehimapDataSet
        {
            Settings = BuildSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "05/2099", "01/2099", "03/2099"),
                new Vehicle("veh_2", "Drobeček", "Autobusy", "Meziměsto", "Karosa C734.20", "4C3 3359", "1992", "154", "", "10/2099", "03/2099", "04/2099"),
                new Vehicle("veh_3", "Božena", "Osobní vozidla", "Srazové", "Škoda 100", "", "1973", "30", "", "09/2099", "", "")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "Běžný provoz", "rodina", "Benzín", "Má klimatizaci", "Řemen", "Manuální"),
                new VehicleMeta("veh_2", "Odstaveno", "autobus", "Nafta", "", "", ""),
                new VehicleMeta("veh_3", "Veterán", "veterán", "Benzín", "", "", "")
            ]
        };

        var projection = projectionService.BuildVehicleList(
            dataSet,
            dataSet.VehicleMetaEntries.ToDictionary(item => item.VehicleId, StringComparer.Ordinal),
            [],
            timelineService,
            new DesktopVehicleListFilters(
                "Milena",
                "Osobní vozidla",
                MainWindowViewModel.AllVehicleStatusFilterLabel,
                true),
            new DateOnly(2026, 4, 3));

        var item = Assert.Single(projection.Items);
        Assert.Equal("veh_1", item.Id);
        Assert.Contains("zobrazeno 1 z 3", projection.Summary, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task Open_nearest_technical_quick_action_selects_matching_vehicle()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.OpenNearestTechnicalCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsDetailTabSelected);
        Assert.Equal("Milena", viewModel.SelectedVehicle?.Name);
        Assert.Equal(DesktopFocusTarget.VehicleList, requestedFocus);
        Assert.Contains("Nejbližší technická kontrola", viewModel.ShellStatus, StringComparison.CurrentCultureIgnoreCase);
    }

    [Fact]
    public async Task Review_green_cards_quick_action_opens_matching_overview_filter()
    {
        var viewModel = CreateViewModel(BuildQuickActionDataSet());
        DesktopFocusTarget? requestedFocus = null;
        viewModel.FocusRequested += target => requestedFocus = target;

        await viewModel.ReviewGreenCardsCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsUpcomingOverviewTabSelected);
        Assert.Equal("Zelené karty", viewModel.SelectedUpcomingOverviewFilter);
        Assert.Equal(DesktopFocusTarget.UpcomingOverviewList, requestedFocus);
        Assert.NotEmpty(viewModel.UpcomingOverviewItems);
    }

    private static MainWindowViewModel CreateViewModel(VehimapDataSet dataSet)
    {
        var dataRoot = new VehimapDataRoot(@"C:\vehimap-test", @"C:\vehimap-test\data", true);
        var dataStore = new VehicleListQuickActionStubLegacyDataStore(dataSet);
        var bootstrapper = new LegacyVehimapBootstrapper(new VehicleListQuickActionStubDataRootLocator(dataRoot), dataStore);

        return new MainWindowViewModel(
            dataStore,
            bootstrapper,
            new ManagedAttachmentPathService(),
            new VehicleListQuickActionStubFileLauncher(),
            new VehicleListQuickActionStubFilePickerService(),
            new LegacyGlobalSearchService(new ManagedAttachmentPathService()),
            new LegacyTimelineService(),
            new LegacyCalendarExportService(),
            new VehicleListQuickActionStubTextFileSaveService(),
            new VehicleListQuickActionStubBackupService(),
            new VehicleListQuickActionStubFileDialogService(),
            new DesktopSupportedSettingsService(),
            new VehicleListQuickActionStubBuildInfoProvider());
    }

    private static VehimapDataSet BuildQuickActionDataSet()
    {
        return new VehimapDataSet
        {
            Settings = BuildSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "05/2026", "01/2026", "06/2026"),
                new Vehicle("veh_2", "Božena", "Osobní vozidla", "Srazové", "Škoda 100", "", "1973", "30", "", "09/2099", "01/2099", "10/2099")
            ],
            VehicleMetaEntries =
            [
                new VehicleMeta("veh_1", "Běžný provoz", "", "Benzín", "Má klimatizaci", "Řemen", "Manuální"),
                new VehicleMeta("veh_2", "Veterán", "", "Benzín", "", "", "")
            ]
        };
    }

    private static VehimapSettings BuildSettings()
    {
        var settings = new VehimapSettings();
        settings.SetValue("notifications", "technical_reminder_days", "365");
        settings.SetValue("notifications", "green_card_reminder_days", "365");
        settings.SetValue("notifications", "maintenance_reminder_days", "31");
        settings.SetValue("notifications", "maintenance_reminder_km", "1000");
        return settings;
    }

    private sealed class VehicleListQuickActionStubDataRootLocator : IDataRootLocator
    {
        private readonly VehimapDataRoot _dataRoot;

        public VehicleListQuickActionStubDataRootLocator(VehimapDataRoot dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public VehimapDataRoot Resolve(string appBasePath) => _dataRoot;
    }

    private sealed class VehicleListQuickActionStubLegacyDataStore : ILegacyDataStore
    {
        public VehicleListQuickActionStubLegacyDataStore(VehimapDataSet dataSet)
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

    private sealed class VehicleListQuickActionStubFileLauncher : IFileLauncher
    {
        public Task OpenAsync(string path, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task OpenFolderAsync(string path, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class VehicleListQuickActionStubTextFileSaveService : ITextFileSaveService
    {
        public Task<string?> SaveTextAsync(string title, string suggestedFileName, string content, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class VehicleListQuickActionStubFilePickerService : IFilePickerService
    {
        public Task<string?> PickFileAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class VehicleListQuickActionStubFileDialogService : IFileDialogService
    {
        public Task<string?> PickOpenFileAsync(string title, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);

        public Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string fileTypeName, string defaultExtension, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);
    }

    private sealed class VehicleListQuickActionStubBuildInfoProvider : IAppBuildInfoProvider
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

    private sealed class VehicleListQuickActionStubBackupService : IBackupService
    {
        public Task ExportAsync(string backupPath, VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<VehimapBackupBundle> ImportAsync(string backupPath, CancellationToken cancellationToken = default) =>
            Task.FromResult(new VehimapBackupBundle(new VehimapDataSet(), []));

        public Task RestoreAsync(VehimapDataRoot dataRoot, VehimapBackupBundle backupBundle, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
