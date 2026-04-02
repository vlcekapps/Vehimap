using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class AuditWorkspaceViewModel : WorkspaceViewModelBase
{
    public AuditWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    [ObservableProperty]
    private string auditSummary = string.Empty;

    [ObservableProperty]
    private AuditItemViewModel? selectedDashboardAuditItem;

    public string WindowTitle => Root.AuditWindowTitle;

    public ObservableCollection<AuditItemViewModel> AuditItems => Root.AuditItems;

    public ICommand OpenSelectedDashboardAuditItemCommand => Root.OpenSelectedDashboardAuditItemCommand;

    partial void OnSelectedDashboardAuditItemChanged(AuditItemViewModel? value)
    {
        Root.NotifyAuditWorkspaceSelectionChanged();
    }
}
