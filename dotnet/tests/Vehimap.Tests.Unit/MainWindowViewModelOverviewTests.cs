using System.Globalization;
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

    [Fact]
    public void Refresh_overview_commands_preserve_selection_and_request_list_focus()
    {
        var requestedFocus = DesktopFocusTarget.VehicleList;
        var viewModel = CreateViewModel(BuildOverviewDataSet());
        viewModel.FocusRequested += target => requestedFocus = target;

        var selectedUpcoming = viewModel.UpcomingOverviewItems.First(item => item.Kind == "custom");
        viewModel.SelectedUpcomingOverviewItem = selectedUpcoming;

        viewModel.UpcomingOverviewWorkspace.RefreshUpcomingOverviewCommand.Execute(null);

        Assert.Equal(selectedUpcoming.EntryId, viewModel.SelectedUpcomingOverviewItem?.EntryId);
        Assert.Equal(DesktopFocusTarget.UpcomingOverviewList, requestedFocus);
        Assert.Contains("blížících se termínů byl obnoven", viewModel.ShellStatus, StringComparison.CurrentCulture);

        var selectedOverdue = viewModel.OverdueOverviewItems.First(item => item.Kind == "record");
        viewModel.SelectedOverdueOverviewItem = selectedOverdue;

        viewModel.OverdueOverviewWorkspace.RefreshOverdueOverviewCommand.Execute(null);

        Assert.Equal(selectedOverdue.EntryId, viewModel.SelectedOverdueOverviewItem?.EntryId);
        Assert.Equal(DesktopFocusTarget.OverdueOverviewList, requestedFocus);
        Assert.Contains("propadlých termínů byl obnoven", viewModel.ShellStatus, StringComparison.CurrentCulture);
    }

    [Fact]
    public void Overview_clear_search_commands_restore_lists_and_focus_search_fields()
    {
        var requestedTargets = new List<DesktopFocusTarget>();
        var viewModel = CreateViewModel(BuildOverviewDataSet());
        viewModel.FocusRequested += requestedTargets.Add;
        var initialUpcomingCount = viewModel.UpcomingOverviewItems.Count;
        var initialOverdueCount = viewModel.OverdueOverviewItems.Count;

        viewModel.UpcomingOverviewWorkspace.UpcomingOverviewSearchText = "Přezutí";
        viewModel.OverdueOverviewWorkspace.OverdueOverviewSearchText = "Povinné";

        Assert.True(viewModel.UpcomingOverviewWorkspace.ClearUpcomingOverviewSearchCommand.CanExecute(null));
        Assert.True(viewModel.OverdueOverviewWorkspace.ClearOverdueOverviewSearchCommand.CanExecute(null));

        viewModel.UpcomingOverviewWorkspace.ClearUpcomingOverviewSearchCommand.Execute(null);
        viewModel.OverdueOverviewWorkspace.ClearOverdueOverviewSearchCommand.Execute(null);

        Assert.Equal(string.Empty, viewModel.UpcomingOverviewWorkspace.UpcomingOverviewSearchText);
        Assert.Equal(string.Empty, viewModel.OverdueOverviewWorkspace.OverdueOverviewSearchText);
        Assert.Equal(initialUpcomingCount, viewModel.UpcomingOverviewItems.Count);
        Assert.Equal(initialOverdueCount, viewModel.OverdueOverviewItems.Count);
        Assert.False(viewModel.UpcomingOverviewWorkspace.ClearUpcomingOverviewSearchCommand.CanExecute(null));
        Assert.False(viewModel.OverdueOverviewWorkspace.ClearOverdueOverviewSearchCommand.CanExecute(null));
        Assert.Equal(
            [
                DesktopFocusTarget.UpcomingOverviewSearch,
                DesktopFocusTarget.OverdueOverviewSearch
            ],
            requestedTargets);
    }

    [Fact]
    public void Upcoming_overview_can_include_missing_green_cards_and_audit_data_issues()
    {
        var dataSet = BuildOverviewDataSet();
        var viewModel = CreateViewModel(dataSet);

        Assert.DoesNotContain(viewModel.UpcomingOverviewItems, item => item.Title == "Chybí zelená karta");
        Assert.DoesNotContain(viewModel.UpcomingOverviewItems, item => item.Kind == "data_issue");

        viewModel.IncludeMissingGreenCardsInUpcomingOverview = true;
        viewModel.IncludeDataIssuesInUpcomingOverview = true;

        Assert.Equal("1", dataSet.Settings.GetValue("overview", "include_missing_green", "0"));
        Assert.Equal("1", dataSet.Settings.GetValue("overview", "include_data_issues", "0"));
        Assert.Contains(viewModel.UpcomingOverviewItems, item => item.Kind == "green" && item.Title == "Chybí zelená karta");
        Assert.Contains(viewModel.UpcomingOverviewItems, item => item.Kind == "data_issue" && item.EntryId == "rec_1");
        Assert.Contains("Datové nedostatky", viewModel.UpcomingOverviewWorkspace.OverviewFilters);
        Assert.DoesNotContain("Datové nedostatky", viewModel.OverdueOverviewWorkspace.OverviewFilters);
        Assert.Contains("datových nedostatků", viewModel.UpcomingOverviewSummary, StringComparison.CurrentCulture);
    }

    [Fact]
    public void Overview_filter_preferences_are_restored_from_settings()
    {
        var dataSet = BuildOverviewDataSet();
        dataSet.Settings.SetValue("overview", "upcoming_filter", "Technické kontroly");
        dataSet.Settings.SetValue("overview", "overdue_filter", "Doklady");

        var viewModel = CreateViewModel(dataSet);

        Assert.Equal("Technické kontroly", viewModel.SelectedUpcomingOverviewFilter);
        Assert.Single(viewModel.UpcomingOverviewItems);
        Assert.All(viewModel.UpcomingOverviewItems, item => Assert.Equal("technical", item.Kind));
        Assert.Equal("Doklady", viewModel.SelectedOverdueOverviewFilter);
        Assert.Single(viewModel.OverdueOverviewItems);
        Assert.All(viewModel.OverdueOverviewItems, item => Assert.Equal("record", item.Kind));
    }

    [Fact]
    public void Overview_filter_changes_are_saved_to_settings()
    {
        var dataSet = BuildOverviewDataSet();
        var viewModel = CreateViewModel(dataSet);

        viewModel.SelectedUpcomingOverviewFilter = "Připomínky";
        viewModel.SelectedOverdueOverviewFilter = "Zelené karty";

        Assert.Equal("Připomínky", dataSet.Settings.GetValue("overview", "upcoming_filter", string.Empty));
        Assert.Equal("Zelené karty", dataSet.Settings.GetValue("overview", "overdue_filter", string.Empty));
        Assert.Single(viewModel.UpcomingOverviewItems);
        Assert.All(viewModel.UpcomingOverviewItems, item => Assert.Equal("custom", item.Kind));
        Assert.Single(viewModel.OverdueOverviewItems);
        Assert.All(viewModel.OverdueOverviewItems, item => Assert.Equal("green", item.Kind));
    }

    [Fact]
    public void Overview_sort_preferences_are_restored_and_saved_to_settings()
    {
        var dataSet = BuildOverviewDataSet();
        var upcomingDate = DateOnly.FromDateTime(DateTime.Today).AddDays(14).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        var overdueDate = DateOnly.FromDateTime(DateTime.Today).AddMonths(-2).ToString("MM/yyyy", CultureInfo.InvariantCulture);
        dataSet.Reminders.Add(new VehicleReminder("rem_2", "veh_2", "Kontrola veterána", upcomingDate, "7", "ročně", ""));
        dataSet.Records.Add(new VehicleRecord("rec_2", "veh_2", "Doklad", "Veteránský doklad", "", "", overdueDate, "", VehicleRecordAttachmentMode.External, "", ""));
        dataSet.Settings.SetValue("overview", "upcoming_sort", WorkspaceSortHelpers.VehicleSortLabel);
        dataSet.Settings.SetValue("overview", "upcoming_sort_descending", "0");
        dataSet.Settings.SetValue("overview", "overdue_sort", WorkspaceSortHelpers.VehicleSortLabel);
        dataSet.Settings.SetValue("overview", "overdue_sort_descending", "0");

        var viewModel = CreateViewModel(dataSet);

        Assert.Equal(WorkspaceSortHelpers.VehicleSortLabel, viewModel.UpcomingOverviewWorkspace.SelectedUpcomingOverviewSortOption);
        Assert.Equal(WorkspaceSortHelpers.VehicleSortLabel, viewModel.OverdueOverviewWorkspace.SelectedOverdueOverviewSortOption);
        Assert.Equal("Božena", viewModel.UpcomingOverviewItems.First().VehicleName);
        Assert.Equal("Božena", viewModel.OverdueOverviewItems.First().VehicleName);

        viewModel.UpcomingOverviewWorkspace.SelectedUpcomingOverviewSortOption = WorkspaceSortHelpers.StatusSortLabel;
        viewModel.UpcomingOverviewWorkspace.UpcomingOverviewSortDescending = true;
        viewModel.OverdueOverviewWorkspace.SelectedOverdueOverviewSortOption = WorkspaceSortHelpers.TitleSortLabel;
        viewModel.OverdueOverviewWorkspace.OverdueOverviewSortDescending = true;

        Assert.Equal(WorkspaceSortHelpers.StatusSortLabel, dataSet.Settings.GetValue("overview", "upcoming_sort", string.Empty));
        Assert.Equal("1", dataSet.Settings.GetValue("overview", "upcoming_sort_descending", string.Empty));
        Assert.Equal(WorkspaceSortHelpers.TitleSortLabel, dataSet.Settings.GetValue("overview", "overdue_sort", string.Empty));
        Assert.Equal("1", dataSet.Settings.GetValue("overview", "overdue_sort_descending", string.Empty));
    }

    [Fact]
    public void Unknown_overview_filter_preferences_fall_back_to_all_items()
    {
        var dataSet = BuildOverviewDataSet();
        dataSet.Settings.SetValue("overview", "upcoming_filter", "Neznámý filtr");
        dataSet.Settings.SetValue("overview", "overdue_filter", "Neznámý filtr");

        var viewModel = CreateViewModel(dataSet);

        Assert.Equal("Vše", viewModel.SelectedUpcomingOverviewFilter);
        Assert.Equal("Vše", viewModel.SelectedOverdueOverviewFilter);
        Assert.Equal(2, viewModel.UpcomingOverviewItems.Count);
        Assert.Equal(2, viewModel.OverdueOverviewItems.Count);

        viewModel.SelectedUpcomingOverviewFilter = "Neznámý filtr";
        viewModel.SelectedOverdueOverviewFilter = "Neznámý filtr";

        Assert.Equal("Vše", viewModel.SelectedUpcomingOverviewFilter);
        Assert.Equal("Vše", viewModel.SelectedOverdueOverviewFilter);
        Assert.Equal("Vše", dataSet.Settings.GetValue("overview", "upcoming_filter", string.Empty));
        Assert.Equal("Vše", dataSet.Settings.GetValue("overview", "overdue_filter", string.Empty));
    }

    [Fact]
    public void Opening_upcoming_overview_data_issue_navigates_to_audit_target()
    {
        var viewModel = CreateViewModel(BuildOverviewDataSet());
        viewModel.IncludeDataIssuesInUpcomingOverview = true;
        viewModel.SelectedUpcomingOverviewItem = viewModel.UpcomingOverviewItems.First(item => item.Kind == "data_issue" && item.EntryId == "rec_1");

        viewModel.OpenSelectedUpcomingOverviewItemCommand.Execute(null);

        Assert.True(viewModel.IsRecordTabSelected);
        Assert.Equal("Povinné ručení", viewModel.SelectedRecord?.Title);
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
        var today = DateOnly.FromDateTime(DateTime.Today);
        var upcomingTechnicalControl = today.AddMonths(1).ToString("MM/yyyy", CultureInfo.InvariantCulture);
        var overdueGreenCard = today.AddMonths(-1).ToString("MM/yyyy", CultureInfo.InvariantCulture);
        var recentHistory = today.AddDays(-14).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        var recentFuel = today.AddDays(-10).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        var upcomingReminder = today.AddDays(7).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        var overdueRecord = today.AddMonths(-1).ToString("MM/yyyy", CultureInfo.InvariantCulture);

        var dataSet = new VehimapDataSet
        {
            Settings = new VehimapSettings(),
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", upcomingTechnicalControl, "01/2026", overdueGreenCard),
                new Vehicle("veh_2", "Božena", "Osobní vozidla", "Veterán", "Škoda 100", "2AB3456", "1972", "33", "", "", "", "")
            ],
            HistoryEntries =
            [
                new VehicleHistoryEntry("hist_1", "veh_1", recentHistory, "Servis", "12000", "100", "Kontrola")
            ],
            FuelEntries =
            [
                new FuelEntry("fuel_1", "veh_1", recentFuel, "12450", "40", "300", true, "Natural 95", "")
            ],
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Přezutí", upcomingReminder, "7", "ročně", "Objednat pneuservis")
            ],
            Records =
            [
                new VehicleRecord("rec_1", "veh_1", "Doklad", "Povinné ručení", "", "", overdueRecord, "2000", VehicleRecordAttachmentMode.External, "", "")
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
