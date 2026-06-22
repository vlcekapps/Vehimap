namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private const string TimelineSettingsSection = "timeline";
    private const string TimelineFilterSettingKey = "filter";

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

        var timelineFilter = NormalizeTimelineFilter(TimelineWorkspace.SelectedTimelineFilter);
        PersistPreferenceSettingsAsync(
            settings => settings.SetValue(TimelineSettingsSection, TimelineFilterSettingKey, timelineFilter),
            "Nepodařilo se uložit filtr časové osy");
    }

    private string NormalizeTimelineFilter(string? value)
    {
        var defaultFilter = GetDefaultTimelineFilter();
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultFilter : value.Trim();
        return TimelineWorkspace.TimelineFilters.Any(item => string.Equals(item, normalized, StringComparison.Ordinal))
            ? normalized
            : defaultFilter;
    }

    private string GetDefaultTimelineFilter() => TimelineWorkspace.TimelineFilters.Count > 0 ? TimelineWorkspace.TimelineFilters[0] : "Vše";
}
