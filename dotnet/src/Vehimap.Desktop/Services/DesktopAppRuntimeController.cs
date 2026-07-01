// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.Views;

namespace Vehimap.Desktop.Services;

internal sealed class DesktopAppRuntimeController : IAsyncDisposable
{
    private static readonly TimeSpan DueCheckInterval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan BackupCheckInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan InitialBackgroundDelay = TimeSpan.FromMilliseconds(1500);
    private static readonly TimeSpan ResumeBackgroundDelay = TimeSpan.FromMilliseconds(1500);

    private readonly IClassicDesktopStyleApplicationLifetime _desktopLifetime;
    private readonly MainWindow _mainWindow;
    private readonly MainWindowViewModel _shell;
    private readonly ITrayService _trayService;
    private readonly INotificationService _notificationService;
    private readonly ISystemResumeService _systemResumeService;
    private readonly IAppShellDialogService _dialogService;
    private readonly DispatcherTimer _dueTimer;
    private readonly DispatcherTimer _backupTimer;
    private bool _allowClose;
    private bool _initialized;
    private bool _closeConfirmationInProgress;
    private bool _resumeRefreshScheduled;
    private string _lastNotificationKey = string.Empty;

    public DesktopAppRuntimeController(
        IClassicDesktopStyleApplicationLifetime desktopLifetime,
        MainWindow mainWindow,
        MainWindowViewModel shell,
        ITrayService trayService,
        INotificationService notificationService,
        ISystemResumeService systemResumeService,
        IAppShellDialogService dialogService)
    {
        _desktopLifetime = desktopLifetime;
        _mainWindow = mainWindow;
        _shell = shell;
        _trayService = trayService;
        _notificationService = notificationService;
        _systemResumeService = systemResumeService;
        _dialogService = dialogService;
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
        _shell.BackgroundRefreshRequested += OnShellBackgroundRefreshRequested;
        _systemResumeService.Resumed += OnSystemResumed;
        await _systemResumeService.InitializeAsync(cancellationToken).ConfigureAwait(false);

        await _trayService.InitializeAsync(
                new TrayServiceConfiguration(
                    _shell.BuildBackgroundSnapshot().ToolTipText,
                    OpenTrayActionsAsync,
                    ShowMainWindowAsync,
                    ShowDashboardAsync,
                    ExitApplicationAsync),
                cancellationToken)
            .ConfigureAwait(false);
        _shell.IsMinimizeToTrayAvailable = _trayService.IsSupported;

        _dueTimer.Start();
        _backupTimer.Start();

        DispatcherTimer.RunOnce(async () =>
        {
            if (DesktopBackgroundRuntimePolicy.CanHideOnLaunch(_trayService.IsSupported, _shell.ShouldHideOnLaunch()))
            {
                HideMainWindow();
                await RefreshBackgroundStateAsync(notifyWhenHidden: true, runAutomaticBackup: true, notifyDue: true).ConfigureAwait(false);
            }
            else
            {
                await RefreshBackgroundStateAsync(notifyWhenHidden: false, runAutomaticBackup: true, notifyDue: true).ConfigureAwait(false);
            }
        }, InitialBackgroundDelay);
    }

    public Task RequestExitAsync()
    {
        return ExitApplicationAsync();
    }

    public Task RequestMinimizeToTrayAsync()
    {
        if (!_trayService.IsSupported)
        {
            return Task.CompletedTask;
        }

        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            HideMainWindow();
        }).GetTask();
    }

    public Task RequestOpenTrayActionsAsync() => OpenTrayActionsAsync();

    public Task RequestShowMainWindowAsync() => ShowMainWindowAsync();

    public async ValueTask DisposeAsync()
    {
        _dueTimer.Stop();
        _backupTimer.Stop();
        _mainWindow.Closing -= OnMainWindowClosing;
        _shell.BackgroundRefreshRequested -= OnShellBackgroundRefreshRequested;
        _systemResumeService.Resumed -= OnSystemResumed;
        await _systemResumeService.DisposeAsync().ConfigureAwait(false);
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

    private void OnShellBackgroundRefreshRequested()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            await RefreshBackgroundStateAsync(
                notifyWhenHidden: !_mainWindow.IsVisible,
                runAutomaticBackup: false,
                reloadData: false).ConfigureAwait(false);
        }, DispatcherPriority.Background);
    }

    private void OnSystemResumed(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_resumeRefreshScheduled)
            {
                return;
            }

            _resumeRefreshScheduled = true;
            DispatcherTimer.RunOnce(async () =>
            {
                _resumeRefreshScheduled = false;
                await RefreshBackgroundStateAsync(notifyWhenHidden: !_mainWindow.IsVisible, runAutomaticBackup: true).ConfigureAwait(false);
            }, ResumeBackgroundDelay);
        }, DispatcherPriority.Background);
    }

    private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose)
        {
            return;
        }

        if (_trayService.IsSupported)
        {
            e.Cancel = true;
            HideMainWindow();
            return;
        }

        if (!_shell.HasPendingEdits || _closeConfirmationInProgress)
        {
            return;
        }

        e.Cancel = true;
        _ = ConfirmAndCloseMainWindowAsync();
    }

    private async Task RefreshBackgroundStateAsync(bool notifyWhenHidden, bool runAutomaticBackup, bool reloadData = true, bool notifyDue = true)
    {
        var hasPendingEdits = _shell.HasPendingEdits;
        if (reloadData && DesktopBackgroundRuntimePolicy.CanReloadInBackground(hasPendingEdits))
        {
            _shell.ReloadForBackgroundMonitoring();
        }

        var background = _shell.BuildBackgroundSnapshot();
        await _trayService.UpdateToolTipAsync(background.ToolTipText).ConfigureAwait(false);

        if (DesktopBackgroundRuntimePolicy.CanRunAutomaticBackup(runAutomaticBackup, hasPendingEdits))
        {
            var backupResult = await _shell.RunAutomaticBackupCheckAsync().ConfigureAwait(false);
            if (DesktopBackgroundRuntimePolicy.CanShowAutomaticBackupNotification(notifyWhenHidden, backupResult.Created, backupResult.IsError))
            {
                await _notificationService.ShowAsync(DesktopLocalization.Localizer.GetString("Notification.AutoBackupTitle"), backupResult.Message).ConfigureAwait(false);
            }
        }

        if (notifyDue
            && !hasPendingEdits
            && DesktopBackgroundRuntimePolicy.CanShowDueNotification(background.HasNotification, background.NotificationKey, _lastNotificationKey)
            && await _shell.ShouldShowAndRememberDueNotificationAsync(background.NotificationKey).ConfigureAwait(false))
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
            _shell.RequestWorkspaceFocus(DesktopFocusTarget.VehicleList);
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

    private Task ShowUpcomingOverviewAsync()
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
            _shell.SelectedVehicleTabIndex = DesktopTabIndexes.UpcomingOverview;
            _shell.RequestWorkspaceFocus(DesktopFocusTarget.UpcomingOverviewSearch);
        }).GetTask();
    }

    private Task ShowOverdueOverviewAsync()
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
            _shell.SelectedVehicleTabIndex = DesktopTabIndexes.OverdueOverview;
            _shell.RequestWorkspaceFocus(DesktopFocusTarget.OverdueOverviewSearch);
        }).GetTask();
    }

    private Task ExecuteShellQuickActionAsync(Func<Task> executeAsync)
    {
        var completion = new TaskCompletionSource();
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
                await executeAsync().ConfigureAwait(true);
                completion.SetResult();
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        });

        return completion.Task;
    }

    private async Task OpenTrayActionsAsync()
    {
        var action = await _dialogService
            .ShowTrayActionsAsync(_mainWindow.IsVisible ? _mainWindow : null, _shell.BuildTrayActionsDialogModel())
            .ConfigureAwait(true);

        switch (action)
        {
            case TrayActionsDialogAction.ShowMainWindow:
                await ShowMainWindowAsync().ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ShowDashboard:
                await ShowDashboardAsync().ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.OpenBackgroundStatus:
                await ExecuteShellQuickActionAsync(() => _shell.OpenBackgroundNotificationAsync()).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ShowUpcomingOverview:
                await ShowUpcomingOverviewAsync().ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ShowOverdueOverview:
                await ShowOverdueOverviewAsync().ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.OpenNearestTechnical:
                await ExecuteShellQuickActionAsync(() => _shell.OpenNearestTechnicalCommand.ExecuteAsync(null)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.OpenNearestGreenCard:
                await ExecuteShellQuickActionAsync(() => _shell.OpenNearestGreenCardCommand.ExecuteAsync(null)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.OpenNearestReminder:
                await ExecuteShellQuickActionAsync(() => _shell.OpenNearestReminderCommand.ExecuteAsync(null)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.OpenNearestMaintenance:
                await ExecuteShellQuickActionAsync(() => _shell.OpenNearestMaintenanceCommand.ExecuteAsync(null)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.OpenNearestRecord:
                await ExecuteShellQuickActionAsync(() => _shell.OpenNearestRecordCommand.ExecuteAsync(null)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ReviewTechnical:
                await ExecuteShellQuickActionAsync(() => _shell.ReviewTechnicalCommand.ExecuteAsync(null)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ReviewGreenCards:
                await ExecuteShellQuickActionAsync(() => _shell.ReviewGreenCardsCommand.ExecuteAsync(null)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ReviewReminders:
                await ExecuteShellQuickActionAsync(() => _shell.ReviewRemindersCommand.ExecuteAsync(null)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ReviewMaintenance:
                await ExecuteShellQuickActionAsync(() => _shell.ReviewMaintenanceCommand.ExecuteAsync(null)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ReviewRecords:
                await ExecuteShellQuickActionAsync(() => _shell.ReviewRecordsCommand.ExecuteAsync(null)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.OpenPrintableReport:
                await ExecuteShellQuickActionAsync(() => _shell.AppShellController.OpenPrintableReportAsync(_shell)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ExportBackup:
                await ExecuteShellQuickActionAsync(() => _shell.AppShellController.ExportBackupAsync(_mainWindow, _shell)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ImportBackup:
                await ExecuteShellQuickActionAsync(() => _shell.AppShellController.ImportBackupAsync(_mainWindow, _shell)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.CreateAutomaticBackupNow:
                await ExecuteShellQuickActionAsync(() => _shell.CreateAutomaticBackupNowAsync()).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.OpenAutomaticBackupFolder:
                await ExecuteShellQuickActionAsync(() => _shell.OpenAutomaticBackupFolderAsync()).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.OpenSettings:
                await ExecuteShellQuickActionAsync(() => _shell.AppShellController.OpenSettingsAsync(_mainWindow, _shell)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ExportCalendar:
                await ExecuteShellQuickActionAsync(() => _shell.ExportCalendarCommand.ExecuteAsync(null)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ReloadData:
                await ExecuteShellQuickActionAsync(() =>
                {
                    _shell.ReloadCommand.Execute(null);
                    return Task.CompletedTask;
                }).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.OpenDataFolder:
                await ExecuteShellQuickActionAsync(() => _shell.OpenDataFolderAsync()).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.OpenAbout:
                await ExecuteShellQuickActionAsync(() => _shell.AppShellController.OpenAboutAsync(_mainWindow, _shell)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ThankAuthor:
                await ExecuteShellQuickActionAsync(() => _shell.AppShellController.OpenAuthorSupportAsync(_shell)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.ReportFeedback:
                await ExecuteShellQuickActionAsync(() => _shell.AppShellController.OpenFeedbackIssueAsync(_shell)).ConfigureAwait(true);
                break;
            case TrayActionsDialogAction.CheckForUpdates:
                var shouldExit = false;
                await ExecuteShellQuickActionAsync(async () =>
                {
                    shouldExit = await _shell.AppShellController.CheckForUpdatesAsync(_mainWindow, _shell).ConfigureAwait(true);
                }).ConfigureAwait(true);
                if (shouldExit)
                {
                    await ExitApplicationAsync().ConfigureAwait(true);
                }

                break;
            case TrayActionsDialogAction.ExitApplication:
                await ExitApplicationAsync().ConfigureAwait(true);
                break;
        }
    }

    private Task ExitApplicationAsync()
    {
        var completion = new TaskCompletionSource();
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                await ExitApplicationCoreAsync().ConfigureAwait(true);
                completion.SetResult();
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        });

        return completion.Task;
    }

    private void HideMainWindow()
    {
        _mainWindow.Hide();
    }

    private async Task ConfirmAndCloseMainWindowAsync()
    {
        _closeConfirmationInProgress = true;
        try
        {
            var confirmed = await _dialogService
                .ConfirmDiscardPendingChangesAsync(_mainWindow, _shell.GetPendingEditLabel(), DesktopLocalization.Localizer.GetString("PendingEdits.Action.ExitApplication"))
                .ConfigureAwait(true);
            if (!confirmed)
            {
                _shell.RequestWorkspaceFocus(_shell.GetPendingEditFocusTarget());
                return;
            }

            _shell.DiscardPendingEdits();
            _allowClose = true;
            _mainWindow.Close();
            _desktopLifetime.Shutdown();
        }
        finally
        {
            _closeConfirmationInProgress = false;
        }
    }

    private async Task ExitApplicationCoreAsync()
    {
        if (_shell.HasPendingEdits)
        {
            if (!_mainWindow.IsVisible)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }

            var confirmed = await _dialogService
                .ConfirmDiscardPendingChangesAsync(_mainWindow, _shell.GetPendingEditLabel(), DesktopLocalization.Localizer.GetString("PendingEdits.Action.ExitApplication"))
                .ConfigureAwait(true);
            if (!confirmed)
            {
                _shell.RequestWorkspaceFocus(_shell.GetPendingEditFocusTarget());
                return;
            }

            _shell.DiscardPendingEdits();
        }

        _allowClose = true;
        _mainWindow.Close();
        _desktopLifetime.Shutdown();
    }
}
