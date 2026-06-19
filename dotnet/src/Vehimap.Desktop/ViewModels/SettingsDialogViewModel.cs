using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class SettingsDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string technicalReminderDays = string.Empty;

    [ObservableProperty]
    private string greenCardReminderDays = string.Empty;

    [ObservableProperty]
    private string maintenanceReminderDays = string.Empty;

    [ObservableProperty]
    private string maintenanceReminderKm = string.Empty;

    [ObservableProperty]
    private bool runAtStartup;

    [ObservableProperty]
    private bool hideOnLaunch;

    [ObservableProperty]
    private bool showDashboardOnLaunch;

    [ObservableProperty]
    private bool automaticBackupsEnabled;

    [ObservableProperty]
    private string automaticBackupIntervalDays = string.Empty;

    [ObservableProperty]
    private string automaticBackupKeepCount = string.Empty;

    [ObservableProperty]
    private string automaticBackupStatus = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Upravte podporované volby a potvrďte je tlačítkem Uložit.";

    public bool CanConfigureAutomaticBackups => AutomaticBackupsEnabled;

    public static SettingsDialogViewModel FromSnapshot(DesktopSupportedSettingsSnapshot snapshot, string automaticBackupStatus)
    {
        return new SettingsDialogViewModel
        {
            TechnicalReminderDays = snapshot.TechnicalReminderDays.ToString(),
            GreenCardReminderDays = snapshot.GreenCardReminderDays.ToString(),
            MaintenanceReminderDays = snapshot.MaintenanceReminderDays.ToString(),
            MaintenanceReminderKm = snapshot.MaintenanceReminderKm.ToString(),
            RunAtStartup = snapshot.RunAtStartup,
            HideOnLaunch = snapshot.HideOnLaunch,
            ShowDashboardOnLaunch = snapshot.ShowDashboardOnLaunch,
            AutomaticBackupsEnabled = snapshot.AutomaticBackupsEnabled,
            AutomaticBackupIntervalDays = snapshot.AutomaticBackupIntervalDays.ToString(),
            AutomaticBackupKeepCount = snapshot.AutomaticBackupKeepCount.ToString(),
            AutomaticBackupStatus = automaticBackupStatus
        };
    }

    public bool TryBuildSnapshot(out DesktopSupportedSettingsSnapshot snapshot, out string errorMessage)
    {
        if (!TryParseBoundedInt(TechnicalReminderDays, 0, 3650, "Upozornění na TK", out var technicalReminderDays, out errorMessage)
            || !TryParseBoundedInt(GreenCardReminderDays, 0, 3650, "Upozornění na zelenou kartu", out var greenCardReminderDays, out errorMessage)
            || !TryParseBoundedInt(MaintenanceReminderDays, 0, 3650, "Upozornění na údržbu podle dnů", out var maintenanceReminderDays, out errorMessage)
            || !TryParseBoundedInt(MaintenanceReminderKm, 1, 999999, "Upozornění na údržbu podle km", out var maintenanceReminderKm, out errorMessage))
        {
            snapshot = default!;
            return false;
        }

        var automaticBackupIntervalDays = 1;
        var automaticBackupKeepCount = 30;
        if (AutomaticBackupsEnabled
            && (!TryParseBoundedInt(AutomaticBackupIntervalDays, 1, 999, "Interval automatické zálohy ve dnech", out automaticBackupIntervalDays, out errorMessage)
                || !TryParseBoundedInt(AutomaticBackupKeepCount, 1, 999, "Počet ponechaných automatických záloh", out automaticBackupKeepCount, out errorMessage)))
        {
            snapshot = default!;
            return false;
        }

        if (!AutomaticBackupsEnabled)
        {
            if (TryParseBoundedInt(AutomaticBackupIntervalDays, 1, 999, "Interval automatické zálohy ve dnech", out var parsedIntervalDays, out _))
            {
                automaticBackupIntervalDays = parsedIntervalDays;
            }

            if (TryParseBoundedInt(AutomaticBackupKeepCount, 1, 999, "Počet ponechaných automatických záloh", out var parsedKeepCount, out _))
            {
                automaticBackupKeepCount = parsedKeepCount;
            }
        }

        snapshot = new DesktopSupportedSettingsSnapshot(
            technicalReminderDays,
            greenCardReminderDays,
            maintenanceReminderDays,
            maintenanceReminderKm,
            RunAtStartup,
            HideOnLaunch,
            ShowDashboardOnLaunch,
            AutomaticBackupsEnabled,
            automaticBackupIntervalDays,
            automaticBackupKeepCount);
        errorMessage = string.Empty;
        return true;
    }

    partial void OnAutomaticBackupsEnabledChanged(bool value)
    {
        OnPropertyChanged(nameof(CanConfigureAutomaticBackups));
    }

    private static bool TryParseBoundedInt(string value, int minValue, int maxValue, string label, out int parsedValue, out string errorMessage)
    {
        if (!int.TryParse(value.Trim(), out parsedValue) || parsedValue < minValue || parsedValue > maxValue)
        {
            errorMessage = $"{label} musí být celé číslo v rozsahu {minValue} až {maxValue}.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
