using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopAppRuntimeController : IAsyncDisposable
{
    private static readonly TimeSpan DueCheckInterval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan BackupCheckInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan InitialBackgroundDelay = TimeSpan.FromMilliseconds(1500);

    private readonly IClassicDesktopStyleApplicationLifetime _desktopLifetime;
    private readonly MainWindow _mainWindow;
    private readonly MainWindowViewModel _shell;
    private readonly ITrayService _trayService;
    private readonly INotificationService _notificationService;
    private readonly DispatcherTimer _dueTimer;
    private readonly DispatcherTimer _backupTimer;
    private bool _allowClose;
    private bool _initialized;
    private string _lastNotificationKey = string.Empty;

    public DesktopAppRuntimeController(
        IClassicDesktopStyleApplicationLifetime desktopLifetime,
        MainWindow mainWindow,
        MainWindowViewModel shell,
        ITrayService trayService,
        INotificationService notificationService)
    {
        _desktopLifetime = desktopLifetime;
        _mainWindow = mainWindow;
        _shell = shell;
        _trayService = trayService;
        _notificationService = notificationService;
        _dueTimer = new DispatcherTimer { Interval = DueCheckInterval };
        _backupTimer = new DispatcherTimer { Interval = BackupCheckInterval };
        _dueTimer.Tick += OnDueTimerTick;
        _backupTimer.Tick += OnBackupTimerTick;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        _mainWindow.Closing += OnMainWindowClosing;

        await _trayService.InitializeAsync(
                new TrayServiceConfiguration(
                    _shell.BuildBackgroundSnapshot().ToolTipText,
                    ShowMainWindowAsync,
                    ShowDashboardAsync,
                    ExitApplicationAsync),
                cancellationToken)
            .ConfigureAwait(false);

        _dueTimer.Start();
        _backupTimer.Start();

        DispatcherTimer.RunOnce(async () =>
        {
            if (_shell.ShouldHideOnLaunch())
            {
                HideMainWindow();
                await RefreshBackgroundStateAsync(notifyWhenHidden: true, runAutomaticBackup: true).ConfigureAwait(false);
            }
            else
            {
                await RefreshBackgroundStateAsync(notifyWhenHidden: false, runAutomaticBackup: true).ConfigureAwait(false);
            }
        }, InitialBackgroundDelay);
    }

    public async ValueTask DisposeAsync()
    {
        _dueTimer.Stop();
        _backupTimer.Stop();
        _mainWindow.Closing -= OnMainWindowClosing;
        await _trayService.DisposeAsync().ConfigureAwait(false);
    }

    private async void OnDueTimerTick(object? sender, EventArgs e)
    {
        await RefreshBackgroundStateAsync(notifyWhenHidden: !_mainWindow.IsVisible, runAutomaticBackup: false).ConfigureAwait(false);
    }

    private async void OnBackupTimerTick(object? sender, EventArgs e)
    {
        await RefreshBackgroundStateAsync(notifyWhenHidden: !_mainWindow.IsVisible, runAutomaticBackup: true).ConfigureAwait(false);
    }

    private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose || !_trayService.IsSupported)
        {
            return;
        }

        e.Cancel = true;
        HideMainWindow();
    }

    private async Task RefreshBackgroundStateAsync(bool notifyWhenHidden, bool runAutomaticBackup)
    {
        _shell.ReloadForBackgroundMonitoring();
        var background = _shell.BuildBackgroundSnapshot();
        await _trayService.UpdateToolTipAsync(background.ToolTipText).ConfigureAwait(false);

        if (runAutomaticBackup)
        {
            var backupResult = await _shell.RunAutomaticBackupCheckAsync().ConfigureAwait(false);
            if (notifyWhenHidden && (backupResult.Created || backupResult.IsError))
            {
                await _notificationService.ShowAsync("Vehimap - automatická záloha", backupResult.Message).ConfigureAwait(false);
            }
        }

        if (notifyWhenHidden && background.HasNotification && !string.Equals(_lastNotificationKey, background.NotificationKey, StringComparison.Ordinal))
        {
            _lastNotificationKey = background.NotificationKey;
            await _notificationService.ShowAsync(background.NotificationTitle, background.NotificationMessage).ConfigureAwait(false);
        }
    }

    private Task ShowMainWindowAsync()
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }).GetTask();
    }

    private Task ShowDashboardAsync()
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
            _shell.ShowDashboardFromTray();
        }).GetTask();
    }

    private Task ExitApplicationAsync()
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            _allowClose = true;
            _mainWindow.Close();
            _desktopLifetime.Shutdown();
        }).GetTask();
    }

    private void HideMainWindow()
    {
        _mainWindow.Hide();
    }
}
