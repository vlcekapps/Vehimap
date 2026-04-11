using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

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

    public string WindowTitle => Root.GlobalSearchWindowTitle;

    public ObservableCollection<GlobalSearchResultItemViewModel> GlobalSearchResults => Root.GlobalSearchResults;

    public ICommand OpenSelectedSearchResultCommand => Root.OpenSelectedSearchResultCommand;

    partial void OnGlobalSearchTextChanged(string value)
    {
        Root.HandleGlobalSearchWorkspaceSearchChanged();
    }

    partial void OnSelectedSearchResultChanged(GlobalSearchResultItemViewModel? value)
    {
        SelectedSearchResultDetail = value is null
            ? "Vyberte výsledek a můžete přejít rovnou na správné vozidlo nebo evidenci."
            : $"{value.SectionLabel}: {value.Title}\nVozidlo: {value.VehicleName}\n{value.Summary}";

        Root.NotifyGlobalSearchWorkspaceSelectionChanged();
    }
}
