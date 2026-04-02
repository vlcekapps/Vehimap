using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class AuditWindow : Window
{
    private MainWindowViewModel? _viewModel;

    public AuditWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += OnOpened;
        Closed += OnClosed;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() => RequestFocus(DesktopFocusTarget.AuditList), DispatcherPriority.Loaded);
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
        if (target == DesktopFocusTarget.AuditList)
        {
            RequestFocus(target);
        }
    }

    private void RequestFocus(DesktopFocusTarget target)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (target != DesktopFocusTarget.AuditList)
            {
                return;
            }

            if (this.FindControl<ListBox>("AuditWindowListBox") is not { } listBox)
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

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
