using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MaintenanceCompletionDialogViewModel : ObservableObject
{
    private static readonly AppNumberFormatService NumberFormatService = new();
    private static readonly AppUnitFormatService UnitFormatService = new();

    private readonly AppCulturePreferences _culturePreferences;
    private readonly AppUnitPreferences _unitPreferences;
    private readonly IAppLocalizer _localizer;

    public MaintenanceCompletionDialogViewModel(
        string vehicleName,
        string planTitle,
        string currentStatus,
        bool requiresOdometer,
        string completedDate,
        string completedOdometer,
        AppCulturePreferences? culturePreferences = null,
        AppUnitPreferences? unitPreferences = null,
        IAppLocalizer? localizer = null)
    {
        VehicleName = vehicleName;
        PlanTitle = planTitle;
        CurrentStatus = currentStatus;
        RequiresOdometer = requiresOdometer;
        CompletedDate = completedDate;
        CompletedOdometer = completedOdometer;
        _culturePreferences = culturePreferences ?? new AppCulturePreferences();
        _unitPreferences = UnitFormatService.Normalize(unitPreferences ?? new AppUnitPreferences());
        _localizer = localizer ?? new ResourceAppLocalizer();
    }

    public string VehicleName { get; }

    public string PlanTitle { get; }

    public string CurrentStatus { get; }

    public bool RequiresOdometer { get; }

    public string DistanceUnitLabel =>
        string.Equals(_unitPreferences.DistanceUnit, AppUnitFormatService.Miles, StringComparison.Ordinal)
            ? "mi"
            : "km";

    public string CompletedOdometerLabel => _localizer.Format("MaintenanceCompletion.CompletedOdometerLabel", DistanceUnitLabel);

    public string CompletedOdometerName => _localizer.Format("MaintenanceCompletion.CompletedOdometerName", DistanceUnitLabel);

    public string CompletedOdometerHelp => _localizer.Format("MaintenanceCompletion.CompletedOdometerHelp", DistanceUnitLabel);

    [ObservableProperty]
    private string completedDate = string.Empty;

    [ObservableProperty]
    private string completedOdometer = string.Empty;

    [ObservableProperty]
    private bool addHistory = true;

    [ObservableProperty]
    private string historyCost = string.Empty;

    [ObservableProperty]
    private string historyNote = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public string? ErrorFocusTarget { get; private set; }

    public bool TryBuildResult(out MaintenanceCompletionDialogResult? result)
    {
        ErrorMessage = string.Empty;
        ErrorFocusTarget = null;
        result = null;

        if (!VehimapValueParser.TryParseEventDate(CompletedDate, out var parsedDate))
        {
            SetError(_localizer.GetString("MaintenanceCompletion.Validation.CompletedDate"), "MaintenanceCompletionDateBox");
            return false;
        }

        var completedOdometerText = (CompletedOdometer ?? string.Empty).Trim();
        var normalizedOdometer = string.Empty;
        if (!string.IsNullOrWhiteSpace(completedOdometerText))
        {
            if (!NumberFormatService.TryParseDecimal(completedOdometerText, _culturePreferences, out var parsedOdometer)
                || parsedOdometer < 0m)
            {
                SetError(_localizer.Format("MaintenanceCompletion.Validation.CompletedOdometerNumber", DistanceUnitLabel), "MaintenanceCompletionOdometerBox");
                return false;
            }

            var convertedKilometers = UnitFormatService.ConvertDistanceToKilometers(parsedOdometer, _unitPreferences);
            normalizedOdometer = ((int)Math.Round(convertedKilometers, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
        }
        else if (RequiresOdometer)
        {
            SetError(_localizer.GetString("MaintenanceCompletion.Validation.CompletedOdometerRequired"), "MaintenanceCompletionOdometerBox");
            return false;
        }

        var historyCostText = (HistoryCost ?? string.Empty).Trim();
        var normalizedCost = string.Empty;
        if (!string.IsNullOrWhiteSpace(historyCostText))
        {
            if (!VehimapValueParser.TryParseMoney(historyCostText, out var parsedCost))
            {
                SetError(_localizer.GetString("MaintenanceCompletion.Validation.HistoryCost"), "MaintenanceCompletionHistoryCostBox");
                return false;
            }

            normalizedCost = parsedCost.ToString("0.##", CultureInfo.InvariantCulture);
        }

        result = new MaintenanceCompletionDialogResult(
            parsedDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
            normalizedOdometer,
            AddHistory,
            normalizedCost,
            (HistoryNote ?? string.Empty).Trim());
        return true;
    }

    private void SetError(string message, string focusTarget)
    {
        ErrorMessage = message;
        ErrorFocusTarget = focusTarget;
    }
}
