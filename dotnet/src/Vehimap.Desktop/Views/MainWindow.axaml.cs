using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class MainWindow : Window
{
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
        "SearchTabButton"
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
        RegisterShiftTabBackNavigation("HistoryListBox");
        RegisterShiftTabBackNavigation("HistoryEditorDateBox");
        RegisterShiftTabBackNavigation("FuelListBox");
        RegisterShiftTabBackNavigation("FuelEditorDateBox");
        RegisterShiftTabBackNavigation("ReminderListBox");
        RegisterShiftTabBackNavigation("ReminderEditorTitleBox");
        RegisterShiftTabBackNavigation("MaintenanceListBox");
        RegisterShiftTabBackNavigation("MaintenanceEditorTitleBox");
        RegisterShiftTabBackNavigation("TimelineFilterComboBox");
        RegisterShiftTabBackNavigation("TimelineSearchBox");
        RegisterShiftTabBackNavigation("TimelineOpenButton");
        RegisterShiftTabBackNavigation("RecordListBox");
        RegisterShiftTabBackNavigation("RecordEditorTitleBox");
        RegisterShiftTabBackNavigation("AuditListBox");
        RegisterShiftTabBackNavigation("CostListBox");
        RegisterShiftTabBackNavigation("DashboardAuditOpenButton");
        RegisterShiftTabBackNavigation("DashboardCostOpenButton");
        RegisterShiftTabBackNavigation("DashboardTimelineOpenButton");
        RegisterShiftTabBackNavigation("GlobalSearchTextBox");
        RegisterShiftTabBackNavigation("SearchResultsListBox");
    }

    private void RegisterTabHeaderNavigation()
    {
        foreach (var controlName in TabHeaderButtonNames)
        {
            if (this.FindControl<Button>(controlName) is { } button)
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

        var selectedButton = this.FindControl<Button>(TabHeaderButtonNames[selectedIndex]);
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
            DesktopFocusTarget.HistoryList => this.FindControl<ListBox>("HistoryListBox"),
            DesktopFocusTarget.HistoryEditorDate => this.FindControl<TextBox>("HistoryEditorDateBox"),
            DesktopFocusTarget.FuelList => this.FindControl<ListBox>("FuelListBox"),
            DesktopFocusTarget.FuelEditorDate => this.FindControl<TextBox>("FuelEditorDateBox"),
            DesktopFocusTarget.ReminderList => this.FindControl<ListBox>("ReminderListBox"),
            DesktopFocusTarget.ReminderEditorTitle => this.FindControl<TextBox>("ReminderEditorTitleBox"),
            DesktopFocusTarget.MaintenanceList => this.FindControl<ListBox>("MaintenanceListBox"),
            DesktopFocusTarget.MaintenanceEditorTitle => this.FindControl<TextBox>("MaintenanceEditorTitleBox"),
            DesktopFocusTarget.TimelineSearch => this.FindControl<TextBox>("TimelineSearchBox"),
            DesktopFocusTarget.TimelineList => this.FindControl<ListBox>("TimelineListBox"),
            DesktopFocusTarget.GlobalSearchBox => this.FindControl<TextBox>("GlobalSearchTextBox"),
            DesktopFocusTarget.GlobalSearchList => this.FindControl<ListBox>("SearchResultsListBox"),
            DesktopFocusTarget.RecordList => this.FindControl<ListBox>("RecordListBox"),
            DesktopFocusTarget.RecordEditorTitle => this.FindControl<TextBox>("RecordEditorTitleBox"),
            DesktopFocusTarget.AuditList => this.FindControl<ListBox>("AuditListBox"),
            DesktopFocusTarget.CostList => this.FindControl<ListBox>("CostListBox"),
            DesktopFocusTarget.DashboardAuditList => this.FindControl<ListBox>("DashboardAuditListBox"),
            DesktopFocusTarget.DashboardCostList => this.FindControl<ListBox>("DashboardCostListBox"),
            DesktopFocusTarget.DashboardTimelineList => this.FindControl<ListBox>("DashboardTimelineListBox"),
            _ => this.FindControl<ListBox>("VehicleListBox")
        };
    }
}
