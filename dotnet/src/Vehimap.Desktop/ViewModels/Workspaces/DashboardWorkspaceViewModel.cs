using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class DashboardWorkspaceViewModel : WorkspaceViewModelBase
{
    private bool _syncingShowDashboardOnLaunch;

    public DashboardWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    [ObservableProperty]
    private bool showDashboardOnLaunch;

    [ObservableProperty]
    private string dashboardTimelineSummary = "Nejbližší termíny napříč vozidly se zobrazí po načtení dat.";

    [ObservableProperty]
    private string selectedDashboardTimelineDetail = "Vyberte nejbližší termín a můžete přejít na související vozidlo nebo evidenci.";

    [ObservableProperty]
    private VehicleTimelineItemViewModel? selectedDashboardTimelineItem;

    public string WindowTitle => Root.DashboardWindowTitle;

    public string AuditSummary => Root.AuditWorkspace.AuditSummary;

    public string CostSummary => Root.CostWorkspace.CostSummary;

    public string CostComparison => Root.CostWorkspace.CostComparison;

    public ObservableCollection<AuditItemViewModel> AuditItems { get; } = [];

    public ObservableCollection<CostVehicleItemViewModel> CostVehicles => Root.CostWorkspace.CostVehicles;

    public ObservableCollection<VehicleTimelineItemViewModel> DashboardUpcomingTimeline { get; } = [];

    public AuditItemViewModel? SelectedDashboardAuditItem
    {
        get => Root.AuditWorkspace.SelectedDashboardAuditItem;
        set => Root.AuditWorkspace.SelectedDashboardAuditItem = value;
    }

    public CostVehicleItemViewModel? SelectedDashboardCostVehicle
    {
        get => Root.CostWorkspace.SelectedDashboardCostVehicle;
        set => Root.CostWorkspace.SelectedDashboardCostVehicle = value;
    }

    public ICommand OpenSelectedDashboardAuditItemCommand => Root.OpenSelectedDashboardAuditItemCommand;

    public ICommand OpenSelectedDashboardCostVehicleCommand => Root.OpenSelectedDashboardCostVehicleCommand;

    public ICommand OpenSelectedDashboardTimelineItemCommand => Root.OpenSelectedDashboardTimelineItemCommand;

    public ICommand OpenSelectedDashboardVehicleCommand => Root.OpenSelectedDashboardVehicleCommand;

    public ICommand EditSelectedDashboardVehicleCommand => Root.EditSelectedDashboardVehicleCommand;

    public ICommand FocusGlobalSearchCommand => Root.FocusGlobalSearchCommand;

    public ICommand FocusUpcomingOverviewCommand => Root.FocusUpcomingOverviewCommand;

    public ICommand FocusOverdueOverviewCommand => Root.FocusOverdueOverviewCommand;

    [RelayCommand]
    private void RefreshDashboard()
    {
        Root.RefreshDashboardWorkspace();
    }

    internal void NotifyDashboardSummariesChanged()
    {
        OnPropertyChanged(nameof(AuditSummary));
        OnPropertyChanged(nameof(CostSummary));
        OnPropertyChanged(nameof(CostComparison));
    }

    internal void NotifyDashboardAuditSelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedDashboardAuditItem));
    }

    internal void NotifyDashboardCostSelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedDashboardCostVehicle));
    }

    internal void SyncShowDashboardOnLaunch(bool value)
    {
        _syncingShowDashboardOnLaunch = true;
        try
        {
            ShowDashboardOnLaunch = value;
        }
        finally
        {
            _syncingShowDashboardOnLaunch = false;
        }
    }

    partial void OnShowDashboardOnLaunchChanged(bool value)
    {
        if (_syncingShowDashboardOnLaunch)
        {
            return;
        }

        _ = Root.SetDashboardShowOnLaunchAsync(value);
    }

    partial void OnSelectedDashboardTimelineItemChanged(VehicleTimelineItemViewModel? value)
    {
        SelectedDashboardTimelineDetail = value is null
            ? "Vyberte nejbližší termín a můžete přejít na související vozidlo nebo evidenci."
            : $"Vozidlo: {value.VehicleName}\nDatum: {value.Date}\nDruh: {value.KindLabel}\nPoložka: {value.Title}\nStav: {Root.FormatWorkspaceValue(value.Status, "-")}\nDetail: {Root.FormatWorkspaceValue(value.Detail, "-")}";

        Root.NotifyDashboardWorkspaceTimelineSelectionChanged();
    }
}
