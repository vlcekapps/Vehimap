using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

    public ObservableCollection<VehicleTimelineItemViewModel> SelectedVehicleTimeline { get; } = [];

    public string WindowTitle => Root.TimelineWindowTitle;

    public ICommand OpenSelectedTimelineItemCommand => Root.OpenSelectedTimelineItemCommand;
    public bool CanClearTimelineSearch => !string.IsNullOrWhiteSpace(TimelineSearchText);

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.TimelineSearch);
    }

    [RelayCommand]
    private void RefreshTimeline()
    {
        Root.RefreshTimelineWorkspace();
    }

    [RelayCommand(CanExecute = nameof(CanClearTimelineSearch))]
    private void ClearTimelineSearch()
    {
        TimelineSearchText = string.Empty;
        RequestFocus(DesktopFocusTarget.TimelineSearch);
    }

    partial void OnSelectedTimelineItemChanged(VehicleTimelineItemViewModel? value)
    {
        SelectedTimelineDetail = value is null
            ? "Vyberte položku časové osy a zobrazí se detail."
            : $"Datum: {value.Date}\nDruh: {value.KindLabel}\nPoložka: {value.Title}\nDetail: {Root.FormatWorkspaceValue(value.Detail, "-")}\nStav: {Root.FormatWorkspaceValue(value.Status, "-")}\nPoznámka: {Root.FormatWorkspaceValue(value.Note, "bez poznámky")}";

        Root.NotifyTimelineWorkspaceSelectionChanged();
    }

    partial void OnTimelineSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanClearTimelineSearch));
        ClearTimelineSearchCommand.NotifyCanExecuteChanged();
        Root.HandleTimelineWorkspaceSearchChanged();
    }

    partial void OnSelectedTimelineFilterChanged(string value)
    {
        Root.HandleTimelineWorkspaceFilterChanged();
    }
}
