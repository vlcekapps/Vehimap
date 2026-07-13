// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Mobile.Services;

namespace Vehimap.Mobile.ViewModels;

public sealed class MobileVehiclesViewModel : ObservableObject
{
    private readonly MobileSessionController _session;
    private MobileVehicleListItemViewModel? _selectedVehicle;
    private MobileVehicleListItemViewModel? _activeVehicle;
    private bool _isVehicleHubVisible;

    public MobileVehiclesViewModel(MobileSessionController session)
    {
        _session = session;
        OpenSelectedVehicleCommand = new RelayCommand(OpenSelectedVehicle, () => SelectedVehicle is not null);
        BackToVehicleListCommand = new RelayCommand(BackToVehicleList);
    }

    public ObservableCollection<MobileVehicleListItemViewModel> Vehicles { get; } = [];

    public IRelayCommand OpenSelectedVehicleCommand { get; }

    public IRelayCommand BackToVehicleListCommand { get; }

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
            }
        }
    }

    public bool IsVehicleHubVisible
    {
        get => _isVehicleHubVisible;
        private set
        {
            if (SetProperty(ref _isVehicleHubVisible, value))
            {
                OnPropertyChanged(nameof(IsVehicleListVisible));
            }
        }
    }

    public bool IsVehicleListVisible => !IsVehicleHubVisible;

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

    public string HistoryCountText => BuildCount("Mobile.Vehicles.HistoryCount", _session.DataSet.HistoryEntries.Count(item => IsActiveVehicle(item.VehicleId)));

    public string FuelCountText => BuildCount("Mobile.Vehicles.FuelCount", _session.DataSet.FuelEntries.Count(item => IsActiveVehicle(item.VehicleId)));

    public string RecordCountText => BuildCount("Mobile.Vehicles.RecordCount", _session.DataSet.Records.Count(item => IsActiveVehicle(item.VehicleId)));

    public string ReminderCountText => BuildCount("Mobile.Vehicles.ReminderCount", _session.DataSet.Reminders.Count(item => IsActiveVehicle(item.VehicleId)));

    public string MaintenanceCountText => BuildCount("Mobile.Vehicles.MaintenanceCount", _session.DataSet.MaintenancePlans.Count(item => IsActiveVehicle(item.VehicleId)));

    internal void Refresh()
    {
        var selectedId = SelectedVehicle?.Id;
        var activeId = ActiveVehicle?.Id;
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
            IsVehicleHubVisible = false;
        }

        OnPropertyChanged(nameof(HasVehicles));
        RaiseLocalizedProperties();
    }

    public bool TryNavigateBack()
    {
        if (!IsVehicleHubVisible)
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
        IsVehicleHubVisible = true;
    }

    private void BackToVehicleList()
    {
        if (ActiveVehicle is not null)
        {
            SelectedVehicle = Vehicles.FirstOrDefault(item => string.Equals(item.Id, ActiveVehicle.Id, StringComparison.OrdinalIgnoreCase))
                ?? SelectedVehicle;
        }

        IsVehicleHubVisible = false;
        ActiveVehicle = null;
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
}
