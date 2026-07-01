// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class OverdueOverviewWorkspaceViewModel : WorkspaceViewModelBase
{
    public OverdueOverviewWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    [ObservableProperty]
    private string overdueOverviewSearchText = string.Empty;

    [ObservableProperty]
    private string selectedOverdueOverviewFilter = L("Overview.Filter.All");

    [ObservableProperty]
    private string selectedOverdueOverviewSortOption = WorkspaceSortHelpers.DateSortLabel;

    [ObservableProperty]
    private bool overdueOverviewSortDescending;

    [ObservableProperty]
    private string overdueOverviewSummary = L("Overview.Summary.OverdueInitial");

    [ObservableProperty]
    private string selectedOverdueOverviewDetail = L("Overview.Detail.EmptyOverdue");

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedOverdueOverviewItem;

    public string WindowTitle => Root.OverdueOverviewWindowTitle;

    public ObservableCollection<VehicleTimelineItemViewModel> OverdueOverviewItems { get; } = [];

    public IReadOnlyList<string> OverviewFilters { get; } =
    [
        L("Overview.Filter.All"),
        L("Overview.Filter.Technical"),
        L("Overview.Filter.GreenCards"),
        L("Overview.Filter.Reminders"),
        L("Overview.Filter.Records"),
        L("Overview.Filter.Maintenance")
    ];

    public IReadOnlyList<string> OverviewSortOptions => WorkspaceSortHelpers.TimelineOverviewSortOptions;

    public ICommand OpenSelectedOverdueOverviewItemCommand => Root.OpenSelectedOverdueOverviewItemCommand;

    public ICommand OpenSelectedOverdueOverviewVehicleCommand => Root.OpenSelectedOverdueOverviewVehicleCommand;
    public bool CanClearOverdueOverviewSearch => !string.IsNullOrWhiteSpace(OverdueOverviewSearchText);

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.OverdueOverviewSearch);
    }

    [RelayCommand]
    private void RefreshOverdueOverview()
    {
        Root.RefreshOverdueOverviewWorkspace();
    }

    [RelayCommand(CanExecute = nameof(CanClearOverdueOverviewSearch))]
    private void ClearOverdueOverviewSearch()
    {
        OverdueOverviewSearchText = string.Empty;
        RequestFocus(DesktopFocusTarget.OverdueOverviewSearch);
    }

    partial void OnOverdueOverviewSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanClearOverdueOverviewSearch));
        ClearOverdueOverviewSearchCommand.NotifyCanExecuteChanged();
        Root.HandleOverdueOverviewWorkspaceSearchChanged();
    }

    partial void OnSelectedOverdueOverviewFilterChanged(string value)
    {
        Root.HandleOverdueOverviewWorkspaceFilterChanged();
    }

    partial void OnSelectedOverdueOverviewSortOptionChanged(string value)
    {
        Root.HandleOverdueOverviewWorkspaceSortChanged();
    }

    partial void OnOverdueOverviewSortDescendingChanged(bool value)
    {
        Root.HandleOverdueOverviewWorkspaceSortChanged();
    }

    partial void OnSelectedOverdueOverviewItemChanged(VehicleTimelineItemViewModel? value)
    {
        SelectedOverdueOverviewDetail = value is null
            ? L("Overview.Detail.EmptyOverdue")
            : LF(
                "Overview.Detail.Selected",
                value.VehicleName,
                value.Date,
                value.KindLabel,
                value.Title,
                Root.FormatWorkspaceValue(value.Detail, "-"),
                Root.FormatWorkspaceValue(value.Status, "-"));

        Root.NotifyOverdueOverviewWorkspaceSelectionChanged();
    }
}
