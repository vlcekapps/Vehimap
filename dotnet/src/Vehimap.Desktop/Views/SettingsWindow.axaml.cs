using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += (_, _) => Dispatcher.UIThread.Post(() => this.FindControl<TextBox>("TechnicalReminderDaysBox")?.Focus());
    }

    private void OnSaveClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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

        Close(new SettingsDialogResult(snapshot, false));
    }

    private void OnBackupNowClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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

        Close(new SettingsDialogResult(snapshot, true));
    }

    private void OnCancelClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(null);
}
