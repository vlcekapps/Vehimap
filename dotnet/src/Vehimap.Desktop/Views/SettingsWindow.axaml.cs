using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        AvaloniaXamlLoader.Load(this);
        KeyboardAccessibilityHelper.RegisterWindow(this);
        AddHandler(InputElement.KeyDownEvent, OnSettingsKeyDown, RoutingStrategies.Tunnel);
        Opened += (_, _) => Dispatcher.UIThread.Post(() => this.FindControl<ComboBox>("LanguageOptionBox")?.Focus());
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e) => SaveAndClose(createBackupNow: false);

    private void OnBackupNowClick(object? sender, RoutedEventArgs e) => SaveAndClose(createBackupNow: true);

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(null);

    private void OnSettingsKeyDown(object? sender, KeyEventArgs e)
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

        if (e.KeyModifiers != KeyModifiers.Control)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.S:
                e.Handled = true;
                SaveAndClose(createBackupNow: false);
                break;
            case Key.B:
                e.Handled = true;
                SaveAndClose(createBackupNow: true);
                break;
        }
    }

    private void SaveAndClose(bool createBackupNow)
    {
        if (DataContext is not SettingsDialogViewModel viewModel)
        {
            Close(null);
            return;
        }

        if (!viewModel.TryBuildSnapshot(out var snapshot, out var errorMessage))
        {
            viewModel.StatusMessage = errorMessage;
            return;
        }

        Close(new SettingsDialogResult(snapshot, createBackupNow));
    }
}
