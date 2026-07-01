// SPDX-License-Identifier: GPL-3.0-or-later
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
    private string timelineSummary = L("TimelineWorkspace.Summary.Initial");

    [ObservableProperty]
    private string timelineSearchText = string.Empty;

    [ObservableProperty]
    private string selectedTimelineFilter = L("TimelineWorkspace.Filter.All");

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedTimelineItem;

    [ObservableProperty]
    private string selectedTimelineDetail = L("TimelineWorkspace.Detail.Empty");

    [ObservableProperty]
    private string exportStatus = L("TimelineWorkspace.ExportStatus.Initial");

    public IReadOnlyList<string> TimelineFilters { get; } =
    [
        L("TimelineWorkspace.Filter.All"),
        L("TimelineWorkspace.Filter.Future"),
        L("TimelineWorkspace.Filter.Past")
    ];

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
            ? L("TimelineWorkspace.Detail.Empty")
            : LF(
                "TimelineWorkspace.Detail.Selected",
                value.Date,
                value.KindLabel,
                value.Title,
                Root.FormatWorkspaceValue(value.Detail, "-"),
                Root.FormatWorkspaceValue(value.Status, "-"),
                Root.FormatWorkspaceValue(value.Note, L("Common.NoNote")));

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
