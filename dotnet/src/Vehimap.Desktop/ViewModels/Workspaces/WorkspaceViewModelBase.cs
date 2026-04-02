using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public abstract class WorkspaceViewModelBase : ObservableObject, IDisposable
{
    protected WorkspaceViewModelBase(MainWindowViewModel root)
    {
        Root = root;
        Root.PropertyChanged += OnRootPropertyChanged;
    }

    protected MainWindowViewModel Root { get; }

    public event Action<DesktopFocusTarget>? FocusRequested
    {
        add => Root.FocusRequested += value;
        remove => Root.FocusRequested -= value;
    }

    public virtual void Dispose()
    {
        Root.PropertyChanged -= OnRootPropertyChanged;
    }

    protected void RequestFocus(DesktopFocusTarget target)
    {
        Root.RequestWorkspaceFocus(target);
    }

    public void RequestWorkspaceFocus(DesktopFocusTarget target)
    {
        RequestFocus(target);
    }

    private void OnRootPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(string.Empty);
    }
}
