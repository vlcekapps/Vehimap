// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed partial class MainWindowViewModelEditingTests
{
    [Theory]
    [InlineData("success")]
    [InlineData("failure")]
    [InlineData("cancel")]
    public async Task Package_import_publishes_only_committed_data_and_blocks_competing_actions(string outcome)
    {
        var root = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(root.DataPath);
        var data = BuildBaseDataSet();
        var store = new MutableStubLegacyDataStore(data) { SaveException = new IOException("Caller must not perform a second save.") };
        var importer = new DelayedPackageImporter();
        var vm = CreateViewModel(root, store, importer);
        var imported = new VehimapDataSet { Vehicles = [data.Vehicles[0] with { Id = "incoming", Name = "Incoming" }] };
        var operation = vm.ImportVehiclePackageAsync("fixture.vehimapvehicle");
        await importer.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal("Milena", Assert.Single(vm.Vehicles).Name);
        Assert.False(vm.CanImportVehiclePackage);
        Assert.False(vm.CanCreateVehicle);
        Assert.False(vm.CanUseVehicleList);
        Assert.True(vm.BlockActionDuringDataImport());
        vm.ConfirmPendingEditsHandler = _ => Task.FromResult(true);
        Assert.False(await vm.ConfirmDiscardPendingEditsAsync("exit"));
        Assert.False(await vm.ShouldShowAndRememberDueNotificationAsync("due-item"));
        var automaticBackup = await vm.RunAutomaticBackupCheckAsync();
        Assert.False(automaticBackup.Created);
        Assert.False(automaticBackup.IsError);
        await vm.SaveSupportedSettingsAsync(vm.GetSupportedSettingsSnapshot());
        await vm.SetDashboardShowOnLaunchAsync(true);
        await vm.ImportVehiclePackageAsync("second.vehimapvehicle");
        Assert.Equal(1, importer.Calls);

        if (outcome == "success") importer.Completion.SetResult(new(imported, "incoming", "Incoming", 0));
        else if (outcome == "cancel") importer.Completion.SetCanceled();
        else importer.Completion.SetException(new IOException("Injected import failure"));

        if (outcome == "cancel") await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
        else await operation;
        Assert.Equal(outcome == "success" ? "Incoming" : "Milena", Assert.Single(vm.Vehicles).Name);
        Assert.True(vm.CanUseVehicleList);
        Assert.True(vm.CanImportVehiclePackage);
        Assert.False(vm.BlockActionDuringDataImport());
        Assert.False(vm.HasPendingEdits);
        Assert.Equal("Milena", Assert.Single(data.Vehicles).Name);
    }

    private sealed class DelayedPackageImporter : IVehiclePackageService
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<VehiclePackageImportResult> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Calls { get; private set; }
        public Task<VehiclePackageExportResult> ExportVehicleAsync(string path, VehimapDataRoot root, VehimapDataSet data, string id, CancellationToken ct = default)
            => throw new NotSupportedException();
        public Task<VehiclePackageImportResult> ImportVehicleAsync(string path, VehimapDataRoot root, VehimapDataSet data, CancellationToken ct = default)
        {
            Calls++;
            Started.SetResult();
            return Completion.Task;
        }
    }
}
