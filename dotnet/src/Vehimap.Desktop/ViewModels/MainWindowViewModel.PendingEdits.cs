namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private Func<string, Task<bool>>? _confirmPendingEditsHandler;
    private Func<string, Task<bool>>? _confirmVehicleDeleteHandler;

    internal bool HasPendingEdits =>
        IsEditingVehicle
        || IsEditingHistory
        || IsEditingFuel
        || IsEditingReminder
        || IsEditingMaintenance
        || IsEditingRecord;

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
        if (IsEditingVehicle)
        {
            labels.Add("detail vozidla");
        }

        if (IsEditingHistory)
        {
            labels.Add("historie");
        }

        if (IsEditingFuel)
        {
            labels.Add("tankování");
        }

        if (IsEditingReminder)
        {
            labels.Add("připomínky");
        }

        if (IsEditingMaintenance)
        {
            labels.Add("údržba");
        }

        if (IsEditingRecord)
        {
            labels.Add("doklady");
        }

        return labels.Count switch
        {
            0 => string.Empty,
            1 => labels[0],
            _ => "více otevřených editorů"
        };
    }

    internal DesktopFocusTarget GetPendingEditFocusTarget()
    {
        if (IsEditingVehicle)
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
            return DesktopFocusTarget.MaintenanceEditorTitle;
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
        OnPropertyChanged(nameof(CanCreateVehicle));
        OnPropertyChanged(nameof(CanEditSelectedVehicle));
        OnPropertyChanged(nameof(CanDeleteSelectedVehicle));
        OnPropertyChanged(nameof(CanOpenSelectedVehicleCosts));
        OnPropertyChanged(nameof(CanOpenVehicleStarterBundle));

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
        CreateRecordCommand.NotifyCanExecuteChanged();
        EditSelectedRecordCommand.NotifyCanExecuteChanged();
        DeleteSelectedRecordCommand.NotifyCanExecuteChanged();
        MoveSelectedRecordToManagedCommand.NotifyCanExecuteChanged();
    }
}
