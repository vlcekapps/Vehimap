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
}

