using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MaintenanceCompletionDialogViewModel : ObservableObject
{
    public MaintenanceCompletionDialogViewModel(
        string vehicleName,
        string planTitle,
        string currentStatus,
        bool requiresOdometer,
        string completedDate,
        string completedOdometer)
    {
        VehicleName = vehicleName;
        PlanTitle = planTitle;
        CurrentStatus = currentStatus;
        RequiresOdometer = requiresOdometer;
        CompletedDate = completedDate;
        CompletedOdometer = completedOdometer;
    }

    public string VehicleName { get; }

    public string PlanTitle { get; }

    public string CurrentStatus { get; }

    public bool RequiresOdometer { get; }

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
            if (!VehimapValueParser.TryParseOdometer(completedOdometerText, out var parsedOdometer))
            {
                SetError("Tachometr při provedení zadejte jako celé číslo.", "MaintenanceCompletionOdometerBox");
                return false;
            }

            normalizedOdometer = parsedOdometer.ToString(CultureInfo.InvariantCulture);
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
