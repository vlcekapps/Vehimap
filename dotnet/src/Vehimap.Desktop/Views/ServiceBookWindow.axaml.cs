using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class ServiceBookWindow : Window
{
    private ServiceBookWindowViewModel? _viewModel;

    public ServiceBookWindow()
    {
        AvaloniaXamlLoader.Load(this);
        DataContextChanged += OnDataContextChanged;
        Opened += OnOpened;
        AddHandler(InputElement.KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Bubble);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.CloseRequested -= OnCloseRequested;
        }

        _viewModel = DataContext as ServiceBookWindowViewModel;
        if (_viewModel is not null)
        {
            _viewModel.CloseRequested += OnCloseRequested;
        }
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(FocusFirstLogicalControl, DispatcherPriority.Loaded);
    }

    private void OnCloseRequested() => Close();

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            Close();
        }
    }

    private void FocusFirstLogicalControl()
    {
        var candidates = new[]
        {
            "ServiceBookHistoryListBox",
            "ServiceBookMaintenanceListBox",
            "ServiceBookRecordListBox",
            "ExportServiceBookHtmlButton",
            "CloseServiceBookWindowButton"
        };

        foreach (var candidate in candidates)
        {
            if (this.FindControl<Control>(candidate) is not { } control || !control.IsVisible || !control.IsEnabled)
            {
                continue;
            }

            if (control is ListBox { ItemCount: > 0 } listBox)
            {
                if (listBox.SelectedItem is null)
                {
                    listBox.SelectedIndex = 0;
                }

                if (listBox.SelectedItem is not null)
                {
                    listBox.ScrollIntoView(listBox.SelectedItem);
                }
                if (listBox.Focus(NavigationMethod.Tab, KeyModifiers.None)
                    || listBox.Focus(NavigationMethod.Unspecified, KeyModifiers.None))
                {
                    return;
                }
            }
            else if (control.Focus(NavigationMethod.Tab, KeyModifiers.None)
                     || control.Focus(NavigationMethod.Unspecified, KeyModifiers.None))
            {
                return;
            }
        }
    }
}
