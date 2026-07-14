// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Mobile.Services;

namespace Vehimap.Mobile.ViewModels;

public sealed class MobileVehiclesViewModel : ObservableObject
{
    private readonly MobileSessionController _session;
    private readonly MobileVehicleEvidenceProjectionService _evidenceProjectionService = new();
    private MobileVehicleListItemViewModel? _selectedVehicle;
    private MobileVehicleListItemViewModel? _activeVehicle;
    private MobileVehicleScreen _screen;

    public MobileVehiclesViewModel(MobileSessionController session)
    {
        _session = session;
        OpenSelectedVehicleCommand = new RelayCommand(OpenSelectedVehicle, () => SelectedVehicle is not null);
        BackToVehicleListCommand = new RelayCommand(BackToVehicleList);
        OpenHistoryCommand = new RelayCommand(OpenHistory, () => ActiveVehicle is not null);
        OpenFuelCommand = new RelayCommand(OpenFuel, () => ActiveVehicle is not null);
        OpenRecordsCommand = new RelayCommand(OpenRecords, () => ActiveVehicle is not null);
        OpenRemindersCommand = new RelayCommand(OpenReminders, () => ActiveVehicle is not null);
        Evidence = new MobileVehicleEvidenceViewModel(session, BackToVehicleHub);
    }

    public ObservableCollection<MobileVehicleListItemViewModel> Vehicles { get; } = [];

    public IRelayCommand OpenSelectedVehicleCommand { get; }

    public IRelayCommand BackToVehicleListCommand { get; }

    public IRelayCommand OpenHistoryCommand { get; }

    public IRelayCommand OpenFuelCommand { get; }

    public IRelayCommand OpenRecordsCommand { get; }

    public IRelayCommand OpenRemindersCommand { get; }

    public MobileVehicleEvidenceViewModel Evidence { get; }

    public MobileVehicleListItemViewModel? SelectedVehicle
    {
        get => _selectedVehicle;
        set
        {
            if (SetProperty(ref _selectedVehicle, value))
            {
                OnPropertyChanged(nameof(CanOpenSelectedVehicle));
                OpenSelectedVehicleCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public MobileVehicleListItemViewModel? ActiveVehicle
    {
        get => _activeVehicle;
        private set
        {
            if (SetProperty(ref _activeVehicle, value))
            {
                RaiseVehicleHubProperties();
                OpenHistoryCommand.NotifyCanExecuteChanged();
                OpenFuelCommand.NotifyCanExecuteChanged();
                OpenRecordsCommand.NotifyCanExecuteChanged();
                OpenRemindersCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private MobileVehicleScreen Screen
    {
        get => _screen;
        set
        {
            if (SetProperty(ref _screen, value))
            {
                OnPropertyChanged(nameof(IsVehicleListVisible));
                OnPropertyChanged(nameof(IsVehicleHubVisible));
                OnPropertyChanged(nameof(IsEvidenceVisible));
            }
        }
    }

    public bool IsVehicleListVisible => Screen == MobileVehicleScreen.List;

    public bool IsVehicleHubVisible => Screen == MobileVehicleScreen.Hub;

    public bool IsEvidenceVisible => Screen == MobileVehicleScreen.Evidence;

    public bool HasVehicles => Vehicles.Count > 0;

    public bool CanOpenSelectedVehicle => SelectedVehicle is not null;

    public string Heading => L("Mobile.Vehicles.Heading");

    public string VehicleListName => L("Mobile.VehicleList.Name");

    public string VehicleListEmptyText => L("Mobile.VehicleList.Empty");

    public string OpenSelectedVehicleText => L("Mobile.Vehicles.OpenSelected");

    public string OpenSelectedVehicleName => L("Mobile.Vehicles.OpenSelectedName");

    public string BackText => L("Mobile.Common.Back");

    public string BackName => L("Mobile.Vehicles.BackName");

    public string VehicleHubHeading => ActiveVehicle is null
        ? L("Mobile.VehicleDetail.EmptyHeading")
        : LF("Mobile.Vehicles.HubHeading", ActiveVehicle.Name);

    public string NameLabel => L("Mobile.VehicleDetail.Name");

    public string MakeModelLabel => L("Mobile.VehicleDetail.MakeModel");

    public string CategoryLabel => L("Mobile.VehicleDetail.Category");

    public string PlateLabel => L("Mobile.VehicleDetail.Plate");

    public string StateLabel => L("Mobile.VehicleDetail.State");

    public string YearLabel => L("Mobile.VehicleDetail.Year");

    public string PowerLabel => L("Mobile.VehicleDetail.Power");

    public string NextTechnicalInspectionLabel => L("Mobile.VehicleDetail.NextTechnicalInspection");

    public string GreenCardToLabel => L("Mobile.VehicleDetail.GreenCardTo");

    public string NoteLabel => L("Mobile.VehicleDetail.Note");

    public string EvidenceHeading => L("Mobile.Vehicles.EvidenceHeading");

    public string OpenHistoryName => L("Mobile.Vehicles.OpenHistoryName");

    public string OpenFuelName => L("Mobile.Vehicles.OpenFuelName");

    public string OpenRecordsName => L("Mobile.Vehicles.OpenRecordsName");

    public string OpenRemindersName => L("Mobile.Vehicles.OpenRemindersName");

    public string HistoryCountText => BuildCount("Mobile.Vehicles.HistoryCount", _session.DataSet.HistoryEntries.Count(item => IsActiveVehicle(item.VehicleId)));

    public string FuelCountText => BuildCount("Mobile.Vehicles.FuelCount", _session.DataSet.FuelEntries.Count(item => IsActiveVehicle(item.VehicleId)));

    public string RecordCountText => BuildCount("Mobile.Vehicles.RecordCount", _session.DataSet.Records.Count(item => IsActiveVehicle(item.VehicleId)));

    public string ReminderCountText => BuildCount("Mobile.Vehicles.ReminderCount", _session.DataSet.Reminders.Count(item => IsActiveVehicle(item.VehicleId)));

    public string MaintenanceCountText => BuildCount("Mobile.Vehicles.MaintenanceCount", _session.DataSet.MaintenancePlans.Count(item => IsActiveVehicle(item.VehicleId)));

    internal void Refresh()
    {
        var selectedId = SelectedVehicle?.Id;
        var activeId = ActiveVehicle?.Id;
        var evidenceKind = Evidence.Kind;
        var selectedEvidenceId = Evidence.SelectedItem?.Id;
        var evidenceDetailWasVisible = Evidence.IsDetailVisible;
        var metaByVehicle = _session.DataSet.VehicleMetaEntries
            .GroupBy(item => item.VehicleId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);
        var projectedVehicles = _session.DataSet.Vehicles
            .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(vehicle => new MobileVehicleListItemViewModel(
                vehicle,
                metaByVehicle.GetValueOrDefault(vehicle.Id),
                _session.Localizer))
            .ToArray();

        Vehicles.Clear();
        foreach (var vehicle in projectedVehicles)
        {
            Vehicles.Add(vehicle);
        }

        SelectedVehicle = Vehicles.FirstOrDefault(item => string.Equals(item.Id, selectedId, StringComparison.OrdinalIgnoreCase))
            ?? Vehicles.FirstOrDefault();
        ActiveVehicle = Vehicles.FirstOrDefault(item => string.Equals(item.Id, activeId, StringComparison.OrdinalIgnoreCase));
        if (ActiveVehicle is null)
        {
            Screen = MobileVehicleScreen.List;
            Evidence.Clear();
        }
        else if (Screen == MobileVehicleScreen.Evidence)
        {
            LoadEvidence(evidenceKind, selectedEvidenceId, evidenceDetailWasVisible);
        }

        OnPropertyChanged(nameof(HasVehicles));
        RaiseLocalizedProperties();
    }

    public bool TryNavigateBack()
    {
        if (Screen == MobileVehicleScreen.Evidence)
        {
            if (Evidence.TryNavigateBack())
            {
                return true;
            }

            BackToVehicleHub();
            return true;
        }

        if (Screen != MobileVehicleScreen.Hub)
        {
            return false;
        }

        BackToVehicleList();
        return true;
    }

    private void OpenSelectedVehicle()
    {
        if (SelectedVehicle is null)
        {
            return;
        }

        ActiveVehicle = SelectedVehicle;
        Screen = MobileVehicleScreen.Hub;
    }

    private void BackToVehicleList()
    {
        if (ActiveVehicle is not null)
        {
            SelectedVehicle = Vehicles.FirstOrDefault(item => string.Equals(item.Id, ActiveVehicle.Id, StringComparison.OrdinalIgnoreCase))
                ?? SelectedVehicle;
        }

        Screen = MobileVehicleScreen.List;
        Evidence.Clear();
        ActiveVehicle = null;
    }

    private void OpenHistory() => OpenEvidence(MobileVehicleEvidenceKind.History);

    private void OpenFuel() => OpenEvidence(MobileVehicleEvidenceKind.Fuel);

    private void OpenRecords() => OpenEvidence(MobileVehicleEvidenceKind.Records);

    private void OpenReminders() => OpenEvidence(MobileVehicleEvidenceKind.Reminders);

    private void OpenEvidence(MobileVehicleEvidenceKind kind)
    {
        if (ActiveVehicle is null)
        {
            return;
        }

        LoadEvidence(kind);
        Screen = MobileVehicleScreen.Evidence;
    }

    private void LoadEvidence(
        MobileVehicleEvidenceKind kind,
        string? selectedItemId = null,
        bool showDetail = false)
    {
        if (ActiveVehicle is null)
        {
            Evidence.Clear();
            return;
        }

        var projection = kind switch
        {
            MobileVehicleEvidenceKind.History => _evidenceProjectionService.BuildHistory(
                _session.DataSet,
                ActiveVehicle.Id,
                ActiveVehicle.Name,
                _session.Settings,
                _session.Localizer),
            MobileVehicleEvidenceKind.Fuel => _evidenceProjectionService.BuildFuel(
                _session.DataSet,
                ActiveVehicle.Id,
                ActiveVehicle.Name,
                _session.Settings,
                _session.Localizer),
            MobileVehicleEvidenceKind.Records => _evidenceProjectionService.BuildRecords(
                _session.DataRoot,
                _session.DataSet,
                ActiveVehicle.Id,
                ActiveVehicle.Name,
                _session.Settings,
                _session.Localizer,
                _session.ResolveManagedAttachmentPath),
            MobileVehicleEvidenceKind.Reminders => _evidenceProjectionService.BuildReminders(
                _session.DataSet,
                ActiveVehicle.Id,
                ActiveVehicle.Name,
                _session.Settings,
                _session.Localizer,
                DateOnly.FromDateTime(DateTime.Today)),
            _ => null
        };

        if (projection is null)
        {
            Evidence.Clear();
            return;
        }

        Evidence.Load(projection, ActiveVehicle.Name, selectedItemId, showDetail);
    }

    private void BackToVehicleHub()
    {
        Evidence.Clear();
        Screen = ActiveVehicle is null ? MobileVehicleScreen.List : MobileVehicleScreen.Hub;
    }

    private bool IsActiveVehicle(string vehicleId) =>
        ActiveVehicle is not null
        && string.Equals(ActiveVehicle.Id, vehicleId, StringComparison.OrdinalIgnoreCase);

    private string BuildCount(string key, int count) => LF(key, count);

    private void RaiseLocalizedProperties()
    {
        foreach (var propertyName in new[]
        {
            nameof(Heading),
            nameof(VehicleListName),
            nameof(VehicleListEmptyText),
            nameof(OpenSelectedVehicleText),
            nameof(OpenSelectedVehicleName),
            nameof(BackText),
            nameof(BackName),
            nameof(VehicleHubHeading),
            nameof(NameLabel),
            nameof(MakeModelLabel),
            nameof(CategoryLabel),
            nameof(PlateLabel),
            nameof(StateLabel),
            nameof(YearLabel),
            nameof(PowerLabel),
            nameof(NextTechnicalInspectionLabel),
            nameof(GreenCardToLabel),
            nameof(NoteLabel),
            nameof(EvidenceHeading),
            nameof(OpenHistoryName),
            nameof(OpenFuelName),
            nameof(OpenRecordsName),
            nameof(OpenRemindersName),
            nameof(HistoryCountText),
            nameof(FuelCountText),
            nameof(RecordCountText),
            nameof(ReminderCountText),
            nameof(MaintenanceCountText)
        })
        {
            OnPropertyChanged(propertyName);
        }
    }

    private void RaiseVehicleHubProperties()
    {
        OnPropertyChanged(nameof(VehicleHubHeading));
        OnPropertyChanged(nameof(HistoryCountText));
        OnPropertyChanged(nameof(FuelCountText));
        OnPropertyChanged(nameof(RecordCountText));
        OnPropertyChanged(nameof(ReminderCountText));
        OnPropertyChanged(nameof(MaintenanceCountText));
    }

    private string L(string key) => _session.Localizer.GetString(key);

    private string LF(string key, params object?[] args) => _session.Localizer.Format(key, args);

    private enum MobileVehicleScreen
    {
        List,
        Hub,
        Evidence
    }
}
