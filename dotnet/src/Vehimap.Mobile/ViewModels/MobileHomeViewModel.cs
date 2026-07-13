// SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Mobile.Services;

namespace Vehimap.Mobile.ViewModels;

public sealed class MobileHomeViewModel : ObservableObject
{
    private readonly MobileSessionController _session;

    public MobileHomeViewModel(
        MobileSessionController session,
        Action<MobilePrimaryDestination> navigate)
    {
        _session = session;
        OpenVehiclesCommand = new RelayCommand(() => navigate(MobilePrimaryDestination.Vehicles));
        OpenAlertsCommand = new RelayCommand(() => navigate(MobilePrimaryDestination.Alerts));
    }

    public IRelayCommand OpenVehiclesCommand { get; }

    public IRelayCommand OpenAlertsCommand { get; }

    public string Heading => L("Mobile.Home.Heading");

    public string Intro => L("Mobile.Home.Intro");

    public string VehicleCountText => LF("Mobile.Home.VehicleCount", _session.DataSet.Vehicles.Count);

    public string AlertCountText => LF("Mobile.Home.AlertCount", _session.AdvisorSummary.TotalCount);

    public string TopAlertHeading => L("Mobile.Home.TopAlertHeading");

    public string TopAlertText
    {
        get
        {
            var item = _session.AdvisorSummary.Items.FirstOrDefault();
            return item is null
                ? L("Mobile.Home.NoAlerts")
                : LF("Mobile.Home.TopAlert", item.VehicleName, item.Title, item.Summary);
        }
    }

    public string OpenVehiclesText => L("Mobile.Home.OpenVehicles");

    public string OpenVehiclesName => L("Mobile.Home.OpenVehiclesName");

    public string OpenAlertsText => L("Mobile.Home.OpenAlerts");

    public string OpenAlertsName => L("Mobile.Home.OpenAlertsName");

    internal void Refresh()
    {
        OnPropertyChanged(nameof(Heading));
        OnPropertyChanged(nameof(Intro));
        OnPropertyChanged(nameof(VehicleCountText));
        OnPropertyChanged(nameof(AlertCountText));
        OnPropertyChanged(nameof(TopAlertHeading));
        OnPropertyChanged(nameof(TopAlertText));
        OnPropertyChanged(nameof(OpenVehiclesText));
        OnPropertyChanged(nameof(OpenVehiclesName));
        OnPropertyChanged(nameof(OpenAlertsText));
        OnPropertyChanged(nameof(OpenAlertsName));
    }

    private string L(string key) => _session.Localizer.GetString(key);

    private string LF(string key, params object?[] args) => _session.Localizer.Format(key, args);
}
