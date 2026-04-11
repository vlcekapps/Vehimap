using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System.ComponentModel;
using Vehimap.Application.Models;
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
        "OverdueOverviewTabButton"
    ];

    private MainWindowViewModel? _viewModel;
    private bool _initialFocusCompleted;
    private bool _initialFocusScheduled;
    private bool _syncingVehicleSelection;

    public Func<Task>? ExitApplicationRequested { get; set; }
    public Func<Task>? MinimizeToTrayRequested { get; set; }

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterTabBoundaryNavigation();
        AddHandler(InputElement.KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
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
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.ConfirmPendingEditsHandler = null;
        }

        _viewModel = DataContext as MainWindowViewModel;
        if (_viewModel is not null)
        {
            var viewModel = _viewModel;
            viewModel.FocusRequested += OnFocusRequested;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.ConfirmPendingEditsHandler = actionDescription =>
                viewModel.AppShellController.ConfirmDiscardPendingChangesAsync(this, viewModel, actionDescription);
            SyncVehicleSelectionFromViewModel();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.Equals(e.PropertyName, nameof(MainWindowViewModel.SelectedVehicle), StringComparison.Ordinal))
        {
            SyncVehicleSelectionFromViewModel();
        }
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
                _initialFocusCompleted = TryFocusTarget(DesktopFocusTarget.VehicleList);
            }, TimeSpan.FromMilliseconds(140));
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

    private void OnForwardTabToHeadersKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab || e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            return;
        }

        e.Handled = FocusSelectedTabHeader();
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F10 && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = FocusAndOpenMainMenu();
            return;
        }

        if (e.KeyModifiers != KeyModifiers.Control)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.N:
                _ = OpenVehicleDetailWindowAsync(startCreate: true);
                e.Handled = true;
                break;
            case Key.U:
                _ = OpenVehicleDetailWindowAsync(startEdit: true);
                e.Handled = true;
                break;
            case Key.O:
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

        _viewModel.SelectedVehicleTabIndex = nextIndex;
        e.Handled = FocusSelectedTabHeader();
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

        _viewModel.SelectedVehicleTabIndex = tabIndex;
        FocusSelectedTabHeader();
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

        if (!await _viewModel.ConfirmDiscardPendingEditsAsync("přejít na jiné vozidlo").ConfigureAwait(true))
        {
            SyncVehicleSelectionFromViewModel();
            RequestFocus(_viewModel.GetPendingEditFocusTarget());
            return;
        }

        _viewModel.SelectedVehicle = selectedVehicle;
    }

    private static bool FocusListBox(ListBox listBox)
    {
        if (listBox.ItemCount <= 0)
        {
            return listBox.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
        }

        if (listBox.SelectedItem is null && listBox.ItemCount > 0)
        {
            listBox.SelectedIndex = 0;
        }

        if (listBox.SelectedItem is not null)
        {
            listBox.ScrollIntoView(listBox.SelectedItem);
            if (listBox.ContainerFromItem(listBox.SelectedItem) is Control itemContainer)
            {
                return itemContainer.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
            }
        }

        return listBox.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
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

    private async void OnAboutClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        await _viewModel.AppShellController.OpenAboutAsync(this, _viewModel);
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
            Close();
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

    private async void OnCreateVehicleMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenVehicleDetailWindowAsync(startCreate: true);
    }

    private async void OnEditVehicleMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenVehicleDetailWindowAsync(startEdit: true);
    }

    private async void OnOpenVehicleDetailMenuClick(object? sender, RoutedEventArgs e)
    {
        await OpenVehicleDetailWindowAsync();
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

    private bool FocusAndOpenMainMenu()
    {
        if (this.FindControl<MenuItem>("FileMenuRoot") is not { } fileMenu)
        {
            return false;
        }

        var focused = fileMenu.Focus(NavigationMethod.Tab, KeyModifiers.None)
            || fileMenu.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
        fileMenu.IsSubMenuOpen = true;
        return focused || fileMenu.IsSubMenuOpen;
    }

    private Control? ResolveFocusTarget(DesktopFocusTarget target)
    {
        return target switch
        {
            DesktopFocusTarget.VehicleList => this.FindControl<ListBox>("VehicleListBox"),
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

    public void SetMinimizeToTrayAvailability(bool isAvailable)
    {
        if (this.FindControl<MenuItem>("MinimizeToTrayButton") is { } button)
        {
            button.IsEnabled = isAvailable;
        }
    }

    private async Task<bool> ConfirmDiscardPendingEditsForWindowAsync(string actionDescription)
    {
        if (_viewModel is null || !_viewModel.HasPendingEdits)
        {
            return true;
        }

        if (!await _viewModel.ConfirmDiscardPendingEditsAsync(actionDescription).ConfigureAwait(true))
        {
            RequestFocus(_viewModel.GetPendingEditFocusTarget());
            return false;
        }

        _viewModel.DiscardPendingEdits();
        return true;
    }

    private async Task OpenVehicleDetailWindowAsync(bool startCreate = false, bool startEdit = false)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (startCreate)
        {
            if (_viewModel.CreateVehicleCommand.CanExecute(null) != true)
            {
                return;
            }

            _viewModel.CreateVehicleCommand.Execute(null);
        }
        else if (startEdit)
        {
            if (_viewModel.EditSelectedVehicleCommand.CanExecute(null) != true)
            {
                return;
            }

            _viewModel.EditSelectedVehicleCommand.Execute(null);
        }
        else
        {
            if (_viewModel.SelectedVehicle is null)
            {
                return;
            }

            if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít detail vybraného vozidla").ConfigureAwait(true))
            {
                return;
            }
        }

        _viewModel.SelectedVehicleTabIndex = DetailTabIndex;
        var dialog = new VehicleDetailWindow
        {
            DataContext = _viewModel.VehicleDetailWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(_viewModel.HasPendingEdits ? _viewModel.GetPendingEditFocusTarget() : DesktopFocusTarget.VehicleList);
    }

    private async Task OpenHistoryWindowAsync()
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít historii vybraného vozidla").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = HistoryTabIndex;
        var dialog = new HistoryWindow
        {
            DataContext = _viewModel.HistoryWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.HistoryList);
    }

    private async Task OpenFuelWindowAsync()
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít tankování vybraného vozidla").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = FuelTabIndex;
        var dialog = new FuelWindow
        {
            DataContext = _viewModel.FuelWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.FuelList);
    }

    private async Task OpenRemindersWindowAsync()
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít připomínky vybraného vozidla").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = ReminderTabIndex;
        var dialog = new RemindersWindow
        {
            DataContext = _viewModel.ReminderWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.ReminderList);
    }

    private async Task OpenMaintenanceWindowAsync()
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít plán údržby vybraného vozidla").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = MaintenanceTabIndex;
        var dialog = new MaintenanceWindow
        {
            DataContext = _viewModel.MaintenanceWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.MaintenanceList);
    }

    private async Task OpenTimelineWindowAsync()
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít časovou osu vybraného vozidla").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = TimelineTabIndex;
        var dialog = new TimelineWindow
        {
            DataContext = _viewModel.TimelineWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.TimelineSearch);
    }

    private async Task OpenAuditWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít audit dat").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = AuditTabIndex;
        var dialog = new AuditWindow
        {
            DataContext = _viewModel.AuditWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.AuditList);
    }

    private async Task OpenCostWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít náklady napříč vozidly").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = CostTabIndex;
        var dialog = new CostWindow
        {
            DataContext = _viewModel.CostWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.CostList);
    }

    private async Task OpenDashboardWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít dashboard").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = DashboardTabIndex;
        var dialog = new DashboardWindow
        {
            DataContext = _viewModel.DashboardWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.DashboardAuditList);
    }

    private async Task OpenGlobalSearchWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít globální hledání").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = SearchTabIndex;
        var dialog = new GlobalSearchWindow
        {
            DataContext = _viewModel.GlobalSearchWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.GlobalSearchBox);
    }

    private async Task OpenUpcomingOverviewWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít blížící se termíny").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = UpcomingOverviewTabIndex;
        var dialog = new UpcomingOverviewWindow
        {
            DataContext = _viewModel.UpcomingOverviewWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.UpcomingOverviewSearch);
    }

    private async Task OpenOverdueOverviewWindowAsync()
    {
        if (_viewModel is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít propadlé termíny").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = OverdueOverviewTabIndex;
        var dialog = new OverdueOverviewWindow
        {
            DataContext = _viewModel.OverdueOverviewWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.OverdueOverviewSearch);
    }

    private async Task OpenRecordsWindowAsync()
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        if (!await ConfirmDiscardPendingEditsForWindowAsync("otevřít doklady vybraného vozidla").ConfigureAwait(true))
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = RecordTabIndex;
        var dialog = new RecordsWindow
        {
            DataContext = _viewModel.RecordWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.RecordList);
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
            workspace.SetVehicleStarterBundleStatus("Balíček pro vozidlo už nemá žádné chybějící položky.");
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
