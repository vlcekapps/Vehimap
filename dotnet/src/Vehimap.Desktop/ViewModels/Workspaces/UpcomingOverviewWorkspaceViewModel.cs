using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

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
    private string selectedUpcomingOverviewFilter = "Vše";

    [ObservableProperty]
    private string upcomingOverviewSummary = "Blížící se termíny napříč vozidly se zobrazí po načtení dat.";

    [ObservableProperty]
    private string selectedUpcomingOverviewDetail = "Vyberte termín a můžete přejít na související vozidlo nebo evidenci.";

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedUpcomingOverviewItem;

    public ObservableCollection<VehicleTimelineItemViewModel> UpcomingOverviewItems => Root.UpcomingOverviewItems;

    public IReadOnlyList<string> OverviewFilters => Root.OverviewFilters;

    public ICommand OpenSelectedUpcomingOverviewItemCommand => Root.OpenSelectedUpcomingOverviewItemCommand;

    public ICommand OpenSelectedUpcomingOverviewVehicleCommand => Root.OpenSelectedUpcomingOverviewVehicleCommand;

    partial void OnUpcomingOverviewSearchTextChanged(string value)
    {
        Root.HandleUpcomingOverviewWorkspaceSearchChanged();
    }

    partial void OnSelectedUpcomingOverviewFilterChanged(string value)
    {
        Root.HandleUpcomingOverviewWorkspaceFilterChanged();
    }

    partial void OnSelectedUpcomingOverviewItemChanged(VehicleTimelineItemViewModel? value)
    {
        SelectedUpcomingOverviewDetail = value is null
            ? "Vyberte termín a můžete přejít na související vozidlo nebo evidenci."
            : $"{value.VehicleName}\n{value.Date} | {value.KindLabel}\n{value.Title}\n{value.Detail}\nStav: {value.Status}";

        Root.NotifyUpcomingOverviewWorkspaceSelectionChanged();
    }
}
