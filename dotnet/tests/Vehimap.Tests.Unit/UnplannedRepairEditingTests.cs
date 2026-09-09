// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.Services;
using Vehimap.Domain.Models;
using Vehimap.Storage.Sqlite;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed partial class MainWindowViewModelEditingTests
{
    [Theory]
    [InlineData("cs-CZ", "Zaznamenat neplánovanou opravu", "Provedená oprava", "9.9.2026", "100", "2500,50", "100")]
    [InlineData("en-US", "Record an unplanned repair", "Repair carried out", "9/9/2026", "100", "2,500.50", "161")]
    public async Task Unplanned_repair_saves_once_to_history_costs_and_service_book_without_changing_plans(
        string language, string heading, string label, string date, string odometer, string cost, string canonicalOdometer)
    {
        var root = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(root.DataPath);
        var data = BuildBaseDataSet();
        var english = language == "en-US";
        ConfigureNumberFormatAndUnits(data, language, english ? "comma" : "none", english ? "dot" : "comma", english ? "mi" : "km", "l");
        var plan = new MaintenancePlan("plan_1", "veh_1", "Pravidelný servis", "10000", "12", "01.01.2026", "50", true, "Vlastní poznámka");
        data.MaintenancePlans.Add(plan);
        var store = new MutableStubLegacyDataStore(data);
        var viewModel = CreateViewModel(root, store);
        var editor = viewModel.HistoryWorkspace;
        var requests = new List<WorkspaceEditorDialogRequest>();
        viewModel.WorkspaceEditorDialogRequested += (_, request) => requests.Add(request);
        var targets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += targets.Add;
        Assert.True(editor.RecordUnplannedRepairCommand.CanExecute(null));

        editor.RecordUnplannedRepairCommand.Execute(null);

        Assert.Equal(new WorkspaceEditorDialogRequest(WorkspaceEditorKind.History, DesktopFocusTarget.RecordUnplannedRepairAction), Assert.Single(requests));
        Assert.Equal(DesktopTabIndexes.History, viewModel.SelectedVehicleTabIndex);
        Assert.True(editor.IsEditingHistory);
        Assert.True(editor.IsRecordingUnplannedRepair);
        Assert.Equal(heading, editor.HistoryEditorHeading);
        Assert.Equal(label, editor.HistoryEditorTypeLabel);
        Assert.Equal(label, editor.HistoryEditorTypeName);
        Assert.Empty(editor.HistoryEditorType);
        Assert.False(editor.RecordUnplannedRepairCommand.CanExecute(null));
        Assert.False(viewModel.CreateMaintenanceCommand.CanExecute(null));
        editor.HistoryEditorDate = date;
        editor.HistoryEditorType = "Výměna prasklé pružiny";
        editor.HistoryEditorOdometer = odometer;
        editor.HistoryEditorCost = cost;
        editor.HistoryEditorNote = "Přední pravá pružina; oprava po poruše.";

        await viewModel.SaveHistoryCommand.ExecuteAsync(null);

        var saved = Assert.Single(store.CurrentDataSet.HistoryEntries);
        Assert.Equal("veh_1", saved.VehicleId);
        Assert.Equal("09.09.2026", saved.EventDate);
        Assert.Equal("Výměna prasklé pružiny", saved.EventType);
        Assert.Equal(canonicalOdometer, saved.Odometer);
        Assert.Equal("2500.5", saved.Cost);
        Assert.Equal("Přední pravá pružina; oprava po poruše.", saved.Note);
        Assert.Equal(plan, Assert.Single(store.CurrentDataSet.MaintenancePlans));
        Assert.Empty(store.CurrentDataSet.Records);
        Assert.Empty(store.CurrentDataSet.Reminders);
        Assert.False(editor.IsEditingHistory);
        Assert.False(editor.IsRecordingUnplannedRepair);
        Assert.Equal(saved.Id, editor.SelectedHistory?.Id);
        Assert.Equal(DesktopFocusTarget.RecordUnplannedRepairAction, targets.Last());
        Assert.Equal(english
            ? "The unplanned repair was saved in history. Maintenance plans were not changed."
            : "Neplánovaná oprava byla uložena do historie. Servisní plány se nezměnily.", editor.HistoryEditorStatus);

        var analysis = new LegacyCostAnalysisService().BuildPeriodSummary(store.CurrentDataSet, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));
        Assert.Equal(2500.5m, analysis.TotalCost);
        var book = new LegacyServiceBookService().BuildVehicleServiceBook(store.CurrentDataSet, "veh_1", new DateOnly(2026, 9, 9));
        Assert.Equal(saved.Id, Assert.Single(book.HistoryEntries).Id);
        Assert.Equal(saved.EventType, book.HistoryEntries[0].EventType);
        Assert.Equal(2500.5m, book.TotalHistoryCost);

        // Reopening is ordinary history editing, not a second repair record or a service-plan completion.
        viewModel.EditSelectedHistoryCommand.Execute(null);
        Assert.False(editor.IsRecordingUnplannedRepair);
        Assert.Equal(saved.EventType, editor.HistoryEditorType);
        await viewModel.SaveHistoryCommand.ExecuteAsync(null);
        Assert.Equal(saved, Assert.Single(store.CurrentDataSet.HistoryEntries));
        Assert.Equal(plan, Assert.Single(store.CurrentDataSet.MaintenancePlans));

        var sqlite = new SqliteVehimapDataStore();
        await sqlite.SaveAsync(root, store.CurrentDataSet);
        var reloaded = await sqlite.LoadAsync(root);
        Assert.Equal(saved, Assert.Single(reloaded.HistoryEntries));
        Assert.Equal(plan, Assert.Single(reloaded.MaintenancePlans));
        Assert.Empty(Directory.GetFiles(root.DataPath, "*.tsv"));
        Assert.False(File.Exists(Path.Combine(root.DataPath, "settings.ini")));
    }

    [Theory]
    [InlineData("31.02.2026", "Pružina", "123", DesktopFocusTarget.HistoryEditorDate)]
    [InlineData("09.09.2026", "", "123", DesktopFocusTarget.HistoryEditorType)]
    [InlineData("09.09.2026", "Pružina", "-1", DesktopFocusTarget.HistoryEditorCost)]
    public async Task Unplanned_repair_validation_keeps_dialog_mode_and_does_not_save(
        string date, string description, string cost, DesktopFocusTarget target)
    {
        var root = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(root.DataPath);
        var store = new MutableStubLegacyDataStore(BuildBaseDataSet());
        var viewModel = CreateViewModel(root, store);
        viewModel.RecordUnplannedRepairCommand.Execute(null);
        var editor = viewModel.HistoryWorkspace;
        editor.HistoryEditorDate = date;
        editor.HistoryEditorType = description;
        editor.HistoryEditorCost = cost;
        var targets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += targets.Add;

        await viewModel.SaveHistoryCommand.ExecuteAsync(null);

        Assert.Equal(target, Assert.Single(targets));
        Assert.True(editor.IsEditingHistory);
        Assert.True(editor.IsRecordingUnplannedRepair);
        if (target == DesktopFocusTarget.HistoryEditorType)
            Assert.Equal("Popište, jaká oprava byla provedena.", editor.HistoryEditorStatus);
        Assert.Empty(store.CurrentDataSet.HistoryEntries);
    }

    [Fact]
    public void Unplanned_repair_cancel_resets_mode_without_saving_and_rejects_reentry()
    {
        var root = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(root.DataPath);
        var store = new MutableStubLegacyDataStore(BuildBaseDataSet());
        var viewModel = CreateViewModel(root, store);
        var editor = viewModel.HistoryWorkspace;
        var requests = new List<WorkspaceEditorDialogRequest>();
        viewModel.WorkspaceEditorDialogRequested += (_, request) => requests.Add(request);
        viewModel.RecordUnplannedRepairCommand.Execute(null);
        editor.HistoryEditorType = "Neuložená oprava";
        viewModel.RecordUnplannedRepairCommand.Execute(null);
        Assert.Single(requests);
        Assert.Equal("Neuložená oprava", editor.HistoryEditorType);
        viewModel.CancelHistoryEditCommand.Execute(null);
        Assert.False(editor.IsEditingHistory);
        Assert.False(editor.IsRecordingUnplannedRepair);
        Assert.Empty(store.CurrentDataSet.HistoryEntries);
        viewModel.CreateHistoryCommand.Execute(null);
        Assert.Equal("Nový historický záznam", editor.HistoryEditorHeading);
        Assert.Equal("Typ historické události", editor.HistoryEditorTypeName);
        Assert.Empty(editor.HistoryEditorType);
        viewModel.CancelHistoryEditCommand.Execute(null);
        viewModel.SelectedVehicle = null;
        Assert.False(viewModel.RecordUnplannedRepairCommand.CanExecute(null));
        var count = requests.Count;
        viewModel.RecordUnplannedRepairCommand.Execute(null);
        Assert.Equal(count, requests.Count);
    }

    [Fact]
    public async Task Unplanned_repair_failed_save_keeps_inputs_and_can_be_retried_without_duplicate()
    {
        var root = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(root.DataPath);
        var store = new MutableStubLegacyDataStore(BuildBaseDataSet(), cloneOnLoad: true) { SaveException = new IOException("Test failure") };
        var viewModel = CreateViewModel(root, store);
        viewModel.RecordUnplannedRepairCommand.Execute(null);
        var editor = viewModel.HistoryWorkspace;
        editor.HistoryEditorDate = "09.09.2026";
        editor.HistoryEditorType = "Oprava pružiny";
        await viewModel.SaveHistoryCommand.ExecuteAsync(null);
        Assert.True(editor.IsEditingHistory);
        Assert.True(editor.IsRecordingUnplannedRepair);
        Assert.Equal("Oprava pružiny", editor.HistoryEditorType);
        Assert.Empty(store.CurrentDataSet.HistoryEntries);
        store.SaveException = null;
        await viewModel.SaveHistoryCommand.ExecuteAsync(null);
        Assert.Single(store.CurrentDataSet.HistoryEntries);
        Assert.False(editor.IsEditingHistory);
    }
}
