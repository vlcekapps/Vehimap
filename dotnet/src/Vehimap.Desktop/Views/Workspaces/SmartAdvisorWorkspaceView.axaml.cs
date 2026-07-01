// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class SmartAdvisorWorkspaceView : WorkspaceViewBase<SmartAdvisorWorkspaceViewModel>
{
    public SmartAdvisorWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("SmartAdvisorSearchBox", "SmartAdvisorListBox");
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() =>
        ViewModel?.VisibleSmartAdvisorItems.Count > 0 ? DesktopFocusTarget.SmartAdvisorList : DesktopFocusTarget.SmartAdvisorSearch;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.SmartAdvisorSearch or DesktopFocusTarget.SmartAdvisorList;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.SmartAdvisorSearch => this.FindControl<TextBox>("SmartAdvisorSearchBox"),
            DesktopFocusTarget.SmartAdvisorList => this.FindControl<ListBox>("SmartAdvisorListBox"),
            _ => null
        };
}
