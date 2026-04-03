namespace Vehimap.Desktop.Services;

internal static class DesktopBackgroundRuntimePolicy
{
    public static bool CanHideOnLaunch(bool traySupported, bool hideOnLaunchRequested) =>
        traySupported && hideOnLaunchRequested;

    public static bool CanReloadInBackground(bool hasPendingEdits) =>
        !hasPendingEdits;

    public static bool CanRunAutomaticBackup(bool backupRequested, bool hasPendingEdits) =>
        backupRequested && !hasPendingEdits;

    public static bool CanShowAutomaticBackupNotification(bool notifyWhenHidden, bool backupCreated, bool backupErrored) =>
        notifyWhenHidden && (backupCreated || backupErrored);

    public static bool CanShowDueNotification(bool hasNotification, string notificationKey, string lastNotificationKey) =>
        hasNotification && !string.Equals(notificationKey, lastNotificationKey, StringComparison.Ordinal);
}
