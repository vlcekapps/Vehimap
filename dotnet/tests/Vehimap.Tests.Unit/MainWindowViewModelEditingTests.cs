using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Vehimap.Storage.Legacy;
using System.Globalization;
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
        var backgroundRefreshCount = 0;
        viewModel.BackgroundRefreshRequested += () => backgroundRefreshCount++;

        viewModel.CreateReminderCommand.Execute(null);
        viewModel.ReminderWorkspace.ReminderEditorTitle = "Objednat pneuservis";
        viewModel.ReminderWorkspace.ReminderEditorDueDate = "10.10.2026";
        viewModel.ReminderWorkspace.ReminderEditorDays = "14";
        viewModel.ReminderWorkspace.ReminderEditorRepeatMode = "Ročně";
        viewModel.ReminderWorkspace.ReminderEditorNote = "Nezapomenout na zimní gumy";

        await viewModel.SaveReminderCommand.ExecuteAsync(null);

        Assert.Equal(1, backgroundRefreshCount);
        Assert.NotNull(viewModel.ReminderWorkspace.SelectedReminder);
        Assert.Equal("Objednat pneuservis", viewModel.ReminderWorkspace.SelectedReminder!.Title);
        var savedReminder = Assert.Single(dataStore.CurrentDataSet.Reminders.Where(item => item.Title == "Objednat pneuservis" && item.VehicleId == "veh_1"));
        Assert.Equal("Každý rok", savedReminder.RepeatMode);
        Assert.Equal("Nová připomínka byla uložena.", viewModel.ReminderWorkspace.ReminderEditorStatus);
    }

    [Fact]
    public async Task Save_reminder_command_rejects_invalid_days_and_focuses_field()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateReminderCommand.Execute(null);
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;
        viewModel.ReminderWorkspace.ReminderEditorTitle = "Objednat pneuservis";
        viewModel.ReminderWorkspace.ReminderEditorDueDate = "10.10.2026";
        viewModel.ReminderWorkspace.ReminderEditorDays = "1000";

        await viewModel.SaveReminderCommand.ExecuteAsync(null);

        Assert.True(viewModel.ReminderWorkspace.IsEditingReminder);
        Assert.Equal("Pole Upozornit dnů předem musí být celé číslo od 0 do 999.", viewModel.ReminderWorkspace.ReminderEditorStatus);
        Assert.Equal(DesktopFocusTarget.ReminderEditorDays, Assert.Single(requestedTargets));
        Assert.Empty(dataStore.CurrentDataSet.Reminders);
    }

    [Fact]
    public async Task Save_history_command_reports_persist_failure_and_keeps_editor_open()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataStore = new MutableStubLegacyDataStore(BuildBaseDataSet())
        {
            SaveException = new IOException("history.tsv nelze zapsat.")
        };
        var viewModel = CreateViewModel(dataRoot, dataStore);
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.CreateHistoryCommand.Execute(null);
        viewModel.HistoryWorkspace.HistoryEditorDate = "10.10.2026";
        viewModel.HistoryWorkspace.HistoryEditorType = "Servis";

        await viewModel.SaveHistoryCommand.ExecuteAsync(null);

        Assert.True(viewModel.HistoryWorkspace.IsEditingHistory);
        Assert.Contains("Historický záznam se nepodařilo uložit", viewModel.HistoryWorkspace.HistoryEditorStatus);
        Assert.Contains("history.tsv nelze zapsat", viewModel.HistoryWorkspace.HistoryEditorStatus);
        Assert.Equal(viewModel.HistoryWorkspace.HistoryEditorStatus, viewModel.ShellStatus);
        Assert.Equal(DesktopFocusTarget.HistoryEditorDate, requestedTargets.Last());
    }

    [Fact]
    public async Task Save_record_command_reports_persist_failure_and_keeps_editor_open()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataStore = new MutableStubLegacyDataStore(BuildBaseDataSet())
        {
            SaveException = new IOException("records.tsv nelze zapsat.")
        };
        var viewModel = CreateViewModel(dataRoot, dataStore);
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.CreateRecordCommand.Execute(null);
        viewModel.RecordWorkspace.SelectedRecordEditorAttachmentMode = "Externí cesta";
        viewModel.RecordWorkspace.RecordEditorRecordType = "Doklad";
        viewModel.RecordWorkspace.RecordEditorTitle = "Povinné ručení";
        viewModel.RecordWorkspace.RecordEditorValidTo = "03/2027";

        await viewModel.SaveRecordCommand.ExecuteAsync(null);

        Assert.True(viewModel.RecordWorkspace.IsEditingRecord);
        Assert.Contains("Doklad se nepodařilo uložit", viewModel.RecordWorkspace.RecordEditorStatus);
        Assert.Contains("records.tsv nelze zapsat", viewModel.RecordWorkspace.RecordEditorStatus);
        Assert.Equal(viewModel.RecordWorkspace.RecordEditorStatus, viewModel.ShellStatus);
        Assert.Equal(DesktopFocusTarget.RecordEditorTitle, requestedTargets.Last());
    }

    [Fact]
    public async Task Save_vehicle_command_reports_persist_failure_and_keeps_editor_open()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataStore = new MutableStubLegacyDataStore(BuildBaseDataSet())
        {
            SaveException = new IOException("vehicles.tsv nelze zapsat.")
        };
        var viewModel = CreateViewModel(dataRoot, dataStore);
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        viewModel.CreateVehicleCommand.Execute(null);
        viewModel.VehicleDetailWorkspace.VehicleEditorName = "Božena";
        viewModel.VehicleDetailWorkspace.VehicleEditorCategory = "Osobní vozidla";
        viewModel.VehicleDetailWorkspace.VehicleEditorMakeModel = "Škoda 100";
        viewModel.VehicleDetailWorkspace.VehicleEditorNextTk = "09/2026";

        await viewModel.SaveVehicleCommand.ExecuteAsync(null);

        Assert.True(viewModel.VehicleDetailWorkspace.IsEditingVehicle);
        Assert.Contains("Vozidlo se nepodařilo uložit", viewModel.VehicleDetailWorkspace.VehicleEditorStatus);
        Assert.Contains("vehicles.tsv nelze zapsat", viewModel.VehicleDetailWorkspace.VehicleEditorStatus);
        Assert.Equal(viewModel.VehicleDetailWorkspace.VehicleEditorStatus, viewModel.ShellStatus);
        Assert.Equal(DesktopFocusTarget.VehicleEditorName, requestedTargets.Last());
    }

    [Fact]
    public async Task Failed_preference_save_does_not_leak_into_later_data_persist()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataStore = new MutableStubLegacyDataStore(BuildBaseDataSet(), cloneOnLoad: true);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        dataStore.SaveException = new IOException("settings.ini nelze zapsat.");
        var changedCategory = LegacyKnownValues.Categories[0];
        viewModel.SelectedVehicleCategoryFilter = changedCategory;

        Assert.Equal(changedCategory, viewModel.SelectedVehicleCategoryFilter);
        Assert.Contains("Nepodařilo se uložit filtry seznamu vozidel", viewModel.ShellStatus);

        dataStore.SaveException = null;
        viewModel.CreateHistoryCommand.Execute(null);
        viewModel.HistoryWorkspace.HistoryEditorDate = "15.10.2026";
        viewModel.HistoryWorkspace.HistoryEditorType = "Servis";

        await viewModel.SaveHistoryCommand.ExecuteAsync(null);

        Assert.Single(dataStore.CurrentDataSet.HistoryEntries);
        Assert.Equal(
            MainWindowViewModel.AllVehicleCategoriesLabel,
            dataStore.CurrentDataSet.Settings.GetValue("app", "vehicle_category_filter", MainWindowViewModel.AllVehicleCategoriesLabel));
    }

    [Fact]
    public async Task Save_record_command_removes_new_managed_copy_when_persist_fails()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);
        var sourceFile = Path.Combine(_tempRoot, "pojistka.pdf");
        await File.WriteAllTextAsync(sourceFile, "managed attachment content");

        var dataStore = new MutableStubLegacyDataStore(BuildBaseDataSet(), cloneOnLoad: true)
        {
            SaveException = new IOException("records.tsv is locked.")
        };
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateRecordCommand.Execute(null);
        viewModel.RecordWorkspace.SelectedRecordEditorAttachmentMode = "Spravovaná kopie";
        viewModel.RecordWorkspace.RecordEditorRecordType = "Doklad";
        viewModel.RecordWorkspace.RecordEditorTitle = "Povinné ručení";
        viewModel.RecordWorkspace.RecordEditorPathInput = sourceFile;

        await viewModel.SaveRecordCommand.ExecuteAsync(null);

        var attachmentDirectory = Path.Combine(dataRoot.DataPath, "attachments", "veh_1");
        Assert.True(viewModel.RecordWorkspace.IsEditingRecord);
        Assert.Contains("Doklad se nepodařilo uložit", viewModel.RecordWorkspace.RecordEditorStatus);
        Assert.Empty(dataStore.CurrentDataSet.Records);
        Assert.False(Directory.Exists(attachmentDirectory) && Directory.EnumerateFiles(attachmentDirectory, "*", SearchOption.AllDirectories).Any());
    }

    [Fact]
    public async Task Delete_vehicle_command_reports_persist_failure_and_keeps_data_and_attachments()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);
        var managedDirectory = Path.Combine(dataRoot.DataPath, "attachments", "veh_1");
        Directory.CreateDirectory(managedDirectory);
        var managedFile = Path.Combine(managedDirectory, "pojisteni.pdf");
        await File.WriteAllTextAsync(managedFile, "managed attachment");

        var dataSet = BuildBaseDataSet();
        dataSet.Vehicles.Add(new Vehicle("veh_2", "Božena", "Osobní vozidla", "Srazové auto", "Škoda 100", "", "1973", "35", "", "09/2026", "", "10/2026"));
        dataSet.Records.Add(new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Pojistka", "", "05/2026", "05/2027", "2000", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/pojisteni.pdf", ""));
        var dataStore = new MutableStubLegacyDataStore(dataSet, cloneOnLoad: true)
        {
            SaveException = new IOException("vehicles.tsv is locked.")
        };
        var viewModel = CreateViewModel(dataRoot, dataStore);
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;
        viewModel.SelectedVehicle = Assert.Single(viewModel.Vehicles.Where(item => item.Id == "veh_1"));
        viewModel.ConfirmVehicleDeleteHandler = _ => Task.FromResult(true);

        await viewModel.DeleteSelectedVehicleCommand.ExecuteAsync(null);

        Assert.Contains(dataStore.CurrentDataSet.Vehicles, item => item.Id == "veh_1");
        Assert.Contains(dataStore.CurrentDataSet.Records, item => item.Id == "rec_1");
        Assert.True(File.Exists(managedFile));
        Assert.Equal("veh_1", viewModel.SelectedVehicle?.Id);
        Assert.Contains("Vozidlo Milena se", viewModel.VehicleDetailWorkspace.VehicleEditorStatus);
        Assert.Contains("odstranit", viewModel.VehicleDetailWorkspace.VehicleEditorStatus);
        Assert.Contains("vehicles.tsv is locked", viewModel.ShellStatus);
        Assert.Equal(DesktopFocusTarget.VehicleDetailPrimaryAction, requestedTargets.Last());
    }

    [Fact]
    public async Task Delete_record_command_reports_persist_failure_and_keeps_managed_attachment()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);
        var managedDirectory = Path.Combine(dataRoot.DataPath, "attachments", "veh_1");
        Directory.CreateDirectory(managedDirectory);
        var managedFile = Path.Combine(managedDirectory, "pojisteni.pdf");
        await File.WriteAllTextAsync(managedFile, "managed attachment");

        var dataSet = BuildBaseDataSet();
        dataSet.Records.Add(new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Pojistka", "", "05/2026", "05/2027", "2000", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/pojisteni.pdf", ""));
        var dataStore = new MutableStubLegacyDataStore(dataSet, cloneOnLoad: true)
        {
            SaveException = new IOException("records.tsv is locked.")
        };
        var viewModel = CreateViewModel(dataRoot, dataStore);
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        await viewModel.DeleteSelectedRecordCommand.ExecuteAsync(null);

        Assert.Contains(dataStore.CurrentDataSet.Records, item => item.Id == "rec_1");
        Assert.True(File.Exists(managedFile));
        Assert.Equal("rec_1", viewModel.RecordWorkspace.SelectedRecord?.Id);
        Assert.Contains("Doklad se nepodařilo odstranit", viewModel.RecordWorkspace.RecordEditorStatus);
        Assert.Contains("records.tsv is locked", viewModel.ShellStatus);
        Assert.Equal(DesktopFocusTarget.RecordList, requestedTargets.Last());
    }

    [Theory]
    [InlineData("Ročně", "10.10.2027")]
    [InlineData("Každý rok", "10.10.2027")]
    [InlineData("Každé 2 roky", "10.10.2028")]
    [InlineData("Každých 5 let", "10.10.2031")]
    public async Task Advance_selected_reminder_moves_recurring_due_date_and_restores_selection(string repeatMode, string expectedDueDate)
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        dataSet.Reminders.Add(new VehicleReminder("rem_1", "veh_1", "Objednat servis", "10.10.2026", "14", repeatMode, "Zavolat servisu"));
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        Assert.True(viewModel.AdvanceSelectedReminderCommand.CanExecute(null));

        await viewModel.AdvanceSelectedReminderCommand.ExecuteAsync(null);

        var savedReminder = Assert.Single(dataStore.CurrentDataSet.Reminders);
        Assert.Equal(expectedDueDate, savedReminder.DueDate);
        Assert.Equal("rem_1", viewModel.ReminderWorkspace.SelectedReminder?.Id);
        Assert.Equal(expectedDueDate, viewModel.ReminderWorkspace.SelectedReminder?.DueDate);
        Assert.Equal($"Připomínka byla posunuta na {expectedDueDate}.", viewModel.ReminderWorkspace.ReminderEditorStatus);
    }

    [Fact]
    public void Advance_selected_reminder_is_disabled_for_one_time_or_editing_reminder()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        dataSet.Reminders.Add(new VehicleReminder("rem_1", "veh_1", "Jednorázová kontrola", "10.10.2026", "14", "Neopakovat", string.Empty));
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        Assert.False(viewModel.AdvanceSelectedReminderCommand.CanExecute(null));

        dataStore.CurrentDataSet.Reminders[0] = dataStore.CurrentDataSet.Reminders[0] with { RepeatMode = "Každý rok" };
        Assert.True(viewModel.AdvanceSelectedReminderCommand.CanExecute(null));

        viewModel.CreateReminderCommand.Execute(null);

        Assert.False(viewModel.AdvanceSelectedReminderCommand.CanExecute(null));
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
        viewModel.RecordWorkspace.RecordEditorRecordType = "Povinné ručení";
        viewModel.RecordWorkspace.RecordEditorTitle = "Kooperativa 2026";
        viewModel.RecordWorkspace.RecordEditorValidTo = "12/2026";
        viewModel.RecordWorkspace.SelectedRecordEditorAttachmentMode = "Spravovaná kopie";
        viewModel.RecordWorkspace.RecordEditorPathInput = sourceFile;

        await viewModel.SaveRecordCommand.ExecuteAsync(null);

        var savedRecord = Assert.Single(dataStore.CurrentDataSet.Records);
        Assert.Equal("Kooperativa 2026", savedRecord.Title);
        Assert.Equal(VehicleRecordAttachmentMode.Managed, savedRecord.AttachmentMode);
        Assert.StartsWith("attachments/veh_1/", savedRecord.FilePath, StringComparison.Ordinal);

        var importedPath = Path.Combine(dataRoot.DataPath, savedRecord.FilePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(importedPath));
        Assert.Equal("Nový doklad byl uložen.", viewModel.RecordWorkspace.RecordEditorStatus);
        Assert.Equal("Spravovaná kopie", viewModel.RecordWorkspace.SelectedRecord?.AttachmentMode);
    }

    [Fact]
    public async Task Save_record_command_normalizes_unknown_type_to_legacy_default()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateRecordCommand.Execute(null);
        viewModel.RecordWorkspace.RecordEditorRecordType = "Vlastní typ";
        viewModel.RecordWorkspace.RecordEditorTitle = "Starý doklad";

        await viewModel.SaveRecordCommand.ExecuteAsync(null);

        var savedRecord = Assert.Single(dataStore.CurrentDataSet.Records);
        Assert.Equal("Povinné ručení", savedRecord.RecordType);
    }

    [Fact]
    public async Task Save_record_command_rejects_reversed_validity_and_focuses_field()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateRecordCommand.Execute(null);
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;
        viewModel.RecordWorkspace.RecordEditorRecordType = "Doklad";
        viewModel.RecordWorkspace.RecordEditorTitle = "Pojistka";
        viewModel.RecordWorkspace.RecordEditorValidFrom = "12/2026";
        viewModel.RecordWorkspace.RecordEditorValidTo = "11/2026";

        await viewModel.SaveRecordCommand.ExecuteAsync(null);

        Assert.True(viewModel.RecordWorkspace.IsEditingRecord);
        Assert.Equal("Pole Platné od nesmí být později než pole Platné do.", viewModel.RecordWorkspace.RecordEditorStatus);
        Assert.Equal(DesktopFocusTarget.RecordEditorValidFrom, Assert.Single(requestedTargets));
        Assert.Empty(dataStore.CurrentDataSet.Records);
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
        viewModel.HistoryWorkspace.HistoryEditorDate = "15.10.2026";
        viewModel.HistoryWorkspace.HistoryEditorType = "Servis";
        viewModel.HistoryWorkspace.HistoryEditorOdometer = "123456";
        viewModel.HistoryWorkspace.HistoryEditorCost = "2500";
        viewModel.HistoryWorkspace.HistoryEditorNote = "Vyměněný olej a filtry";

        await viewModel.SaveHistoryCommand.ExecuteAsync(null);

        Assert.NotNull(viewModel.HistoryWorkspace.SelectedHistory);
        Assert.Equal("Servis", viewModel.HistoryWorkspace.SelectedHistory!.EventType);
        Assert.Contains(dataStore.CurrentDataSet.HistoryEntries, item => item.EventType == "Servis" && item.VehicleId == "veh_1");
        Assert.Equal("Nový historický záznam byl uložen.", viewModel.HistoryWorkspace.HistoryEditorStatus);
    }

    [Fact]
    public async Task History_editor_displays_selected_distance_unit_and_saves_canonical_kilometers()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        ConfigureUnits(dataSet, distanceUnit: "mi", volumeUnit: "us_gal");
        dataSet.HistoryEntries.Add(new VehicleHistoryEntry("hist_1", "veh_1", "15.10.2026", "Servis", "161", "2500", "Test"));
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.EditSelectedHistoryCommand.Execute(null);

        Assert.Equal("100", viewModel.HistoryWorkspace.HistoryEditorOdometer);
        Assert.Equal("Tachometr (mi)", viewModel.HistoryWorkspace.HistoryEditorOdometerLabel);

        viewModel.HistoryWorkspace.HistoryEditorOdometer = "200";

        await viewModel.SaveHistoryCommand.ExecuteAsync(null);

        var saved = Assert.Single(dataStore.CurrentDataSet.HistoryEntries);
        Assert.Equal("322", saved.Odometer);
    }

    [Fact]
    public async Task Save_history_command_rejects_invalid_cost_and_focuses_field()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateHistoryCommand.Execute(null);
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;
        viewModel.HistoryWorkspace.HistoryEditorDate = "15.10.2026";
        viewModel.HistoryWorkspace.HistoryEditorType = "Servis";
        viewModel.HistoryWorkspace.HistoryEditorCost = "není číslo";

        await viewModel.SaveHistoryCommand.ExecuteAsync(null);

        Assert.True(viewModel.HistoryWorkspace.IsEditingHistory);
        Assert.Equal("Cenu události zadejte jako číslo, například 2500.", viewModel.HistoryWorkspace.HistoryEditorStatus);
        Assert.Equal(DesktopFocusTarget.HistoryEditorCost, Assert.Single(requestedTargets));
        Assert.Empty(dataStore.CurrentDataSet.HistoryEntries);
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
        viewModel.FuelWorkspace.FuelEditorDate = "20.10.2026";
        viewModel.FuelWorkspace.FuelEditorFuelType = "Nafta";
        viewModel.FuelWorkspace.FuelEditorFuelDetail = "Shell FuelSave";
        viewModel.FuelWorkspace.FuelEditorStation = "Shell Brno Vídeňská";
        viewModel.FuelWorkspace.FuelEditorLiters = "38.5";
        viewModel.FuelWorkspace.FuelEditorTotalCost = "1890";
        viewModel.FuelWorkspace.FuelEditorOdometer = "123789";
        viewModel.FuelWorkspace.FuelEditorFullTank = false;
        viewModel.FuelWorkspace.FuelEditorNote = "Dálnice";

        await viewModel.SaveFuelCommand.ExecuteAsync(null);

        Assert.NotNull(viewModel.FuelWorkspace.SelectedFuel);
        Assert.Equal("Nafta", viewModel.FuelWorkspace.SelectedFuel!.FuelType);
        Assert.Contains(dataStore.CurrentDataSet.FuelEntries, item =>
            item.FuelType == "Nafta"
            && item.FuelDetail == "Shell FuelSave"
            && item.Station == "Shell Brno Vídeňská"
            && item.VehicleId == "veh_1");
        Assert.Contains("Shell FuelSave", viewModel.FuelWorkspace.SelectedFuelDetail, StringComparison.Ordinal);
        Assert.Contains("Shell Brno Vídeňská", viewModel.FuelWorkspace.SelectedFuelDetail, StringComparison.Ordinal);
        Assert.Equal("Nové tankování bylo uloženo.", viewModel.FuelWorkspace.FuelEditorStatus);
    }

    [Fact]
    public async Task Fuel_editor_displays_selected_units_and_saves_canonical_kilometers_and_liters()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        ConfigureUnits(dataSet, distanceUnit: "mi", volumeUnit: "us_gal");
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateFuelCommand.Execute(null);

        Assert.Equal("Tachometr (mi)", viewModel.FuelWorkspace.FuelEditorOdometerLabel);
        Assert.Equal("Množství (US gal)", viewModel.FuelWorkspace.FuelEditorVolumeLabel);

        viewModel.FuelWorkspace.FuelEditorDate = "20.10.2026";
        viewModel.FuelWorkspace.FuelEditorFuelType = "Nafta";
        viewModel.FuelWorkspace.FuelEditorLiters = "10";
        viewModel.FuelWorkspace.FuelEditorOdometer = "100";

        await viewModel.SaveFuelCommand.ExecuteAsync(null);

        var saved = Assert.Single(dataStore.CurrentDataSet.FuelEntries);
        Assert.Equal("161", saved.Odometer);
        Assert.Equal("37.85", saved.Liters);
    }

    [Fact]
    public async Task Fuel_editor_accepts_selected_decimal_separator_and_saves_invariant_liters()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        ConfigureNumberFormatAndUnits(dataSet, language: "cs-CZ", thousandsSeparator: "none", decimalSeparator: "comma", distanceUnit: "km", volumeUnit: "l");
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateFuelCommand.Execute(null);
        viewModel.FuelWorkspace.FuelEditorDate = "20.10.2026";
        viewModel.FuelWorkspace.FuelEditorFuelType = "Nafta";
        viewModel.FuelWorkspace.FuelEditorLiters = "10,5";
        viewModel.FuelWorkspace.FuelEditorOdometer = "123456";

        await viewModel.SaveFuelCommand.ExecuteAsync(null);

        var saved = Assert.Single(dataStore.CurrentDataSet.FuelEntries);
        Assert.Equal("10.5", saved.Liters);
    }

    [Fact]
    public async Task Save_fuel_command_normalizes_unknown_fuel_type_to_legacy_default()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateFuelCommand.Execute(null);
        viewModel.FuelWorkspace.FuelEditorDate = "20.10.2026";
        viewModel.FuelWorkspace.FuelEditorFuelType = "Natural 95";
        viewModel.FuelWorkspace.FuelEditorLiters = "38.5";
        viewModel.FuelWorkspace.FuelEditorTotalCost = "1890";
        viewModel.FuelWorkspace.FuelEditorOdometer = "123789";

        await viewModel.SaveFuelCommand.ExecuteAsync(null);

        var savedFuel = Assert.Single(dataStore.CurrentDataSet.FuelEntries);
        Assert.Equal(string.Empty, savedFuel.FuelType);
    }

    [Fact]
    public void Edit_selected_fuel_normalizes_unknown_fuel_type_for_dropdown()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        dataSet.FuelEntries.Add(new FuelEntry("fuel_1", "veh_1", "20.10.2026", "123789", "38.5", "1890", true, "Natural 95", string.Empty));
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.EditSelectedFuelCommand.Execute(null);

        Assert.Equal(string.Empty, viewModel.FuelWorkspace.FuelEditorFuelType);
    }

    [Fact]
    public async Task Save_fuel_command_requires_liters_when_cost_is_filled_and_focuses_field()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateFuelCommand.Execute(null);
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;
        viewModel.FuelWorkspace.FuelEditorDate = "20.10.2026";
        viewModel.FuelWorkspace.FuelEditorOdometer = "123789";
        viewModel.FuelWorkspace.FuelEditorTotalCost = "1890";

        await viewModel.SaveFuelCommand.ExecuteAsync(null);

        Assert.True(viewModel.FuelWorkspace.IsEditingFuel);
        Assert.Equal("Pokud zadáváte cenu tankování, doplňte i počet litrů.", viewModel.FuelWorkspace.FuelEditorStatus);
        Assert.Equal(DesktopFocusTarget.FuelEditorLiters, Assert.Single(requestedTargets));
        Assert.Empty(dataStore.CurrentDataSet.FuelEntries);
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
        viewModel.MaintenanceWorkspace.MaintenanceEditorTitle = "Motorový olej";
        viewModel.MaintenanceWorkspace.MaintenanceEditorIntervalKm = "15000";
        viewModel.MaintenanceWorkspace.MaintenanceEditorIntervalMonths = "12";
        viewModel.MaintenanceWorkspace.MaintenanceEditorLastServiceDate = "01.04.2026";
        viewModel.MaintenanceWorkspace.MaintenanceEditorLastServiceOdometer = "120000";
        viewModel.MaintenanceWorkspace.MaintenanceEditorIsActive = true;
        viewModel.MaintenanceWorkspace.MaintenanceEditorNote = "Každoroční servis";

        await viewModel.SaveMaintenanceCommand.ExecuteAsync(null);

        Assert.NotNull(viewModel.MaintenanceWorkspace.SelectedMaintenance);
        Assert.Equal("Motorový olej", viewModel.MaintenanceWorkspace.SelectedMaintenance!.Title);
        Assert.Contains(dataStore.CurrentDataSet.MaintenancePlans, item => item.Title == "Motorový olej" && item.VehicleId == "veh_1");
        Assert.Equal("Nový servisní plán byl uložen.", viewModel.MaintenanceWorkspace.MaintenanceEditorStatus);
    }

    [Fact]
    public async Task Maintenance_editor_displays_selected_distance_unit_and_saves_canonical_kilometers()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        ConfigureUnits(dataSet, distanceUnit: "mi", volumeUnit: "l");
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateMaintenanceCommand.Execute(null);

        Assert.Equal("Interval vzdálenosti (mi)", viewModel.MaintenanceWorkspace.MaintenanceEditorIntervalDistanceLabel);
        Assert.Equal("Poslední servis - tachometr (mi)", viewModel.MaintenanceWorkspace.MaintenanceEditorLastServiceOdometerLabel);

        viewModel.MaintenanceWorkspace.MaintenanceEditorTitle = "Motorový olej";
        viewModel.MaintenanceWorkspace.MaintenanceEditorIntervalKm = "100";
        viewModel.MaintenanceWorkspace.MaintenanceEditorLastServiceDate = "01.04.2026";
        viewModel.MaintenanceWorkspace.MaintenanceEditorLastServiceOdometer = "200";

        await viewModel.SaveMaintenanceCommand.ExecuteAsync(null);

        var saved = Assert.Single(dataStore.CurrentDataSet.MaintenancePlans);
        Assert.Equal("161", saved.IntervalKm);
        Assert.Equal("322", saved.LastServiceOdometer);
    }

    [Fact]
    public async Task Save_maintenance_command_requires_last_odometer_for_km_interval_and_focuses_field()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateMaintenanceCommand.Execute(null);
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;
        viewModel.MaintenanceWorkspace.MaintenanceEditorTitle = "Motorový olej";
        viewModel.MaintenanceWorkspace.MaintenanceEditorIntervalKm = "15000";

        await viewModel.SaveMaintenanceCommand.ExecuteAsync(null);

        Assert.True(viewModel.MaintenanceWorkspace.IsEditingMaintenance);
        Assert.Equal("Pro interval podle tachometru vyplňte i stav tachometru při posledním servisu.", viewModel.MaintenanceWorkspace.MaintenanceEditorStatus);
        Assert.Equal(DesktopFocusTarget.MaintenanceEditorLastServiceOdometer, Assert.Single(requestedTargets));
        Assert.Empty(dataStore.CurrentDataSet.MaintenancePlans);
    }

    [Fact]
    public void Selecting_maintenance_editor_template_prefills_plan_fields()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateMaintenanceCommand.Execute(null);

        var cabinFilterOption = VehicleStarterBundleService.BuildMaintenanceTemplateDisplayName(
            Assert.Single(VehicleStarterBundleService.GetMaintenanceTemplateCatalog(), item => item.Title == "Kabinový filtr"));
        Assert.Contains(cabinFilterOption, viewModel.MaintenanceWorkspace.MaintenanceTemplateOptions);
        Assert.Equal("Vlastní položka", viewModel.MaintenanceWorkspace.SelectedMaintenanceTemplate);

        viewModel.MaintenanceWorkspace.SelectedMaintenanceTemplate = cabinFilterOption;

        Assert.Equal("Kabinový filtr", viewModel.MaintenanceWorkspace.MaintenanceEditorTitle);
        Assert.Equal("15000", viewModel.MaintenanceWorkspace.MaintenanceEditorIntervalKm);
        Assert.Equal("12", viewModel.MaintenanceWorkspace.MaintenanceEditorIntervalMonths);
        Assert.Equal("Pravidelná výměna pylového nebo kabinového filtru.", viewModel.MaintenanceWorkspace.MaintenanceEditorNote);
        Assert.Contains("předvyplnila", viewModel.MaintenanceWorkspace.MaintenanceEditorStatus);

        viewModel.CancelMaintenanceEditCommand.Execute(null);

        Assert.Equal("Vlastní položka", viewModel.MaintenanceWorkspace.SelectedMaintenanceTemplate);
    }

    [Fact]
    public async Task Complete_selected_maintenance_updates_last_service_from_current_vehicle_data()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        dataSet.HistoryEntries.Add(new VehicleHistoryEntry("hist_1", "veh_1", "01.05.2026", "Servis", "120000", "1500", "Starší servis"));
        dataSet.FuelEntries.Add(new FuelEntry("fuel_1", "veh_1", "02.06.2026", "123456", "35", "1700", true, "Natural 95", string.Empty));
        dataSet.MaintenancePlans.Add(new MaintenancePlan("mnt_1", "veh_1", "Motorový olej", "15000", "12", "01.04.2025", "100000", true, "Roční servis"));
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);
        var expectedDate = DateOnly.FromDateTime(DateTime.Today).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

        Assert.True(viewModel.CompleteSelectedMaintenanceCommand.CanExecute(null));

        await viewModel.CompleteSelectedMaintenanceCommand.ExecuteAsync(null);

        var savedPlan = Assert.Single(dataStore.CurrentDataSet.MaintenancePlans);
        Assert.Equal(expectedDate, savedPlan.LastServiceDate);
        Assert.Equal("123456", savedPlan.LastServiceOdometer);
        Assert.Equal("mnt_1", viewModel.MaintenanceWorkspace.SelectedMaintenance?.Id);
        Assert.Contains(expectedDate, viewModel.MaintenanceWorkspace.MaintenanceEditorStatus);
        Assert.Contains("123456 km", viewModel.MaintenanceWorkspace.MaintenanceEditorStatus);
    }

    [Fact]
    public async Task Maintenance_completion_can_add_history_entry()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        dataSet.MaintenancePlans.Add(new MaintenancePlan("mnt_1", "veh_1", "Motorový olej", "15000", "12", "01.04.2025", "100000", true, "Roční servis"));
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        var dialogViewModel = viewModel.BuildMaintenanceCompletionDialogViewModel();
        Assert.NotNull(dialogViewModel);
        Assert.Equal("Motorový olej", dialogViewModel!.PlanTitle);
        Assert.True(dialogViewModel.RequiresOdometer);

        var message = await viewModel.ApplyMaintenanceCompletionAsync(new MaintenanceCompletionDialogResult(
            "15.06.2026",
            "123456",
            AddHistory: true,
            HistoryCost: "2500,50",
            HistoryNote: string.Empty));

        var savedPlan = Assert.Single(dataStore.CurrentDataSet.MaintenancePlans);
        Assert.Equal("15.06.2026", savedPlan.LastServiceDate);
        Assert.Equal("123456", savedPlan.LastServiceOdometer);
        var history = Assert.Single(dataStore.CurrentDataSet.HistoryEntries);
        Assert.Equal("15.06.2026", history.EventDate);
        Assert.Equal("Motorový olej", history.EventType);
        Assert.Equal("123456", history.Odometer);
        Assert.Equal("2500.5", history.Cost);
        Assert.Equal("Zapsáno z plánu údržby.", history.Note);
        Assert.Contains("historie", message);
        Assert.Equal("mnt_1", viewModel.MaintenanceWorkspace.SelectedMaintenance?.Id);
    }

    [Fact]
    public async Task Maintenance_completion_dialog_displays_selected_distance_unit_and_saves_canonical_kilometers()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        ConfigureUnits(dataSet, distanceUnit: "mi", volumeUnit: "l");
        dataSet.MaintenancePlans.Add(new MaintenancePlan("mnt_1", "veh_1", "Motorový olej", "161", "12", "01.04.2025", "161", true, "Roční servis"));
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        var dialogViewModel = viewModel.BuildMaintenanceCompletionDialogViewModel();

        Assert.NotNull(dialogViewModel);
        Assert.Equal("100", dialogViewModel!.CompletedOdometer);
        Assert.Equal("Tachometr při provedení (mi)", dialogViewModel.CompletedOdometerLabel);

        dialogViewModel.CompletedOdometer = "200";
        Assert.True(dialogViewModel.TryBuildResult(out var result));
        Assert.NotNull(result);

        var message = await viewModel.ApplyMaintenanceCompletionAsync(result!);

        var savedPlan = Assert.Single(dataStore.CurrentDataSet.MaintenancePlans);
        Assert.Equal("322", savedPlan.LastServiceOdometer);
        Assert.Contains("322 km", message);
    }

    [Fact]
    public void Maintenance_completion_dialog_validates_required_fields()
    {
        var dialog = new MaintenanceCompletionDialogViewModel(
            "Milena",
            "Motorový olej",
            "Blíží se servis",
            requiresOdometer: true,
            completedDate: "15.06.2026",
            completedOdometer: string.Empty);

        Assert.False(dialog.TryBuildResult(out var missingOdometerResult));
        Assert.Null(missingOdometerResult);
        Assert.Equal("MaintenanceCompletionOdometerBox", dialog.ErrorFocusTarget);

        dialog.CompletedOdometer = "123456";
        dialog.HistoryCost = "abc";
        Assert.False(dialog.TryBuildResult(out var invalidCostResult));
        Assert.Null(invalidCostResult);
        Assert.Equal("MaintenanceCompletionHistoryCostBox", dialog.ErrorFocusTarget);

        dialog.HistoryCost = "2500,50";
        Assert.True(dialog.TryBuildResult(out var result));
        Assert.NotNull(result);
        Assert.Equal("15.06.2026", result!.CompletedDate);
        Assert.Equal("123456", result.CompletedOdometer);
        Assert.Equal("2500.5", result.HistoryCost);
    }

    [Fact]
    public void Complete_selected_maintenance_is_disabled_without_selection_or_during_editing()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        Assert.False(viewModel.CompleteSelectedMaintenanceCommand.CanExecute(null));

        dataStore.CurrentDataSet.MaintenancePlans.Add(new MaintenancePlan("mnt_1", "veh_1", "Motorový olej", "15000", "12", string.Empty, string.Empty, true, string.Empty));
        var viewModelWithPlan = CreateViewModel(dataRoot, dataStore);
        Assert.True(viewModelWithPlan.CompleteSelectedMaintenanceCommand.CanExecute(null));

        viewModelWithPlan.CreateMaintenanceCommand.Execute(null);

        Assert.False(viewModelWithPlan.CompleteSelectedMaintenanceCommand.CanExecute(null));
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
        viewModel.VehicleDetailWorkspace.VehicleEditorName = "Božena";
        viewModel.VehicleDetailWorkspace.VehicleEditorCategory = "Osobní";
        viewModel.VehicleDetailWorkspace.VehicleEditorMakeModel = "Škoda 100";
        viewModel.VehicleDetailWorkspace.VehicleEditorPlate = " 2ab3456 ";
        viewModel.VehicleDetailWorkspace.VehicleEditorYear = "1973";
        viewModel.VehicleDetailWorkspace.VehicleEditorPower = "35";
        viewModel.VehicleDetailWorkspace.VehicleEditorNote = "Srazové";
        viewModel.VehicleDetailWorkspace.VehicleEditorLastTk = "5.2025";
        viewModel.VehicleDetailWorkspace.VehicleEditorNextTk = "9-2026";
        viewModel.VehicleDetailWorkspace.VehicleEditorGreenCardFrom = "7.2025";
        viewModel.VehicleDetailWorkspace.VehicleEditorGreenCardTo = "10.2026";
        viewModel.VehicleDetailWorkspace.VehicleEditorState = "Veterán";
        viewModel.VehicleDetailWorkspace.VehicleEditorTags = "srazové, veterán";
        viewModel.VehicleDetailWorkspace.VehicleEditorPowertrain = "Benzín";
        viewModel.VehicleDetailWorkspace.VehicleEditorClimateProfile = "Má klimatizaci";
        viewModel.VehicleDetailWorkspace.VehicleEditorTimingDrive = "Řemen";
        viewModel.VehicleDetailWorkspace.VehicleEditorTransmission = "Manuální";
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        await viewModel.SaveVehicleCommand.ExecuteAsync(null);

        Assert.NotNull(viewModel.SelectedVehicle);
        Assert.Equal("Božena", viewModel.SelectedVehicle!.Name);
        Assert.Contains(dataStore.CurrentDataSet.Vehicles, item => item.Name == "Božena");
        var savedVehicle = Assert.Single(dataStore.CurrentDataSet.Vehicles.Where(item => item.Id == viewModel.SelectedVehicle.Id));
        Assert.Equal("Osobní vozidla", savedVehicle.Category);
        Assert.Equal("2AB3456", savedVehicle.Plate);
        Assert.Equal("05/2025", savedVehicle.LastTk);
        Assert.Equal("09/2026", savedVehicle.NextTk);
        Assert.Equal("07/2025", savedVehicle.GreenCardFrom);
        Assert.Equal("10/2026", savedVehicle.GreenCardTo);
        Assert.Contains(
            dataStore.CurrentDataSet.VehicleMetaEntries,
            item => item.VehicleId == viewModel.SelectedVehicle.Id
                && item.State == "Veterán"
                && item.Tags == "srazové, veterán"
                && item.Powertrain == "Benzín"
                && item.ClimateProfile == "Má klimatizaci"
                && item.TimingDrive == "Řemen"
                && item.Transmission == "Manuální");
        Assert.Equal("Nové vozidlo bylo uloženo.", viewModel.VehicleDetailWorkspace.VehicleEditorStatus);
        Assert.Equal(DesktopFocusTarget.VehicleList, requestedTargets.Last());
        Assert.True(viewModel.TryConsumePendingVehicleStarterBundleOffer(viewModel.SelectedVehicle.Id));
    }

    [Fact]
    public async Task Save_vehicle_command_rejects_invalid_month_year_and_focuses_field()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.CreateVehicleCommand.Execute(null);
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;
        viewModel.VehicleDetailWorkspace.VehicleEditorName = "Božena";
        viewModel.VehicleDetailWorkspace.VehicleEditorCategory = "Osobní vozidla";
        viewModel.VehicleDetailWorkspace.VehicleEditorMakeModel = "Škoda 100";
        viewModel.VehicleDetailWorkspace.VehicleEditorNextTk = "13/2026";

        await viewModel.SaveVehicleCommand.ExecuteAsync(null);

        Assert.True(viewModel.VehicleDetailWorkspace.IsEditingVehicle);
        Assert.Equal("Pole Příští TK je povinné a musí být ve formátu MM/RRRR.", viewModel.VehicleDetailWorkspace.VehicleEditorStatus);
        Assert.Equal(DesktopFocusTarget.VehicleEditorNextTk, Assert.Single(requestedTargets));
        Assert.DoesNotContain(dataStore.CurrentDataSet.Vehicles, item => item.Name == "Božena");
    }

    [Fact]
    public async Task Save_vehicle_command_updates_existing_vehicle_and_meta()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        dataSet.VehicleMetaEntries.Add(new VehicleMeta("veh_1", "Běžný provoz", "test", "Benzín", "Má klimatizaci", "Řemen", "Manuální"));

        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.EditSelectedVehicleCommand.Execute(null);
        Assert.Equal("test", viewModel.VehicleDetailWorkspace.VehicleEditorTags);
        viewModel.VehicleDetailWorkspace.VehicleEditorName = "Milena po servisu";
        viewModel.VehicleDetailWorkspace.VehicleEditorPowertrain = "Nafta";
        viewModel.VehicleDetailWorkspace.VehicleEditorState = "Odstaveno";
        viewModel.VehicleDetailWorkspace.VehicleEditorTags = " rodina; servis, RODINA ";
        viewModel.VehicleDetailWorkspace.VehicleEditorClimateProfile = "Bez klimatizace";
        viewModel.VehicleDetailWorkspace.VehicleEditorTimingDrive = "Řetěz";
        viewModel.VehicleDetailWorkspace.VehicleEditorTransmission = "Automatická";
        var requestedTargets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += requestedTargets.Add;

        await viewModel.SaveVehicleCommand.ExecuteAsync(null);

        Assert.Equal("Milena po servisu", viewModel.SelectedVehicle?.Name);
        var savedVehicle = Assert.Single(dataStore.CurrentDataSet.Vehicles.Where(item => item.Id == "veh_1"));
        Assert.Equal("Milena po servisu", savedVehicle.Name);
        var savedMeta = Assert.Single(dataStore.CurrentDataSet.VehicleMetaEntries.Where(item => item.VehicleId == "veh_1"));
        Assert.Equal("Odstaveno", savedMeta.State);
        Assert.Equal("Nafta", savedMeta.Powertrain);
        Assert.Equal("rodina, servis", savedMeta.Tags);
        Assert.Equal("Bez klimatizace", savedMeta.ClimateProfile);
        Assert.Equal("Řetěz", savedMeta.TimingDrive);
        Assert.Equal("Automatická", savedMeta.Transmission);
        Assert.Equal("Vozidlo bylo upraveno.", viewModel.VehicleDetailWorkspace.VehicleEditorStatus);
        Assert.Equal(DesktopFocusTarget.VehicleList, requestedTargets.Last());
    }

    [Fact]
    public async Task Save_vehicle_command_normalizes_unknown_meta_dropdown_values()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.EditSelectedVehicleCommand.Execute(null);
        viewModel.VehicleDetailWorkspace.VehicleEditorState = "Neznámý stav";
        viewModel.VehicleDetailWorkspace.VehicleEditorPowertrain = "benzín";
        viewModel.VehicleDetailWorkspace.VehicleEditorClimateProfile = "klima";
        viewModel.VehicleDetailWorkspace.VehicleEditorTimingDrive = "řetěz";
        viewModel.VehicleDetailWorkspace.VehicleEditorTransmission = "manual";
        viewModel.VehicleDetailWorkspace.VehicleEditorTags = " test ";

        await viewModel.SaveVehicleCommand.ExecuteAsync(null);

        var savedMeta = Assert.Single(dataStore.CurrentDataSet.VehicleMetaEntries.Where(item => item.VehicleId == "veh_1"));
        Assert.Equal(string.Empty, savedMeta.State);
        Assert.Equal(string.Empty, savedMeta.Powertrain);
        Assert.Equal(string.Empty, savedMeta.ClimateProfile);
        Assert.Equal(string.Empty, savedMeta.TimingDrive);
        Assert.Equal(string.Empty, savedMeta.Transmission);
        Assert.Equal("test", savedMeta.Tags);
    }

    [Fact]
    public void Edit_selected_vehicle_normalizes_unknown_meta_values_for_dropdowns()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        dataSet.VehicleMetaEntries.Add(new VehicleMeta("veh_1", "Neznámý stav", "test", "benzín", "klima", "řetěz", "manual"));
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        viewModel.EditSelectedVehicleCommand.Execute(null);

        Assert.Equal(string.Empty, viewModel.VehicleDetailWorkspace.VehicleEditorState);
        Assert.Equal(string.Empty, viewModel.VehicleDetailWorkspace.VehicleEditorPowertrain);
        Assert.Equal(string.Empty, viewModel.VehicleDetailWorkspace.VehicleEditorClimateProfile);
        Assert.Equal(string.Empty, viewModel.VehicleDetailWorkspace.VehicleEditorTimingDrive);
        Assert.Equal(string.Empty, viewModel.VehicleDetailWorkspace.VehicleEditorTransmission);
        Assert.Equal("test", viewModel.VehicleDetailWorkspace.VehicleEditorTags);
    }

    [Fact]
    public async Task Delete_vehicle_command_requires_confirmation_and_keeps_data_when_rejected()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);
        var confirmationWasShown = false;
        viewModel.ConfirmVehicleDeleteHandler = message =>
        {
            confirmationWasShown = true;
            Assert.Contains("Milena", message);
            return Task.FromResult(false);
        };

        await viewModel.DeleteSelectedVehicleCommand.ExecuteAsync(null);

        Assert.True(confirmationWasShown);
        Assert.Single(dataStore.CurrentDataSet.Vehicles);
        Assert.Equal("Milena", viewModel.SelectedVehicle?.Name);
    }

    [Fact]
    public async Task Delete_vehicle_command_removes_related_data_and_managed_attachments()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);
        var managedDirectory = Path.Combine(dataRoot.DataPath, "attachments", "veh_1");
        Directory.CreateDirectory(managedDirectory);
        await File.WriteAllTextAsync(Path.Combine(managedDirectory, "pojisteni.pdf"), "managed attachment");

        var dataSet = BuildBaseDataSet();
        dataSet.Vehicles.Add(new Vehicle("veh_2", "Božena", "Osobní vozidla", "Srazové auto", "Škoda 100", "", "1973", "35", "", "09/2026", "", "10/2026"));
        dataSet.HistoryEntries.Add(new VehicleHistoryEntry("hist_1", "veh_1", "01.05.2026", "Servis", "123000", "1500", "Olej"));
        dataSet.FuelEntries.Add(new FuelEntry("fuel_1", "veh_1", "02.05.2026", "123200", "35", "1700", true, "Natural 95", ""));
        dataSet.Records.Add(new VehicleRecord("rec_1", "veh_1", "Povinné ručení", "Pojistka", "", "05/2026", "05/2027", "2000", VehicleRecordAttachmentMode.Managed, "attachments/veh_1/pojisteni.pdf", ""));
        dataSet.Records.Add(new VehicleRecord("rec_2", "veh_2", "Doklad", "Doklad druhého vozidla", "", "", "", "", VehicleRecordAttachmentMode.External, @"C:\externi.pdf", ""));
        dataSet.Reminders.Add(new VehicleReminder("rem_1", "veh_1", "Objednat servis", "10.06.2026", "14", "", ""));
        dataSet.MaintenancePlans.Add(new MaintenancePlan("mnt_1", "veh_1", "Motorový olej", "15000", "12", "", "", true, ""));
        dataSet.VehicleMetaEntries.Add(new VehicleMeta("veh_1", "Běžný provoz", "rodina", "Benzín", "", "", ""));
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);
        viewModel.SelectedVehicle = Assert.Single(viewModel.Vehicles.Where(item => item.Id == "veh_1"));
        string? confirmationMessage = null;
        viewModel.ConfirmVehicleDeleteHandler = message =>
        {
            confirmationMessage = message;
            return Task.FromResult(true);
        };

        await viewModel.DeleteSelectedVehicleCommand.ExecuteAsync(null);

        Assert.NotNull(confirmationMessage);
        Assert.Contains("historie: 1", confirmationMessage);
        Assert.Contains("tankování: 1", confirmationMessage);
        Assert.Contains("doklady: 1", confirmationMessage);
        Assert.Contains("připomínky: 1", confirmationMessage);
        Assert.Contains("údržba: 1", confirmationMessage);
        Assert.DoesNotContain(dataStore.CurrentDataSet.Vehicles, item => item.Id == "veh_1");
        Assert.DoesNotContain(dataStore.CurrentDataSet.HistoryEntries, item => item.VehicleId == "veh_1");
        Assert.DoesNotContain(dataStore.CurrentDataSet.FuelEntries, item => item.VehicleId == "veh_1");
        Assert.DoesNotContain(dataStore.CurrentDataSet.Records, item => item.VehicleId == "veh_1");
        Assert.DoesNotContain(dataStore.CurrentDataSet.Reminders, item => item.VehicleId == "veh_1");
        Assert.DoesNotContain(dataStore.CurrentDataSet.MaintenancePlans, item => item.VehicleId == "veh_1");
        Assert.DoesNotContain(dataStore.CurrentDataSet.VehicleMetaEntries, item => item.VehicleId == "veh_1");
        Assert.Contains(dataStore.CurrentDataSet.Records, item => item.Id == "rec_2");
        Assert.False(Directory.Exists(managedDirectory));
        Assert.Equal("veh_2", viewModel.SelectedVehicle?.Id);
        Assert.Contains("bylo odstraněno", viewModel.ShellStatus);
    }

    [Fact]
    public async Task Vehicle_starter_bundle_preview_and_apply_adds_missing_items_without_duplicates()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        dataSet.VehicleMetaEntries.Add(new VehicleMeta("veh_1", "Běžný provoz", string.Empty, "Nafta", "Má klimatizaci", "Řemen", "Automatická"));

        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        var preview = viewModel.BuildVehicleStarterBundlePreview("veh_1");

        Assert.True(preview.TotalMissingCount >= 5);
        Assert.Contains(preview.Items, item => item.Section == VehicleStarterBundleSection.Maintenance && item.Title == "Motorový olej a filtr");
        Assert.Contains(preview.Items, item => item.Section == VehicleStarterBundleSection.Record && item.Title == "Povinné ručení");
        Assert.Contains(preview.Items, item => item.Section == VehicleStarterBundleSection.Reminder && item.Title.Contains("kontrola", StringComparison.CurrentCultureIgnoreCase));

        var message = await viewModel.ApplyVehicleStarterBundleAsync("veh_1", preview.Items);

        Assert.Contains("Balíček pro vozidlo přidal", message);
        Assert.Contains(dataStore.CurrentDataSet.MaintenancePlans, item => item.VehicleId == "veh_1" && item.Title == "Motorový olej a filtr");
        Assert.Contains(dataStore.CurrentDataSet.Records, item => item.VehicleId == "veh_1" && item.Title == "Povinné ručení" && item.AttachmentMode == VehicleRecordAttachmentMode.Managed);
        Assert.Contains(dataStore.CurrentDataSet.Reminders, item => item.VehicleId == "veh_1" && item.Title.Contains("kontrola", StringComparison.CurrentCultureIgnoreCase));

        var secondMessage = await viewModel.ApplyVehicleStarterBundleAsync("veh_1", preview.Items);
        Assert.Equal("Balíček pro vozidlo už neměl žádné nové položky k doplnění.", secondMessage);
    }

    [Fact]
    public async Task Vehicle_starter_bundle_apply_normalizes_record_type_and_reminder_repeat_mode()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);
        var items = new[]
        {
            new VehicleStarterBundleTemplate(
                VehicleStarterBundleSection.Record,
                "Doklad",
                "Starý doklad",
                string.Empty,
                string.Empty,
                "Vlastní typ",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty),
            new VehicleStarterBundleTemplate(
                VehicleStarterBundleSection.Reminder,
                "Připomínka",
                "Kontrola sezóny",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                "10.10.2026",
                "30",
                "Ročně",
                string.Empty)
        };

        var message = await viewModel.ApplyVehicleStarterBundleAsync("veh_1", items);

        Assert.Contains("Balíček pro vozidlo přidal", message);
        var savedRecord = Assert.Single(dataStore.CurrentDataSet.Records);
        Assert.Equal("Povinné ručení", savedRecord.RecordType);
        var savedReminder = Assert.Single(dataStore.CurrentDataSet.Reminders);
        Assert.Equal("Každý rok", savedReminder.RepeatMode);
    }

    [Fact]
    public async Task Maintenance_template_preview_and_apply_adds_only_service_plans()
    {
        var dataRoot = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(dataRoot.DataPath);

        var dataSet = BuildBaseDataSet();
        dataSet.VehicleMetaEntries.Add(new VehicleMeta("veh_1", "Běžný provoz", string.Empty, "Nafta", "Má klimatizaci", "Řemen", "Automatická"));

        var dataStore = new MutableStubLegacyDataStore(dataSet);
        var viewModel = CreateViewModel(dataRoot, dataStore);

        var preview = viewModel.BuildMaintenanceTemplatePreview("veh_1");

        Assert.NotEmpty(preview.Items);
        Assert.All(preview.Items, item => Assert.Equal(VehicleStarterBundleSection.Maintenance, item.Section));

        var message = await viewModel.ApplyMaintenanceTemplatesAsync("veh_1", preview.Items);

        Assert.Contains("Doporučené šablony přidaly", message);
        Assert.Contains(dataStore.CurrentDataSet.MaintenancePlans, item => item.VehicleId == "veh_1" && item.Title == "Motorový olej a filtr");
        Assert.Empty(dataStore.CurrentDataSet.Records);
        Assert.Empty(dataStore.CurrentDataSet.Reminders);
        Assert.Equal(DesktopTabIndexes.Maintenance, viewModel.SelectedVehicleTabIndex);

        var secondMessage = await viewModel.ApplyMaintenanceTemplatesAsync("veh_1", preview.Items);
        Assert.Equal("Doporučené šablony už neměly žádné nové servisní plány k doplnění.", secondMessage);
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

    private static void ConfigureUnits(VehimapDataSet dataSet, string distanceUnit, string volumeUnit)
    {
        ConfigureNumberFormatAndUnits(dataSet, "en-US", "comma", "dot", distanceUnit, volumeUnit);
    }

    private static void ConfigureNumberFormatAndUnits(
        VehimapDataSet dataSet,
        string language,
        string thousandsSeparator,
        string decimalSeparator,
        string distanceUnit,
        string volumeUnit)
    {
        dataSet.Settings.SetValue("app", "language", language);
        dataSet.Settings.SetValue("app", "thousands_separator", thousandsSeparator);
        dataSet.Settings.SetValue("app", "decimal_separator", decimalSeparator);
        dataSet.Settings.SetValue("app", "distance_unit", distanceUnit);
        dataSet.Settings.SetValue("app", "volume_unit", volumeUnit);
    }

    private sealed class MutableStubLegacyDataStore : ILegacyDataStore
    {
        private readonly bool _cloneOnLoad;

        public MutableStubLegacyDataStore(VehimapDataSet dataSet, bool cloneOnLoad = false)
        {
            CurrentDataSet = dataSet;
            _cloneOnLoad = cloneOnLoad;
        }

        public VehimapDataSet CurrentDataSet { get; private set; }

        public Exception? SaveException { get; set; }

        public Task<VehimapDataSet> LoadAsync(VehimapDataRoot dataRoot, CancellationToken cancellationToken = default)
            => Task.FromResult(_cloneOnLoad ? CloneDataSet(CurrentDataSet) : CurrentDataSet);

        public Task SaveAsync(VehimapDataRoot dataRoot, VehimapDataSet dataSet, CancellationToken cancellationToken = default)
        {
            if (SaveException is not null)
            {
                throw SaveException;
            }

            CurrentDataSet = _cloneOnLoad ? CloneDataSet(dataSet) : dataSet;
            return Task.CompletedTask;
        }

        private static VehimapDataSet CloneDataSet(VehimapDataSet source)
        {
            return new VehimapDataSet
            {
                Settings = CloneSettings(source.Settings),
                Vehicles = [.. source.Vehicles],
                HistoryEntries = [.. source.HistoryEntries],
                FuelEntries = [.. source.FuelEntries],
                Records = [.. source.Records],
                VehicleMetaEntries = [.. source.VehicleMetaEntries],
                Reminders = [.. source.Reminders],
                MaintenancePlans = [.. source.MaintenancePlans]
            };
        }

        private static VehimapSettings CloneSettings(VehimapSettings source)
        {
            var clone = new VehimapSettings();
            foreach (var (section, values) in source.Sections)
            {
                foreach (var (key, value) in values)
                {
                    clone.SetValue(section, key, value);
                }
            }

            return clone;
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
