using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class TimelineWorkspaceView : WorkspaceViewBase<TimelineWorkspaceViewModel>
{
    public TimelineWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("TimelineFilterComboBox");
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() => DesktopFocusTarget.TimelineSearch;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.TimelineSearch or DesktopFocusTarget.TimelineList;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.TimelineSearch => this.FindControl<TextBox>("TimelineSearchBox"),
            DesktopFocusTarget.TimelineList => this.FindControl<ListBox>("TimelineListBox"),
            _ => null
        };
}
