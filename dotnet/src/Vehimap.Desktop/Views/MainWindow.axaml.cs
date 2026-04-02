using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Application.Models;
using Vehimap.Desktop.Services;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class MainWindow : Window
{
    private const int DetailTabIndex = 0;
    private const int HistoryTabIndex = 1;
    private const int FuelTabIndex = 2;
    private const int ReminderTabIndex = 3;
    private const int MaintenanceTabIndex = 4;
    private const int RecordTabIndex = 6;
    private const int AuditTabIndex = 7;
    private const int DashboardTabIndex = 9;

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

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        RegisterTabBoundaryNavigation();
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
        }

        _viewModel = DataContext as MainWindowViewModel;
        if (_viewModel is not null)
        {
            _viewModel.FocusRequested += OnFocusRequested;
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

        var dialog = new SettingsWindow
        {
            DataContext = SettingsDialogViewModel.FromSnapshot(_viewModel.GetSupportedSettingsSnapshot())
        };
        var result = await dialog.ShowDialog<DesktopSupportedSettingsSnapshot?>(this);
        if (result is null)
        {
            RequestFocus(DesktopFocusTarget.VehicleList);
            return;
        }

        await _viewModel.SaveSupportedSettingsAsync(result);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnBackupExportClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        var backupPath = await _viewModel.PickBackupExportPathAsync();
        if (string.IsNullOrWhiteSpace(backupPath))
        {
            RequestFocus(DesktopFocusTarget.VehicleList);
            return;
        }

        await _viewModel.ExportBackupAsync(backupPath);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnBackupImportClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        var backupPath = await _viewModel.PickBackupImportPathAsync();
        if (string.IsNullOrWhiteSpace(backupPath))
        {
            RequestFocus(DesktopFocusTarget.VehicleList);
            return;
        }

        var confirmation = new ConfirmationWindow
        {
            DataContext = new ConfirmationDialogViewModel(
                "Obnovit data ze zálohy",
                $"Opravdu chcete nahradit aktuální načtená data obsahem zálohy?\n\n{backupPath}\n\nTento krok přepíše aktuální pracovní data v desktopové větvi.",
                "Obnovit data",
                "Zrušit")
        };
        var confirmed = await confirmation.ShowDialog<bool>(this);
        if (!confirmed)
        {
            RequestFocus(DesktopFocusTarget.VehicleList);
            return;
        }

        await _viewModel.ImportBackupAsync(backupPath);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnAboutClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        var dialog = new AboutWindow
        {
            DataContext = _viewModel.BuildAboutDialogModel()
        };
        var openReleaseNotes = await dialog.ShowDialog<bool>(this);
        if (openReleaseNotes && dialog.DataContext is AboutDialogViewModel aboutViewModel)
        {
            await _viewModel.OpenExternalAsync(aboutViewModel.ReleaseNotesUrl);
        }

        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnUpdateCheckClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        var result = await _viewModel.CheckForUpdatesAsync();
        var dialog = new UpdateCheckWindow
        {
            DataContext = new UpdateDialogViewModel(result)
        };
        var action = await dialog.ShowDialog<UpdateDialogAction>(this);

        switch (action)
        {
            case UpdateDialogAction.PrimaryAction:
                if (result.IsUpdateAvailable && result.CanInstallAutomatically)
                {
                    var installResult = await _viewModel.PrepareUpdateInstallAsync(result);
                    if (installResult.IsReady && installResult.InstallPlan is not null)
                    {
                        UpdateInstallLauncher.Launch(installResult.InstallPlan);
                        Close();
                        return;
                    }

                    var failureDialog = new UpdateCheckWindow
                    {
                        DataContext = new UpdateDialogViewModel(new Vehimap.Application.UpdateCheckResult(
                            result.CurrentVersion,
                            result.LatestVersion,
                            false,
                            result.PublishedAt,
                            result.NotesUrl,
                            result.AssetUrl,
                            result.Sha256,
                            result.AssetSize,
                            false,
                            installResult.Message,
                            installResult.Message))
                    };
                    await failureDialog.ShowDialog<UpdateDialogAction>(this);
                }
                else if (!string.IsNullOrWhiteSpace(result.NotesUrl))
                {
                    await _viewModel.OpenExternalAsync(result.NotesUrl);
                }
                else if (!string.IsNullOrWhiteSpace(result.AssetUrl))
                {
                    await _viewModel.OpenExternalAsync(result.AssetUrl);
                }

                break;
            case UpdateDialogAction.OpenAsset:
                if (!string.IsNullOrWhiteSpace(result.AssetUrl))
                {
                    await _viewModel.OpenExternalAsync(result.AssetUrl);
                }

                break;
        }

        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnOpenRemindersWindowClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.SelectedVehicle is null)
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

    private async void OnOpenRecordsWindowClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.SelectedVehicle is null)
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

    private async void OnOpenHistoryWindowClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.SelectedVehicle is null)
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

    private async void OnOpenFuelWindowClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.SelectedVehicle is null)
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

    private async void OnOpenMaintenanceWindowClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.SelectedVehicle is null)
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

    private async void OnOpenVehicleDetailWindowClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.SelectedVehicle is null)
        {
            return;
        }

        _viewModel.SelectedVehicleTabIndex = DetailTabIndex;
        var dialog = new VehicleDetailWindow
        {
            DataContext = _viewModel.VehicleDetailWorkspace
        };

        await dialog.ShowDialog(this);
        RequestFocus(DesktopFocusTarget.VehicleList);
    }

    private async void OnOpenAuditWindowClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
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

    private async void OnOpenDashboardWindowClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
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

    private Control? ResolveFocusTarget(DesktopFocusTarget target)
    {
        return target switch
        {
            DesktopFocusTarget.VehicleList => this.FindControl<ListBox>("VehicleListBox"),
            _ => null
        };
    }
}
