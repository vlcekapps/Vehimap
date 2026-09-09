// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed partial class MainWindowViewModelEditingTests
{
    [Theory]
    [InlineData("us_gal")]
    [InlineData("imp_gal")]
    public async Task Repeated_fuel_edits_preserve_canonical_distance_volume_and_cost(string volume)
    {
        var root = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        var data = BuildBaseDataSet();
        ConfigureNumberFormatAndUnits(data, "en-US", "comma", "dot", "mi", volume);
        data.FuelEntries.Add(new FuelEntry("test_fuel", "veh_1", "01.01.2026", "12345", "3.12", "1234.56", true, "Benzin", ""));
        var store = new MutableStubLegacyDataStore(data);
        var model = CreateViewModel(root, store);
        for (var i = 0; i < 5; i++)
        {
            model.EditSelectedFuelCommand.Execute(null);
            Assert.Equal("1,234.56", model.FuelWorkspace.FuelEditorTotalCost);
            await model.SaveFuelCommand.ExecuteAsync(null);
            var saved = Assert.Single(store.CurrentDataSet.FuelEntries);
            Assert.Equal("12345", saved.Odometer);
            Assert.Equal("3.12", saved.Liters);
            Assert.Equal("1234.56", saved.TotalCost);
            Assert.False(model.FuelWorkspace.IsEditingFuel);
        }
    }

    [Theory]
    [InlineData("1e3")]
    [InlineData("1,23")]
    [InlineData("79228162514264337593543950335")]
    public async Task Invalid_fuel_distance_keeps_editor_open_and_data_unchanged(string input)
    {
        var root = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        var data = BuildBaseDataSet();
        ConfigureNumberFormatAndUnits(data, "en-US", "comma", "dot", "mi", "us_gal");
        data.FuelEntries.Add(new FuelEntry("test_fuel", "veh_1", "01.01.2026", "12345", "3.12", "1234.56", true, "Benzin", ""));
        var store = new MutableStubLegacyDataStore(data);
        var model = CreateViewModel(root, store);
        model.EditSelectedFuelCommand.Execute(null);
        model.FuelWorkspace.FuelEditorOdometer = input;
        await model.SaveFuelCommand.ExecuteAsync(null);
        Assert.True(model.FuelWorkspace.IsEditingFuel);
        Assert.Equal("12345", Assert.Single(store.CurrentDataSet.FuelEntries).Odometer);
    }

    [Fact]
    public void Excessive_settings_distance_reports_validation_error_without_crashing()
    {
        var model = SettingsDialogViewModel.FromSnapshot(
            new DesktopSupportedSettingsSnapshot(30, 30, 31, 1000, false, false, false, false, 7, 10), "");
        model.MaintenanceReminderDistance = "79228162514264337593543950335";
        Assert.False(model.TryBuildSnapshot(out _, out var error));
        Assert.NotEmpty(error);
    }
}
