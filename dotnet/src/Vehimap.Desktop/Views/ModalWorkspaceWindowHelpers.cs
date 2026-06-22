using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels.Workspaces;
using Vehimap.Desktop.Views.Workspaces;

namespace Vehimap.Desktop.Views;

internal static class ModalWorkspaceWindowHelpers
{
    public static void RegisterWorkspaceLifecycle(Window window, string workspaceHostName, string? closeActionDescription = null)
    {
        window.Opened += (_, _) => FocusDefaultWorkspaceControl(window, workspaceHostName);
        window.AddHandler(
            InputElement.KeyDownEvent,
            (_, e) => OnWorkspaceWindowKeyDown(window, e),
            RoutingStrategies.Bubble);

        if (!string.IsNullOrWhiteSpace(closeActionDescription))
        {
            RegisterPendingEditCloseConfirmation(window, closeActionDescription);
        }
    }

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

    private static void FocusDefaultWorkspaceControl(Window window, string workspaceHostName)
    {
        if (window.FindControl<Control>(workspaceHostName) is IWorkspaceView workspaceView)
        {
            Dispatcher.UIThread.Post(workspaceView.FocusDefaultControl, DispatcherPriority.Loaded);
        }
    }

    private static void RegisterPendingEditCloseConfirmation(Window window, string actionDescription)
    {
        var closeConfirmed = false;
        window.Closing += async (_, e) =>
        {
            if (closeConfirmed || !HasPendingEdits(window.DataContext))
            {
                return;
            }

            e.Cancel = true;
            if (!await ConfirmCloseAsync(window.DataContext, actionDescription).ConfigureAwait(true))
            {
                return;
            }

            closeConfirmed = true;
            window.Close();
        };
    }

    private static void OnWorkspaceWindowKeyDown(Window window, KeyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            window.Close();
        }
    }
}
