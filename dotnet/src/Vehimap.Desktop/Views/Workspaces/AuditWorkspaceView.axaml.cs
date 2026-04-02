using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class AuditWorkspaceView : WorkspaceViewBase<AuditWorkspaceViewModel>
{
    public AuditWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("AuditListBox");
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() => DesktopFocusTarget.AuditList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) => target == DesktopFocusTarget.AuditList;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target == DesktopFocusTarget.AuditList ? this.FindControl<ListBox>("AuditListBox") : null;
}
