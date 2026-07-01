// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels.Workspaces;

internal static class WorkspaceSortHelpers
{
    private const string DateSortKey = "date";
    private const string TypeSortKey = "type";
    private const string OdometerSortKey = "odometer";
    private const string CostSortKey = "cost";
    private const string NoteSortKey = "note";
    private const string FuelTypeSortKey = "fuel_type";
    private const string FuelDetailSortKey = "fuel_detail";
    private const string FuelStationSortKey = "fuel_station";
    private const string LitersSortKey = "fuel_volume";
    private const string TotalCostSortKey = "total_cost";
    private const string TankStateSortKey = "tank_state";
    private const string TitleSortKey = "title";
    private const string VehicleSortKey = "vehicle";
    private const string DueDateSortKey = "due_date";
    private const string StatusSortKey = "status";
    private const string RepeatModeSortKey = "repeat_mode";
    private const string IntervalSortKey = "interval";
    private const string LastServiceSortKey = "last_service";
    private const string ValiditySortKey = "validity";
    private const string ProviderSortKey = "provider";
    private const string AttachmentModeSortKey = "attachment_mode";
    private const string AttachmentStateSortKey = "attachment_state";
    private const string SeveritySortKey = "severity";
    private const string CategorySortKey = "category";
    private const string SummarySortKey = "summary";

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

    public static string DateSortLabel => GetSortLabel(DateSortKey);
    public static string TypeSortLabel => GetSortLabel(TypeSortKey);
    public static string OdometerSortLabel => GetSortLabel(OdometerSortKey);
    public static string CostSortLabel => GetSortLabel(CostSortKey);
    public static string NoteSortLabel => GetSortLabel(NoteSortKey);
    public static string FuelTypeSortLabel => GetSortLabel(FuelTypeSortKey);
    public static string FuelDetailSortLabel => GetSortLabel(FuelDetailSortKey);
    public static string FuelStationSortLabel => GetSortLabel(FuelStationSortKey);
    public static string LitersSortLabel => GetSortLabel(LitersSortKey);
    public static string TotalCostSortLabel => GetSortLabel(TotalCostSortKey);
    public static string TankStateSortLabel => GetSortLabel(TankStateSortKey);
    public static string TitleSortLabel => GetSortLabel(TitleSortKey);
    public static string VehicleSortLabel => GetSortLabel(VehicleSortKey);
    public static string DueDateSortLabel => GetSortLabel(DueDateSortKey);
    public static string StatusSortLabel => GetSortLabel(StatusSortKey);
    public static string RepeatModeSortLabel => GetSortLabel(RepeatModeSortKey);
    public static string IntervalSortLabel => GetSortLabel(IntervalSortKey);
    public static string LastServiceSortLabel => GetSortLabel(LastServiceSortKey);
    public static string ValiditySortLabel => GetSortLabel(ValiditySortKey);
    public static string ProviderSortLabel => GetSortLabel(ProviderSortKey);
    public static string AttachmentModeSortLabel => GetSortLabel(AttachmentModeSortKey);
    public static string AttachmentStateSortLabel => GetSortLabel(AttachmentStateSortKey);
    public static string SeveritySortLabel => GetSortLabel(SeveritySortKey);
    public static string CategorySortLabel => GetSortLabel(CategorySortKey);
    public static string SummarySortLabel => GetSortLabel(SummarySortKey);

    public static IReadOnlyList<string> HistorySortOptions =>
    [
        DateSortLabel,
        TypeSortLabel,
        OdometerSortLabel,
        CostSortLabel,
        NoteSortLabel
    ];

    public static IReadOnlyList<string> FuelSortOptions =>
    [
        DateSortLabel,
        FuelTypeSortLabel,
        FuelDetailSortLabel,
        FuelStationSortLabel,
        LitersSortLabel,
        TotalCostSortLabel,
        OdometerSortLabel,
        TankStateSortLabel
    ];

    public static IReadOnlyList<string> ReminderSortOptions =>
    [
        DueDateSortLabel,
        TitleSortLabel,
        StatusSortLabel,
        RepeatModeSortLabel,
        NoteSortLabel
    ];

    public static IReadOnlyList<string> MaintenanceSortOptions =>
    [
        TitleSortLabel,
        IntervalSortLabel,
        LastServiceSortLabel,
        StatusSortLabel,
        NoteSortLabel
    ];

    public static IReadOnlyList<string> RecordSortOptions =>
    [
        ValiditySortLabel,
        TitleSortLabel,
        TypeSortLabel,
        ProviderSortLabel,
        CostSortLabel,
        AttachmentModeSortLabel,
        AttachmentStateSortLabel
    ];

    public static IReadOnlyList<string> TimelineOverviewSortOptions =>
    [
        DateSortLabel,
        TypeSortLabel,
        VehicleSortLabel,
        TitleSortLabel,
        StatusSortLabel
    ];

    public static IReadOnlyList<string> AuditSortOptions =>
    [
        SeveritySortLabel,
        VehicleSortLabel,
        TitleSortLabel,
        CategorySortLabel,
        TypeSortLabel
    ];

    public static IReadOnlyList<string> GlobalSearchSortOptions =>
    [
        TypeSortLabel,
        VehicleSortLabel,
        TitleSortLabel,
        SummarySortLabel
    ];

    public static string NormalizeSortOption(string? value, IReadOnlyList<string> supportedOptions, string defaultOption)
    {
        var defaultKey = TryGetSortKey(defaultOption) ?? DateSortKey;
        var selectedKey = TryGetSortKey(value);
        var supportedKeys = supportedOptions
            .Select(TryGetSortKey)
            .Where(key => key is not null)
            .ToHashSet(StringComparer.Ordinal);

        return selectedKey is not null && supportedKeys.Contains(selectedKey)
            ? GetSortLabel(selectedKey)
            : GetSortLabel(defaultKey);
    }

    private static string NormalizeSortKey(string? value, IReadOnlyList<string> supportedOptions, string defaultOption)
    {
        var defaultKey = TryGetSortKey(defaultOption) ?? DateSortKey;
        var selectedKey = TryGetSortKey(value);
        var supportedKeys = supportedOptions
            .Select(TryGetSortKey)
            .Where(key => key is not null)
            .ToHashSet(StringComparer.Ordinal);

        return selectedKey is not null && supportedKeys.Contains(selectedKey) ? selectedKey : defaultKey;
    }

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
        string selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, HistorySortOptions, DateSortLabel) switch
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
        string selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, FuelSortOptions, DateSortLabel) switch
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
        string selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, ReminderSortOptions, DueDateSortLabel) switch
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
        string selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, MaintenanceSortOptions, TitleSortLabel) switch
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
        string selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, RecordSortOptions, ValiditySortLabel) switch
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
        string selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, TimelineOverviewSortOptions, DateSortLabel) switch
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
        string selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, AuditSortOptions, SeveritySortLabel) switch
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
        string selectedOption,
        bool descending)
    {
        return NormalizeSortKey(selectedOption, GlobalSearchSortOptions, TypeSortLabel) switch
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
