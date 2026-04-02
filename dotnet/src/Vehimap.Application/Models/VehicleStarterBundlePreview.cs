namespace Vehimap.Application.Models;

public sealed record VehicleStarterBundlePreview(
    string VehicleId,
    string VehicleName,
    string ProfileLabel,
    IReadOnlyList<VehicleStarterBundleTemplate> Items)
{
    public int TotalMissingCount => Items.Count;

    public int MaintenanceCount => Items.Count(item => item.Section == VehicleStarterBundleSection.Maintenance);

    public int RecordCount => Items.Count(item => item.Section == VehicleStarterBundleSection.Record);

    public int ReminderCount => Items.Count(item => item.Section == VehicleStarterBundleSection.Reminder);
}
