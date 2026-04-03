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
        ApplyHostMode();
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

    protected override void OnAllowEditingChanged()
    {
        ApplyHostMode();
    }

    private void ApplyHostMode()
    {
        if (this.FindControl<Control>("ReminderActionPanel") is { } actionPanel)
        {
            actionPanel.IsVisible = AllowEditing;
        }

        if (this.FindControl<Control>("ReminderEditorHost") is { } editorHost)
        {
            editorHost.IsVisible = AllowEditing;
        }
    }
}
