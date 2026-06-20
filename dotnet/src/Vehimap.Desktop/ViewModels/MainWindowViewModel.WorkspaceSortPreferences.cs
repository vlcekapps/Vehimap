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
                WorkspaceSortHelpers.SeveritySortLabel);
            AuditWorkspace.AuditSortDescending = ReadWorkspaceSortDescending(AuditSortDescendingSettingKey, defaultValue: false);

            GlobalSearchWorkspace.SelectedGlobalSearchSortOption = ReadWorkspaceSortOption(
                GlobalSearchSortSettingKey,
                WorkspaceSortHelpers.GlobalSearchSortOptions,
                WorkspaceSortHelpers.TypeSortLabel);
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

    private string ReadWorkspaceSortOption(string key, IReadOnlyList<string> supportedOptions, string defaultOption) =>
        WorkspaceSortHelpers.NormalizeSortOption(
            _dataSet.Settings.GetValue(WorkspaceSortSettingsSection, key, defaultOption),
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

        _dataSet.Settings.SetValue(
            WorkspaceSortSettingsSection,
            AuditSortSettingKey,
            WorkspaceSortHelpers.NormalizeSortOption(
                AuditWorkspace.SelectedAuditSortOption,
                WorkspaceSortHelpers.AuditSortOptions,
                WorkspaceSortHelpers.SeveritySortLabel));
        _dataSet.Settings.SetValue(WorkspaceSortSettingsSection, AuditSortDescendingSettingKey, AuditWorkspace.AuditSortDescending ? "1" : "0");
        _dataSet.Settings.SetValue(
            WorkspaceSortSettingsSection,
            GlobalSearchSortSettingKey,
            WorkspaceSortHelpers.NormalizeSortOption(
                GlobalSearchWorkspace.SelectedGlobalSearchSortOption,
                WorkspaceSortHelpers.GlobalSearchSortOptions,
                WorkspaceSortHelpers.TypeSortLabel));
        _dataSet.Settings.SetValue(WorkspaceSortSettingsSection, GlobalSearchSortDescendingSettingKey, GlobalSearchWorkspace.GlobalSearchSortDescending ? "1" : "0");
        _ = PersistWorkspaceSortPreferencesCoreAsync();
    }

    private async Task PersistWorkspaceSortPreferencesCoreAsync()
    {
        try
        {
            await _session.PersistAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ShellStatus = $"Nepodařilo se uložit řazení přehledů: {ex.Message}";
        }
    }
}
