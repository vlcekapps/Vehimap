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
    private string selectedUpcomingOverviewFilter = L("Overview.Filter.All");

    [ObservableProperty]
    private string selectedUpcomingOverviewSortOption = WorkspaceSortHelpers.DateSortLabel;

    [ObservableProperty]
    private bool upcomingOverviewSortDescending;

    [ObservableProperty]
    private bool includeMissingGreenCardsInUpcomingOverview;

    [ObservableProperty]
    private bool includeDataIssuesInUpcomingOverview;

    [ObservableProperty]
    private string upcomingOverviewSummary = L("Overview.Summary.UpcomingInitial");

    [ObservableProperty]
    private string selectedUpcomingOverviewDetail = L("Overview.Detail.EmptyUpcoming");

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedUpcomingOverviewItem;

    public string WindowTitle => Root.UpcomingOverviewWindowTitle;

    public ObservableCollection<VehicleTimelineItemViewModel> UpcomingOverviewItems { get; } = [];

    public IReadOnlyList<string> OverviewFilters { get; } =
    [
        L("Overview.Filter.All"),
        L("Overview.Filter.Technical"),
        L("Overview.Filter.GreenCards"),
        L("Overview.Filter.Reminders"),
        L("Overview.Filter.Records"),
        L("Overview.Filter.Maintenance"),
        L("Overview.Filter.DataIssues")
    ];

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
            ? L("Overview.Detail.EmptyUpcoming")
            : LF(
                "Overview.Detail.Selected",
                value.VehicleName,
                value.Date,
                value.KindLabel,
                value.Title,
                Root.FormatWorkspaceValue(value.Detail, "-"),
                Root.FormatWorkspaceValue(value.Status, "-"));

        Root.NotifyUpcomingOverviewWorkspaceSelectionChanged();
    }
}
