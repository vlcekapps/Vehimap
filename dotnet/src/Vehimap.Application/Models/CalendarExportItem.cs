namespace Vehimap.Application.Models;

public sealed record CalendarExportItem(
    string Kind,
    string KindLabel,
    string VehicleId,
    string VehicleName,
    DateOnly Date,
    string Summary,
    string Description,
    string Uid);
