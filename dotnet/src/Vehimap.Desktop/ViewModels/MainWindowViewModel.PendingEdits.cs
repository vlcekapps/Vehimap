// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private Func<string, Task<bool>>? _confirmPendingEditsHandler;
    private Func<string, Task<bool>>? _confirmVehicleDeleteHandler;

    internal bool HasPendingEdits =>
        VehicleDetailWorkspace.IsEditingVehicle
        || IsEditingHistory
        || IsEditingFuel
        || IsEditingReminder
        || IsEditingMaintenance
        || IsEditingRecord;

    public bool IsVehicleListLocked => HasPendingEdits;

    public bool CanUseVehicleList => !HasPendingEdits;

    public bool IsWorkspaceNavigationLocked => HasPendingEdits;

    public bool CanUseWorkspaceNavigation => !HasPendingEdits;

    public string VehicleListLockStatus =>
        HasPendingEdits
            ? LFP("PendingEdits.VehicleListLockStatus", GetPendingEditLabel())
            : string.Empty;

    public string WorkspaceNavigationLockStatus =>
        HasPendingEdits
            ? LFP("PendingEdits.WorkspaceNavigationLockStatus", GetPendingEditLabel())
            : string.Empty;

    internal Func<string, Task<bool>>? ConfirmPendingEditsHandler
    {
        get => _confirmPendingEditsHandler;
        set => _confirmPendingEditsHandler = value;
    }

    internal Func<string, Task<bool>>? ConfirmVehicleDeleteHandler
    {
        get => _confirmVehicleDeleteHandler;
        set => _confirmVehicleDeleteHandler = value;
    }

    internal string GetPendingEditLabel()
    {
        var labels = new List<string>();
        if (VehicleDetailWorkspace.IsEditingVehicle)
        {
            labels.Add(LP("PendingEdits.Label.VehicleDetail"));
        }

        if (IsEditingHistory)
        {
            labels.Add(LP("PendingEdits.Label.History"));
        }

        if (IsEditingFuel)
        {
            labels.Add(LP("PendingEdits.Label.Fuel"));
        }

        if (IsEditingReminder)
        {
            labels.Add(LP("PendingEdits.Label.Reminder"));
        }

        if (IsEditingMaintenance)
        {
            labels.Add(LP("PendingEdits.Label.Maintenance"));
        }

        if (IsEditingRecord)
        {
            labels.Add(LP("PendingEdits.Label.Record"));
        }

        return labels.Count switch
        {
            0 => string.Empty,
            1 => labels[0],
            _ => LP("PendingEdits.Label.MultipleEditors")
        };
    }

    internal DesktopFocusTarget GetPendingEditFocusTarget()
    {
        if (VehicleDetailWorkspace.IsEditingVehicle)
        {
            return DesktopFocusTarget.VehicleEditorName;
        }

        if (IsEditingHistory)
        {
            return DesktopFocusTarget.HistoryEditorDate;
        }

        if (IsEditingFuel)
        {
            return DesktopFocusTarget.FuelEditorDate;
        }

        if (IsEditingReminder)
        {
            return DesktopFocusTarget.ReminderEditorTitle;
        }

        if (IsEditingMaintenance)
        {
            return DesktopFocusTarget.MaintenanceEditorTemplate;
        }

        if (IsEditingRecord)
        {
            return DesktopFocusTarget.RecordEditorTitle;
        }

        return DesktopFocusTarget.VehicleList;
    }

    internal async Task<bool> ConfirmDiscardPendingEditsAsync(string actionDescription)
    {
        if (!HasPendingEdits)
        {
            return true;
        }

        if (_confirmPendingEditsHandler is null)
        {
            return false;
        }

        return await _confirmPendingEditsHandler(actionDescription).ConfigureAwait(true);
    }

    internal void DiscardPendingEdits(bool clearStatus = true)
    {
        CancelVehicleEditCore(clearStatus);
        CancelHistoryEditCore(clearStatus);
        CancelFuelEditCore(clearStatus);
        CancelReminderEditCore(clearStatus);
        CancelMaintenanceEditCore(clearStatus);
        CancelRecordEditCore(clearStatus);
    }

    internal void NotifyPendingEditStateChanged()
    {
        OnPropertyChanged(nameof(HasPendingEdits));
        OnPropertyChanged(nameof(IsVehicleListLocked));
        OnPropertyChanged(nameof(CanUseVehicleList));
        OnPropertyChanged(nameof(VehicleListLockStatus));
        OnPropertyChanged(nameof(IsWorkspaceNavigationLocked));
        OnPropertyChanged(nameof(CanUseWorkspaceNavigation));
        OnPropertyChanged(nameof(WorkspaceNavigationLockStatus));
        OnPropertyChanged(nameof(CanOpenReminderWindow));
        OnPropertyChanged(nameof(CanOpenRecordWindow));
        OnPropertyChanged(nameof(CanOpenHistoryWindow));
        OnPropertyChanged(nameof(CanOpenFuelWindow));
        OnPropertyChanged(nameof(CanOpenMaintenanceWindow));
        OnPropertyChanged(nameof(CanOpenVehicleDetailWindow));
        OnPropertyChanged(nameof(CanCreateVehicle));
        OnPropertyChanged(nameof(CanEditSelectedVehicle));
        OnPropertyChanged(nameof(CanDeleteSelectedVehicle));
        OnPropertyChanged(nameof(CanOpenSelectedVehicleCosts));
        OnPropertyChanged(nameof(CanOpenSelectedVehicleServiceBook));
        OnPropertyChanged(nameof(CanExportSelectedVehiclePackage));
        OnPropertyChanged(nameof(CanImportVehiclePackage));
        OnPropertyChanged(nameof(CanUseDataActions));
        OnPropertyChanged(nameof(CanOpenDataFolder));
        OnPropertyChanged(nameof(CanCheckDataStoreHealth));
        OnPropertyChanged(nameof(CanCreateAutomaticBackupNow));
        OnPropertyChanged(nameof(CanOpenAutomaticBackupFolder));
        OnPropertyChanged(nameof(CanOpenPreMigrationBackupFolder));
        OnPropertyChanged(nameof(CanOpenVehicleStarterBundle));
        OnPropertyChanged(nameof(CanOpenMaintenanceRecommendations));
        OnPropertyChanged(nameof(CanEditSelectedDashboardVehicle));
        OnPropertyChanged(nameof(CanOpenDashboardCostOverview));
        OnPropertyChanged(nameof(CanOpenSelectedDashboardVehicleHistory));
        OnPropertyChanged(nameof(CanOpenSelectedDashboardVehicleCosts));
        OnPropertyChanged(nameof(CanClearVehicleFilters));
        NotifyQuickActionAvailabilityChanged();
        VehicleDetailWorkspace.NotifyVehicleRelatedWorkspaceStateChanged();
        MaintenanceWorkspace.NotifyMaintenanceRecommendationStateChanged();
        DashboardWorkspace.NotifyDashboardActionStateChanged();

        ClearVehicleFiltersCommand.NotifyCanExecuteChanged();
        CreateVehicleCommand.NotifyCanExecuteChanged();
        EditSelectedVehicleCommand.NotifyCanExecuteChanged();
        DeleteSelectedVehicleCommand.NotifyCanExecuteChanged();
        OpenSelectedVehicleCostsCommand.NotifyCanExecuteChanged();
        CreateHistoryCommand.NotifyCanExecuteChanged();
        EditSelectedHistoryCommand.NotifyCanExecuteChanged();
        DeleteSelectedHistoryCommand.NotifyCanExecuteChanged();
        CreateFuelCommand.NotifyCanExecuteChanged();
        EditSelectedFuelCommand.NotifyCanExecuteChanged();
        DeleteSelectedFuelCommand.NotifyCanExecuteChanged();
        CreateReminderCommand.NotifyCanExecuteChanged();
        EditSelectedReminderCommand.NotifyCanExecuteChanged();
        DeleteSelectedReminderCommand.NotifyCanExecuteChanged();
        AdvanceSelectedReminderCommand.NotifyCanExecuteChanged();
        CreateMaintenanceCommand.NotifyCanExecuteChanged();
        EditSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        DeleteSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        CompleteSelectedMaintenanceCommand.NotifyCanExecuteChanged();
        MaintenanceWorkspace.NotifyMaintenanceCompletionStateChanged();
        CreateRecordCommand.NotifyCanExecuteChanged();
        EditSelectedRecordCommand.NotifyCanExecuteChanged();
        DeleteSelectedRecordCommand.NotifyCanExecuteChanged();
        MoveSelectedRecordToManagedCommand.NotifyCanExecuteChanged();
        OpenSelectedDashboardVehicleCommand.NotifyCanExecuteChanged();
        EditSelectedDashboardVehicleCommand.NotifyCanExecuteChanged();
        OpenDashboardCostOverviewCommand.NotifyCanExecuteChanged();
        OpenSelectedDashboardVehicleHistoryCommand.NotifyCanExecuteChanged();
        OpenSelectedDashboardVehicleCostsCommand.NotifyCanExecuteChanged();
    }

    internal bool BlockWorkspaceNavigationIfEditing()
    {
        if (!HasPendingEdits)
        {
            return false;
        }

        ShellStatus = WorkspaceNavigationLockStatus;
        RequestFocus(GetPendingEditFocusTarget());
        return true;
    }

    internal bool BlockDataActionIfEditing(string actionDescription)
    {
        if (!HasPendingEdits)
        {
            return false;
        }

        var action = string.IsNullOrWhiteSpace(actionDescription)
            ? LP("PendingEdits.Action.Continue")
            : actionDescription.Trim();
        ShellStatus = LFP("PendingEdits.BlockDataAction", GetPendingEditLabel(), action);
        RequestFocus(GetPendingEditFocusTarget());
        return true;
    }

    private static string LP(string key) =>
        DesktopLocalization.Localizer.GetString(key);

    private static string LFP(string key, params object?[] args) =>
        DesktopLocalization.Localizer.Format(key, args);
}
