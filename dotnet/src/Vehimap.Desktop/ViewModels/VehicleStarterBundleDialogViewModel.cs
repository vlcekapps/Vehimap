using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class VehicleStarterBundleDialogViewModel : ObservableObject
{
    public VehicleStarterBundleDialogViewModel(VehicleStarterBundlePreview preview)
    {
        VehicleId = preview.VehicleId;
        VehicleName = preview.VehicleName;
        ProfileLabel = string.IsNullOrWhiteSpace(preview.ProfileLabel) ? "Bez doplňujícího profilu" : preview.ProfileLabel;
        Items = new ObservableCollection<VehicleStarterBundleItemEditorViewModel>(preview.Items.Select(item => new VehicleStarterBundleItemEditorViewModel(item)));
        Items.CollectionChanged += OnItemsCollectionChanged;

        foreach (var item in Items)
        {
            item.PropertyChanged += OnItemPropertyChanged;
        }

        SelectedItem = Items.FirstOrDefault();
        RefreshSummary();
    }

    public string VehicleId { get; }

    public string VehicleName { get; }

    public string ProfileLabel { get; }

    public ObservableCollection<VehicleStarterBundleItemEditorViewModel> Items { get; }

    [ObservableProperty]
    private VehicleStarterBundleItemEditorViewModel? selectedItem;

    [ObservableProperty]
    private string summaryText = string.Empty;

    public bool CanApply => Items.Any(item => item.IsSelected);

    public ICommand SelectAllCommand => new RelayCommand(SelectAll);

    public ICommand ClearSelectionCommand => new RelayCommand(ClearSelection);

    public IReadOnlyList<VehicleStarterBundleTemplate> BuildSelectedTemplates() =>
        Items
            .Where(item => item.IsSelected)
            .Select(item => item.ToTemplate())
            .ToList();

    private void SelectAll()
    {
        foreach (var item in Items)
        {
            item.IsSelected = true;
        }

        RefreshSummary();
    }

    private void ClearSelection()
    {
        foreach (var item in Items)
        {
            item.IsSelected = false;
        }

        RefreshSummary();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (VehicleStarterBundleItemEditorViewModel item in e.OldItems)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (VehicleStarterBundleItemEditorViewModel item in e.NewItems)
            {
                item.PropertyChanged += OnItemPropertyChanged;
            }
        }

        RefreshSummary();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(VehicleStarterBundleItemEditorViewModel.IsSelected))
        {
            OnPropertyChanged(nameof(CanApply));
        }

        RefreshSummary();
    }

    private void RefreshSummary()
    {
        var selectedItems = Items.Where(item => item.IsSelected).ToList();
        var maintenanceCount = selectedItems.Count(item => item.Section == VehicleStarterBundleSection.Maintenance);
        var recordCount = selectedItems.Count(item => item.Section == VehicleStarterBundleSection.Record);
        var reminderCount = selectedItems.Count(item => item.Section == VehicleStarterBundleSection.Reminder);
        SummaryText = selectedItems.Count == 0
            ? "Není vybraná žádná položka."
            : $"Vybráno: {selectedItems.Count} položek | Servis {maintenanceCount} | Doklady {recordCount} | Připomínky {reminderCount}";
        OnPropertyChanged(nameof(CanApply));
    }
}
