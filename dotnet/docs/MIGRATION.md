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
- globalni hledani napric vozidly a hlavnim evidencemi s otevrenim na spravnou kartu a polozku
- flotilovy `Prehled terminu` a `Propadle terminy`, ktere umi otevrit spravne vozidlo nebo souvisejici evidenci
- keyboard-first vrstvu shellu s focus managementem, shortcuty a enter-akcemi na hlavnich seznamech
- desktopovy shell, ktery uz ukazuje vozidla, detail vybraneho vozidla, historii, tankovani, doklady, pripominky, plan udrzby, audit, naklady a casovou osu z realnych legacy dat
- editacni workflow pro pripominky, doklady, historii, tankovani a plan udrzby, vcetne importu spravovanych priloh
- editacni workflow pro zakladni udaje vozidla, vcetne stavu vozidla a pohonu pro servisni profil
- samostatna desktopova okna pro `Historii`, `Tankovani`, `Pripominky`, `Udrzbu`, `Doklady`, `Detail vozidla`, `Audit` a `Dashboard`
- app-level dialogy `Nastaveni`, `O programu` a `Zkontrolovat aktualizace`
- typed vrstvu nad podporovanymi hodnotami ze `settings.ini`, ktera umi menit reminder thresholdy a `show_dashboard_on_launch`
- centralizovane build info z root `src/VERSION`, ktere desktop vetvi dava stejnou semver a file version jako AHK release tok
- kompatibilni parser `update/latest.ini` a Windows pripravu automaticke instalace pres `Vehimap.Updater`
- modalni workflow pro `Export dat` a `Obnovit data`, ktere pouziva stejny `.vehimapbak` format jako AHK verze
- Appium harness a zivy Windows UI smoke nad publish buildem
- rozsireny Appium smoke pro samostatna workflow okna a hlavni app-level dialogy vcetne nastaveni, o programu a kontroly aktualizaci
- sjednocena korenova accessibility metadata hlavniho shellu, workspace oken a app-level dialogu pro stabilnejsi screen-reader diagnostiku a UI automatizaci
- nastaveni `run_at_startup`, `hide_on_launch` a automatickych zaloh vcetne rucni akce `Zalohovat ihned`
- multiplatformni publish matrix pro `.NET` desktop preview
- draft release workflow pro tagy `dotnet-preview-v<verze>` s verzovanymi balicky a checksumy
- runtime-specific preview update manifesty `update/latest-dotnet-preview-<rid>.ini`
- Windows CI Appium smoke nad publish buildem desktop preview

## Co je dalsi na rade

1. Portovat zbytek aplikacovych toku, ktere jeste v .NET vetvi chybi proti AHK verzi
2. Rozsirovat Appium smoke a accessibility kontrakty vzdy s kazdym novym dialogem nebo workflow
3. Rozhodnout, kdy se `.NET` desktop preview kanal zmeni z preview manifestu na plnohodnotny release kanal
