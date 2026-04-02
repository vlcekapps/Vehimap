using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels.Workspaces;

public sealed class VehicleDetailWorkspaceViewModel : WorkspaceViewModelBase
{
    public VehicleDetailWorkspaceViewModel(MainWindowViewModel root)
        : base(root)
    {
    }

    public string WindowTitle => Root.VehicleDetailWindowTitle;
    public string VehiclePanelHeading => Root.VehiclePanelHeading;
    public string SelectedVehicleHeading => Root.SelectedVehicleHeading;
    public string SelectedVehicleOverview => Root.SelectedVehicleOverview;
    public string SelectedVehicleDates => Root.SelectedVehicleDates;
    public string SelectedVehicleProfile => Root.SelectedVehicleProfile;
    public bool IsEditingVehicle => Root.IsEditingVehicle;
    public bool IsVehicleDetailVisible => Root.IsVehicleDetailVisible;
    public string VehicleEditorStatus => Root.VehicleEditorStatus;
    public IReadOnlyList<string> VehicleCategoryOptions => LegacyKnownValues.Categories;
    public IReadOnlyList<string> VehicleStateOptions => LegacyKnownValues.VehicleStates;
    public IReadOnlyList<string> VehiclePowertrainOptions => LegacyKnownValues.VehiclePowertrains;
    public IReadOnlyList<string> VehicleClimateProfileOptions => LegacyKnownValues.VehicleClimateProfiles;
    public IReadOnlyList<string> VehicleTimingDriveOptions => LegacyKnownValues.VehicleTimingDrives;
    public IReadOnlyList<string> VehicleTransmissionOptions => LegacyKnownValues.VehicleTransmissions;
    public bool CanOpenVehicleStarterBundle => Root.CanOpenVehicleStarterBundle;

    public string VehicleEditorName
    {
        get => Root.VehicleEditorName;
        set => Root.VehicleEditorName = value;
    }

    public string VehicleEditorCategory
    {
        get => Root.VehicleEditorCategory;
        set => Root.VehicleEditorCategory = value;
    }

    public string VehicleEditorNote
    {
        get => Root.VehicleEditorNote;
        set => Root.VehicleEditorNote = value;
    }

    public string VehicleEditorMakeModel
    {
        get => Root.VehicleEditorMakeModel;
        set => Root.VehicleEditorMakeModel = value;
    }

    public string VehicleEditorPlate
    {
        get => Root.VehicleEditorPlate;
        set => Root.VehicleEditorPlate = value;
    }

    public string VehicleEditorYear
    {
        get => Root.VehicleEditorYear;
        set => Root.VehicleEditorYear = value;
    }

    public string VehicleEditorPower
    {
        get => Root.VehicleEditorPower;
        set => Root.VehicleEditorPower = value;
    }

    public string VehicleEditorLastTk
    {
        get => Root.VehicleEditorLastTk;
        set => Root.VehicleEditorLastTk = value;
    }

    public string VehicleEditorNextTk
    {
        get => Root.VehicleEditorNextTk;
        set => Root.VehicleEditorNextTk = value;
    }

    public string VehicleEditorGreenCardFrom
    {
        get => Root.VehicleEditorGreenCardFrom;
        set => Root.VehicleEditorGreenCardFrom = value;
    }

    public string VehicleEditorGreenCardTo
    {
        get => Root.VehicleEditorGreenCardTo;
        set => Root.VehicleEditorGreenCardTo = value;
    }

    public string VehicleEditorState
    {
        get => Root.VehicleEditorState;
        set => Root.VehicleEditorState = value;
    }

    public string VehicleEditorPowertrain
    {
        get => Root.VehicleEditorPowertrain;
        set => Root.VehicleEditorPowertrain = value;
    }

    public string VehicleEditorClimateProfile
    {
        get => Root.VehicleEditorClimateProfile;
        set => Root.VehicleEditorClimateProfile = value;
    }

    public string VehicleEditorTimingDrive
    {
        get => Root.VehicleEditorTimingDrive;
        set => Root.VehicleEditorTimingDrive = value;
    }

    public string VehicleEditorTransmission
    {
        get => Root.VehicleEditorTransmission;
        set => Root.VehicleEditorTransmission = value;
    }

    public ICommand CreateVehicleCommand => Root.CreateVehicleCommand;
    public ICommand EditSelectedVehicleCommand => Root.EditSelectedVehicleCommand;
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
        OnPropertyChanged(nameof(VehicleEditorStatus));
    }
}
