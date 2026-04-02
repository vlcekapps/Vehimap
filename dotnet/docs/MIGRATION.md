# Migrace AHK -> .NET

Tato mapa drzi prvni prepis v C# navazany na soucasny Vehimap, misto aby vznikla nova aplikace bez vazby na realitu.

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
  - `Vehimap.Application` + pozdeji `Vehimap.Desktop`
- `src/lib/HelpAndUpdates.ahk`
  - `Vehimap.Application` + `Vehimap.Updater` + release workflow

## Co uz tato vetev umi

- zalozene solution a projekty
- desktop shell v Avalonia
- portable/system data root locator
- prime cteni a zapis soucasnych TSV/INI souboru
- import/export `.vehimapbak` vcetne spravovanych priloh
- prvni C# audit engine nad legacy daty
- prvni C# nakladovy souhrn vcetne `Cena / km` a srovnani proti stejne dlouhemu obdobi loni
- builder casove osy vozidla nad historii, tankovanim, pripominkami, doklady, TK/ZK a planem udrzby
- manualni ICS export budouciho kalendare z nove C# vetve
- akcni casovou osu, ktera umi otevrit souvisejici historii, doklad, pripominku nebo plan udrzby na spravne karte shellu
- prvni dashboard nad auditem, naklady a nejblizsimi terminy napric vozidly
- prvni keyboard-first vrstvu shellu s focus managementem, shortcuty a enter-akcemi na hlavnich seznamech
- desktopovy shell, ktery uz ukazuje vozidla, detail vybraneho vozidla, historii, tankovani, doklady, pripominky, plan udrzby, audit, naklady a casovou osu z realnych legacy dat
- zaklad release workflow pro GitHub Actions

## Co je dalsi na rade

1. Dopsat realne UI testy pres Appium a prvni smoke pro screen readery na Windows
2. Rozsirit accessibility metadata a focus chovani z hlavniho shellu i do dalsich budoucich dialogu
3. Zacit portovat editacni workflow, nejen cteci prehledy
4. Dodelat dalsi prehledove obrazovky a globalni hledani
