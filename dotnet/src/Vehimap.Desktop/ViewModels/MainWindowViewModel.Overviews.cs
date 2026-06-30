using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application;
using Vehimap.Application.Models;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels.Workspaces;
using Vehimap.Domain.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private const string OverviewDataIssueKind = "data_issue";
    private const string OverviewAllFilterLegacyCzech = "V\u0161e";
    private const string OverviewTechnicalFilterLegacyCzech = "Technick\u00E9 kontroly";
    private const string OverviewGreenCardsFilterLegacyCzech = "Zelen\u00E9 karty";
    private const string OverviewRemindersFilterLegacyCzech = "P\u0159ipom\u00EDnky";
    private const string OverviewRecordsFilterLegacyCzech = "Doklady";
    private const string OverviewMaintenanceFilterLegacyCzech = "\u00DAdr\u017Eba";
    private const string OverviewDataIssuesFilterLegacyCzech = "Datov\u00E9 nedostatky";
    private const string OverviewAllFilterLegacyEnglish = "All";
    private const string OverviewTechnicalFilterLegacyEnglish = "Technical inspections";
    private const string OverviewGreenCardsFilterLegacyEnglish = "Green cards";
    private const string OverviewRemindersFilterLegacyEnglish = "Reminders";
    private const string OverviewRecordsFilterLegacyEnglish = "Documents";
    private const string OverviewMaintenanceFilterLegacyEnglish = "Maintenance";
    private const string OverviewDataIssuesFilterLegacyEnglish = "Data issues";
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

    private static string OverviewAllFilterLabel => LO("Overview.Filter.All");

    private static string OverviewDataIssueFilterLabel => LO("Overview.Filter.DataIssues");

    private static string OverviewMissingGreenDateLabel => LO("Overview.MissingGreen.Date");

    private static string LO(string key) => DesktopLocalization.Localizer.GetString(key);

    private static string LFO(string key, params object?[] args) => DesktopLocalization.Localizer.Format(key, args);

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
            LO("Overview.Persistence.Error"));
    }

    private string NormalizeUpcomingOverviewFilter(string? value) =>
        NormalizeOverviewFilter(value, UpcomingOverviewWorkspace.OverviewFilters);

    private string NormalizeOverdueOverviewFilter(string? value) =>
        NormalizeOverviewFilter(value, OverdueOverviewWorkspace.OverviewFilters);

    private static string NormalizeOverviewFilter(string? value, IReadOnlyList<string> supportedFilters)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? OverviewAllFilterLabel
            : NormalizeOverviewFilterAlias(value.Trim());
        return supportedFilters.Any(item => string.Equals(item, normalized, StringComparison.Ordinal))
            ? normalized
            : OverviewAllFilterLabel;
    }

    private static string NormalizeOverviewFilterAlias(string value)
    {
        if (IsOverviewFilterValue(value, "Overview.Filter.All", OverviewAllFilterLegacyCzech, OverviewAllFilterLegacyEnglish))
        {
            return OverviewAllFilterLabel;
        }

        if (IsOverviewFilterValue(value, "Overview.Filter.Technical", OverviewTechnicalFilterLegacyCzech, OverviewTechnicalFilterLegacyEnglish))
        {
            return LO("Overview.Filter.Technical");
        }

        if (IsOverviewFilterValue(value, "Overview.Filter.GreenCards", OverviewGreenCardsFilterLegacyCzech, OverviewGreenCardsFilterLegacyEnglish))
        {
            return LO("Overview.Filter.GreenCards");
        }

        if (IsOverviewFilterValue(value, "Overview.Filter.Reminders", OverviewRemindersFilterLegacyCzech, OverviewRemindersFilterLegacyEnglish))
        {
            return LO("Overview.Filter.Reminders");
        }

        if (IsOverviewFilterValue(value, "Overview.Filter.Records", OverviewRecordsFilterLegacyCzech, OverviewRecordsFilterLegacyEnglish))
        {
            return LO("Overview.Filter.Records");
        }

        if (IsOverviewFilterValue(value, "Overview.Filter.Maintenance", OverviewMaintenanceFilterLegacyCzech, OverviewMaintenanceFilterLegacyEnglish))
        {
            return LO("Overview.Filter.Maintenance");
        }

        if (IsOverviewFilterValue(value, "Overview.Filter.DataIssues", OverviewDataIssuesFilterLegacyCzech, OverviewDataIssuesFilterLegacyEnglish))
        {
            return OverviewDataIssueFilterLabel;
        }

        return value;
    }

    private static bool IsOverviewFilterValue(string value, string resourceKey, params string[] aliases) =>
        string.Equals(value, LO(resourceKey), StringComparison.OrdinalIgnoreCase)
        || aliases.Any(alias => string.Equals(value, alias, StringComparison.OrdinalIgnoreCase));

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
            UpcomingOverviewWorkspace.SelectedUpcomingOverviewDetail = LO("Overview.Detail.EmptyUpcoming");
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
            ? LO("Overview.Summary.OverdueEmpty")
            : LFO("Overview.Summary.OverdueWithItems", items.Count);

        OverdueOverviewWorkspace.SelectedOverdueOverviewItem = FindById(OverdueOverviewItems, BuildOverviewSelectionKey, previousKey) ?? OverdueOverviewItems.FirstOrDefault();
        if (OverdueOverviewWorkspace.SelectedOverdueOverviewItem is null)
        {
            OverdueOverviewWorkspace.SelectedOverdueOverviewDetail = LO("Overview.Detail.EmptyOverdue");
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
        var normalizedFilter = string.IsNullOrWhiteSpace(filter)
            ? OverviewAllFilterLabel
            : NormalizeOverviewFilterAlias(filter.Trim());

        if (IsOverviewFilterValue(normalizedFilter, "Overview.Filter.Technical", OverviewTechnicalFilterLegacyCzech, OverviewTechnicalFilterLegacyEnglish))
        {
            return item.Kind == "technical";
        }

        if (IsOverviewFilterValue(normalizedFilter, "Overview.Filter.GreenCards", OverviewGreenCardsFilterLegacyCzech, OverviewGreenCardsFilterLegacyEnglish))
        {
            return item.Kind == "green";
        }

        if (IsOverviewFilterValue(normalizedFilter, "Overview.Filter.Reminders", OverviewRemindersFilterLegacyCzech, OverviewRemindersFilterLegacyEnglish))
        {
            return item.Kind == "custom";
        }

        if (IsOverviewFilterValue(normalizedFilter, "Overview.Filter.Records", OverviewRecordsFilterLegacyCzech, OverviewRecordsFilterLegacyEnglish))
        {
            return item.Kind == "record";
        }

        if (IsOverviewFilterValue(normalizedFilter, "Overview.Filter.Maintenance", OverviewMaintenanceFilterLegacyCzech, OverviewMaintenanceFilterLegacyEnglish))
        {
            return item.Kind == "maintenance";
        }

        if (IsOverviewFilterValue(normalizedFilter, "Overview.Filter.DataIssues", OverviewDataIssuesFilterLegacyCzech, OverviewDataIssuesFilterLegacyEnglish))
        {
            return item.Kind == OverviewDataIssueKind;
        }

        return true;
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
                    LO("Timeline.Kind.GreenCard"),
                    OverviewMissingGreenDateLabel,
                    LO("Overview.MissingGreen.Title"),
                    BuildOverviewVehicleDetail(vehicle),
                    LO("Overview.MissingGreen.Status"),
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
                    LO("Overview.DataIssue.KindLabel"),
                    LO("Overview.DataIssue.Date"),
                    item.Title,
                    LFO("Overview.DataIssue.Detail", item.Category, item.Message),
                    item.Severity switch
                    {
                        AuditSeverity.Error => LO("Overview.DataIssue.Severity.Error"),
                        AuditSeverity.Warning => LO("Overview.DataIssue.Severity.Warning"),
                        _ => LO("Overview.DataIssue.Severity.Info")
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
                ? LO("Overview.Summary.UpcomingEmptyWithDataIssues")
                : LO("Overview.Summary.UpcomingEmpty")
            : LFO("Overview.Summary.UpcomingWithItems", items.Count);

        if (visibleDataIssueCount > 0)
        {
            summary += " " + LFO("Overview.Summary.UpcomingVisibleDataIssues", visibleDataIssueCount);
        }

        if (missingGreenCount > 0 && !UpcomingOverviewWorkspace.IncludeMissingGreenCardsInUpcomingOverview)
        {
            summary += " " + LFO("Overview.Summary.UpcomingMissingGreenCardsHidden", missingGreenCount);
        }

        if (dataIssueCount > 0 && !UpcomingOverviewWorkspace.IncludeDataIssuesInUpcomingOverview)
        {
            summary += " " + LFO("Overview.Summary.UpcomingDataIssuesHidden", dataIssueCount);
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
