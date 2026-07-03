// SPDX-License-Identifier: GPL-3.0-or-later
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
    private LocalizedOptionViewModel selectedUpcomingOverviewFilter = OverviewFilterOptions.All;

    [ObservableProperty]
    private LocalizedOptionViewModel selectedUpcomingOverviewSortOption = WorkspaceSortHelpers.DateSortOption;

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

    public IReadOnlyList<LocalizedOptionViewModel> OverviewFilters => OverviewFilterOptions.AllOptions;

    public IReadOnlyList<LocalizedOptionViewModel> OverviewSortOptions => WorkspaceSortHelpers.TimelineOverviewSortOptions;

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

    partial void OnSelectedUpcomingOverviewFilterChanged(LocalizedOptionViewModel value)
    {
        Root.HandleUpcomingOverviewWorkspaceFilterChanged();
    }

    partial void OnSelectedUpcomingOverviewSortOptionChanged(LocalizedOptionViewModel value)
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
