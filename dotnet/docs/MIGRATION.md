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
- globalni hledani napric vozidly a hlavnimi evidencemi s otevrenim na spravnou kartu a polozku
- flotilovy `Prehled terminu` a `Propadle terminy`, ktere umi otevrit spravne vozidlo nebo souvisejici evidenci
- prvni keyboard-first vrstvu shellu s focus managementem, shortcuty a enter-akcemi na hlavnich seznamech
- desktopovy shell, ktery uz ukazuje vozidla, detail vybraneho vozidla, historii, tankovani, doklady, pripominky, plan udrzby, audit, naklady a casovou osu z realnych legacy dat
- editacni workflow pro pripominky, doklady, historii, tankovani a plan udrzby, vcetne importu spravovanych priloh
- editacni workflow pro zakladni udaje vozidla, vcetne stavu vozidla a pohonu pro servisni profil
- vlastni focusovatelnou listu hlavnim karet, aby shell sel rozumne obsluhovat i se cteckami obrazovky
- app-level dialogy `Nastaveni`, `O programu` a `Zkontrolovat aktualizace`
- typed vrstvu nad podporovanymi hodnotami ze `settings.ini`, ktera uz umi menit reminder thresholdy a `show_dashboard_on_launch`
- centralizovane build info z root `src/VERSION`, ktere desktop vetvi dava stejnou semver a file version jako AHK release tok
- kompatibilni parser `update/latest.ini` a Windows pripravu automaticke instalace pres `Vehimap.Updater`
- modalni workflow pro `Export dat` a `Obnovit data`, ktere pouziva stejny `.vehimapbak` format jako AHK verze
- zaklad release workflow pro GitHub Actions

## Co je dalsi na rade

1. Dopsat realne UI testy pres Appium a prvni smoke pro screen readery na Windows
2. Rozsirit accessibility metadata a focus chovani z hlavniho shellu i do dalsich budoucich dialogu
3. Portovat zbytek aplikacovych toku, ktere jeste v .NET vetvi chybi proti AHK verzi
4. Rozdelit hlavni shell na mensi dialogy nebo routed stranky, aby dalsi a11y a testy uz nevisely na jednom obrim okne
