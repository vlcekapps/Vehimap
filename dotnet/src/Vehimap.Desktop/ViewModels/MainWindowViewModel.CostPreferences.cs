// SPDX-License-Identifier: GPL-3.0-or-later
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

    internal const string CostPeriodYearToDateKey = "year_to_date";
    internal const string CostPeriodLast30DaysKey = "last_30_days";
    internal const string CostPeriodLast90DaysKey = "last_90_days";
    internal const string CostPeriodCurrentYearKey = "current_year";
    internal const string CostPeriodPreviousYearKey = "previous_year";
    internal const string CostPeriodCustomKey = "custom";

    private const string CostPeriodSettingsSection = "costs";
    private const string CostPeriodPresetSettingKey = "period_preset";
    private const string CostPeriodStartSettingKey = "period_start";
    private const string CostPeriodEndSettingKey = "period_end";
    private static readonly CultureInfo CzechCulture = CultureInfo.GetCultureInfo("cs-CZ");

    private bool _suppressCostPeriodRefresh;
    private DateOnly _costPeriodStart;
    private DateOnly _costPeriodEnd;

    private void ApplyCostPeriodPreferences()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var presetKey = NormalizeCostPeriodPresetKey(
            _dataSet.Settings.GetValue(CostPeriodSettingsSection, CostPeriodPresetSettingKey, CostPeriodYearToDateKey));
        var preset = CostPeriodLabelFromKey(presetKey);
        var (start, end) = BuildCostPeriodForPreset(preset, today);

        if (string.Equals(presetKey, CostPeriodCustomKey, StringComparison.Ordinal))
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

        if (string.Equals(NormalizeCostPeriodPresetKey(preset), CostPeriodCustomKey, StringComparison.Ordinal))
        {
            CostWorkspace.CostPeriodStatus = LO("CostPeriod.Status.CustomPrompt");
            return;
        }

        var (start, end) = BuildCostPeriodForPreset(preset, DateOnly.FromDateTime(DateTime.Today));
        ApplyCostPeriodSelection(preset, start, end, persist: true, requestFocus: false, LO("CostPeriod.Status.PresetChanged"));
    }

    internal void HandleCostPeriodCustomDateChanged()
    {
        if (_suppressCostPeriodRefresh || !_session.IsLoaded)
        {
            return;
        }

        if (!string.Equals(NormalizeCostPeriodPresetKey(CostWorkspace.SelectedCostPeriodPreset), CostPeriodCustomKey, StringComparison.Ordinal))
        {
            _suppressCostPeriodRefresh = true;
            try
            {
                CostWorkspace.SelectedCostPeriodPreset = CostPeriodLabelFromKey(CostPeriodCustomKey);
            }
            finally
            {
                _suppressCostPeriodRefresh = false;
            }
        }

        CostWorkspace.CostPeriodStatus = LO("CostPeriod.Status.CustomPending");
    }

    internal void ApplyCostPeriodFromWorkspace()
    {
        if (!_session.IsLoaded)
        {
            return;
        }

        if (!TryParseCostDate(CostWorkspace.CostPeriodStartText, out var start))
        {
            CostWorkspace.CostPeriodStatus = LO("CostPeriod.Status.InvalidStartDate");
            ShellStatus = CostWorkspace.CostPeriodStatus;
            RequestFocus(DesktopFocusTarget.CostPeriodStart);
            return;
        }

        if (!TryParseCostDate(CostWorkspace.CostPeriodEndText, out var end))
        {
            CostWorkspace.CostPeriodStatus = LO("CostPeriod.Status.InvalidEndDate");
            ShellStatus = CostWorkspace.CostPeriodStatus;
            RequestFocus(DesktopFocusTarget.CostPeriodStart);
            return;
        }

        var status = LO("CostPeriod.Status.Applied");
        if (end < start)
        {
            (start, end) = (end, start);
            status = LO("CostPeriod.Status.AppliedSwapped");
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
            (_costPeriodStart, _costPeriodEnd) = BuildCostPeriodForPreset(CostPeriodYearToDateKey, today);
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

        CostWorkspace.CostExportStatus = status;
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

        var normalizedPreset = NormalizeCostPeriodPresetKey(preset);
        var startValue = start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var endValue = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        PersistPreferenceSettingsAsync(
            settings =>
            {
                settings.SetValue(CostPeriodSettingsSection, CostPeriodPresetSettingKey, normalizedPreset);
                settings.SetValue(CostPeriodSettingsSection, CostPeriodStartSettingKey, startValue);
                settings.SetValue(CostPeriodSettingsSection, CostPeriodEndSettingKey, endValue);
            },
            LO("CostPeriod.PreferenceSaveFailed"));
    }

    private string NormalizeCostPeriodPreset(string? value)
    {
        return CostPeriodLabelFromKey(NormalizeCostPeriodPresetKey(value));
    }

    private static (DateOnly Start, DateOnly End) BuildCostPeriodForPreset(string preset, DateOnly today)
    {
        return NormalizeCostPeriodPresetKey(preset) switch
        {
            CostPeriodLast30DaysKey => (today.AddDays(-29), today),
            CostPeriodLast90DaysKey => (today.AddDays(-89), today),
            CostPeriodCurrentYearKey => (new DateOnly(today.Year, 1, 1), new DateOnly(today.Year, 12, 31)),
            CostPeriodPreviousYearKey => (new DateOnly(today.Year - 1, 1, 1), new DateOnly(today.Year - 1, 12, 31)),
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
        LFO(
            "CostPeriod.Status.CurrentPeriod",
            CostPeriodLabelFromKey(NormalizeCostPeriodPresetKey(preset)),
            FormatCostDate(start),
            FormatCostDate(end));

    private static string NormalizeCostPeriodPresetKey(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return CostPeriodYearToDateKey;
        }

        if (MatchesCostPeriodPreset(normalized, CostPeriodYearToDateKey, "CostPeriod.YearToDate", CostPeriodYearToDateLabel, "Year to date"))
        {
            return CostPeriodYearToDateKey;
        }

        if (MatchesCostPeriodPreset(normalized, CostPeriodLast30DaysKey, "CostPeriod.Last30Days", CostPeriodLast30DaysLabel, "Last 30 days"))
        {
            return CostPeriodLast30DaysKey;
        }

        if (MatchesCostPeriodPreset(normalized, CostPeriodLast90DaysKey, "CostPeriod.Last90Days", CostPeriodLast90DaysLabel, "Last 90 days"))
        {
            return CostPeriodLast90DaysKey;
        }

        if (MatchesCostPeriodPreset(normalized, CostPeriodCurrentYearKey, "CostPeriod.CurrentYear", CostPeriodCurrentYearLabel, "Current year"))
        {
            return CostPeriodCurrentYearKey;
        }

        if (MatchesCostPeriodPreset(normalized, CostPeriodPreviousYearKey, "CostPeriod.PreviousYear", CostPeriodPreviousYearLabel, "Previous year"))
        {
            return CostPeriodPreviousYearKey;
        }

        if (MatchesCostPeriodPreset(normalized, CostPeriodCustomKey, "CostPeriod.Custom", CostPeriodCustomLabel, "Custom period"))
        {
            return CostPeriodCustomKey;
        }

        return CostPeriodYearToDateKey;
    }

    private static bool MatchesCostPeriodPreset(string normalized, string key, string resourceKey, params string[] aliases) =>
        string.Equals(normalized, key, StringComparison.OrdinalIgnoreCase)
        || string.Equals(normalized, LO(resourceKey), StringComparison.CurrentCultureIgnoreCase)
        || aliases.Any(alias => string.Equals(normalized, alias, StringComparison.OrdinalIgnoreCase));

    private static string CostPeriodLabelFromKey(string key) =>
        key switch
        {
            CostPeriodLast30DaysKey => LO("CostPeriod.Last30Days"),
            CostPeriodLast90DaysKey => LO("CostPeriod.Last90Days"),
            CostPeriodCurrentYearKey => LO("CostPeriod.CurrentYear"),
            CostPeriodPreviousYearKey => LO("CostPeriod.PreviousYear"),
            CostPeriodCustomKey => LO("CostPeriod.Custom"),
            _ => LO("CostPeriod.YearToDate")
        };
}
