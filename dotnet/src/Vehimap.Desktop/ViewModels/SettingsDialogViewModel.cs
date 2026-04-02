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
    private bool showDashboardOnLaunch;

    [ObservableProperty]
    private string statusMessage = "Upravte podporované volby a potvrďte je tlačítkem Uložit.";

    public static SettingsDialogViewModel FromSnapshot(DesktopSupportedSettingsSnapshot snapshot)
    {
        return new SettingsDialogViewModel
        {
            TechnicalReminderDays = snapshot.TechnicalReminderDays.ToString(),
            GreenCardReminderDays = snapshot.GreenCardReminderDays.ToString(),
            MaintenanceReminderDays = snapshot.MaintenanceReminderDays.ToString(),
            MaintenanceReminderKm = snapshot.MaintenanceReminderKm.ToString(),
            ShowDashboardOnLaunch = snapshot.ShowDashboardOnLaunch
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

        snapshot = new DesktopSupportedSettingsSnapshot(
            technicalReminderDays,
            greenCardReminderDays,
            maintenanceReminderDays,
            maintenanceReminderKm,
            ShowDashboardOnLaunch);
        errorMessage = string.Empty;
        return true;
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
