// SPDX-License-Identifier: GPL-3.0-or-later
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows.Input;
using Vehimap.Application.Models;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class MainWindow : Window
{
    private const int DetailTabIndex = DesktopTabIndexes.Detail;
    private const int HistoryTabIndex = DesktopTabIndexes.History;
    private const int FuelTabIndex = DesktopTabIndexes.Fuel;
    private const int ReminderTabIndex = DesktopTabIndexes.Reminder;
    private const int MaintenanceTabIndex = DesktopTabIndexes.Maintenance;
    private const int TimelineTabIndex = DesktopTabIndexes.Timeline;
    private const int RecordTabIndex = DesktopTabIndexes.Record;
    private const int AuditTabIndex = DesktopTabIndexes.Audit;
    private const int CostTabIndex = DesktopTabIndexes.Cost;
    private const int DashboardTabIndex = DesktopTabIndexes.Dashboard;
    private const int SearchTabIndex = DesktopTabIndexes.Search;
    private const int UpcomingOverviewTabIndex = DesktopTabIndexes.UpcomingOverview;
    private const int OverdueOverviewTabIndex = DesktopTabIndexes.OverdueOverview;
    private const int SmartAdvisorTabIndex = DesktopTabIndexes.SmartAdvisor;

    private static readonly string[] TabHeaderButtonNames =
    [
        "DetailTabButton",
        "HistoryTabButton",
        "FuelTabButton",
        "ReminderTabButton",
        "MaintenanceTabButton",
        "TimelineTabButton",
        "RecordTabButton",
        "AuditTabButton",
        "CostTabButton",
        "DashboardTabButton",
        "SearchTabButton",
        "UpcomingOverviewTabButton",
        "OverdueOverviewTabButton",
        "SmartAdvisorTabButton"
    ];

    private static readonly string[] MainMenuRootNames =
    [
        "FileMenuRoot",
        "VehicleMenuRoot",
        "OverviewMenuRoot",
        "QuickActionsMenuRoot",
        "AppMenuRoot"
    ];

    private const int MaxInitialFocusAttempts = 8;

    private static string L(string key) => DesktopLocalization.Localizer.GetString(key);

    private static string LF(string key, params object?[] args) => DesktopLocalization.Localizer.Format(key, args);

    private MainWindowViewModel? _viewModel;
    private bool _initialFocusCompleted;
    private bool _initialFocusScheduled;
    private int _initialFocusAttempts;
    private bool _syncingVehicleSelection;
    private bool _vehicleEditorDialogOpen;
    private bool _workspaceEditorDialogOpen;
    private Control? _lastNonMenuFocusTarget;

    public Func<Task>? ExitApplicationRequested { get; set; }
    public Func<Task>? MinimizeToTrayRequested { get; set; }
    public Func<Task>? OpenTrayActionsRequested { get; set; }
    public ICommand OpenTrayActionsCommand { get; }

    public MainWindow()
    {
        OpenTrayActionsCommand = new AsyncRelayCommand(OpenTrayActionsAsync);
        AvaloniaXamlLoader.Load(this);
        KeyBindings.Add(new KeyBinding
        {
            Gesture = KeyGesture.Parse("Ctrl+Shift+Y"),
            Command = OpenTrayActionsCommand
        });
        KeyboardAccessibilityHelper.RegisterWindow(this);
        RegisterTabBoundaryNavigation();
        AddHandler(InputElement.KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
        AddHandler(InputElement.GotFocusEvent, OnElementGotFocus, RoutingStrategies.Tunnel);
        Opened += OnOpened;
        Activated += OnActivated;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        ScheduleInitialFocus();
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        ScheduleInitialFocus();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.FocusRequested -= OnFocusRequested;
            _viewModel.VehicleEditorDialogRequested -= OnVehicleEditorDialogRequested;
            _viewModel.WorkspaceEditorDialogRequested -= OnWorkspaceEditorDialogRequested;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.ConfirmPendingEditsHandler = null;
            _viewModel.ConfirmVehicleDeleteHandler = null;
        }

        _viewModel = DataContext as MainWindowViewModel;
        if (_viewModel is not null)
        {
            var viewModel = _viewModel;
            viewModel.FocusRequested += OnFocusRequested;
            viewModel.VehicleEditorDialogRequested += OnVehicleEditorDialogRequested;
            viewModel.WorkspaceEditorDialogRequested += OnWorkspaceEditorDialogRequested;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.ConfirmPendingEditsHandler = actionDescription =>
                viewModel.AppShellController.ConfirmDiscardPendingChangesAsync(this, viewModel, actionDescription);
            viewModel.ConfirmVehicleDeleteHandler = ConfirmDeleteVehicleAsync;
            SyncVehicleSelectionFromViewModel();
            ScheduleInitialFocus();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.Equals(e.PropertyName, nameof(MainWindowViewModel.SelectedVehicle), StringComparison.Ordinal))
        {
            SyncVehicleSelectionFromViewModel();
        }
    }

    private async void OnVehicleEditorDialogRequested(object? sender, VehicleEditorDialogRequest request)
    {
        await ShowVehicleEditorDialogAsync(request.ReturnFocusTarget).ConfigureAwait(true);
    }

    private async void OnWorkspaceEditorDialogRequested(object? sender, WorkspaceEditorDialogRequest request)
    {
        await ShowWorkspaceEditorDialogAsync(request).ConfigureAwait(true);
    }

    private void OnFocusRequested(DesktopFocusTarget target)
    {
        RequestFocus(target);
    }

    private void ScheduleInitialFocus()
    {
        if (_initialFocusCompleted || _initialFocusScheduled)
        {
            return;
        }

        _initialFocusScheduled = true;
        Dispatcher.UIThread.Post(() =>
        {
            Focus();
            DispatcherTimer.RunOnce(() =>
            {
                _initialFocusScheduled = false;
                _initialFocusAttempts++;
                _initialFocusCompleted = TryFocusInitialVehicleList();
                if (!_initialFocusCompleted && _initialFocusAttempts < MaxInitialFocusAttempts)
                {
                    ScheduleInitialFocus();
                }
            }, TimeSpan.FromMilliseconds(160));
        }, DispatcherPriority.Loaded);
    }

    private void RequestFocus(DesktopFocusTarget target)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!TryFocusTarget(target))
            {
                Dispatcher.UIThread.Post(() => TryFocusTarget(target), DispatcherPriority.Input);
            }
        }, DispatcherPriority.Background);
    }

    private bool TryFocusTarget(DesktopFocusTarget target)
    {
        if (target == DesktopFocusTarget.SelectedVehicleTabHeader)
        {
            return FocusSelectedTabHeader();
        }

        var control = ResolveFocusTarget(target);
        if (control is null)
        {
            return false;
        }

        if (control is ListBox listBox)
        {
            return FocusListBox(listBox);
        }

        return control.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
    }

    private void RegisterTabBoundaryNavigation()
    {
        RegisterForwardTabToVehicleList("HideInactiveVehiclesCheckBox");
        RegisterForwardTabToHeaders("VehicleListBox");
        RegisterTabHeaderNavigation();
    }

    private void RegisterTabHeaderNavigation()
    {
        foreach (var controlName in TabHeaderButtonNames)
        {
            if (this.FindControl<Control>(controlName) is { } button)
            {
                button.AddHandler(InputElement.KeyDownEvent, OnTabHeaderKeyDown, RoutingStrategies.Tunnel);
            }
        }
    }

    private void RegisterForwardTabToHeaders(string controlName)
    {
        if (this.FindControl<Control>(controlName) is { } control)
        {
            control.AddHandler(InputElement.KeyDownEvent, OnForwardTabToHeadersKeyDown, RoutingStrategies.Tunnel);
        }
    }

    private void RegisterForwardTabToVehicleList(string controlName)
    {
        if (this.FindControl<Control>(controlName) is { } control)
        {
            control.AddHandler(InputElement.KeyDownEvent, OnForwardTabToVehicleListKeyDown, RoutingStrategies.Tunnel);
        }
    }

    private void OnForwardTabToVehicleListKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab || e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            return;
        }

        e.Handled = TryFocusTarget(DesktopFocusTarget.VehicleList);
    }

    private void OnForwardTabToHeadersKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab || e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            return;
        }

        e.Handled = FocusSelectedTabHeader();
    }

    private async void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (KeyboardAccessibilityHelper.ShouldSkipGlobalShortcut(e))
        {
            return;
        }

        if (IsMainMenuInvocationKey(e))
        {
            e.Handled = ToggleMainMenuFocus(e.Source);
            return;
        }

        if (e.Key == Key.F2 && e.KeyModifiers == KeyModifiers.None)
        {
            if (_viewModel?.IsCurrentWorkspaceEditShortcutContext == true)
            {
                e.Handled = true;
                await OpenCurrentWorkspaceEditWindowAsync().ConfigureAwait(true);
                return;
            }

            OpenVehicleEditorFromCommand(_viewModel?.EditSelectedVehicleCommand, DesktopFocusTarget.VehicleDetailPrimaryAction);
            e.Handled = true;
            return;
        }

        if (e.KeyModifiers != KeyModifiers.Control)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.N:
                if (_viewModel?.IsCurrentWorkspaceCreateShortcutContext == true)
                {
                    e.Handled = true;
                    await OpenCurrentWorkspaceCreateWindowAsync().ConfigureAwait(true);
                    break;
                }

                OpenVehicleEditorFromCommand(_viewModel?.CreateVehicleCommand, DesktopFocusTarget.VehicleList);
                e.Handled = true;
                break;
            case Key.U:
                if (_viewModel?.IsCurrentWorkspaceEditShortcutContext == true)
                {
                    e.Handled = true;
                    await OpenCurrentWorkspaceEditWindowAsync().ConfigureAwait(true);
                    break;
                }

                OpenVehicleEditorFromCommand(_viewModel?.EditSelectedVehicleCommand, GetShellVehicleEditReturnFocusTarget());
                e.Handled = true;
                break;
            case Key.S:
                if (_viewModel?.IsCurrentWorkspaceSaveShortcutContext == true)
                {
                    e.Handled = true;
                    await _viewModel.HandleCurrentWorkspaceSaveShortcutAsync().ConfigureAwait(true);
                }

                break;
            case Key.O:
                if (_viewModel?.IsCurrentWorkspacePrimaryOpenShortcutContext == true)
                {
                    e.Handled = true;
                    await _viewModel.HandleCurrentWorkspacePrimaryOpenShortcutAsync().ConfigureAwait(true);
                    break;
                }

                _ = OpenVehicleDetailWindowAsync();
                e.Handled = true;
                break;
            case Key.H:
                _ = OpenHistoryWindowAsync();
                e.Handled = true;
                break;
            case Key.K:
                _ = OpenFuelWindowAsync();
                e.Handled = true;
                break;
            case Key.P:
                if (_viewModel?.IsCurrentWorkspaceItemOpenShortcutContext == true)
                {
                    e.Handled = true;
                    await _viewModel.HandleCurrentWorkspaceItemOpenShortcutAsync().ConfigureAwait(true);
                    break;
                }

                _ = OpenRecordsWindowAsync();
                e.Handled = true;
                break;
            case Key.R:
                _ = OpenRemindersWindowAsync();
                e.Handled = true;
                break;
            case Key.M:
                _ = OpenMaintenanceWindowAsync();
                e.Handled = true;
                break;
        }
    }

    private void OnTabHeaderKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = TryFocusTarget(DesktopFocusTarget.VehicleList);
            return;
        }

        if (sender is not Control source)
        {
            return;
        }

        var currentIndex = Array.IndexOf(TabHeaderButtonNames, source.Name);
        if (currentIndex < 0)
        {
            return;
        }

        var nextIndex = e.Key switch
        {
            Key.Left => Math.Max(0, currentIndex - 1),
            Key.Right => Math.Min(TabHeaderButtonNames.Length - 1, currentIndex + 1),
            Key.Home => 0,
            Key.End => TabHeaderButtonNames.Length - 1,
            _ => -1
        };

        if (nextIndex < 0 || nextIndex == currentIndex)
        {
            return;
        }

        if (_viewModel is null)
        {
            return;
        }

        if (nextIndex != _viewModel.SelectedVehicleTabIndex && _viewModel.BlockWorkspaceNavigationIfEditing())
        {
            e.Handled = true;
            return;
        }

        _viewModel.SelectedVehicleTabIndex = nextIndex;
        e.Handled = FocusSelectedTabHeader();
    }

    private static bool IsMainMenuInvocationKey(KeyEventArgs e)
    {
        return (e.Key == Key.F10 && e.KeyModifiers == KeyModifiers.None)
            || (e.Key is Key.LeftAlt or Key.RightAlt
                && (e.KeyModifiers == KeyModifiers.None || e.KeyModifiers == KeyModifiers.Alt));
    }

    private void OnTabHeaderClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null || sender is not Control { Tag: { } tagValue })
        {
            return;
        }

        if (!int.TryParse(tagValue.ToString(), out var tabIndex))
        {
            return;
        }

        if (tabIndex != _viewModel.SelectedVehicleTabIndex && _viewModel.BlockWorkspaceNavigationIfEditing())
        {
            e.Handled = true;
            return;
        }

        _viewModel.SelectedVehicleTabIndex = tabIndex;
        FocusSelectedTabHeader();
    }

    private void OnElementGotFocus(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Control control || ReferenceEquals(control, this) || IsMainMenuFocusSource(control))
        {
            return;
        }

        _lastNonMenuFocusTarget = control;
    }

    private async void OnVehicleSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_syncingVehicleSelection || _viewModel is null || sender is not ListBox listBox)
        {
            return;
        }

        var selectedVehicle = listBox.SelectedItem as VehicleListItemViewModel;
        if (string.Equals(selectedVehicle?.Id, _viewModel.SelectedVehicle?.Id, StringComparison.Ordinal))
        {
            return;
        }

        if (_viewModel.HasPendingEdits)
        {
            _viewModel.ShellStatus = _viewModel.VehicleListLockStatus;
            SyncVehicleSelectionFromViewModel();
            RequestFocus(_viewModel.GetPendingEditFocusTarget());
            return;
        }

        if (!await _viewModel.ConfirmDiscardPendingEditsAsync(L("PendingEdits.Action.SwitchVehicle")).ConfigureAwait(true))
        {
            SyncVehicleSelectionFromViewModel();
            RequestFocus(_viewModel.GetPendingEditFocusTarget());
            return;
        }

        _viewModel.SelectedVehicle = selectedVehicle;
    }

    private static bool FocusListBox(ListBox listBox)
    {
        if (listBox.SelectedItem is null && listBox.ItemCount > 0)
        {
            listBox.SelectedIndex = 0;
        }

        if (listBox.SelectedItem is not null)
        {
            listBox.ScrollIntoView(listBox.SelectedItem);
        }

        return TryFocusControl(listBox, NavigationMethod.Tab);
    }

    private async void OnSettingsClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.AppShellController.OpenSettingsAsync(this, _viewModel);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnBackupExportClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.AppShellController.ExportBackupAsync(this, _viewModel);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnBackupImportClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.AppShellController.ImportBackupAsync(this, _viewModel);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnOpenDataFolderClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.OpenDataFolderAsync().ConfigureAwait(true);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnCheckDataStoreHealthClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.AppShellController.OpenDataStoreHealthAsync(this, _viewModel).ConfigureAwait(true);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnOpenPreMigrationBackupFolderClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.OpenPreMigrationBackupFolderAsync().ConfigureAwait(true);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnCreateAutomaticBackupNowClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.CreateAutomaticBackupNowAsync().ConfigureAwait(true);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnOpenAutomaticBackupFolderClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.OpenAutomaticBackupFolderAsync().ConfigureAwait(true);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnAboutClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.AppShellController.OpenAboutAsync(this, _viewModel);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnThankAuthorClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.AppShellController.OpenAuthorSupportAsync(_viewModel);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnFeedbackIssueClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.AppShellController.OpenFeedbackIssueAsync(_viewModel);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnUpdateCheckClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (await _viewModel.AppShellController.CheckForUpdatesAsync(this, _viewModel))
        {
            if (ExitApplicationRequested is not null)
            {
                await ExitApplicationRequested().ConfigureAwait(true);
            }
            else
            {
                Close();
            }

            return;
        }
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnExitClick(object? sender, RoutedEventArgs e)
    {
        if (ExitApplicationRequested is null)
        {
            Close();
            return;
        }

        await ExitApplicationRequested().ConfigureAwait(true);
    }

    private async void OnMinimizeToTrayClick(object? sender, RoutedEventArgs e)
    {
        if (MinimizeToTrayRequested is null)
        {
            return;
        }

        await MinimizeToTrayRequested().ConfigureAwait(true);
    }

    private async void OnOpenTrayActionsClick(object? sender, RoutedEventArgs e)
    {
        await OpenTrayActionsAsync().ConfigureAwait(true);
    }

    private async Task OpenTrayActionsAsync()
    {
        if (OpenTrayActionsRequested is null)
        {
            return;
        }

        await OpenTrayActionsRequested().ConfigureAwait(true);
    }

    private void OnCalendarExportClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.ExportCalendarCommand.CanExecute(null) == true)
        {
            _viewModel.ExportCalendarCommand.Execute(null);
        }

        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnPrintableReportClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.AppShellController.OpenPrintableReportAsync(_viewModel).ConfigureAwait(true);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private void OnReloadClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.ReloadCommand.CanExecute(null) == true)
        {
            _viewModel.ReloadCommand.Execute(null);
        }

        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private void OnCreateVehicleMenuClick(object? sender, RoutedEventArgs e)
    {
        OpenVehicleEditorFromCommand(_viewModel?.CreateVehicleCommand, DesktopFocusTarget.VehicleList);
    }

    private void OnEditVehicleMenuClick(object? sender, RoutedEventArgs e)
    {
        OpenVehicleEditorFromCommand(_viewModel?.EditSelectedVehicleCommand, DesktopFocusTarget.VehicleList);
    }

    private async void OnOpenVehicleDetailMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenVehicleDetailWindowAsync();
    }

    private async void OnDeleteVehicleMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.DeleteSelectedVehicleCommand.CanExecute(null) == true)
        {
            await _viewModel.DeleteSelectedVehicleCommand.ExecuteAsync(null);
        }

        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnOpenHistoryMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenHistoryWindowAsync();
    }

    private async void OnOpenFuelMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenFuelWindowAsync();
    }

    private async void OnOpenRecordsMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenRecordsWindowAsync();
    }

    private async void OnOpenRemindersMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenRemindersWindowAsync();
    }

    private async void OnOpenMaintenanceMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenMaintenanceWindowAsync();
    }

    private async void OnOpenVehicleStarterBundleMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenVehicleStarterBundleDialogAsync();
    }

    private async void OnExportVehiclePackageMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.AppShellController.ExportVehiclePackageAsync(this, _viewModel).ConfigureAwait(true);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnImportVehiclePackageMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.AppShellController.ImportVehiclePackageAsync(this, _viewModel).ConfigureAwait(true);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnOpenServiceBookMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenServiceBookWindowAsync();
    }

    private async void OnOpenSelectedVehicleCostsMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.OpenSelectedVehicleCostsCommand.CanExecute(null) == true)
        {
            await _viewModel.OpenSelectedVehicleCostsCommand.ExecuteAsync(null);
        }
    }

    private async void OnOpenTimelineMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenTimelineWindowAsync();
    }

    private async void OnOpenGlobalSearchMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenGlobalSearchWindowAsync();
    }

    private async void OnOpenAuditMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenAuditWindowAsync();
    }

    private async void OnOpenDashboardMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenDashboardWindowAsync();
    }

    private async void OnOpenCostMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenCostWindowAsync();
    }

    private async void OnOpenUpcomingOverviewMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenUpcomingOverviewWindowAsync();
    }

    private async void OnOpenOverdueOverviewMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenOverdueOverviewWindowAsync();
    }

    private async void OnOpenSmartAdvisorMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenSmartAdvisorWindowAsync();
    }

    private async void OnOpenBackgroundNotificationMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.OpenBackgroundNotificationQuickActionCommand is { } command)
        {
            await command.ExecuteAsync(null);
        }
    }

    private async void OnOpenNearestTechnicalMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.OpenNearestTechnicalCommand is { } command)
        {
            await command.ExecuteAsync(null);
        }
    }

    private async void OnReviewTechnicalMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.ReviewTechnicalCommand is { } command)
        {
            await command.ExecuteAsync(null);
        }
    }

    private async void OnOpenNearestGreenCardMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.OpenNearestGreenCardCommand is { } command)
        {
            await command.ExecuteAsync(null);
        }
    }

    private async void OnReviewGreenCardsMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.ReviewGreenCardsCommand is { } command)
        {
            await command.ExecuteAsync(null);
        }
    }

    private async void OnOpenNearestReminderMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.OpenNearestReminderCommand is { } command)
        {
            await command.ExecuteAsync(null);
        }
    }

    private async void OnReviewRemindersMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.ReviewRemindersCommand is { } command)
        {
            await command.ExecuteAsync(null);
        }
    }

    private async void OnOpenNearestMaintenanceMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.OpenNearestMaintenanceCommand is { } command)
        {
            await command.ExecuteAsync(null);
        }
    }

    private async void OnReviewMaintenanceMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.ReviewMaintenanceCommand is { } command)
        {
            await command.ExecuteAsync(null);
        }
    }

    private async void OnOpenNearestRecordMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.OpenNearestRecordCommand is { } command)
        {
            await command.ExecuteAsync(null);
        }
    }

    private async void OnReviewRecordsMenuClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.ReviewRecordsCommand is { } command)
        {
            await command.ExecuteAsync(null);
        }
    }

    private async void OnOpenRemindersWindowClick(object? sender, RoutedEventArgs e)
    {
        await OpenRemindersWindowAsync();
    }

    private async void OnOpenRecordsWindowClick(object? sender, RoutedEventArgs e)
    {
        await OpenRecordsWindowAsync();
    }

    private async void OnOpenHistoryWindowClick(object? sender, RoutedEventArgs e)
    {
        await OpenHistoryWindowAsync();
    }

    private async void OnOpenFuelWindowClick(object? sender, RoutedEventArgs e)
    {
        await OpenFuelWindowAsync();
    }

    private async void OnOpenMaintenanceWindowClick(object? sender, RoutedEventArgs e)
    {
        await OpenMaintenanceWindowAsync();
    }

    private async void OnOpenVehicleDetailWindowClick(object? sender, RoutedEventArgs e)
    {
        await OpenVehicleDetailWindowAsync();
    }

    private async void OnOpenTimelineWindowClick(object? sender, RoutedEventArgs e) => await OpenTimelineWindowAsync();

    private async void OnOpenAuditWindowClick(object? sender, RoutedEventArgs e) => await OpenAuditWindowAsync();

    private async void OnOpenCostWindowClick(object? sender, RoutedEventArgs e) => await OpenCostWindowAsync();

    private async void OnOpenDashboardWindowClick(object? sender, RoutedEventArgs e) => await OpenDashboardWindowAsync();

    private async void OnOpenGlobalSearchWindowClick(object? sender, RoutedEventArgs e) => await OpenGlobalSearchWindowAsync();

    private async void OnOpenUpcomingOverviewWindowClick(object? sender, RoutedEventArgs e) => await OpenUpcomingOverviewWindowAsync();

    private async void OnOpenOverdueOverviewWindowClick(object? sender, RoutedEventArgs e) => await OpenOverdueOverviewWindowAsync();

    private async void OnOpenSmartAdvisorWindowClick(object? sender, RoutedEventArgs e) => await OpenSmartAdvisorWindowAsync();

    private bool FocusSelectedTabHeader()
    {
        if (_viewModel is null)
        {
            return false;
        }

        var selectedIndex = _viewModel.SelectedVehicleTabIndex;
        if (selectedIndex < 0 || selectedIndex >= TabHeaderButtonNames.Length)
        {
            selectedIndex = 0;
        }

        var selectedButton = this.FindControl<Control>(TabHeaderButtonNames[selectedIndex]);
        if (selectedButton is not null)
        {
            return selectedButton.Focus(NavigationMethod.Tab, KeyModifiers.None)
                || selectedButton.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
        }

        return false;
    }

    private bool ToggleMainMenuFocus(object? source)
    {
        if (IsMainMenuFocusSource(source))
        {
            return CloseMainMenuAndRestoreFocus();
        }

        if (source is Control control && !ReferenceEquals(control, this))
        {
            _lastNonMenuFocusTarget = control;
        }

        return FocusMainMenuRoot();
    }

    private bool FocusMainMenuRoot()
    {
        if (this.FindControl<MenuItem>("FileMenuRoot") is not { } fileMenu)
        {
            return false;
        }

        return fileMenu.Focus(NavigationMethod.Tab, KeyModifiers.None)
            || fileMenu.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
    }

    private bool CloseMainMenuAndRestoreFocus()
    {
        CloseMainMenuRoots();
        if (_lastNonMenuFocusTarget is { } target && TryFocusControl(target, NavigationMethod.Unspecified))
        {
            return true;
        }

        return TryFocusTarget(DesktopFocusTarget.VehicleList);
    }

    private void CloseMainMenuRoots()
    {
        foreach (var rootName in MainMenuRootNames)
        {
            if (this.FindControl<MenuItem>(rootName) is { } menuRoot)
            {
                menuRoot.IsSubMenuOpen = false;
            }
        }
    }

    private static bool IsMainMenuFocusSource(object? source)
    {
        return source is Menu or MenuItem;
    }

    private static bool TryFocusControl(Control control, NavigationMethod navigationMethod)
    {
        if (!control.IsEnabled || !control.IsVisible)
        {
            return false;
        }

        return control.Focus(navigationMethod, KeyModifiers.None)
            || control.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
    }

    private bool TryFocusInitialVehicleList()
    {
        if (this.FindControl<ListBox>("VehicleListBox") is not { } listBox)
        {
            return false;
        }

        return FocusListBox(listBox);
    }

    private async Task<bool> ConfirmDeleteVehicleAsync(string message)
    {
        var confirmation = new ConfirmationWindow
        {
            DataContext = new ConfirmationDialogViewModel(
                DesktopLocalization.Localizer.GetString("VehicleDelete.Dialog.Title"),
                message,
                DesktopLocalization.Localizer.GetString("VehicleDelete.Dialog.Confirm"),
                DesktopLocalization.Localizer.GetString("Dialog.Cancel"))
        };

        return await confirmation.ShowDialog<bool>(this);
    }

    private Control? ResolveFocusTarget(DesktopFocusTarget target)
    {
        return target switch
        {
            DesktopFocusTarget.VehicleList => this.FindControl<ListBox>("VehicleListBox"),
            DesktopFocusTarget.VehicleSearch => this.FindControl<TextBox>("VehicleSearchBox"),
            _ => null
        };
    }

    private void SyncVehicleSelectionFromViewModel()
    {
        if (this.FindControl<ListBox>("VehicleListBox") is not { } listBox)
        {
            return;
        }

        _syncingVehicleSelection = true;
        try
        {
            listBox.SelectedItem = _viewModel?.SelectedVehicle;
        }
        finally
        {
            _syncingVehicleSelection = false;
        }
    }

    private async Task<bool> ConfirmDiscardPendingEditsForWindowAsync(string actionDescription)
    {
        if (_viewModel is null || !_viewModel.HasPendingEdits)
        {
            return true;
        }

        if (_viewModel.BlockWorkspaceNavigationIfEditing())
        {
            return false;
        }

        if (!await _viewModel.ConfirmDiscardPendingEditsAsync(actionDescription).ConfigureAwait(true))
        {
            RequestFocus(_viewModel.GetPendingEditFocusTarget());
            return false;
        }

        _viewModel.DiscardPendingEdits();
        return true;
    }

    private async Task<bool> OpenCurrentWorkspaceCreateWindowAsync()
    {
        if (_viewModel is null)
        {
            return false;
        }

        var command = _viewModel.SelectedVehicleTabIndex switch
        {
            HistoryTabIndex => _viewModel.CreateHistoryCommand,
            FuelTabIndex => _viewModel.CreateFuelCommand,
            ReminderTabIndex => _viewModel.CreateReminderCommand,
            MaintenanceTabIndex => _viewModel.CreateMaintenanceCommand,
            RecordTabIndex => _viewModel.CreateRecordCommand,
            _ => null
        };

        return await ExecuteShortcutAndOpenEditorWindowAsync(command).ConfigureAwait(true);
    }

    private async Task<bool> OpenCurrentWorkspaceEditWindowAsync()
    {
        if (_viewModel is null)
        {
            return false;
        }

        switch (_viewModel.SelectedVehicleTabIndex)
        {
            case HistoryTabIndex:
                return await ExecuteShortcutAndOpenEditorWindowAsync(_viewModel.EditSelectedHistoryCommand).ConfigureAwait(true);
            case FuelTabIndex:
                return await ExecuteShortcutAndOpenEditorWindowAsync(_viewModel.EditSelectedFuelCommand).ConfigureAwait(true);
            case ReminderTabIndex:
                return await ExecuteShortcutAndOpenEditorWindowAsync(_viewModel.EditSelectedReminderCommand).ConfigureAwait(true);
            case MaintenanceTabIndex:
                return await ExecuteShortcutAndOpenEditorWindowAsync(_viewModel.EditSelectedMaintenanceCommand).ConfigureAwait(true);
            case RecordTabIndex:
                return await ExecuteShortcutAndOpenEditorWindowAsync(_viewModel.EditSelectedRecordCommand).ConfigureAwait(true);
            default:
                if (!await _viewModel.HandleCurrentWorkspaceEditShortcutAsync().ConfigureAwait(true))
                {
                    return false;
                }

                if (_viewModel.VehicleDetailWorkspace.IsEditingVehicle)
                {
                    return true;
                }

                return await OpenActiveEditorWindowAsync().ConfigureAwait(true);
        }
    }

    private async Task<bool> ExecuteShortcutAndOpenEditorWindowAsync(ICommand? command)
    {
        if (command?.CanExecute(null) != true)
        {
            return false;
        }

        command.Execute(null);
        return true;
    }

    private async Task<bool> OpenActiveEditorWindowAsync()
    {
        if (_viewModel is null)
        {
            return false;
        }

        if (_viewModel.VehicleDetailWorkspace.IsEditingVehicle)
        {
            await ShowVehicleEditorDialogAsync(GetShellVehicleEditReturnFocusTarget()).ConfigureAwait(true);
            return true;
        }

        if (_viewModel.HistoryWorkspace.IsEditingHistory)
        {
            await ShowWorkspaceEditorDialogAsync(new WorkspaceEditorDialogRequest(
                WorkspaceEditorKind.History,
                DesktopFocusTarget.HistoryList)).ConfigureAwait(true);
            return true;
        }

        if (_viewModel.FuelWorkspace.IsEditingFuel)
        {
            await ShowWorkspaceEditorDialogAsync(new WorkspaceEditorDialogRequest(
                WorkspaceEditorKind.Fuel,
                DesktopFocusTarget.FuelList)).ConfigureAwait(true);
            return true;
        }

        if (_viewModel.ReminderWorkspace.IsEditingReminder)
        {
            await ShowWorkspaceEditorDialogAsync(new WorkspaceEditorDialogRequest(
                WorkspaceEditorKind.Reminder,
                DesktopFocusTarget.ReminderList)).ConfigureAwait(true);
            return true;
        }

        if (_viewModel.MaintenanceWorkspace.IsEditingMaintenance)
        {
            await ShowWorkspaceEditorDialogAsync(new WorkspaceEditorDialogRequest(
                WorkspaceEditorKind.Maintenance,
                DesktopFocusTarget.MaintenanceList)).ConfigureAwait(true);
            return true;
        }

        if (_viewModel.RecordWorkspace.IsEditingRecord)
        {
            await ShowWorkspaceEditorDialogAsync(new WorkspaceEditorDialogRequest(
                WorkspaceEditorKind.Record,
                DesktopFocusTarget.RecordList)).ConfigureAwait(true);
            return true;
        }

        return false;
    }

    private void OpenVehicleEditorFromCommand(ICommand? command, DesktopFocusTarget returnFocusTarget)
    {
        if (_viewModel is null || command?.CanExecute(null) != true)
        {
            return;
        }

        _viewModel.SetNextVehicleEditorReturnFocusTarget(returnFocusTarget);
        command.Execute(null);
    }

    private DesktopFocusTarget GetShellVehicleEditReturnFocusTarget() =>
        _viewModel?.SelectedVehicleTabIndex == DetailTabIndex
            ? DesktopFocusTarget.VehicleDetailPrimaryAction
            : DesktopFocusTarget.VehicleList;

    private async Task ShowVehicleEditorDialogAsync(DesktopFocusTarget returnFocusTarget)
    {
        var viewModel = _viewModel;
        if (viewModel is null || _vehicleEditorDialogOpen)
        {
            return;
        }

        _vehicleEditorDialogOpen = true;
        try
        {
            var dialog = new VehicleEditorWindow
            {
                DataContext = viewModel.VehicleDetailWorkspace
            };

            var saved = await dialog.ShowDialog<bool?>(this).ConfigureAwait(true) == true;
            if (saved)
            {
                await OfferPendingVehicleStarterBundleAsync().ConfigureAwait(true);
            }
        }
        finally
        {
            _vehicleEditorDialogOpen = false;
            RequestFocus(viewModel.HasPendingEdits ? viewModel.GetPendingEditFocusTarget() : returnFocusTarget);
        }
    }

    private async Task ShowWorkspaceEditorDialogAsync(WorkspaceEditorDialogRequest request)
    {
        var viewModel = _viewModel;
        if (viewModel is null || _workspaceEditorDialogOpen)
        {
            return;
        }

        _workspaceEditorDialogOpen = true;
        try
        {
            Window dialog = request.Kind switch
            {
                WorkspaceEditorKind.History => new HistoryEditorWindow { DataContext = viewModel.HistoryWorkspace },
                WorkspaceEditorKind.Fuel => new FuelEditorWindow { DataContext = viewModel.FuelWorkspace },
                WorkspaceEditorKind.Reminder => new ReminderEditorWindow { DataContext = viewModel.ReminderWorkspace },
                WorkspaceEditorKind.Maintenance => new MaintenanceEditorWindow { DataContext = viewModel.MaintenanceWorkspace },
                WorkspaceEditorKind.Record => new RecordEditorWindow { DataContext = viewModel.RecordWorkspace },
                _ => throw new InvalidOperationException(DesktopLocalization.Localizer.GetString("WorkspaceEditor.Error.UnknownKind"))
            };

            await dialog.ShowDialog<bool?>(this).ConfigureAwait(true);
        }
        finally
        {
            _workspaceEditorDialogOpen = false;
            RequestFocus(viewModel.HasPendingEdits ? viewModel.GetPendingEditFocusTarget() : request.ReturnFocusTarget);
        }
    }

    private async Task OfferPendingVehicleStarterBundleAsync()
    {
        if (_viewModel is null || !_viewModel.VehicleDetailWorkspace.TryConsumePendingVehicleStarterBundleOffer())
        {
            return;
        }

        try
        {
            var workspace = _viewModel.VehicleDetailWorkspace;
            var preview = workspace.BuildVehicleStarterBundlePreview();
            if (preview.TotalMissingCount == 0)
            {
                workspace.SetVehicleStarterBundleStatus(L("VehicleDetail.Status.NewVehicleBundleNoItems"));
                return;
            }

            var dialog = new VehicleStarterBundleWindow
            {
                DataContext = new VehicleStarterBundleDialogViewModel(preview)
            };

            var result = await dialog.ShowDialog<VehicleStarterBundleDialogResult?>(this).ConfigureAwait(true);
            if (result is null)
            {
                return;
            }

            var message = await workspace.ApplyVehicleStarterBundleAsync(result.SelectedItems).ConfigureAwait(true);
            workspace.SetVehicleStarterBundleStatus(message);
        }
        catch (Exception ex)
        {
            _viewModel.VehicleDetailWorkspace.SetVehicleStarterBundleStatus(
                LF("VehicleDetail.Status.NewVehicleBundleOpenFailed", ex.Message));
        }
    }

    private async Task ShowWorkspaceWindowAsync<TWindow>(
        int tabIndex,
        object workspace,
        DesktopFocusTarget returnFocusTarget,
        string confirmActionDescription,
        bool allowActiveEditor = false)
        where TWindow : Window, new()
    {
        if (_viewModel is null)
        {
            return;
        }

        if (!allowActiveEditor && !await ConfirmDiscardPendingEditsForWindowAsync(confirmActionDescription).ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = tabIndex;
        var dialog = new TWindow
        {
            DataContext = workspace
        };

        await dialog.ShowDialog(this).ConfigureAwait(true);
        RequestFocus(_viewModel.HasPendingEdits ? _viewModel.GetPendingEditFocusTarget() : returnFocusTarget);
    }

    private async Task OpenVehicleDetailWindowAsync(bool startCreate = false, bool startEdit = false, bool allowActiveEditor = false)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (startCreate)
        {
            OpenVehicleEditorFromCommand(_viewModel.CreateVehicleCommand, DesktopFocusTarget.VehicleList);
            return;
        }
        else if (startEdit)
        {
            OpenVehicleEditorFromCommand(_viewModel.EditSelectedVehicleCommand, GetShellVehicleEditReturnFocusTarget());
            return;
        }
        else
        {
            if (!allowActiveEditor && _viewModel.SelectedVehicle is null)
            {
                return;
            }

            if (!allowActiveEditor && !await ConfirmDiscardPendingEditsForWindowAsync(L("PendingEdits.Action.OpenVehicleDetailWindow")).ConfigureAwait(true))
            {
                return;
            }
        }

        await ShowWorkspaceWindowAsync<VehicleDetailWindow>(
            DetailTabIndex,
            _viewModel.VehicleDetailWorkspace,
            DesktopFocusTarget.VehicleList,
            L("PendingEdits.Action.OpenVehicleDetailWindow"),
            allowActiveEditor: true).ConfigureAwait(true);
    }

    private async Task OpenHistoryWindowAsync(bool allowActiveEditor = false)
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<HistoryWindow>(
            HistoryTabIndex,
            _viewModel.HistoryWorkspace,
            DesktopFocusTarget.HistoryList,
            L("PendingEdits.Action.OpenHistoryWindow"),
            allowActiveEditor).ConfigureAwait(true);
    }

    private async Task OpenFuelWindowAsync(bool allowActiveEditor = false)
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<FuelWindow>(
            FuelTabIndex,
            _viewModel.FuelWorkspace,
            DesktopFocusTarget.FuelList,
            L("PendingEdits.Action.OpenFuelWindow"),
            allowActiveEditor).ConfigureAwait(true);
    }

    private async Task OpenRemindersWindowAsync(bool allowActiveEditor = false)
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<RemindersWindow>(
            ReminderTabIndex,
            _viewModel.ReminderWorkspace,
            DesktopFocusTarget.ReminderList,
            L("PendingEdits.Action.OpenRemindersWindow"),
            allowActiveEditor).ConfigureAwait(true);
    }

    private async Task OpenMaintenanceWindowAsync(bool allowActiveEditor = false)
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<MaintenanceWindow>(
            MaintenanceTabIndex,
            _viewModel.MaintenanceWorkspace,
            DesktopFocusTarget.MaintenanceList,
            L("PendingEdits.Action.OpenMaintenanceWindow"),
            allowActiveEditor).ConfigureAwait(true);
    }

    private async Task OpenTimelineWindowAsync()
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<TimelineWindow>(
            TimelineTabIndex,
            _viewModel.TimelineWorkspace,
            DesktopFocusTarget.TimelineSearch,
            L("PendingEdits.Action.OpenTimelineWindow")).ConfigureAwait(true);
    }

    private async Task OpenServiceBookWindowAsync()
    {
        if (_viewModel?.SelectedVehicle is null || !_viewModel.CanOpenSelectedVehicleServiceBook)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync(L("PendingEdits.Action.OpenServiceBook")).ConfigureAwait(true))
        {
            return;
        }

        var model = _viewModel.BuildSelectedVehicleServiceBookModel();
        if (model is null)
        {
            RequestFocus(DesktopFocusTarget.VehicleList);
            return;
        }

        var dialog = new ServiceBookWindow
        {
            DataContext = model
        };

        await dialog.ShowDialog(this).ConfigureAwait(true);
        RequestFocus(model.DidOpenSelectedItem ? DesktopFocusTarget.SelectedVehicleTabHeader : DesktopFocusTarget.VehicleList);
    }

    private async Task OpenAuditWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<AuditWindow>(
            AuditTabIndex,
            _viewModel.AuditWorkspace,
            DesktopFocusTarget.AuditList,
            L("PendingEdits.Action.OpenAuditWindow")).ConfigureAwait(true);
    }

    private async Task OpenCostWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<CostWindow>(
            CostTabIndex,
            _viewModel.CostWorkspace,
            DesktopFocusTarget.CostList,
            L("PendingEdits.Action.OpenCostWindow")).ConfigureAwait(true);
    }

    private async Task OpenDashboardWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<DashboardWindow>(
            DashboardTabIndex,
            _viewModel.DashboardWorkspace,
            DesktopFocusTarget.DashboardAuditList,
            L("PendingEdits.Action.OpenDashboardWindow")).ConfigureAwait(true);
    }

    private async Task OpenGlobalSearchWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<GlobalSearchWindow>(
            SearchTabIndex,
            _viewModel.GlobalSearchWorkspace,
            DesktopFocusTarget.GlobalSearchBox,
            L("PendingEdits.Action.OpenGlobalSearchWindow")).ConfigureAwait(true);
    }

    private async Task OpenUpcomingOverviewWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<UpcomingOverviewWindow>(
            UpcomingOverviewTabIndex,
            _viewModel.UpcomingOverviewWorkspace,
            DesktopFocusTarget.UpcomingOverviewSearch,
            L("PendingEdits.Action.OpenUpcomingOverviewWindow")).ConfigureAwait(true);
    }

    private async Task OpenOverdueOverviewWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<OverdueOverviewWindow>(
            OverdueOverviewTabIndex,
            _viewModel.OverdueOverviewWorkspace,
            DesktopFocusTarget.OverdueOverviewSearch,
            L("PendingEdits.Action.OpenOverdueOverviewWindow")).ConfigureAwait(true);
    }

    private async Task OpenSmartAdvisorWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<SmartAdvisorWindow>(
            SmartAdvisorTabIndex,
            _viewModel.SmartAdvisorWorkspace,
            DesktopFocusTarget.SmartAdvisorList,
            L("PendingEdits.Action.OpenSmartAdvisorWindow")).ConfigureAwait(true);
    }

    private async Task OpenRecordsWindowAsync(bool allowActiveEditor = false)
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        await ShowWorkspaceWindowAsync<RecordsWindow>(
            RecordTabIndex,
            _viewModel.RecordWorkspace,
            DesktopFocusTarget.RecordList,
            L("PendingEdits.Action.OpenRecordsWindow"),
            allowActiveEditor).ConfigureAwait(true);
    }

    private async Task OpenVehicleStarterBundleDialogAsync()
    {
        if (_viewModel?.SelectedVehicle is null || !_viewModel.CanOpenVehicleStarterBundle)
        {
            return;
        }

        var workspace = _viewModel.VehicleDetailWorkspace;
        var preview = workspace.BuildVehicleStarterBundlePreview();
        if (preview.TotalMissingCount == 0)
        {
            workspace.SetVehicleStarterBundleStatus(L("VehicleDetail.Status.BundleNoMissingItems"));
            RequestFocus(DesktopFocusTarget.VehicleList);
            return;
        }

        var dialog = new VehicleStarterBundleWindow
        {
            DataContext = new VehicleStarterBundleDialogViewModel(preview)
        };

        var result = await dialog.ShowDialog<VehicleStarterBundleDialogResult?>(this);
        if (result is null)
        {
            RequestFocus(DesktopFocusTarget.VehicleList);
            return;
        }

        var message = await workspace.ApplyVehicleStarterBundleAsync(result.SelectedItems).ConfigureAwait(true);
        workspace.SetVehicleStarterBundleStatus(message);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }
}
