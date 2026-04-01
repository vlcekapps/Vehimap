namespace Vehimap.Domain.Models;

public sealed record FuelEntry(
    string Id,
    string VehicleId,
    string EntryDate,
    string Odometer,
    string Liters,
    string TotalCost,
    bool FullTank,
    string FuelType,
    string Note);
