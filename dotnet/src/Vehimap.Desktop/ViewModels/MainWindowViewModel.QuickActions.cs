using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    [RelayCommand]
    private async Task OpenNearestTechnicalAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít nejbližší technickou kontrolu").ConfigureAwait(true))
        {
            return;
        }

        var items = BuildQuickActionItems("technical");
        if (items.Count == 0)
        {
            ShellStatus = "Momentálně není žádné vozidlo s blížící se nebo propadlou technickou kontrolou.";
            return;
        }

        ShellStatus = $"Nejbližší technická kontrola: {items[0].VehicleName} - {items[0].Date}.";
        OpenTimelineItem(items[0]);
    }

    [RelayCommand]
    private async Task ReviewTechnicalAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("zkontrolovat technické kontroly").ConfigureAwait(true))
        {
            return;
        }

        OpenQuickActionOverview(
            "technical",
            AttentionVehicleStatusFilterLabel,
            "Technické kontroly",
            "Žádná vozidla teď nevyžadují upozornění na technickou kontrolu.");
    }

    [RelayCommand]
    private async Task OpenNearestGreenCardAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít nejbližší zelenou kartu").ConfigureAwait(true))
        {
            return;
        }

        if (!HasAnyGreenCardConfigured())
        {
            ShellStatus = "U žádného vozidla není vyplněná zelená karta. Můžete ji doplnit v detailu vozidla.";
            return;
        }

        var items = BuildQuickActionItems("green");
        if (items.Count == 0)
        {
            ShellStatus = HasAnyMissingGreenCard()
                ? "Žádná vyplněná zelená karta teď nevyžaduje upozornění. U některých vozidel zelená karta vyplněná není."
                : "Žádná vyplněná zelená karta teď nevyžaduje upozornění.";
            return;
        }

        ShellStatus = $"Nejbližší zelená karta: {items[0].VehicleName} - {items[0].Date}.";
        OpenTimelineItem(items[0]);
    }

    [RelayCommand]
    private async Task ReviewGreenCardsAsync()
    {
        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("zkontrolovat zelené karty").ConfigureAwait(true))
        {
            return;
        }

        if (!HasAnyGreenCardConfigured())
        {
            ShellStatus = "U žádného vozidla není vyplněná zelená karta. Můžete ji doplnit v detailu vozidla.";
            return;
        }

        OpenQuickActionOverview(
            "green",
            MissingGreenVehicleStatusFilterLabel,
            "Zelené karty",
            HasAnyMissingGreenCard()
                ? "Žádná vyplněná zelená karta teď nevyžaduje upozornění. U některých vozidel zelená karta vyplněná není."
                : "Žádná vyplněná zelená karta teď nevyžaduje upozornění.");
    }

    private List<VehicleTimelineItemViewModel> BuildQuickActionItems(string kind)
    {
        return _dataSet.Vehicles
            .SelectMany(vehicle => _timelineService.BuildVehicleTimeline(_dataSet, vehicle.Id, DateOnly.FromDateTime(DateTime.Today)))
            .Where(item => string.Equals(item.Kind, kind, StringComparison.Ordinal))
            .Where(item => !string.IsNullOrWhiteSpace(item.Status) && !string.Equals(item.Status, "Bez upozornění", StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(item => item.IsFuture ? 1 : 0)
            .ThenBy(item => item.IsFuture ? item.Date.DayNumber : -item.Date.DayNumber)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => new VehicleTimelineItemViewModel(
                item.Kind,
                item.KindLabel,
                item.DateText,
                item.Title,
                item.Detail,
                item.Status,
                item.VehicleName,
                item.VehicleId,
                item.EntryId,
                item.IsFuture,
                item.Note))
            .ToList();
    }

    private void OpenQuickActionOverview(string kind, string emptyFilterStatusMessage, string overviewFilterLabel, string emptyMessage)
    {
        var items = BuildQuickActionItems(kind);
        if (items.Count == 0)
        {
            ShellStatus = emptyMessage;
            return;
        }

        var targetTabIndex = items.Any(item => !item.IsFuture)
            ? OverdueOverviewTabIndex
            : UpcomingOverviewTabIndex;

        if (targetTabIndex == UpcomingOverviewTabIndex)
        {
            UpcomingOverviewSearchText = string.Empty;
            SelectedUpcomingOverviewFilter = overviewFilterLabel;
            SelectedVehicleTabIndex = UpcomingOverviewTabIndex;
            SelectedUpcomingOverviewItem = FindById(
                UpcomingOverviewItems,
                BuildOverviewSelectionKey,
                BuildOverviewSelectionKey(items[0]));
            RequestFocus(DesktopFocusTarget.UpcomingOverviewList);
        }
        else
        {
            OverdueOverviewSearchText = string.Empty;
            SelectedOverdueOverviewFilter = overviewFilterLabel;
            SelectedVehicleTabIndex = OverdueOverviewTabIndex;
            SelectedOverdueOverviewItem = FindById(
                OverdueOverviewItems,
                BuildOverviewSelectionKey,
                BuildOverviewSelectionKey(items[0]));
            RequestFocus(DesktopFocusTarget.OverdueOverviewList);
        }

        ShellStatus = kind switch
        {
            "technical" => $"Technické kontroly k prověření: {items.Count}. Otevřen je příslušný přehled.",
            "green" => $"Zelené karty k prověření: {items.Count}. Otevřen je příslušný přehled.",
            _ => emptyFilterStatusMessage
        };
    }

    private bool HasAnyGreenCardConfigured() =>
        _dataSet.Vehicles.Any(vehicle => !string.IsNullOrWhiteSpace(vehicle.GreenCardTo));

    private bool HasAnyMissingGreenCard() =>
        _dataSet.Vehicles.Any(vehicle => string.IsNullOrWhiteSpace(vehicle.GreenCardTo));
}
