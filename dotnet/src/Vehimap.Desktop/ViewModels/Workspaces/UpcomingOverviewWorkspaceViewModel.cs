using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class UpcomingOverviewWorkspaceViewModel : WorkspaceViewModelBase
{
    public UpcomingOverviewWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    [ObservableProperty]
    private string upcomingOverviewSearchText = string.Empty;

    [ObservableProperty]
    private string selectedUpcomingOverviewFilter = "Vše";

    [ObservableProperty]
    private string selectedUpcomingOverviewSortOption = WorkspaceSortHelpers.DateSortLabel;

    [ObservableProperty]
    private bool upcomingOverviewSortDescending;

    [ObservableProperty]
    private bool includeMissingGreenCardsInUpcomingOverview;

    [ObservableProperty]
    private bool includeDataIssuesInUpcomingOverview;

    [ObservableProperty]
    private string upcomingOverviewSummary = "Blížící se termíny napříč vozidly se zobrazí po načtení dat.";

    [ObservableProperty]
    private string selectedUpcomingOverviewDetail = "Vyberte termín a můžete přejít na související vozidlo nebo evidenci.";

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedUpcomingOverviewItem;

    public string WindowTitle => Root.UpcomingOverviewWindowTitle;

    public ObservableCollection<VehicleTimelineItemViewModel> UpcomingOverviewItems { get; } = [];

    public IReadOnlyList<string> OverviewFilters => Root.UpcomingOverviewFilters;

    public IReadOnlyList<string> OverviewSortOptions => WorkspaceSortHelpers.TimelineOverviewSortOptions;

    public ICommand OpenSelectedUpcomingOverviewItemCommand => Root.OpenSelectedUpcomingOverviewItemCommand;

    public ICommand OpenSelectedUpcomingOverviewVehicleCommand => Root.OpenSelectedUpcomingOverviewVehicleCommand;
    public bool CanClearUpcomingOverviewSearch => !string.IsNullOrWhiteSpace(UpcomingOverviewSearchText);

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.UpcomingOverviewSearch);
    }

    [RelayCommand]
    private void RefreshUpcomingOverview()
    {
        Root.RefreshUpcomingOverviewWorkspace();
    }

    [RelayCommand(CanExecute = nameof(CanClearUpcomingOverviewSearch))]
    private void ClearUpcomingOverviewSearch()
    {
        UpcomingOverviewSearchText = string.Empty;
        RequestFocus(DesktopFocusTarget.UpcomingOverviewSearch);
    }

    partial void OnUpcomingOverviewSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanClearUpcomingOverviewSearch));
        ClearUpcomingOverviewSearchCommand.NotifyCanExecuteChanged();
        Root.HandleUpcomingOverviewWorkspaceSearchChanged();
    }

    partial void OnSelectedUpcomingOverviewFilterChanged(string value)
    {
        Root.HandleUpcomingOverviewWorkspaceFilterChanged();
    }

    partial void OnSelectedUpcomingOverviewSortOptionChanged(string value)
    {
        Root.HandleUpcomingOverviewWorkspaceSortChanged();
    }

    partial void OnUpcomingOverviewSortDescendingChanged(bool value)
    {
        Root.HandleUpcomingOverviewWorkspaceSortChanged();
    }

    partial void OnIncludeMissingGreenCardsInUpcomingOverviewChanged(bool value)
    {
        Root.HandleUpcomingOverviewWorkspaceOptionsChanged();
    }

    partial void OnIncludeDataIssuesInUpcomingOverviewChanged(bool value)
    {
        Root.HandleUpcomingOverviewWorkspaceOptionsChanged();
    }

    partial void OnSelectedUpcomingOverviewItemChanged(VehicleTimelineItemViewModel? value)
    {
        SelectedUpcomingOverviewDetail = value is null
            ? "Vyberte termín a můžete přejít na související vozidlo nebo evidenci."
            : $"{value.VehicleName}\n{value.Date} | {value.KindLabel}\n{value.Title}\n{value.Detail}\nStav: {value.Status}";

        Root.NotifyUpcomingOverviewWorkspaceSelectionChanged();
    }
}
