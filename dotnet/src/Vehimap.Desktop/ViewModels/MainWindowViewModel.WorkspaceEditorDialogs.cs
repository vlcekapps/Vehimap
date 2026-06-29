namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private DesktopFocusTarget? _nextWorkspaceEditorReturnFocusTarget;

    internal event EventHandler<WorkspaceEditorDialogRequest>? WorkspaceEditorDialogRequested;

    internal void SetNextWorkspaceEditorReturnFocusTarget(DesktopFocusTarget target)
    {
        _nextWorkspaceEditorReturnFocusTarget = target;
    }

    private void RequestWorkspaceEditorDialog(WorkspaceEditorKind kind, DesktopFocusTarget defaultReturnFocusTarget)
    {
        var returnFocusTarget = _nextWorkspaceEditorReturnFocusTarget ?? defaultReturnFocusTarget;
        _nextWorkspaceEditorReturnFocusTarget = null;
        WorkspaceEditorDialogRequested?.Invoke(this, new WorkspaceEditorDialogRequest(kind, returnFocusTarget));
    }
}
