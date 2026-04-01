namespace Vehimap.Desktop.ViewModels;

public sealed record VehicleReminderItemViewModel(
    string Title,
    string DueDate,
    string Status,
    string RepeatMode,
    string Note);
