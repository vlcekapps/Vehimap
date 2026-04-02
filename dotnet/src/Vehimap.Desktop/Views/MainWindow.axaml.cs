using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class MainWindow : Window
{
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
        RegisterShiftTabBackNavigation("HistoryListBox");
        RegisterShiftTabBackNavigation("FuelListBox");
        RegisterShiftTabBackNavigation("ReminderListBox");
        RegisterShiftTabBackNavigation("MaintenanceListBox");
        RegisterShiftTabBackNavigation("TimelineFilterComboBox");
        RegisterShiftTabBackNavigation("TimelineSearchBox");
        RegisterShiftTabBackNavigation("TimelineOpenButton");
        RegisterShiftTabBackNavigation("RecordListBox");
        RegisterShiftTabBackNavigation("AuditListBox");
        RegisterShiftTabBackNavigation("CostListBox");
        RegisterShiftTabBackNavigation("DashboardAuditOpenButton");
        RegisterShiftTabBackNavigation("DashboardCostOpenButton");
        RegisterShiftTabBackNavigation("DashboardTimelineOpenButton");
    }

    private void RegisterForwardTabToHeaders(string controlName)
    {
        if (this.FindControl<Control>(controlName) is { } control)
        {
            control.AddHandler(InputElement.KeyDownEvent, OnForwardTabToHeadersKeyDown, RoutingStrategies.Tunnel);
        }
    }

    private void RegisterShiftTabBackNavigation(string controlName)
    {
        if (this.FindControl<Control>(controlName) is { } control)
        {
            control.AddHandler(InputElement.KeyDownEvent, OnTabBoundaryKeyDown, RoutingStrategies.Tunnel);
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

    private void OnTabBoundaryKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab || !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            return;
        }

        e.Handled = FocusSelectedTabHeader();
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

    private bool FocusSelectedTabHeader()
    {
        if (this.FindControl<TabControl>("VehicleTabControl") is not { } tabControl)
        {
            return false;
        }

        var selectedIndex = tabControl.SelectedIndex;
        if (selectedIndex < 0)
        {
            selectedIndex = 0;
        }

        if (tabControl.FindDescendantOfType<TabStrip>() is { } tabStrip
            && tabStrip.ContainerFromIndex(selectedIndex) is Control selectedTabHeader)
        {
            return selectedTabHeader.Focus(NavigationMethod.Tab, KeyModifiers.Shift)
                || selectedTabHeader.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
        }

        if (tabControl.ContainerFromIndex(selectedIndex) is Control selectedTab)
        {
            return selectedTab.Focus(NavigationMethod.Tab, KeyModifiers.Shift)
                || selectedTab.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
        }

        return tabControl.Focus(NavigationMethod.Tab, KeyModifiers.Shift)
            || tabControl.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
    }

    private Control? ResolveFocusTarget(DesktopFocusTarget target)
    {
        return target switch
        {
            DesktopFocusTarget.VehicleList => this.FindControl<ListBox>("VehicleListBox"),
            DesktopFocusTarget.HistoryList => this.FindControl<ListBox>("HistoryListBox"),
            DesktopFocusTarget.FuelList => this.FindControl<ListBox>("FuelListBox"),
            DesktopFocusTarget.ReminderList => this.FindControl<ListBox>("ReminderListBox"),
            DesktopFocusTarget.MaintenanceList => this.FindControl<ListBox>("MaintenanceListBox"),
            DesktopFocusTarget.TimelineSearch => this.FindControl<TextBox>("TimelineSearchBox"),
            DesktopFocusTarget.TimelineList => this.FindControl<ListBox>("TimelineListBox"),
            DesktopFocusTarget.RecordList => this.FindControl<ListBox>("RecordListBox"),
            DesktopFocusTarget.AuditList => this.FindControl<ListBox>("AuditListBox"),
            DesktopFocusTarget.CostList => this.FindControl<ListBox>("CostListBox"),
            DesktopFocusTarget.DashboardAuditList => this.FindControl<ListBox>("DashboardAuditListBox"),
            DesktopFocusTarget.DashboardCostList => this.FindControl<ListBox>("DashboardCostListBox"),
            DesktopFocusTarget.DashboardTimelineList => this.FindControl<ListBox>("DashboardTimelineListBox"),
            _ => this.FindControl<ListBox>("VehicleListBox")
        };
    }
}
