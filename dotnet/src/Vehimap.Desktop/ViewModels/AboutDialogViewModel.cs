// SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class AboutDialogViewModel : ObservableObject
{
    private const string AuthorText = "by Vlcek apps";
    public const string AuthorSupportUrl = "https://obchod.pvlcek.cz/produkt/kupte-autorovi-kavu-podpora-tvorby-podcastu-a-karosy/";

    public AboutDialogViewModel(
        string title,
        string appVersion,
        string fileVersion,
        string runtimeMode,
        string dataPath,
        string dataMode,
        string platformDescription,
        string frameworkDescription,
        string applicationPath,
        string releaseNotesUrl,
        string releaseChannel = "stable",
        IAppLocalizer? localizer = null)
    {
        _localizer = localizer ?? new ResourceAppLocalizer();
        Title = title;
        AppVersion = appVersion;
        FileVersion = fileVersion;
        RuntimeMode = runtimeMode;
        DataPath = dataPath;
        DataMode = dataMode;
        PlatformDescription = platformDescription;
        FrameworkDescription = frameworkDescription;
        ApplicationPath = applicationPath;
        ReleaseNotesUrl = releaseNotesUrl;
        ReleaseChannel = string.IsNullOrWhiteSpace(releaseChannel) ? "stable" : releaseChannel.Trim();
        StatusMessage = _localizer.GetString("About.Status.Basic");
    }

    public string Title { get; }

    public string Author => AuthorText;

    private readonly IAppLocalizer _localizer;

    public string ThankAuthorLabel => _localizer.GetString("MainMenu.App.ThankAuthor");

    public string ThankAuthorHelpText => _localizer.GetString("About.ThankAuthorHelpText");

    public string AppVersion { get; }

    public string FileVersion { get; }

    public string ReleaseChannel { get; }

    public string DisplayVersion =>
        string.Equals(ReleaseChannel, "stable", StringComparison.OrdinalIgnoreCase)
            ? AppVersion
            : $"{AppVersion} ({ReleaseChannel})";

    public string RuntimeMode { get; }

    public string DataPath { get; }

    public string DataMode { get; }

    public string PlatformDescription { get; }

    public string FrameworkDescription { get; }

    public string ApplicationPath { get; }

    public string ReleaseNotesUrl { get; }

    [ObservableProperty]
    private bool isDiagnosticsVisible;

    public string ToggleDiagnosticsLabel => IsDiagnosticsVisible
        ? _localizer.GetString("About.Toggle.HideDiagnostics")
        : _localizer.GetString("About.Toggle.ShowDiagnostics");

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public string DiagnosticText => string.Join(
        Environment.NewLine,
        new[]
        {
            _localizer.GetString("About.Diagnostics.Header"),
            _localizer.GetString("About.DiagnosticsName"),
            _localizer.Format("About.Diagnostics.Title", Title),
            _localizer.Format("About.Diagnostics.Author", Author),
            _localizer.Format("About.Diagnostics.AppVersion", AppVersion),
            _localizer.Format("About.Diagnostics.FileVersion", FileVersion),
            _localizer.Format("About.Diagnostics.ReleaseChannel", ReleaseChannel),
            _localizer.Format("About.Diagnostics.RuntimeMode", RuntimeMode),
            _localizer.Format("About.Diagnostics.DataPath", DataPath),
            _localizer.Format("About.Diagnostics.DataMode", DataMode),
            _localizer.Format("About.Diagnostics.Platform", PlatformDescription),
            _localizer.Format("About.Diagnostics.Framework", FrameworkDescription),
            _localizer.Format("About.Diagnostics.ApplicationPath", ApplicationPath),
            _localizer.Format("About.Diagnostics.ReleaseNotes", ReleaseNotesUrl),
            _localizer.Format("About.Diagnostics.AuthorSupport", AuthorSupportUrl)
        });

    public string ClipboardText => DiagnosticText;

    public void ToggleDiagnostics()
    {
        IsDiagnosticsVisible = !IsDiagnosticsVisible;
        StatusMessage = IsDiagnosticsVisible
            ? _localizer.GetString("About.Status.DiagnosticsVisible")
            : _localizer.GetString("About.Status.DiagnosticsHidden");
    }

    partial void OnIsDiagnosticsVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(ToggleDiagnosticsLabel));
    }
}
