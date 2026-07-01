// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class DashboardWorkspaceViewModel : WorkspaceViewModelBase
{
    private bool _syncingShowDashboardOnLaunch;

    public DashboardWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public event EventHandler? DashboardMaintenanceCompletionRequested;

    [ObservableProperty]
    private bool showDashboardOnLaunch;

    [ObservableProperty]
    private string dashboardTimelineSummary = L("Overview.Summary.DashboardInitial");

    [ObservableProperty]
    private string selectedDashboardTimelineDetail = L("DashboardTimeline.Detail.Empty");

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

    public ICommand OpenDashboardCostOverviewCommand => Root.OpenDashboardCostOverviewCommand;

    public ICommand OpenSelectedDashboardVehicleHistoryCommand => Root.OpenSelectedDashboardVehicleHistoryCommand;

    public ICommand OpenSelectedDashboardVehicleCostsCommand => Root.OpenSelectedDashboardVehicleCostsCommand;

    public ICommand FocusGlobalSearchCommand => Root.FocusGlobalSearchCommand;

    public ICommand FocusUpcomingOverviewCommand => Root.FocusUpcomingOverviewCommand;

    public ICommand FocusOverdueOverviewCommand => Root.FocusOverdueOverviewCommand;

    public bool CanCompleteSelectedDashboardMaintenance => Root.CanCompleteSelectedDashboardMaintenance;

    [RelayCommand]
    private void RefreshDashboard()
    {
        Root.RefreshDashboardWorkspace();
    }

    [RelayCommand(CanExecute = nameof(CanCompleteSelectedDashboardMaintenance))]
    private async Task CompleteSelectedDashboardMaintenance()
    {
        if (await Root.SelectDashboardMaintenanceForCompletionAsync().ConfigureAwait(true))
        {
            DashboardMaintenanceCompletionRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public MaintenanceCompletionDialogViewModel? BuildDashboardMaintenanceCompletionDialogViewModel() =>
        Root.BuildMaintenanceCompletionDialogViewModel();

    public Task<string> ApplyDashboardMaintenanceCompletionAsync(MaintenanceCompletionDialogResult result) =>
        Root.ApplyMaintenanceCompletionAsync(result);

    public void SetDashboardMaintenanceStatus(string message)
    {
        Root.MaintenanceWorkspace.MaintenanceEditorStatus = message;
        Root.ShellStatus = message;
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

    internal void NotifyDashboardActionStateChanged()
    {
        OnPropertyChanged(nameof(CanCompleteSelectedDashboardMaintenance));
        CompleteSelectedDashboardMaintenanceCommand.NotifyCanExecuteChanged();
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
            ? L("DashboardTimeline.Detail.Empty")
            : LF(
                "DashboardTimeline.Detail.Selected",
                value.VehicleName,
                value.Date,
                value.KindLabel,
                value.Title,
                Root.FormatWorkspaceValue(value.Status, "-"),
                Root.FormatWorkspaceValue(value.Detail, "-"));

        Root.NotifyDashboardWorkspaceTimelineSelectionChanged();
        NotifyDashboardActionStateChanged();
    }
}
