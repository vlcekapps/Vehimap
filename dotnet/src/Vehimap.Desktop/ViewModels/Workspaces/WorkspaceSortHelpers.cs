// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels.Workspaces;

internal static class WorkspaceSortHelpers
{
    internal const string DateSortKey = "date";
    internal const string TypeSortKey = "type";
    internal const string OdometerSortKey = "odometer";
    internal const string CostSortKey = "cost";
    internal const string NoteSortKey = "note";
    internal const string FuelTypeSortKey = "fuel_type";
    internal const string FuelDetailSortKey = "fuel_detail";
    internal const string FuelStationSortKey = "fuel_station";
    internal const string LitersSortKey = "fuel_volume";
    internal const string TotalCostSortKey = "total_cost";
    internal const string TankStateSortKey = "tank_state";
    internal const string TitleSortKey = "title";
    internal const string VehicleSortKey = "vehicle";
    internal const string DueDateSortKey = "due_date";
    internal const string StatusSortKey = "status";
    internal const string RepeatModeSortKey = "repeat_mode";
    internal const string IntervalSortKey = "interval";
    internal const string LastServiceSortKey = "last_service";
    internal const string ValiditySortKey = "validity";
    internal const string ProviderSortKey = "provider";
    internal const string AttachmentModeSortKey = "attachment_mode";
    internal const string AttachmentStateSortKey = "attachment_state";
    internal const string SeveritySortKey = "severity";
    internal const string CategorySortKey = "category";
    internal const string SummarySortKey = "summary";

    private static readonly SortOptionDefinition[] SortDefinitions =
    [
        new(DateSortKey, "WorkspaceSort.Date", "Datum", "Date"),
        new(TypeSortKey, "WorkspaceSort.Type", "Typ", "Type"),
        new(OdometerSortKey, "WorkspaceSort.Odometer", "Tachometr", "Odometer"),
        new(CostSortKey, "WorkspaceSort.Cost", "Cena", "Cost"),
        new(NoteSortKey, "WorkspaceSort.Note", "Poznámka", "Note"),
        new(FuelTypeSortKey, "WorkspaceSort.FuelType", "Palivo", "Fuel"),
        new(FuelDetailSortKey, "WorkspaceSort.FuelDetail", "Detail paliva", "Fuel detail"),
        new(FuelStationSortKey, "WorkspaceSort.FuelStation", "Místo tankování", "Fuel station"),
        new(LitersSortKey, "WorkspaceSort.FuelVolume", "Litry", "Fuel volume"),
        new(TotalCostSortKey, "WorkspaceSort.TotalCost", "Cena celkem", "Total cost"),
        new(TankStateSortKey, "WorkspaceSort.TankState", "Stav nádrže", "Tank state"),
        new(TitleSortKey, "WorkspaceSort.Title", "Název", "Title"),
        new(VehicleSortKey, "WorkspaceSort.Vehicle", "Vozidlo", "Vehicle"),
        new(DueDateSortKey, "WorkspaceSort.DueDate", "Termín", "Due date"),
        new(StatusSortKey, "WorkspaceSort.Status", "Stav", "Status"),
        new(RepeatModeSortKey, "WorkspaceSort.RepeatMode", "Opakování", "Repeat"),
        new(IntervalSortKey, "WorkspaceSort.Interval", "Interval", "Interval"),
        new(LastServiceSortKey, "WorkspaceSort.LastService", "Poslední servis", "Last service"),
        new(ValiditySortKey, "WorkspaceSort.Validity", "Platnost", "Validity"),
        new(ProviderSortKey, "WorkspaceSort.Provider", "Poskytovatel", "Provider"),
        new(AttachmentModeSortKey, "WorkspaceSort.AttachmentMode", "Režim přílohy", "Attachment mode"),
        new(AttachmentStateSortKey, "WorkspaceSort.AttachmentState", "Stav přílohy", "Attachment status"),
        new(SeveritySortKey, "WorkspaceSort.Severity", "Závažnost", "Severity"),
        new(CategorySortKey, "WorkspaceSort.Category", "Evidence", "Record area"),
        new(SummarySortKey, "WorkspaceSort.Summary", "Souhrn", "Summary")
    ];

    public static LocalizedOptionViewModel DateSortOption => GetSortOption(DateSortKey);
    public static LocalizedOptionViewModel TypeSortOption => GetSortOption(TypeSortKey);
    public static LocalizedOptionViewModel OdometerSortOption => GetSortOption(OdometerSortKey);
    public static LocalizedOptionViewModel CostSortOption => GetSortOption(CostSortKey);
    public static LocalizedOptionViewModel DueDateSortOption => GetSortOption(DueDateSortKey);
    public static LocalizedOptionViewModel TitleSortOption => GetSortOption(TitleSortKey);
    public static LocalizedOptionViewModel VehicleSortOption => GetSortOption(VehicleSortKey);
    public static LocalizedOptionViewModel StatusSortOption => GetSortOption(StatusSortKey);
    public static LocalizedOptionViewModel IntervalSortOption => GetSortOption(IntervalSortKey);
    public static LocalizedOptionViewModel ValiditySortOption => GetSortOption(ValiditySortKey);
    public static LocalizedOptionViewModel SeveritySortOption => GetSortOption(SeveritySortKey);

    public static IReadOnlyList<LocalizedOptionViewModel> HistorySortOptions =>
        BuildOptions(DateSortKey, TypeSortKey, OdometerSortKey, CostSortKey, NoteSortKey);

    public static IReadOnlyList<LocalizedOptionViewModel> FuelSortOptions =>
        BuildOptions(DateSortKey, FuelTypeSortKey, FuelDetailSortKey, FuelStationSortKey, LitersSortKey, TotalCostSortKey, OdometerSortKey, TankStateSortKey);

    public static IReadOnlyList<LocalizedOptionViewModel> ReminderSortOptions =>
        BuildOptions(DueDateSortKey, TitleSortKey, StatusSortKey, RepeatModeSortKey, NoteSortKey);

    public static IReadOnlyList<LocalizedOptionViewModel> MaintenanceSortOptions =>
        BuildOptions(TitleSortKey, IntervalSortKey, LastServiceSortKey, StatusSortKey, NoteSortKey);

    public static IReadOnlyList<LocalizedOptionViewModel> RecordSortOptions =>
        BuildOptions(ValiditySortKey, TitleSortKey, TypeSortKey, ProviderSortKey, CostSortKey, AttachmentModeSortKey, AttachmentStateSortKey);

    public static IReadOnlyList<LocalizedOptionViewModel> TimelineOverviewSortOptions =>
        BuildOptions(DateSortKey, TypeSortKey, VehicleSortKey, TitleSortKey, StatusSortKey);

    public static IReadOnlyList<LocalizedOptionViewModel> AuditSortOptions =>
        BuildOptions(SeveritySortKey, VehicleSortKey, TitleSortKey, CategorySortKey, TypeSortKey);

    public static IReadOnlyList<LocalizedOptionViewModel> GlobalSearchSortOptions =>
        BuildOptions(TypeSortKey, VehicleSortKey, TitleSortKey, SummarySortKey);

    public static LocalizedOptionViewModel NormalizeSortOption(string? value, IReadOnlyList<LocalizedOptionViewModel> supportedOptions, LocalizedOptionViewModel defaultOption)
    {
        var selectedKey = NormalizeSortKey(value, supportedOptions, defaultOption);
        return GetSortOption(selectedKey);
    }

    public static LocalizedOptionViewModel NormalizeSortOption(LocalizedOptionViewModel? value, IReadOnlyList<LocalizedOptionViewModel> supportedOptions, LocalizedOptionViewModel defaultOption) =>
        NormalizeSortOption(value?.Value, supportedOptions, defaultOption);

    public static string NormalizeSortKey(LocalizedOptionViewModel? value, IReadOnlyList<LocalizedOptionViewModel> supportedOptions, LocalizedOptionViewModel defaultOption) =>
        NormalizeSortKey(value?.Value, supportedOptions, defaultOption);

    public static string NormalizeSortKey(string? value, IReadOnlyList<LocalizedOptionViewModel> supportedOptions, LocalizedOptionViewModel defaultOption)
    {
        var defaultKey = TryGetSortKey(defaultOption.Value) ?? DateSortKey;
        var selectedKey = TryGetSortKey(value);
        var supportedKeys = supportedOptions
            .Select(option => option.Value)
            .ToHashSet(StringComparer.Ordinal);

        return selectedKey is not null && supportedKeys.Contains(selectedKey) ? selectedKey : defaultKey;
    }

    private static IReadOnlyList<LocalizedOptionViewModel> BuildOptions(params string[] keys) =>
        keys.Select(GetSortOption).ToList();

    private static LocalizedOptionViewModel GetSortOption(string key) =>
        new(key, GetSortLabel(key));

    private static string GetSortLabel(string key) =>
        DesktopLocalization.Localizer.GetString(GetSortDefinition(key).ResourceKey);

    private static string? TryGetSortKey(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        foreach (var definition in SortDefinitions)
        {
            if (string.Equals(normalized, definition.Key, StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, definition.LegacyLabel, StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, definition.EnglishLabel, StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, DesktopLocalization.Localizer.GetString(definition.ResourceKey), StringComparison.OrdinalIgnoreCase))
            {
                return definition.Key;
            }
        }

        return null;
    }

    private static SortOptionDefinition GetSortDefinition(string key) =>
        SortDefinitions.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.Ordinal))
        ?? SortDefinitions.First(item => string.Equals(item.Key, DateSortKey, StringComparison.Ordinal));

    public static IEnumerable<VehicleHistoryItemViewModel> SortHistory(
        IEnumerable<VehicleHistoryItemViewModel> items,
        LocalizedOptionViewModel? selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, HistorySortOptions, DateSortOption) switch
        {
            TypeSortKey => OrderByText(items, descending, item => item.EventType, item => item.Date),
            OdometerSortKey => OrderByNumber(items, descending, item => TryParseOdometer(item.Odometer), item => item.Date),
            CostSortKey => OrderByMoney(items, descending, item => TryParseMoney(item.Cost), item => item.Date),
            NoteSortKey => OrderByText(items, descending, item => item.Note, item => item.Date),
            _ => OrderByDate(items, descending, item => TryParseDate(item.Date), item => item.EventType)
        };
    }

    public static IEnumerable<VehicleFuelItemViewModel> SortFuel(
        IEnumerable<VehicleFuelItemViewModel> items,
        LocalizedOptionViewModel? selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, FuelSortOptions, DateSortOption) switch
        {
            FuelTypeSortKey => OrderByText(items, descending, item => item.FuelType, item => item.Date),
            FuelDetailSortKey => OrderByText(items, descending, item => item.FuelDetail, item => item.Date),
            FuelStationSortKey => OrderByText(items, descending, item => item.Station, item => item.Date),
            LitersSortKey => OrderByMoney(items, descending, item => TryParseMoney(item.Liters), item => item.Date),
            TotalCostSortKey => OrderByMoney(items, descending, item => TryParseMoney(item.TotalCost), item => item.Date),
            OdometerSortKey => OrderByNumber(items, descending, item => TryParseOdometer(item.Odometer), item => item.Date),
            TankStateSortKey => OrderByText(items, descending, item => item.TankState, item => item.Date),
            _ => OrderByDate(items, descending, item => TryParseDate(item.Date), item => item.FuelType)
        };
    }

    public static IEnumerable<VehicleReminderItemViewModel> SortReminders(
        IEnumerable<VehicleReminderItemViewModel> items,
        LocalizedOptionViewModel? selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, ReminderSortOptions, DueDateSortOption) switch
        {
            TitleSortKey => OrderByText(items, descending, item => item.Title, item => item.DueDate),
            StatusSortKey => OrderByText(items, descending, item => item.Status, item => item.DueDate),
            RepeatModeSortKey => OrderByText(items, descending, item => item.RepeatMode, item => item.DueDate),
            NoteSortKey => OrderByText(items, descending, item => item.Note, item => item.DueDate),
            _ => OrderByDate(items, descending, item => TryParseDate(item.DueDate), item => item.Title)
        };
    }

    public static IEnumerable<VehicleMaintenanceItemViewModel> SortMaintenance(
        IEnumerable<VehicleMaintenanceItemViewModel> items,
        LocalizedOptionViewModel? selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, MaintenanceSortOptions, TitleSortOption) switch
        {
            IntervalSortKey => OrderByNumber(items, descending, item => TryParseFirstNumber(item.Interval), item => item.Title),
            LastServiceSortKey => OrderByDate(items, descending, item => TryParseMaintenanceLastService(item.LastService), item => item.Title),
            StatusSortKey => OrderByText(items, descending, item => item.Status, item => item.Title),
            NoteSortKey => OrderByText(items, descending, item => item.Note, item => item.Title),
            _ => OrderByText(items, descending, item => item.Title, item => item.Status)
        };
    }

    public static IEnumerable<VehicleRecordItemViewModel> SortRecords(
        IEnumerable<VehicleRecordItemViewModel> items,
        LocalizedOptionViewModel? selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, RecordSortOptions, ValiditySortOption) switch
        {
            TitleSortKey => OrderByText(items, descending, item => item.Title, item => item.Validity),
            TypeSortKey => OrderByText(items, descending, item => item.RecordType, item => item.Validity),
            ProviderSortKey => OrderByText(items, descending, item => item.Provider, item => item.Validity),
            CostSortKey => OrderByMoney(items, descending, item => TryParseMoney(item.Price), item => item.Validity),
            AttachmentModeSortKey => OrderByText(items, descending, item => item.AttachmentMode, item => item.Validity),
            AttachmentStateSortKey => OrderByText(items, descending, item => item.AttachmentState, item => item.Validity),
            _ => OrderByDate(items, descending, item => TryParseRecordValidity(item.Validity), item => item.Title)
        };
    }

    public static IEnumerable<VehicleTimelineItemViewModel> SortTimelineOverview(
        IEnumerable<VehicleTimelineItemViewModel> items,
        LocalizedOptionViewModel? selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, TimelineOverviewSortOptions, DateSortOption) switch
        {
            TypeSortKey => OrderByText(items, descending, item => item.KindLabel, item => item.Date),
            VehicleSortKey => OrderByText(items, descending, item => item.VehicleName, item => item.Date),
            TitleSortKey => OrderByText(items, descending, item => item.Title, item => item.Date),
            StatusSortKey => OrderByText(items, descending, item => item.Status, item => item.Date),
            _ => OrderByDate(items, descending, item => TryParseDate(item.Date), item => item.VehicleName)
        };
    }

    public static IEnumerable<AuditItemViewModel> SortAudit(
        IEnumerable<AuditItemViewModel> items,
        LocalizedOptionViewModel? selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, AuditSortOptions, SeveritySortOption) switch
        {
            VehicleSortKey => OrderByText(items, descending, item => item.VehicleName, item => item.Title),
            TitleSortKey => OrderByText(items, descending, item => item.Title, item => item.VehicleName),
            CategorySortKey => OrderByText(items, descending, item => item.Category, item => item.VehicleName),
            TypeSortKey => OrderByText(items, descending, item => item.EntityKind, item => item.VehicleName),
            _ => OrderBySeverity(items, descending, item => item.Severity, item => item.VehicleName)
        };
    }

    public static IEnumerable<GlobalSearchResultItemViewModel> SortGlobalSearch(
        IEnumerable<GlobalSearchResultItemViewModel> items,
        LocalizedOptionViewModel? selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, GlobalSearchSortOptions, TypeSortOption) switch
        {
            VehicleSortKey => OrderByText(items, descending, item => item.VehicleName, item => item.Title),
            TitleSortKey => OrderByText(items, descending, item => item.Title, item => item.VehicleName),
            SummarySortKey => OrderByText(items, descending, item => item.Summary, item => item.VehicleName),
            _ => OrderByText(items, descending, item => item.SectionLabel, item => item.VehicleName)
        };
    }

    private static IEnumerable<T> OrderByText<T>(
        IEnumerable<T> items,
        bool descending,
        Func<T, string> keySelector,
        Func<T, string> secondarySelector)
    {
        return descending
            ? items.OrderByDescending(keySelector, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(secondarySelector, StringComparer.CurrentCultureIgnoreCase)
            : items.OrderBy(keySelector, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(secondarySelector, StringComparer.CurrentCultureIgnoreCase);
    }

    private static IEnumerable<T> OrderByDate<T>(
        IEnumerable<T> items,
        bool descending,
        Func<T, DateOnly?> keySelector,
        Func<T, string> secondarySelector)
    {
        return descending
            ? items.OrderByDescending(item => keySelector(item).HasValue)
                .ThenByDescending(item => keySelector(item) ?? DateOnly.MinValue)
                .ThenBy(secondarySelector, StringComparer.CurrentCultureIgnoreCase)
            : items.OrderBy(item => keySelector(item).HasValue ? 0 : 1)
                .ThenBy(item => keySelector(item) ?? DateOnly.MaxValue)
                .ThenBy(secondarySelector, StringComparer.CurrentCultureIgnoreCase);
    }

    private static IEnumerable<T> OrderByNumber<T>(
        IEnumerable<T> items,
        bool descending,
        Func<T, int?> keySelector,
        Func<T, string> secondarySelector)
    {
        return descending
            ? items.OrderByDescending(item => keySelector(item).HasValue)
                .ThenByDescending(item => keySelector(item) ?? int.MinValue)
                .ThenBy(secondarySelector, StringComparer.CurrentCultureIgnoreCase)
            : items.OrderBy(item => keySelector(item).HasValue ? 0 : 1)
                .ThenBy(item => keySelector(item) ?? int.MaxValue)
                .ThenBy(secondarySelector, StringComparer.CurrentCultureIgnoreCase);
    }

    private static IEnumerable<T> OrderByMoney<T>(
        IEnumerable<T> items,
        bool descending,
        Func<T, decimal?> keySelector,
        Func<T, string> secondarySelector)
    {
        return descending
            ? items.OrderByDescending(item => keySelector(item).HasValue)
                .ThenByDescending(item => keySelector(item) ?? decimal.MinValue)
                .ThenBy(secondarySelector, StringComparer.CurrentCultureIgnoreCase)
            : items.OrderBy(item => keySelector(item).HasValue ? 0 : 1)
                .ThenBy(item => keySelector(item) ?? decimal.MaxValue)
                .ThenBy(secondarySelector, StringComparer.CurrentCultureIgnoreCase);
    }

    private static IEnumerable<T> OrderBySeverity<T>(
        IEnumerable<T> items,
        bool descending,
        Func<T, string> keySelector,
        Func<T, string> secondarySelector)
    {
        return descending
            ? items.OrderByDescending(item => GetSeverityRank(keySelector(item)))
                .ThenBy(secondarySelector, StringComparer.CurrentCultureIgnoreCase)
            : items.OrderBy(item => GetSeverityRank(keySelector(item)))
                .ThenBy(secondarySelector, StringComparer.CurrentCultureIgnoreCase);
    }

    private static int GetSeverityRank(string? severity)
    {
        if (string.IsNullOrWhiteSpace(severity))
        {
            return 3;
        }

        return severity.Trim().ToLowerInvariant() switch
        {
            "chyba" or "error" => 0,
            "varování" or "warning" => 1,
            "upozornění" or "info" or "informace" => 2,
            _ => 3
        };
    }

    private static DateOnly? TryParseDate(string? value)
    {
        return VehimapValueParser.TryParseEventDate(value, out var eventDate)
            || VehimapValueParser.TryParseMonthYear(value, out eventDate)
            ? eventDate
            : null;
    }

    private static DateOnly? TryParseRecordValidity(string? value)
    {
        var parts = (value ?? string.Empty)
            .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var prefix in new[] { "do ", "od " })
        {
            foreach (var part in parts)
            {
                if (part.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase)
                    && VehimapValueParser.TryParseMonthYear(part[prefix.Length..], out var date))
                {
                    return date;
                }
            }
        }

        return TryParseDate(value);
    }

    private static DateOnly? TryParseMaintenanceLastService(string? value)
    {
        var datePart = (value ?? string.Empty).Split('|', 2, StringSplitOptions.TrimEntries)[0];
        return TryParseDate(datePart);
    }

    private static int? TryParseOdometer(string? value) =>
        VehimapValueParser.TryParseOdometer(value, out var parsed) ? parsed : null;

    private static int? TryParseFirstNumber(string? value)
    {
        var token = (value ?? string.Empty)
            .Split([' ', '/', '|'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(item => item.Any(char.IsDigit));

        return token is not null && int.TryParse(new string(token.Where(char.IsDigit).ToArray()), out var parsed)
            ? parsed
            : null;
    }

    private static decimal? TryParseMoney(string? value) =>
        VehimapValueParser.TryParseMoney(value, out var parsed) ? parsed : null;

    private sealed record SortOptionDefinition(string Key, string ResourceKey, string LegacyLabel, string EnglishLabel);
}
