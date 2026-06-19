using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
        AddHandler(InputElement.KeyDownEvent, OnTrayActionsKeyDown, RoutingStrategies.Tunnel);
        Opened += (_, _) => Dispatcher.UIThread.Post(
            () => this.FindControl<Button>("ShowMainWindowTrayActionButton")?.Focus(),
            DispatcherPriority.Input);
    }

    private void OnShowMainWindowClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ShowMainWindow;
        Close();
    }

    private void OnShowDashboardClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ShowDashboard;
        Close();
    }

    private void OnShowUpcomingOverviewClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ShowUpcomingOverview;
        Close();
    }

    private void OnShowOverdueOverviewClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ShowOverdueOverview;
        Close();
    }

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.ExitApplication;
        Close();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Result = TrayActionsDialogAction.None;
        Close();
    }

    private void OnTrayActionsKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        e.Handled = true;
        Result = TrayActionsDialogAction.None;
        Close();
    }
}
