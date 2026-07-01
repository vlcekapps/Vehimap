// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private readonly SemaphoreSlim _preferencePersistGate = new(1, 1);

    private void PersistPreferenceSettingsAsync(Action<VehimapSettings> updateSettings, string failurePrefix)
    {
        if (!_session.IsLoaded)
        {
            return;
        }

        _ = PersistPreferenceSettingsCoreAsync(updateSettings, failurePrefix);
    }

    private async Task PersistPreferenceSettingsCoreAsync(Action<VehimapSettings> updateSettings, string failurePrefix)
    {
        await _preferencePersistGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!_session.IsLoaded)
            {
                return;
            }

            var rollbackDataSet = CloneDataSet(_dataSet);
            updateSettings(_dataSet.Settings);

            try
            {
                await _session.PersistAsync().ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _session.RestoreDataSet(rollbackDataSet);
                ShellStatus = $"{failurePrefix}: {ex.Message}";
            }
        }
        finally
        {
            _preferencePersistGate.Release();
        }
    }
}
