using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Enums;
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
        Assert.Equal(VehicleRecordAttachmentMode.Managed, savedRecord.AttachmentMode);
        Assert.StartsWith("attachments/veh_1/", savedRecord.FilePath, StringComparison.Ordinal);

        var importedPath = Path.Combine(dataRoot.DataPath, savedRecord.FilePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(importedPath));
        Assert.Equal("Nový doklad byl uložen.", viewModel.RecordEditorStatus);
        Assert.Equal("Spravovaná kopie", viewModel.SelectedRecord?.AttachmentMode);
    }

    [Fact]
    public async Task Save_history_command_adds_new_history_entry_and_restores_selection()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateHistoryCommand.Execute(null);
        viewModel.HistoryEditorDate = "15.10.2026";
        viewModel.HistoryEditorType = "Servis";
        viewModel.HistoryEditorOdometer = "123456";
        viewModel.HistoryEditorCost = "2500";
        viewModel.HistoryEditorNote = "Vyměněný olej a filtry";

        await viewModel.SaveHistoryCommand.ExecuteAsync(null);

        Assert.NotNull(viewModel.SelectedHistory);
        Assert.Equal("Servis", viewModel.SelectedHistory!.EventType);
        Assert.Contains(dataStore.CurrentDataSet.HistoryEntries, item => item.EventType == "Servis" && item.VehicleId == "veh_1");
        Assert.Equal("Nový historický záznam byl uložen.", viewModel.HistoryEditorStatus);
    }

    [Fact]
    public async Task Save_fuel_command_adds_new_fuel_entry_and_restores_selection()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateFuelCommand.Execute(null);
        viewModel.FuelEditorDate = "20.10.2026";
        viewModel.FuelEditorFuelType = "Natural 95";
        viewModel.FuelEditorLiters = "38.5";
        viewModel.FuelEditorTotalCost = "1890";
        viewModel.FuelEditorOdometer = "123789";
        viewModel.FuelEditorFullTank = false;
        viewModel.FuelEditorNote = "Dálnice";

        await viewModel.SaveFuelCommand.ExecuteAsync(null);

        Assert.NotNull(viewModel.SelectedFuel);
        Assert.Equal("Natural 95", viewModel.SelectedFuel!.FuelType);
        Assert.Contains(dataStore.CurrentDataSet.FuelEntries, item => item.FuelType == "Natural 95" && item.VehicleId == "veh_1");
        Assert.Equal("Nové tankování bylo uloženo.", viewModel.FuelEditorStatus);
    }

    [Fact]
    public async Task Save_maintenance_command_adds_new_plan_and_restores_selection()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateMaintenanceCommand.Execute(null);
        viewModel.MaintenanceEditorTitle = "Motorový olej";
        viewModel.MaintenanceEditorIntervalKm = "15000";
        viewModel.MaintenanceEditorIntervalMonths = "12";
        viewModel.MaintenanceEditorLastServiceDate = "01.04.2026";
        viewModel.MaintenanceEditorLastServiceOdometer = "120000";
        viewModel.MaintenanceEditorIsActive = true;
        viewModel.MaintenanceEditorNote = "Každoroční servis";

        await viewModel.SaveMaintenanceCommand.ExecuteAsync(null);

        Assert.NotNull(viewModel.SelectedMaintenance);
        Assert.Equal("Motorový olej", viewModel.SelectedMaintenance!.Title);
        Assert.Contains(dataStore.CurrentDataSet.MaintenancePlans, item => item.Title == "Motorový olej" && item.VehicleId == "veh_1");
        Assert.Equal("Nový servisní plán byl uložen.", viewModel.MaintenanceEditorStatus);
    }

    [Fact]
    public async Task Save_vehicle_command_adds_new_vehicle_and_restores_selection()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateVehicleCommand.Execute(null);
        viewModel.VehicleEditorName = "Božena";
        viewModel.VehicleEditorCategory = "Osobní vozidla";
        viewModel.VehicleEditorMakeModel = "Škoda 100";
        viewModel.VehicleEditorPlate = "2AB3456";
        viewModel.VehicleEditorYear = "1973";
        viewModel.VehicleEditorPower = "35";
        viewModel.VehicleEditorNote = "Srazové";
        viewModel.VehicleEditorNextTk = "09/2026";
        viewModel.VehicleEditorGreenCardTo = "10/2026";
        viewModel.VehicleEditorState = "Veterán";
        viewModel.VehicleEditorPowertrain = "benzín";

        await viewModel.SaveVehicleCommand.ExecuteAsync(null);

        Assert.NotNull(viewModel.SelectedVehicle);
        Assert.Equal("Božena", viewModel.SelectedVehicle!.Name);
        Assert.Contains(dataStore.CurrentDataSet.Vehicles, item => item.Name == "Božena");
        Assert.Contains(dataStore.CurrentDataSet.VehicleMetaEntries, item => item.VehicleId == viewModel.SelectedVehicle.Id && item.State == "Veterán" && item.Powertrain == "benzín");
        Assert.Equal("Nové vozidlo bylo uloženo.", viewModel.VehicleEditorStatus);
    }

    [Fact]
    public async Task Save_vehicle_command_updates_existing_vehicle_and_meta()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        dataSet.VehicleMetaEntries.Add(new VehicleMeta("veh_1", "Běžný provoz", "test", "benzín", "ano", "řemen", "manuál"));

        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.EditSelectedVehicleCommand.Execute(null);
        viewModel.VehicleEditorName = "Milena po servisu";
        viewModel.VehicleEditorPowertrain = "diesel";
        viewModel.VehicleEditorState = "Odstaveno";

        await viewModel.SaveVehicleCommand.ExecuteAsync(null);

        Assert.Equal("Milena po servisu", viewModel.SelectedVehicle?.Name);
        var savedVehicle = Assert.Single(dataStore.CurrentDataSet.Vehicles.Where(item => item.Id == "veh_1"));
        Assert.Equal("Milena po servisu", savedVehicle.Name);
        var savedMeta = Assert.Single(dataStore.CurrentDataSet.VehicleMetaEntries.Where(item => item.VehicleId == "veh_1"));
        Assert.Equal("Odstaveno", savedMeta.State);
        Assert.Equal("diesel", savedMeta.Powertrain);
        Assert.Equal("test", savedMeta.Tags);
        Assert.Equal("ano", savedMeta.ClimateProfile);
        Assert.Equal("řemen", savedMeta.TimingDrive);
        Assert.Equal("manuál", savedMeta.Transmission);
        Assert.Equal("Vozidlo bylo upraveno.", viewModel.VehicleEditorStatus);
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
