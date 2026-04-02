using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;

namespace Vehimap.Desktop.Views;

public partial class VehicleDetailWindow : Window
{
    private MainWindowViewModel? _viewModel;

    public VehicleDetailWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Opened += OnOpened;
        Closed += OnClosed;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_viewModel?.IsEditingVehicle == true)
            {
                RequestFocus(DesktopFocusTarget.VehicleEditorName);
                return;
            }

            Focus(NavigationMethod.Unspecified, KeyModifiers.None);
        }, DispatcherPriority.Loaded);
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
        if (target == DesktopFocusTarget.VehicleEditorName)
        {
            RequestFocus(target);
        }
    }

    private void RequestFocus(DesktopFocusTarget target)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ResolveFocusTarget(target) is Control control)
            {
                control.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
            }
        }, DispatcherPriority.Background);
    }

    private Control? ResolveFocusTarget(DesktopFocusTarget target) =>
        target == DesktopFocusTarget.VehicleEditorName
            ? this.FindControl<TextBox>("VehicleDetailNameBox")
            : null;

    private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
