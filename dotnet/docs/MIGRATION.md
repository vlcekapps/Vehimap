# Migrace AHK -> .NET

Tato mapa drží první přepis v C# navázaný na současný Vehimap, místo aby vznikla nová aplikace bez vazby na realitu.

## AHK moduly -> .NET vrstvy

- `src/lib/DataStore.ahk`
  - `Vehimap.Storage.Legacy`
- `src/lib/ImportExport.ahk`
  - `Vehimap.Storage.Legacy` + `Vehimap.Updater`
- `src/lib/BackupsAndAlerts.ahk`
  - `Vehimap.Application`
- `src/lib/MainWindow.ahk`, `VehicleDialogs.ahk`, `HistoryDialog.ahk`, `FuelDialog.ahk`, `RecordsDialog.ahk`, `ReminderDialog.ahk`, `MaintenancePlans.ahk`
  - `Vehimap.Desktop`
- `src/lib/Dashboard.ahk`, `Overviews.ahk`, `AuditTools.ahk`, `TimelineAndCalendar.ahk`, `Costs.ahk`
  - `Vehimap.Application` + později `Vehimap.Desktop`
- `src/lib/HelpAndUpdates.ahk`
  - `Vehimap.Application` + `Vehimap.Updater` + release workflow

## Co už tato větev umí

- založené solution a projekty
- desktop shell v Avalonia
- portable/system data root locator
- přímé čtení a zápis současných TSV/INI souborů
- import/export `.vehimapbak` včetně spravovaných příloh
- základ release workflow pro GitHub Actions

## Co je další na řadě

1. Port dashboardu, audit enginu a nákladových souhrnů do `Vehimap.Application`
2. Rozšířit Avalonia shell na hlavní seznam vozidel a detail
3. Přidat klávesové workflow a accessibility metadata
4. Dopsat reálné UI testy přes Appium
