namespace Vehimap.Application.Models;

public sealed record ServiceBookSummary(
    string VehicleId,
    string VehicleName,
    string VehicleCategory,
    string VehicleMakeModel,
    string VehiclePlate,
    string CurrentOdometer,
    decimal TotalHistoryCost,
    string Status,
    IReadOnlyList<ServiceBookHistoryEntry> HistoryEntries,
    IReadOnlyList<ServiceBookMaintenanceEntry> MaintenancePlans,
    IReadOnlyList<ServiceBookRecordEntry> Records);

public sealed record ServiceBookHistoryEntry(
    string Id,
    string DateText,
    DateOnly? Date,
    string EventType,
    string Odometer,
    string Cost,
    decimal? ParsedCost,
    string Note);

public sealed record ServiceBookMaintenanceEntry(
    string Id,
    string Title,
    string Interval,
    string LastService,
    string Status,
    bool IsActive,
    string Note);

public sealed record ServiceBookRecordEntry(
    string Id,
    string RecordType,
    string Title,
    string Provider,
    string Validity,
    string Price,
    string AttachmentMode,
    string StoredPath,
    string Note);
