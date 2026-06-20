using System.Globalization;
using Vehimap.Application;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    internal const string CostPeriodYearToDateLabel = "Od začátku roku";
    internal const string CostPeriodLast30DaysLabel = "Posledních 30 dní";
    internal const string CostPeriodLast90DaysLabel = "Posledních 90 dní";
    internal const string CostPeriodCurrentYearLabel = "Aktuální rok";
    internal const string CostPeriodPreviousYearLabel = "Minulý rok";
    internal const string CostPeriodCustomLabel = "Vlastní období";

    private const string CostPeriodSettingsSection = "costs";
    private const string CostPeriodPresetSettingKey = "period_preset";
    private const string CostPeriodStartSettingKey = "period_start";
    private const string CostPeriodEndSettingKey = "period_end";
    private static readonly CultureInfo CzechCulture = CultureInfo.GetCultureInfo("cs-CZ");

    private bool _suppressCostPeriodRefresh;
    private DateOnly _costPeriodStart;
    private DateOnly _costPeriodEnd;

    public IReadOnlyList<string> CostPeriodPresets { get; } =
    [
        CostPeriodYearToDateLabel,
        CostPeriodLast30DaysLabel,
        CostPeriodLast90DaysLabel,
        CostPeriodCurrentYearLabel,
        CostPeriodPreviousYearLabel,
        CostPeriodCustomLabel
    ];

    private void ApplyCostPeriodPreferences()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var preset = NormalizeCostPeriodPreset(_dataSet.Settings.GetValue(CostPeriodSettingsSection, CostPeriodPresetSettingKey, CostPeriodYearToDateLabel));
        var (start, end) = BuildCostPeriodForPreset(preset, today);

        if (string.Equals(preset, CostPeriodCustomLabel, StringComparison.Ordinal))
        {
            if (TryParseCostDate(_dataSet.Settings.GetValue(CostPeriodSettingsSection, CostPeriodStartSettingKey, string.Empty), out var customStart))
            {
                start = customStart;
            }

            if (TryParseCostDate(_dataSet.Settings.GetValue(CostPeriodSettingsSection, CostPeriodEndSettingKey, string.Empty), out var customEnd))
            {
                end = customEnd;
            }

            if (end < start)
            {
                (start, end) = (end, start);
            }
        }

        _costPeriodStart = start;
        _costPeriodEnd = end;
        SetCostPeriodWorkspaceState(preset, start, end, BuildCostPeriodStatus(preset, start, end));
    }

    internal void HandleCostPeriodPresetChanged(string value)
    {
        if (_suppressCostPeriodRefresh || !_session.IsLoaded)
        {
            return;
        }

        var preset = NormalizeCostPeriodPreset(value);
        if (!string.Equals(CostWorkspace.SelectedCostPeriodPreset, preset, StringComparison.Ordinal))
        {
            _suppressCostPeriodRefresh = true;
            try
            {
                CostWorkspace.SelectedCostPeriodPreset = preset;
            }
            finally
            {
                _suppressCostPeriodRefresh = false;
            }
        }

        if (string.Equals(preset, CostPeriodCustomLabel, StringComparison.Ordinal))
        {
            CostWorkspace.CostPeriodStatus = "Zadejte začátek a konec vlastního období a použijte tlačítko Přepočítat.";
            return;
        }

        var (start, end) = BuildCostPeriodForPreset(preset, DateOnly.FromDateTime(DateTime.Today));
        ApplyCostPeriodSelection(preset, start, end, persist: true, requestFocus: false, "Období nákladů bylo přepnuto.");
    }

    internal void HandleCostPeriodCustomDateChanged()
    {
        if (_suppressCostPeriodRefresh || !_session.IsLoaded)
        {
            return;
        }

        if (!string.Equals(CostWorkspace.SelectedCostPeriodPreset, CostPeriodCustomLabel, StringComparison.Ordinal))
        {
            _suppressCostPeriodRefresh = true;
            try
            {
                CostWorkspace.SelectedCostPeriodPreset = CostPeriodCustomLabel;
            }
            finally
            {
                _suppressCostPeriodRefresh = false;
            }
        }

        CostWorkspace.CostPeriodStatus = "Vlastní období bude použito po tlačítku Přepočítat.";
    }

    internal void ApplyCostPeriodFromWorkspace()
    {
        if (!_session.IsLoaded)
        {
            return;
        }

        if (!TryParseCostDate(CostWorkspace.CostPeriodStartText, out var start))
        {
            CostWorkspace.CostPeriodStatus = "Začátek období není platné datum. Zadejte například 01.01.2026.";
            ShellStatus = CostWorkspace.CostPeriodStatus;
            RequestFocus(DesktopFocusTarget.CostPeriodStart);
            return;
        }

        if (!TryParseCostDate(CostWorkspace.CostPeriodEndText, out var end))
        {
            CostWorkspace.CostPeriodStatus = "Konec období není platné datum. Zadejte například 31.12.2026.";
            ShellStatus = CostWorkspace.CostPeriodStatus;
            RequestFocus(DesktopFocusTarget.CostPeriodStart);
            return;
        }

        var status = "Období nákladů bylo použito.";
        if (end < start)
        {
            (start, end) = (end, start);
            status = "Období nákladů bylo použito; začátek a konec byly prohozeny do správného pořadí.";
        }

        ApplyCostPeriodSelection(
            NormalizeCostPeriodPreset(CostWorkspace.SelectedCostPeriodPreset),
            start,
            end,
            persist: true,
            requestFocus: true,
            status);
    }

    private CostAnalysisSummary BuildSelectedCostSummary()
    {
        if (_costPeriodStart == default || _costPeriodEnd == default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            (_costPeriodStart, _costPeriodEnd) = BuildCostPeriodForPreset(CostPeriodYearToDateLabel, today);
        }

        return _session.BuildCostSummary(_costPeriodStart, _costPeriodEnd);
    }

    private void RefreshCostWorkspaceFromSelectedPeriod(string status, bool requestFocus)
    {
        var previousCostVehicleId = CostWorkspace.SelectedDashboardCostVehicle?.VehicleId ?? string.Empty;
        _currentCostSummary = BuildSelectedCostSummary();

        CostWorkspace.CostSummary = _projectionService.BuildCostSummary(_currentCostSummary);
        CostWorkspace.CostComparison = _projectionService.BuildCostComparison(_currentCostSummary);

        CostVehicles.Clear();
        foreach (var row in _projectionService.BuildDashboardCostVehicles(_currentCostSummary))
        {
            CostVehicles.Add(row);
        }

        CostWorkspace.SelectedDashboardCostVehicle = FindById(CostVehicles, item => item.VehicleId, previousCostVehicleId);
        CostWorkspace.RefreshVisibleCostVehicles();
        DashboardWorkspace.NotifyDashboardSummariesChanged();
        ExportFleetCostSummaryCommand.NotifyCanExecuteChanged();
        ExportSelectedVehicleCostDetailCommand.NotifyCanExecuteChanged();
        ExportSelectedVehicleCostReportCommand.NotifyCanExecuteChanged();

        CostExportStatus = status;
        CostWorkspace.CostPeriodStatus = $"{status} {BuildCostPeriodStatus(CostWorkspace.SelectedCostPeriodPreset, _costPeriodStart, _costPeriodEnd)}";
        ShellStatus = status;

        if (requestFocus)
        {
            RequestFocus(CostWorkspace.VisibleCostVehicles.Count == 0 ? DesktopFocusTarget.CostSearch : DesktopFocusTarget.CostList);
        }
    }

    private void ApplyCostPeriodSelection(string preset, DateOnly start, DateOnly end, bool persist, bool requestFocus, string status)
    {
        _costPeriodStart = start;
        _costPeriodEnd = end;
        SetCostPeriodWorkspaceState(preset, start, end, BuildCostPeriodStatus(preset, start, end));

        if (persist)
        {
            PersistCostPeriodPreferencesAsync(preset, start, end);
        }

        RefreshCostWorkspaceFromSelectedPeriod(status, requestFocus);
    }

    private void SetCostPeriodWorkspaceState(string preset, DateOnly start, DateOnly end, string status)
    {
        _suppressCostPeriodRefresh = true;
        try
        {
            CostWorkspace.SelectedCostPeriodPreset = preset;
            CostWorkspace.CostPeriodStartText = FormatCostDate(start);
            CostWorkspace.CostPeriodEndText = FormatCostDate(end);
            CostWorkspace.CostPeriodStatus = status;
        }
        finally
        {
            _suppressCostPeriodRefresh = false;
        }
    }

    private void PersistCostPeriodPreferencesAsync(string preset, DateOnly start, DateOnly end)
    {
        if (!_session.IsLoaded)
        {
            return;
        }

        _dataSet.Settings.SetValue(CostPeriodSettingsSection, CostPeriodPresetSettingKey, NormalizeCostPeriodPreset(preset));
        _dataSet.Settings.SetValue(CostPeriodSettingsSection, CostPeriodStartSettingKey, start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        _dataSet.Settings.SetValue(CostPeriodSettingsSection, CostPeriodEndSettingKey, end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        _ = PersistCostPeriodPreferencesCoreAsync();
    }

    private async Task PersistCostPeriodPreferencesCoreAsync()
    {
        try
        {
            await _session.PersistAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ShellStatus = $"Nepodařilo se uložit období nákladů: {ex.Message}";
        }
    }

    private string NormalizeCostPeriodPreset(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? CostPeriodYearToDateLabel : value.Trim();
        return CostPeriodPresets.Any(item => string.Equals(item, normalized, StringComparison.Ordinal))
            ? normalized
            : CostPeriodYearToDateLabel;
    }

    private static (DateOnly Start, DateOnly End) BuildCostPeriodForPreset(string preset, DateOnly today)
    {
        return preset switch
        {
            CostPeriodLast30DaysLabel => (today.AddDays(-29), today),
            CostPeriodLast90DaysLabel => (today.AddDays(-89), today),
            CostPeriodCurrentYearLabel => (new DateOnly(today.Year, 1, 1), new DateOnly(today.Year, 12, 31)),
            CostPeriodPreviousYearLabel => (new DateOnly(today.Year - 1, 1, 1), new DateOnly(today.Year - 1, 12, 31)),
            _ => (new DateOnly(today.Year, 1, 1), today)
        };
    }

    private static bool TryParseCostDate(string? value, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        string[] formats = ["d.M.yyyy", "dd.MM.yyyy", "yyyy-MM-dd"];
        return DateOnly.TryParseExact(trimmed, formats, CzechCulture, DateTimeStyles.None, out date)
            || DateOnly.TryParse(trimmed, CzechCulture, DateTimeStyles.None, out date);
    }

    private static string FormatCostDate(DateOnly date) =>
        date.ToString("dd.MM.yyyy", CzechCulture);

    private static string BuildCostPeriodStatus(string preset, DateOnly start, DateOnly end) =>
        $"Aktuální období: {preset}, od {FormatCostDate(start)} do {FormatCostDate(end)}. Srovnání používá stejné období loni.";
}
