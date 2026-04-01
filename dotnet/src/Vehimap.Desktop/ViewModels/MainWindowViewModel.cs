using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Vehimap.Application.Services;
using Vehimap.Storage.Legacy;

namespace Vehimap.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
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

    public ObservableCollection<VehicleListItemViewModel> Vehicles { get; } = new();

    public MainWindowViewModel()
    {
        Load();
    }

    private void Load()
    {
        try
        {
            var bootstrapper = new LegacyVehimapBootstrapper(
                new LegacyDataRootLocator(),
                new LegacyVehimapDataStore());

            var result = bootstrapper.LoadAsync(AppContext.BaseDirectory).GetAwaiter().GetResult();
            DataMode = result.DataRoot.IsPortable ? "Portable data vedle aplikace" : "Systémová datová složka";
            DataPath = result.DataRoot.DataPath;
            VehicleCount = result.DataSet.Vehicles.Count;
            HistoryCount = result.DataSet.HistoryEntries.Count;
            FuelCount = result.DataSet.FuelEntries.Count;
            RecordsCount = result.DataSet.Records.Count;
            RemindersCount = result.DataSet.Reminders.Count;
            MaintenanceCount = result.DataSet.MaintenancePlans.Count;

            Vehicles.Clear();
            foreach (var vehicle in result.DataSet.Vehicles.OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                Vehicles.Add(new VehicleListItemViewModel(
                    vehicle.Name,
                    vehicle.Category,
                    string.IsNullOrWhiteSpace(vehicle.Plate) ? "Bez SPZ" : vehicle.Plate,
                    vehicle.MakeModel));
            }
        }
        catch (Exception ex)
        {
            LoadError = ex.Message;
        }
    }
}
