using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class GlobalSearchWorkspaceViewModel : WorkspaceViewModelBase
{
    public GlobalSearchWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    [ObservableProperty]
    private string globalSearchSummary = "Zadejte hledaný text a zobrazí se odpovídající vozidla i záznamy napříč aplikací.";

    [ObservableProperty]
    private string globalSearchText = string.Empty;

    [ObservableProperty]
    private GlobalSearchResultItemViewModel? selectedSearchResult;

    [ObservableProperty]
    private string selectedSearchResultDetail = "Vyberte výsledek a můžete přejít rovnou na správné vozidlo nebo evidenci.";

    [ObservableProperty]
    private string selectedGlobalSearchSortOption = WorkspaceSortHelpers.TypeSortLabel;

    [ObservableProperty]
    private bool globalSearchSortDescending;

    public string WindowTitle => Root.GlobalSearchWindowTitle;

    public IReadOnlyList<string> GlobalSearchSortOptions => WorkspaceSortHelpers.GlobalSearchSortOptions;

    public ObservableCollection<GlobalSearchResultItemViewModel> GlobalSearchResults => Root.GlobalSearchResults;

    public ICommand OpenSelectedSearchResultCommand => Root.OpenSelectedSearchResultCommand;
    public bool CanClearGlobalSearch => !string.IsNullOrWhiteSpace(GlobalSearchText);

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.GlobalSearchBox);
    }

    [RelayCommand]
    private void RefreshGlobalSearch()
    {
        Root.RefreshGlobalSearchWorkspace();
    }

    [RelayCommand(CanExecute = nameof(CanClearGlobalSearch))]
    private void ClearGlobalSearch()
    {
        GlobalSearchText = string.Empty;
        RequestFocus(DesktopFocusTarget.GlobalSearchBox);
    }

    partial void OnGlobalSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanClearGlobalSearch));
        ClearGlobalSearchCommand.NotifyCanExecuteChanged();
        Root.HandleGlobalSearchWorkspaceSearchChanged();
    }

    partial void OnSelectedSearchResultChanged(GlobalSearchResultItemViewModel? value)
    {
        SelectedSearchResultDetail = value is null
            ? "Vyberte výsledek a můžete přejít rovnou na správné vozidlo nebo evidenci."
            : $"{value.SectionLabel}: {value.Title}\nVozidlo: {value.VehicleName}\n{value.Summary}";

        Root.NotifyGlobalSearchWorkspaceSelectionChanged();
    }

    partial void OnSelectedGlobalSearchSortOptionChanged(string value)
    {
        Root.HandleGlobalSearchWorkspaceSortChanged();
    }

    partial void OnGlobalSearchSortDescendingChanged(bool value)
    {
        Root.HandleGlobalSearchWorkspaceSortChanged();
    }
}
