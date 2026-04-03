using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    public ObservableCollection<VehicleTimelineItemViewModel> UpcomingOverviewItems { get; } = [];

    public ObservableCollection<VehicleTimelineItemViewModel> OverdueOverviewItems { get; } = [];

    public IReadOnlyList<string> OverviewFilters { get; } =
    [
        "Vše",
        "Technické kontroly",
        "Zelené karty",
        "Připomínky",
        "Doklady",
        "Údržba"
    ];

    [RelayCommand(CanExecute = nameof(CanOpenSelectedUpcomingOverviewItem))]
    private async Task OpenSelectedUpcomingOverviewItemAsync()
    {
        if (SelectedUpcomingOverviewItem is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít blížící se termín").ConfigureAwait(true))
        {
            return;
        }

        OpenTimelineItem(SelectedUpcomingOverviewItem);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedUpcomingOverviewVehicle))]
    private async Task OpenSelectedUpcomingOverviewVehicleAsync()
    {
        if (SelectedUpcomingOverviewItem is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít vozidlo z přehledu termínů").ConfigureAwait(true))
        {
            return;
        }

        SelectVehicleAndOpenEntity(SelectedUpcomingOverviewItem.VehicleId, "Vozidlo", SelectedUpcomingOverviewItem.VehicleId);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedOverdueOverviewItem))]
    private async Task OpenSelectedOverdueOverviewItemAsync()
    {
        if (SelectedOverdueOverviewItem is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít propadlý termín").ConfigureAwait(true))
        {
            return;
        }

        OpenTimelineItem(SelectedOverdueOverviewItem);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedOverdueOverviewVehicle))]
    private async Task OpenSelectedOverdueOverviewVehicleAsync()
    {
        if (SelectedOverdueOverviewItem is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít vozidlo z propadlých termínů").ConfigureAwait(true))
        {
            return;
        }

        SelectVehicleAndOpenEntity(SelectedOverdueOverviewItem.VehicleId, "Vozidlo", SelectedOverdueOverviewItem.VehicleId);
    }

    private void RefreshFleetOverviews()
    {
        RefreshUpcomingOverview();
        RefreshOverdueOverview();
    }

    private void RefreshUpcomingOverview()
    {
        var previousKey = BuildOverviewSelectionKey(SelectedUpcomingOverviewItem);
        var items = BuildFleetOverviewItems(isFuture: true, SelectedUpcomingOverviewFilter, UpcomingOverviewSearchText);

        UpcomingOverviewItems.Clear();
        foreach (var item in items)
        {
            UpcomingOverviewItems.Add(item);
        }

        UpcomingOverviewSummary = items.Count == 0
            ? "V dostupných legacy datech zatím nejsou žádné blížící se termíny s konkrétním datem."
            : $"Blížících se termínů: {items.Count}. Vyberte položku a můžete otevřít evidenci nebo vozidlo.";

        SelectedUpcomingOverviewItem = FindById(UpcomingOverviewItems, BuildOverviewSelectionKey, previousKey) ?? UpcomingOverviewItems.FirstOrDefault();
        if (SelectedUpcomingOverviewItem is null)
        {
            SelectedUpcomingOverviewDetail = "Vyberte termín a můžete přejít na související vozidlo nebo evidenci.";
            NotifyUpcomingOverviewWorkspaceSelectionChanged();
        }
    }

    private void RefreshOverdueOverview()
    {
        var previousKey = BuildOverviewSelectionKey(SelectedOverdueOverviewItem);
        var items = BuildFleetOverviewItems(isFuture: false, SelectedOverdueOverviewFilter, OverdueOverviewSearchText);

        OverdueOverviewItems.Clear();
        foreach (var item in items)
        {
            OverdueOverviewItems.Add(item);
        }

        OverdueOverviewSummary = items.Count == 0
            ? "V dostupných legacy datech zatím nejsou žádné propadlé termíny s konkrétním datem."
            : $"Propadlých termínů: {items.Count}. Vyberte položku a můžete otevřít evidenci nebo vozidlo.";

        SelectedOverdueOverviewItem = FindById(OverdueOverviewItems, BuildOverviewSelectionKey, previousKey) ?? OverdueOverviewItems.FirstOrDefault();
        if (SelectedOverdueOverviewItem is null)
        {
            SelectedOverdueOverviewDetail = "Vyberte propadlý termín a můžete přejít na související vozidlo nebo evidenci.";
            NotifyOverdueOverviewWorkspaceSelectionChanged();
        }
    }

    private List<VehicleTimelineItemViewModel> BuildFleetOverviewItems(bool isFuture, string? filter, string? search)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var items = _dataSet.Vehicles
            .SelectMany(vehicle => _timelineService.BuildVehicleTimeline(_dataSet, vehicle.Id, today))
            .Where(IsOverviewTimelineItem)
            .Where(item => item.IsFuture == isFuture)
            .Where(item => MatchesOverviewFilter(item, filter))
            .Where(item => MatchesOverviewSearch(item, search))
            .OrderBy(item => item.Date)
            .ThenBy(item => item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.KindLabel, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(CreateTimelineItemViewModel)
            .ToList();

        return items;
    }

    private static bool IsOverviewTimelineItem(VehicleTimelineItem item) =>
        item.Kind is "technical" or "green" or "custom" or "record" or "maintenance";

    private static bool MatchesOverviewFilter(VehicleTimelineItem item, string? filter)
    {
        return filter switch
        {
            "Technické kontroly" => item.Kind == "technical",
            "Zelené karty" => item.Kind == "green",
            "Připomínky" => item.Kind == "custom",
            "Doklady" => item.Kind == "record",
            "Údržba" => item.Kind == "maintenance",
            _ => true
        };
    }

    private static bool MatchesOverviewSearch(VehicleTimelineItem item, string? search)
    {
        var needle = search?.Trim();
        if (string.IsNullOrWhiteSpace(needle))
        {
            return true;
        }

        var haystack = string.Join(' ', new[]
        {
            item.DateText,
            item.KindLabel,
            item.Title,
            item.Detail,
            item.Status,
            item.Note,
            item.VehicleName,
            item.VehiclePlate,
            item.VehicleMakeModel
        });

        return haystack.Contains(needle, StringComparison.CurrentCultureIgnoreCase);
    }

    private static string BuildOverviewSelectionKey(VehicleTimelineItemViewModel? item) =>
        item is null ? string.Empty : $"{item.Kind}|{item.EntryId}|{item.VehicleId}|{item.Date}";

    private static VehicleTimelineItemViewModel CreateTimelineItemViewModel(VehicleTimelineItem item) => new(
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
        item.Note);
}
