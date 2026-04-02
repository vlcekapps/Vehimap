using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.ViewModels.Workspaces;

namespace Vehimap.Desktop.Views.Workspaces;

public abstract class WorkspaceViewBase<TViewModel> : UserControl
    where TViewModel : WorkspaceViewModelBase
{
    private TViewModel? _viewModel;

    protected TViewModel? ViewModel => _viewModel;

    protected WorkspaceViewBase()
    {
        DataContextChanged += OnDataContextChanged;
    }

    public void FocusDefaultControl()
    {
        var target = GetDefaultFocusTarget();
        if (target is not null)
        {
            RequestFocus(target.Value);
        }
    }

    protected abstract DesktopFocusTarget? GetDefaultFocusTarget();

    protected abstract bool SupportsFocusTarget(DesktopFocusTarget target);

    protected abstract Control? ResolveFocusTarget(DesktopFocusTarget target);

    protected void RegisterShiftTabBackNavigation(params string[] controlNames)
    {
        foreach (var controlName in controlNames)
        {
            if (this.FindControl<Control>(controlName) is { } control)
            {
                control.AddHandler(InputElement.KeyDownEvent, OnTabBoundaryKeyDown, RoutingStrategies.Tunnel);
            }
        }
    }

    protected void RequestFocus(DesktopFocusTarget target)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ResolveFocusTarget(target) is not { } control)
            {
                return;
            }

            if (control is ListBox listBox)
            {
                WorkspaceFocusHelpers.FocusListBox(listBox);
                return;
            }

            control.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
        }, DispatcherPriority.Background);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.FocusRequested -= OnFocusRequested;
        }

        _viewModel = DataContext as TViewModel;
        if (_viewModel is not null)
        {
            _viewModel.FocusRequested += OnFocusRequested;
        }
    }

    private void OnFocusRequested(DesktopFocusTarget target)
    {
        if (SupportsFocusTarget(target))
        {
            RequestFocus(target);
        }
    }

    private void OnTabBoundaryKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab || !e.KeyModifiers.HasFlag(KeyModifiers.Shift) || _viewModel is null)
        {
            return;
        }

        _viewModel.RequestWorkspaceFocus(DesktopFocusTarget.SelectedVehicleTabHeader);
        e.Handled = true;
    }
}
