namespace Vehimap.Application.Models;

public sealed record DesktopSupportedSettingsSnapshot(
    int TechnicalReminderDays,
    int GreenCardReminderDays,
    int MaintenanceReminderDays,
    int MaintenanceReminderKm,
    bool RunAtStartup,
    bool HideOnLaunch,
    bool ShowDashboardOnLaunch,
    bool AutomaticBackupsEnabled,
    int AutomaticBackupIntervalDays,
    int AutomaticBackupKeepCount,
    string Language = "system",
    string ThousandsSeparator = "culture",
    string DecimalSeparator = "culture",
    string DistanceUnit = "km",
    string VolumeUnit = "l");
