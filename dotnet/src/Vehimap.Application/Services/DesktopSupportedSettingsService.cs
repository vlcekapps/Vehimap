using System.Globalization;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class DesktopSupportedSettingsService
{
    public DesktopSupportedSettingsSnapshot Read(VehimapSettings settings)
    {
        return new DesktopSupportedSettingsSnapshot(
            ReadBoundedInt(settings, "notifications", "technical_reminder_days", 30, 0, 3650),
            ReadBoundedInt(settings, "notifications", "green_card_reminder_days", 30, 0, 3650),
            ReadBoundedInt(settings, "notifications", "maintenance_reminder_days", 31, 0, 3650),
            ReadBoundedInt(settings, "notifications", "maintenance_reminder_km", 1000, 1, 999999),
            settings.GetValue("app", "show_dashboard_on_launch", "0") == "1");
    }

    public void Apply(VehimapSettings settings, DesktopSupportedSettingsSnapshot snapshot)
    {
        settings.SetValue("notifications", "technical_reminder_days", snapshot.TechnicalReminderDays.ToString(CultureInfo.InvariantCulture));
        settings.SetValue("notifications", "green_card_reminder_days", snapshot.GreenCardReminderDays.ToString(CultureInfo.InvariantCulture));
        settings.SetValue("notifications", "maintenance_reminder_days", snapshot.MaintenanceReminderDays.ToString(CultureInfo.InvariantCulture));
        settings.SetValue("notifications", "maintenance_reminder_km", snapshot.MaintenanceReminderKm.ToString(CultureInfo.InvariantCulture));
        settings.SetValue("app", "show_dashboard_on_launch", snapshot.ShowDashboardOnLaunch ? "1" : "0");
    }

    private static int ReadBoundedInt(VehimapSettings settings, string section, string key, int defaultValue, int minValue, int maxValue)
    {
        var raw = settings.GetValue(section, key, defaultValue.ToString(CultureInfo.InvariantCulture));
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value >= minValue && value <= maxValue
            ? value
            : defaultValue;
    }
}
