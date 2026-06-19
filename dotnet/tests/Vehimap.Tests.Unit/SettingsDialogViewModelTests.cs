using Vehimap.Application.Models;
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
            "Bez automatické zálohy.");
        viewModel.AutomaticBackupIntervalDays = "0";

        var valid = viewModel.TryBuildSnapshot(out _, out var errorMessage);

        Assert.False(valid);
        Assert.Contains("Interval automatické zálohy", errorMessage);
    }
}
