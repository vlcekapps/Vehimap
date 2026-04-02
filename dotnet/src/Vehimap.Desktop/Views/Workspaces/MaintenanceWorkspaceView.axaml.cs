using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class MaintenanceWorkspaceView : WorkspaceViewBase<MaintenanceWorkspaceViewModel>
{
    public MaintenanceWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("MaintenanceListBox", "MaintenanceEditorTitleBox");
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() =>
        ViewModel?.IsEditingMaintenance == true ? DesktopFocusTarget.MaintenanceEditorTitle : DesktopFocusTarget.MaintenanceList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.MaintenanceList or DesktopFocusTarget.MaintenanceEditorTitle;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.MaintenanceList => this.FindControl<ListBox>("MaintenanceListBox"),
            DesktopFocusTarget.MaintenanceEditorTitle => this.FindControl<TextBox>("MaintenanceEditorTitleBox"),
            _ => null
        };
}
