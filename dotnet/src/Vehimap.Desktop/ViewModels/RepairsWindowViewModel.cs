// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed record RepairListItemViewModel(VehicleRepair Repair, string Label, string StateLabel, string Detail)
{
    public string Id => Repair.Id;
    public string AccessibleLabel => Label;
    public override string ToString() => Label;
}

public sealed partial class RepairsWindowViewModel : ObservableObject
{
    private readonly string _vehicleId;
    private readonly Func<VehimapDataSet> _data;
    private readonly Func<RepairChange, Task<string?>> _save;
    private readonly AppCulturePreferences _culture;
    private readonly AppUnitPreferences _units;
    public string Heading { get; }
    public event Action? CloseRequested;
    [RelayCommand] private void Close() => CloseRequested?.Invoke();
    public ObservableCollection<RepairListItemViewModel> Items { get; } = [];
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChange), nameof(SelectedDetail))]
    private RepairListItemViewModel? selectedItem;
    [ObservableProperty] private string status = "";
    public bool CanChange => SelectedItem?.Repair.State == VehicleRepair.Waiting;
    public string SelectedDetail => SelectedItem?.Detail ?? L("Repairs.Empty");

    public RepairsWindowViewModel(string vehicleId, string vehicleName, Func<VehimapDataSet> data,
        Func<RepairChange, Task<string?>> save, AppCulturePreferences culture, AppUnitPreferences units)
    {
        _vehicleId = vehicleId; _data = data; _save = save; _culture = culture; _units = units;
        Heading = DesktopLocalization.LiveLocalizer.Format("Repairs.WindowTitle", vehicleName);
        Refresh();
    }

    public void Refresh(string? selectedId = null)
    {
        selectedId ??= SelectedItem?.Id;
        Items.Clear();
        foreach (var r in _data().Repairs.Where(r => r.VehicleId == _vehicleId).OrderBy(r => r.State != VehicleRepair.Waiting)
                     .ThenBy(r => VehimapValueParser.TryParseEventDate(r.PlannedDate, out var due) ? due.DayNumber : int.MaxValue))
        {
            var state = L(r.State == VehicleRepair.Repaired ? "Repairs.State.Repaired" : r.State == VehicleRepair.Unrepairable ? "Repairs.State.Unrepairable" : string.IsNullOrEmpty(r.PlannedDate) ? "Repairs.State.Unscheduled" : "Repairs.State.Waiting");
            var label = DesktopLocalization.LiveLocalizer.Format("Repairs.Item", r.Title, state, Date(r.PlannedDate));
            var detail = DesktopLocalization.LiveLocalizer.Format("Repairs.Detail", r.Title, r.Description, Date(r.ReportedDate), Date(r.PlannedDate),
                new AppNumberFormatService().FormatDecimal(r.ReminderDays, _culture, 0), state, r.Resolution);
            foreach (var change in r.ScheduleChanges)
                detail += Environment.NewLine + DesktopLocalization.LiveLocalizer.Format("Repairs.ScheduleChange", Date(change.PreviousDate), Date(change.NewDate), change.Reason);
            Items.Add(new RepairListItemViewModel(r, label, state, detail));
        }
        SelectedItem = Items.FirstOrDefault(i => i.Id == selectedId) ?? Items.FirstOrDefault();
    }

    public RepairEditorViewModel? CreateEditor(RepairAction action)
    {
        if (action != RepairAction.Create && !CanChange) return null;
        var history = _data().HistoryEntries.Where(h => h.VehicleId == _vehicleId
            && !_data().Repairs.Any(r => r.HistoryEntryId == h.Id)
            && VehimapValueParser.TryParseEventDate(h.EventDate, out var date)
            && SelectedItem is { } selected && VehimapValueParser.TryParseEventDate(selected.Repair.ReportedDate, out var reported) && date >= reported);
        RepairEditorViewModel? editor = null;
        editor = new RepairEditorViewModel(action, _vehicleId, action == RepairAction.Create ? null : SelectedItem?.Repair, history, _culture, _units, async change =>
        {
            var error = await _save(change);
            if (error is null) editor!.SavedRepairId = change.RepairId ?? _data().Repairs.Last(r => r.VehicleId == _vehicleId).Id;
            return error;
        });
        return editor;
    }

    private string Date(string text) => VehimapValueParser.TryParseEventDate(text, out var date)
        ? new AppDateFormatService().FormatDate(date, _culture) : L("Repairs.NoDate");
    private static string L(string key) => DesktopLocalization.LiveLocalizer.GetString(key);
}
