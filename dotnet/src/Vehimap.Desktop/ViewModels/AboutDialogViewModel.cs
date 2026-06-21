namespace Vehimap.Desktop.ViewModels;

public sealed record AboutDialogViewModel(
    string Title,
    string AppVersion,
    string FileVersion,
    string RuntimeMode,
    string DataPath,
    string DataMode,
    string PlatformDescription,
    string FrameworkDescription,
    string ApplicationPath,
    string ReleaseNotesUrl)
{
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
