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
        if (_dataRoot is null)
        {
            return "Export se nepodařilo připravit, protože nejsou načtená data.";
        }

        await _backupService.ExportAsync(backupPath, _dataRoot, _dataSet, cancellationToken).ConfigureAwait(false);
        ShellStatus = $"Záloha byla uložena do {backupPath}.";
        return ShellStatus;
    }

    internal async Task<string> ImportBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        if (_dataRoot is null)
        {
            return "Obnovu se nepodařilo připravit, protože nejsou načtená data.";
        }

        var bundle = await _backupService.ImportAsync(backupPath, cancellationToken).ConfigureAwait(false);
        await _backupService.RestoreAsync(_dataRoot, bundle, cancellationToken).ConfigureAwait(false);
        Load(applyLaunchTabPreference: false);
        ShellStatus = $"Data byla obnovena ze zálohy {backupPath}.";
        return ShellStatus;
    }

    internal DesktopSupportedSettingsSnapshot GetSupportedSettingsSnapshot() =>
        _supportedSettingsService.Read(_dataSet.Settings);

    internal async Task SaveSupportedSettingsAsync(DesktopSupportedSettingsSnapshot snapshot)
    {
        if (_dataRoot is null)
        {
            return;
        }

        _supportedSettingsService.Apply(_dataSet.Settings, snapshot);
        await _legacyDataStore.SaveAsync(_dataRoot, _dataSet).ConfigureAwait(false);
        Load(SelectedVehicle?.Id, SelectedVehicleTabIndex, applyLaunchTabPreference: false);
        ShellStatus = "Nastavení byla uložena a přehledy byly přepočítány.";
    }

    internal AboutDialogViewModel BuildAboutDialogModel()
    {
        var appInfo = _appBuildInfoProvider.GetCurrent();
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
        _updateService.CheckForUpdatesAsync(_appBuildInfoProvider.GetCurrent().AppVersion, cancellationToken);

    internal Task<UpdateInstallResult> PrepareUpdateInstallAsync(UpdateCheckResult result, CancellationToken cancellationToken = default) =>
        _updateService.PrepareInstallAsync(result, cancellationToken);
}
