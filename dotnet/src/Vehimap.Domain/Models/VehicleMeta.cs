namespace Vehimap.Domain.Models;

public sealed record VehicleMeta(
    string VehicleId,
    string State,
    string Tags,
    string Powertrain,
    string ClimateProfile,
    string TimingDrive,
    string Transmission);
