using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Models;
using Vehimap.Application.Services;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class ServiceBookWindowViewModel : ObservableObject
{
    private readonly Func<ServiceBookItemViewModel?, bool> _openItem;
    private readonly Func<ServiceBookWindowViewModel, Task<string>> _exportHtml;
    private readonly IAppLocalizer _localizer;
    private readonly IAppNumberFormatService _numberFormatService;
    private readonly AppCulturePreferences _culturePreferences;
    private readonly string _currency;
    private bool _syncingSelection;
    private ServiceBookItemViewModel? _selectedHistoryItem;
    private ServiceBookItemViewModel? _selectedMaintenanceItem;
    private ServiceBookItemViewModel? _selectedRecordItem;

    public ServiceBookWindowViewModel(
        ServiceBookSummary summary,
        IReadOnlyList<ServiceBookItemViewModel> historyItems,
        IReadOnlyList<ServiceBookItemViewModel> maintenanceItems,
        IReadOnlyList<ServiceBookItemViewModel> recordItems,
        Func<ServiceBookItemViewModel?, bool> openItem,
        Func<ServiceBookWindowViewModel, Task<string>> exportHtml,
        IAppLocalizer? localizer = null,
        DesktopSupportedSettingsSnapshot? supportedSettings = null,
        IAppNumberFormatService? numberFormatService = null)
    {
        Summary = summary;
        _openItem = openItem;
        _exportHtml = exportHtml;
        _localizer = localizer ?? new ResourceAppLocalizer(CultureInfo.GetCultureInfo(AppCultureService.CzechLanguage));
        _numberFormatService = numberFormatService ?? new AppNumberFormatService();
        var settings = supportedSettings ?? new DesktopSupportedSettingsSnapshot(30, 30, 31, 1000, false, false, false, false, 1, 30);
        _culturePreferences = new AppCulturePreferences(settings.Language, settings.ThousandsSeparator, settings.DecimalSeparator);
        _currency = AppCurrencyFormatService.NormalizeCurrency(settings.Currency);
        WindowTitle = LF("ServiceBook.Window.TitleWithVehicle", summary.VehicleName);
        VehicleSummary = LF("ServiceBook.Window.VehicleSummary", summary.VehicleMakeModel, summary.VehicleCategory, summary.VehiclePlate, summary.CurrentOdometer);
        CostSummary = LF("ServiceBook.Window.CostSummary", FormatMoney(summary.TotalHistoryCost));
        StatusText = summary.Status;

        foreach (var item in historyItems)
        {
            HistoryItems.Add(item);
        }

        foreach (var item in maintenanceItems)
        {
            MaintenanceItems.Add(item);
        }

        foreach (var item in recordItems)
        {
            RecordItems.Add(item);
        }

        SelectedHistoryItem = HistoryItems.FirstOrDefault();
        if (SelectedHistoryItem is null)
        {
            SelectedMaintenanceItem = MaintenanceItems.FirstOrDefault();
        }

        if (SelectedServiceBookItem is null)
        {
            SelectedRecordItem = RecordItems.FirstOrDefault();
        }
    }

    public event Action? CloseRequested;

    public ServiceBookSummary Summary { get; }

    public string WindowTitle { get; }

    public string VehicleSummary { get; }

    public string CostSummary { get; }

    [ObservableProperty]
    private string statusText;

    public ObservableCollection<ServiceBookItemViewModel> HistoryItems { get; } = [];

    public ObservableCollection<ServiceBookItemViewModel> MaintenanceItems { get; } = [];

    public ObservableCollection<ServiceBookItemViewModel> RecordItems { get; } = [];

    public bool HasHistoryItems => HistoryItems.Count > 0;

    public bool HasMaintenanceItems => MaintenanceItems.Count > 0;

    public bool HasRecordItems => RecordItems.Count > 0;

    public ServiceBookItemViewModel? SelectedHistoryItem
    {
        get => _selectedHistoryItem;
        set
        {
            if (SetProperty(ref _selectedHistoryItem, value) && value is not null)
            {
                ClearOtherSelections(ServiceBookSection.History);
            }
        }
    }

    public ServiceBookItemViewModel? SelectedMaintenanceItem
    {
        get => _selectedMaintenanceItem;
        set
        {
            if (SetProperty(ref _selectedMaintenanceItem, value) && value is not null)
            {
                ClearOtherSelections(ServiceBookSection.Maintenance);
            }
        }
    }

    public ServiceBookItemViewModel? SelectedRecordItem
    {
        get => _selectedRecordItem;
        set
        {
            if (SetProperty(ref _selectedRecordItem, value) && value is not null)
            {
                ClearOtherSelections(ServiceBookSection.Record);
            }
        }
    }

    public ServiceBookItemViewModel? SelectedServiceBookItem =>
        SelectedHistoryItem ?? SelectedMaintenanceItem ?? SelectedRecordItem;

    public string SelectedItemDetail =>
        SelectedServiceBookItem is null
            ? L("ServiceBook.Window.SelectedItemEmpty")
            : SelectedServiceBookItem.AccessibleLabel;

    public bool CanOpenSelectedServiceBookItem => SelectedServiceBookItem is not null;

    public bool DidOpenSelectedItem { get; private set; }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedServiceBookItem))]
    private void OpenSelectedServiceBookItem()
    {
        if (!_openItem(SelectedServiceBookItem))
        {
            StatusText = L("ServiceBook.Window.OpenRelatedFailed");
            return;
        }

        DidOpenSelectedItem = true;
        StatusText = L("ServiceBook.Window.OpenRelatedSuccess");
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private async Task ExportHtmlAsync()
    {
        StatusText = await _exportHtml(this).ConfigureAwait(true);
    }

    private void ClearOtherSelections(ServiceBookSection selectedSection)
    {
        if (_syncingSelection)
        {
            return;
        }

        _syncingSelection = true;
        try
        {
            if (selectedSection != ServiceBookSection.History)
            {
                SelectedHistoryItem = null;
            }

            if (selectedSection != ServiceBookSection.Maintenance)
            {
                SelectedMaintenanceItem = null;
            }

            if (selectedSection != ServiceBookSection.Record)
            {
                SelectedRecordItem = null;
            }
        }
        finally
        {
            _syncingSelection = false;
        }

        OnPropertyChanged(nameof(SelectedServiceBookItem));
        OnPropertyChanged(nameof(SelectedItemDetail));
        OnPropertyChanged(nameof(CanOpenSelectedServiceBookItem));
        OpenSelectedServiceBookItemCommand.NotifyCanExecuteChanged();
    }

    private enum ServiceBookSection
    {
        History,
        Maintenance,
        Record
    }

    private string FormatMoney(decimal value) =>
        _numberFormatService.FormatMoney(value, _culturePreferences, _currency);

    private string L(string key) => _localizer.GetString(key);

    private string LF(string key, params object?[] args) => _localizer.Format(key, args);
}
