// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed partial class MaintenanceWorkspaceViewModel : WorkspaceViewModelBase
{
    private static string CustomMaintenanceTemplateLabel => L("MaintenanceEditor.CustomTemplate");
    private bool _suppressMaintenanceTemplateApply;

    public MaintenanceWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
        MaintenanceTemplateOptions =
        [
            CustomMaintenanceTemplateLabel,
            .. VehicleStarterBundleService.GetMaintenanceTemplateCatalog(DesktopLocalization.Localizer).Select(VehicleStarterBundleService.BuildMaintenanceTemplateDisplayName)
        ];
    }

    public string WindowTitle => Root.MaintenanceWindowTitle;
    public ObservableCollection<VehicleMaintenanceItemViewModel> SelectedVehicleMaintenance { get; } = [];
    public ObservableCollection<VehicleMaintenanceItemViewModel> VisibleMaintenanceItems { get; } = [];
    public bool CanOpenMaintenanceRecommendations => Root.CanOpenMaintenanceRecommendations;
    public bool CanCompleteSelectedMaintenance => Root.CanCompleteSelectedMaintenance;
    public IReadOnlyList<string> MaintenanceTemplateOptions { get; }

    public event EventHandler? MaintenanceTemplatesRequested;

    public event EventHandler? MaintenanceCompletionRequested;

    [ObservableProperty]
    private string maintenanceSummary = L("MaintenanceWorkspace.Summary.Initial");

    [ObservableProperty]
    private VehicleMaintenanceItemViewModel? selectedMaintenance;

    [ObservableProperty]
    private string maintenanceSearchText = string.Empty;

    [ObservableProperty]
    private string maintenanceSearchSummary = L("MaintenanceWorkspace.SearchSummary.Initial");

    [ObservableProperty]
    private string selectedMaintenanceSortOption = WorkspaceSortHelpers.TitleSortLabel;

    [ObservableProperty]
    private bool maintenanceSortDescending;

    public IReadOnlyList<string> MaintenanceSortOptions => WorkspaceSortHelpers.MaintenanceSortOptions;

    public bool CanClearMaintenanceSearch => !string.IsNullOrWhiteSpace(MaintenanceSearchText);

    [ObservableProperty]
    private string selectedMaintenanceDetail = L("MaintenanceWorkspace.Detail.Empty");

    [ObservableProperty]
    private string maintenancePanelHeading = L("MaintenanceWorkspace.PanelHeading");

    [ObservableProperty]
    private string maintenanceEditorHeading = L("MaintenanceEditor.NewTitle");

    [ObservableProperty]
    private bool isEditingMaintenance;

    [ObservableProperty]
    private string maintenanceEditorStatus = string.Empty;

    [ObservableProperty]
    private string selectedMaintenanceTemplate = CustomMaintenanceTemplateLabel;

    [ObservableProperty]
    private string maintenanceEditorTitle = string.Empty;

    [ObservableProperty]
    private string maintenanceEditorIntervalKm = string.Empty;

    public string MaintenanceEditorIntervalDistanceLabel => LF("MaintenanceEditor.IntervalDistanceLabel", Root.CurrentDistanceUnitLabel);

    public string MaintenanceEditorIntervalDistanceName => LF("MaintenanceEditor.IntervalDistanceName", Root.CurrentDistanceUnitLabel);

    public string MaintenanceEditorIntervalDistanceHelp => LF("MaintenanceEditor.IntervalDistanceHelp", Root.CurrentDistanceUnitLabel);

    [ObservableProperty]
    private string maintenanceEditorIntervalMonths = string.Empty;

    [ObservableProperty]
    private string maintenanceEditorLastServiceDate = string.Empty;

    [ObservableProperty]
    private string maintenanceEditorLastServiceOdometer = string.Empty;

    public string MaintenanceEditorLastServiceOdometerLabel => LF("MaintenanceEditor.LastServiceOdometerLabel", Root.CurrentDistanceUnitLabel);

    public string MaintenanceEditorLastServiceOdometerName => LF("MaintenanceEditor.LastServiceOdometerName", Root.CurrentDistanceUnitLabel);

    public string MaintenanceEditorLastServiceOdometerHelp => LF("MaintenanceEditor.LastServiceOdometerHelp", Root.CurrentDistanceUnitLabel);

    [ObservableProperty]
    private bool maintenanceEditorIsActive = true;

    [ObservableProperty]
    private string maintenanceEditorNote = string.Empty;

    public bool IsMaintenanceDetailVisible => !IsEditingMaintenance;

    public ICommand CreateMaintenanceCommand => Root.CreateMaintenanceCommand;
    public ICommand EditSelectedMaintenanceCommand => Root.EditSelectedMaintenanceCommand;
    public ICommand DeleteSelectedMaintenanceCommand => Root.DeleteSelectedMaintenanceCommand;
    public ICommand SaveMaintenanceCommand => Root.SaveMaintenanceCommand;
    public ICommand CancelMaintenanceEditCommand => Root.CancelMaintenanceEditCommand;

    [RelayCommand]
    private void FocusSearch()
    {
        RequestFocus(DesktopFocusTarget.MaintenanceSearch);
    }

    [RelayCommand(CanExecute = nameof(CanClearMaintenanceSearch))]
    private void ClearMaintenanceSearch()
    {
        MaintenanceSearchText = string.Empty;
        RequestFocus(DesktopFocusTarget.MaintenanceSearch);
    }

    public VehicleStarterBundlePreview BuildMaintenanceTemplatePreview()
    {
        return Root.SelectedVehicle is null
            ? new VehicleStarterBundlePreview(string.Empty, string.Empty, string.Empty, [])
            : Root.BuildMaintenanceTemplatePreview(Root.SelectedVehicle.Id);
    }

    public Task<string> ApplyMaintenanceTemplatesAsync(IReadOnlyList<VehicleStarterBundleTemplate> items)
    {
        return Root.SelectedVehicle is null
            ? Task.FromResult(L("MaintenanceWorkspace.Status.SelectVehicleFirst"))
            : Root.ApplyMaintenanceTemplatesAsync(Root.SelectedVehicle.Id, items);
    }

    public MaintenanceCompletionDialogViewModel? BuildMaintenanceCompletionDialogViewModel()
    {
        return Root.BuildMaintenanceCompletionDialogViewModel();
    }

    public Task<string> ApplyMaintenanceCompletionAsync(MaintenanceCompletionDialogResult result)
    {
        return Root.ApplyMaintenanceCompletionAsync(result);
    }

    public void SetMaintenanceTemplateStatus(string message)
    {
        MaintenanceEditorStatus = message;
    }

    public void SetMaintenanceStatus(string message)
    {
        MaintenanceEditorStatus = message;
    }

    public void ResetMaintenanceTemplateSelection()
    {
        _suppressMaintenanceTemplateApply = true;
        SelectedMaintenanceTemplate = CustomMaintenanceTemplateLabel;
        _suppressMaintenanceTemplateApply = false;
    }

    internal void NotifyMaintenanceRecommendationStateChanged()
    {
        OnPropertyChanged(nameof(CanOpenMaintenanceRecommendations));
        OpenMaintenanceTemplatesCommand.NotifyCanExecuteChanged();
    }

    internal void NotifyMaintenanceCompletionStateChanged()
    {
        OnPropertyChanged(nameof(CanCompleteSelectedMaintenance));
        CompleteSelectedMaintenanceCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanOpenMaintenanceRecommendations))]
    private void OpenMaintenanceTemplates()
    {
        MaintenanceTemplatesRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand(CanExecute = nameof(CanCompleteSelectedMaintenance))]
    private void CompleteSelectedMaintenance()
    {
        MaintenanceCompletionRequested?.Invoke(this, EventArgs.Empty);
    }

    public void RefreshVisibleMaintenanceItems(bool preserveSelection = true)
    {
        var previousSelection = preserveSelection ? SelectedMaintenance : null;
        var filteredItems = WorkspaceSortHelpers
            .SortMaintenance(SelectedVehicleMaintenance.Where(MatchesSearch), SelectedMaintenanceSortOption, MaintenanceSortDescending)
            .ToList();

        VisibleMaintenanceItems.Clear();
        foreach (var item in filteredItems)
        {
            VisibleMaintenanceItems.Add(item);
        }

        SelectedMaintenance = previousSelection is not null
            ? VisibleMaintenanceItems.FirstOrDefault(item => string.Equals(item.Id, previousSelection.Id, StringComparison.Ordinal))
            : null;

        SelectedMaintenance ??= VisibleMaintenanceItems.FirstOrDefault();
        if (SelectedMaintenance is null)
        {
            SelectedMaintenanceDetail = L("MaintenanceWorkspace.Detail.Empty");
            Root.NotifyMaintenanceWorkspaceSelectionChanged();
        }

        UpdateSearchSummary();
    }

    partial void OnSelectedMaintenanceChanged(VehicleMaintenanceItemViewModel? value)
    {
        SelectedMaintenanceDetail = value is null
            ? L("MaintenanceWorkspace.Detail.Empty")
            : string.Join(
                Environment.NewLine,
                LF("MaintenanceWorkspace.Detail.Task", value.Title),
                LF("MaintenanceWorkspace.Detail.Interval", value.Interval),
                LF("MaintenanceWorkspace.Detail.LastService", value.LastService),
                LF("MaintenanceWorkspace.Detail.Status", value.Status),
                LF("MaintenanceWorkspace.Detail.Note", Root.FormatWorkspaceValue(value.Note, L("Common.NoNote"))));

        Root.NotifyMaintenanceWorkspaceSelectionChanged();
    }

    partial void OnMaintenanceSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(CanClearMaintenanceSearch));
        ClearMaintenanceSearchCommand.NotifyCanExecuteChanged();
        RefreshVisibleMaintenanceItems();
    }

    partial void OnSelectedMaintenanceSortOptionChanged(string value)
    {
        Root.HandleMaintenanceWorkspaceSortChanged();
    }

    partial void OnMaintenanceSortDescendingChanged(bool value)
    {
        Root.HandleMaintenanceWorkspaceSortChanged();
    }

    partial void OnSelectedMaintenanceTemplateChanged(string value)
    {
        if (_suppressMaintenanceTemplateApply || !IsEditingMaintenance)
        {
            return;
        }

        var template = VehicleStarterBundleService.FindMaintenanceTemplateByDisplayName(value, DesktopLocalization.Localizer);
        if (template is null)
        {
            return;
        }

        MaintenanceEditorTitle = template.Title;
        MaintenanceEditorIntervalKm = Root.FormatCanonicalDistanceForEditor(template.IntervalKm);
        MaintenanceEditorIntervalMonths = template.IntervalMonths;
        MaintenanceEditorNote = template.Note;
        var categoryText = string.IsNullOrWhiteSpace(template.Category)
            ? string.Empty
            : LF("MaintenanceEditor.TemplateCategorySuffix", template.Category);
        MaintenanceEditorStatus = LF("MaintenanceEditor.TemplateApplied", template.Title, categoryText);
        RequestFocus(DesktopFocusTarget.MaintenanceEditorTitle);
    }

    partial void OnIsEditingMaintenanceChanged(bool value)
    {
        if (value)
        {
            MaintenanceEditorHeading = Root.GetEditingMaintenanceId() is null
                ? L("MaintenanceEditor.NewTitle")
                : L("MaintenanceEditor.EditTitle");
            NotifyUnitMetadataChanged();
        }

        if (!value)
        {
            ResetMaintenanceTemplateSelection();
        }

        OnPropertyChanged(nameof(IsMaintenanceDetailVisible));
        Root.NotifyMaintenanceWorkspaceEditingChanged();
    }

    private bool MatchesSearch(VehicleMaintenanceItemViewModel item)
    {
        if (string.IsNullOrWhiteSpace(MaintenanceSearchText))
        {
            return true;
        }

        var query = MaintenanceSearchText.Trim();
        return Contains(item.Title, query)
            || Contains(item.Interval, query)
            || Contains(item.LastService, query)
            || Contains(item.Status, query)
            || Contains(item.Note, query)
            || Contains(item.AccessibleLabel, query);
    }

    private void UpdateSearchSummary()
    {
        if (string.IsNullOrWhiteSpace(MaintenanceSearchText))
        {
            MaintenanceSearchSummary = LF("MaintenanceWorkspace.SearchSummary.All", VisibleMaintenanceItems.Count);
            return;
        }

        MaintenanceSearchSummary = VisibleMaintenanceItems.Count == 0
            ? LF("MaintenanceWorkspace.SearchSummary.Empty", MaintenanceSearchText.Trim())
            : LF("MaintenanceWorkspace.SearchSummary.Filtered", MaintenanceSearchText.Trim(), VisibleMaintenanceItems.Count);
    }

    private static bool Contains(string value, string query) =>
        value.Contains(query, StringComparison.CurrentCultureIgnoreCase);

    internal void NotifyUnitMetadataChanged()
    {
        OnPropertyChanged(nameof(MaintenanceEditorIntervalDistanceLabel));
        OnPropertyChanged(nameof(MaintenanceEditorIntervalDistanceName));
        OnPropertyChanged(nameof(MaintenanceEditorIntervalDistanceHelp));
        OnPropertyChanged(nameof(MaintenanceEditorLastServiceOdometerLabel));
        OnPropertyChanged(nameof(MaintenanceEditorLastServiceOdometerName));
        OnPropertyChanged(nameof(MaintenanceEditorLastServiceOdometerHelp));
    }
}
