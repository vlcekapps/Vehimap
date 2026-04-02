using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
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
        Assert.Equal(viewModel.HistoryWindowTitle, viewModel.HistoryWorkspace.WindowTitle);
        Assert.Equal(viewModel.RecordWindowTitle, viewModel.RecordWorkspace.WindowTitle);
    }

    [Fact]
    public void Reminder_workspace_shares_selection_and_editing_state_with_root()
    {
        var viewModel = CreateViewModel();
        var reminder = Assert.Single(viewModel.ReminderWorkspace.SelectedVehicleReminders);

        viewModel.ReminderWorkspace.SelectedReminder = reminder;

        Assert.Same(reminder, viewModel.SelectedReminder);

        viewModel.ReminderWorkspace.EditSelectedReminderCommand.Execute(null);
        viewModel.ReminderWorkspace.ReminderEditorTitle = "Upravená připomínka";

        Assert.True(viewModel.IsEditingReminder);
        Assert.True(viewModel.ReminderWorkspace.IsEditingReminder);
        Assert.Equal("Upravená připomínka", viewModel.ReminderEditorTitle);
    }

    [Fact]
    public void Timeline_and_search_workspaces_share_root_state()
    {
        var viewModel = CreateViewModel();

        viewModel.TimelineWorkspace.TimelineSearchText = "technická";
        viewModel.GlobalSearchWorkspace.GlobalSearchText = "Octavia";

        Assert.Equal("technická", viewModel.TimelineSearchText);
        Assert.Equal("Octavia", viewModel.GlobalSearchText);
        Assert.Same(viewModel.SelectedVehicleTimeline, viewModel.TimelineWorkspace.SelectedVehicleTimeline);
        Assert.Same(viewModel.GlobalSearchResults, viewModel.GlobalSearchWorkspace.GlobalSearchResults);
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
            Reminders =
            [
                new VehicleReminder("rem_1", "veh_1", "Objednat servis", "01.12.2099", "30", "Ročně", "Zavolat servisu")
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
