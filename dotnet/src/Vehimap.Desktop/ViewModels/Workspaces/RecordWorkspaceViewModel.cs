// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Domain.Enums;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class RecordWorkspaceViewModel : WorkspaceViewModelBase
{
    public RecordWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.RecordWindowTitle;
    public ObservableCollection<VehicleRecordItemViewModel> SelectedVehicleRecords { get; } = [];
    public ObservableCollection<VehicleRecordItemViewModel> VisibleRecordItems { get; } = [];
    public static string ManagedAttachmentModeLabel => L("Record.Projection.AttachmentMode.Managed");
    public static string ExternalAttachmentModeLabel => L("Record.Projection.AttachmentMode.External");

    [ObservableProperty]
    private string recordSummary = L("RecordWorkspace.Summary.Initial");

    [ObservableProperty]
    private VehicleRecordItemViewModel? selectedRecord;

    [ObservableProperty]
    private string recordSearchText = string.Empty;

    [ObservableProperty]
    private string recordSearchSummary = L("RecordWorkspace.SearchSummary.Initial");

    [ObservableProperty]
    private string selectedRecordSortOption = WorkspaceSortHelpers.ValiditySortLabel;

    [ObservableProperty]
    private bool recordSortDescending;

    public IReadOnlyList<string> RecordSortOptions => WorkspaceSortHelpers.RecordSortOptions;
    public IReadOnlyList<string> RecordTypeOptions => LegacyKnownValues.RecordTypes;

    public bool CanClearRecordSearch => !string.IsNullOrWhiteSpace(RecordSearchText);

    [ObservableProperty]
    private string selectedRecordDetail = L("RecordWorkspace.Detail.Empty");

    [ObservableProperty]
    private string recordPanelHeading = L("RecordWorkspace.PanelHeading");

    [ObservableProperty]
    private string recordEditorHeading = L("RecordEditor.NewTitle");

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
    private string selectedRecordEditorAttachmentMode = ManagedAttachmentModeLabel;

    [ObservableProperty]
    private string recordEditorPathInput = string.Empty;

    [ObservableProperty]
    private string recordEditorStoredPath = string.Empty;

    [ObservableProperty]
    private string recordEditorResolvedPath = string.Empty;

    [ObservableProperty]
    private string recordEditorAvailability = L("RecordEditor.AttachmentAvailability.SelectOrEnterPath");

    [ObservableProperty]
    private string recordEditorNote = string.Empty;

    public IReadOnlyList<string> RecordAttachmentModes => [ManagedAttachmentModeLabel, ExternalAttachmentModeLabel];
    public bool IsRecordDetailVisible => !IsEditingRecord;
    public bool IsRecordEditorManaged => IsManagedAttachmentModeLabel(SelectedRecordEditorAttachmentMode);
    public string RecordEditorPathInputLabel => IsRecordEditorManaged
        ? L("RecordEditor.ManagedSourceLabel")
        : L("RecordEditor.ExternalPathLabel");

    public string RecordEditorPathInputHelp => IsRecordEditorManaged
        ? L("RecordEditor.ManagedSourceHelp")
        : L("RecordEditor.ExternalPathHelp");
    public string RecordEditorStoredPathAccessibleName => BuildPathAccessibleName(L("RecordEditor.StoredPathAccessibleLabel"), RecordEditorStoredPath);
    public string RecordEditorResolvedPathAccessibleName => BuildPathAccessibleName(L("RecordEditor.ResolvedPathAccessibleLabel"), RecordEditorResolvedPath);

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

    internal static string GetAttachmentModeLabel(VehicleRecordAttachmentMode mode) =>
        mode == VehicleRecordAttachmentMode.Managed ? ManagedAttachmentModeLabel : ExternalAttachmentModeLabel;

    internal static bool IsManagedAttachmentModeLabel(string? value) =>
        MatchesAttachmentModeLabel(value, ManagedAttachmentModeLabel, "Spravovaná kopie", "Managed copy", "managed");

    internal static bool IsExternalAttachmentModeLabel(string? value) =>
        MatchesAttachmentModeLabel(value, ExternalAttachmentModeLabel, "Externí cesta", "External path", "external");

    private static bool MatchesAttachmentModeLabel(string? value, params string[] candidates)
    {
        var normalized = (value ?? string.Empty).Trim();
        return candidates.Any(candidate => string.Equals(normalized, candidate, StringComparison.OrdinalIgnoreCase));
    }

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
            SelectedRecordDetail = L("RecordWorkspace.Detail.Empty");
            Root.NotifyRecordWorkspaceSelectionChanged();
        }

        UpdateSearchSummary();
    }

    partial void OnSelectedRecordChanged(VehicleRecordItemViewModel? value)
    {
        SelectedRecordDetail = value is null
            ? L("RecordWorkspace.Detail.Empty")
            : string.Join(
                Environment.NewLine,
                LF("RecordWorkspace.Detail.Type", value.RecordType),
                LF("RecordWorkspace.Detail.Validity", value.Validity),
                LF("RecordWorkspace.Detail.Price", value.Price),
                LF("RecordWorkspace.Detail.AttachmentMode", value.AttachmentMode),
                LF("RecordWorkspace.Detail.AttachmentState", value.AttachmentState),
                LF("RecordWorkspace.Detail.StoredPath", Root.FormatWorkspaceValue(value.StoredPath, L("Common.EmptyValue"))),
                LF("RecordWorkspace.Detail.ResolvedPath", Root.FormatWorkspaceValue(value.ResolvedPath, L("Common.EmptyValue"))),
                LF("RecordWorkspace.Detail.Note", Root.FormatWorkspaceValue(value.Note, L("Common.NoNote"))));

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
        if (value)
        {
            RecordEditorHeading = Root.GetEditingRecordId() is null
                ? L("RecordEditor.NewTitle")
                : L("RecordEditor.EditTitle");
        }

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

    partial void OnRecordEditorStoredPathChanged(string value)
    {
        OnPropertyChanged(nameof(RecordEditorStoredPathAccessibleName));
    }

    partial void OnRecordEditorResolvedPathChanged(string value)
    {
        OnPropertyChanged(nameof(RecordEditorResolvedPathAccessibleName));
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
            RecordSearchSummary = LF("RecordWorkspace.SearchSummary.All", VisibleRecordItems.Count);
            return;
        }

        RecordSearchSummary = VisibleRecordItems.Count == 0
            ? LF("RecordWorkspace.SearchSummary.Empty", RecordSearchText.Trim())
            : LF("RecordWorkspace.SearchSummary.Filtered", RecordSearchText.Trim(), VisibleRecordItems.Count);
    }

    private static bool Contains(string value, string query) =>
        value.Contains(query, StringComparison.CurrentCultureIgnoreCase);

    private static string BuildPathAccessibleName(string label, string value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Format(CultureInfo.CurrentCulture, "{0}: {1}", label, L("Common.EmptyValue"))
            : $"{label}: {value}";
}
