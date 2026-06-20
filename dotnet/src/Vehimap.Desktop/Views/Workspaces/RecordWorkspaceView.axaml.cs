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
        ApplyHostMode();
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() =>
        ViewModel?.IsEditingRecord == true ? DesktopFocusTarget.RecordEditorTitle : DesktopFocusTarget.RecordList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.RecordSearch
            or DesktopFocusTarget.RecordList
            or DesktopFocusTarget.RecordEditorType
            or DesktopFocusTarget.RecordEditorTitle
            or DesktopFocusTarget.RecordEditorValidFrom
            or DesktopFocusTarget.RecordEditorValidTo
            or DesktopFocusTarget.RecordEditorPrice
            or DesktopFocusTarget.RecordEditorPathInput;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.RecordSearch => this.FindControl<TextBox>("RecordSearchBox"),
            DesktopFocusTarget.RecordList => this.FindControl<ListBox>("RecordListBox"),
            DesktopFocusTarget.RecordEditorType => this.FindControl<ComboBox>("RecordEditorTypeBox"),
            DesktopFocusTarget.RecordEditorTitle => this.FindControl<TextBox>("RecordEditorTitleBox"),
            DesktopFocusTarget.RecordEditorValidFrom => this.FindControl<TextBox>("RecordEditorValidFromBox"),
            DesktopFocusTarget.RecordEditorValidTo => this.FindControl<TextBox>("RecordEditorValidToBox"),
            DesktopFocusTarget.RecordEditorPrice => this.FindControl<TextBox>("RecordEditorPriceBox"),
            DesktopFocusTarget.RecordEditorPathInput => this.FindControl<TextBox>("RecordEditorPathInputBox"),
            _ => null
        };

    protected override void OnAllowEditingChanged()
    {
        ApplyHostMode();
    }

    private void ApplyHostMode()
    {
        if (this.FindControl<Control>("RecordEditActionPanel") is { } editActions)
        {
            editActions.IsVisible = AllowEditing;
        }

        if (this.FindControl<Control>("RecordEditorHost") is { } editorHost)
        {
            editorHost.IsVisible = AllowEditing;
        }
    }
}
