using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleHistoryItemViewModel(
    string Id,
    string Date,
    string EventType,
    string Odometer,
    string Cost,
    string Note)
{
    public string AccessibleLabel =>
        DesktopLocalization.Localizer.Format("HistoryItem.AccessibleLabel", Date, EventType, Odometer, Cost, Note);

    public override string ToString() => AccessibleLabel;
}
