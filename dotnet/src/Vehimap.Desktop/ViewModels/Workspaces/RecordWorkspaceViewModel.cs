using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class RecordWorkspaceViewModel : WorkspaceViewModelBase
{
    public RecordWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.RecordWindowTitle;
    public string RecordSummary => Root.RecordSummary;
    public ObservableCollection<VehicleRecordItemViewModel> SelectedVehicleRecords => Root.SelectedVehicleRecords;
    public VehicleRecordItemViewModel? SelectedRecord
    {
        get => Root.SelectedRecord;
        set => Root.SelectedRecord = value;
    }

    public string SelectedRecordDetail => Root.SelectedRecordDetail;
    public string RecordPanelHeading => Root.RecordPanelHeading;
    public bool IsEditingRecord => Root.IsEditingRecord;
    public bool IsRecordDetailVisible => Root.IsRecordDetailVisible;
    public string RecordEditorStatus => Root.RecordEditorStatus;
    public string RecordEditorRecordType
    {
        get => Root.RecordEditorRecordType;
        set => Root.RecordEditorRecordType = value;
    }

    public string RecordEditorTitle
    {
        get => Root.RecordEditorTitle;
        set => Root.RecordEditorTitle = value;
    }

    public string RecordEditorProvider
    {
        get => Root.RecordEditorProvider;
        set => Root.RecordEditorProvider = value;
    }

    public string RecordEditorValidFrom
    {
        get => Root.RecordEditorValidFrom;
        set => Root.RecordEditorValidFrom = value;
    }

    public string RecordEditorValidTo
    {
        get => Root.RecordEditorValidTo;
        set => Root.RecordEditorValidTo = value;
    }

    public string RecordEditorPrice
    {
        get => Root.RecordEditorPrice;
        set => Root.RecordEditorPrice = value;
    }

    public IReadOnlyList<string> RecordAttachmentModes => Root.RecordAttachmentModes;

    public string SelectedRecordEditorAttachmentMode
    {
        get => Root.SelectedRecordEditorAttachmentMode;
        set => Root.SelectedRecordEditorAttachmentMode = value;
    }

    public string RecordEditorPathInput
    {
        get => Root.RecordEditorPathInput;
        set => Root.RecordEditorPathInput = value;
    }

    public string RecordEditorStoredPath => Root.RecordEditorStoredPath;
    public string RecordEditorResolvedPath => Root.RecordEditorResolvedPath;
    public string RecordEditorAvailability => Root.RecordEditorAvailability;
    public string RecordEditorNote
    {
        get => Root.RecordEditorNote;
        set => Root.RecordEditorNote = value;
    }

    public bool IsRecordEditorManaged => Root.IsRecordEditorManaged;
    public string RecordEditorPathInputLabel => Root.RecordEditorPathInputLabel;
    public string RecordEditorPathInputHelp => Root.RecordEditorPathInputHelp;

    public ICommand CreateRecordCommand => Root.CreateRecordCommand;
    public ICommand EditSelectedRecordCommand => Root.EditSelectedRecordCommand;
    public ICommand DeleteSelectedRecordCommand => Root.DeleteSelectedRecordCommand;
    public ICommand SaveRecordCommand => Root.SaveRecordCommand;
    public ICommand CancelRecordEditCommand => Root.CancelRecordEditCommand;
    public ICommand BrowseRecordAttachmentCommand => Root.BrowseRecordAttachmentCommand;
    public ICommand MoveSelectedRecordToManagedCommand => Root.MoveSelectedRecordToManagedCommand;
    public ICommand OpenSelectedRecordFileCommand => Root.OpenSelectedRecordFileCommand;
    public ICommand OpenSelectedRecordFolderCommand => Root.OpenSelectedRecordFolderCommand;
}

