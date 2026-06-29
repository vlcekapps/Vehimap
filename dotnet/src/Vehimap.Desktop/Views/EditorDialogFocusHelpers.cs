using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Vehimap.Desktop.Views;

internal static class EditorDialogFocusHelpers
{
    public static void RegisterEditorDialog(
        Window window,
        string firstControlName,
        string cancelButtonName,
        Action cancelAction)
    {
        KeyboardAccessibilityHelper.RegisterWindow(window);
        window.Opened += (_, _) => FocusControl(window, firstControlName);

        window.AddHandler(
            InputElement.KeyDownEvent,
            (_, e) =>
            {
                if (KeyboardAccessibilityHelper.ShouldSkipGlobalShortcut(e))
                {
                    return;
                }

                if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
                {
                    e.Handled = true;
                    cancelAction();
                }
            },
            RoutingStrategies.Tunnel);

        if (window.FindControl<Control>(firstControlName) is { } firstControl)
        {
            firstControl.AddHandler(
                InputElement.KeyDownEvent,
                (_, e) =>
                {
                    if (e.Key != Key.Tab || !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    {
                        return;
                    }

                    FocusControl(window, cancelButtonName);
                    e.Handled = true;
                },
                RoutingStrategies.Tunnel);
        }
    }

    public static void FocusControl(Window window, string controlName)
    {
        Dispatcher.UIThread.Post(
            () => window.FindControl<Control>(controlName)?.Focus(NavigationMethod.Unspecified, KeyModifiers.None),
            DispatcherPriority.Background);
    }
}
