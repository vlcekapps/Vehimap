using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class TimelineWorkspaceViewModel : WorkspaceViewModelBase
{
    public TimelineWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string TimelineSummary => Root.TimelineSummary;

    public IReadOnlyList<string> TimelineFilters => Root.TimelineFilters;

    public string SelectedTimelineFilter
    {
        get => Root.SelectedTimelineFilter;
        set => Root.SelectedTimelineFilter = value;
    }

    public string TimelineSearchText
    {
        get => Root.TimelineSearchText;
        set => Root.TimelineSearchText = value;
    }

    public ObservableCollection<VehicleTimelineItemViewModel> SelectedVehicleTimeline => Root.SelectedVehicleTimeline;

    public VehicleTimelineItemViewModel? SelectedTimelineItem
    {
        get => Root.SelectedTimelineItem;
        set => Root.SelectedTimelineItem = value;
    }

    public string SelectedTimelineDetail => Root.SelectedTimelineDetail;

    public string ExportStatus => Root.ExportStatus;

    public ICommand OpenSelectedTimelineItemCommand => Root.OpenSelectedTimelineItemCommand;
}
