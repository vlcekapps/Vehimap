using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class RecordWorkspaceViewModel : WorkspaceViewModelBase
{
    public RecordWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.RecordWindowTitle;
    public string RecordSummary => Root.RecordSummary;
    public ObservableCollection<VehicleRecordItemViewModel> SelectedVehicleRecords => Root.SelectedVehicleRecords;

    [ObservableProperty]
    private VehicleRecordItemViewModel? selectedRecord;

    [ObservableProperty]
    private string selectedRecordDetail = "Vyberte doklad a zobrazí se detail přílohy.";

    [ObservableProperty]
    private string recordPanelHeading = "Detail dokladu";

    [ObservableProperty]
    private bool isEditingRecord;

    [ObservableProperty]
    private string recordEditorStatus = string.Empty;

    [ObservableProperty]
    private string recordEditorRecordType = string.Empty;

    [ObservableProperty]
    private string recordEditorTitle = string.Empty;

    [ObservableProperty]
    private string recordEditorProvider = string.Empty;

    [ObservableProperty]
    private string recordEditorValidFrom = string.Empty;

    [ObservableProperty]
    private string recordEditorValidTo = string.Empty;

    [ObservableProperty]
    private string recordEditorPrice = string.Empty;

    [ObservableProperty]
    private string selectedRecordEditorAttachmentMode = "Spravovaná kopie";

    [ObservableProperty]
    private string recordEditorPathInput = string.Empty;

    [ObservableProperty]
    private string recordEditorStoredPath = string.Empty;

    [ObservableProperty]
    private string recordEditorResolvedPath = string.Empty;

    [ObservableProperty]
    private string recordEditorAvailability = "Vyberte soubor nebo zadejte cestu přílohy.";

    [ObservableProperty]
    private string recordEditorNote = string.Empty;

    public IReadOnlyList<string> RecordAttachmentModes { get; } = ["Spravovaná kopie", "Externí cesta"];
    public bool IsRecordDetailVisible => !IsEditingRecord;
    public bool IsRecordEditorManaged => string.Equals(SelectedRecordEditorAttachmentMode, "Spravovaná kopie", StringComparison.CurrentCulture);
    public string RecordEditorPathInputLabel => IsRecordEditorManaged ? "Zdroj souboru pro import" : "Externí cesta k souboru";
    public string RecordEditorPathInputHelp => IsRecordEditorManaged
        ? "Vybraný soubor se po uložení zkopíruje do spravovaných příloh."
        : "Zadejte nebo vyberte externí cestu, která se nebude kopírovat.";

    public ICommand CreateRecordCommand => Root.CreateRecordCommand;
    public ICommand EditSelectedRecordCommand => Root.EditSelectedRecordCommand;
    public ICommand DeleteSelectedRecordCommand => Root.DeleteSelectedRecordCommand;
    public ICommand SaveRecordCommand => Root.SaveRecordCommand;
    public ICommand CancelRecordEditCommand => Root.CancelRecordEditCommand;
    public ICommand BrowseRecordAttachmentCommand => Root.BrowseRecordAttachmentCommand;
    public ICommand MoveSelectedRecordToManagedCommand => Root.MoveSelectedRecordToManagedCommand;
    public ICommand OpenSelectedRecordFileCommand => Root.OpenSelectedRecordFileCommand;
    public ICommand OpenSelectedRecordFolderCommand => Root.OpenSelectedRecordFolderCommand;

    partial void OnSelectedRecordChanged(VehicleRecordItemViewModel? value)
    {
        SelectedRecordDetail = value is null
            ? "Vyberte doklad a zobrazí se detail přílohy."
            : $"Typ: {value.RecordType}\nPlatnost: {value.Validity}\nCena: {value.Price}\nRežim přílohy: {value.AttachmentMode}\nStav přílohy: {value.AttachmentState}\nUložená cesta: {Root.FormatWorkspaceValue(value.StoredPath, "nevyplněno")}\nVyřešená cesta: {Root.FormatWorkspaceValue(value.ResolvedPath, "nevyplněno")}\nPoznámka: {Root.FormatWorkspaceValue(value.Note, "bez poznámky")}";

        Root.NotifyRecordWorkspaceSelectionChanged();
    }

    partial void OnIsEditingRecordChanged(bool value)
    {
        RecordPanelHeading = value
            ? (Root.GetEditingRecordId() is null ? "Nový doklad" : "Upravit doklad")
            : "Detail dokladu";

        OnPropertyChanged(nameof(IsRecordDetailVisible));
        Root.NotifyRecordWorkspaceEditingChanged();
    }

    partial void OnSelectedRecordEditorAttachmentModeChanged(string value)
    {
        Root.HandleRecordAttachmentModeChanged();
        OnPropertyChanged(nameof(IsRecordEditorManaged));
        OnPropertyChanged(nameof(RecordEditorPathInputLabel));
        OnPropertyChanged(nameof(RecordEditorPathInputHelp));
    }

    partial void OnRecordEditorPathInputChanged(string value)
    {
        Root.HandleRecordAttachmentPathChanged();
    }
}
