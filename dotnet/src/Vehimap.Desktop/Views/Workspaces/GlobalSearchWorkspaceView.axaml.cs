using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class GlobalSearchWorkspaceView : WorkspaceViewBase<GlobalSearchWorkspaceViewModel>
{
    public GlobalSearchWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("GlobalSearchTextBox");
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() => DesktopFocusTarget.GlobalSearchBox;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.GlobalSearchBox or DesktopFocusTarget.GlobalSearchList;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.GlobalSearchBox => this.FindControl<TextBox>("GlobalSearchTextBox"),
            DesktopFocusTarget.GlobalSearchList => this.FindControl<ListBox>("SearchResultsListBox"),
            _ => null
        };
}
