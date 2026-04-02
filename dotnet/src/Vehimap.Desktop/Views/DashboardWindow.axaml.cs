using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class DashboardWindow : Window
{
    private MainWindowViewModel? _viewModel;

    public DashboardWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += OnOpened;
        Closed += OnClosed;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() => RequestFocus(DesktopFocusTarget.DashboardAuditList), DispatcherPriority.Loaded);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.FocusRequested -= OnFocusRequested;
        }
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
        switch (target)
        {
            case DesktopFocusTarget.DashboardAuditList:
            case DesktopFocusTarget.DashboardCostList:
            case DesktopFocusTarget.DashboardTimelineList:
                RequestFocus(target);
                break;
        }
    }

    private void RequestFocus(DesktopFocusTarget target)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var listBox = ResolveListBox(target);
            if (listBox is null)
            {
                return;
            }

            if (listBox.ItemCount > 0 && listBox.SelectedItem is null)
            {
                listBox.SelectedIndex = 0;
            }

            if (listBox.SelectedItem is not null)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
                if (listBox.ContainerFromItem(listBox.SelectedItem) is Control itemContainer)
                {
                    itemContainer.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
                    return;
                }
            }

            listBox.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
        }, DispatcherPriority.Background);
    }

    private ListBox? ResolveListBox(DesktopFocusTarget target)
    {
        return target switch
        {
            DesktopFocusTarget.DashboardAuditList => this.FindControl<ListBox>("DashboardWindowAuditListBox"),
            DesktopFocusTarget.DashboardCostList => this.FindControl<ListBox>("DashboardWindowCostListBox"),
            DesktopFocusTarget.DashboardTimelineList => this.FindControl<ListBox>("DashboardWindowTimelineListBox"),
            _ => null
        };
    }

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
