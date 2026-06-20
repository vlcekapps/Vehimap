using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class RecordWorkspaceViewModel : WorkspaceViewModelBase
{
    public RecordWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.RecordWindowTitle;
    public ObservableCollection<VehicleRecordItemViewModel> SelectedVehicleRecords => Root.SelectedVehicleRecords;
    public ObservableCollection<VehicleRecordItemViewModel> VisibleRecordItems { get; } = [];

    [ObservableProperty]
    private string recordSummary = "Doklady a přílohy vybraného vozidla se zobrazí po výběru vozidla.";

    [ObservableProperty]
    private VehicleRecordItemViewModel? selectedRecord;

    [ObservableProperty]
    private string recordSearchText = string.Empty;

    [ObservableProperty]
    private string recordSearchSummary = "Ctrl+F přesune fokus do hledání dokladů.";

    [ObservableProperty]
    private string selectedRecordSortOption = WorkspaceSortHelpers.ValiditySortLabel;

    [ObservableProperty]
    private bool recordSortDescending;

    public IReadOnlyList<string> RecordSortOptions => WorkspaceSortHelpers.RecordSortOptions;

    public bool CanClearRecordSearch => !string.IsNullOrWhiteSpace(RecordSearchText);

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
    public ICommand CopySelectedRecordPathCommand => Root.CopySelectedRecordPathCommand;

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.RecordSearch);
    }

    [RelayCommand(CanExecute = nameof(CanClearRecordSearch))]
    private void ClearRecordSearch()
    {
        RecordSearchText = string.Empty;
        RequestFocus(DesktopFocusTarget.RecordSearch);
    }

    public void RefreshVisibleRecordItems(bool preserveSelection = true)
    {
        var previousSelection = preserveSelection ? SelectedRecord : null;
        var filteredItems = WorkspaceSortHelpers
            .SortRecords(SelectedVehicleRecords.Where(MatchesSearch), SelectedRecordSortOption, RecordSortDescending)
            .ToList();

        VisibleRecordItems.Clear();
        foreach (var item in filteredItems)
        {
            VisibleRecordItems.Add(item);
        }

        SelectedRecord = previousSelection is not null
            ? VisibleRecordItems.FirstOrDefault(item => string.Equals(item.Id, previousSelection.Id, StringComparison.Ordinal))
            : null;

        SelectedRecord ??= VisibleRecordItems.FirstOrDefault();
        if (SelectedRecord is null)
        {
            SelectedRecordDetail = "Vyberte doklad a zobrazí se detail přílohy.";
            Root.NotifyRecordWorkspaceSelectionChanged();
        }

        UpdateSearchSummary();
    }

    partial void OnSelectedRecordChanged(VehicleRecordItemViewModel? value)
    {
        SelectedRecordDetail = value is null
            ? "Vyberte doklad a zobrazí se detail přílohy."
            : $"Typ: {value.RecordType}\nPlatnost: {value.Validity}\nCena: {value.Price}\nRežim přílohy: {value.AttachmentMode}\nStav přílohy: {value.AttachmentState}\nUložená cesta: {Root.FormatWorkspaceValue(value.StoredPath, "nevyplněno")}\nVyřešená cesta: {Root.FormatWorkspaceValue(value.ResolvedPath, "nevyplněno")}\nPoznámka: {Root.FormatWorkspaceValue(value.Note, "bez poznámky")}";

        Root.NotifyRecordWorkspaceSelectionChanged();
    }

    partial void OnRecordSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanClearRecordSearch));
        ClearRecordSearchCommand.NotifyCanExecuteChanged();
        RefreshVisibleRecordItems();
    }

    partial void OnSelectedRecordSortOptionChanged(string value)
    {
        Root.HandleRecordWorkspaceSortChanged();
    }

    partial void OnRecordSortDescendingChanged(bool value)
    {
        Root.HandleRecordWorkspaceSortChanged();
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

    private bool MatchesSearch(VehicleRecordItemViewModel item)
    {
        if (string.IsNullOrWhiteSpace(RecordSearchText))
        {
            return true;
        }

        var query = RecordSearchText.Trim();
        return Contains(item.RecordType, query)
            || Contains(item.Title, query)
            || Contains(item.Provider, query)
            || Contains(item.Validity, query)
            || Contains(item.Price, query)
            || Contains(item.AttachmentMode, query)
            || Contains(item.AttachmentState, query)
            || Contains(item.StoredPath, query)
            || Contains(item.ResolvedPath, query)
            || Contains(item.Note, query)
            || Contains(item.AccessibleLabel, query);
    }

    private void UpdateSearchSummary()
    {
        if (string.IsNullOrWhiteSpace(RecordSearchText))
        {
            RecordSearchSummary = $"Zobrazeno {VisibleRecordItems.Count} dokladů. Ctrl+F přesune fokus do hledání.";
            return;
        }

        RecordSearchSummary = VisibleRecordItems.Count == 0
            ? $"Hledání „{RecordSearchText.Trim()}“ nenašlo v dokladech žádný záznam."
            : $"Hledání „{RecordSearchText.Trim()}“ našlo {VisibleRecordItems.Count} dokladů.";
    }

    private static bool Contains(string value, string query) =>
        value.Contains(query, StringComparison.CurrentCultureIgnoreCase);
}
