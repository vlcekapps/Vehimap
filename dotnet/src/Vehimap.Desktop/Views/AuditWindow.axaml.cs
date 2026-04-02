using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.Views.Workspaces;

namespace Vehimap.Desktop.Views;

public partial class AuditWindow : Window
{
    public AuditWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += OnOpened;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        if (this.FindControl<AuditWorkspaceView>("AuditWorkspaceHost") is { } workspaceView)
        {
            Dispatcher.UIThread.Post(workspaceView.FocusDefaultControl, DispatcherPriority.Loaded);
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
