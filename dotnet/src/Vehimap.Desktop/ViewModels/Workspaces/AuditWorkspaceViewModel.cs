using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class AuditWorkspaceViewModel : WorkspaceViewModelBase
{
    public AuditWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.AuditWindowTitle;
    public string AuditSummary => Root.AuditSummary;
    public ObservableCollection<AuditItemViewModel> AuditItems => Root.AuditItems;
    public AuditItemViewModel? SelectedDashboardAuditItem
    {
        get => Root.SelectedDashboardAuditItem;
        set => Root.SelectedDashboardAuditItem = value;
    }

    public ICommand OpenSelectedDashboardAuditItemCommand => Root.OpenSelectedDashboardAuditItemCommand;
}

