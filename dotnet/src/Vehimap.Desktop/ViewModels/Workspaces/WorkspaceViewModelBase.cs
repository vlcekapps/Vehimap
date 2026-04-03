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

    internal bool HasPendingEdits => Root.HasPendingEdits;

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

    internal Task<bool> ConfirmDiscardPendingEditsAsync(string actionDescription)
    {
        return Root.ConfirmDiscardPendingEditsAsync(actionDescription);
    }

    internal void DiscardPendingEdits(bool clearStatus = true)
    {
        Root.DiscardPendingEdits(clearStatus);
    }

    private void OnRootPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(string.Empty);
    }
}
