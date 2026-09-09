// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class RepairEditorViewModel : ObservableObject
{
    private readonly RepairAction _action;
    private readonly string _vehicleId;
    private readonly VehicleRepair? _repair;
    private readonly AppCulturePreferences _culture;
    private readonly AppUnitPreferences _units;
    private readonly Func<RepairChange, Task<string?>> _save;
    private readonly AppDateFormatService _dates = new();
    private readonly AppNumberFormatService _numbers = new();
    public event Action<DesktopFocusTarget>? FocusRequested;
    public bool IsEditing { get; private set; } = true;
    public string? SavedRepairId { get; set; }
    public bool IsDefinition => _action is RepairAction.Create or RepairAction.Edit;
    public bool IsReschedule => _action == RepairAction.Reschedule;
    public bool HasPlannedDate => IsDefinition || IsReschedule;
    public bool IsCompletion => _action == RepairAction.Complete;
    public bool HasReason => !IsDefinition || _action == RepairAction.Edit;
    public bool NeedsNewHistory => IsCompletion && string.IsNullOrEmpty(SelectedHistory?.Value);
    public bool RequiresReason => NeedsNewHistory || _action == RepairAction.CannotRepair;
    public string ReasonLabel => L(IsCompletion ? "Repairs.Work" : "Repairs.Reason");
    public string Heading => L(_action switch
    {
        RepairAction.Create => "Repairs.Action.Create",
        RepairAction.Edit => "Repairs.Action.Edit",
        RepairAction.Reschedule => "Repairs.Action.Reschedule",
        RepairAction.Complete => "Repairs.Action.Complete",
        _ => "Repairs.Action.CannotRepair"
    });
    public string FirstControlName => IsDefinition ? "RepairTitleBox" : IsReschedule ? "RepairPlannedDateBox" : IsCompletion ? "RepairHistoryBox" : "RepairReasonBox";
    public string DateExample => _dates.FormatDate(new DateOnly(2026, 9, 30), _culture);
    public string OdometerLabel => DesktopLocalization.LiveLocalizer.Format("Repairs.Odometer", new AppUnitFormatService().GetDistanceUnitLabel(_units));
    public IReadOnlyList<LocalizedOptionViewModel> HistoryOptions { get; }

    [ObservableProperty] private string repairTitle = "";
    [ObservableProperty] private string description = "";
    [ObservableProperty] private string reportedDate = "";
    [ObservableProperty] private string plannedDate = "";
    [ObservableProperty] private string reminderDays = "7";
    [ObservableProperty] private string reason = "";
    [ObservableProperty] private string completedDate = "";
    [ObservableProperty] private string odometer = "";
    [ObservableProperty] private string cost = "";
    [ObservableProperty] private string status = "";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsNewHistory))]
    [NotifyPropertyChangedFor(nameof(RequiresReason))]
    private LocalizedOptionViewModel selectedHistory;

    public RepairEditorViewModel(RepairAction action, string vehicleId, VehicleRepair? repair,
        IEnumerable<VehicleHistoryEntry> history, AppCulturePreferences culture, AppUnitPreferences units,
        Func<RepairChange, Task<string?>> save)
    {
        _action = action; _vehicleId = vehicleId; _repair = repair; _culture = culture; _units = units; _save = save;
        RepairTitle = repair?.Title ?? ""; Description = repair?.Description ?? "";
        ReportedDate = DisplayDate(repair?.ReportedDate ?? VehimapValueParser.FormatCanonicalEventDate(DateOnly.FromDateTime(DateTime.Today)));
        PlannedDate = DisplayDate(repair?.PlannedDate ?? "");
        ReminderDays = _numbers.FormatDecimal(repair?.ReminderDays ?? 7, _culture, 0);
        CompletedDate = _dates.FormatDate(DateOnly.FromDateTime(DateTime.Today), _culture);
        HistoryOptions = new[] { new LocalizedOptionViewModel("", L("Repairs.NewHistory")) }
            .Concat(history.Select(h => new LocalizedOptionViewModel(h.Id, $"{DisplayDate(h.EventDate)}: {h.EventType}"))).ToArray();
        selectedHistory = HistoryOptions[0];
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!IsEditing) return;
        Status = "";
        string Canonical(string value, DesktopFocusTarget target, bool optional = false)
        {
            if (optional && string.IsNullOrWhiteSpace(value)) return "";
            if (!_dates.TryParseDate(value, _culture, out var date)) throw new RepairInputException("Repairs.Error.Date", target);
            return VehimapValueParser.FormatCanonicalEventDate(date);
        }
        try
        {
            if (IsDefinition && string.IsNullOrWhiteSpace(RepairTitle)) throw new RepairInputException("Repairs.Error.Title", DesktopFocusTarget.RepairTitle);
            var reported = IsDefinition ? Canonical(ReportedDate, DesktopFocusTarget.RepairReportedDate) : _repair!.ReportedDate;
            var planned = IsDefinition || IsReschedule ? Canonical(PlannedDate, DesktopFocusTarget.RepairPlannedDate, IsDefinition) : _repair!.PlannedDate;
            if (!string.IsNullOrEmpty(planned) && VehimapValueParser.TryParseEventDate(planned, out var due)
                && VehimapValueParser.TryParseEventDate(reported, out var start) && due < start)
                throw new RepairInputException("Repairs.Error.PlannedDate", DesktopFocusTarget.RepairPlannedDate);
            var days = _repair?.ReminderDays ?? 7;
            if (IsDefinition)
            {
                if (!_numbers.TryParseDecimal(ReminderDays, _culture, out var parsedDays) || parsedDays < 0 || parsedDays > 3650 || decimal.Truncate(parsedDays) != parsedDays)
                    throw new RepairInputException("Repairs.Error.Days", DesktopFocusTarget.RepairReminderDays);
                days = (int)parsedDays;
            }
            var completed = NeedsNewHistory ? Canonical(CompletedDate, DesktopFocusTarget.RepairCompletedDate) : "";
            if (NeedsNewHistory && VehimapValueParser.TryParseEventDate(completed, out var end)
                && VehimapValueParser.TryParseEventDate(reported, out var begin) && end < begin)
                throw new RepairInputException("Repairs.Error.Date", DesktopFocusTarget.RepairCompletedDate);
            if ((NeedsNewHistory || _action == RepairAction.CannotRepair) && string.IsNullOrWhiteSpace(Reason))
                throw new RepairInputException(NeedsNewHistory ? "Repairs.Error.Work" : "Repairs.Error.Reason", DesktopFocusTarget.RepairReason);
            var km = "";
            var amount = "";
            if (NeedsNewHistory && !string.IsNullOrWhiteSpace(Odometer))
            {
                if (!_numbers.TryParseDecimal(Odometer, _culture, out var value) || !AppUnitFormatService.TryConvertWholeKilometers(value, _units, out var canonicalKm))
                    throw new RepairInputException("Repairs.Error.Odometer", DesktopFocusTarget.RepairOdometer);
                km = canonicalKm.ToString(CultureInfo.InvariantCulture);
            }
            if (NeedsNewHistory && !string.IsNullOrWhiteSpace(Cost))
            {
                if (!_numbers.TryParseDecimal(Cost, _culture, out var value) || value < 0)
                    throw new RepairInputException("Repairs.Error.Cost", DesktopFocusTarget.RepairCost);
                amount = value.ToString("0.##", CultureInfo.InvariantCulture);
            }
            var error = await _save(new RepairChange(_action, _vehicleId, _repair?.Id, RepairTitle, Description,
                reported, planned, days, Reason, completed, km, amount, SelectedHistory?.Value ?? ""));
            if (error is not null) { Status = error; return; }
            IsEditing = false;
        }
        catch (RepairInputException ex)
        {
            Status = L(ex.Key); FocusRequested?.Invoke(ex.Target);
        }
        catch (Exception)
        {
            Status = L("Repairs.Error.Save");
        }
    }

    [RelayCommand] private void Cancel() { if (!SaveCommand.IsRunning) IsEditing = false; }
    private string DisplayDate(string value) => VehimapValueParser.TryParseEventDate(value, out var date) ? _dates.FormatDate(date, _culture) : value;
    private static string L(string key) => DesktopLocalization.LiveLocalizer.GetString(key);
    private sealed class RepairInputException(string key, DesktopFocusTarget target) : Exception
    { public string Key { get; } = key; public DesktopFocusTarget Target { get; } = target; }
}
