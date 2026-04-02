using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class CostWorkspaceView : WorkspaceViewBase<CostWorkspaceViewModel>
{
    public CostWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("CostListBox");
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() => DesktopFocusTarget.CostList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target == DesktopFocusTarget.CostList;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target == DesktopFocusTarget.CostList
            ? this.FindControl<ListBox>("CostListBox")
            : null;
}
