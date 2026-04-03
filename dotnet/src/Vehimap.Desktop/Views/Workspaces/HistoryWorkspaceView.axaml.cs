using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public partial class HistoryWorkspaceView : WorkspaceViewBase<HistoryWorkspaceViewModel>
{
    public HistoryWorkspaceView()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterShiftTabBackNavigation("HistoryListBox", "HistoryEditorDateBox");
        ApplyHostMode();
    }

    protected override DesktopFocusTarget? GetDefaultFocusTarget() =>
        ViewModel?.IsEditingHistory == true ? DesktopFocusTarget.HistoryEditorDate : DesktopFocusTarget.HistoryList;

    protected override bool SupportsFocusTarget(DesktopFocusTarget target) =>
        target is DesktopFocusTarget.HistoryList or DesktopFocusTarget.HistoryEditorDate;

    protected override Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target switch
        {
            DesktopFocusTarget.HistoryList => this.FindControl<ListBox>("HistoryListBox"),
            DesktopFocusTarget.HistoryEditorDate => this.FindControl<TextBox>("HistoryEditorDateBox"),
            _ => null
        };

    protected override void OnAllowEditingChanged()
    {
        ApplyHostMode();
    }

    private void ApplyHostMode()
    {
        if (this.FindControl<Control>("HistoryActionPanel") is { } actionPanel)
        {
            actionPanel.IsVisible = AllowEditing;
        }

        if (this.FindControl<Control>("HistoryEditorHost") is { } editorHost)
        {
            editorHost.IsVisible = AllowEditing;
        }
    }
}
