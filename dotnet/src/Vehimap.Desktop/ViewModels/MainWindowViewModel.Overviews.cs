using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application;
using Vehimap.Application.Models;
using Vehimap.Desktop.ViewModels.Workspaces;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private const string OverviewDataIssueKind = "data_issue";
    private const string OverviewDataIssueFilterLabel = "Datové nedostatky";
    private const string OverviewMissingGreenDateLabel = "Nevyplněno";
    private const string OverviewAllFilterLabel = "Vše";
    private const string OverviewIncludeMissingGreenSettingKey = "include_missing_green";
    private const string OverviewIncludeDataIssuesSettingKey = "include_data_issues";
    private const string OverviewUpcomingFilterSettingKey = "upcoming_filter";
    private const string OverviewOverdueFilterSettingKey = "overdue_filter";
    private const string OverviewUpcomingSortSettingKey = "upcoming_sort";
    private const string OverviewUpcomingSortDescendingSettingKey = "upcoming_sort_descending";
    private const string OverviewOverdueSortSettingKey = "overdue_sort";
    private const string OverviewOverdueSortDescendingSettingKey = "overdue_sort_descending";
    private bool _suppressOverviewPreferenceRefresh;

    private ObservableCollection<VehicleTimelineItemViewModel> UpcomingOverviewItems => UpcomingOverviewWorkspace.UpcomingOverviewItems;

    private ObservableCollection<VehicleTimelineItemViewModel> OverdueOverviewItems => OverdueOverviewWorkspace.OverdueOverviewItems;

    [RelayCommand(CanExecute = nameof(CanOpenSelectedUpcomingOverviewItem))]
    private async Task OpenSelectedUpcomingOverviewItemAsync()
    {
        if (UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít blížící se termín").ConfigureAwait(true))
        {
            return;
        }

        OpenOverviewTimelineItem(UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedUpcomingOverviewVehicle))]
    private async Task OpenSelectedUpcomingOverviewVehicleAsync()
    {
        if (UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít vozidlo z přehledu termínů").ConfigureAwait(true))
        {
            return;
        }

        SelectVehicleAndOpenEntity(
            UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem.VehicleId,
            "Vozidlo",
            UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem.VehicleId);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedOverdueOverviewItem))]
    private async Task OpenSelectedOverdueOverviewItemAsync()
    {
        if (OverdueOverviewWorkspace.SelectedOverdueOverviewItem is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít propadlý termín").ConfigureAwait(true))
        {
            return;
        }

        OpenTimelineItem(OverdueOverviewWorkspace.SelectedOverdueOverviewItem);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedOverdueOverviewVehicle))]
    private async Task OpenSelectedOverdueOverviewVehicleAsync()
    {
        if (OverdueOverviewWorkspace.SelectedOverdueOverviewItem is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsBeforeNavigationAsync("otevřít vozidlo z propadlých termínů").ConfigureAwait(true))
        {
            return;
        }

        SelectVehicleAndOpenEntity(
            OverdueOverviewWorkspace.SelectedOverdueOverviewItem.VehicleId,
            "Vozidlo",
            OverdueOverviewWorkspace.SelectedOverdueOverviewItem.VehicleId);
    }

    private void RefreshFleetOverviews()
    {
        RefreshUpcomingOverview();
        RefreshOverdueOverview();
    }

    private void ApplyOverviewPreferences()
    {
        _suppressOverviewPreferenceRefresh = true;
        try
        {
            UpcomingOverviewWorkspace.IncludeMissingGreenCardsInUpcomingOverview = ReadOverviewBooleanSetting(OverviewIncludeMissingGreenSettingKey);
            UpcomingOverviewWorkspace.IncludeDataIssuesInUpcomingOverview = ReadOverviewBooleanSetting(OverviewIncludeDataIssuesSettingKey);
            UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter = NormalizeUpcomingOverviewFilter(_dataSet.Settings.GetValue("overview", OverviewUpcomingFilterSettingKey, OverviewAllFilterLabel));
            OverdueOverviewWorkspace.SelectedOverdueOverviewFilter = NormalizeOverdueOverviewFilter(_dataSet.Settings.GetValue("overview", OverviewOverdueFilterSettingKey, OverviewAllFilterLabel));
            UpcomingOverviewWorkspace.SelectedUpcomingOverviewSortOption = ReadOverviewSortOption(OverviewUpcomingSortSettingKey);
            UpcomingOverviewWorkspace.UpcomingOverviewSortDescending = ReadOverviewBooleanSetting(OverviewUpcomingSortDescendingSettingKey);
            OverdueOverviewWorkspace.SelectedOverdueOverviewSortOption = ReadOverviewSortOption(OverviewOverdueSortSettingKey);
            OverdueOverviewWorkspace.OverdueOverviewSortDescending = ReadOverviewBooleanSetting(OverviewOverdueSortDescendingSettingKey);
        }
        finally
        {
            _suppressOverviewPreferenceRefresh = false;
        }
    }

    private bool ReadOverviewBooleanSetting(string key) =>
        string.Equals(_dataSet.Settings.GetValue("overview", key, "0").Trim(), "1", StringComparison.Ordinal);

    private string ReadOverviewSortOption(string key) =>
        WorkspaceSortHelpers.NormalizeSortOption(
            _dataSet.Settings.GetValue("overview", key, WorkspaceSortHelpers.DateSortLabel),
            WorkspaceSortHelpers.TimelineOverviewSortOptions,
            WorkspaceSortHelpers.DateSortLabel);

    private void PersistOverviewPreferencesAsync()
    {
        if (_suppressOverviewPreferenceRefresh || !_session.IsLoaded)
        {
            return;
        }

        var includeMissingGreen = UpcomingOverviewWorkspace.IncludeMissingGreenCardsInUpcomingOverview ? "1" : "0";
        var includeDataIssues = UpcomingOverviewWorkspace.IncludeDataIssuesInUpcomingOverview ? "1" : "0";
        var upcomingFilter = NormalizeUpcomingOverviewFilter(UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter);
        var overdueFilter = NormalizeOverdueOverviewFilter(OverdueOverviewWorkspace.SelectedOverdueOverviewFilter);
        var upcomingSort = WorkspaceSortHelpers.NormalizeSortOption(UpcomingOverviewWorkspace.SelectedUpcomingOverviewSortOption, WorkspaceSortHelpers.TimelineOverviewSortOptions, WorkspaceSortHelpers.DateSortLabel);
        var upcomingDescending = UpcomingOverviewWorkspace.UpcomingOverviewSortDescending ? "1" : "0";
        var overdueSort = WorkspaceSortHelpers.NormalizeSortOption(OverdueOverviewWorkspace.SelectedOverdueOverviewSortOption, WorkspaceSortHelpers.TimelineOverviewSortOptions, WorkspaceSortHelpers.DateSortLabel);
        var overdueDescending = OverdueOverviewWorkspace.OverdueOverviewSortDescending ? "1" : "0";

        PersistPreferenceSettingsAsync(
            settings =>
            {
                settings.SetValue("overview", OverviewIncludeMissingGreenSettingKey, includeMissingGreen);
                settings.SetValue("overview", OverviewIncludeDataIssuesSettingKey, includeDataIssues);
                settings.SetValue("overview", OverviewUpcomingFilterSettingKey, upcomingFilter);
                settings.SetValue("overview", OverviewOverdueFilterSettingKey, overdueFilter);
                settings.SetValue("overview", OverviewUpcomingSortSettingKey, upcomingSort);
                settings.SetValue("overview", OverviewUpcomingSortDescendingSettingKey, upcomingDescending);
                settings.SetValue("overview", OverviewOverdueSortSettingKey, overdueSort);
                settings.SetValue("overview", OverviewOverdueSortDescendingSettingKey, overdueDescending);
            },
            "Nepodařilo se uložit volby přehledu termínů");
    }

    private string NormalizeUpcomingOverviewFilter(string? value) =>
        NormalizeOverviewFilter(value, UpcomingOverviewWorkspace.OverviewFilters);

    private string NormalizeOverdueOverviewFilter(string? value) =>
        NormalizeOverviewFilter(value, OverdueOverviewWorkspace.OverviewFilters);

    private static string NormalizeOverviewFilter(string? value, IReadOnlyList<string> supportedFilters)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? OverviewAllFilterLabel : value.Trim();
        return supportedFilters.Any(item => string.Equals(item, normalized, StringComparison.Ordinal))
            ? normalized
            : OverviewAllFilterLabel;
    }

    private void RefreshUpcomingOverview()
    {
        var previousKey = BuildOverviewSelectionKey(UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem);
        var items = WorkspaceSortHelpers.SortTimelineOverview(
                BuildFleetOverviewItems(
                    isFuture: true,
                    UpcomingOverviewWorkspace.SelectedUpcomingOverviewFilter,
                    UpcomingOverviewWorkspace.UpcomingOverviewSearchText,
                    UpcomingOverviewWorkspace.IncludeMissingGreenCardsInUpcomingOverview,
                    UpcomingOverviewWorkspace.IncludeDataIssuesInUpcomingOverview),
                UpcomingOverviewWorkspace.SelectedUpcomingOverviewSortOption,
                UpcomingOverviewWorkspace.UpcomingOverviewSortDescending)
            .ToList();

        UpcomingOverviewItems.Clear();
        foreach (var item in items)
        {
            UpcomingOverviewItems.Add(item);
        }

        UpcomingOverviewWorkspace.UpcomingOverviewSummary = BuildUpcomingOverviewSummary(items);

        UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem = FindById(UpcomingOverviewItems, BuildOverviewSelectionKey, previousKey) ?? UpcomingOverviewItems.FirstOrDefault();
        if (UpcomingOverviewWorkspace.SelectedUpcomingOverviewItem is null)
        {
            UpcomingOverviewWorkspace.SelectedUpcomingOverviewDetail = "Vyberte termín a můžete přejít na související vozidlo nebo evidenci.";
            NotifyUpcomingOverviewWorkspaceSelectionChanged();
        }
    }

    private void RefreshOverdueOverview()
    {
        var previousKey = BuildOverviewSelectionKey(OverdueOverviewWorkspace.SelectedOverdueOverviewItem);
        var items = WorkspaceSortHelpers.SortTimelineOverview(
                BuildFleetOverviewItems(isFuture: false, OverdueOverviewWorkspace.SelectedOverdueOverviewFilter, OverdueOverviewWorkspace.OverdueOverviewSearchText),
                OverdueOverviewWorkspace.SelectedOverdueOverviewSortOption,
                OverdueOverviewWorkspace.OverdueOverviewSortDescending)
            .ToList();

        OverdueOverviewItems.Clear();
        foreach (var item in items)
        {
            OverdueOverviewItems.Add(item);
        }

        OverdueOverviewWorkspace.OverdueOverviewSummary = items.Count == 0
            ? "V dostupných datech zatím nejsou žádné propadlé termíny s konkrétním datem."
            : $"Propadlých termínů: {items.Count}. Vyberte položku a můžete otevřít evidenci nebo vozidlo.";

        OverdueOverviewWorkspace.SelectedOverdueOverviewItem = FindById(OverdueOverviewItems, BuildOverviewSelectionKey, previousKey) ?? OverdueOverviewItems.FirstOrDefault();
        if (OverdueOverviewWorkspace.SelectedOverdueOverviewItem is null)
        {
            OverdueOverviewWorkspace.SelectedOverdueOverviewDetail = "Vyberte propadlý termín a můžete přejít na související vozidlo nebo evidenci.";
            NotifyOverdueOverviewWorkspaceSelectionChanged();
        }
    }

    private List<VehicleTimelineItemViewModel> BuildFleetOverviewItems(
        bool isFuture,
        string? filter,
        string? search,
        bool includeMissingGreenCards = false,
        bool includeDataIssues = false)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var items = _dataSet.Vehicles
            .SelectMany(vehicle => _timelineService.BuildVehicleTimeline(_dataSet, vehicle.Id, today))
            .Where(IsOverviewTimelineItem)
            .Where(item => item.IsFuture == isFuture)
            .Select(item => new FleetOverviewProjection(CreateTimelineItemViewModel(item), item.Date, 0));

        if (isFuture && includeMissingGreenCards)
        {
            items = items.Concat(BuildMissingGreenCardOverviewItems());
        }

        if (isFuture && includeDataIssues)
        {
            items = items.Concat(BuildDataIssueOverviewItems());
        }

        return items
            .Where(item => MatchesOverviewFilter(item.Item, filter))
            .Where(item => MatchesOverviewSearch(item.Item, search))
            .OrderBy(item => item.SortGroup)
            .ThenBy(item => item.SortDate)
            .ThenBy(item => item.Item.VehicleName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Item.KindLabel, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Item.Title, StringComparer.CurrentCultureIgnoreCase)
            .Select(item => item.Item)
            .ToList();
    }

    private static bool IsOverviewTimelineItem(VehicleTimelineItem item) =>
        item.Kind is "technical" or "green" or "custom" or "record" or "maintenance";

    private static bool MatchesOverviewFilter(VehicleTimelineItemViewModel item, string? filter)
    {
        return filter switch
        {
            "Technické kontroly" => item.Kind == "technical",
            "Zelené karty" => item.Kind == "green",
            "Připomínky" => item.Kind == "custom",
            "Doklady" => item.Kind == "record",
            "Údržba" => item.Kind == "maintenance",
            OverviewDataIssueFilterLabel => item.Kind == OverviewDataIssueKind,
            _ => true
        };
    }

    private static bool MatchesOverviewSearch(VehicleTimelineItemViewModel item, string? search)
    {
        var needle = search?.Trim();
        if (string.IsNullOrWhiteSpace(needle))
        {
            return true;
        }

        var haystack = string.Join(' ', new[]
        {
            item.Date,
            item.KindLabel,
            item.Title,
            item.Detail,
            item.Status,
            item.Note,
            item.VehicleName,
            item.EntryId
        });

        return haystack.Contains(needle, StringComparison.CurrentCultureIgnoreCase);
    }

    private static string BuildOverviewSelectionKey(VehicleTimelineItemViewModel? item) =>
        item is null ? string.Empty : $"{item.Kind}|{item.EntryId}|{item.VehicleId}|{item.Date}";

    private List<FleetOverviewProjection> BuildMissingGreenCardOverviewItems()
    {
        var sortDate = DateOnly.MaxValue.AddDays(-1);
        return _dataSet.Vehicles
            .Where(vehicle => string.IsNullOrWhiteSpace(vehicle.GreenCardTo))
            .OrderBy(vehicle => vehicle.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(vehicle => new FleetOverviewProjection(
                new VehicleTimelineItemViewModel(
                    "green",
                    "Zelená karta",
                    OverviewMissingGreenDateLabel,
                    "Chybí zelená karta",
                    BuildOverviewVehicleDetail(vehicle),
                    "Chybí",
                    vehicle.Name,
                    vehicle.Id,
                    string.Empty,
                    true,
                    string.Empty),
                sortDate,
                1))
            .ToList();
    }

    private List<FleetOverviewProjection> BuildDataIssueOverviewItems()
    {
        var sortDate = DateOnly.MaxValue;
        return _auditItems
            .Select(item => new FleetOverviewProjection(
                new VehicleTimelineItemViewModel(
                    OverviewDataIssueKind,
                    "Datový nedostatek",
                    "Doplnit",
                    item.Title,
                    $"{item.Category}: {item.Message}",
                    item.Severity switch
                    {
                        AuditSeverity.Error => "Chyba",
                        AuditSeverity.Warning => "Upozornění",
                        _ => "Info"
                    },
                    item.VehicleName,
                    item.VehicleId,
                    item.EntityId,
                    true,
                    item.EntityKind),
                sortDate,
                2))
            .ToList();
    }

    private string BuildUpcomingOverviewSummary(IReadOnlyCollection<VehicleTimelineItemViewModel> items)
    {
        var missingGreenCount = _dataSet.Vehicles.Count(vehicle => string.IsNullOrWhiteSpace(vehicle.GreenCardTo));
        var dataIssueCount = _auditItems.Count;
        var visibleDataIssueCount = items.Count(item => item.Kind == OverviewDataIssueKind);

        var summary = items.Count == 0
            ? UpcomingOverviewWorkspace.IncludeDataIssuesInUpcomingOverview
                ? "V dostupných datech zatím nejsou žádné blížící se termíny ani datové nedostatky k doplnění."
                : "V dostupných datech zatím nejsou žádné blížící se termíny s konkrétním datem."
            : $"Blížících se položek: {items.Count}. Vyberte položku a můžete otevřít evidenci nebo vozidlo.";

        if (visibleDataIssueCount > 0)
        {
            summary += $" Z toho datových nedostatků: {visibleDataIssueCount}.";
        }

        if (missingGreenCount > 0 && !UpcomingOverviewWorkspace.IncludeMissingGreenCardsInUpcomingOverview)
        {
            summary += $" U {missingGreenCount} vozidel není vyplněná zelená karta; můžete je přidat volbou pod filtrem.";
        }

        if (dataIssueCount > 0 && !UpcomingOverviewWorkspace.IncludeDataIssuesInUpcomingOverview)
        {
            summary += $" Audit eviduje {dataIssueCount} datových nedostatků; můžete je přidat volbou pod filtrem.";
        }

        return summary;
    }

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

    private static string BuildOverviewVehicleDetail(Vehicle vehicle) =>
        string.Join(" | ", new[] { vehicle.Plate, vehicle.MakeModel, vehicle.Category }
            .Where(part => !string.IsNullOrWhiteSpace(part)));

    private void OpenOverviewTimelineItem(VehicleTimelineItemViewModel item)
    {
        if (item.Kind == OverviewDataIssueKind)
        {
            SelectVehicleAndOpenEntity(item.VehicleId, item.Note, item.EntryId);
            return;
        }

        OpenTimelineItem(item);
    }

    private sealed record FleetOverviewProjection(
        VehicleTimelineItemViewModel Item,
        DateOnly SortDate,
        int SortGroup);
}
