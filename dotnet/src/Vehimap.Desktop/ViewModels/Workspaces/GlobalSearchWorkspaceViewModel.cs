using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class GlobalSearchWorkspaceViewModel : WorkspaceViewModelBase
{
    public GlobalSearchWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string GlobalSearchSummary => Root.GlobalSearchSummary;

    public string GlobalSearchText
    {
        get => Root.GlobalSearchText;
        set => Root.GlobalSearchText = value;
    }

    public ObservableCollection<GlobalSearchResultItemViewModel> GlobalSearchResults => Root.GlobalSearchResults;

    public GlobalSearchResultItemViewModel? SelectedSearchResult
    {
        get => Root.SelectedSearchResult;
        set => Root.SelectedSearchResult = value;
    }

    public string SelectedSearchResultDetail => Root.SelectedSearchResultDetail;

    public ICommand OpenSelectedSearchResultCommand => Root.OpenSelectedSearchResultCommand;
}
