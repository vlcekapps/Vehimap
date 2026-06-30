using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class UpdateDialogViewModel : ObservableObject
{
    private readonly IAppLocalizer _localizer;

    public UpdateDialogViewModel(UpdateCheckResult result, IAppLocalizer? localizer = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer();
        Result = result;
        Heading = result.FailureReason is not null
            ? _localizer.GetString("UpdateCheck.Heading.Failed")
            : result.IsUpdateAvailable
                ? _localizer.GetString("UpdateCheck.Heading.UpdateAvailable")
                : _localizer.GetString("UpdateCheck.Heading.Default");
        Summary = result.FailureReason ?? result.Message;
        Details = BuildDetails(result);
        ClipboardText = BuildClipboardText(Heading, Summary, Details);
        StatusMessage = _localizer.GetString("UpdateCheck.Status.ReadyToCopy");
        PrimaryActionLabel = result.IsUpdateAvailable
            ? result.CanInstallAutomatically
                ? _localizer.GetString("UpdateCheck.Primary.Install")
                : !string.IsNullOrWhiteSpace(result.NotesUrl)
                    ? _localizer.GetString("UpdateCheck.Primary.OpenRelease")
                    : _localizer.GetString("UpdateCheck.Primary.DownloadAsset")
            : _localizer.GetString("Common.Close");
    }

    public UpdateCheckResult Result { get; }

    public string Heading { get; }

    public string Summary { get; }

    public string Details { get; }

    public string ClipboardText { get; }

    public string PrimaryActionLabel { get; }

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public bool ShowPrimaryAction => Result.IsUpdateAvailable && (Result.CanInstallAutomatically || !string.IsNullOrWhiteSpace(Result.NotesUrl) || !string.IsNullOrWhiteSpace(Result.AssetUrl));

    public bool ShowSecondaryAssetAction => Result.IsUpdateAvailable && !Result.CanInstallAutomatically && !string.IsNullOrWhiteSpace(Result.AssetUrl) && !string.IsNullOrWhiteSpace(Result.NotesUrl);

    private string BuildDetails(UpdateCheckResult result)
    {
        var lines = new List<string>
        {
            _localizer.Format("UpdateCheck.Details.CurrentVersion", result.CurrentVersion),
            _localizer.Format("UpdateCheck.Details.LatestVersion", result.LatestVersion)
        };

        if (!string.IsNullOrWhiteSpace(result.PublishedAt))
        {
            lines.Add(_localizer.Format("UpdateCheck.Details.PublishedAt", result.PublishedAt));
        }

        if (result.AssetSize is > 0)
        {
            lines.Add(_localizer.Format("UpdateCheck.Details.AssetSize", FormatBytes(result.AssetSize.Value)));
        }

        if (result.IsUpdateAvailable)
        {
            lines.Add(result.CanInstallAutomatically
                ? _localizer.GetString("UpdateCheck.Details.AutomaticInstallAvailable")
                : _localizer.Format("UpdateCheck.Details.AutomaticInstallUnavailable", BuildManualInstallReason(result)));
        }

        if (!string.IsNullOrWhiteSpace(result.NotesUrl))
        {
            lines.Add(_localizer.Format("UpdateCheck.Details.ReleaseNotes", result.NotesUrl));
        }

        if (result.IsUpdateAvailable && !string.IsNullOrWhiteSpace(result.AssetUrl))
        {
            lines.Add(_localizer.Format("UpdateCheck.Details.AssetUrl", result.AssetUrl));
        }

        if (result.IsUpdateAvailable && !string.IsNullOrWhiteSpace(result.Sha256))
        {
            lines.Add(_localizer.Format("UpdateCheck.Details.Sha256", result.Sha256));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private string BuildClipboardText(string heading, string summary, string details)
    {
        return string.Join(
            Environment.NewLine,
            new[]
            {
                _localizer.GetString("UpdateCheck.ClipboardTitle"),
                heading,
                summary,
                string.Empty,
                details
            });
    }

    private string BuildManualInstallReason(UpdateCheckResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.AutomaticInstallUnavailableReason))
        {
            return result.AutomaticInstallUnavailableReason;
        }

        return _localizer.GetString("UpdateCheck.ManualInstallFallback");
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
