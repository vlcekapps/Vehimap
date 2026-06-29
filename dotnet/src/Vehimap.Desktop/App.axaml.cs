using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Platform;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop;

public partial class App : Avalonia.Application
{
    private static DesktopSingleInstanceCoordinator? SingleInstanceCoordinator;
    private DesktopAppRuntimeController? _runtimeController;

    internal static void SetSingleInstanceCoordinator(DesktopSingleInstanceCoordinator? coordinator)
    {
        SingleInstanceCoordinator = coordinator;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var buildInfoProvider = new AssemblyAppBuildInfoProvider();
            var mainWindowViewModel = new MainWindowViewModel();
            var mainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
            var notificationService = new DesktopNotificationService(buildInfoProvider);
            notificationService.BindHostWindow(mainWindow);

            desktop.MainWindow = mainWindow;
            var dialogService = new AvaloniaAppShellDialogService();
            _runtimeController = new DesktopAppRuntimeController(
                desktop,
                mainWindow,
                mainWindowViewModel,
                new AvaloniaTrayService(buildInfoProvider),
                notificationService,
                new SystemEventsResumeService(),
                dialogService);
            mainWindow.ExitApplicationRequested = _runtimeController.RequestExitAsync;
            mainWindow.MinimizeToTrayRequested = _runtimeController.RequestMinimizeToTrayAsync;
            mainWindow.OpenTrayActionsRequested = _runtimeController.RequestOpenTrayActionsAsync;
            SingleInstanceCoordinator?.SetActivationHandler(_runtimeController.RequestShowMainWindowAsync);
            desktop.Exit += OnDesktopExit;
            _ = _runtimeController.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (_runtimeController is not null)
        {
            await _runtimeController.DisposeAsync().ConfigureAwait(false);
            _runtimeController = null;
        }

        SingleInstanceCoordinator?.ClearActivationHandler();
        SingleInstanceCoordinator?.Dispose();
        SingleInstanceCoordinator = null;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit -= OnDesktopExit;
        }
    }
}
