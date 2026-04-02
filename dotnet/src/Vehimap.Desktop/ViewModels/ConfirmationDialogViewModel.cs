namespace Vehimap.Desktop.ViewModels;

public sealed record ConfirmationDialogViewModel(
    string Title,
    string Message,
    string ConfirmLabel,
    string CancelLabel);
