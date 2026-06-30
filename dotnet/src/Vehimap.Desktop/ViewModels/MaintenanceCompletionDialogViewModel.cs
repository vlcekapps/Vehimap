using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MaintenanceCompletionDialogViewModel : ObservableObject
{
    private static readonly AppNumberFormatService NumberFormatService = new();
    private static readonly AppUnitFormatService UnitFormatService = new();

    private readonly AppCulturePreferences _culturePreferences;
    private readonly AppUnitPreferences _unitPreferences;

    public MaintenanceCompletionDialogViewModel(
        string vehicleName,
        string planTitle,
        string currentStatus,
        bool requiresOdometer,
        string completedDate,
        string completedOdometer,
        AppCulturePreferences? culturePreferences = null,
        AppUnitPreferences? unitPreferences = null)
    {
        VehicleName = vehicleName;
        PlanTitle = planTitle;
        CurrentStatus = currentStatus;
        RequiresOdometer = requiresOdometer;
        CompletedDate = completedDate;
        CompletedOdometer = completedOdometer;
        _culturePreferences = culturePreferences ?? new AppCulturePreferences();
        _unitPreferences = UnitFormatService.Normalize(unitPreferences ?? new AppUnitPreferences());
    }

    public string VehicleName { get; }

    public string PlanTitle { get; }

    public string CurrentStatus { get; }

    public bool RequiresOdometer { get; }

    public string DistanceUnitLabel =>
        string.Equals(_unitPreferences.DistanceUnit, AppUnitFormatService.Miles, StringComparison.Ordinal)
            ? "mi"
            : "km";

    public string CompletedOdometerLabel => $"Tachometr při provedení ({DistanceUnitLabel})";

    public string CompletedOdometerName => $"Tachometr při provedení servisního úkonu v {DistanceUnitLabel}";

    public string CompletedOdometerHelp => $"Zadejte stav tachometru v {DistanceUnitLabel}. Vehimap hodnotu uloží interně v kilometrech.";

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
            SetError("Datum provedení musí být ve formátu DD.MM.RRRR.", "MaintenanceCompletionDateBox");
            return false;
        }

        var completedOdometerText = (CompletedOdometer ?? string.Empty).Trim();
        var normalizedOdometer = string.Empty;
        if (!string.IsNullOrWhiteSpace(completedOdometerText))
        {
            if (!NumberFormatService.TryParseDecimal(completedOdometerText, _culturePreferences, out var parsedOdometer)
                || parsedOdometer < 0m)
            {
                SetError($"Tachometr při provedení zadejte jako číslo v {DistanceUnitLabel}.", "MaintenanceCompletionOdometerBox");
                return false;
            }

            var convertedKilometers = UnitFormatService.ConvertDistanceToKilometers(parsedOdometer, _unitPreferences);
            normalizedOdometer = ((int)Math.Round(convertedKilometers, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
        }
        else if (RequiresOdometer)
        {
            SetError("Pro kilometrický interval vyplňte i stav tachometru při provedení úkonu.", "MaintenanceCompletionOdometerBox");
            return false;
        }

        var historyCostText = (HistoryCost ?? string.Empty).Trim();
        var normalizedCost = string.Empty;
        if (!string.IsNullOrWhiteSpace(historyCostText))
        {
            if (!VehimapValueParser.TryParseMoney(historyCostText, out var parsedCost))
            {
                SetError("Cenu do historie zadejte jako číslo, například 2500.", "MaintenanceCompletionHistoryCostBox");
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
