// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private const string TimelineSettingsSection = "timeline";
    private const string TimelineFilterSettingKey = "filter";
    private const string TimelineFilterAllKey = "all";
    private const string TimelineFilterFutureKey = "future";
    private const string TimelineFilterPastKey = "past";

    private bool _suppressTimelinePreferenceRefresh;

    private void ApplyTimelinePreferences()
    {
        _suppressTimelinePreferenceRefresh = true;
        try
        {
            TimelineWorkspace.SelectedTimelineFilter = NormalizeTimelineFilter(_dataSet.Settings.GetValue(
                TimelineSettingsSection,
                TimelineFilterSettingKey,
                GetDefaultTimelineFilter()));
        }
        finally
        {
            _suppressTimelinePreferenceRefresh = false;
        }
    }

    private void PersistTimelinePreferencesAsync()
    {
        if (_suppressTimelinePreferenceRefresh || !_session.IsLoaded)
        {
            return;
        }

        var timelineFilter = NormalizeTimelineFilterKey(TimelineWorkspace.SelectedTimelineFilter.Value);
        PersistPreferenceSettingsAsync(
            settings => settings.SetValue(TimelineSettingsSection, TimelineFilterSettingKey, timelineFilter),
            LO("TimelineWorkspace.PreferenceSaveFailed"));
    }

    private LocalizedOptionViewModel NormalizeTimelineFilter(string? value)
    {
        return TimelineFilterOptions.Option(NormalizeTimelineFilterKey(value));
    }

    private string NormalizeTimelineFilterKey(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.Equals(normalized, TimelineFilterFutureKey, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, TimelineFilterOptions.LabelForKey(TimelineFilterOptions.FutureKey), StringComparison.CurrentCultureIgnoreCase)
            || string.Equals(normalized, "Budoucí", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "Future", StringComparison.OrdinalIgnoreCase))
        {
            return TimelineFilterFutureKey;
        }

        if (string.Equals(normalized, TimelineFilterPastKey, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, TimelineFilterOptions.LabelForKey(TimelineFilterOptions.PastKey), StringComparison.CurrentCultureIgnoreCase)
            || string.Equals(normalized, "Minulé", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "Past", StringComparison.OrdinalIgnoreCase))
        {
            return TimelineFilterPastKey;
        }

        if (string.Equals(normalized, TimelineFilterAllKey, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, TimelineFilterOptions.LabelForKey(TimelineFilterOptions.AllKey), StringComparison.CurrentCultureIgnoreCase)
            || string.Equals(normalized, "Vše", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "All", StringComparison.OrdinalIgnoreCase))
        {
            return TimelineFilterAllKey;
        }

        return TimelineFilterAllKey;
    }

    private static string GetDefaultTimelineFilter() => TimelineFilterOptions.AllKey;
}
