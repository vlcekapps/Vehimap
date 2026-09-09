// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed partial class MainWindowViewModelEditingTests
{
    [Fact]
    public async Task Repair_workspace_preserves_failed_inputs_navigates_from_audit_and_never_notifies()
    {
        var root = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(root.DataPath);
        var store = new MutableStubLegacyDataStore(BuildBaseDataSet(), cloneOnLoad: true);
        var shell = CreateViewModel(root, store);
        var baseline = shell.BuildBackgroundSnapshot();
        var model = shell.BuildRepairsModel()!;
        Assert.False(model.CanChange);
        var create = model.CreateEditor(RepairAction.Create)!;
        create.RepairTitle = "Závada topení"; create.ReportedDate = "1.9.2026";
        store.SaveException = new IOException("Test failure");
        await create.SaveCommand.ExecuteAsync(null);
        Assert.True(create.IsEditing); Assert.NotEmpty(create.Status); Assert.Empty(store.CurrentDataSet.Repairs);
        store.SaveException = null;
        await create.SaveCommand.ExecuteAsync(null);
        Assert.False(create.IsEditing);
        model.Refresh(create.SavedRepairId);
        Assert.Equal("Závada topení", Assert.Single(model.Items).Repair.Title);
        var snapshot = shell.BuildBackgroundSnapshot();
        Assert.Equal(baseline.NotificationKey, snapshot.NotificationKey);
        Assert.Equal(baseline.NotificationMessage, snapshot.NotificationMessage);
        string? requested = null;
        shell.RepairsRequested += id => requested = id;
        var audit = shell.AuditWorkspace.AuditItems.Single(i => i.EntityKind == ApplicationEntityKinds.Repair);
        Assert.True(await shell.OpenAuditItemAsync(audit));
        Assert.Equal(create.SavedRepairId, requested);
        var complete = model.CreateEditor(RepairAction.Complete)!;
        shell.IsRepairsWindowOpen = true;
        Assert.False(shell.CanUseVehicleList);
        complete.CompletedDate = "2.9.2026"; complete.Reason = "Oprava topení"; complete.Cost = "125,50";
        await complete.SaveCommand.ExecuteAsync(null);
        Assert.False(complete.IsEditing);
        shell.IsRepairsWindowOpen = false;
        Assert.True(shell.CanUseVehicleList);
        Assert.True(shell.RecordUnplannedRepairCommand.CanExecute(null));
        model.Refresh(complete.SavedRepairId);
        Assert.False(model.CanChange);
        Assert.Null(model.CreateEditor(RepairAction.Complete));
        var repair = Assert.Single(store.CurrentDataSet.Repairs);
        var history = Assert.Single(store.CurrentDataSet.HistoryEntries);
        Assert.Equal(history.Id, repair.HistoryEntryId);
        shell.HistoryWorkspace.SelectedHistory = shell.HistoryWorkspace.SelectedVehicleHistory.Single(h => h.Id == history.Id);
        await shell.DeleteSelectedHistoryCommand.ExecuteAsync(null);
        Assert.Single(store.CurrentDataSet.HistoryEntries);
    }
}
