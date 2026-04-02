using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class HistoryWorkspaceViewModel : WorkspaceViewModelBase
{
    public HistoryWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.HistoryWindowTitle;
    public string HistorySummary => Root.HistorySummary;
    public ObservableCollection<VehicleHistoryItemViewModel> SelectedVehicleHistory => Root.SelectedVehicleHistory;

    [ObservableProperty]
    private VehicleHistoryItemViewModel? selectedHistory;

    [ObservableProperty]
    private string selectedHistoryDetail = "Vyberte historický záznam a zobrazí se detail položky.";

    [ObservableProperty]
    private string historyPanelHeading = "Detail historie";

    [ObservableProperty]
    private bool isEditingHistory;

    [ObservableProperty]
    private string historyEditorStatus = string.Empty;

    [ObservableProperty]
    private string historyEditorDate = string.Empty;

    [ObservableProperty]
    private string historyEditorType = string.Empty;

    [ObservableProperty]
    private string historyEditorOdometer = string.Empty;

    [ObservableProperty]
    private string historyEditorCost = string.Empty;

    [ObservableProperty]
    private string historyEditorNote = string.Empty;

    public bool IsHistoryDetailVisible => !IsEditingHistory;

    public ICommand CreateHistoryCommand => Root.CreateHistoryCommand;
    public ICommand EditSelectedHistoryCommand => Root.EditSelectedHistoryCommand;
    public ICommand DeleteSelectedHistoryCommand => Root.DeleteSelectedHistoryCommand;
    public ICommand SaveHistoryCommand => Root.SaveHistoryCommand;
    public ICommand CancelHistoryEditCommand => Root.CancelHistoryEditCommand;

    partial void OnSelectedHistoryChanged(VehicleHistoryItemViewModel? value)
    {
        SelectedHistoryDetail = value is null
            ? "Vyberte historický záznam a zobrazí se detail položky."
            : $"Datum: {value.Date}\nTyp události: {value.EventType}\nTachometr: {value.Odometer}\nCena: {value.Cost}\nPoznámka: {Root.FormatWorkspaceValue(value.Note, "bez poznámky")}";

        Root.NotifyHistoryWorkspaceSelectionChanged();
    }

    partial void OnIsEditingHistoryChanged(bool value)
    {
        HistoryPanelHeading = value
            ? (Root.GetEditingHistoryId() is null ? "Nový záznam historie" : "Upravit historii")
            : "Detail historie";

        OnPropertyChanged(nameof(IsHistoryDetailVisible));
        Root.NotifyHistoryWorkspaceEditingChanged();
    }
}
