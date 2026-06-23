using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class AboutDialogViewModel : ObservableObject
{
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
        string releaseNotesUrl)
    {
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
    }

    public string Title { get; }

    public string AppVersion { get; }

    public string FileVersion { get; }

    public string RuntimeMode { get; }

    public string DataPath { get; }

    public string DataMode { get; }

    public string PlatformDescription { get; }

    public string FrameworkDescription { get; }

    public string ApplicationPath { get; }

    public string ReleaseNotesUrl { get; }

    [ObservableProperty]
    private string statusMessage = "Informace jsou připravené ke zkopírování.";

    public string ClipboardText => string.Join(
        Environment.NewLine,
        new[]
        {
            "Vehimap - O programu",
            $"Název: {Title}",
            $"Verze aplikace: {AppVersion}",
            $"Souborová verze: {FileVersion}",
            $"Režim spuštění: {RuntimeMode}",
            $"Datová složka: {DataPath}",
            $"Režim dat: {DataMode}",
            $"Platforma: {PlatformDescription}",
            $".NET runtime: {FrameworkDescription}",
            $"Soubor aplikace: {ApplicationPath}",
            $"Release poznámky: {ReleaseNotesUrl}"
        });
}
