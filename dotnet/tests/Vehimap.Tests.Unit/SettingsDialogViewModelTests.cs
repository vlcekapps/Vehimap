using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.ViewModels;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class SettingsDialogViewModelTests
{
    [Fact]
    public void Backup_interval_fields_are_configurable_only_when_automatic_backups_are_enabled()
    {
        var viewModel = SettingsDialogViewModel.FromSnapshot(
            new DesktopSupportedSettingsSnapshot(30, 30, 31, 1000, false, false, true, false, 7, 10),
            "Bez automatické zálohy.");

        Assert.False(viewModel.CanConfigureAutomaticBackups);

        viewModel.AutomaticBackupsEnabled = true;

        Assert.True(viewModel.CanConfigureAutomaticBackups);
    }

    [Fact]
    public void Disabled_automatic_backups_do_not_require_valid_interval_fields()
    {
        var viewModel = SettingsDialogViewModel.FromSnapshot(
            new DesktopSupportedSettingsSnapshot(30, 30, 31, 1000, false, false, true, false, 7, 10),
            "Bez automatické zálohy.");
        viewModel.AutomaticBackupIntervalDays = "neplatné";
        viewModel.AutomaticBackupKeepCount = "také neplatné";

        var valid = viewModel.TryBuildSnapshot(out var snapshot, out var errorMessage);

        Assert.True(valid);
        Assert.Empty(errorMessage);
        Assert.False(snapshot.AutomaticBackupsEnabled);
        Assert.Equal(1, snapshot.AutomaticBackupIntervalDays);
        Assert.Equal(30, snapshot.AutomaticBackupKeepCount);
    }

    [Fact]
    public void Enabled_automatic_backups_require_valid_interval_fields()
    {
        var viewModel = SettingsDialogViewModel.FromSnapshot(
            new DesktopSupportedSettingsSnapshot(30, 30, 31, 1000, false, false, true, true, 7, 10),
            "Bez automatické zálohy.",
            new ResourceAppLocalizer(System.Globalization.CultureInfo.GetCultureInfo("cs-CZ")));
        viewModel.AutomaticBackupIntervalDays = "0";

        var valid = viewModel.TryBuildSnapshot(out _, out var errorMessage);

        Assert.False(valid);
        Assert.Contains("Interval automatické zálohy", errorMessage);
    }

    [Fact]
    public void Localization_and_unit_options_round_trip_through_snapshot()
    {
        var viewModel = SettingsDialogViewModel.FromSnapshot(
            new DesktopSupportedSettingsSnapshot(
                30,
                30,
                31,
                1000,
                false,
                false,
                true,
                false,
                7,
                10,
                "en-US",
                "comma",
                "dot",
                "mi",
                "us_gal"),
            "Bez automatické zálohy.",
            new ResourceAppLocalizer(System.Globalization.CultureInfo.GetCultureInfo("en-US")));

        Assert.Equal("en-US", viewModel.SelectedLanguageOption?.Value);
        Assert.Equal("comma", viewModel.SelectedThousandsSeparatorOption?.Value);
        Assert.Equal("dot", viewModel.SelectedDecimalSeparatorOption?.Value);
        Assert.Equal("mi", viewModel.SelectedDistanceUnitOption?.Value);
        Assert.Equal("us_gal", viewModel.SelectedVolumeUnitOption?.Value);

        var valid = viewModel.TryBuildSnapshot(out var snapshot, out var errorMessage);

        Assert.True(valid);
        Assert.Empty(errorMessage);
        Assert.Equal("en-US", snapshot.Language);
        Assert.Equal("comma", snapshot.ThousandsSeparator);
        Assert.Equal("dot", snapshot.DecimalSeparator);
        Assert.Equal("mi", snapshot.DistanceUnit);
        Assert.Equal("us_gal", snapshot.VolumeUnit);
    }

    [Fact]
    public void Maintenance_reminder_distance_is_displayed_in_selected_unit_and_stored_as_kilometers()
    {
        var viewModel = SettingsDialogViewModel.FromSnapshot(
            new DesktopSupportedSettingsSnapshot(
                30,
                30,
                31,
                1000,
                false,
                false,
                true,
                false,
                7,
                10,
                "en-US",
                "comma",
                "dot",
                "mi",
                "us_gal"),
            "No automatic backup.",
            new ResourceAppLocalizer(System.Globalization.CultureInfo.GetCultureInfo("en-US")));

        Assert.Equal("621.4", viewModel.MaintenanceReminderKm);
        Assert.Equal("Maintenance reminder distance in mi", viewModel.MaintenanceReminderDistanceName);

        var valid = viewModel.TryBuildSnapshot(out var snapshot, out var errorMessage);

        Assert.True(valid);
        Assert.Empty(errorMessage);
        Assert.Equal(1000, snapshot.MaintenanceReminderKm);

        viewModel.MaintenanceReminderKm = "100";

        valid = viewModel.TryBuildSnapshot(out snapshot, out errorMessage);

        Assert.True(valid);
        Assert.Empty(errorMessage);
        Assert.Equal(161, snapshot.MaintenanceReminderKm);
    }

    [Fact]
    public void Changing_distance_unit_reformats_maintenance_reminder_without_changing_storage_meaning()
    {
        var viewModel = SettingsDialogViewModel.FromSnapshot(
            new DesktopSupportedSettingsSnapshot(
                30,
                30,
                31,
                1000,
                false,
                false,
                true,
                false,
                7,
                10,
                "cs-CZ",
                "none",
                "comma",
                "km",
                "l"),
            "Bez automatické zálohy.",
            new ResourceAppLocalizer(System.Globalization.CultureInfo.GetCultureInfo("cs-CZ")));

        Assert.Equal("1000", viewModel.MaintenanceReminderKm);

        viewModel.SelectedDistanceUnitOption = viewModel.DistanceUnitOptions.First(option => option.Value == "mi");

        Assert.Equal("621,4", viewModel.MaintenanceReminderKm);
        Assert.Equal("Upozornění na údržbu podle vzdálenosti v mi", viewModel.MaintenanceReminderDistanceName);

        var valid = viewModel.TryBuildSnapshot(out var snapshot, out var errorMessage);

        Assert.True(valid);
        Assert.Empty(errorMessage);
        Assert.Equal(1000, snapshot.MaintenanceReminderKm);
        Assert.Equal("mi", snapshot.DistanceUnit);
    }

    [Fact]
    public void Changing_number_separators_reformats_maintenance_reminder_without_changing_storage_meaning()
    {
        var viewModel = SettingsDialogViewModel.FromSnapshot(
            new DesktopSupportedSettingsSnapshot(
                30,
                30,
                31,
                999999,
                false,
                false,
                true,
                false,
                7,
                10,
                "en-US",
                "comma",
                "dot",
                "km",
                "l"),
            "No automatic backup.",
            new ResourceAppLocalizer(System.Globalization.CultureInfo.GetCultureInfo("en-US")));

        Assert.Equal("999,999", viewModel.MaintenanceReminderKm);

        viewModel.SelectedThousandsSeparatorOption = viewModel.ThousandsSeparatorOptions.First(option => option.Value == "none");

        Assert.Equal("999999", viewModel.MaintenanceReminderKm);

        var valid = viewModel.TryBuildSnapshot(out var snapshot, out var errorMessage);

        Assert.True(valid);
        Assert.Empty(errorMessage);
        Assert.Equal(999999, snapshot.MaintenanceReminderKm);
        Assert.Equal("none", snapshot.ThousandsSeparator);
    }

    [Fact]
    public void Conflicting_number_separators_are_rejected_before_ambiguous_parsing()
    {
        var viewModel = SettingsDialogViewModel.FromSnapshot(
            new DesktopSupportedSettingsSnapshot(
                30,
                30,
                31,
                1000,
                false,
                false,
                true,
                false,
                7,
                10,
                "en-US",
                "comma",
                "dot",
                "mi",
                "us_gal"),
            "No automatic backup.",
            new ResourceAppLocalizer(System.Globalization.CultureInfo.GetCultureInfo("en-US")));

        viewModel.SelectedDecimalSeparatorOption = viewModel.DecimalSeparatorOptions.First(option => option.Value == "comma");

        var valid = viewModel.TryBuildSnapshot(out _, out var errorMessage);

        Assert.False(valid);
        Assert.Equal("The thousands separator and decimal separator must be different. Choose no thousands separator or a different symbol.", errorMessage);
    }
}
