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

    [ObservableProperty]
    private string auditSearchText = string.Empty;

    [ObservableProperty]
    private AuditItemViewModel? selectedDashboardAuditItem;

    public string WindowTitle => Root.AuditWindowTitle;

    public ObservableCollection<AuditItemViewModel> VisibleAuditItems { get; } = [];

    public bool CanOpenSelectedAuditItem => SelectedDashboardAuditItem is not null;

    [RelayCommand]
    private void FocusSearch()
    {
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

    public void RefreshVisibleAuditItems(bool preserveSelection = true)
    {
        var previousSelection = preserveSelection ? SelectedDashboardAuditItem : null;
        var filteredItems = Root.AuditItems
            .Where(MatchesSearch)
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
        RefreshVisibleAuditItems();
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
            AuditSummary = Root.AuditSummary;
            return;
        }

        AuditSummary = VisibleAuditItems.Count == 0
            ? $"Pro hledání „{AuditSearchText.Trim()}“ nejsou v auditu žádné položky."
            : $"Hledání „{AuditSearchText.Trim()}“ našlo {VisibleAuditItems.Count} z {Root.AuditItems.Count} auditních položek.";
    }

    private static bool Contains(string value, string query) =>
        value.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0;
}
