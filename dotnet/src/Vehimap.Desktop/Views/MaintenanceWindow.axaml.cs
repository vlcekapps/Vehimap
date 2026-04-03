using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.Views.Workspaces;

namespace Vehimap.Desktop.Views;

public partial class MaintenanceWindow : Window
{
    private bool _closeConfirmed;

    public MaintenanceWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += OnOpened;
        Closing += OnClosing;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        if (this.FindControl<MaintenanceWorkspaceView>("MaintenanceWorkspaceHost") is { } workspaceView)
        {
            Dispatcher.UIThread.Post(workspaceView.FocusDefaultControl, DispatcherPriority.Loaded);
        }
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_closeConfirmed || !ModalWorkspaceWindowHelpers.HasPendingEdits(DataContext))
        {
            return;
        }

        e.Cancel = true;
        if (!await ModalWorkspaceWindowHelpers.ConfirmCloseAsync(DataContext, "zavřít editor údržby").ConfigureAwait(true))
        {
            return;
        }

        _closeConfirmed = true;
        Close();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
