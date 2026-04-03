using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class TrayActionsWindow : Window
{
    public TrayActionsDialogAction Result { get; private set; }

    public TrayActionsWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += (_, _) => Dispatcher.UIThread.Post(
            () => this.FindControl<Button>("ShowMainWindowTrayActionButton")?.Focus(),
            DispatcherPriority.Input);
    }

    private void OnShowMainWindowClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ShowMainWindow;
        Close();
    }

    private void OnShowDashboardClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ShowDashboard;
        Close();
    }

    private void OnExitClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ExitApplication;
        Close();
    }

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.None;
        Close();
    }
}
