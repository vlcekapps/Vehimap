// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Runtime.CompilerServices;

namespace Vehimap.Desktop.Views;

internal static class KeyboardAccessibilityHelper
{
    private static readonly ConditionalWeakTable<Window, TextBlock> TextEditingLiveRegions = new();

    public static void RegisterWindow(Window window)
    {
        if (TextEditingLiveRegions.TryGetValue(window, out _))
        {
            return;
        }

        var liveRegion = CreateTextEditingLiveRegion();
        TextEditingLiveRegions.Add(window, liveRegion);
        InstallTextEditingLiveRegion(window, liveRegion);

        window.AddHandler(
            InputElement.KeyDownEvent,
            OnPreviewKeyboardAccessibilityKeyDown,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
        window.AddHandler(
            InputElement.KeyUpEvent,
            OnTextBoxNavigationKeyUp,
            RoutingStrategies.Bubble,
            handledEventsToo: true);
        window.Opened += (_, _) => InstallTextEditingLiveRegion(window, liveRegion);
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

    private static void OnTextBoxNavigationKeyUp(object? sender, KeyEventArgs e)
    {
        if (sender is not Window window
            || !TextEditingLiveRegions.TryGetValue(window, out var liveRegion)
            || !IsTextBoxAnnouncementKey(e)
            || FindSourceControl<TextBox>(e.Source) is not { } textBox)
        {
            return;
        }

        Dispatcher.UIThread.Post(
            () =>
            {
                var fieldName = AutomationProperties.GetName(textBox);
                AnnounceTextEditingState(
                    liveRegion,
                    BuildTextBoxEditingAnnouncement(
                        fieldName,
                        textBox.Text,
                        textBox.CaretIndex,
                        textBox.SelectionStart,
                        textBox.SelectionEnd));
            },
            DispatcherPriority.Background);
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

    private static bool IsTextBoxAnnouncementKey(KeyEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
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

        if (e.Key is Key.Left or Key.Right or Key.Up or Key.Down
            or Key.Home or Key.End or Key.Back or Key.Delete)
        {
            return true;
        }

        return hasControl && e.Key == Key.A;
    }

    internal static string BuildTextBoxEditingAnnouncement(
        string? fieldName,
        string? text,
        int caretIndex,
        int selectionStart,
        int selectionEnd)
    {
        var label = string.IsNullOrWhiteSpace(fieldName) ? "Textové pole" : fieldName.Trim();
        var value = text ?? string.Empty;
        var length = value.Length;
        var caret = Math.Clamp(caretIndex, 0, length);
        var start = Math.Clamp(Math.Min(selectionStart, selectionEnd), 0, length);
        var end = Math.Clamp(Math.Max(selectionStart, selectionEnd), 0, length);

        if (start != end)
        {
            var selected = value[start..end];
            return $"{label}: vybráno {end - start} znaků, {DescribeSnippet(selected)}.";
        }

        if (length == 0)
        {
            return $"{label}: prázdné pole.";
        }

        if (caret == 0)
        {
            return $"{label}: začátek textu, před znakem {DescribeCharacter(value[0])}, pozice 0 z {length}.";
        }

        if (caret >= length)
        {
            return $"{label}: konec textu, za znakem {DescribeCharacter(value[^1])}, pozice {length} z {length}.";
        }

        return $"{label}: za znakem {DescribeCharacter(value[caret - 1])}, před znakem {DescribeCharacter(value[caret])}, pozice {caret} z {length}.";
    }

    private static TextBlock CreateTextEditingLiveRegion()
    {
        var liveRegion = new TextBlock
        {
            Width = 1,
            Height = 1,
            Opacity = 0.01,
            IsHitTestVisible = false,
            Focusable = false,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            TextWrapping = Avalonia.Media.TextWrapping.NoWrap
        };

        AutomationProperties.SetName(liveRegion, string.Empty);
        AutomationProperties.SetAutomationId(liveRegion, "TextEditingLiveRegion");
        AutomationProperties.SetLiveSetting(liveRegion, AutomationLiveSetting.Assertive);
        AutomationProperties.SetAccessibilityView(liveRegion, AccessibilityView.Control);
        return liveRegion;
    }

    private static void InstallTextEditingLiveRegion(Window window, TextBlock liveRegion)
    {
        if (liveRegion.Parent is not null)
        {
            return;
        }

        if (window.Content is Panel panel)
        {
            panel.Children.Add(liveRegion);
        }
    }

    private static void AnnounceTextEditingState(TextBlock liveRegion, string message)
    {
        liveRegion.Text = string.Empty;
        AutomationProperties.SetName(liveRegion, string.Empty);
        DispatcherTimer.RunOnce(
            () =>
            {
                liveRegion.Text = message;
                AutomationProperties.SetName(liveRegion, message);
            },
            TimeSpan.FromMilliseconds(30));
    }

    private static string DescribeSnippet(string value)
    {
        const int maxLength = 32;
        var normalized = value.Replace("\r", string.Empty, StringComparison.Ordinal).Replace("\n", " konec řádku ", StringComparison.Ordinal).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "prázdný výběr";
        }

        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength] + "...";
    }

    private static string DescribeCharacter(char character) =>
        character switch
        {
            '\r' or '\n' => "konec řádku",
            '\t' => "tabulátor",
            ' ' => "mezera",
            _ => character.ToString()
        };

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
