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
        target is DesktopFocusTarget.CostList or DesktopFocusTarget.CostDetail;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) => target switch
    {
        DesktopFocusTarget.CostList => this.FindControl<ListBox>("CostListBox"),
        DesktopFocusTarget.CostDetail => this.FindControl<Control>("SelectedCostVehicleDetailHost"),
        _ => null
    };
}
