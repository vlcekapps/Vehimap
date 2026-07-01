// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class AuditWorkspaceViewModel : WorkspaceViewModelBase
{
    public AuditWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    [ObservableProperty]
    private string auditSummary = string.Empty;

    private string unfilteredAuditSummary = string.Empty;

    [ObservableProperty]
    private string auditSearchText = string.Empty;

    [ObservableProperty]
    private AuditItemViewModel? selectedDashboardAuditItem;

    [ObservableProperty]
    private string selectedAuditSortOption = WorkspaceSortHelpers.SeveritySortLabel;

    [ObservableProperty]
    private bool auditSortDescending;

    public string WindowTitle => Root.AuditWindowTitle;

    public IReadOnlyList<string> AuditSortOptions => WorkspaceSortHelpers.AuditSortOptions;

    public ObservableCollection<AuditItemViewModel> AuditItems { get; } = [];

    public ObservableCollection<AuditItemViewModel> VisibleAuditItems { get; } = [];

    public bool CanOpenSelectedAuditItem => SelectedDashboardAuditItem is not null;
    public bool CanClearAuditSearch => !string.IsNullOrWhiteSpace(AuditSearchText);

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.AuditSearch);
    }

    [RelayCommand]
    private void RefreshAudit()
    {
        Root.RefreshAuditWorkspace();
    }

    [RelayCommand(CanExecute = nameof(CanClearAuditSearch))]
    private void ClearAuditSearch()
    {
        AuditSearchText = string.Empty;
        RequestFocus(DesktopFocusTarget.AuditSearch);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedAuditItem))]
    private async Task OpenSelectedAuditItemAsync()
    {
        await Root.OpenAuditItemAsync(SelectedDashboardAuditItem).ConfigureAwait(true);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedAuditItem))]
    private async Task OpenSelectedAuditVehicleAsync()
    {
        await Root.OpenAuditVehicleAsync(SelectedDashboardAuditItem).ConfigureAwait(true);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedAuditItem))]
    private async Task EditSelectedAuditItemAsync()
    {
        await Root.EditAuditItemAsync(SelectedDashboardAuditItem).ConfigureAwait(true);
    }

    public ICommand OpenSelectedDashboardAuditItemCommand => OpenSelectedAuditItemCommand;

    public void SetAuditSummary(string summary)
    {
        unfilteredAuditSummary = summary;
        if (string.IsNullOrWhiteSpace(AuditSearchText))
        {
            AuditSummary = summary;
        }
    }

    public void RefreshVisibleAuditItems(bool preserveSelection = true)
    {
        var previousSelection = preserveSelection ? SelectedDashboardAuditItem : null;
        var filteredItems = WorkspaceSortHelpers
            .SortAudit(AuditItems.Where(MatchesSearch), SelectedAuditSortOption, AuditSortDescending)
            .ToList();

        VisibleAuditItems.Clear();
        foreach (var item in filteredItems)
        {
            VisibleAuditItems.Add(item);
        }

        SelectedDashboardAuditItem = previousSelection is not null
            ? VisibleAuditItems.FirstOrDefault(item =>
                string.Equals(item.VehicleId, previousSelection.VehicleId, StringComparison.Ordinal)
                && string.Equals(item.EntityKind, previousSelection.EntityKind, StringComparison.Ordinal)
                && string.Equals(item.EntityId, previousSelection.EntityId, StringComparison.Ordinal))
            : null;

        SelectedDashboardAuditItem ??= VisibleAuditItems.FirstOrDefault();
        UpdateFilteredSummary();
    }

    partial void OnSelectedDashboardAuditItemChanged(AuditItemViewModel? value)
    {
        OnPropertyChanged(nameof(CanOpenSelectedAuditItem));
        OpenSelectedAuditItemCommand.NotifyCanExecuteChanged();
        OpenSelectedAuditVehicleCommand.NotifyCanExecuteChanged();
        EditSelectedAuditItemCommand.NotifyCanExecuteChanged();
        Root.NotifyAuditWorkspaceSelectionChanged();
    }

    partial void OnAuditSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanClearAuditSearch));
        ClearAuditSearchCommand.NotifyCanExecuteChanged();
        RefreshVisibleAuditItems();
    }

    partial void OnSelectedAuditSortOptionChanged(string value)
    {
        Root.HandleAuditWorkspaceSortChanged();
    }

    partial void OnAuditSortDescendingChanged(bool value)
    {
        Root.HandleAuditWorkspaceSortChanged();
    }

    private bool MatchesSearch(AuditItemViewModel item)
    {
        if (string.IsNullOrWhiteSpace(AuditSearchText))
        {
            return true;
        }

        var query = AuditSearchText.Trim();
        return Contains(item.VehicleName, query)
            || Contains(item.Title, query)
            || Contains(item.Message, query)
            || Contains(item.Category, query)
            || Contains(item.Severity, query)
            || Contains(item.EntityKind, query);
    }

    private void UpdateFilteredSummary()
    {
        if (string.IsNullOrWhiteSpace(AuditSearchText))
        {
            AuditSummary = unfilteredAuditSummary;
            return;
        }

        AuditSummary = VisibleAuditItems.Count == 0
            ? $"Pro hledání „{AuditSearchText.Trim()}“ nejsou v auditu žádné položky."
            : $"Hledání „{AuditSearchText.Trim()}“ našlo {VisibleAuditItems.Count} z {AuditItems.Count} auditních položek.";
    }

    private static bool Contains(string value, string query) =>
        value.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0;
}
