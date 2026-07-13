// SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Mobile.Services;
using Vehimap.Storage.Sqlite;

namespace Vehimap.Mobile.ViewModels;

public sealed class MobileMainViewModel : ObservableObject
{
    private readonly IVehimapDataStore _dataStore;
    private readonly IMobileDataRootProvider _dataRootProvider;
    private readonly AppCultureService _cultureService;
    private readonly DesktopSupportedSettingsService _settingsService;
    private IAppLocalizer _localizer;
    private MobileVehicleListItemViewModel? _selectedVehicle;
    private string _statusText;
    private bool _isBusy;
    private bool _initialized;

    public MobileMainViewModel(
        IVehimapDataStore dataStore,
        IMobileDataRootProvider dataRootProvider,
        AppCultureService cultureService,
        DesktopSupportedSettingsService settingsService,
        IAppLocalizer localizer)
    {
        _dataStore = dataStore;
        _dataRootProvider = dataRootProvider;
        _cultureService = cultureService;
        _settingsService = settingsService;
        _localizer = localizer;
        _statusText = L("Mobile.Status.Ready");
        ReloadCommand = new AsyncRelayCommand(ReloadAsync, () => !IsBusy);
    }

    public ObservableCollection<MobileVehicleListItemViewModel> Vehicles { get; } = [];

    public IAsyncRelayCommand ReloadCommand { get; }

    public MobileVehicleListItemViewModel? SelectedVehicle
    {
        get => _selectedVehicle;
        set
        {
            if (SetProperty(ref _selectedVehicle, value))
            {
                OnPropertyChanged(nameof(HasSelectedVehicle));
                OnPropertyChanged(nameof(VehicleDetailHeading));
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

    public bool HasVehicles => Vehicles.Count > 0;

    public bool HasSelectedVehicle => SelectedVehicle is not null;

    public string AppTitle => L("Mobile.App.Title");

    public string Heading => L("Mobile.Shell.Heading");

    public string IntroText => L("Mobile.Shell.Intro");

    public string ReadOnlyText => L("Mobile.Shell.ReadOnly");

    public string VehicleListName => L("Mobile.VehicleList.Name");

    public string VehicleListHelp => L("Mobile.VehicleList.Help");

    public string VehicleListEmptyText => L("Mobile.VehicleList.Empty");

    public string VehicleDetailHeading => SelectedVehicle is null
        ? L("Mobile.VehicleDetail.EmptyHeading")
        : LF("Mobile.VehicleDetail.Heading", SelectedVehicle.Name);

    public string VehicleDetailEmptyText => L("Mobile.VehicleDetail.EmptyText");

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

    public string ReloadText => L("Mobile.Action.Reload");

    public string ReloadName => L("Mobile.Action.ReloadName");

    public static MobileMainViewModel CreateDefault()
    {
        var cultureService = new AppCultureService();
        var preferences = AppLocaleDefaultsService.GetCurrentCultureDefaults().ToCulturePreferences();
        var culture = cultureService.ResolveCulture(preferences.Language);
        return new MobileMainViewModel(
            new SqliteVehimapDataStore(),
            new MobileDataRootProvider(),
            cultureService,
            new DesktopSupportedSettingsService(),
            new ResourceAppLocalizer(culture));
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

    public async Task ReloadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        StatusText = L("Mobile.Status.Loading");
        try
        {
            var dataSet = await _dataStore.LoadAsync(_dataRootProvider.GetDataRoot());
            ConfigureLocalization(_settingsService.Read(dataSet.Settings));

            var previouslySelectedId = SelectedVehicle?.Id;
            var metaByVehicle = dataSet.VehicleMetaEntries
                .GroupBy(item => item.VehicleId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);
            var projectedVehicles = dataSet.Vehicles
                .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(vehicle => new MobileVehicleListItemViewModel(
                    vehicle,
                    metaByVehicle.GetValueOrDefault(vehicle.Id),
                    _localizer))
                .ToArray();

            Vehicles.Clear();
            foreach (var vehicle in projectedVehicles)
            {
                Vehicles.Add(vehicle);
            }

            SelectedVehicle = Vehicles.FirstOrDefault(item =>
                    string.Equals(item.Id, previouslySelectedId, StringComparison.OrdinalIgnoreCase))
                ?? Vehicles.FirstOrDefault();
            OnPropertyChanged(nameof(HasVehicles));
            StatusText = Vehicles.Count == 0
                ? L("Mobile.Status.NoVehicles")
                : LF("Mobile.Status.Loaded", Vehicles.Count);
        }
        catch (Exception exception)
        {
            Vehicles.Clear();
            SelectedVehicle = null;
            OnPropertyChanged(nameof(HasVehicles));
            StatusText = LF(
                "Mobile.Status.LoadFailed",
                UserFacingExceptionMessageService.Describe(exception, _localizer));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ConfigureLocalization(DesktopSupportedSettingsSnapshot settings)
    {
        var preferences = new AppCulturePreferences(
            settings.Language,
            settings.ThousandsSeparator,
            settings.DecimalSeparator);
        _cultureService.ApplyThreadCulture(preferences);
        _localizer = new ResourceAppLocalizer(_cultureService.ResolveCulture(preferences.Language));
        RaiseLocalizedProperties();
    }

    private void RaiseLocalizedProperties()
    {
        foreach (var propertyName in new[]
        {
            nameof(AppTitle),
            nameof(Heading),
            nameof(IntroText),
            nameof(ReadOnlyText),
            nameof(VehicleListName),
            nameof(VehicleListHelp),
            nameof(VehicleListEmptyText),
            nameof(VehicleDetailHeading),
            nameof(VehicleDetailEmptyText),
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
            nameof(ReloadText),
            nameof(ReloadName)
        })
        {
            OnPropertyChanged(propertyName);
        }
    }

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);
}
