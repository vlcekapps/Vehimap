using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

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
    private string overdueOverviewSummary = "Propadlé termíny napříč vozidly se zobrazí po načtení dat.";

    [ObservableProperty]
    private string selectedOverdueOverviewDetail = "Vyberte propadlý termín a můžete přejít na související vozidlo nebo evidenci.";

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedOverdueOverviewItem;

    public string WindowTitle => Root.OverdueOverviewWindowTitle;

    public ObservableCollection<VehicleTimelineItemViewModel> OverdueOverviewItems => Root.OverdueOverviewItems;

    public IReadOnlyList<string> OverviewFilters => Root.OverviewFilters;

    public ICommand OpenSelectedOverdueOverviewItemCommand => Root.OpenSelectedOverdueOverviewItemCommand;

    public ICommand OpenSelectedOverdueOverviewVehicleCommand => Root.OpenSelectedOverdueOverviewVehicleCommand;

    partial void OnOverdueOverviewSearchTextChanged(string value)
    {
        Root.HandleOverdueOverviewWorkspaceSearchChanged();
    }

    partial void OnSelectedOverdueOverviewFilterChanged(string value)
    {
        Root.HandleOverdueOverviewWorkspaceFilterChanged();
    }

    partial void OnSelectedOverdueOverviewItemChanged(VehicleTimelineItemViewModel? value)
    {
        SelectedOverdueOverviewDetail = value is null
            ? "Vyberte propadlý termín a můžete přejít na související vozidlo nebo evidenci."
            : $"{value.VehicleName}\n{value.Date} | {value.KindLabel}\n{value.Title}\n{value.Detail}\nStav: {value.Status}";

        Root.NotifyOverdueOverviewWorkspaceSelectionChanged();
    }
}
