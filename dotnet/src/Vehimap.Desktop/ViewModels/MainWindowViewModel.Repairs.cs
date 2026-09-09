// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    internal event Action<string>? RepairsRequested;
    private bool _isRepairsWindowOpen;
    internal bool IsRepairsWindowOpen
    {
        get => _isRepairsWindowOpen;
        set
        {
            if (_isRepairsWindowOpen == value) return;
            _isRepairsWindowOpen = value;
            NotifyPendingEditStateChanged();
        }
    }

    internal RepairsWindowViewModel? BuildRepairsModel()
    {
        if (SelectedVehicle is null || HasPendingEdits) return null;
        var vehicleId = SelectedVehicle.Id;
        return new RepairsWindowViewModel(vehicleId, SelectedVehicle.Name, () => _dataSet,
            async change =>
            {
                var rollback = CloneDataSet(_dataSet);
                try
                {
                    new VehicleRepairService(DesktopLocalization.LiveLocalizer).Apply(_dataSet, change, DateTimeOffset.UtcNow);
                    string? error = null;
                    var success = await PersistDataAndRestoreSelectionAsync(vehicleId, SelectedVehicleTabIndex,
                        rollbackDataSet: rollback, setFailureStatus: text => error = text);
                    return success ? null : error ?? LO("Repairs.Error.Save");
                }
                catch (RepairValidationException ex)
                {
                    _session.RestoreDataSet(rollback);
                    return LO(ex.ResourceKey);
                }
                catch (Exception)
                {
                    _session.RestoreDataSet(rollback);
                    return LO("Repairs.Error.Save");
                }
            }, CurrentCulturePreferences, CurrentUnitPreferences);
    }
}
