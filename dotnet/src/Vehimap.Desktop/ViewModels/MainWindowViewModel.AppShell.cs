using Vehimap.Application;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    internal async Task<string?> PickBackupExportPathAsync(CancellationToken cancellationToken = default)
    {
        var suggestedFileName = $"vehimap-{DateTime.Today:yyyy-MM-dd}.vehimapbak";
        return await _fileDialogService
            .PickSaveFileAsync("Export dat Vehimapu", suggestedFileName, "Záloha Vehimap", "vehimapbak", cancellationToken)
            .ConfigureAwait(false);
    }

    internal async Task<string?> PickBackupImportPathAsync(CancellationToken cancellationToken = default)
    {
        return await _fileDialogService
            .PickOpenFileAsync("Import zálohy Vehimapu", "Záloha Vehimap", "vehimapbak", cancellationToken)
            .ConfigureAwait(false);
    }

    internal async Task<string> ExportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return "Export se nepodařilo připravit, protože nejsou načtená data.";
        }

        await _session.ExportBackupAsync(backupPath, cancellationToken).ConfigureAwait(false);
        ShellStatus = $"Záloha byla uložena do {backupPath}.";
        return ShellStatus;
    }

    internal async Task<string> ImportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (!_session.IsLoaded)
        {
            return "Obnovu se nepodařilo připravit, protože nejsou načtená data.";
        }

        await _session.RestoreBackupAsync(backupPath, cancellationToken).ConfigureAwait(false);
        Load(applyLaunchTabPreference: false);
        ShellStatus = $"Data byla obnovena ze zálohy {backupPath}.";
        return ShellStatus;
    }

    internal DesktopSupportedSettingsSnapshot GetSupportedSettingsSnapshot() =>
        _session.ReadSupportedSettings();

    internal async Task SaveSupportedSettingsAsync(DesktopSupportedSettingsSnapshot snapshot)
    {
        if (!_session.IsLoaded)
        {
            return;
        }

        await _session.ApplySupportedSettingsAsync(snapshot).ConfigureAwait(false);
        Load(SelectedVehicle?.Id, SelectedVehicleTabIndex, applyLaunchTabPreference: false);
        ShellStatus = "Nastavení byla uložena a přehledy byly přepočítány.";
    }

    internal AboutDialogViewModel BuildAboutDialogModel()
    {
        var appInfo = _session.GetAppInfo();
        var dataMode = _dataRoot?.IsPortable == true ? "Portable data vedle aplikace" : "Systémová datová složka";
        var dataPath = _dataRoot?.DataPath ?? "Datová složka zatím nebyla načtena";

        return new AboutDialogViewModel(
            appInfo.ApplicationName,
            appInfo.AppVersion,
            appInfo.FileVersion,
            appInfo.RuntimeMode,
            dataPath,
            dataMode,
            appInfo.PlatformDescription,
            appInfo.FrameworkDescription,
            appInfo.ApplicationPath,
            appInfo.ReleaseNotesUrl);
    }

    internal Task OpenExternalAsync(string path, CancellationToken cancellationToken = default) =>
        _fileLauncher.OpenAsync(path, cancellationToken);

    internal Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default) =>
        _session.CheckForUpdatesAsync(cancellationToken);

    internal Task<UpdateInstallResult> PrepareUpdateInstallAsync(UpdateCheckResult result, CancellationToken cancellationToken = default) =>
        _session.PrepareInstallAsync(result, cancellationToken);
}
