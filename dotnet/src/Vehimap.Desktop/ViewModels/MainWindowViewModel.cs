using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application;
using Vehimap.Application.Services;
using Vehimap.Domain.Models;
using Vehimap.Platform;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly Dictionary<string, VehicleMeta> _metaByVehicleId = new(StringComparer.Ordinal);

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
    private VehicleListItemViewModel? selectedVehicle;

    public ObservableCollection<VehicleListItemViewModel> Vehicles { get; } = [];

    public ObservableCollection<AuditItemViewModel> AuditItems { get; } = [];

    public ObservableCollection<CostVehicleItemViewModel> CostVehicles { get; } = [];

    public MainWindowViewModel()
    {
        Load();
    }

    partial void OnSelectedVehicleChanged(VehicleListItemViewModel? value)
    {
        if (value is null)
        {
            SelectedVehicleHeading = "Nevybrané vozidlo";
            SelectedVehicleOverview = "Vyberte vozidlo vlevo a zobrazí se jeho základní souhrn.";
            SelectedVehicleDates = string.Empty;
            SelectedVehicleProfile = string.Empty;
            return;
        }

        var state = string.IsNullOrWhiteSpace(value.State) ? "Běžný provoz" : value.State;
        var note = string.IsNullOrWhiteSpace(value.VehicleNote) ? "Bez poznámky" : value.VehicleNote;
        var powertrain = string.IsNullOrWhiteSpace(value.Powertrain) ? "Servisní profil zatím nevyplněn" : value.Powertrain;

        SelectedVehicleHeading = value.Name;
        SelectedVehicleOverview = $"{value.MakeModel} | {value.Category} | {value.Plate}\nStav: {state}\nPoznámka: {note}";
        SelectedVehicleDates = $"Příští TK: {FormatValue(value.NextTk, "nevyplněno")}\nZelená karta do: {FormatValue(value.GreenCardTo, "nevyplněno")}\nSouhrnný stav: {FormatValue(value.StatusSummary, "bez upozornění")}";
        SelectedVehicleProfile = $"Pohon a servisní profil: {powertrain}";
    }

    private void Load()
    {
        try
        {
            var bootstrapper = new LegacyVehimapBootstrapper(
                new LegacyDataRootLocator(),
                new LegacyVehimapDataStore());

            var result = bootstrapper.LoadAsync(AppContext.BaseDirectory).GetAwaiter().GetResult();
            var auditService = new LegacyAuditService(new ManagedAttachmentPathService());
            var costService = new LegacyCostAnalysisService();
            var audit = auditService.BuildAudit(result.DataRoot, result.DataSet);
            var costSummary = costService.BuildYearToDateSummary(result.DataSet, DateOnly.FromDateTime(DateTime.Today));

            DataMode = result.DataRoot.IsPortable ? "Portable data vedle aplikace" : "Systémová datová složka";
            DataPath = result.DataRoot.DataPath;
            VehicleCount = result.DataSet.Vehicles.Count;
            HistoryCount = result.DataSet.HistoryEntries.Count;
            FuelCount = result.DataSet.FuelEntries.Count;
            RecordsCount = result.DataSet.Records.Count;
            RemindersCount = result.DataSet.Reminders.Count;
            MaintenanceCount = result.DataSet.MaintenancePlans.Count;
            AuditCount = audit.Count;
            AuditSummary = BuildAuditSummary(audit);
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
                    BuildVehicleStatusSummary(vehicle, meta, audit)));
            }

            AuditItems.Clear();
            foreach (var item in audit.Take(8))
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
        }
        catch (Exception ex)
        {
            LoadError = ex.Message;
        }
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
