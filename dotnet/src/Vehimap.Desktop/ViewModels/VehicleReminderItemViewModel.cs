using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleReminderItemViewModel(
    string Id,
    string Title,
    string DueDate,
    string Status,
    string RepeatMode,
    string Note)
{
    public string AccessibleLabel =>
        DesktopLocalization.Localizer.Format("ReminderItem.AccessibleLabel", Title, DueDate, Status, RepeatMode, Note);

    public override string ToString() => AccessibleLabel;
}
