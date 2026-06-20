using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application.Models;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class VehicleStarterBundleDialogViewModel : ObservableObject
{
    private readonly bool _showSectionCounts;
    private readonly string _emptySelectionText;

    public VehicleStarterBundleDialogViewModel(VehicleStarterBundlePreview preview)
        : this(
            preview,
            "Balíček pro vozidlo",
            "Dialog pro výběr servisních plánů, dokladů a připomínek pro vozidlo. Ctrl+S přidá vybrané položky, Ctrl+A vybere vše, Ctrl+Shift+A výběr vymaže a Escape zavře dialog.",
            "Doporučené položky",
            "Seznam položek balíčku",
            "Vyberte položku vlevo a můžete upravit její obsah před přidáním.",
            "Není vybraná žádná položka.",
            showSectionCounts: true)
    {
    }

    private VehicleStarterBundleDialogViewModel(
        VehicleStarterBundlePreview preview,
        string dialogTitle,
        string dialogHelpText,
        string itemsHeading,
        string itemsListName,
        string detailHint,
        string emptySelectionText,
        bool showSectionCounts)
    {
        DialogTitle = dialogTitle;
        DialogHelpText = dialogHelpText;
        ItemsHeading = itemsHeading;
        ItemsListName = itemsListName;
        DetailHint = detailHint;
        _emptySelectionText = emptySelectionText;
        _showSectionCounts = showSectionCounts;
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

    public static VehicleStarterBundleDialogViewModel CreateMaintenanceTemplates(VehicleStarterBundlePreview preview) =>
        new(
            preview,
            "Doporučené servisní šablony",
            "Dialog pro výběr doporučených servisních plánů podle kategorie a servisního profilu vozidla. Ctrl+S přidá vybrané šablony, Ctrl+A vybere vše, Ctrl+Shift+A výběr vymaže a Escape zavře dialog.",
            "Servisní šablony",
            "Seznam doporučených servisních šablon",
            "Vyberte šablonu vlevo a můžete upravit její intervaly nebo poznámku před přidáním.",
            "Není vybraná žádná servisní šablona.",
            showSectionCounts: false);

    public string DialogTitle { get; }

    public string DialogHelpText { get; }

    public string ItemsHeading { get; }

    public string ItemsListName { get; }

    public string DetailHint { get; }

    public string VehicleId { get; }

    public string VehicleName { get; }

    public string ProfileLabel { get; }

    public ObservableCollection<VehicleStarterBundleItemEditorViewModel> Items { get; }

    public IReadOnlyList<string> RecordTypeOptions => LegacyKnownValues.RecordTypes;

    public IReadOnlyList<string> ReminderRepeatModeOptions => LegacyKnownValues.ReminderRepeatModes;

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
            ? _emptySelectionText
            : _showSectionCounts
                ? $"Vybráno: {selectedItems.Count} položek | Servis {maintenanceCount} | Doklady {recordCount} | Připomínky {reminderCount}"
                : $"Vybráno: {maintenanceCount} servisních šablon.";
        OnPropertyChanged(nameof(CanApply));
    }
}
