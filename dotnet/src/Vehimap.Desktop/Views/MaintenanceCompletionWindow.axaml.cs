using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class MaintenanceCompletionWindow : Window
{
    public MaintenanceCompletionWindow()
    {
        AvaloniaXamlLoader.Load(this);
        KeyboardAccessibilityHelper.RegisterWindow(this);
        AddHandler(InputElement.KeyDownEvent, OnMaintenanceCompletionKeyDown, RoutingStrategies.Tunnel);
        Opened += (_, _) => Dispatcher.UIThread.Post(() => this.FindControl<TextBox>("MaintenanceCompletionDateBox")?.Focus());
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        SaveAndCloseIfValid();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(null);

    private void OnMaintenanceCompletionKeyDown(object? sender, KeyEventArgs e)
    {
        if (KeyboardAccessibilityHelper.ShouldSkipGlobalShortcut(e))
        {
            return;
        }

        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            Close(null);
            return;
        }

        if (e.Key == Key.S && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            SaveAndCloseIfValid();
        }
    }

    private void SaveAndCloseIfValid()
    {
        if (DataContext is not MaintenanceCompletionDialogViewModel viewModel)
        {
            Close(null);
            return;
        }

        if (viewModel.TryBuildResult(out var result) && result is not null)
        {
            Close(result);
            return;
        }

        FocusErrorTarget(viewModel.ErrorFocusTarget);
    }

    private void FocusErrorTarget(string? targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
        {
            return;
        }

        Dispatcher.UIThread.Post(() => this.FindControl<Control>(targetName)?.Focus(), DispatcherPriority.Background);
    }
}
