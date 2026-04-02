using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class DashboardWorkspaceView : WorkspaceViewBase<DashboardWorkspaceViewModel>
{
    public DashboardWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("DashboardAuditOpenButton", "DashboardCostOpenButton", "DashboardTimelineOpenButton");
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
}
