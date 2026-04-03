namespace Vehimap.Desktop.Services;

internal static class DesktopBackgroundRuntimePolicy
{
    public static bool CanHideOnLaunch(bool traySupported, bool hideOnLaunchRequested) =>
        traySupported && hideOnLaunchRequested;

    public static bool CanReloadInBackground(bool hasPendingEdits) =>
        !hasPendingEdits;

    public static bool CanRunAutomaticBackup(bool backupRequested, bool hasPendingEdits) =>
        backupRequested && !hasPendingEdits;
}
