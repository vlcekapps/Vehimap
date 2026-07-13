// SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Mobile.Services;
using Vehimap.Storage.Sqlite;

namespace Vehimap.Mobile.ViewModels;

public sealed class MobileMainViewModel : ObservableObject
{
    private readonly MobileSessionController _session;
    private MobilePrimaryDestination _selectedDestination = MobilePrimaryDestination.Home;
    private string _statusText;
    private bool _isBusy;
    private bool _initialized;

    public MobileMainViewModel(MobileSessionController session)
    {
        _session = session;
        _statusText = L("Mobile.Status.Ready");
        ReloadCommand = new AsyncRelayCommand(ReloadAsync, () => !IsBusy);
        SelectHomeCommand = new RelayCommand(() => SelectDestination(MobilePrimaryDestination.Home));
        SelectVehiclesCommand = new RelayCommand(() => SelectDestination(MobilePrimaryDestination.Vehicles));
        SelectAlertsCommand = new RelayCommand(() => SelectDestination(MobilePrimaryDestination.Alerts));
        SelectMoreCommand = new RelayCommand(() => SelectDestination(MobilePrimaryDestination.More));
        Home = new MobileHomeViewModel(session, SelectDestination);
        Vehicles = new MobileVehiclesViewModel(session);
        Alerts = new MobileAlertsViewModel(session);
        More = new MobileMoreViewModel(session, ReloadCommand);
    }

    public MobileHomeViewModel Home { get; }

    public MobileVehiclesViewModel Vehicles { get; }

    public MobileAlertsViewModel Alerts { get; }

    public MobileMoreViewModel More { get; }

    public IAsyncRelayCommand ReloadCommand { get; }

    public IRelayCommand SelectHomeCommand { get; }

    public IRelayCommand SelectVehiclesCommand { get; }

    public IRelayCommand SelectAlertsCommand { get; }

    public IRelayCommand SelectMoreCommand { get; }

    public MobilePrimaryDestination SelectedDestination
    {
        get => _selectedDestination;
        private set
        {
            if (SetProperty(ref _selectedDestination, value))
            {
                RaiseNavigationProperties();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ReloadCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsHomeSelected => SelectedDestination == MobilePrimaryDestination.Home;

    public bool IsVehiclesSelected => SelectedDestination == MobilePrimaryDestination.Vehicles;

    public bool IsAlertsSelected => SelectedDestination == MobilePrimaryDestination.Alerts;

    public bool IsMoreSelected => SelectedDestination == MobilePrimaryDestination.More;

    public bool HasVehicles => Vehicles.HasVehicles;

    public string AppTitle => L("Mobile.App.Title");

    public string NavigationName => L("Mobile.Navigation.Name");

    public string NavigationHelp => L("Mobile.Navigation.Help");

    public string HomeNavigationText => L("Mobile.Navigation.Home");

    public string VehiclesNavigationText => L("Mobile.Navigation.Vehicles");

    public string AlertsNavigationText => L("Mobile.Navigation.Alerts");

    public string MoreNavigationText => L("Mobile.Navigation.More");

    public static MobileMainViewModel CreateDefault()
    {
        var cultureService = new AppCultureService();
        var preferences = AppLocaleDefaultsService.GetCurrentCultureDefaults().ToCulturePreferences();
        var culture = cultureService.ResolveCulture(preferences.Language);
        var session = new MobileSessionController(
            new SqliteVehimapDataStore(),
            new MobileDataRootProvider(),
            cultureService,
            new DesktopSupportedSettingsService(),
            new ResourceAppLocalizer(culture));
        return new MobileMainViewModel(session);
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ReloadAsync();
    }

    public bool TryNavigateBack()
    {
        if (SelectedDestination == MobilePrimaryDestination.Vehicles && Vehicles.TryNavigateBack())
        {
            return true;
        }

        if (SelectedDestination == MobilePrimaryDestination.Home)
        {
            return false;
        }

        SelectDestination(MobilePrimaryDestination.Home);
        return true;
    }

    private async Task ReloadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        StatusText = L("Mobile.Status.Loading");
        try
        {
            await _session.ReloadAsync();
            RefreshChildViewModels();
            RaiseLocalizedProperties();
            StatusText = Vehicles.HasVehicles
                ? LF("Mobile.Status.Loaded", Vehicles.Vehicles.Count)
                : L("Mobile.Status.NoVehicles");
        }
        catch (Exception exception)
        {
            StatusText = LF(
                "Mobile.Status.LoadFailed",
                UserFacingExceptionMessageService.Describe(exception, _session.Localizer));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SelectDestination(MobilePrimaryDestination destination)
    {
        SelectedDestination = destination;
    }

    private void RefreshChildViewModels()
    {
        Home.Refresh();
        Vehicles.Refresh();
        Alerts.Refresh();
        More.Refresh();
        OnPropertyChanged(nameof(HasVehicles));
    }

    private void RaiseNavigationProperties()
    {
        OnPropertyChanged(nameof(IsHomeSelected));
        OnPropertyChanged(nameof(IsVehiclesSelected));
        OnPropertyChanged(nameof(IsAlertsSelected));
        OnPropertyChanged(nameof(IsMoreSelected));
    }

    private void RaiseLocalizedProperties()
    {
        foreach (var propertyName in new[]
        {
            nameof(AppTitle),
            nameof(NavigationName),
            nameof(NavigationHelp),
            nameof(HomeNavigationText),
            nameof(VehiclesNavigationText),
            nameof(AlertsNavigationText),
            nameof(MoreNavigationText)
        })
        {
            OnPropertyChanged(propertyName);
        }
    }

    private string L(string key) => _session.Localizer.GetString(key);

    private string LF(string key, params object?[] args) => _session.Localizer.Format(key, args);
}
