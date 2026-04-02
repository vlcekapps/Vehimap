using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class HistoryWorkspaceViewModel : WorkspaceViewModelBase
{
    public HistoryWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.HistoryWindowTitle;
    public string HistorySummary => Root.HistorySummary;
    public ObservableCollection<VehicleHistoryItemViewModel> SelectedVehicleHistory => Root.SelectedVehicleHistory;
    public VehicleHistoryItemViewModel? SelectedHistory
    {
        get => Root.SelectedHistory;
        set => Root.SelectedHistory = value;
    }

    public string SelectedHistoryDetail => Root.SelectedHistoryDetail;
    public string HistoryPanelHeading => Root.HistoryPanelHeading;
    public bool IsEditingHistory => Root.IsEditingHistory;
    public bool IsHistoryDetailVisible => Root.IsHistoryDetailVisible;
    public string HistoryEditorStatus => Root.HistoryEditorStatus;
    public string HistoryEditorDate
    {
        get => Root.HistoryEditorDate;
        set => Root.HistoryEditorDate = value;
    }

    public string HistoryEditorType
    {
        get => Root.HistoryEditorType;
        set => Root.HistoryEditorType = value;
    }

    public string HistoryEditorOdometer
    {
        get => Root.HistoryEditorOdometer;
        set => Root.HistoryEditorOdometer = value;
    }

    public string HistoryEditorCost
    {
        get => Root.HistoryEditorCost;
        set => Root.HistoryEditorCost = value;
    }

    public string HistoryEditorNote
    {
        get => Root.HistoryEditorNote;
        set => Root.HistoryEditorNote = value;
    }

    public ICommand CreateHistoryCommand => Root.CreateHistoryCommand;
    public ICommand EditSelectedHistoryCommand => Root.EditSelectedHistoryCommand;
    public ICommand DeleteSelectedHistoryCommand => Root.DeleteSelectedHistoryCommand;
    public ICommand SaveHistoryCommand => Root.SaveHistoryCommand;
    public ICommand CancelHistoryEditCommand => Root.CancelHistoryEditCommand;
}

