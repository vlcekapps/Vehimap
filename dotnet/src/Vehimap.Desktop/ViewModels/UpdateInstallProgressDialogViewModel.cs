// SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class UpdateInstallProgressDialogViewModel : ObservableObject
{
    private readonly IAppLocalizer _localizer;
    private readonly AppCulturePreferences _culturePreferences;
    private readonly IAppFileSizeFormatService _fileSizeFormatService;
    private readonly IAppNumberFormatService _numberFormatService;

    public UpdateInstallProgressDialogViewModel(
        IAppLocalizer? localizer = null,
        AppCulturePreferences? culturePreferences = null,
        IAppFileSizeFormatService? fileSizeFormatService = null,
        IAppNumberFormatService? numberFormatService = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer();
        _culturePreferences = culturePreferences ?? AppLocaleDefaultsService.GetCurrentCultureDefaults().ToCulturePreferences();
        _fileSizeFormatService = fileSizeFormatService ?? new AppFileSizeFormatService();
        _numberFormatService = numberFormatService ?? new AppNumberFormatService();
        Heading = _localizer.GetString("UpdateInstall.Title");
        StatusMessage = _localizer.GetString("UpdateInstall.InitialStatus");
        CancelButtonLabel = _localizer.GetString("UpdateInstall.Cancel");
        ProgressText = FormatPercent(0);
    }

    [ObservableProperty]
    private string heading = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private double progressValue;

    [ObservableProperty]
    private string progressText = string.Empty;

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
                FormatPercentValue(ProgressValue),
                _fileSizeFormatService.FormatBytes(boundedReceived, _culturePreferences),
                _fileSizeFormatService.FormatBytes(progress.TotalBytes.Value, _culturePreferences));
        }
        else if (progress.BytesReceived > 0)
        {
            ProgressText = _localizer.Format(
                "UpdateInstall.DownloadedBytes",
                _fileSizeFormatService.FormatBytes(progress.BytesReceived, _culturePreferences));
        }
    }

    public void MarkCompleted(string message)
    {
        StatusMessage = message;
        IsIndeterminate = false;
        ProgressValue = 100;
        ProgressText = FormatPercent(100);
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

    private string FormatPercent(double value) =>
        _localizer.Format("UpdateInstall.ProgressPercent", FormatPercentValue(value));

    private string FormatPercentValue(double value) =>
        _numberFormatService.FormatDecimal((decimal)value, _culturePreferences, 0);
}
