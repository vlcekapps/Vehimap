// SPDX-License-Identifier: GPL-3.0-or-later
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private const string WorkspaceSortSettingsSection = "workspace_sort";
    private const string AuditSortSettingKey = "audit_sort";
    private const string AuditSortDescendingSettingKey = "audit_descending";
    private const string GlobalSearchSortSettingKey = "global_search_sort";
    private const string GlobalSearchSortDescendingSettingKey = "global_search_descending";

    private bool _suppressWorkspaceSortPreferenceRefresh;

    private void ApplyWorkspaceSortPreferences()
    {
        _suppressWorkspaceSortPreferenceRefresh = true;
        try
        {
            AuditWorkspace.SelectedAuditSortOption = ReadWorkspaceSortOption(
                AuditSortSettingKey,
                WorkspaceSortHelpers.AuditSortOptions,
                WorkspaceSortHelpers.SeveritySortOption);
            AuditWorkspace.AuditSortDescending = ReadWorkspaceSortDescending(AuditSortDescendingSettingKey, defaultValue: false);

            GlobalSearchWorkspace.SelectedGlobalSearchSortOption = ReadWorkspaceSortOption(
                GlobalSearchSortSettingKey,
                WorkspaceSortHelpers.GlobalSearchSortOptions,
                WorkspaceSortHelpers.TypeSortOption);
            GlobalSearchWorkspace.GlobalSearchSortDescending = ReadWorkspaceSortDescending(GlobalSearchSortDescendingSettingKey, defaultValue: false);
        }
        finally
        {
            _suppressWorkspaceSortPreferenceRefresh = false;
        }
    }

    internal void HandleAuditWorkspaceSortChanged()
    {
        if (_suppressWorkspaceSortPreferenceRefresh)
        {
            return;
        }

        AuditWorkspace.RefreshVisibleAuditItems();
        PersistWorkspaceSortPreferencesAsync();
    }

    internal void HandleGlobalSearchWorkspaceSortChanged()
    {
        if (_suppressWorkspaceSortPreferenceRefresh)
        {
            return;
        }

        RefreshGlobalSearch();
        PersistWorkspaceSortPreferencesAsync();
    }

    private LocalizedOptionViewModel ReadWorkspaceSortOption(string key, IReadOnlyList<LocalizedOptionViewModel> supportedOptions, LocalizedOptionViewModel defaultOption) =>
        WorkspaceSortHelpers.NormalizeSortOption(
            _dataSet.Settings.GetValue(WorkspaceSortSettingsSection, key, defaultOption.Value),
            supportedOptions,
            defaultOption);

    private bool ReadWorkspaceSortDescending(string key, bool defaultValue)
    {
        var value = _dataSet.Settings.GetValue(WorkspaceSortSettingsSection, key, defaultValue ? "1" : "0").Trim();
        return string.Equals(value, "1", StringComparison.Ordinal)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private void PersistWorkspaceSortPreferencesAsync()
    {
        if (!_session.IsLoaded)
        {
            return;
        }

        var auditSort = WorkspaceSortHelpers.NormalizeSortOption(
            AuditWorkspace.SelectedAuditSortOption,
            WorkspaceSortHelpers.AuditSortOptions,
            WorkspaceSortHelpers.SeveritySortOption).Value;
        var auditDescending = AuditWorkspace.AuditSortDescending ? "1" : "0";
        var globalSearchSort = WorkspaceSortHelpers.NormalizeSortOption(
            GlobalSearchWorkspace.SelectedGlobalSearchSortOption,
            WorkspaceSortHelpers.GlobalSearchSortOptions,
            WorkspaceSortHelpers.TypeSortOption).Value;
        var globalSearchDescending = GlobalSearchWorkspace.GlobalSearchSortDescending ? "1" : "0";

        PersistPreferenceSettingsAsync(
            settings =>
            {
                settings.SetValue(WorkspaceSortSettingsSection, AuditSortSettingKey, auditSort);
                settings.SetValue(WorkspaceSortSettingsSection, AuditSortDescendingSettingKey, auditDescending);
                settings.SetValue(WorkspaceSortSettingsSection, GlobalSearchSortSettingKey, globalSearchSort);
                settings.SetValue(WorkspaceSortSettingsSection, GlobalSearchSortDescendingSettingKey, globalSearchDescending);
            },
            LO("Preferences.Persistence.WorkspaceSortFailed"));
    }
}
