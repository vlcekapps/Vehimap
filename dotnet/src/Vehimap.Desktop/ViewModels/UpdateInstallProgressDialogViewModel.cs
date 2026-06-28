using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class UpdateInstallProgressDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string heading = "Stahování aktualizace";

    [ObservableProperty]
    private string statusMessage = "Připravuji stahování aktualizace.";

    [ObservableProperty]
    private double progressValue;

    [ObservableProperty]
    private string progressText = "0 %";

    [ObservableProperty]
    private bool isIndeterminate = true;

    [ObservableProperty]
    private bool canCancel = true;

    [ObservableProperty]
    private string cancelButtonLabel = "Zrušit";

    public string HelpText =>
        "Aktualizace se stahuje a ověřuje. Tlačítkem Zrušit můžete stahování přerušit.";

    public void ApplyProgress(UpdateInstallProgress progress)
    {
        StatusMessage = string.IsNullOrWhiteSpace(progress.Message)
            ? "Stahuji aktualizaci."
            : progress.Message;
        IsIndeterminate = progress.IsIndeterminate || progress.TotalBytes is null or <= 0;

        if (progress.TotalBytes is > 0)
        {
            var boundedReceived = Math.Clamp(progress.BytesReceived, 0, progress.TotalBytes.Value);
            ProgressValue = progress.TotalBytes.Value == 0
                ? 0
                : boundedReceived * 100d / progress.TotalBytes.Value;
            ProgressText = $"{ProgressValue:0} % ({FormatBytes(boundedReceived)} z {FormatBytes(progress.TotalBytes.Value)})";
        }
        else if (progress.BytesReceived > 0)
        {
            ProgressText = $"Staženo {FormatBytes(progress.BytesReceived)}.";
        }
    }

    public void MarkCompleted(string message)
    {
        StatusMessage = message;
        IsIndeterminate = false;
        ProgressValue = 100;
        ProgressText = "100 %";
        CanCancel = false;
        CancelButtonLabel = "Zavřít";
    }

    public void MarkCancelled()
    {
        StatusMessage = "Stahování aktualizace bylo zrušeno.";
        IsIndeterminate = false;
        CanCancel = false;
        CancelButtonLabel = "Zavřít";
    }

    private static string FormatBytes(long sizeBytes)
    {
        if (sizeBytes < 1024)
        {
            return $"{sizeBytes} B";
        }

        var sizeKb = sizeBytes / 1024d;
        if (sizeKb < 1024)
        {
            return $"{sizeKb:0.0} KB";
        }

        var sizeMb = sizeKb / 1024d;
        if (sizeMb < 1024)
        {
            return $"{sizeMb:0.0} MB";
        }

        var sizeGb = sizeMb / 1024d;
        return $"{sizeGb:0.00} GB";
    }
}
