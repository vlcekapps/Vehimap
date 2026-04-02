using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class OverdueOverviewWorkspaceView : WorkspaceViewBase<OverdueOverviewWorkspaceViewModel>
{
    public OverdueOverviewWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("OverdueOverviewSearchBox");
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() => DesktopFocusTarget.OverdueOverviewSearch;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.OverdueOverviewSearch or DesktopFocusTarget.OverdueOverviewList;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.OverdueOverviewSearch => this.FindControl<TextBox>("OverdueOverviewSearchBox"),
            DesktopFocusTarget.OverdueOverviewList => this.FindControl<ListBox>("OverdueOverviewListBox"),
            _ => null
        };
}
