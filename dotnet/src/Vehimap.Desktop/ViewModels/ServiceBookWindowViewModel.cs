using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class ServiceBookWindowViewModel : ObservableObject
{
    private readonly Func<ServiceBookItemViewModel?, bool> _openItem;
    private readonly Func<ServiceBookWindowViewModel, Task<string>> _exportHtml;
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
        Func<ServiceBookWindowViewModel, Task<string>> exportHtml)
    {
        Summary = summary;
        _openItem = openItem;
        _exportHtml = exportHtml;
        WindowTitle = $"Servisní knížka - {summary.VehicleName}";
        VehicleSummary = $"{summary.VehicleMakeModel} | {summary.VehicleCategory} | SPZ {summary.VehiclePlate} | Tachometr {summary.CurrentOdometer}";
        CostSummary = $"Součet číselných částek v historii: {summary.TotalHistoryCost:0.00} Kč.";
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
            ? "Vyberte položku servisní knížky a můžete otevřít související evidenci."
            : SelectedServiceBookItem.AccessibleLabel;

    public bool CanOpenSelectedServiceBookItem => SelectedServiceBookItem is not null;

    public bool DidOpenSelectedItem { get; private set; }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedServiceBookItem))]
    private void OpenSelectedServiceBookItem()
    {
        if (!_openItem(SelectedServiceBookItem))
        {
            StatusText = "Související položku se nepodařilo otevřít.";
            return;
        }

        DidOpenSelectedItem = true;
        StatusText = "Související položka byla otevřena v hlavním okně.";
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
}
