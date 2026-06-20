using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private const string EvidenceSortSettingsSection = "evidence_sort";
    private const string HistorySortSettingKey = "history_sort";
    private const string HistorySortDescendingSettingKey = "history_descending";
    private const string FuelSortSettingKey = "fuel_sort";
    private const string FuelSortDescendingSettingKey = "fuel_descending";
    private const string ReminderSortSettingKey = "reminder_sort";
    private const string ReminderSortDescendingSettingKey = "reminder_descending";
    private const string MaintenanceSortSettingKey = "maintenance_sort";
    private const string MaintenanceSortDescendingSettingKey = "maintenance_descending";
    private const string RecordSortSettingKey = "record_sort";
    private const string RecordSortDescendingSettingKey = "record_descending";

    private bool _suppressEvidenceSortPreferenceRefresh;

    private void ApplyEvidenceSortPreferences()
    {
        _suppressEvidenceSortPreferenceRefresh = true;
        try
        {
            HistoryWorkspace.SelectedHistorySortOption = ReadSortOption(HistorySortSettingKey, WorkspaceSortHelpers.HistorySortOptions, WorkspaceSortHelpers.DateSortLabel);
            HistoryWorkspace.HistorySortDescending = ReadSortDescending(HistorySortDescendingSettingKey, defaultValue: true);

            FuelWorkspace.SelectedFuelSortOption = ReadSortOption(FuelSortSettingKey, WorkspaceSortHelpers.FuelSortOptions, WorkspaceSortHelpers.DateSortLabel);
            FuelWorkspace.FuelSortDescending = ReadSortDescending(FuelSortDescendingSettingKey, defaultValue: true);

            ReminderWorkspace.SelectedReminderSortOption = ReadSortOption(ReminderSortSettingKey, WorkspaceSortHelpers.ReminderSortOptions, WorkspaceSortHelpers.DueDateSortLabel);
            ReminderWorkspace.ReminderSortDescending = ReadSortDescending(ReminderSortDescendingSettingKey, defaultValue: false);

            MaintenanceWorkspace.SelectedMaintenanceSortOption = ReadSortOption(MaintenanceSortSettingKey, WorkspaceSortHelpers.MaintenanceSortOptions, WorkspaceSortHelpers.TitleSortLabel);
            MaintenanceWorkspace.MaintenanceSortDescending = ReadSortDescending(MaintenanceSortDescendingSettingKey, defaultValue: false);

            RecordWorkspace.SelectedRecordSortOption = ReadSortOption(RecordSortSettingKey, WorkspaceSortHelpers.RecordSortOptions, WorkspaceSortHelpers.ValiditySortLabel);
            RecordWorkspace.RecordSortDescending = ReadSortDescending(RecordSortDescendingSettingKey, defaultValue: false);
        }
        finally
        {
            _suppressEvidenceSortPreferenceRefresh = false;
        }
    }

    internal void HandleHistoryWorkspaceSortChanged()
    {
        if (_suppressEvidenceSortPreferenceRefresh)
        {
            return;
        }

        HistoryWorkspace.RefreshVisibleHistoryItems();
        PersistEvidenceSortPreferencesAsync();
    }

    internal void HandleFuelWorkspaceSortChanged()
    {
        if (_suppressEvidenceSortPreferenceRefresh)
        {
            return;
        }

        FuelWorkspace.RefreshVisibleFuelItems();
        PersistEvidenceSortPreferencesAsync();
    }

    internal void HandleReminderWorkspaceSortChanged()
    {
        if (_suppressEvidenceSortPreferenceRefresh)
        {
            return;
        }

        ReminderWorkspace.RefreshVisibleReminderItems();
        PersistEvidenceSortPreferencesAsync();
    }

    internal void HandleRecordWorkspaceSortChanged()
    {
        if (_suppressEvidenceSortPreferenceRefresh)
        {
            return;
        }

        RecordWorkspace.RefreshVisibleRecordItems();
        PersistEvidenceSortPreferencesAsync();
    }

    internal void HandleMaintenanceWorkspaceSortChanged()
    {
        if (_suppressEvidenceSortPreferenceRefresh)
        {
            return;
        }

        MaintenanceWorkspace.RefreshVisibleMaintenanceItems();
        PersistEvidenceSortPreferencesAsync();
    }

    private string ReadSortOption(string key, IReadOnlyList<string> supportedOptions, string defaultOption) =>
        WorkspaceSortHelpers.NormalizeSortOption(
            _dataSet.Settings.GetValue(EvidenceSortSettingsSection, key, defaultOption),
            supportedOptions,
            defaultOption);

    private bool ReadSortDescending(string key, bool defaultValue)
    {
        var value = _dataSet.Settings.GetValue(EvidenceSortSettingsSection, key, defaultValue ? "1" : "0").Trim();
        return string.Equals(value, "1", StringComparison.Ordinal)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private void PersistEvidenceSortPreferencesAsync()
    {
        if (!_session.IsLoaded)
        {
            return;
        }

        _dataSet.Settings.SetValue(EvidenceSortSettingsSection, HistorySortSettingKey, WorkspaceSortHelpers.NormalizeSortOption(HistoryWorkspace.SelectedHistorySortOption, WorkspaceSortHelpers.HistorySortOptions, WorkspaceSortHelpers.DateSortLabel));
        _dataSet.Settings.SetValue(EvidenceSortSettingsSection, HistorySortDescendingSettingKey, HistoryWorkspace.HistorySortDescending ? "1" : "0");
        _dataSet.Settings.SetValue(EvidenceSortSettingsSection, FuelSortSettingKey, WorkspaceSortHelpers.NormalizeSortOption(FuelWorkspace.SelectedFuelSortOption, WorkspaceSortHelpers.FuelSortOptions, WorkspaceSortHelpers.DateSortLabel));
        _dataSet.Settings.SetValue(EvidenceSortSettingsSection, FuelSortDescendingSettingKey, FuelWorkspace.FuelSortDescending ? "1" : "0");
        _dataSet.Settings.SetValue(EvidenceSortSettingsSection, ReminderSortSettingKey, WorkspaceSortHelpers.NormalizeSortOption(ReminderWorkspace.SelectedReminderSortOption, WorkspaceSortHelpers.ReminderSortOptions, WorkspaceSortHelpers.DueDateSortLabel));
        _dataSet.Settings.SetValue(EvidenceSortSettingsSection, ReminderSortDescendingSettingKey, ReminderWorkspace.ReminderSortDescending ? "1" : "0");
        _dataSet.Settings.SetValue(EvidenceSortSettingsSection, MaintenanceSortSettingKey, WorkspaceSortHelpers.NormalizeSortOption(MaintenanceWorkspace.SelectedMaintenanceSortOption, WorkspaceSortHelpers.MaintenanceSortOptions, WorkspaceSortHelpers.TitleSortLabel));
        _dataSet.Settings.SetValue(EvidenceSortSettingsSection, MaintenanceSortDescendingSettingKey, MaintenanceWorkspace.MaintenanceSortDescending ? "1" : "0");
        _dataSet.Settings.SetValue(EvidenceSortSettingsSection, RecordSortSettingKey, WorkspaceSortHelpers.NormalizeSortOption(RecordWorkspace.SelectedRecordSortOption, WorkspaceSortHelpers.RecordSortOptions, WorkspaceSortHelpers.ValiditySortLabel));
        _dataSet.Settings.SetValue(EvidenceSortSettingsSection, RecordSortDescendingSettingKey, RecordWorkspace.RecordSortDescending ? "1" : "0");
        _ = PersistEvidenceSortPreferencesCoreAsync();
    }

    private async Task PersistEvidenceSortPreferencesCoreAsync()
    {
        try
        {
            await _session.PersistAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ShellStatus = $"Nepodařilo se uložit řazení evidencí: {ex.Message}";
        }
    }
}
