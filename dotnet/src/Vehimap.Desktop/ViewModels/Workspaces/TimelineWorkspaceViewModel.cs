using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class TimelineWorkspaceViewModel : WorkspaceViewModelBase
{
    public TimelineWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    [ObservableProperty]
    private string timelineSummary = "Časová osa vybraného vozidla se zobrazí po výběru vozidla.";

    [ObservableProperty]
    private string timelineSearchText = string.Empty;

    [ObservableProperty]
    private string selectedTimelineFilter = "Vše";

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedTimelineItem;

    [ObservableProperty]
    private string selectedTimelineDetail = "Vyberte položku časové osy a zobrazí se detail.";

    [ObservableProperty]
    private string exportStatus = "Kalendářový export zatím nebyl spuštěn.";

    public IReadOnlyList<string> TimelineFilters { get; } = ["Vše", "Budoucí", "Minulé"];

    public ObservableCollection<VehicleTimelineItemViewModel> SelectedVehicleTimeline => Root.SelectedVehicleTimeline;

    public string WindowTitle => Root.TimelineWindowTitle;

    public ICommand OpenSelectedTimelineItemCommand => Root.OpenSelectedTimelineItemCommand;

    partial void OnSelectedTimelineItemChanged(VehicleTimelineItemViewModel? value)
    {
        SelectedTimelineDetail = value is null
            ? "Vyberte položku časové osy a zobrazí se detail."
            : $"Datum: {value.Date}\nDruh: {value.KindLabel}\nPoložka: {value.Title}\nDetail: {Root.FormatWorkspaceValue(value.Detail, "-")}\nStav: {Root.FormatWorkspaceValue(value.Status, "-")}\nPoznámka: {Root.FormatWorkspaceValue(value.Note, "bez poznámky")}";

        Root.NotifyTimelineWorkspaceSelectionChanged();
    }

    partial void OnTimelineSearchTextChanged(string value)
    {
        Root.HandleTimelineWorkspaceSearchChanged();
    }

    partial void OnSelectedTimelineFilterChanged(string value)
    {
        Root.HandleTimelineWorkspaceFilterChanged();
    }
}
