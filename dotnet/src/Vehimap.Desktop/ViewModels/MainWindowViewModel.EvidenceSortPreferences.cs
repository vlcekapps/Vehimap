// SPDX-License-Identifier: GPL-3.0-or-later
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
            HistoryWorkspace.SelectedHistorySortOption = ReadSortOption(HistorySortSettingKey, WorkspaceSortHelpers.HistorySortOptions, WorkspaceSortHelpers.DateSortOption);
            HistoryWorkspace.HistorySortDescending = ReadSortDescending(HistorySortDescendingSettingKey, defaultValue: true);

            FuelWorkspace.SelectedFuelSortOption = ReadSortOption(FuelSortSettingKey, WorkspaceSortHelpers.FuelSortOptions, WorkspaceSortHelpers.DateSortOption);
            FuelWorkspace.FuelSortDescending = ReadSortDescending(FuelSortDescendingSettingKey, defaultValue: true);

            ReminderWorkspace.SelectedReminderSortOption = ReadSortOption(ReminderSortSettingKey, WorkspaceSortHelpers.ReminderSortOptions, WorkspaceSortHelpers.DueDateSortOption);
            ReminderWorkspace.ReminderSortDescending = ReadSortDescending(ReminderSortDescendingSettingKey, defaultValue: false);

            MaintenanceWorkspace.SelectedMaintenanceSortOption = ReadSortOption(MaintenanceSortSettingKey, WorkspaceSortHelpers.MaintenanceSortOptions, WorkspaceSortHelpers.TitleSortOption);
            MaintenanceWorkspace.MaintenanceSortDescending = ReadSortDescending(MaintenanceSortDescendingSettingKey, defaultValue: false);

            RecordWorkspace.SelectedRecordSortOption = ReadSortOption(RecordSortSettingKey, WorkspaceSortHelpers.RecordSortOptions, WorkspaceSortHelpers.ValiditySortOption);
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

    private LocalizedOptionViewModel ReadSortOption(string key, IReadOnlyList<LocalizedOptionViewModel> supportedOptions, LocalizedOptionViewModel defaultOption) =>
        WorkspaceSortHelpers.NormalizeSortOption(
            _dataSet.Settings.GetValue(EvidenceSortSettingsSection, key, defaultOption.Value),
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

        var historySort = WorkspaceSortHelpers.NormalizeSortKey(HistoryWorkspace.SelectedHistorySortOption, WorkspaceSortHelpers.HistorySortOptions, WorkspaceSortHelpers.DateSortOption);
        var historyDescending = HistoryWorkspace.HistorySortDescending ? "1" : "0";
        var fuelSort = WorkspaceSortHelpers.NormalizeSortKey(FuelWorkspace.SelectedFuelSortOption, WorkspaceSortHelpers.FuelSortOptions, WorkspaceSortHelpers.DateSortOption);
        var fuelDescending = FuelWorkspace.FuelSortDescending ? "1" : "0";
        var reminderSort = WorkspaceSortHelpers.NormalizeSortKey(ReminderWorkspace.SelectedReminderSortOption, WorkspaceSortHelpers.ReminderSortOptions, WorkspaceSortHelpers.DueDateSortOption);
        var reminderDescending = ReminderWorkspace.ReminderSortDescending ? "1" : "0";
        var maintenanceSort = WorkspaceSortHelpers.NormalizeSortKey(MaintenanceWorkspace.SelectedMaintenanceSortOption, WorkspaceSortHelpers.MaintenanceSortOptions, WorkspaceSortHelpers.TitleSortOption);
        var maintenanceDescending = MaintenanceWorkspace.MaintenanceSortDescending ? "1" : "0";
        var recordSort = WorkspaceSortHelpers.NormalizeSortKey(RecordWorkspace.SelectedRecordSortOption, WorkspaceSortHelpers.RecordSortOptions, WorkspaceSortHelpers.ValiditySortOption);
        var recordDescending = RecordWorkspace.RecordSortDescending ? "1" : "0";

        PersistPreferenceSettingsAsync(
            settings =>
            {
                settings.SetValue(EvidenceSortSettingsSection, HistorySortSettingKey, historySort);
                settings.SetValue(EvidenceSortSettingsSection, HistorySortDescendingSettingKey, historyDescending);
                settings.SetValue(EvidenceSortSettingsSection, FuelSortSettingKey, fuelSort);
                settings.SetValue(EvidenceSortSettingsSection, FuelSortDescendingSettingKey, fuelDescending);
                settings.SetValue(EvidenceSortSettingsSection, ReminderSortSettingKey, reminderSort);
                settings.SetValue(EvidenceSortSettingsSection, ReminderSortDescendingSettingKey, reminderDescending);
                settings.SetValue(EvidenceSortSettingsSection, MaintenanceSortSettingKey, maintenanceSort);
                settings.SetValue(EvidenceSortSettingsSection, MaintenanceSortDescendingSettingKey, maintenanceDescending);
                settings.SetValue(EvidenceSortSettingsSection, RecordSortSettingKey, recordSort);
                settings.SetValue(EvidenceSortSettingsSection, RecordSortDescendingSettingKey, recordDescending);
            },
            LO("Preferences.Persistence.EvidenceSortFailed"));
    }
}
