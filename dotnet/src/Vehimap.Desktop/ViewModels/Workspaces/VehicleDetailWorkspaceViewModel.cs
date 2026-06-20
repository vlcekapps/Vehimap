using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class VehicleDetailWorkspaceViewModel : WorkspaceViewModelBase
{
    private bool isEditingVehicle;
    private string vehiclePanelHeading = "Detail vozidla";
    private string selectedVehicleHeading = "Nevybrané vozidlo";
    private string selectedVehicleOverview = "Vyberte vozidlo vlevo a zobrazí se jeho základní souhrn.";
    private string selectedVehicleDates = string.Empty;
    private string selectedVehicleProfile = string.Empty;
    private string selectedVehicleEvidenceSummary = "Navazující evidence se zobrazí po výběru vozidla.";
    private string selectedVehicleRecentHistorySummary = "Poslední události se zobrazí po výběru vozidla.";
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
    private string vehicleEditorPowertrain = string.Empty;
    private string vehicleEditorClimateProfile = string.Empty;
    private string vehicleEditorTimingDrive = string.Empty;
    private string vehicleEditorTransmission = string.Empty;

    public VehicleDetailWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.VehicleDetailWindowTitle;

    public string VehiclePanelHeading
    {
        get => vehiclePanelHeading;
        private set => SetProperty(ref vehiclePanelHeading, value);
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

    public bool IsVehicleDetailVisible => !IsEditingVehicle;

    public string VehicleEditorStatus
    {
        get => vehicleEditorStatus;
        set => SetProperty(ref vehicleEditorStatus, value);
    }

    public IReadOnlyList<string> VehicleCategoryOptions => LegacyKnownValues.Categories;
    public IReadOnlyList<string> VehicleStateOptions => LegacyKnownValues.VehicleStates;
    public IReadOnlyList<string> VehiclePowertrainOptions => LegacyKnownValues.VehiclePowertrains;
    public IReadOnlyList<string> VehicleClimateProfileOptions => LegacyKnownValues.VehicleClimateProfiles;
    public IReadOnlyList<string> VehicleTimingDriveOptions => LegacyKnownValues.VehicleTimingDrives;
    public IReadOnlyList<string> VehicleTransmissionOptions => LegacyKnownValues.VehicleTransmissions;
    public bool CanOpenVehicleStarterBundle => Root.CanOpenVehicleStarterBundle;

    public string VehicleEditorName
    {
        get => vehicleEditorName;
        set => SetProperty(ref vehicleEditorName, value);
    }

    public string VehicleEditorCategory
    {
        get => vehicleEditorCategory;
        set => SetProperty(ref vehicleEditorCategory, value);
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
        set => SetProperty(ref vehicleEditorState, value);
    }

    public string VehicleEditorPowertrain
    {
        get => vehicleEditorPowertrain;
        set => SetProperty(ref vehicleEditorPowertrain, value);
    }

    public string VehicleEditorClimateProfile
    {
        get => vehicleEditorClimateProfile;
        set => SetProperty(ref vehicleEditorClimateProfile, value);
    }

    public string VehicleEditorTimingDrive
    {
        get => vehicleEditorTimingDrive;
        set => SetProperty(ref vehicleEditorTimingDrive, value);
    }

    public string VehicleEditorTransmission
    {
        get => vehicleEditorTransmission;
        set => SetProperty(ref vehicleEditorTransmission, value);
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
            ? Task.FromResult("Nejprve vyberte vozidlo.")
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

    internal void SetVehicleEditingState(bool isEditing, bool isNewVehicle)
    {
        VehiclePanelHeading = isEditing
            ? (isNewVehicle ? "Nové vozidlo" : "Upravit vozidlo")
            : "Detail vozidla";
        IsEditingVehicle = isEditing;
    }
}
