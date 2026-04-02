using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class VehicleDetailWorkspaceView : WorkspaceViewBase<VehicleDetailWorkspaceViewModel>
{
    public VehicleDetailWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("VehicleEditorNameBox");
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() =>
        ViewModel?.IsEditingVehicle == true ? DesktopFocusTarget.VehicleEditorName : null;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target == DesktopFocusTarget.VehicleEditorName;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target == DesktopFocusTarget.VehicleEditorName ? this.FindControl<TextBox>("VehicleEditorNameBox") : null;
}

