// SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Domain.Models;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private static readonly AppNumberFormatService EditorNumberFormatService = new();
    private static readonly AppUnitFormatService EditorUnitFormatService = new();

    private string? _editingHistoryId;
    private string? _editingFuelId;
    private string? _editingMaintenanceId;

    internal string CurrentDistanceUnitLabel =>
        string.Equals(CurrentUnitPreferences.DistanceUnit, AppUnitFormatService.Miles, StringComparison.Ordinal)
            ? "mi"
            : "km";

    internal string CurrentVolumeUnitLabel =>
        CurrentUnitPreferences.VolumeUnit switch
        {
            AppUnitFormatService.UsGallons => "US gal",
            AppUnitFormatService.ImperialGallons => "imp gal",
            _ => "l"
        };

    private DesktopSupportedSettingsSnapshot CurrentSupportedSettings => _session.ReadSupportedSettings();

    private AppCulturePreferences CurrentCulturePreferences =>
        new(
            CurrentSupportedSettings.Language,
            CurrentSupportedSettings.ThousandsSeparator,
            CurrentSupportedSettings.DecimalSeparator);

    private AppUnitPreferences CurrentUnitPreferences =>
        new(
            CurrentSupportedSettings.DistanceUnit,
            CurrentSupportedSettings.VolumeUnit);

    internal string FormatCanonicalDistanceForEditor(string? canonicalKilometers) =>
        FormatCanonicalDistanceForEditor(canonicalKilometers, allowDecimalMiles: true);

    private string FormatCanonicalOdometerForEditor(string? canonicalKilometers) =>
        FormatCanonicalDistanceForEditor(canonicalKilometers, allowDecimalMiles: false);

    private string FormatCanonicalDistanceForEditor(string? canonicalKilometers, bool allowDecimalMiles)
    {
        var value = (canonicalKilometers ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return string.Empty;
        }

        if (!VehimapValueParser.TryParseDecimalNumber(value, out var kilometers))
        {
            return value;
        }

        var units = CurrentUnitPreferences;
        var decimalPlaces = allowDecimalMiles && string.Equals(AppUnitFormatService.NormalizeDistanceUnit(units.DistanceUnit), AppUnitFormatService.Miles, StringComparison.Ordinal)
            ? 1
            : 0;
        var distance = EditorUnitFormatService.ConvertDistanceFromKilometers(kilometers, units);
        return EditorNumberFormatService.FormatDecimal(distance, CurrentCulturePreferences, decimalPlaces);
    }

    private string FormatCanonicalVolumeForEditor(string? canonicalLiters)
    {
        var value = (canonicalLiters ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return string.Empty;
        }

        if (!VehimapValueParser.TryParseDecimalNumber(value, out var liters))
        {
            return value;
        }

        var units = CurrentUnitPreferences;
        var volume = EditorUnitFormatService.ConvertVolumeFromLiters(liters, units);
        var maxDecimalPlaces = string.Equals(AppUnitFormatService.NormalizeVolumeUnit(units.VolumeUnit), AppUnitFormatService.Liters, StringComparison.Ordinal)
            ? 2
            : 3;
        var decimalPlaces = CountMeaningfulDecimalPlaces(volume, maxDecimalPlaces);
        return EditorNumberFormatService.FormatDecimal(volume, CurrentCulturePreferences, decimalPlaces);
    }

    private bool TryNormalizeEditorDistanceToKilometers(string? text, bool allowEmpty, out string kilometers)
    {
        var value = (text ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            kilometers = string.Empty;
            return allowEmpty;
        }

        if (!TryParseEditorDecimal(value, out var distance) || distance < 0m)
        {
            kilometers = string.Empty;
            return false;
        }

        var convertedKilometers = EditorUnitFormatService.ConvertDistanceToKilometers(distance, CurrentUnitPreferences);
        kilometers = ((int)Math.Round(convertedKilometers, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
        return true;
    }

    private bool TryNormalizeEditorPositiveDistanceToKilometers(string? text, bool allowEmpty, out string kilometers)
    {
        if (!TryNormalizeEditorDistanceToKilometers(text, allowEmpty, out kilometers))
        {
            return false;
        }

        return kilometers.Length == 0 || int.Parse(kilometers, CultureInfo.InvariantCulture) > 0;
    }

    private bool TryNormalizeEditorVolumeToLiters(string? text, bool allowEmpty, out string liters)
    {
        var value = (text ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            liters = string.Empty;
            return allowEmpty;
        }

        if (!TryParseEditorDecimal(value, out var volume) || volume < 0m)
        {
            liters = string.Empty;
            return false;
        }

        var convertedLiters = EditorUnitFormatService.ConvertVolumeToLiters(volume, CurrentUnitPreferences);
        liters = convertedLiters.ToString("0.##", CultureInfo.InvariantCulture);
        return true;
    }

    private static int CountMeaningfulDecimalPlaces(decimal value, int maxDecimalPlaces)
    {
        var max = Math.Clamp(maxDecimalPlaces, 0, 9);
        for (var decimalPlaces = 0; decimalPlaces < max; decimalPlaces++)
        {
            if (decimal.Round(value, decimalPlaces) == value)
            {
                return decimalPlaces;
            }
        }

        return max;
    }

    private bool TryParseEditorDecimal(string value, out decimal number) =>
        EditorNumberFormatService.TryParseDecimal(value, CurrentCulturePreferences, out number)
        || VehimapValueParser.TryParseDecimalNumber(value, out number);

    private static string LWF(string key, params object?[] args) =>
        DesktopLocalization.Localizer.Format(key, args);

    private void NotifyEditorUnitMetadataChanged()
    {
        HistoryWorkspace.NotifyUnitMetadataChanged();
        FuelWorkspace.NotifyUnitMetadataChanged();
        MaintenanceWorkspace.NotifyUnitMetadataChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCreateHistory))]
    private void CreateHistory()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        _editingHistoryId = null;
        HistoryEditorDate = string.Empty;
        HistoryEditorType = string.Empty;
        HistoryEditorOdometer = string.Empty;
        HistoryEditorCost = string.Empty;
        HistoryEditorNote = string.Empty;
        HistoryEditorStatus = LO("HistoryEditor.Status.CreatePrompt");
        IsEditingHistory = true;
        SelectedVehicleTabIndex = HistoryTabIndex;
        RequestWorkspaceEditorDialog(WorkspaceEditorKind.History, DesktopFocusTarget.HistoryList);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelectedHistory))]
    private void EditSelectedHistory()
    {
        var entry = GetSelectedHistoryModel();
        if (entry is null)
        {
            return;
        }

        _editingHistoryId = entry.Id;
        HistoryEditorDate = entry.EventDate;
        HistoryEditorType = entry.EventType;
        HistoryEditorOdometer = FormatCanonicalOdometerForEditor(entry.Odometer);
        HistoryEditorCost = entry.Cost;
        HistoryEditorNote = entry.Note;
        HistoryEditorStatus = LO("HistoryEditor.Status.EditPrompt");
        IsEditingHistory = true;
        SelectedVehicleTabIndex = HistoryTabIndex;
        RequestWorkspaceEditorDialog(WorkspaceEditorKind.History, DesktopFocusTarget.HistoryList);
    }

    [RelayCommand(CanExecute = nameof(CanSaveHistory))]
    private async Task SaveHistoryAsync()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        var eventDateText = (HistoryEditorDate ?? string.Empty).Trim();
        var eventDate = LegacyVehicleValueNormalization.NormalizeEventDate(eventDateText);
        var eventType = (HistoryEditorType ?? string.Empty).Trim();
        var odometerText = (HistoryEditorOdometer ?? string.Empty).Trim();
        var odometer = string.Empty;
        var costText = (HistoryEditorCost ?? string.Empty).Trim();
        var cost = string.Empty;

        if (eventDate.Length == 0)
        {
            HistoryEditorStatus = LO("HistoryEditor.Validation.DateRequired");
            RequestFocus(DesktopFocusTarget.HistoryEditorDate);
            return;
        }

        if (eventType.Length == 0)
        {
            HistoryEditorStatus = LO("HistoryEditor.Validation.TypeRequired");
            RequestFocus(DesktopFocusTarget.HistoryEditorType);
            return;
        }

        if (!TryNormalizeEditorDistanceToKilometers(odometerText, allowEmpty: true, out odometer))
        {
            HistoryEditorStatus = LWF("HistoryEditor.Validation.OdometerInvalid", CurrentDistanceUnitLabel);
            RequestFocus(DesktopFocusTarget.HistoryEditorOdometer);
            return;
        }

        if (costText.Length > 0)
        {
            if (!VehimapValueParser.TryParseMoney(costText, out var parsedCost) || parsedCost < 0)
            {
                HistoryEditorStatus = LO("HistoryEditor.Validation.CostInvalid");
                RequestFocus(DesktopFocusTarget.HistoryEditorCost);
                return;
            }

            cost = parsedCost.ToString("0.##", CultureInfo.InvariantCulture);
        }

        var historyId = _editingHistoryId ?? GenerateLegacyId(_dataSet.HistoryEntries.Select(item => item.Id));
        var updatedEntry = new VehicleHistoryEntry(
            historyId,
            SelectedVehicle.Id,
            eventDate,
            eventType,
            odometer,
            cost,
            (HistoryEditorNote ?? string.Empty).Trim());

        var rollbackDataSet = CloneDataSet(_dataSet);
        UpsertHistoryEntry(updatedEntry);
        if (!await PersistDataAndRestoreSelectionAsync(
                SelectedVehicle.Id,
                HistoryTabIndex,
                historyId: historyId,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => HistoryEditorStatus = status,
                failureFocus: DesktopFocusTarget.HistoryEditorDate,
                failurePrefix: LO("HistoryEditor.Persistence.SaveFailed")))
        {
            return;
        }

        var wasNew = _editingHistoryId is null;
        CancelHistoryEditCore(clearStatus: false);
        HistoryEditorStatus = wasNew
            ? LO("HistoryEditor.Status.Created")
            : LO("HistoryEditor.Status.Updated");
        SelectedHistory = FindById(SelectedVehicleHistory, item => item.Id, historyId);
        RequestFocus(DesktopFocusTarget.HistoryList);
    }

    [RelayCommand(CanExecute = nameof(CanCancelHistoryEdit))]
    private void CancelHistoryEdit()
    {
        CancelHistoryEditCore(clearStatus: true);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedHistory))]
    private async Task DeleteSelectedHistoryAsync()
    {
        if (SelectedVehicle is null || SelectedHistory is null)
        {
            return;
        }

        var rollbackDataSet = CloneDataSet(_dataSet);
        _dataSet.HistoryEntries.RemoveAll(item => string.Equals(item.Id, SelectedHistory.Id, StringComparison.Ordinal));
        if (!await PersistDataAndRestoreSelectionAsync(
                SelectedVehicle.Id,
                HistoryTabIndex,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => HistoryEditorStatus = status,
                failureFocus: DesktopFocusTarget.HistoryList,
                failurePrefix: LO("HistoryEditor.Persistence.DeleteFailed")))
        {
            return;
        }
        HistoryEditorStatus = LO("HistoryEditor.Status.Deleted");
        RequestFocus(DesktopFocusTarget.HistoryList);
    }

    [RelayCommand(CanExecute = nameof(CanCreateFuel))]
    private void CreateFuel()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        _editingFuelId = null;
        FuelEditorDate = string.Empty;
        FuelEditorFuelType = string.Empty;
        FuelEditorFuelDetail = string.Empty;
        FuelEditorStation = string.Empty;
        FuelEditorLiters = string.Empty;
        FuelEditorTotalCost = string.Empty;
        FuelEditorOdometer = string.Empty;
        FuelEditorFullTank = true;
        FuelEditorNote = string.Empty;
        FuelEditorStatus = LO("FuelEditor.Status.CreatePrompt");
        IsEditingFuel = true;
        SelectedVehicleTabIndex = FuelTabIndex;
        RequestWorkspaceEditorDialog(WorkspaceEditorKind.Fuel, DesktopFocusTarget.FuelList);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelectedFuel))]
    private void EditSelectedFuel()
    {
        var entry = GetSelectedFuelModel();
        if (entry is null)
        {
            return;
        }

        _editingFuelId = entry.Id;
        FuelEditorDate = entry.EntryDate;
        FuelEditorFuelType = LegacyVehicleValueNormalization.NormalizeFuelType(entry.FuelType);
        FuelEditorFuelDetail = entry.FuelDetail;
        FuelEditorStation = entry.Station;
        FuelEditorLiters = FormatCanonicalVolumeForEditor(entry.Liters);
        FuelEditorTotalCost = entry.TotalCost;
        FuelEditorOdometer = FormatCanonicalOdometerForEditor(entry.Odometer);
        FuelEditorFullTank = entry.FullTank;
        FuelEditorNote = entry.Note;
        FuelEditorStatus = LO("FuelEditor.Status.EditPrompt");
        IsEditingFuel = true;
        SelectedVehicleTabIndex = FuelTabIndex;
        RequestWorkspaceEditorDialog(WorkspaceEditorKind.Fuel, DesktopFocusTarget.FuelList);
    }

    [RelayCommand(CanExecute = nameof(CanSaveFuel))]
    private async Task SaveFuelAsync()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        var entryDateText = (FuelEditorDate ?? string.Empty).Trim();
        var entryDate = LegacyVehicleValueNormalization.NormalizeEventDate(entryDateText);
        var odometerText = (FuelEditorOdometer ?? string.Empty).Trim();
        var odometer = string.Empty;
        var litersText = (FuelEditorLiters ?? string.Empty).Trim();
        var liters = string.Empty;
        var totalCostText = (FuelEditorTotalCost ?? string.Empty).Trim();
        var totalCost = string.Empty;

        if (entryDate.Length == 0)
        {
            FuelEditorStatus = LO("FuelEditor.Validation.DateRequired");
            RequestFocus(DesktopFocusTarget.FuelEditorDate);
            return;
        }

        if (!TryNormalizeEditorDistanceToKilometers(odometerText, allowEmpty: false, out odometer))
        {
            FuelEditorStatus = LWF("FuelEditor.Validation.OdometerRequired", CurrentDistanceUnitLabel);
            RequestFocus(DesktopFocusTarget.FuelEditorOdometer);
            return;
        }

        if (!TryNormalizeEditorVolumeToLiters(litersText, allowEmpty: true, out liters))
        {
            FuelEditorStatus = LWF("FuelEditor.Validation.VolumeInvalid", CurrentVolumeUnitLabel);
            RequestFocus(DesktopFocusTarget.FuelEditorLiters);
            return;
        }

        if (totalCostText.Length > 0)
        {
            if (!VehimapValueParser.TryParseMoney(totalCostText, out var parsedTotalCost) || parsedTotalCost < 0)
            {
                FuelEditorStatus = LO("FuelEditor.Validation.TotalCostInvalid");
                RequestFocus(DesktopFocusTarget.FuelEditorTotalCost);
                return;
            }

            totalCost = parsedTotalCost.ToString("0.##", CultureInfo.InvariantCulture);
        }

        if (totalCost.Length > 0 && liters.Length == 0)
        {
            FuelEditorStatus = LO("FuelEditor.Validation.CostRequiresVolume");
            RequestFocus(DesktopFocusTarget.FuelEditorLiters);
            return;
        }

        var fuelId = _editingFuelId ?? GenerateLegacyId(_dataSet.FuelEntries.Select(item => item.Id));
        var updatedEntry = new FuelEntry(
            fuelId,
            SelectedVehicle.Id,
            entryDate,
            odometer,
            liters,
            totalCost,
            FuelEditorFullTank,
            LegacyVehicleValueNormalization.NormalizeFuelType(FuelEditorFuelType),
            (FuelEditorNote ?? string.Empty).Trim(),
            (FuelEditorFuelDetail ?? string.Empty).Trim(),
            (FuelEditorStation ?? string.Empty).Trim());

        var rollbackDataSet = CloneDataSet(_dataSet);
        UpsertFuelEntry(updatedEntry);
        if (!await PersistDataAndRestoreSelectionAsync(
                SelectedVehicle.Id,
                FuelTabIndex,
                fuelId: fuelId,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => FuelEditorStatus = status,
                failureFocus: DesktopFocusTarget.FuelEditorDate,
                failurePrefix: LO("FuelEditor.Persistence.SaveFailed")))
        {
            return;
        }

        var wasNew = _editingFuelId is null;
        CancelFuelEditCore(clearStatus: false);
        FuelEditorStatus = wasNew
            ? LO("FuelEditor.Status.Created")
            : LO("FuelEditor.Status.Updated");
        SelectedFuel = FindById(SelectedVehicleFuel, item => item.Id, fuelId);
        RequestFocus(DesktopFocusTarget.FuelList);
    }

    [RelayCommand(CanExecute = nameof(CanCancelFuelEdit))]
    private void CancelFuelEdit()
    {
        CancelFuelEditCore(clearStatus: true);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedFuel))]
    private async Task DeleteSelectedFuelAsync()
    {
        if (SelectedVehicle is null || SelectedFuel is null)
        {
            return;
        }

        var rollbackDataSet = CloneDataSet(_dataSet);
        _dataSet.FuelEntries.RemoveAll(item => string.Equals(item.Id, SelectedFuel.Id, StringComparison.Ordinal));
        if (!await PersistDataAndRestoreSelectionAsync(
                SelectedVehicle.Id,
                FuelTabIndex,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => FuelEditorStatus = status,
                failureFocus: DesktopFocusTarget.FuelList,
                failurePrefix: LO("FuelEditor.Persistence.DeleteFailed")))
        {
            return;
        }
        FuelEditorStatus = LO("FuelEditor.Status.Deleted");
        RequestFocus(DesktopFocusTarget.FuelList);
    }

    [RelayCommand(CanExecute = nameof(CanCreateMaintenance))]
    private void CreateMaintenance()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        _editingMaintenanceId = null;
        MaintenanceEditorTitle = string.Empty;
        MaintenanceEditorIntervalKm = string.Empty;
        MaintenanceEditorIntervalMonths = string.Empty;
        MaintenanceEditorLastServiceDate = string.Empty;
        MaintenanceEditorLastServiceOdometer = string.Empty;
        MaintenanceEditorIsActive = true;
        MaintenanceEditorNote = string.Empty;
        MaintenanceEditorStatus = LO("MaintenanceEditor.Status.CreatePrompt");
        IsEditingMaintenance = true;
        SelectedVehicleTabIndex = MaintenanceTabIndex;
        RequestWorkspaceEditorDialog(WorkspaceEditorKind.Maintenance, DesktopFocusTarget.MaintenanceList);
    }

    [RelayCommand(CanExecute = nameof(CanEditSelectedMaintenance))]
    private void EditSelectedMaintenance()
    {
        var plan = GetSelectedMaintenanceModel();
        if (plan is null)
        {
            return;
        }

        _editingMaintenanceId = plan.Id;
        MaintenanceEditorTitle = plan.Title;
        MaintenanceEditorIntervalKm = FormatCanonicalDistanceForEditor(plan.IntervalKm);
        MaintenanceEditorIntervalMonths = plan.IntervalMonths;
        MaintenanceEditorLastServiceDate = plan.LastServiceDate;
        MaintenanceEditorLastServiceOdometer = FormatCanonicalOdometerForEditor(plan.LastServiceOdometer);
        MaintenanceEditorIsActive = plan.IsActive;
        MaintenanceEditorNote = plan.Note;
        MaintenanceEditorStatus = LO("MaintenanceEditor.Status.EditPrompt");
        IsEditingMaintenance = true;
        SelectedVehicleTabIndex = MaintenanceTabIndex;
        RequestWorkspaceEditorDialog(WorkspaceEditorKind.Maintenance, DesktopFocusTarget.MaintenanceList);
    }

    [RelayCommand(CanExecute = nameof(CanSaveMaintenance))]
    private async Task SaveMaintenanceAsync()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        var title = (MaintenanceEditorTitle ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MaintenanceEditorStatus = LO("MaintenanceEditor.Validation.TitleRequired");
            RequestFocus(DesktopFocusTarget.MaintenanceEditorTitle);
            return;
        }

        var intervalKmText = (MaintenanceEditorIntervalKm ?? string.Empty).Trim();
        var intervalMonthsText = (MaintenanceEditorIntervalMonths ?? string.Empty).Trim();
        var intervalKm = string.Empty;
        var intervalMonths = LegacyVehicleValueNormalization.NormalizePositiveInteger(intervalMonthsText);
        var lastServiceDateText = (MaintenanceEditorLastServiceDate ?? string.Empty).Trim();
        var lastServiceDate = LegacyVehicleValueNormalization.NormalizeEventDate(lastServiceDateText);
        var lastServiceOdometerText = (MaintenanceEditorLastServiceOdometer ?? string.Empty).Trim();
        var lastServiceOdometer = string.Empty;

        if (!TryNormalizeEditorPositiveDistanceToKilometers(intervalKmText, allowEmpty: true, out intervalKm))
        {
            MaintenanceEditorStatus = LWF("MaintenanceEditor.Validation.IntervalDistanceInvalid", CurrentDistanceUnitLabel);
            RequestFocus(DesktopFocusTarget.MaintenanceEditorIntervalKm);
            return;
        }

        if (intervalKm.Length == 0 && intervalMonths.Length == 0)
        {
            MaintenanceEditorStatus = LO("MaintenanceEditor.Validation.IntervalRequired");
            RequestFocus(DesktopFocusTarget.MaintenanceEditorIntervalKm);
            return;
        }

        if (intervalMonthsText.Length > 0 && intervalMonths.Length == 0)
        {
            MaintenanceEditorStatus = LO("MaintenanceEditor.Validation.IntervalMonthsInvalid");
            RequestFocus(DesktopFocusTarget.MaintenanceEditorIntervalMonths);
            return;
        }

        if (lastServiceDateText.Length > 0 && lastServiceDate.Length == 0)
        {
            MaintenanceEditorStatus = LO("MaintenanceEditor.Validation.LastServiceDateInvalid");
            RequestFocus(DesktopFocusTarget.MaintenanceEditorLastServiceDate);
            return;
        }

        if (intervalMonths.Length > 0 && lastServiceDate.Length == 0)
        {
            MaintenanceEditorStatus = LO("MaintenanceEditor.Validation.LastServiceDateRequired");
            RequestFocus(DesktopFocusTarget.MaintenanceEditorLastServiceDate);
            return;
        }

        if (!TryNormalizeEditorDistanceToKilometers(lastServiceOdometerText, allowEmpty: true, out lastServiceOdometer))
        {
            MaintenanceEditorStatus = LWF("MaintenanceEditor.Validation.LastServiceOdometerInvalid", CurrentDistanceUnitLabel);
            RequestFocus(DesktopFocusTarget.MaintenanceEditorLastServiceOdometer);
            return;
        }

        if (intervalKm.Length > 0 && lastServiceOdometer.Length == 0)
        {
            MaintenanceEditorStatus = LO("MaintenanceEditor.Validation.LastServiceOdometerRequired");
            RequestFocus(DesktopFocusTarget.MaintenanceEditorLastServiceOdometer);
            return;
        }

        var maintenanceId = _editingMaintenanceId ?? GenerateLegacyId(_dataSet.MaintenancePlans.Select(item => item.Id));
        var updatedPlan = new MaintenancePlan(
            maintenanceId,
            SelectedVehicle.Id,
            title,
            intervalKm,
            intervalMonths,
            lastServiceDate,
            lastServiceOdometer,
            MaintenanceEditorIsActive,
            (MaintenanceEditorNote ?? string.Empty).Trim());

        var rollbackDataSet = CloneDataSet(_dataSet);
        UpsertMaintenancePlan(updatedPlan);
        if (!await PersistDataAndRestoreSelectionAsync(
                SelectedVehicle.Id,
                MaintenanceTabIndex,
                maintenanceId: maintenanceId,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => MaintenanceEditorStatus = status,
                failureFocus: DesktopFocusTarget.MaintenanceEditorTitle,
                failurePrefix: LO("MaintenanceEditor.Persistence.SaveFailed")))
        {
            return;
        }

        var wasNew = _editingMaintenanceId is null;
        CancelMaintenanceEditCore(clearStatus: false);
        MaintenanceEditorStatus = wasNew
            ? LO("MaintenanceEditor.Status.Created")
            : LO("MaintenanceEditor.Status.Updated");
        SelectedMaintenance = FindById(SelectedVehicleMaintenance, item => item.Id, maintenanceId);
        RequestFocus(DesktopFocusTarget.MaintenanceList);
    }

    [RelayCommand(CanExecute = nameof(CanCancelMaintenanceEdit))]
    private void CancelMaintenanceEdit()
    {
        CancelMaintenanceEditCore(clearStatus: true);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedMaintenance))]
    private async Task DeleteSelectedMaintenanceAsync()
    {
        if (SelectedVehicle is null || SelectedMaintenance is null)
        {
            return;
        }

        var rollbackDataSet = CloneDataSet(_dataSet);
        _dataSet.MaintenancePlans.RemoveAll(item => string.Equals(item.Id, SelectedMaintenance.Id, StringComparison.Ordinal));
        if (!await PersistDataAndRestoreSelectionAsync(
                SelectedVehicle.Id,
                MaintenanceTabIndex,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => MaintenanceEditorStatus = status,
                failureFocus: DesktopFocusTarget.MaintenanceList,
                failurePrefix: LO("MaintenanceEditor.Persistence.DeleteFailed")))
        {
            return;
        }
        MaintenanceEditorStatus = LO("MaintenanceEditor.Status.Deleted");
        RequestFocus(DesktopFocusTarget.MaintenanceList);
    }

    [RelayCommand(CanExecute = nameof(CanCompleteSelectedMaintenance))]
    private async Task CompleteSelectedMaintenanceAsync()
    {
        var plan = GetSelectedMaintenanceModel();
        if (SelectedVehicle is null || plan is null)
        {
            return;
        }

        var todayText = FormatMaintenanceServiceDate(DateOnly.FromDateTime(DateTime.Today));
        var currentOdometer = TryGetCurrentVehicleOdometer(SelectedVehicle.Id, out var odometer)
            ? odometer.ToString(CultureInfo.InvariantCulture)
            : plan.LastServiceOdometer;

        var message = await ApplyMaintenanceCompletionAsync(new MaintenanceCompletionDialogResult(
            todayText,
            currentOdometer,
            AddHistory: false,
            HistoryCost: string.Empty,
            HistoryNote: string.Empty));
        MaintenanceEditorStatus = message;
    }

    internal MaintenanceCompletionDialogViewModel? BuildMaintenanceCompletionDialogViewModel()
    {
        var plan = GetSelectedMaintenanceModel();
        if (SelectedVehicle is null || plan is null)
        {
            return null;
        }

        var completedDate = FormatMaintenanceServiceDate(DateOnly.FromDateTime(DateTime.Today));
        var completedOdometer = TryGetCurrentVehicleOdometer(SelectedVehicle.Id, out var odometer)
            ? odometer.ToString(CultureInfo.InvariantCulture)
            : plan.LastServiceOdometer;

        return new MaintenanceCompletionDialogViewModel(
            SelectedVehicle.Name,
            plan.Title,
            SelectedMaintenance?.Status ?? LO("MaintenanceCompletion.Status.Unavailable"),
            !string.IsNullOrWhiteSpace(plan.IntervalKm),
            completedDate,
            FormatCanonicalOdometerForEditor(completedOdometer),
            CurrentCulturePreferences,
            CurrentUnitPreferences,
            DesktopLocalization.LiveLocalizer);
    }

    internal async Task<string> ApplyMaintenanceCompletionAsync(MaintenanceCompletionDialogResult completion)
    {
        var plan = GetSelectedMaintenanceModel();
        if (SelectedVehicle is null || plan is null)
        {
            return LO("MaintenanceCompletion.Status.SelectPlanFirst");
        }

        if (!plan.IsActive)
        {
            return LO("MaintenanceCompletion.Status.ReactivateFirst");
        }

        if (!VehimapValueParser.TryParseEventDate(completion.CompletedDate, out var completedDate))
        {
            return LO("MaintenanceCompletion.Validation.CompletedDate");
        }

        var completedOdometer = (completion.CompletedOdometer ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(completedOdometer))
        {
            if (!VehimapValueParser.TryParseOdometer(completedOdometer, out var parsedOdometer))
            {
                return LWF("MaintenanceCompletion.Validation.CompletedOdometerNumber", CurrentDistanceUnitLabel);
            }

            completedOdometer = parsedOdometer.ToString(CultureInfo.InvariantCulture);
        }
        else if (!string.IsNullOrWhiteSpace(plan.IntervalKm))
        {
            return LO("MaintenanceCompletion.Validation.CompletedOdometerRequired");
        }

        var historyCost = (completion.HistoryCost ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(historyCost))
        {
            if (!VehimapValueParser.TryParseMoney(historyCost, out var parsedCost))
            {
                return LO("MaintenanceCompletion.Validation.HistoryCost");
            }

            historyCost = parsedCost.ToString("0.##", CultureInfo.InvariantCulture);
        }

        var completedDateText = FormatMaintenanceServiceDate(completedDate);
        var updatedPlan = plan with
        {
            LastServiceDate = completedDateText,
            LastServiceOdometer = string.IsNullOrWhiteSpace(completedOdometer) ? plan.LastServiceOdometer : completedOdometer,
            IsActive = true
        };

        var rollbackDataSet = CloneDataSet(_dataSet);
        UpsertMaintenancePlan(updatedPlan);

        if (completion.AddHistory)
        {
            var historyNote = string.IsNullOrWhiteSpace(completion.HistoryNote)
                ? LO("MaintenanceCompletion.Status.HistoryNoteDefault")
                : completion.HistoryNote.Trim();
            UpsertHistoryEntry(new VehicleHistoryEntry(
                GenerateLegacyId(_dataSet.HistoryEntries.Select(item => item.Id)),
                updatedPlan.VehicleId,
                completedDateText,
                updatedPlan.Title,
                completedOdometer,
                historyCost,
                historyNote));
        }

        if (!await PersistDataAndRestoreSelectionAsync(
                updatedPlan.VehicleId,
                MaintenanceTabIndex,
                maintenanceId: updatedPlan.Id,
                rollbackDataSet: rollbackDataSet,
                setFailureStatus: status => MaintenanceEditorStatus = status,
                failureFocus: DesktopFocusTarget.MaintenanceList,
                failurePrefix: LO("MaintenanceCompletion.Persistence.SaveFailed")))
        {
            return MaintenanceEditorStatus;
        }

        var odometerMessage = string.IsNullOrWhiteSpace(updatedPlan.LastServiceOdometer)
            ? LO("MaintenanceCompletion.Status.OdometerEmpty")
            : LWF("MaintenanceCompletion.Status.OdometerValue", FormatCanonicalOdometerForEditor(updatedPlan.LastServiceOdometer), CurrentDistanceUnitLabel);
        var historyMessage = completion.AddHistory
            ? LO("MaintenanceCompletion.Status.HistoryAddedSuffix")
            : string.Empty;
        var message = LWF("MaintenanceCompletion.Status.Completed", completedDateText, odometerMessage, historyMessage);
        MaintenanceEditorStatus = message;
        SelectedMaintenance = FindById(SelectedVehicleMaintenance, item => item.Id, updatedPlan.Id);
        RequestFocus(DesktopFocusTarget.MaintenanceList);
        return message;
    }

    private VehicleHistoryEntry? GetSelectedHistoryModel()
    {
        if (SelectedHistory is null)
        {
            return null;
        }

        return _dataSet.HistoryEntries.FirstOrDefault(item => string.Equals(item.Id, SelectedHistory.Id, StringComparison.Ordinal));
    }

    private FuelEntry? GetSelectedFuelModel()
    {
        if (SelectedFuel is null)
        {
            return null;
        }

        return _dataSet.FuelEntries.FirstOrDefault(item => string.Equals(item.Id, SelectedFuel.Id, StringComparison.Ordinal));
    }

    private MaintenancePlan? GetSelectedMaintenanceModel()
    {
        if (SelectedMaintenance is null)
        {
            return null;
        }

        return _dataSet.MaintenancePlans.FirstOrDefault(item => string.Equals(item.Id, SelectedMaintenance.Id, StringComparison.Ordinal));
    }

    private bool TryGetCurrentVehicleOdometer(string vehicleId, out int odometer)
    {
        odometer = 0;
        var found = false;

        foreach (var entry in _dataSet.HistoryEntries.Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal)))
        {
            if (VehimapValueParser.TryParseOdometer(entry.Odometer, out var parsed) && (!found || parsed > odometer))
            {
                odometer = parsed;
                found = true;
            }
        }

        foreach (var entry in _dataSet.FuelEntries.Where(item => string.Equals(item.VehicleId, vehicleId, StringComparison.Ordinal)))
        {
            if (VehimapValueParser.TryParseOdometer(entry.Odometer, out var parsed) && (!found || parsed > odometer))
            {
                odometer = parsed;
                found = true;
            }
        }

        return found;
    }

    private static string FormatMaintenanceServiceDate(DateOnly date) =>
        date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    private void UpsertHistoryEntry(VehicleHistoryEntry updatedEntry)
    {
        var index = _dataSet.HistoryEntries.FindIndex(item => string.Equals(item.Id, updatedEntry.Id, StringComparison.Ordinal));
        if (index >= 0)
        {
            _dataSet.HistoryEntries[index] = updatedEntry;
        }
        else
        {
            _dataSet.HistoryEntries.Add(updatedEntry);
        }
    }

    private void UpsertFuelEntry(FuelEntry updatedEntry)
    {
        var index = _dataSet.FuelEntries.FindIndex(item => string.Equals(item.Id, updatedEntry.Id, StringComparison.Ordinal));
        if (index >= 0)
        {
            _dataSet.FuelEntries[index] = updatedEntry;
        }
        else
        {
            _dataSet.FuelEntries.Add(updatedEntry);
        }
    }

    private void UpsertMaintenancePlan(MaintenancePlan updatedPlan)
    {
        var index = _dataSet.MaintenancePlans.FindIndex(item => string.Equals(item.Id, updatedPlan.Id, StringComparison.Ordinal));
        if (index >= 0)
        {
            _dataSet.MaintenancePlans[index] = updatedPlan;
        }
        else
        {
            _dataSet.MaintenancePlans.Add(updatedPlan);
        }
    }

    private void CancelHistoryEditCore(bool clearStatus)
    {
        _editingHistoryId = null;
        IsEditingHistory = false;
        HistoryEditorDate = string.Empty;
        HistoryEditorType = string.Empty;
        HistoryEditorOdometer = string.Empty;
        HistoryEditorCost = string.Empty;
        HistoryEditorNote = string.Empty;
        if (clearStatus)
        {
            HistoryEditorStatus = string.Empty;
        }
    }

    private void CancelFuelEditCore(bool clearStatus)
    {
        _editingFuelId = null;
        IsEditingFuel = false;
        FuelEditorDate = string.Empty;
        FuelEditorFuelType = string.Empty;
        FuelEditorFuelDetail = string.Empty;
        FuelEditorStation = string.Empty;
        FuelEditorLiters = string.Empty;
        FuelEditorTotalCost = string.Empty;
        FuelEditorOdometer = string.Empty;
        FuelEditorFullTank = true;
        FuelEditorNote = string.Empty;
        if (clearStatus)
        {
            FuelEditorStatus = string.Empty;
        }
    }

    private void CancelMaintenanceEditCore(bool clearStatus)
    {
        _editingMaintenanceId = null;
        IsEditingMaintenance = false;
        MaintenanceEditorTitle = string.Empty;
        MaintenanceEditorIntervalKm = string.Empty;
        MaintenanceEditorIntervalMonths = string.Empty;
        MaintenanceEditorLastServiceDate = string.Empty;
        MaintenanceEditorLastServiceOdometer = string.Empty;
        MaintenanceEditorIsActive = true;
        MaintenanceEditorNote = string.Empty;
        if (clearStatus)
        {
            MaintenanceEditorStatus = string.Empty;
        }
    }
}
