using Vehimap.Application.Services;

namespace Vehimap.Desktop.ViewModels.Workspaces;

internal static class WorkspaceSortHelpers
{
    public const string DateSortLabel = "Datum";
    public const string TypeSortLabel = "Typ";
    public const string OdometerSortLabel = "Tachometr";
    public const string CostSortLabel = "Cena";
    public const string NoteSortLabel = "Poznámka";
    public const string FuelTypeSortLabel = "Palivo";
    public const string LitersSortLabel = "Litry";
    public const string TotalCostSortLabel = "Cena celkem";
    public const string TankStateSortLabel = "Stav nádrže";
    public const string TitleSortLabel = "Název";
    public const string DueDateSortLabel = "Termín";
    public const string StatusSortLabel = "Stav";
    public const string RepeatModeSortLabel = "Opakování";
    public const string IntervalSortLabel = "Interval";
    public const string LastServiceSortLabel = "Poslední servis";
    public const string ValiditySortLabel = "Platnost";
    public const string ProviderSortLabel = "Poskytovatel";
    public const string AttachmentModeSortLabel = "Režim přílohy";
    public const string AttachmentStateSortLabel = "Stav přílohy";

    public static IReadOnlyList<string> HistorySortOptions { get; } =
    [
        DateSortLabel,
        TypeSortLabel,
        OdometerSortLabel,
        CostSortLabel,
        NoteSortLabel
    ];

    public static IReadOnlyList<string> FuelSortOptions { get; } =
    [
        DateSortLabel,
        FuelTypeSortLabel,
        LitersSortLabel,
        TotalCostSortLabel,
        OdometerSortLabel,
        TankStateSortLabel
    ];

    public static IReadOnlyList<string> ReminderSortOptions { get; } =
    [
        DueDateSortLabel,
        TitleSortLabel,
        StatusSortLabel,
        RepeatModeSortLabel,
        NoteSortLabel
    ];

    public static IReadOnlyList<string> MaintenanceSortOptions { get; } =
    [
        TitleSortLabel,
        IntervalSortLabel,
        LastServiceSortLabel,
        StatusSortLabel,
        NoteSortLabel
    ];

    public static IReadOnlyList<string> RecordSortOptions { get; } =
    [
        ValiditySortLabel,
        TitleSortLabel,
        TypeSortLabel,
        ProviderSortLabel,
        CostSortLabel,
        AttachmentModeSortLabel,
        AttachmentStateSortLabel
    ];

    public static string NormalizeSortOption(string? value, IReadOnlyList<string> supportedOptions, string defaultOption)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultOption : value.Trim();
        return supportedOptions.Any(item => string.Equals(item, normalized, StringComparison.Ordinal))
            ? normalized
            : defaultOption;
    }

    public static IEnumerable<VehicleHistoryItemViewModel> SortHistory(
        IEnumerable<VehicleHistoryItemViewModel> items,
        string selectedOption,
        bool descending)
    {
        return NormalizeSortOption(selectedOption, HistorySortOptions, DateSortLabel) switch
        {
            TypeSortLabel => OrderByText(items, descending, item => item.EventType, item => item.Date),
            OdometerSortLabel => OrderByNumber(items, descending, item => TryParseOdometer(item.Odometer), item => item.Date),
            CostSortLabel => OrderByMoney(items, descending, item => TryParseMoney(item.Cost), item => item.Date),
            NoteSortLabel => OrderByText(items, descending, item => item.Note, item => item.Date),
            _ => OrderByDate(items, descending, item => TryParseDate(item.Date), item => item.EventType)
        };
    }

    public static IEnumerable<VehicleFuelItemViewModel> SortFuel(
        IEnumerable<VehicleFuelItemViewModel> items,
        string selectedOption,
        bool descending)
    {
        return NormalizeSortOption(selectedOption, FuelSortOptions, DateSortLabel) switch
        {
            FuelTypeSortLabel => OrderByText(items, descending, item => item.FuelType, item => item.Date),
            LitersSortLabel => OrderByMoney(items, descending, item => TryParseMoney(item.Liters), item => item.Date),
            TotalCostSortLabel => OrderByMoney(items, descending, item => TryParseMoney(item.TotalCost), item => item.Date),
            OdometerSortLabel => OrderByNumber(items, descending, item => TryParseOdometer(item.Odometer), item => item.Date),
            TankStateSortLabel => OrderByText(items, descending, item => item.TankState, item => item.Date),
            _ => OrderByDate(items, descending, item => TryParseDate(item.Date), item => item.FuelType)
        };
    }

    public static IEnumerable<VehicleReminderItemViewModel> SortReminders(
        IEnumerable<VehicleReminderItemViewModel> items,
        string selectedOption,
        bool descending)
    {
        return NormalizeSortOption(selectedOption, ReminderSortOptions, DueDateSortLabel) switch
        {
            TitleSortLabel => OrderByText(items, descending, item => item.Title, item => item.DueDate),
            StatusSortLabel => OrderByText(items, descending, item => item.Status, item => item.DueDate),
            RepeatModeSortLabel => OrderByText(items, descending, item => item.RepeatMode, item => item.DueDate),
            NoteSortLabel => OrderByText(items, descending, item => item.Note, item => item.DueDate),
            _ => OrderByDate(items, descending, item => TryParseDate(item.DueDate), item => item.Title)
        };
    }

    public static IEnumerable<VehicleMaintenanceItemViewModel> SortMaintenance(
        IEnumerable<VehicleMaintenanceItemViewModel> items,
        string selectedOption,
        bool descending)
    {
        return NormalizeSortOption(selectedOption, MaintenanceSortOptions, TitleSortLabel) switch
        {
            IntervalSortLabel => OrderByNumber(items, descending, item => TryParseFirstNumber(item.Interval), item => item.Title),
            LastServiceSortLabel => OrderByDate(items, descending, item => TryParseMaintenanceLastService(item.LastService), item => item.Title),
            StatusSortLabel => OrderByText(items, descending, item => item.Status, item => item.Title),
            NoteSortLabel => OrderByText(items, descending, item => item.Note, item => item.Title),
            _ => OrderByText(items, descending, item => item.Title, item => item.Status)
        };
    }

    public static IEnumerable<VehicleRecordItemViewModel> SortRecords(
        IEnumerable<VehicleRecordItemViewModel> items,
        string selectedOption,
        bool descending)
    {
        return NormalizeSortOption(selectedOption, RecordSortOptions, ValiditySortLabel) switch
        {
            TitleSortLabel => OrderByText(items, descending, item => item.Title, item => item.Validity),
            TypeSortLabel => OrderByText(items, descending, item => item.RecordType, item => item.Validity),
            ProviderSortLabel => OrderByText(items, descending, item => item.Provider, item => item.Validity),
            CostSortLabel => OrderByMoney(items, descending, item => TryParseMoney(item.Price), item => item.Validity),
            AttachmentModeSortLabel => OrderByText(items, descending, item => item.AttachmentMode, item => item.Validity),
            AttachmentStateSortLabel => OrderByText(items, descending, item => item.AttachmentState, item => item.Validity),
            _ => OrderByDate(items, descending, item => TryParseRecordValidity(item.Validity), item => item.Title)
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
}
