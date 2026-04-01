namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleListItemViewModel(
    string Id,
    string Name,
    string Category,
    string Plate,
    string MakeModel,
    string VehicleNote,
    string NextTk,
    string GreenCardTo,
    string State,
    string Powertrain,
    string StatusSummary);
