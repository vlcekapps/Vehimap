using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class SettingsDialogViewModel : ObservableObject
{
    private const int MinMaintenanceReminderKm = 1;
    private const int MaxMaintenanceReminderKm = 999999;

    private IAppLocalizer _localizer = new ResourceAppLocalizer();
    private readonly IAppNumberFormatService _numberFormatService = new AppNumberFormatService();
    private readonly IAppUnitFormatService _unitFormatService = new AppUnitFormatService();
    private decimal maintenanceReminderDistanceKilometers = 1000m;

    [ObservableProperty]
    private string technicalReminderDays = string.Empty;

    [ObservableProperty]
    private string greenCardReminderDays = string.Empty;

    [ObservableProperty]
    private string maintenanceReminderDays = string.Empty;

    [ObservableProperty]
    private string maintenanceReminderKm = string.Empty;

    [ObservableProperty]
    private bool runAtStartup;

    [ObservableProperty]
    private bool hideOnLaunch;

    [ObservableProperty]
    private bool showDashboardOnLaunch;

    [ObservableProperty]
    private bool automaticBackupsEnabled;

    [ObservableProperty]
    private string automaticBackupIntervalDays = string.Empty;

    [ObservableProperty]
    private string automaticBackupKeepCount = string.Empty;

    [ObservableProperty]
    private string automaticBackupStatus = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private SettingsOptionViewModel? selectedLanguageOption;

    [ObservableProperty]
    private SettingsOptionViewModel? selectedThousandsSeparatorOption;

    [ObservableProperty]
    private SettingsOptionViewModel? selectedDecimalSeparatorOption;

    [ObservableProperty]
    private SettingsOptionViewModel? selectedDistanceUnitOption;

    [ObservableProperty]
    private SettingsOptionViewModel? selectedVolumeUnitOption;

    [ObservableProperty]
    private SettingsOptionViewModel? selectedCurrencyOption;

    public IReadOnlyList<SettingsOptionViewModel> LanguageOptions { get; private init; } = [];

    public IReadOnlyList<SettingsOptionViewModel> ThousandsSeparatorOptions { get; private init; } = [];

    public IReadOnlyList<SettingsOptionViewModel> DecimalSeparatorOptions { get; private init; } = [];

    public IReadOnlyList<SettingsOptionViewModel> DistanceUnitOptions { get; private init; } = [];

    public IReadOnlyList<SettingsOptionViewModel> VolumeUnitOptions { get; private init; } = [];

    public IReadOnlyList<SettingsOptionViewModel> CurrencyOptions { get; private init; } = [];

    public bool CanConfigureAutomaticBackups => AutomaticBackupsEnabled;

    public string MaintenanceReminderDistanceLabel =>
        _localizer.Format("Settings.MaintenanceReminderDistance", CurrentDistanceUnitLabel);

    public string MaintenanceReminderDistanceName =>
        _localizer.Format("Settings.MaintenanceReminderDistanceName", CurrentDistanceUnitLabel);

    public string MaintenanceReminderDistanceHelp =>
        _localizer.Format("Settings.MaintenanceReminderDistanceHelp", CurrentDistanceUnitLabel);

    public static SettingsDialogViewModel FromSnapshot(
        DesktopSupportedSettingsSnapshot snapshot,
        string automaticBackupStatus,
        IAppLocalizer? localizer = null)
    {
        var effectiveLocalizer = localizer ?? new ResourceAppLocalizer();
        var languageOptions = BuildLanguageOptions(effectiveLocalizer);
        var thousandsOptions = BuildThousandsSeparatorOptions(effectiveLocalizer);
        var decimalOptions = BuildDecimalSeparatorOptions(effectiveLocalizer);
        var distanceUnitOptions = BuildDistanceUnitOptions(effectiveLocalizer);
        var volumeUnitOptions = BuildVolumeUnitOptions(effectiveLocalizer);
        var currencyOptions = BuildCurrencyOptions(effectiveLocalizer);

        var viewModel = new SettingsDialogViewModel
        {
            _localizer = effectiveLocalizer,
            TechnicalReminderDays = snapshot.TechnicalReminderDays.ToString(),
            GreenCardReminderDays = snapshot.GreenCardReminderDays.ToString(),
            MaintenanceReminderDays = snapshot.MaintenanceReminderDays.ToString(),
            RunAtStartup = snapshot.RunAtStartup,
            HideOnLaunch = snapshot.HideOnLaunch,
            ShowDashboardOnLaunch = snapshot.ShowDashboardOnLaunch,
            AutomaticBackupsEnabled = snapshot.AutomaticBackupsEnabled,
            AutomaticBackupIntervalDays = snapshot.AutomaticBackupIntervalDays.ToString(),
            AutomaticBackupKeepCount = snapshot.AutomaticBackupKeepCount.ToString(),
            AutomaticBackupStatus = automaticBackupStatus,
            LanguageOptions = languageOptions,
            ThousandsSeparatorOptions = thousandsOptions,
            DecimalSeparatorOptions = decimalOptions,
            DistanceUnitOptions = distanceUnitOptions,
            VolumeUnitOptions = volumeUnitOptions,
            CurrencyOptions = currencyOptions,
            SelectedLanguageOption = FindOption(languageOptions, AppCultureService.NormalizeLanguage(snapshot.Language)),
            SelectedThousandsSeparatorOption = FindOption(thousandsOptions, AppCultureService.NormalizeThousandsSeparator(snapshot.ThousandsSeparator)),
            SelectedDecimalSeparatorOption = FindOption(decimalOptions, AppCultureService.NormalizeDecimalSeparator(snapshot.DecimalSeparator)),
            SelectedDistanceUnitOption = FindOption(distanceUnitOptions, AppUnitFormatService.NormalizeDistanceUnit(snapshot.DistanceUnit)),
            SelectedVolumeUnitOption = FindOption(volumeUnitOptions, AppUnitFormatService.NormalizeVolumeUnit(snapshot.VolumeUnit)),
            SelectedCurrencyOption = FindOption(currencyOptions, AppCurrencyFormatService.NormalizeCurrency(snapshot.Currency)),
            StatusMessage = effectiveLocalizer.GetString("Settings.AutomaticBackupStatusInitial")
        };

        viewModel.SetMaintenanceReminderDistanceKilometers(snapshot.MaintenanceReminderKm);
        return viewModel;
    }

    public bool TryBuildSnapshot(out DesktopSupportedSettingsSnapshot snapshot, out string errorMessage)
    {
        if (!TryParseBoundedIntLocalized(TechnicalReminderDays, 0, 3650, _localizer.GetString("Settings.TechnicalReminderDaysName"), out var technicalReminderDays, out errorMessage)
            || !TryParseBoundedIntLocalized(GreenCardReminderDays, 0, 3650, _localizer.GetString("Settings.GreenCardReminderDaysName"), out var greenCardReminderDays, out errorMessage)
            || !TryParseBoundedIntLocalized(MaintenanceReminderDays, 0, 3650, _localizer.GetString("Settings.MaintenanceReminderDaysName"), out var maintenanceReminderDays, out errorMessage)
            || !TryValidateDistinctNumberSeparators(out errorMessage)
            || !TryParseMaintenanceReminderDistance(out var maintenanceReminderKm, out errorMessage))
        {
            snapshot = default!;
            return false;
        }

        var automaticBackupIntervalDays = 1;
        var automaticBackupKeepCount = 30;
        if (AutomaticBackupsEnabled
            && (!TryParseBoundedIntLocalized(AutomaticBackupIntervalDays, 1, 999, _localizer.GetString("Settings.AutomaticBackupsIntervalDays"), out automaticBackupIntervalDays, out errorMessage)
                || !TryParseBoundedIntLocalized(AutomaticBackupKeepCount, 1, 999, _localizer.GetString("Settings.AutomaticBackupKeepCount"), out automaticBackupKeepCount, out errorMessage)))
        {
            snapshot = default!;
            return false;
        }

        if (!AutomaticBackupsEnabled)
        {
            if (TryParseBoundedIntLocalized(AutomaticBackupIntervalDays, 1, 999, _localizer.GetString("Settings.AutomaticBackupsIntervalDays"), out var parsedIntervalDays, out _))
            {
                automaticBackupIntervalDays = parsedIntervalDays;
            }

            if (TryParseBoundedIntLocalized(AutomaticBackupKeepCount, 1, 999, _localizer.GetString("Settings.AutomaticBackupKeepCount"), out var parsedKeepCount, out _))
            {
                automaticBackupKeepCount = parsedKeepCount;
            }
        }

        snapshot = new DesktopSupportedSettingsSnapshot(
            technicalReminderDays,
            greenCardReminderDays,
            maintenanceReminderDays,
            maintenanceReminderKm,
            RunAtStartup,
            HideOnLaunch,
            ShowDashboardOnLaunch,
            AutomaticBackupsEnabled,
            automaticBackupIntervalDays,
            automaticBackupKeepCount,
            SelectedLanguageOption?.Value ?? AppCultureService.SystemLanguage,
            SelectedThousandsSeparatorOption?.Value ?? AppCultureService.CultureSeparator,
            SelectedDecimalSeparatorOption?.Value ?? AppCultureService.CultureSeparator,
            SelectedDistanceUnitOption?.Value ?? AppUnitFormatService.Kilometers,
            SelectedVolumeUnitOption?.Value ?? AppUnitFormatService.Liters,
            SelectedCurrencyOption?.Value ?? AppCurrencyFormatService.CzechCrowns);
        errorMessage = string.Empty;
        return true;
    }

    private bool TryValidateDistinctNumberSeparators(out string errorMessage)
    {
        var format = new AppNumberFormatService().CreateNumberFormat(BuildCulturePreferences());
        if (!string.IsNullOrEmpty(format.NumberGroupSeparator)
            && string.Equals(format.NumberGroupSeparator, format.NumberDecimalSeparator, StringComparison.Ordinal))
        {
            errorMessage = _localizer.GetString("Settings.Validation.SeparatorsMustDiffer");
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    partial void OnAutomaticBackupsEnabledChanged(bool value)
    {
        OnPropertyChanged(nameof(CanConfigureAutomaticBackups));
    }

    partial void OnSelectedLanguageOptionChanging(SettingsOptionViewModel? oldValue, SettingsOptionViewModel? newValue)
    {
        if (oldValue is not null && newValue is not null && !string.Equals(oldValue.Value, newValue.Value, StringComparison.Ordinal))
        {
            CaptureMaintenanceReminderDistanceKilometers(language: oldValue.Value);
        }
    }

    partial void OnSelectedLanguageOptionChanged(SettingsOptionViewModel? value)
    {
        RefreshMaintenanceReminderDistanceText();
    }

    partial void OnSelectedThousandsSeparatorOptionChanging(SettingsOptionViewModel? oldValue, SettingsOptionViewModel? newValue)
    {
        if (oldValue is not null && newValue is not null && !string.Equals(oldValue.Value, newValue.Value, StringComparison.Ordinal))
        {
            CaptureMaintenanceReminderDistanceKilometers(thousandsSeparator: oldValue.Value);
        }
    }

    partial void OnSelectedThousandsSeparatorOptionChanged(SettingsOptionViewModel? value)
    {
        RefreshMaintenanceReminderDistanceText();
    }

    partial void OnSelectedDecimalSeparatorOptionChanging(SettingsOptionViewModel? oldValue, SettingsOptionViewModel? newValue)
    {
        if (oldValue is not null && newValue is not null && !string.Equals(oldValue.Value, newValue.Value, StringComparison.Ordinal))
        {
            CaptureMaintenanceReminderDistanceKilometers(decimalSeparator: oldValue.Value);
        }
    }

    partial void OnSelectedDecimalSeparatorOptionChanged(SettingsOptionViewModel? value)
    {
        RefreshMaintenanceReminderDistanceText();
    }

    partial void OnSelectedDistanceUnitOptionChanging(SettingsOptionViewModel? oldValue, SettingsOptionViewModel? newValue)
    {
        if (oldValue is not null && newValue is not null && !string.Equals(oldValue.Value, newValue.Value, StringComparison.Ordinal))
        {
            CaptureMaintenanceReminderDistanceKilometers(distanceUnit: oldValue.Value);
        }
    }

    partial void OnSelectedDistanceUnitOptionChanged(SettingsOptionViewModel? value)
    {
        RefreshMaintenanceReminderDistanceText();
        NotifyMaintenanceReminderDistanceMetadataChanged();
    }

    private bool TryParseBoundedIntLocalized(string value, int minValue, int maxValue, string label, out int parsedValue, out string errorMessage)
    {
        if (!int.TryParse(value.Trim(), out parsedValue) || parsedValue < minValue || parsedValue > maxValue)
        {
            errorMessage = _localizer.Format("Settings.Validation.IntegerRange", label, minValue, maxValue);
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private bool TryParseMaintenanceReminderDistance(out int kilometers, out string errorMessage)
    {
        var culturePreferences = BuildCulturePreferences();
        var unitPreferences = BuildUnitPreferences();
        if (!_numberFormatService.TryParseDecimal(MaintenanceReminderKm, culturePreferences, out var distance)
            || distance <= 0m)
        {
            kilometers = 0;
            errorMessage = BuildMaintenanceReminderDistanceRangeMessage(culturePreferences, unitPreferences);
            return false;
        }

        var convertedKilometers = _unitFormatService.ConvertDistanceToKilometers(distance, unitPreferences);
        var roundedKilometers = (int)Math.Round(convertedKilometers, MidpointRounding.AwayFromZero);
        if (roundedKilometers < MinMaintenanceReminderKm || roundedKilometers > MaxMaintenanceReminderKm)
        {
            kilometers = 0;
            errorMessage = BuildMaintenanceReminderDistanceRangeMessage(culturePreferences, unitPreferences);
            return false;
        }

        kilometers = roundedKilometers;
        maintenanceReminderDistanceKilometers = roundedKilometers;
        errorMessage = string.Empty;
        return true;
    }

    private string BuildMaintenanceReminderDistanceRangeMessage(AppCulturePreferences culturePreferences, AppUnitPreferences unitPreferences)
    {
        var minValue = FormatMaintenanceReminderDistance(MinMaintenanceReminderKm, culturePreferences, unitPreferences);
        var maxValue = FormatMaintenanceReminderDistance(MaxMaintenanceReminderKm, culturePreferences, unitPreferences);
        return _localizer.Format("Settings.Validation.NumberRange", MaintenanceReminderDistanceName, minValue, maxValue);
    }

    private void SetMaintenanceReminderDistanceKilometers(int kilometers)
    {
        maintenanceReminderDistanceKilometers = Math.Clamp(kilometers, MinMaintenanceReminderKm, MaxMaintenanceReminderKm);
        RefreshMaintenanceReminderDistanceText();
    }

    private void CaptureMaintenanceReminderDistanceKilometers(
        string? language = null,
        string? thousandsSeparator = null,
        string? decimalSeparator = null,
        string? distanceUnit = null)
    {
        var culturePreferences = BuildCulturePreferences(language, thousandsSeparator, decimalSeparator);
        var unitPreferences = BuildUnitPreferences(distanceUnit);
        if (_numberFormatService.TryParseDecimal(MaintenanceReminderKm, culturePreferences, out var distance)
            && distance > 0m)
        {
            var convertedKilometers = _unitFormatService.ConvertDistanceToKilometers(distance, unitPreferences);
            if (convertedKilometers >= MinMaintenanceReminderKm && convertedKilometers <= MaxMaintenanceReminderKm)
            {
                maintenanceReminderDistanceKilometers = convertedKilometers;
            }
        }
    }

    private void RefreshMaintenanceReminderDistanceText()
    {
        MaintenanceReminderKm = FormatMaintenanceReminderDistance(maintenanceReminderDistanceKilometers, BuildCulturePreferences(), BuildUnitPreferences());
    }

    private string FormatMaintenanceReminderDistance(decimal kilometers, AppCulturePreferences culturePreferences, AppUnitPreferences unitPreferences)
    {
        var distance = _unitFormatService.ConvertDistanceFromKilometers(kilometers, unitPreferences);
        return _numberFormatService.FormatDecimal(distance, culturePreferences, GetDistanceDecimalPlaces(unitPreferences.DistanceUnit));
    }

    private AppCulturePreferences BuildCulturePreferences(
        string? language = null,
        string? thousandsSeparator = null,
        string? decimalSeparator = null) =>
        new(
            language ?? SelectedLanguageOption?.Value ?? AppCultureService.SystemLanguage,
            thousandsSeparator ?? SelectedThousandsSeparatorOption?.Value ?? AppCultureService.CultureSeparator,
            decimalSeparator ?? SelectedDecimalSeparatorOption?.Value ?? AppCultureService.CultureSeparator);

    private AppUnitPreferences BuildUnitPreferences(string? distanceUnit = null) =>
        new(
            distanceUnit ?? SelectedDistanceUnitOption?.Value ?? AppUnitFormatService.Kilometers,
            SelectedVolumeUnitOption?.Value ?? AppUnitFormatService.Liters);

    private string CurrentDistanceUnitLabel =>
        string.Equals(AppUnitFormatService.NormalizeDistanceUnit(SelectedDistanceUnitOption?.Value), AppUnitFormatService.Miles, StringComparison.Ordinal)
            ? "mi"
            : "km";

    private void NotifyMaintenanceReminderDistanceMetadataChanged()
    {
        OnPropertyChanged(nameof(MaintenanceReminderDistanceLabel));
        OnPropertyChanged(nameof(MaintenanceReminderDistanceName));
        OnPropertyChanged(nameof(MaintenanceReminderDistanceHelp));
    }

    private static int GetDistanceDecimalPlaces(string distanceUnit) =>
        string.Equals(AppUnitFormatService.NormalizeDistanceUnit(distanceUnit), AppUnitFormatService.Miles, StringComparison.Ordinal)
            ? 1
            : 0;

    private static IReadOnlyList<SettingsOptionViewModel> BuildLanguageOptions(IAppLocalizer localizer) =>
        [
            new(AppCultureService.SystemLanguage, localizer.GetString("Settings.Option.System")),
            new(AppCultureService.CzechLanguage, localizer.GetString("Settings.Option.Czech")),
            new(AppCultureService.EnglishLanguage, localizer.GetString("Settings.Option.English"))
        ];

    private static IReadOnlyList<SettingsOptionViewModel> BuildThousandsSeparatorOptions(IAppLocalizer localizer) =>
        [
            new(AppCultureService.CultureSeparator, localizer.GetString("Settings.Option.Culture")),
            new(AppCultureService.SpaceSeparator, localizer.GetString("Settings.Option.ThousandsSpace")),
            new(AppCultureService.CommaSeparator, localizer.GetString("Settings.Option.ThousandsComma")),
            new(AppCultureService.DotSeparator, localizer.GetString("Settings.Option.ThousandsDot")),
            new(AppCultureService.NoSeparator, localizer.GetString("Settings.Option.NoThousands"))
        ];

    private static IReadOnlyList<SettingsOptionViewModel> BuildDecimalSeparatorOptions(IAppLocalizer localizer) =>
        [
            new(AppCultureService.CultureSeparator, localizer.GetString("Settings.Option.Culture")),
            new(AppCultureService.CommaSeparator, localizer.GetString("Settings.Option.DecimalComma")),
            new(AppCultureService.DotSeparator, localizer.GetString("Settings.Option.DecimalDot"))
        ];

    private static IReadOnlyList<SettingsOptionViewModel> BuildDistanceUnitOptions(IAppLocalizer localizer) =>
        [
            new(AppUnitFormatService.Kilometers, localizer.GetString("Settings.Option.Kilometers")),
            new(AppUnitFormatService.Miles, localizer.GetString("Settings.Option.Miles"))
        ];

    private static IReadOnlyList<SettingsOptionViewModel> BuildVolumeUnitOptions(IAppLocalizer localizer) =>
        [
            new(AppUnitFormatService.Liters, localizer.GetString("Settings.Option.Liters")),
            new(AppUnitFormatService.UsGallons, localizer.GetString("Settings.Option.UsGallons")),
            new(AppUnitFormatService.ImperialGallons, localizer.GetString("Settings.Option.ImperialGallons"))
        ];

    private static IReadOnlyList<SettingsOptionViewModel> BuildCurrencyOptions(IAppLocalizer localizer) =>
        [
            new(AppCurrencyFormatService.CzechCrowns, localizer.GetString("Settings.Option.CurrencyCzk")),
            new(AppCurrencyFormatService.UsDollars, localizer.GetString("Settings.Option.CurrencyUsd")),
            new(AppCurrencyFormatService.Euros, localizer.GetString("Settings.Option.CurrencyEur")),
            new(AppCurrencyFormatService.BritishPounds, localizer.GetString("Settings.Option.CurrencyGbp"))
        ];

    private static SettingsOptionViewModel FindOption(IReadOnlyList<SettingsOptionViewModel> options, string value) =>
        options.FirstOrDefault(option => string.Equals(option.Value, value, StringComparison.OrdinalIgnoreCase))
        ?? options[0];
}
