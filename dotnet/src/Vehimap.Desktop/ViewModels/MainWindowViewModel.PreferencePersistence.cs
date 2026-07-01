// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private readonly SemaphoreSlim _preferencePersistGate = new(1, 1);
    private bool _suppressPreferencePersistence;

    private void PersistPreferenceSettingsAsync(Action<VehimapSettings> updateSettings, string failurePrefix)
    {
        if (!_session.IsLoaded || _suppressPreferencePersistence)
        {
            return;
        }

        _ = PersistPreferenceSettingsCoreAsync(updateSettings, failurePrefix);
    }

    private IDisposable SuppressPreferencePersistence()
    {
        var previous = _suppressPreferencePersistence;
        _suppressPreferencePersistence = true;
        return new PreferencePersistenceSuppressionScope(this, previous);
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

    private sealed class PreferencePersistenceSuppressionScope(
        MainWindowViewModel owner,
        bool previousValue) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            owner._suppressPreferencePersistence = previousValue;
            _disposed = true;
        }
    }
}
