using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    [ObservableProperty]
    private string upcomingOverviewSearchText = string.Empty;

    [ObservableProperty]
    private string overdueOverviewSearchText = string.Empty;

    [ObservableProperty]
    private string selectedUpcomingOverviewFilter = "Vše";

    [ObservableProperty]
    private string selectedOverdueOverviewFilter = "Vše";

    [ObservableProperty]
    private string upcomingOverviewSummary = "Blížící se termíny napříč vozidly se zobrazí po načtení dat.";

    [ObservableProperty]
    private string overdueOverviewSummary = "Propadlé termíny napříč vozidly se zobrazí po načtení dat.";

    [ObservableProperty]
    private string selectedUpcomingOverviewDetail = "Vyberte termín a můžete přejít na související vozidlo nebo evidenci.";

    [ObservableProperty]
    private string selectedOverdueOverviewDetail = "Vyberte propadlý termín a můžete přejít na související vozidlo nebo evidenci.";

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedUpcomingOverviewItem;

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedOverdueOverviewItem;

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

    public bool CanOpenSelectedUpcomingOverviewItem => SelectedUpcomingOverviewItem is not null;

    public bool CanOpenSelectedUpcomingOverviewVehicle => SelectedUpcomingOverviewItem is not null;

    public bool CanOpenSelectedOverdueOverviewItem => SelectedOverdueOverviewItem is not null;

    public bool CanOpenSelectedOverdueOverviewVehicle => SelectedOverdueOverviewItem is not null;

    partial void OnUpcomingOverviewSearchTextChanged(string value)
    {
        RefreshUpcomingOverview();
    }

    partial void OnOverdueOverviewSearchTextChanged(string value)
    {
        RefreshOverdueOverview();
    }

    partial void OnSelectedUpcomingOverviewFilterChanged(string value)
    {
        RefreshUpcomingOverview();
    }

    partial void OnSelectedOverdueOverviewFilterChanged(string value)
    {
        RefreshOverdueOverview();
    }

    partial void OnSelectedUpcomingOverviewItemChanged(VehicleTimelineItemViewModel? value)
    {
        SelectedUpcomingOverviewDetail = value is null
            ? "Vyberte termín a můžete přejít na související vozidlo nebo evidenci."
            : $"{value.VehicleName}\n{value.Date} | {value.KindLabel}\n{value.Title}\n{value.Detail}\nStav: {value.Status}";

        OpenSelectedUpcomingOverviewItemCommand.NotifyCanExecuteChanged();
        OpenSelectedUpcomingOverviewVehicleCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedOverdueOverviewItemChanged(VehicleTimelineItemViewModel? value)
    {
        SelectedOverdueOverviewDetail = value is null
            ? "Vyberte propadlý termín a můžete přejít na související vozidlo nebo evidenci."
            : $"{value.VehicleName}\n{value.Date} | {value.KindLabel}\n{value.Title}\n{value.Detail}\nStav: {value.Status}";

        OpenSelectedOverdueOverviewItemCommand.NotifyCanExecuteChanged();
        OpenSelectedOverdueOverviewVehicleCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedUpcomingOverviewItem))]
    private void OpenSelectedUpcomingOverviewItem()
    {
        if (SelectedUpcomingOverviewItem is null)
        {
            return;
        }

        OpenTimelineItem(SelectedUpcomingOverviewItem);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedUpcomingOverviewVehicle))]
    private void OpenSelectedUpcomingOverviewVehicle()
    {
        if (SelectedUpcomingOverviewItem is null)
        {
            return;
        }

        SelectVehicleAndOpenEntity(SelectedUpcomingOverviewItem.VehicleId, "Vozidlo", SelectedUpcomingOverviewItem.VehicleId);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedOverdueOverviewItem))]
    private void OpenSelectedOverdueOverviewItem()
    {
        if (SelectedOverdueOverviewItem is null)
        {
            return;
        }

        OpenTimelineItem(SelectedOverdueOverviewItem);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedOverdueOverviewVehicle))]
    private void OpenSelectedOverdueOverviewVehicle()
    {
        if (SelectedOverdueOverviewItem is null)
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
            OnSelectedUpcomingOverviewItemChanged(null);
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
            OnSelectedOverdueOverviewItemChanged(null);
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
