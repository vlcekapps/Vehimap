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
    string ReleaseNotesUrl);
