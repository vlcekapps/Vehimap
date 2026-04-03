using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;
using Vehimap.Platform;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop;

public partial class App : Avalonia.Application
{
    private DesktopAppRuntimeController? _runtimeController;

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

            desktop.MainWindow = mainWindow;
            var dialogService = new AvaloniaAppShellDialogService();
            _runtimeController = new DesktopAppRuntimeController(
                desktop,
                mainWindow,
                mainWindowViewModel,
                new AvaloniaTrayService(buildInfoProvider),
                new DesktopNotificationService(buildInfoProvider),
                dialogService);
            mainWindow.ExitApplicationRequested = _runtimeController.RequestExitAsync;
            mainWindow.MinimizeToTrayRequested = _runtimeController.RequestMinimizeToTrayAsync;
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

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit -= OnDesktopExit;
        }
    }
}
