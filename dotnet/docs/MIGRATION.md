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
- klavesove dotazeni nakladoveho workspace: `Ctrl+P` cte rozpad vybraneho vozidla, `Ctrl+O` nebo `Enter` otevre vozidlo a `Ctrl+U` / `F2` otevre editor vozidla
- nakladovy workspace v Avalonia vetvi umi zvolit predvolbu obdobi nebo vlastni datumovy rozsah; volba se uklada do `settings.ini` a sdili ji dashboard i exporty
- builder casove osy vozidla nad historii, tankovanim, pripominkami, doklady, TK/ZK a planem udrzby
- casova osa vozidla v Avalonia vetvi si pamatuje posledni filtr `Vse` / `Budouci` / `Minule` v `settings.ini`, ale rychle textove hledani zustava jen docasne
- manualni ICS export budouciho kalendare z nove C# vetve
- akcni casovou osu, ktera umi otevrit souvisejici historii, doklad, pripominku nebo plan udrzby na spravne karte shellu
- prvni dashboard nad auditem, naklady a nejblizsimi terminy napric vozidly
- globalni hledani napric vozidly a hlavnim evidencemi s otevrenim na spravnou kartu a polozku
- flotilovy `Prehled terminu` a `Propadle terminy`, ktere umi otevrit spravne vozidlo nebo souvisejici evidenci
- Avalonia `Blizici se terminy` umi volitelne pridat vozidla bez zelene karty a datove nedostatky z auditu; otevreni datoveho nedostatku pouziva stejnou navigaci jako `Audit dat`
- terminove prehledy v Avalonia vetvi si pamatuji posledni rozbalovaci filtr typu polozky i pristupne razeni v `settings.ini`, ale rychle textove hledani zustava jen docasne
- `Audit dat` a `Globalni hledani` v Avalonia vetvi si pamatuji posledni pristupne razeni v `settings.ini`, ale rychle textove hledani zustava jen docasne
- dashboard v Avalonia vetvi ma sdilene `Obnovit` / `Ctrl+R`, ktere prepocita auditni vyrez, naklady a nejblizsi terminy bez ztraty vyberu
- terminove prehledy v Avalonia vetvi maji sdilene `Obnovit` / `Ctrl+R`, ktere prepocita seznam bez ztraty vyberu
- keyboard-first vrstvu shellu s focus managementem, shortcuty a enter-akcemi na hlavnich seznamech
- desktopovy shell, ktery uz ukazuje vozidla, detail vybraneho vozidla, historii, tankovani, doklady, pripominky, plan udrzby, audit, naklady a casovou osu z realnych legacy dat
- detail vozidla v Avalonia vetvi ukazuje stav, stitky, posledni historicke zaznamy, posledni znamy tachometr a samostatne stavove souhrny historie, tankovani, pripominek, dokladu a udrzby, aby se priblizil kontrolnimu detailu z AHK verze
- detail vozidla v Avalonia vetvi umi z pristupneho bloku `Souvisejici evidence` prepnout na historii, tankovani, pripominky, udrzbu, doklady, casovou osu nebo naklady vybraneho vozidla
- hlavni seznam vozidel v Avalonia shellu si pamatuje posledni rozbalovaci filtr kategorie a stavovy filtr v `settings.ini`, ale rychle textove hledani zustava jen docasne
- globalni hledani v Avalonia vetvi prohledava rozsirena metadata vozidla a u navazujicich evidenci bere v uvahu i nazev vozidla, SPZ a znacku/model stejne jako AHK implementace
- editacni workflow pro pripominky, doklady, historii, tankovani a plan udrzby, vcetne importu spravovanych priloh, AHK-kompatibilni validace/normalizace datumu, tachometru, castek, intervalu, typu paliva a platnosti dokladu i rozbalovacich hodnot pro typ dokladu a opakovani pripominky v beznych editorech i v balicku pro vozidlo
- pristupne razeni evidencnich workspace `Historie`, `Tankovani`, `Pripominky`, `Plan udrzby` a `Doklady` pres ovladace `Radit` a `Sestupne`; posledni volba se uklada do legacy `settings.ini`
- dokladove akce pro otevreni souboru, otevreni slozky a zkopirovani vyresene cesty prilohy pres `Ctrl+Shift+C`
- posun opakovanych pripominek na dalsi termin v Avalonia workspace, vcetne zkratky `Ctrl+Shift+N`
- oznaceni servisniho planu jako splneneho v Avalonia workspace, vcetne zkratky `Ctrl+L`, potvrzeni data a tachometru a volitelneho zapisu stejne udalosti do historie vozidla
- doplneni doporucenych servisnich sablon primo z Avalonia planu udrzby pres sdileny vyberovy dialog a zkratku `Ctrl+Shift+N`
- vyber bezne servisni sablony v Avalonia editoru udrzby, ktery predvyplni nazev, intervaly a poznamku
- pristupne polozky a klavesove ovladani dialogu `Balicek pro vozidlo` / `Doporucene servisni sablony`: ctecky dostavaji lidsky popis polozky, mezernik prepina vybranou polozku, `Ctrl+S` potvrzuje, `Ctrl+A` vybira vse, `Ctrl+Shift+A` vyber maze a `Escape` zavira dialog
- editacni workflow pro zakladni udaje vozidla, vcetne AHK-kompatibilne normalizovanych stitku, kategorie, SPZ, terminu TK/ZK, stavu vozidla a celeho servisniho profilu
- samostatna desktopova okna pro `Historii`, `Tankovani`, `Pripominky`, `Udrzbu`, `Doklady`, `Detail vozidla`, `Audit` a `Dashboard`
- app-level dialogy `Nastaveni`, `O programu` a `Zkontrolovat aktualizace`
- typed vrstvu nad podporovanymi hodnotami ze `settings.ini`, ktera umi menit reminder thresholdy a `show_dashboard_on_launch`
- centralizovane build info z root `src/VERSION`, ktere desktop vetvi dava stejnou semver a file version jako AHK release tok
- kompatibilni parser `update/latest.ini` a Windows pripravu automaticke instalace pres `Vehimap.Updater`
- modalni workflow pro `Export dat` a `Obnovit data`, ktere pouziva stejny `.vehimapbak` format jako AHK verze
- Appium harness a zivy Windows UI smoke nad publish buildem
- rozsireny Appium smoke pro samostatna workflow okna a hlavni app-level dialogy vcetne nastaveni, o programu a kontroly aktualizaci
- sjednocena korenova accessibility metadata hlavniho shellu, workspace oken a app-level dialogu pro stabilnejsi screen-reader diagnostiku a UI automatizaci
- sdileny lifecycle samostatnych workspace oken pro prvni fokus po otevreni a potvrzeni zavreni rozpracovanych editoru
- nazvy samostatnych workspace oken jsou vlastnene workspace viewmodely; root `MainWindowViewModel` uz je nevystavuje jako verejne proxy vlastnosti
- action-state podminky evidencnich editoru, dokladovych priloh a exportu nakladu jsou interni soucasti prikazu a workspace vrstvy; root `MainWindowViewModel` uz je nevystavuje jako verejne proxy vlastnosti
- sdilene otevirani samostatnych workspace oken z hlavniho shellu, aby potvrzeni rozpracovanych editaci, prepnuti karty a navrat fokusu mely jeden kodovy tok
- stav `Casove osy` a `Globalniho hledani` je vlastneny jejich child workspace viewmodely; root `MainWindowViewModel` uz pro ne nevystavuje duplicitni proxy vlastnosti
- nactene kolekce polozek `Casove osy` a vysledku `Globalniho hledani` jsou vlastnene jejich child workspace viewmodely; root je jen obnovuje pri zmene dat, filtru nebo hledani
- stav terminovych prehledu `Blizici se terminy` a `Propadle terminy` je take vlastneny jejich child workspace viewmodely; root `MainWindowViewModel` uz pro ne nevystavuje duplicitni proxy vlastnosti
- nactene kolekce polozek terminovych prehledu `Blizici se terminy` a `Propadle terminy` jsou vlastnene jejich child workspace viewmodely; root je jen obnovuje pri zmene dat, filtru nebo hledani
- dashboardove souhrny auditu, nakladu a nejblizsich terminu jsou take vlastnene sdilenymi workspace viewmodely; root `MainWindowViewModel` zustava jen orchestrator jejich obnovy
- plny auditni seznam je vlastneny `AuditWorkspace`, dashboardovy auditni vyrez a nejblizsi terminy `DashboardWorkspace` a flotilovy seznam nakladu `CostWorkspace`; root uz je nevystavuje jako verejne kolekce
- volby nakladoveho obdobi, terminovych filtru, sablon udrzby a rezimu dokladovych priloh jsou vlastnene odpovidajicimi workspace viewmodely; root uz pro ne nevystavuje proxy vlastnosti
- stav exportu nakladoveho prehledu je vlastneny `CostWorkspace`; root `MainWindowViewModel` pouze provadi exportni prikazy a nastavuje vysledek do workspace
- souhrnne texty evidenci `Historie`, `Tankovani`, `Pripominky`, `Udrzba` a `Doklady` jsou vlastnene jejich workspace viewmodely; root je jen prepocitava pri zmene vozidla nebo dat
- nactene kolekce zaznamu evidenci `Historie`, `Tankovani`, `Pripominky`, `Udrzba` a `Doklady` jsou vlastnene jejich workspace viewmodely; root je jen plni pri zmene vozidla nebo dat
- vyber polozky, detailni text a editorovy stav evidenci `Historie`, `Tankovani`, `Pripominky`, `Udrzba` a `Doklady` jsou vlastnene jejich workspace viewmodely; root je pouziva jen interne pri persistu a navigaci
- souhrnne texty detailu vozidla jsou vlastnene `VehicleDetailWorkspace`; root je jen prepocitava pri zmene vyberu vozidla
- formularove hodnoty editoru vozidla jsou vlastnene `VehicleDetailWorkspace`; root zustava orchestrator prikazu, validace a persistu
- rezim editace vozidla, viditelnost detailu a nadpis detailniho panelu jsou vlastnene `VehicleDetailWorkspace`; root pouze synchronizuje prikazy, pending-edit ochrany a shellovou navigaci
- sdilene workspace zkratky pro hledani a otevreni polozek v casove ose, globalnim hledani a terminovych prehledech, stejne v kartach i samostatnych oknech; hlavni shell je routuje kontextove, aby je neprebijely globalni akce vozidla
- sdilene editacni workspace zkratky pro historii, tankovani, pripominky, udrzbu a doklady; `Ctrl+N`, `Ctrl+U` / `F2`, `Ctrl+S` a dokladove `Ctrl+O` / `Ctrl+Shift+O` funguji stejne v karte i samostatnem okne
- `Ctrl+N` a `Ctrl+U` / `F2` z hlavni karty evidence oteviraji prislusne modalni workspace okno s viditelnym editorem, aby se v read-only shellu nespoustel skryty inline editor
- plny auditni workspace s vlastnim hledanim, vsemi auditnimi polozkami a oddelenym dashboardovym top vyrezem
- nastaveni `run_at_startup`, `hide_on_launch` a automatickych zaloh vcetne rucni akce `Zalohovat ihned`
- settings UX pro automaticke zalohy: interval a pocet ponechanych zaloh jsou aktivni jen pri zapnutych pravidelnych zalohach a vypnuta sekce neblokuje ulozeni ostatnich voleb
- klavesove ovladani dialogu `Nastaveni`: `Ctrl+S`, `Ctrl+B` a `Esc`
- sjednocene klavesove ovladani app-level dialogu: `O programu` ma `Ctrl+O` pro release poznamky, kontrola aktualizaci, potvrzeni, upozorneni a tray akce maji `Esc` pro bezpecne zavreni
- multiplatformni publish matrix pro `.NET` desktop preview
- draft release workflow pro tagy `dotnet-preview-v<verze>` s verzovanymi balicky a checksumy
- runtime-specific preview update manifesty `update/latest-dotnet-preview-<rid>.ini`
- regresni testy packaging skriptu a generatoru manifestu, vcetne kontroly, ze `.sha256`, metadata a fyzicky balicek souhlasi
- Windows CI Appium smoke nad publish buildem desktop preview
- ulozeni `Tiskoveho prehledu` vozidel jako HTML souboru pres sdilenou exportni sluzbu a otevreni ulozeneho souboru
- rychle akce v Avalonia menu pro nejblizsi TK, zelenou kartu, vlastni pripominku, servisni udrzbu a doklad i pro filtrovanou kontrolu techto terminu
- primy vstup `Casova osa vozidla` v menu `Vozidlo`, aby C# shell zachoval stejnou informacni architekturu jako AHK menu
- primy vstup `Export terminu do kalendare (.ics)` v menu `Prehledy`, napojeny na sdileny kalendarovy export
- nativni `InputGesture` popisky u hlavnich Avalonia menu polozek, aby menu ukazovalo stejne klavesove zkratky jako shell
- otevreni aktualni datove slozky z menu `Soubor` i z pristupneho tray akcniho okna pres sdileny multiplatformni file launcher
- pristupne tray akcni okno s primym otevrenim hlavniho okna, dashboardu, blizicich se terminu, propadlych terminu, nejblizsi TK/ZK/pripominky/servisu/dokladu, filtrovanych kontrol, tiskoveho prehledu, exportu/importu zalohy, exportu kalendare, znovunacteni dat, otevreni datove slozky, nastaveni, O programu, kontroly aktualizaci a ukonceni aplikace
- regresni kontrola desktop UI zdroju proti typickym mojibake znakum, aby ctecky obrazovky nedostavaly poskozenou UTF-8 diakritiku

## Co je dalsi na rade

1. Portovat zbytek aplikacovych toku, ktere jeste v .NET vetvi chybi proti AHK verzi
2. Rozsirovat Appium smoke a accessibility kontrakty vzdy s kazdym novym dialogem nebo workflow
3. Rozhodnout, kdy se `.NET` desktop preview kanal zmeni z preview manifestu na plnohodnotny release kanal
