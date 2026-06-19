namespace Vehimap.Desktop.ViewModels;

public sealed record MaintenanceCompletionDialogResult(
    string CompletedDate,
    string CompletedOdometer,
    bool AddHistory,
    string HistoryCost,
    string HistoryNote);
