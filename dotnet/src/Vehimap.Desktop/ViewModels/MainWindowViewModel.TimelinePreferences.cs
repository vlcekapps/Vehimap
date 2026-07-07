// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.Localization;
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
        if (LocalizedCompatibilityAliases.MatchesStableValueOrResource(
                normalized,
                TimelineFilterFutureKey,
                "TimelineWorkspace.Filter.Future"))
        {
            return TimelineFilterFutureKey;
        }

        if (LocalizedCompatibilityAliases.MatchesStableValueOrResource(
                normalized,
                TimelineFilterPastKey,
                "TimelineWorkspace.Filter.Past"))
        {
            return TimelineFilterPastKey;
        }

        if (LocalizedCompatibilityAliases.MatchesStableValueOrResource(
                normalized,
                TimelineFilterAllKey,
                "TimelineWorkspace.Filter.All"))
        {
            return TimelineFilterAllKey;
        }

        return TimelineFilterAllKey;
    }

    private static string GetDefaultTimelineFilter() => TimelineFilterOptions.AllKey;
}
