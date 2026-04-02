using Vehimap.Application;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
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
