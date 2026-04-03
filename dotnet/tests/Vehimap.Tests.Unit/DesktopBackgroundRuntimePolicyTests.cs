using Vehimap.Desktop.Services;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class DesktopBackgroundRuntimePolicyTests
{
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    public void Can_hide_on_launch_requires_supported_tray(bool traySupported, bool hideOnLaunchRequested, bool expected)
    {
        var actual = DesktopBackgroundRuntimePolicy.CanHideOnLaunch(traySupported, hideOnLaunchRequested);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Can_reload_in_background_is_disabled_while_editing(bool hasPendingEdits, bool expected)
    {
        var actual = DesktopBackgroundRuntimePolicy.CanReloadInBackground(hasPendingEdits);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    public void Can_run_automatic_backup_requires_request_and_no_pending_edits(bool backupRequested, bool hasPendingEdits, bool expected)
    {
        var actual = DesktopBackgroundRuntimePolicy.CanRunAutomaticBackup(backupRequested, hasPendingEdits);

        Assert.Equal(expected, actual);
    }
}
