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
        ? "Skrýt diagnostická data"
        : "Zobrazit diagnostická data";

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public string DiagnosticText => string.Join(
        Environment.NewLine,
        new[]
        {
            "Vehimap - O programu",
            _localizer.GetString("About.DiagnosticsName"),
            $"Název: {Title}",
            $"Autor: {Author}",
            $"Verze aplikace: {AppVersion}",
            $"Souborová verze: {FileVersion}",
            $"Kanál: {ReleaseChannel}",
            $"Režim spuštění: {RuntimeMode}",
            $"Datová složka: {DataPath}",
            $"Režim dat: {DataMode}",
            $"Platforma: {PlatformDescription}",
            $".NET runtime: {FrameworkDescription}",
            $"Soubor aplikace: {ApplicationPath}",
            $"Release poznámky: {ReleaseNotesUrl}",
            $"Poděkování autorovi: {AuthorSupportUrl}"
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
