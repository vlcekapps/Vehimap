// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class UpdateDialogViewModel : ObservableObject
{
    private readonly IAppLocalizer _localizer;
    private readonly IAppDateFormatService _dateFormatService;
    private readonly IAppFileSizeFormatService _fileSizeFormatService;
    private readonly AppCulturePreferences _culturePreferences;

    public UpdateDialogViewModel(
        UpdateCheckResult result,
        IAppLocalizer? localizer = null,
        AppCulturePreferences? culturePreferences = null,
        IAppDateFormatService? dateFormatService = null,
        IAppFileSizeFormatService? fileSizeFormatService = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer();
        _dateFormatService = dateFormatService ?? new AppDateFormatService();
        _fileSizeFormatService = fileSizeFormatService ?? new AppFileSizeFormatService();
        _culturePreferences = culturePreferences ?? new AppCulturePreferences(CultureInfo.CurrentCulture.Name);
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
        PrimaryActionHelpText = result.IsUpdateAvailable
            ? result.CanInstallAutomatically
                ? _localizer.GetString("UpdateCheck.Primary.InstallHelp")
                : !string.IsNullOrWhiteSpace(result.NotesUrl)
                    ? _localizer.GetString("UpdateCheck.Primary.OpenReleaseHelp")
                    : _localizer.GetString("UpdateCheck.Primary.DownloadAssetHelp")
            : _localizer.GetString("UpdateCheck.CloseName");
        AssetActionHelpText = _localizer.GetString("UpdateCheck.AssetActionHelp");
    }

    public UpdateCheckResult Result { get; }

    public string Heading { get; }

    public string Summary { get; }

    public string Details { get; }

    public string ClipboardText { get; }

    public string PrimaryActionLabel { get; }

    public string PrimaryActionHelpText { get; }

    public string AssetActionHelpText { get; }

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
            lines.Add(_localizer.Format("UpdateCheck.Details.PublishedAt", FormatPublishedAt(result.PublishedAt)));
        }

        if (result.AssetSize is > 0)
        {
            lines.Add(_localizer.Format("UpdateCheck.Details.AssetSize", _fileSizeFormatService.FormatBytes(result.AssetSize.Value, _culturePreferences)));
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

    private string FormatPublishedAt(string value) =>
        _dateFormatService.TryParseDate(value, _culturePreferences, out var date)
            ? _dateFormatService.FormatDate(date, _culturePreferences)
            : value;

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

}
