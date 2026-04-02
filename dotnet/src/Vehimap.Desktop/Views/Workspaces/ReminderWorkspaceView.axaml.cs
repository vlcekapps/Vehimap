using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class ReminderWorkspaceView : WorkspaceViewBase<ReminderWorkspaceViewModel>
{
    public ReminderWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("ReminderListBox", "ReminderEditorTitleBox");
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() =>
        ViewModel?.IsEditingReminder == true ? DesktopFocusTarget.ReminderEditorTitle : DesktopFocusTarget.ReminderList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.ReminderList or DesktopFocusTarget.ReminderEditorTitle;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.ReminderList => this.FindControl<ListBox>("ReminderListBox"),
            DesktopFocusTarget.ReminderEditorTitle => this.FindControl<TextBox>("ReminderEditorTitleBox"),
            _ => null
        };
}

