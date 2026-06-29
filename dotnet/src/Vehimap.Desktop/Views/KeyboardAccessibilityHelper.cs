using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Vehimap.Desktop.Views;

internal static class KeyboardAccessibilityHelper
{
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
        for (var current = source as StyledElement; current is not null; current = current.Parent)
        {
            if (current is TControl control)
            {
                return control;
            }
        }

        return source as TControl;
    }
}
