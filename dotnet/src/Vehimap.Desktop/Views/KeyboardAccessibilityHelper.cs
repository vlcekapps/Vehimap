using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Vehimap.Desktop.Views;

internal static class KeyboardAccessibilityHelper
{
    public static void RegisterWindow(Window window)
    {
        window.AddHandler(
            InputElement.KeyDownEvent,
            OnPreviewKeyboardAccessibilityKeyDown,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }

    public static bool ShouldSkipGlobalShortcut(KeyEventArgs e)
    {
        if (e.Handled)
        {
            return true;
        }

        if (TryPrepareComboBoxArrowNavigation(e))
        {
            return true;
        }

        return IsTextBoxEditingKey(e);
    }

    private static void OnPreviewKeyboardAccessibilityKeyDown(object? sender, KeyEventArgs e)
    {
        TryPrepareComboBoxArrowNavigation(e);
    }

    private static bool TryPrepareComboBoxArrowNavigation(KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.None || e.Key is not (Key.Up or Key.Down))
        {
            return false;
        }

        var comboBox = FindSourceControl<ComboBox>(e.Source);
        if (comboBox is null || !comboBox.IsEnabled || !comboBox.IsVisible)
        {
            return false;
        }

        if (!comboBox.IsDropDownOpen)
        {
            comboBox.IsDropDownOpen = true;
            e.Handled = true;
        }

        return true;
    }

    private static bool IsTextBoxEditingKey(KeyEventArgs e)
    {
        if (FindSourceControl<TextBox>(e.Source) is null || e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            return false;
        }

        var hasControl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var hasShift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var supportedModifiers = hasControl ? KeyModifiers.Control : KeyModifiers.None;
        if (hasShift)
        {
            supportedModifiers |= KeyModifiers.Shift;
        }

        if ((e.KeyModifiers & ~supportedModifiers) != KeyModifiers.None)
        {
            return false;
        }

        if (!hasControl)
        {
            return e.Key is Key.Left or Key.Right or Key.Up or Key.Down
                or Key.Home or Key.End or Key.PageUp or Key.PageDown
                or Key.Back or Key.Delete or Key.Insert;
        }

        return e.Key is Key.Left or Key.Right or Key.Up or Key.Down
            or Key.Home or Key.End or Key.Back or Key.Delete
            or Key.A or Key.C or Key.X or Key.V or Key.Z or Key.Y;
    }

    private static TControl? FindSourceControl<TControl>(object? source)
        where TControl : Control
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        for (var current = source; current is not null && visited.Add(current);)
        {
            if (current is TControl control)
            {
                return control;
            }

            if (current is StyledElement { TemplatedParent: TControl templatedControl })
            {
                return templatedControl;
            }

            if (current is StyledElement styledElement)
            {
                current = styledElement.Parent
                    ?? styledElement.TemplatedParent
                    ?? (styledElement as Visual)?.GetVisualParent();
                continue;
            }

            if (current is Visual visual)
            {
                current = visual.GetVisualParent();
                continue;
            }

            break;
        }

        return null;
    }
}
