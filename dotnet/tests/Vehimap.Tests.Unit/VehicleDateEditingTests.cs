// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;
using Vehimap.Application.Abstractions;
using Vehimap.Desktop.ViewModels;
using Vehimap.Domain.Models;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed partial class MainWindowViewModelEditingTests
{
    [Theory]
    [InlineData("cs-CZ", "3.7.2027", "03.07.2027")]
    [InlineData("en-US", "7/3/2027", "7/3/2027")]
    public async Task Vehicle_editor_saves_full_dates_and_reopens_in_selected_language(string language, string input, string displayed)
    {
        var root = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(root.DataPath);
        var data = BuildBaseDataSet();
        ConfigureNumberFormatAndUnits(data, language, "none", "comma", "km", "l");
        var store = new MutableStubLegacyDataStore(data);
        var viewModel = CreateViewModel(root, store);
        var before = data.Vehicles[0];
        viewModel.EditSelectedVehicleCommand.Execute(null);
        var editor = viewModel.VehicleDetailWorkspace;
        editor.VehicleEditorLastTk = input;
        editor.VehicleEditorNextTk = input;
        editor.VehicleEditorGreenCardFrom = input;
        editor.VehicleEditorGreenCardTo = input;
        await viewModel.SaveVehicleCommand.ExecuteAsync(null);
        var expected = before with { LastTk = "03.07.2027", NextTk = "03.07.2027", GreenCardFrom = "03.07.2027", GreenCardTo = "03.07.2027" };
        Assert.Equal(expected, store.CurrentDataSet.Vehicles[0]);
        Assert.False(editor.IsEditingVehicle);
        viewModel.EditSelectedVehicleCommand.Execute(null);
        Assert.Equal(displayed, editor.VehicleEditorLastTk);
        Assert.Equal(displayed, editor.VehicleEditorNextTk);
        Assert.Equal(displayed, editor.VehicleEditorGreenCardFrom);
        Assert.Equal(displayed, editor.VehicleEditorGreenCardTo);
        await viewModel.SaveVehicleCommand.ExecuteAsync(null);
        Assert.Equal(expected, store.CurrentDataSet.Vehicles[0]);
    }

    [Theory]
    [InlineData("LastTk", DesktopFocusTarget.VehicleEditorLastTk)]
    [InlineData("NextTk", DesktopFocusTarget.VehicleEditorNextTk)]
    [InlineData("GreenCardFrom", DesktopFocusTarget.VehicleEditorGreenCardFrom)]
    [InlineData("GreenCardTo", DesktopFocusTarget.VehicleEditorGreenCardTo)]
    public async Task Vehicle_editor_rejects_nonexistent_day_without_closing_or_saving(string field, DesktopFocusTarget focus)
    {
        var root = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(root.DataPath);
        var data = BuildBaseDataSet();
        var before = data.Vehicles[0];
        var store = new MutableStubLegacyDataStore(data);
        var viewModel = CreateViewModel(root, store);
        viewModel.EditSelectedVehicleCommand.Execute(null);
        var editor = viewModel.VehicleDetailWorkspace;
        var property = editor.GetType().GetProperty("VehicleEditor" + field)!;
        property.SetValue(editor, "31.02.2027");
        var targets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += targets.Add;
        await viewModel.SaveVehicleCommand.ExecuteAsync(null);
        Assert.Equal(focus, Assert.Single(targets));
        Assert.True(editor.IsEditingVehicle);
        Assert.Equal(before, store.CurrentDataSet.Vehicles[0]);
    }

    [Fact]
    public async Task Vehicle_editor_rejects_reversed_insurance_days_in_same_month()
    {
        var root = new VehimapDataRoot(_tempRoot, Path.Combine(_tempRoot, "data"), true);
        Directory.CreateDirectory(root.DataPath);
        var store = new MutableStubLegacyDataStore(BuildBaseDataSet());
        var viewModel = CreateViewModel(root, store);
        viewModel.EditSelectedVehicleCommand.Execute(null);
        viewModel.VehicleDetailWorkspace.VehicleEditorGreenCardFrom = "04.07.2027";
        viewModel.VehicleDetailWorkspace.VehicleEditorGreenCardTo = "03.07.2027";
        var targets = new List<DesktopFocusTarget>();
        viewModel.FocusRequested += targets.Add;
        await viewModel.SaveVehicleCommand.ExecuteAsync(null);
        Assert.Equal(DesktopFocusTarget.VehicleEditorGreenCardFrom, Assert.Single(targets));
        Assert.True(viewModel.VehicleDetailWorkspace.IsEditingVehicle);
        Assert.Equal("05/2025", store.CurrentDataSet.Vehicles[0].GreenCardFrom);
    }
}
