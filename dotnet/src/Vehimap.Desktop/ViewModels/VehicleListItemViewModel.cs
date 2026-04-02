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
    string StatusSummary)
{
    public string AccessibleLabel =>
        $"{Name}, {MakeModel}, {Category}, SPZ {Plate}, stav {StateOrFallback}, {StatusSummary}";

    private string StateOrFallback => string.IsNullOrWhiteSpace(State) ? "bez stavu" : State;

    public override string ToString() => AccessibleLabel;
}
