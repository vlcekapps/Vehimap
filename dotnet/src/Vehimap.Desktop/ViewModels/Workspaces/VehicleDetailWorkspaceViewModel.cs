// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;
using Vehimap.Desktop.ViewModels;
using Vehimap.Desktop.Services;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class VehicleDetailWorkspaceViewModel : WorkspaceViewModelBase
{
    private bool isEditingVehicle;
    private string vehiclePanelHeading = L("VehicleDetail.PanelHeading");
    private string vehicleEditorHeading = L("VehicleEditor.Title.Default");
    private string selectedVehicleHeading = L("VehicleDetail.Projection.EmptyHeading");
    private string selectedVehicleOverview = L("VehicleDetail.Projection.EmptyOverview");
    private string selectedVehicleDates = string.Empty;
    private string selectedVehicleProfile = string.Empty;
    private string selectedVehicleEvidenceSummary = L("VehicleDetail.Projection.EmptyEvidence");
    private string selectedVehicleRecentHistorySummary = L("VehicleDetail.Projection.EmptyRecentHistory");
    private string vehicleEditorStatus = string.Empty;
    private string vehicleEditorName = string.Empty;
    private string vehicleEditorCategory = string.Empty;
    private string vehicleEditorNote = string.Empty;
    private string vehicleEditorMakeModel = string.Empty;
    private string vehicleEditorPlate = string.Empty;
    private string vehicleEditorYear = string.Empty;
    private string vehicleEditorPower = string.Empty;
    private string vehicleEditorLastTk = string.Empty;
    private string vehicleEditorNextTk = string.Empty;
    private string vehicleEditorGreenCardFrom = string.Empty;
    private string vehicleEditorGreenCardTo = string.Empty;
    private string vehicleEditorState = string.Empty;
    private string vehicleEditorTags = string.Empty;
    private string vehicleEditorPowertrain = string.Empty;
    private string vehicleEditorClimateProfile = string.Empty;
    private string vehicleEditorTimingDrive = string.Empty;
    private string vehicleEditorTransmission = string.Empty;

    public VehicleDetailWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.VehicleDetailWindowTitle;

    internal AppCulturePreferences CurrentCulturePreferences => Root.CurrentCulturePreferences;

    internal AppUnitPreferences CurrentUnitPreferences => Root.CurrentUnitPreferences;

    public string VehiclePanelHeading
    {
        get => vehiclePanelHeading;
        private set => SetProperty(ref vehiclePanelHeading, value);
    }

    public string VehicleEditorHeading
    {
        get => vehicleEditorHeading;
        private set => SetProperty(ref vehicleEditorHeading, value);
    }

    public string SelectedVehicleHeading
    {
        get => selectedVehicleHeading;
        set => SetProperty(ref selectedVehicleHeading, value);
    }

    public string SelectedVehicleOverview
    {
        get => selectedVehicleOverview;
        set => SetProperty(ref selectedVehicleOverview, value);
    }

    public string SelectedVehicleDates
    {
        get => selectedVehicleDates;
        set => SetProperty(ref selectedVehicleDates, value);
    }

    public string SelectedVehicleProfile
    {
        get => selectedVehicleProfile;
        set => SetProperty(ref selectedVehicleProfile, value);
    }

    public string SelectedVehicleEvidenceSummary
    {
        get => selectedVehicleEvidenceSummary;
        set => SetProperty(ref selectedVehicleEvidenceSummary, value);
    }

    public string SelectedVehicleRecentHistorySummary
    {
        get => selectedVehicleRecentHistorySummary;
        set => SetProperty(ref selectedVehicleRecentHistorySummary, value);
    }

    public ObservableCollection<VehicleDetailEvidenceSummaryItemViewModel> EvidenceSummaryItems { get; } = [];

    public ObservableCollection<VehicleHistoryItemViewModel> RecentHistoryItems { get; } = [];

    public bool IsEditingVehicle
    {
        get => isEditingVehicle;
        private set
        {
            if (SetProperty(ref isEditingVehicle, value))
            {
                OnPropertyChanged(nameof(IsVehicleDetailVisible));
            }
        }
    }

    public bool IsVehicleDetailVisible => true;

    public string VehicleEditorStatus
    {
        get => vehicleEditorStatus;
        set => SetProperty(ref vehicleEditorStatus, value);
    }

    public IReadOnlyList<LocalizedOptionViewModel> VehicleCategoryOptions => KnownValueOptions.VehicleCategories(VehicleEditorCategory);
    public IReadOnlyList<LocalizedOptionViewModel> VehicleStateOptions => KnownValueOptions.VehicleStates(VehicleEditorState);
    public IReadOnlyList<LocalizedOptionViewModel> VehiclePowertrainOptions => KnownValueOptions.VehiclePowertrains(VehicleEditorPowertrain);
    public IReadOnlyList<LocalizedOptionViewModel> VehicleClimateProfileOptions => KnownValueOptions.VehicleClimateProfiles(VehicleEditorClimateProfile);
    public IReadOnlyList<LocalizedOptionViewModel> VehicleTimingDriveOptions => KnownValueOptions.VehicleTimingDrives(VehicleEditorTimingDrive);
    public IReadOnlyList<LocalizedOptionViewModel> VehicleTransmissionOptions => KnownValueOptions.VehicleTransmissions(VehicleEditorTransmission);
    public bool CanOpenVehicleStarterBundle => Root.CanOpenVehicleStarterBundle;
    public bool CanOpenVehicleRelatedWorkspace => Root.SelectedVehicle is not null && Root.CanUseWorkspaceNavigation;

    public string VehicleEditorName
    {
        get => vehicleEditorName;
        set => SetProperty(ref vehicleEditorName, value);
    }

    public string VehicleEditorCategory
    {
        get => vehicleEditorCategory;
        set
        {
            if (SetProperty(ref vehicleEditorCategory, value))
            {
                NotifyKnownValueOptionChanged(nameof(SelectedVehicleCategoryOption), nameof(VehicleCategoryOptions));
            }
        }
    }

    public LocalizedOptionViewModel SelectedVehicleCategoryOption
    {
        get => KnownValueOptions.SelectVehicleCategory(VehicleEditorCategory);
        set => VehicleEditorCategory = value?.Value ?? string.Empty;
    }

    public string VehicleEditorNote
    {
        get => vehicleEditorNote;
        set => SetProperty(ref vehicleEditorNote, value);
    }

    public string VehicleEditorMakeModel
    {
        get => vehicleEditorMakeModel;
        set => SetProperty(ref vehicleEditorMakeModel, value);
    }

    public string VehicleEditorPlate
    {
        get => vehicleEditorPlate;
        set => SetProperty(ref vehicleEditorPlate, value);
    }

    public string VehicleEditorYear
    {
        get => vehicleEditorYear;
        set => SetProperty(ref vehicleEditorYear, value);
    }

    public string VehicleEditorPower
    {
        get => vehicleEditorPower;
        set => SetProperty(ref vehicleEditorPower, value);
    }

    public string VehicleEditorLastTk
    {
        get => vehicleEditorLastTk;
        set => SetProperty(ref vehicleEditorLastTk, value);
    }

    public string VehicleEditorNextTk
    {
        get => vehicleEditorNextTk;
        set => SetProperty(ref vehicleEditorNextTk, value);
    }

    public string VehicleEditorGreenCardFrom
    {
        get => vehicleEditorGreenCardFrom;
        set => SetProperty(ref vehicleEditorGreenCardFrom, value);
    }

    public string VehicleEditorGreenCardTo
    {
        get => vehicleEditorGreenCardTo;
        set => SetProperty(ref vehicleEditorGreenCardTo, value);
    }

    public string VehicleEditorState
    {
        get => vehicleEditorState;
        set
        {
            if (SetProperty(ref vehicleEditorState, value))
            {
                NotifyKnownValueOptionChanged(nameof(SelectedVehicleStateOption), nameof(VehicleStateOptions));
            }
        }
    }

    public LocalizedOptionViewModel SelectedVehicleStateOption
    {
        get => KnownValueOptions.SelectVehicleState(VehicleEditorState);
        set => VehicleEditorState = value?.Value ?? string.Empty;
    }

    public string VehicleEditorTags
    {
        get => vehicleEditorTags;
        set => SetProperty(ref vehicleEditorTags, value);
    }

    public string VehicleEditorPowertrain
    {
        get => vehicleEditorPowertrain;
        set
        {
            if (SetProperty(ref vehicleEditorPowertrain, value))
            {
                NotifyKnownValueOptionChanged(nameof(SelectedVehiclePowertrainOption), nameof(VehiclePowertrainOptions));
            }
        }
    }

    public LocalizedOptionViewModel SelectedVehiclePowertrainOption
    {
        get => KnownValueOptions.SelectVehiclePowertrain(VehicleEditorPowertrain);
        set => VehicleEditorPowertrain = value?.Value ?? string.Empty;
    }

    public string VehicleEditorClimateProfile
    {
        get => vehicleEditorClimateProfile;
        set
        {
            if (SetProperty(ref vehicleEditorClimateProfile, value))
            {
                NotifyKnownValueOptionChanged(nameof(SelectedVehicleClimateProfileOption), nameof(VehicleClimateProfileOptions));
            }
        }
    }

    public LocalizedOptionViewModel SelectedVehicleClimateProfileOption
    {
        get => KnownValueOptions.SelectVehicleClimateProfile(VehicleEditorClimateProfile);
        set => VehicleEditorClimateProfile = value?.Value ?? string.Empty;
    }

    public string VehicleEditorTimingDrive
    {
        get => vehicleEditorTimingDrive;
        set
        {
            if (SetProperty(ref vehicleEditorTimingDrive, value))
            {
                NotifyKnownValueOptionChanged(nameof(SelectedVehicleTimingDriveOption), nameof(VehicleTimingDriveOptions));
            }
        }
    }

    public LocalizedOptionViewModel SelectedVehicleTimingDriveOption
    {
        get => KnownValueOptions.SelectVehicleTimingDrive(VehicleEditorTimingDrive);
        set => VehicleEditorTimingDrive = value?.Value ?? string.Empty;
    }

    public string VehicleEditorTransmission
    {
        get => vehicleEditorTransmission;
        set
        {
            if (SetProperty(ref vehicleEditorTransmission, value))
            {
                NotifyKnownValueOptionChanged(nameof(SelectedVehicleTransmissionOption), nameof(VehicleTransmissionOptions));
            }
        }
    }

    public LocalizedOptionViewModel SelectedVehicleTransmissionOption
    {
        get => KnownValueOptions.SelectVehicleTransmission(VehicleEditorTransmission);
        set => VehicleEditorTransmission = value?.Value ?? string.Empty;
    }

    public ICommand CreateVehicleCommand => Root.CreateVehicleCommand;
    public ICommand EditSelectedVehicleCommand => Root.EditSelectedVehicleCommand;
    public IAsyncRelayCommand DeleteSelectedVehicleCommand => Root.DeleteSelectedVehicleCommand;
    public IAsyncRelayCommand SaveVehicleCommand => Root.SaveVehicleCommand;
    public ICommand CancelVehicleEditCommand => Root.CancelVehicleEditCommand;

    public VehicleStarterBundlePreview BuildVehicleStarterBundlePreview()
    {
        return Root.SelectedVehicle is null
            ? new VehicleStarterBundlePreview(string.Empty, string.Empty, string.Empty, [])
            : Root.BuildVehicleStarterBundlePreview(Root.SelectedVehicle.Id);
    }

    public Task<string> ApplyVehicleStarterBundleAsync(IReadOnlyList<VehicleStarterBundleTemplate> items)
    {
        return Root.SelectedVehicle is null
            ? Task.FromResult(L("VehicleDetail.Status.SelectVehicleFirst"))
            : Root.ApplyVehicleStarterBundleAsync(Root.SelectedVehicle.Id, items);
    }

    public bool TryConsumePendingVehicleStarterBundleOffer()
    {
        return Root.SelectedVehicle is not null && Root.TryConsumePendingVehicleStarterBundleOffer(Root.SelectedVehicle.Id);
    }

    public void SetVehicleStarterBundleStatus(string message)
    {
        Root.SetVehicleStarterBundleStatus(message);
    }

    public bool OpenVehicleHistoryWorkspace() =>
        OpenVehicleRelatedWorkspace(DesktopTabIndexes.History, DesktopFocusTarget.HistoryList);

    public bool OpenVehicleFuelWorkspace() =>
        OpenVehicleRelatedWorkspace(DesktopTabIndexes.Fuel, DesktopFocusTarget.FuelList);

    public bool OpenVehicleReminderWorkspace() =>
        OpenVehicleRelatedWorkspace(DesktopTabIndexes.Reminder, DesktopFocusTarget.ReminderList);

    public bool OpenVehicleMaintenanceWorkspace() =>
        OpenVehicleRelatedWorkspace(DesktopTabIndexes.Maintenance, DesktopFocusTarget.MaintenanceList);

    public bool OpenVehicleTimelineWorkspace() =>
        OpenVehicleRelatedWorkspace(DesktopTabIndexes.Timeline, DesktopFocusTarget.TimelineList);

    public bool OpenVehicleRecordWorkspace() =>
        OpenVehicleRelatedWorkspace(DesktopTabIndexes.Record, DesktopFocusTarget.RecordList);

    public async Task<bool> OpenVehicleCostsWorkspaceAsync()
    {
        if (!CanOpenVehicleRelatedWorkspace || !Root.OpenSelectedVehicleCostsCommand.CanExecute(null))
        {
            return false;
        }

        await Root.OpenSelectedVehicleCostsCommand.ExecuteAsync(null).ConfigureAwait(true);
        return Root.SelectedVehicleTabIndex == DesktopTabIndexes.Cost;
    }

    public ServiceBookWindowViewModel? BuildVehicleServiceBookModel() =>
        CanOpenVehicleRelatedWorkspace ? Root.BuildSelectedVehicleServiceBookModel() : null;

    internal void SetVehicleEditingState(bool isEditing, bool isNewVehicle)
    {
        VehiclePanelHeading = L("VehicleDetail.PanelHeading");
        VehicleEditorHeading = isEditing
            ? (isNewVehicle ? L("VehicleEditor.Title.New") : L("VehicleEditor.Title.Edit"))
            : L("VehicleEditor.Title.Default");
        IsEditingVehicle = isEditing;
        NotifyVehicleRelatedWorkspaceStateChanged();
    }

    internal void NotifyVehicleRelatedWorkspaceStateChanged()
    {
        OnPropertyChanged(nameof(CanOpenVehicleRelatedWorkspace));
    }

    private bool OpenVehicleRelatedWorkspace(int tabIndex, DesktopFocusTarget focusTarget)
    {
        if (!CanOpenVehicleRelatedWorkspace)
        {
            return false;
        }

        Root.SelectedVehicleTabIndex = tabIndex;
        RequestFocus(focusTarget);
        return true;
    }

    private void NotifyKnownValueOptionChanged(string selectedOptionPropertyName, string optionsPropertyName)
    {
        OnPropertyChanged(selectedOptionPropertyName);
        OnPropertyChanged(optionsPropertyName);
    }
}
