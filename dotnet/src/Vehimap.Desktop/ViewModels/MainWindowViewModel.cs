using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vehimap.Application;
using Vehimap.Application.Abstractions;
using Vehimap.Application.Services;
using Vehimap.Domain.Enums;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly LegacyVehimapBootstrapper _bootstrapper;
    private readonly IFileAttachmentService _attachmentService;
    private readonly IFileLauncher _fileLauncher;
    private readonly IAuditService _auditService;
    private readonly ICostAnalysisService _costAnalysisService;
    private readonly Dictionary<string, VehicleMeta> _metaByVehicleId = new(StringComparer.Ordinal);

    private VehimapDataRoot? _dataRoot;
    private VehimapDataSet _dataSet = new();
    private IReadOnlyList<AuditItem> _auditItems = [];

    [ObservableProperty]
    private string title = "Vehimap Desktop Preview";

    [ObservableProperty]
    private string subtitle = "První Avalonia shell nad legacy daty Vehimap.";

    [ObservableProperty]
    private string dataMode = string.Empty;

    [ObservableProperty]
    private string dataPath = string.Empty;

    [ObservableProperty]
    private string loadError = string.Empty;

    [ObservableProperty]
    private int vehicleCount;

    [ObservableProperty]
    private int historyCount;

    [ObservableProperty]
    private int fuelCount;

    [ObservableProperty]
    private int recordsCount;

    [ObservableProperty]
    private int remindersCount;

    [ObservableProperty]
    private int maintenanceCount;

    [ObservableProperty]
    private int auditCount;

    [ObservableProperty]
    private string auditSummary = string.Empty;

    [ObservableProperty]
    private string costSummary = string.Empty;

    [ObservableProperty]
    private string costComparison = string.Empty;

    [ObservableProperty]
    private string selectedVehicleHeading = "Nevybrané vozidlo";

    [ObservableProperty]
    private string selectedVehicleOverview = "Vyberte vozidlo vlevo a zobrazí se jeho základní souhrn.";

    [ObservableProperty]
    private string selectedVehicleDates = string.Empty;

    [ObservableProperty]
    private string selectedVehicleProfile = string.Empty;

    [ObservableProperty]
    private string historySummary = "Historie vybraného vozidla se zobrazí po výběru vozidla.";

    [ObservableProperty]
    private string recordSummary = "Doklady a přílohy vybraného vozidla se zobrazí po výběru vozidla.";

    [ObservableProperty]
    private string selectedRecordDetail = string.Empty;

    [ObservableProperty]
    private VehicleListItemViewModel? selectedVehicle;

    [ObservableProperty]
    private VehicleRecordItemViewModel? selectedRecord;

    public ObservableCollection<VehicleListItemViewModel> Vehicles { get; } = [];

    public ObservableCollection<AuditItemViewModel> AuditItems { get; } = [];

    public ObservableCollection<CostVehicleItemViewModel> CostVehicles { get; } = [];

    public ObservableCollection<VehicleHistoryItemViewModel> SelectedVehicleHistory { get; } = [];

    public ObservableCollection<VehicleRecordItemViewModel> SelectedVehicleRecords { get; } = [];

    public MainWindowViewModel()
        : this(
            new LegacyVehimapBootstrapper(new LegacyDataRootLocator(), new LegacyVehimapDataStore()),
            new ManagedAttachmentPathService(),
            new ProcessFileLauncher())
    {
    }

    internal MainWindowViewModel(
        LegacyVehimapBootstrapper bootstrapper,
        IFileAttachmentService attachmentService,
        IFileLauncher fileLauncher)
    {
        _bootstrapper = bootstrapper;
        _attachmentService = attachmentService;
        _fileLauncher = fileLauncher;
        _auditService = new LegacyAuditService(_attachmentService);
        _costAnalysisService = new LegacyCostAnalysisService();
        Load();
    }

    public bool CanOpenSelectedRecordFile => SelectedRecord is { FileExists: true } && !string.IsNullOrWhiteSpace(SelectedRecord.ResolvedPath);

    public bool CanOpenSelectedRecordFolder =>
        SelectedRecord is not null
        && !string.IsNullOrWhiteSpace(GetSelectedRecordFolderPath());

    partial void OnSelectedVehicleChanged(VehicleListItemViewModel? value)
    {
        if (value is null)
        {
            SelectedVehicleHeading = "Nevybrané vozidlo";
            SelectedVehicleOverview = "Vyberte vozidlo vlevo a zobrazí se jeho základní souhrn.";
            SelectedVehicleDates = string.Empty;
            SelectedVehicleProfile = string.Empty;
            HistorySummary = "Historie vybraného vozidla se zobrazí po výběru vozidla.";
            RecordSummary = "Doklady a přílohy vybraného vozidla se zobrazí po výběru vozidla.";
            SelectedVehicleHistory.Clear();
            SelectedVehicleRecords.Clear();
            SelectedRecord = null;
            return;
        }

        var state = string.IsNullOrWhiteSpace(value.State) ? "Běžný provoz" : value.State;
        var note = string.IsNullOrWhiteSpace(value.VehicleNote) ? "Bez poznámky" : value.VehicleNote;
        var powertrain = string.IsNullOrWhiteSpace(value.Powertrain) ? "Servisní profil zatím nevyplněn" : value.Powertrain;

        SelectedVehicleHeading = value.Name;
        SelectedVehicleOverview = $"{value.MakeModel} | {value.Category} | {value.Plate}\nStav: {state}\nPoznámka: {note}";
        SelectedVehicleDates = $"Příští TK: {FormatValue(value.NextTk, "nevyplněno")}\nZelená karta do: {FormatValue(value.GreenCardTo, "nevyplněno")}\nSouhrnný stav: {FormatValue(value.StatusSummary, "bez upozornění")}";
        SelectedVehicleProfile = $"Pohon a servisní profil: {powertrain}";

        PopulateVehicleHistory(value.Id);
        PopulateVehicleRecords(value.Id);
    }

    partial void OnSelectedRecordChanged(VehicleRecordItemViewModel? value)
    {
        SelectedRecordDetail = value is null
            ? "Vyberte doklad a zobrazí se detail přílohy."
            : $"Typ: {value.RecordType}\nPlatnost: {value.Validity}\nCena: {value.Price}\nRežim přílohy: {value.AttachmentMode}\nStav přílohy: {value.AttachmentState}\nUložená cesta: {FormatValue(value.StoredPath, "nevyplněno")}\nVyřešená cesta: {FormatValue(value.ResolvedPath, "nevyplněno")}\nPoznámka: {FormatValue(value.Note, "bez poznámky")}";

        OpenSelectedRecordFileCommand.NotifyCanExecuteChanged();
        OpenSelectedRecordFolderCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void Reload()
    {
        Load();
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedRecordFile))]
    private async Task OpenSelectedRecordFileAsync()
    {
        if (!CanOpenSelectedRecordFile || SelectedRecord is null)
        {
            return;
        }

        await _fileLauncher.OpenAsync(SelectedRecord.ResolvedPath).ConfigureAwait(false);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedRecordFolder))]
    private async Task OpenSelectedRecordFolderAsync()
    {
        var folderPath = GetSelectedRecordFolderPath();
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return;
        }

        await _fileLauncher.OpenFolderAsync(folderPath).ConfigureAwait(false);
    }

    private void Load()
    {
        try
        {
            var result = _bootstrapper.LoadAsync(AppContext.BaseDirectory).GetAwaiter().GetResult();
            _dataRoot = result.DataRoot;
            _dataSet = result.DataSet;
            _auditItems = _auditService.BuildAudit(result.DataRoot, result.DataSet);
            var costSummary = _costAnalysisService.BuildYearToDateSummary(result.DataSet, DateOnly.FromDateTime(DateTime.Today));

            LoadError = string.Empty;
            DataMode = result.DataRoot.IsPortable ? "Portable data vedle aplikace" : "Systémová datová složka";
            DataPath = result.DataRoot.DataPath;
            VehicleCount = result.DataSet.Vehicles.Count;
            HistoryCount = result.DataSet.HistoryEntries.Count;
            FuelCount = result.DataSet.FuelEntries.Count;
            RecordsCount = result.DataSet.Records.Count;
            RemindersCount = result.DataSet.Reminders.Count;
            MaintenanceCount = result.DataSet.MaintenancePlans.Count;
            AuditCount = _auditItems.Count;
            AuditSummary = BuildAuditSummary(_auditItems);
            CostSummary = BuildCostSummary(costSummary);
            CostComparison = BuildCostComparison(costSummary);

            _metaByVehicleId.Clear();
            foreach (var meta in result.DataSet.VehicleMetaEntries.GroupBy(item => item.VehicleId, StringComparer.Ordinal))
            {
                _metaByVehicleId[meta.Key] = meta.First();
            }

            Vehicles.Clear();
            foreach (var vehicle in result.DataSet.Vehicles.OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                var meta = _metaByVehicleId.GetValueOrDefault(vehicle.Id);
                Vehicles.Add(new VehicleListItemViewModel(
                    vehicle.Id,
                    vehicle.Name,
                    vehicle.Category,
                    FormatValue(vehicle.Plate, "Bez SPZ"),
                    FormatValue(vehicle.MakeModel, "Bez značky / modelu"),
                    vehicle.VehicleNote,
                    vehicle.NextTk,
                    vehicle.GreenCardTo,
                    meta?.State ?? string.Empty,
                    meta?.Powertrain ?? string.Empty,
                    BuildVehicleStatusSummary(vehicle, meta, _auditItems)));
            }

            AuditItems.Clear();
            foreach (var item in _auditItems.Take(8))
            {
                AuditItems.Add(new AuditItemViewModel(
                    item.Severity switch
                    {
                        AuditSeverity.Error => "Chyba",
                        AuditSeverity.Warning => "Upozornění",
                        _ => "Info"
                    },
                    item.Category,
                    item.VehicleName,
                    item.Title,
                    item.Message));
            }

            CostVehicles.Clear();
            foreach (var row in costSummary.Vehicles.Where(item => item.TotalCost > 0m || item.Status != "Neaktivní").Take(8))
            {
                CostVehicles.Add(new CostVehicleItemViewModel(
                    row.VehicleName,
                    row.Category,
                    FormatMoney(row.TotalCost),
                    row.DistanceKm.HasValue ? $"{row.DistanceKm.Value} km" : "nedostupné",
                    row.CostPerKm.HasValue ? $"{row.CostPerKm.Value:0.00} Kč/km" : "nedostupné",
                    row.Status));
            }

            SelectedVehicle = Vehicles.FirstOrDefault();
            if (SelectedVehicle is null)
            {
                OnSelectedVehicleChanged(null);
            }
        }
        catch (Exception ex)
        {
            LoadError = ex.Message;
        }
    }

    private void PopulateVehicleHistory(string vehicleId)
    {
        SelectedVehicleHistory.Clear();

        var items = _dataSet.HistoryEntries
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => new
            {
                Item = item,
                HasDate = VehimapValueParser.TryParseEventDate(item.EventDate, out var parsedDate),
                Date = parsedDate
            })
            .OrderByDescending(item => item.HasDate)
            .ThenByDescending(item => item.Date)
            .ThenBy(item => item.Item.EventType, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        foreach (var item in items)
        {
            SelectedVehicleHistory.Add(new VehicleHistoryItemViewModel(
                FormatValue(item.Item.EventDate, "bez data"),
                FormatValue(item.Item.EventType, "bez typu"),
                FormatValue(item.Item.Odometer, "bez tachometru"),
                FormatValue(item.Item.Cost, "bez ceny"),
                FormatValue(item.Item.Note, "bez poznámky")));
        }

        HistorySummary = SelectedVehicleHistory.Count == 0
            ? "Vybrané vozidlo zatím nemá žádné záznamy v historii."
            : $"Vybrané vozidlo má {SelectedVehicleHistory.Count} historických záznamů.";
    }

    private void PopulateVehicleRecords(string vehicleId)
    {
        SelectedVehicleRecords.Clear();

        var items = _dataSet.Records
            .Where(item => item.VehicleId == vehicleId)
            .Select(item => BuildVehicleRecordItem(item))
            .OrderBy(item => item.Validity, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        foreach (var item in items)
        {
            SelectedVehicleRecords.Add(item);
        }

        RecordSummary = SelectedVehicleRecords.Count == 0
            ? "Vybrané vozidlo zatím nemá žádné doklady."
            : $"Vybrané vozidlo má {SelectedVehicleRecords.Count} dokladů. Vyberte záznam a můžete otevřít soubor nebo jeho složku.";

        SelectedRecord = SelectedVehicleRecords.FirstOrDefault();
        if (SelectedRecord is null)
        {
            OnSelectedRecordChanged(null);
        }
    }

    private VehicleRecordItemViewModel BuildVehicleRecordItem(VehicleRecord record)
    {
        var resolvedPath = ResolveRecordPath(record);
        var fileExists = !string.IsNullOrWhiteSpace(resolvedPath) && File.Exists(resolvedPath);

        return new VehicleRecordItemViewModel(
            record.Id,
            FormatValue(record.RecordType, "Doklad"),
            FormatValue(record.Title, "Bez názvu"),
            FormatValue(record.Provider, "Bez poskytovatele"),
            BuildRecordValidity(record),
            FormatValue(record.Price, "bez ceny"),
            record.AttachmentMode == VehicleRecordAttachmentMode.Managed ? "Spravovaná kopie" : "Externí cesta",
            BuildAttachmentState(record, resolvedPath, fileExists),
            record.FilePath,
            resolvedPath,
            fileExists,
            record.Note);
    }

    private string ResolveRecordPath(VehicleRecord record)
    {
        if (_dataRoot is null || string.IsNullOrWhiteSpace(record.FilePath))
        {
            return string.Empty;
        }

        if (record.AttachmentMode == VehicleRecordAttachmentMode.Managed)
        {
            return _attachmentService.ResolveManagedAttachmentPath(_dataRoot, record.FilePath);
        }

        return Path.IsPathRooted(record.FilePath)
            ? record.FilePath
            : Path.GetFullPath(Path.Combine(_dataRoot.AppBasePath, record.FilePath));
    }

    private static string BuildRecordValidity(VehicleRecord record)
    {
        var from = string.IsNullOrWhiteSpace(record.ValidFrom) ? "od nevyplněno" : $"od {record.ValidFrom}";
        var to = string.IsNullOrWhiteSpace(record.ValidTo) ? "do nevyplněno" : $"do {record.ValidTo}";
        return $"{from} | {to}";
    }

    private static string BuildAttachmentState(VehicleRecord record, string resolvedPath, bool fileExists)
    {
        if (string.IsNullOrWhiteSpace(record.FilePath))
        {
            return "Bez cesty";
        }

        if (fileExists)
        {
            return "Soubor dostupný";
        }

        return record.AttachmentMode == VehicleRecordAttachmentMode.Managed
            ? "Chybí spravovaná příloha"
            : string.IsNullOrWhiteSpace(resolvedPath) ? "Cesta nevyřešena" : "Chybí externí příloha";
    }

    private string? GetSelectedRecordFolderPath()
    {
        if (SelectedRecord is null)
        {
            return null;
        }

        if (SelectedRecord.FileExists)
        {
            return Path.GetDirectoryName(SelectedRecord.ResolvedPath);
        }

        if (!string.IsNullOrWhiteSpace(SelectedRecord.ResolvedPath))
        {
            return Path.GetDirectoryName(SelectedRecord.ResolvedPath);
        }

        return null;
    }

    private static string BuildAuditSummary(IReadOnlyCollection<AuditItem> audit)
    {
        if (audit.Count == 0)
        {
            return "Audit zatím nenašel žádné problémy, které by potřebovaly zásah.";
        }

        var errorCount = audit.Count(item => item.Severity == AuditSeverity.Error);
        var warningCount = audit.Count(item => item.Severity == AuditSeverity.Warning);
        return $"K řešení je {audit.Count} položek: {errorCount} chyb a {warningCount} upozornění.";
    }

    private static string BuildCostSummary(CostAnalysisSummary summary)
    {
        var costPerKmText = summary.CostPerKm.HasValue ? $"{summary.CostPerKm.Value:0.00} Kč/km" : "nedostupné";
        var distanceText = summary.DistanceKm.HasValue ? $"{summary.DistanceKm.Value} km" : "nedostupné";
        return $"{summary.PeriodLabel}\nCelkem: {FormatMoney(summary.TotalCost)} | Ujeto: {distanceText} | Cena / km: {costPerKmText}\nBez číselného nákladu: {summary.ActiveWithoutCostCount} z {summary.ActiveVehicleCount} aktivních vozidel.";
    }

    private static string BuildCostComparison(CostAnalysisSummary summary)
    {
        var totalDelta = summary.TotalCostDifference >= 0m
            ? $"+{summary.TotalCostDifference:0.00} Kč"
            : $"{summary.TotalCostDifference:0.00} Kč";

        var costDelta = summary.CostPerKmDifference.HasValue
            ? (summary.CostPerKmDifference.Value >= 0m
                ? $"+{summary.CostPerKmDifference.Value:0.00} Kč/km"
                : $"{summary.CostPerKmDifference.Value:0.00} Kč/km")
            : "nedostupné";

        return $"Proti stejně dlouhému období loni: náklady {totalDelta}, cena / km {costDelta}. U {summary.CostPerKmUnavailableCount} vozidel s náklady zatím chybí spolehlivý výpočet ceny za kilometr.";
    }

    private static string BuildVehicleStatusSummary(Vehicle vehicle, VehicleMeta? meta, IReadOnlyCollection<AuditItem> audit)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(meta?.State))
        {
            parts.Add(meta.State);
        }

        var attentionCount = audit.Count(item => item.VehicleId == vehicle.Id);
        if (attentionCount > 0)
        {
            parts.Add($"{attentionCount} položek k řešení");
        }

        if (parts.Count == 0)
        {
            return "V pořádku";
        }

        return string.Join(" | ", parts);
    }

    private static string FormatMoney(decimal value) => $"{value:0.00} Kč";

    private static string FormatValue(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}
