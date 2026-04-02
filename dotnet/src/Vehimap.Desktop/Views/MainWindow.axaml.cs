using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += OnOpened;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        RequestFocus(DesktopFocusTarget.VehicleList);
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

    private void RequestFocus(DesktopFocusTarget target)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var control = ResolveFocusTarget(target);
            control?.Focus();
        }, DispatcherPriority.Background);
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
