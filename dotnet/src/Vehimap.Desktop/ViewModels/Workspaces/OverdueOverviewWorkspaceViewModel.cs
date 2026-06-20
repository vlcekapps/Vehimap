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
    private string selectedOverdueOverviewFilter = "Vše";

    [ObservableProperty]
    private string selectedOverdueOverviewSortOption = WorkspaceSortHelpers.DateSortLabel;

    [ObservableProperty]
    private bool overdueOverviewSortDescending;

    [ObservableProperty]
    private string overdueOverviewSummary = "Propadlé termíny napříč vozidly se zobrazí po načtení dat.";

    [ObservableProperty]
    private string selectedOverdueOverviewDetail = "Vyberte propadlý termín a můžete přejít na související vozidlo nebo evidenci.";

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedOverdueOverviewItem;

    public string WindowTitle => Root.OverdueOverviewWindowTitle;

    public ObservableCollection<VehicleTimelineItemViewModel> OverdueOverviewItems { get; } = [];

    public IReadOnlyList<string> OverviewFilters { get; } =
    [
        "Vše",
        "Technické kontroly",
        "Zelené karty",
        "Připomínky",
        "Doklady",
        "Údržba"
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
            ? "Vyberte propadlý termín a můžete přejít na související vozidlo nebo evidenci."
            : $"{value.VehicleName}\n{value.Date} | {value.KindLabel}\n{value.Title}\n{value.Detail}\nStav: {value.Status}";

        Root.NotifyOverdueOverviewWorkspaceSelectionChanged();
    }
}
