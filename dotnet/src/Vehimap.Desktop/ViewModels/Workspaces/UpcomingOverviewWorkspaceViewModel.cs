using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class UpcomingOverviewWorkspaceViewModel : WorkspaceViewModelBase
{
    public UpcomingOverviewWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string UpcomingOverviewSummary => Root.UpcomingOverviewSummary;

    public string UpcomingOverviewSearchText
    {
        get => Root.UpcomingOverviewSearchText;
        set => Root.UpcomingOverviewSearchText = value;
    }

    public IReadOnlyList<string> OverviewFilters => Root.OverviewFilters;

    public string SelectedUpcomingOverviewFilter
    {
        get => Root.SelectedUpcomingOverviewFilter;
        set => Root.SelectedUpcomingOverviewFilter = value;
    }

    public ObservableCollection<VehicleTimelineItemViewModel> UpcomingOverviewItems => Root.UpcomingOverviewItems;

    public VehicleTimelineItemViewModel? SelectedUpcomingOverviewItem
    {
        get => Root.SelectedUpcomingOverviewItem;
        set => Root.SelectedUpcomingOverviewItem = value;
    }

    public string SelectedUpcomingOverviewDetail => Root.SelectedUpcomingOverviewDetail;

    public ICommand OpenSelectedUpcomingOverviewItemCommand => Root.OpenSelectedUpcomingOverviewItemCommand;

    public ICommand OpenSelectedUpcomingOverviewVehicleCommand => Root.OpenSelectedUpcomingOverviewVehicleCommand;
}
