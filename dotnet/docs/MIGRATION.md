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
- první C# audit engine nad legacy daty
- první C# nákladový souhrn včetně `Cena / km` a srovnání proti stejně dlouhému období loni
- desktopový shell, který už ukazuje vozidla, detail vybraného vozidla, historii, doklady, audit a náklady z reálných legacy dat
- základ release workflow pro GitHub Actions

## Co je další na řadě

1. Port tankování, připomínek a plánu údržby do `Vehimap.Desktop`
2. Rozšířit `Vehimap.Application` o timeline, ICS export a servisní doporučení
3. Přidat klávesové workflow, fokus management a accessibility metadata
4. Dopsat reálné UI testy přes Appium a první smoke pro screen readery na Windows
