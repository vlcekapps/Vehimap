using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class UpcomingOverviewWorkspaceView : WorkspaceViewBase<UpcomingOverviewWorkspaceViewModel>
{
    public UpcomingOverviewWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("UpcomingOverviewSearchBox");
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() => DesktopFocusTarget.UpcomingOverviewSearch;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.UpcomingOverviewSearch or DesktopFocusTarget.UpcomingOverviewList;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.UpcomingOverviewSearch => this.FindControl<TextBox>("UpcomingOverviewSearchBox"),
            DesktopFocusTarget.UpcomingOverviewList => this.FindControl<ListBox>("UpcomingOverviewListBox"),
            _ => null
        };
}
