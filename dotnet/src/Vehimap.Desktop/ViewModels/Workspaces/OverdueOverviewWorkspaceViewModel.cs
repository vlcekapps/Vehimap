using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class OverdueOverviewWorkspaceViewModel : WorkspaceViewModelBase
{
    public OverdueOverviewWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string OverdueOverviewSummary => Root.OverdueOverviewSummary;

    public string OverdueOverviewSearchText
    {
        get => Root.OverdueOverviewSearchText;
        set => Root.OverdueOverviewSearchText = value;
    }

    public IReadOnlyList<string> OverviewFilters => Root.OverviewFilters;

    public string SelectedOverdueOverviewFilter
    {
        get => Root.SelectedOverdueOverviewFilter;
        set => Root.SelectedOverdueOverviewFilter = value;
    }

    public ObservableCollection<VehicleTimelineItemViewModel> OverdueOverviewItems => Root.OverdueOverviewItems;

    public VehicleTimelineItemViewModel? SelectedOverdueOverviewItem
    {
        get => Root.SelectedOverdueOverviewItem;
        set => Root.SelectedOverdueOverviewItem = value;
    }

    public string SelectedOverdueOverviewDetail => Root.SelectedOverdueOverviewDetail;

    public ICommand OpenSelectedOverdueOverviewItemCommand => Root.OpenSelectedOverdueOverviewItemCommand;

    public ICommand OpenSelectedOverdueOverviewVehicleCommand => Root.OpenSelectedOverdueOverviewVehicleCommand;
}
