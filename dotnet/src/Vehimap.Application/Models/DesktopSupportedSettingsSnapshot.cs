namespace Vehimap.Application.Models;

public sealed record DesktopSupportedSettingsSnapshot(
    int TechnicalReminderDays,
    int GreenCardReminderDays,
    int MaintenanceReminderDays,
    int MaintenanceReminderKm,
    bool ShowDashboardOnLaunch);
