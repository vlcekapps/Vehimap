using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class MainWindowViewModelEditingTests : IDisposable
{
    private readonly string _tempRoot;

    public MainWindowViewModelEditingTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "vehimap-dotnet-edit-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public async Task Save_reminder_command_adds_new_reminder_and_restores_selection()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateReminderCommand.Execute(null);
        viewModel.ReminderEditorTitle = "Objednat pneuservis";
        viewModel.ReminderEditorDueDate = "10.10.2026";
        viewModel.ReminderEditorDays = "14";
        viewModel.ReminderEditorRepeatMode = "Ročně";
        viewModel.ReminderEditorNote = "Nezapomenout na zimní gumy";

        await viewModel.SaveReminderCommand.ExecuteAsync(null);

        Assert.NotNull(viewModel.SelectedReminder);
        Assert.Equal("Objednat pneuservis", viewModel.SelectedReminder!.Title);
        Assert.Contains(dataStore.CurrentDataSet.Reminders, item => item.Title == "Objednat pneuservis" && item.VehicleId == "veh_1");
        Assert.Equal("Nová připomínka byla uložena.", viewModel.ReminderEditorStatus);
    }

    [Fact]
    public async Task Save_record_command_imports_managed_attachment_and_persists_relative_path()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var sourceFile = Path.Combine(_tempRoot, "pojistka.pdf");
        await File.WriteAllTextAsync(sourceFile, "dummy attachment content");

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateRecordCommand.Execute(null);
        viewModel.RecordEditorRecordType = "Povinné ručení";
        viewModel.RecordEditorTitle = "Kooperativa 2026";
        viewModel.RecordEditorValidTo = "12/2026";
        viewModel.SelectedRecordEditorAttachmentMode = "Spravovaná kopie";
        viewModel.RecordEditorPathInput = sourceFile;

        await viewModel.SaveRecordCommand.ExecuteAsync(null);

        var savedRecord = Assert.Single(dataStore.CurrentDataSet.Records);
        Assert.Equal("Kooperativa 2026", savedRecord.Title);
        Assert.Equal(Domain.Enums.VehicleRecordAttachmentMode.Managed, savedRecord.AttachmentMode);
        Assert.StartsWith("attachments/veh_1/", savedRecord.FilePath, StringComparison.Ordinal);

        var importedPath = Path.Combine(dataRoot.DataPath, savedRecord.FilePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(importedPath));
        Assert.Equal("Nový doklad byl uložen.", viewModel.RecordEditorStatus);
        Assert.Equal("Spravovaná kopie", viewModel.SelectedRecord?.AttachmentMode);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, true);
        }
    }

    private MainWindowViewModel CreateViewModel(VehimapDataRoot dataRoot, MutableStubLegacyDataStore dataStore)
    {
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

    private static VehimapDataSet BuildBaseDataSet()
    {
        return new VehimapDataSet
        {
            Vehicles =
            [
                new Vehicle("veh_1", "Milena", "Osobní vozidla", "Rodinné auto", "Škoda 120L", "1AB2345", "1988", "43", "", "08/2026", "05/2025", "06/2026")
            ]
        };
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
