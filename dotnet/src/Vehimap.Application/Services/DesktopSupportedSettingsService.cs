using System.Globalization;
using Vehimap.Application.Models;
using Vehimap.Domain.Models;

namespace Vehimap.Application.Services;

public sealed class DesktopSupportedSettingsService
{
    private readonly AppLocaleDefaultsService _localeDefaultsService;

    public DesktopSupportedSettingsService()
        : this(new AppLocaleDefaultsService())
    {
    }

    public DesktopSupportedSettingsService(AppLocaleDefaultsService localeDefaultsService)
    {
        _localeDefaultsService = localeDefaultsService;
    }

    public DesktopSupportedSettingsSnapshot Read(VehimapSettings settings, bool? runAtStartupOverride = null)
    {
        var language = AppCultureService.NormalizeLanguage(settings.GetValue("app", "language", AppCultureService.SystemLanguage));
        var localeDefaults = _localeDefaultsService.GetDefaultsForLanguage(language);

        return new DesktopSupportedSettingsSnapshot(
            ReadBoundedInt(settings, "notifications", "technical_reminder_days", 30, 0, 3650),
            ReadBoundedInt(settings, "notifications", "green_card_reminder_days", 30, 0, 3650),
            ReadBoundedInt(settings, "notifications", "maintenance_reminder_days", 31, 0, 3650),
            ReadBoundedInt(settings, "notifications", "maintenance_reminder_km", 1000, 1, 999999),
            runAtStartupOverride ?? ReadBool(settings, "app", "run_at_startup", false),
            ReadBool(settings, "app", "hide_on_launch", false),
            ReadBool(settings, "app", "show_dashboard_on_launch", false),
            ReadBool(settings, "backups", "automatic_backups_enabled", false),
            ReadBoundedInt(settings, "backups", "automatic_backup_interval_days", 1, 1, 999),
            ReadBoundedInt(settings, "backups", "automatic_backup_keep_count", 30, 1, 999),
            language,
            AppCultureService.NormalizeThousandsSeparator(settings.GetValue("app", "thousands_separator", localeDefaults.ThousandsSeparator)),
            AppCultureService.NormalizeDecimalSeparator(settings.GetValue("app", "decimal_separator", localeDefaults.DecimalSeparator)),
            AppUnitFormatService.NormalizeDistanceUnit(settings.GetValue("app", "distance_unit", localeDefaults.DistanceUnit)),
            AppUnitFormatService.NormalizeVolumeUnit(settings.GetValue("app", "volume_unit", localeDefaults.VolumeUnit)));
    }

    public void Apply(VehimapSettings settings, DesktopSupportedSettingsSnapshot snapshot)
    {
        settings.SetValue("notifications", "technical_reminder_days", snapshot.TechnicalReminderDays.ToString(CultureInfo.InvariantCulture));
        settings.SetValue("notifications", "green_card_reminder_days", snapshot.GreenCardReminderDays.ToString(CultureInfo.InvariantCulture));
        settings.SetValue("notifications", "maintenance_reminder_days", snapshot.MaintenanceReminderDays.ToString(CultureInfo.InvariantCulture));
        settings.SetValue("notifications", "maintenance_reminder_km", snapshot.MaintenanceReminderKm.ToString(CultureInfo.InvariantCulture));
        settings.SetValue("app", "run_at_startup", snapshot.RunAtStartup ? "1" : "0");
        settings.SetValue("app", "hide_on_launch", snapshot.HideOnLaunch ? "1" : "0");
        settings.SetValue("app", "show_dashboard_on_launch", snapshot.ShowDashboardOnLaunch ? "1" : "0");
        settings.SetValue("backups", "automatic_backups_enabled", snapshot.AutomaticBackupsEnabled ? "1" : "0");
        settings.SetValue("backups", "automatic_backup_interval_days", snapshot.AutomaticBackupIntervalDays.ToString(CultureInfo.InvariantCulture));
        settings.SetValue("backups", "automatic_backup_keep_count", snapshot.AutomaticBackupKeepCount.ToString(CultureInfo.InvariantCulture));
        settings.SetValue("app", "language", AppCultureService.NormalizeLanguage(snapshot.Language));
        settings.SetValue("app", "thousands_separator", AppCultureService.NormalizeThousandsSeparator(snapshot.ThousandsSeparator));
        settings.SetValue("app", "decimal_separator", AppCultureService.NormalizeDecimalSeparator(snapshot.DecimalSeparator));
        settings.SetValue("app", "distance_unit", AppUnitFormatService.NormalizeDistanceUnit(snapshot.DistanceUnit));
        settings.SetValue("app", "volume_unit", AppUnitFormatService.NormalizeVolumeUnit(snapshot.VolumeUnit));
    }

    private static bool ReadBool(VehimapSettings settings, string section, string key, bool defaultValue)
    {
        return settings.GetValue(section, key, defaultValue ? "1" : "0") == "1";
    }

    private static int ReadBoundedInt(VehimapSettings settings, string section, string key, int defaultValue, int minValue, int maxValue)
    {
        var raw = settings.GetValue(section, key, defaultValue.ToString(CultureInfo.InvariantCulture));
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value >= minValue && value <= maxValue
            ? value
            : defaultValue;
    }
}
