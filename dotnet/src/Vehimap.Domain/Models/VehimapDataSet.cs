namespace Vehimap.Domain.Models;

public sealed class VehimapDataSet
{
    public VehimapSettings Settings { get; init; } = new();
    public List<Vehicle> Vehicles { get; init; } = new();
    public List<VehicleHistoryEntry> HistoryEntries { get; init; } = new();
    public List<FuelEntry> FuelEntries { get; init; } = new();
    public List<VehicleRecord> Records { get; init; } = new();
    public List<VehicleMeta> VehicleMetaEntries { get; init; } = new();
    public List<VehicleReminder> Reminders { get; init; } = new();
    public List<MaintenancePlan> MaintenancePlans { get; init; } = new();
}
