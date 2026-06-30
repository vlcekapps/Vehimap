using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class UpdateInstallProgressDialogViewModel : ObservableObject
{
    private readonly IAppLocalizer _localizer;

    public UpdateInstallProgressDialogViewModel(IAppLocalizer? localizer = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer();
        Heading = _localizer.GetString("UpdateInstall.Title");
        StatusMessage = _localizer.GetString("UpdateInstall.InitialStatus");
        CancelButtonLabel = _localizer.GetString("UpdateInstall.Cancel");
    }

    [ObservableProperty]
    private string heading = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private double progressValue;

    [ObservableProperty]
    private string progressText = "0 %";

    [ObservableProperty]
    private bool isIndeterminate = true;

    [ObservableProperty]
    private bool canCancel = true;

    [ObservableProperty]
    private string cancelButtonLabel = string.Empty;

    public string HelpText =>
        _localizer.GetString("UpdateInstall.HelpText");

    public void ApplyProgress(UpdateInstallProgress progress)
    {
        StatusMessage = string.IsNullOrWhiteSpace(progress.Message)
            ? _localizer.GetString("UpdateInstall.Downloading")
            : progress.Message;
        IsIndeterminate = progress.IsIndeterminate || progress.TotalBytes is null or <= 0;

        if (progress.TotalBytes is > 0)
        {
            var boundedReceived = Math.Clamp(progress.BytesReceived, 0, progress.TotalBytes.Value);
            ProgressValue = progress.TotalBytes.Value == 0
                ? 0
                : boundedReceived * 100d / progress.TotalBytes.Value;
            ProgressText = _localizer.Format(
                "UpdateInstall.ProgressWithBytes",
                ProgressValue.ToString("0"),
                FormatBytes(boundedReceived),
                FormatBytes(progress.TotalBytes.Value));
        }
        else if (progress.BytesReceived > 0)
        {
            ProgressText = _localizer.Format("UpdateInstall.DownloadedBytes", FormatBytes(progress.BytesReceived));
        }
    }

    public void MarkCompleted(string message)
    {
        StatusMessage = message;
        IsIndeterminate = false;
        ProgressValue = 100;
        ProgressText = "100 %";
        CanCancel = false;
        CancelButtonLabel = _localizer.GetString("Common.Close");
    }

    public void MarkCancelled()
    {
        StatusMessage = _localizer.GetString("UpdateInstall.CancelledResult");
        IsIndeterminate = false;
        CanCancel = false;
        CancelButtonLabel = _localizer.GetString("Common.Close");
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
