using Avalonia.Controls;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views;

internal static class ModalWorkspaceWindowHelpers
{
    public static bool HasPendingEdits(object? dataContext)
    {
        return dataContext is WorkspaceViewModelBase { HasPendingEdits: true };
    }

    public static async Task<bool> ConfirmCloseAsync(object? dataContext, string actionDescription)
    {
        if (dataContext is not WorkspaceViewModelBase workspace || !workspace.HasPendingEdits)
        {
            return true;
        }

        if (!await workspace.ConfirmDiscardPendingEditsAsync(actionDescription).ConfigureAwait(true))
        {
            return false;
        }

        workspace.DiscardPendingEdits(clearStatus: false);
        return true;
    }
}
