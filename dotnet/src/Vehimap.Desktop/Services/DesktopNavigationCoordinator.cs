using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopNavigationCoordinator
{
    public DesktopNavigationPlan BuildForEntity(string vehicleId, string entityKind, string entityId)
    {
        return entityKind switch
        {
            "Historie" => new DesktopNavigationPlan(vehicleId, DesktopTabIndexes.History, DesktopFocusTarget.HistoryList, DesktopNavigationSelectionKind.History, entityId),
            "Tankování" => new DesktopNavigationPlan(vehicleId, DesktopTabIndexes.Fuel, DesktopFocusTarget.FuelList, DesktopNavigationSelectionKind.Fuel, entityId),
            "Doklad" => new DesktopNavigationPlan(vehicleId, DesktopTabIndexes.Record, DesktopFocusTarget.RecordList, DesktopNavigationSelectionKind.Record, entityId),
            "Údržba" => new DesktopNavigationPlan(vehicleId, DesktopTabIndexes.Maintenance, DesktopFocusTarget.MaintenanceList, DesktopNavigationSelectionKind.Maintenance, entityId),
            "Připomínka" => new DesktopNavigationPlan(vehicleId, DesktopTabIndexes.Reminder, DesktopFocusTarget.ReminderList, DesktopNavigationSelectionKind.Reminder, entityId),
            _ => new DesktopNavigationPlan(vehicleId, DesktopTabIndexes.Detail, DesktopFocusTarget.VehicleList, DesktopNavigationSelectionKind.Vehicle, entityId)
        };
    }

    public DesktopNavigationPlan BuildForTimeline(VehicleTimelineItemViewModel item)
    {
        return item.Kind switch
        {
            "history" => new DesktopNavigationPlan(item.VehicleId, DesktopTabIndexes.History, DesktopFocusTarget.HistoryList, DesktopNavigationSelectionKind.History, item.EntryId),
            "fuel" => new DesktopNavigationPlan(item.VehicleId, DesktopTabIndexes.Fuel, DesktopFocusTarget.FuelList, DesktopNavigationSelectionKind.Fuel, item.EntryId),
            "custom" => new DesktopNavigationPlan(item.VehicleId, DesktopTabIndexes.Reminder, DesktopFocusTarget.ReminderList, DesktopNavigationSelectionKind.Reminder, item.EntryId),
            "maintenance" => new DesktopNavigationPlan(item.VehicleId, DesktopTabIndexes.Maintenance, DesktopFocusTarget.MaintenanceList, DesktopNavigationSelectionKind.Maintenance, item.EntryId),
            "record" => new DesktopNavigationPlan(item.VehicleId, DesktopTabIndexes.Record, DesktopFocusTarget.RecordList, DesktopNavigationSelectionKind.Record, item.EntryId),
            _ => new DesktopNavigationPlan(item.VehicleId, DesktopTabIndexes.Detail, DesktopFocusTarget.VehicleList, DesktopNavigationSelectionKind.Vehicle, item.EntryId)
        };
    }
}

internal sealed record DesktopNavigationPlan(
    string VehicleId,
    int TabIndex,
    DesktopFocusTarget FocusTarget,
    DesktopNavigationSelectionKind SelectionKind,
    string? EntityId);

internal enum DesktopNavigationSelectionKind
{
    Vehicle = 0,
    History = 1,
    Fuel = 2,
    Reminder = 3,
    Maintenance = 4,
    Record = 5
}
