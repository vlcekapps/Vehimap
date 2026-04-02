using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class RecordWorkspaceView : WorkspaceViewBase<RecordWorkspaceViewModel>
{
    public RecordWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("RecordListBox", "RecordEditorTitleBox", "RecordEditorPathInputBox");
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() =>
        ViewModel?.IsEditingRecord == true ? DesktopFocusTarget.RecordEditorTitle : DesktopFocusTarget.RecordList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.RecordList or DesktopFocusTarget.RecordEditorTitle;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.RecordList => this.FindControl<ListBox>("RecordListBox"),
            DesktopFocusTarget.RecordEditorTitle => this.FindControl<TextBox>("RecordEditorTitleBox"),
            _ => null
        };
}
