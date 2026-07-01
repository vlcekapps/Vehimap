// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class DashboardWorkspaceView : WorkspaceViewBase<DashboardWorkspaceViewModel>
{
    private DashboardWorkspaceViewModel? _subscribedViewModel;

    public DashboardWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("DashboardRefreshButton", "DashboardAuditOpenButton", "DashboardCostOpenButton", "DashboardTimelineOpenButton");
        DataContextChanged += OnDashboardDataContextChanged;
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() => DesktopFocusTarget.DashboardAuditList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.DashboardAuditList or DesktopFocusTarget.DashboardCostList or DesktopFocusTarget.DashboardTimelineList;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.DashboardAuditList => this.FindControl<ListBox>("DashboardAuditListBox"),
            DesktopFocusTarget.DashboardCostList => this.FindControl<ListBox>("DashboardCostListBox"),
            DesktopFocusTarget.DashboardTimelineList => this.FindControl<ListBox>("DashboardTimelineListBox"),
            _ => null
        };

    private void OnDashboardDataContextChanged(object? sender, EventArgs e)
    {
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.DashboardMaintenanceCompletionRequested -= OnDashboardMaintenanceCompletionRequested;
        }

        _subscribedViewModel = DataContext as DashboardWorkspaceViewModel;
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.DashboardMaintenanceCompletionRequested += OnDashboardMaintenanceCompletionRequested;
        }
    }

    private async void OnDashboardMaintenanceCompletionRequested(object? sender, EventArgs e)
    {
        await OpenDashboardMaintenanceCompletionDialogAsync();
    }

    private async Task OpenDashboardMaintenanceCompletionDialogAsync()
    {
        if (ViewModel is null)
        {
            return;
        }

        var dialogViewModel = ViewModel.BuildDashboardMaintenanceCompletionDialogViewModel();
        if (dialogViewModel is null)
        {
            ViewModel.SetDashboardMaintenanceStatus(
                DesktopLocalization.Localizer.GetString("DashboardWorkspace.Status.SelectMaintenancePlan"));
            return;
        }

        if (TopLevel.GetTopLevel(this) is not Window owner)
        {
            return;
        }

        var dialog = new MaintenanceCompletionWindow
        {
            DataContext = dialogViewModel
        };

        var result = await dialog.ShowDialog<MaintenanceCompletionDialogResult?>(owner);
        if (result is null)
        {
            return;
        }

        var message = await ViewModel.ApplyDashboardMaintenanceCompletionAsync(result);
        ViewModel.SetDashboardMaintenanceStatus(message);
    }
}
