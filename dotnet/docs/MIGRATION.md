# Migrace AHK -> .NET

Tato mapa drzi prepis Vehimapu z puvodni AHK aplikace do C#/.NET. AHK runtime, knihovny a smoke testy byly po prvnim stabilnim Windows release odstraneny; dokument zustava jako historicka mapa parity a dalsich kroku multiplatformni vetve.

## Storage 2.0

- Vehimap 2.0 pouziva jako primarni runtime storage SQLite databazi `data/vehimap.db`.
- Legacy `TSV/INI` soubory z 1.0.2 se pouzivaji jen jako jednorazovy migracni vstup: pri prvnim startu bez `vehimap.db` aplikace vytvori `data/migration-backups/<cas>`, zkopiruje puvodni soubory i `data/attachments`, nacte data pres `Vehimap.Storage.Legacy`, ulozi je do SQLite a po overenem nacteni databaze presune zive TSV/INI soubory do `data/migration-backups/<cas>/removed-from-data-root/`.
- Pokud uz `vehimap.db` existuje a v koreni `data/` zustaly legacy TSV/INI soubory po starsi nightly, start aplikace je odlozi do nove migracni zalohy bez opakovaneho importu. Aktivni `data/attachments` zustava na miste, protoze spravovane prilohy jsou soucasti datove sady 2.0.
- Nove `.vehimapbak` zalohy jsou SQLite backupy s `vehimap.db` a referenced spravovanymi prilohami; starsi textove `.vehimapbak` zustavaji podporovane jako importni/migracni vstup.
- Jedno vozidlo lze prenaset uzivatelskym balickem `*.vehimapvehicle`, ktery obsahuje vozidlo, jeho evidence a relevantni spravovane prilohy bez zmeny hlavni databaze mimo explicitni import.
- Runtime zapis po migraci je hlidany SQLite-only gate: bezne ulozeni nastaveni, vozidla, tankovani nebo dokladu smi menit `vehimap.db`, ale nesmi znovu vytvorit zive legacy TSV/INI soubory v `data/`.
- Zdravi datove sady 2.0 lze overit rucne z menu `Soubor -> Zkontrolovat datovou sadu 2.0`; health check overuje otevreni `vehimap.db`, `PRAGMA quick_check`, ocekavane tabulky, schema marker, zapisovatelnost datove slozky, aktivni `attachments` a zbytky legacy TSV/INI bez automatickeho mazani nebo oprav databaze.
- Migracni a health diagnostika datove sady 2.0 pouziva EN/CS `.resx` zdroje, vcetne hlasek automaticke migrace, cleanupu zbylych legacy TSV/INI souboru, `quick_check`, schema markeru, priloh a textu kopirovaneho z health dialogu.
- `Vehimap.Storage.Legacy` zustava read-only kompatibilitni vrstva pro migraci a import starsich zaloh, ne dlouhodoby runtime format 2.x.

## Lokalizace pred Androidem

- Vehimap 2.0 nightly zavadi lokalizacni a formatovaci zaklad pred dalsimi velkymi funkcemi a pred Android UI.
- Primarni cesta je `.resx`: `Strings.resx` jako anglicky fallback a `Strings.cs-CZ.resx` jako ceska verze.
- Pilotni oblasti jsou `Nastaveni`, `O programu` vcetne kopirovacich statusu a runtime rezimu, `Editor vozidla` vcetne potvrzeni mazani, hlavni shell, hlavni menu, levy panel seznamu vozidel, nazvy pracovnich karet, staticke povrchy editoru historie/tankovani/udrzby/pripominek/dokladu, runtime souhrny/detailove panely/vysledky hledani a screen-reader popisky polozek evidenci historie/tankovani/pripominek/udrzby/dokladu, runtime stavove a validacni hlasky evidencnich editoru vcetne vyberu souboru a akci priloh, dialog `Dokonceni udrzby`, dialogy kontroly a stahovani aktualizaci vcetne platformnich vysledku kontroly aktualizaci a validace manifestu, kratke desktopove notifikacni okno, domain i18n pass analyzy tankovani, auditu dat, Chytreho poradce, casove osy, globalniho hledani, terminovych prehledu/dashboard timeline, rychlych akci, app-shell workflow statusu a nakladoveho workflow, workspace `Detail vozidla`, `Globalni hledani`, `Casova osa`, `Blizici se terminy`, `Propadle terminy`, `Chytry poradce`, `Audit dat`, `Naklady` a `Dashboard`, staticke texty jejich workspace karet vcetne vyhledavani, razeni, akcnich tlacitek, terminovych detailu, doporucovacich/auditnich filtru, dashboardovych sekci, nakladovych exportu a dokladovych priloh, servisni knizka vcetne generovanych polozek a HTML exportu, tiskovy HTML prehled vozidel, rucni ICS export vcetne kalendarovych nazvu/popisu a lokalizovane dynamicke texty karty `Casova osa`, kratke akcni statusy dashboardu/udrzby/detailu vozidla, workspace refresh statusy, globalni dialogy `Potvrzeni` / `Kontrola datove sady 2.0`, pending-edit popisy cilovych akci, chrome samostatnych workspace oken, automaticke zalohy, filtrovane souhrny auditu/poradce, a11y text-edit live-region a pristupne okno `Akce na liste`.
- Projekcni texty seznamu vozidel, detailu vozidla a zakladnich evidenci uz take spadaji do i18n pilotu: fallback SPZ/modelu, stav vozidla, souhrn navazujicich evidenci, platnost dokladu, dostupnost priloh, prazdne stavy a pocty polozek se skladaji z EN/CS `.resx` zdroju, zatimco uzivatelsky ulozena data se neprekladaji.
- Viditelne ulozene volby, ktere jeste nejsou prevedene na stabilni interni klice, musi pri zmene jazyka prijimat kompatibilni aliasy. Hlavni filtr seznamu vozidel a rezim dokladovych priloh uz rozpoznavaji stare ceske i nove anglicke labely, aby lokalizace nerozbila ulozene preference.
- Viditelne volby razeni evidenci, terminovych prehledu, auditu a globalniho hledani uz maji stabilni interni sort klice a lokalizovane EN/CS labely; starsi ceske i anglicke labely zustavaji prijimane jako kompatibilni ulozene hodnoty.
- Pilotni lokalizace se vztahuje i na accessibility metadata, napriklad `AutomationProperties.Name` u seznamu, scrollovanych oblasti a kontejneru; tyto texty nesmi zustat natvrdo v XAML.
- Editor vozidla uz pouziva EN/CS `.resx` i pro titulky dialogu, uvodni instrukce a validacni hlasky ve stavovem live-regionu, nejen pro staticke field labely.
- App-level ochranny text pro rozpracovane editory, globalni napoveda rozbalovacich seznamu a GitHub feedback issue sablona uz take pouzivaji EN/CS `.resx`, aby se shellove texty ridily aktivnim jazykem.
- Do stejne app-level lokalizacni vrstvy patri i hlasky installer locale seedu, potvrzeni obnovy zalohy, kratke native tray menu, popisy zavirani rozpracovanych evidencnich editoru v samostatnych oknech a platformni texty kontroly/stahovani aktualizaci.
- SQLite 2.0 migracni hlasky, cleanup zbylych legacy TSV/INI souboru a health diagnostika datove sady jsou take soucasti lokalizacniho pilotu.
- Dialog `Balicek pro vozidlo` / `Doporucene servisni sablony` je v pilotni lokalizaci pro titulky, napovedu, tlacitka, souhrny a pristupne popisky polozek; samotny katalog sablon zustava samostatny budouci template-id/catalog pass, protoze potvrzene hodnoty se ukladaji jako uzivatelska data.
- Nastaveni uz nese jazyk, oddelovac tisicu, oddelovac desetin, jednotku vzdalenosti, jednotku objemu paliva a menu.
- Podporovane jednotky jsou kilometry/mile a litry/US galony/imperialni galony.
- Vzdalenostni nastaveni v UI, napr. upozorneni na udrzbu podle vzdalenosti, se zobrazuji a parsují ve zvolene jednotce, ale runtime storage dal uklada normalizovane kilometry.
- Tachometry, servisni intervaly, dokonceni udrzby a objem tankovani se v editorech zobrazuji a parsují podle zvolenych jednotek, ale SQLite dal uklada kanonicke kilometry a litry.
- Oddelovace cisel jsou jen formatovaci preference; prepnuti oddelovace znovu vykresli hodnoty a nejednoznacne kombinace tisicu/desetin se stejnym znakem se odmitnou.
- Interni data zustavaji invariantni a SQLite storage se kvuli lokalizaci nemeni; zobrazeni, vstup a exporty budou postupne pouzivat formatovaci sluzby.
- Pred sirsim verejnym testovanim 2.0 je blokujici prubezny i18n/unit conformance pass pro plosne pouziti zvolene meny, formatovani peneznich castek, exportni vystupy a vsechny zbyvajici km/l/mile/galon texty. Dashboardove/nakladove souhrny, analyza tankovani vcetne upozorneni na klesajici tachometr, servisni knizka vcetne tachometru a servisnich vzdalenosti, casova osa vcetne tachometru/servisnich vzdalenosti/objemu paliva, globalni hledani vcetne viditelnych tachometru, servisnich intervalu a objemu paliva, stavove texty udrzby, nastaveni vzdalenostnich upozorneni, balicek pro vozidlo, nakladove hledani/stavy a nakladove TSV/HTML exporty uz zvolenou menu a jednotky respektuji nebo pouzivaji jednotkove neutralni formulaci; analyza tankovani zobrazuje pri kombinaci mile + galony spotrebu jako `mpg`. Dalsi zbyvajici exportni/reportovaci vystupy se musi dodelat. Preklad resource retezce sam o sobe nestaci, pokud hodnota porad obchazi formatovaci, menovou nebo jednotkovou sluzbu.
- Prekladatelska pravidla jsou v `dotnet/docs/I18N.md`; commit messages zustavaji vyhradne anglicky.

## Historicka mapa AHK modulu -> .NET vrstvy

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
- kanalove oddeleny single-instance desktop runtime: opakovane spusteni stejneho kanalu `stable`, `beta` nebo `nightly` misto nove instance obnovi bezici hlavni okno z listy
- primarni cteni a zapis datove sady 2.0 do SQLite `data/vehimap.db`
- jednorazovou automatickou migraci legacy TSV/INI souboru do SQLite s predmigracni kopii puvodnich dat a presunem zivych TSV/INI mimo runtime koren `data/`
- diagnostiku poskozenych legacy TSV/INI souboru s nazvem souboru, plnou cestou a puvodnim parser detailem pro shell i testy
- import/export `.vehimapbak` vcetne spravovanych priloh; nova 2.0 zaloha obsahuje SQLite databazi a starsi textove zalohy se importuji pres legacy parser s citelnou diagnostikou
- SQLite 2.0 health check s rucnim dialogem, kopirovanim diagnostiky a testy pro zdravou databazi, poskozeny soubor, chybejici schema marker i zive legacy soubory vedle existujici databaze
- prvni C# audit engine nad sdilenym datasetem bez zavislosti na konkretni runtime storage vrstve
- prvni C# nakladovy souhrn vcetne ceny za zvolenou vzdalenost a srovnani proti stejne dlouhemu obdobi loni
- klavesove dotazeni nakladoveho workspace: `Ctrl+P` cte rozpad vybraneho vozidla, `Ctrl+O` nebo `Enter` otevre vozidlo a `Ctrl+U` / `F2` otevre editor vozidla
- nakladovy workspace v Avalonia vetvi umi zvolit predvolbu obdobi nebo vlastni datumovy rozsah; volba se uklada do nastaveni datove sady a sdili ji dashboard i exporty
- exporty nakladoveho workspace hlasi uspech, zruseni i selhani ve stavovem textu workspace i hlavniho shellu vcetne situace, kdy je HTML soubor ulozeny, ale nejde otevrit
- exporty nakladoveho workspace do TSV i HTML pouzivaji EN/CS resource texty, zvolenou menu a zvolene jednotky vzdalenosti/objemu; cena za vzdalenost se v exportu zobrazi za km nebo mili podle preference, ale ulozene castky se bez kurzove politiky neprepocitavaji
- builder casove osy vozidla nad historii, tankovanim, pripominkami, doklady, TK/ZK a planem udrzby
- casova osa vozidla v Avalonia vetvi si pamatuje posledni filtr `Vse` / `Budouci` / `Minule` v nastaveni datove sady, ale rychle textove hledani zustava jen docasne
- manualni ICS export budouciho kalendare z nove C# vetve vcetne foldingu dlouhych iCalendar radku
- ICS export uz pouziva EN/CS `.resx` pro uzivatelske nazvy udalosti, popisy udalosti i stavove hlasky v shellu; technicky iCalendar format, UID a PRODID zustavaji stabilni.
- akcni casovou osu, ktera umi otevrit souvisejici historii, doklad, pripominku nebo plan udrzby na spravne karte shellu
- pristupnou `Servisni knizku` vybraneho vozidla jako novou C# nightly funkci mimo AHK paritu; cte soucasnou historii, servisni plany a servisne relevantni doklady bez zmeny datovych formatu, umi otevrit souvisejici evidenci a exportovat HTML pro tisk nebo archivaci
- offline `Chytry poradce` jako novou C# nightly funkci mimo AHK paritu; bez AI/API sklada konzervativni doporuceni z auditu, terminu, udrzby, tankovani, dokladovych priloh a nakladovych signalu, podporuje filtry a umi prejit na souvisejici evidenci
- prvni dashboard nad auditem, naklady a nejblizsimi terminy napric vozidly
- globalni hledani napric vozidly a hlavnim evidencemi s otevrenim na spravnou kartu a polozku
- flotilovy `Prehled terminu` a `Propadle terminy`, ktere umi otevrit spravne vozidlo nebo souvisejici evidenci
- Avalonia `Blizici se terminy` umi volitelne pridat vozidla bez zelene karty a datove nedostatky z auditu; otevreni datoveho nedostatku pouziva stejnou navigaci jako `Audit dat`
- terminove prehledy v Avalonia vetvi si pamatuji posledni rozbalovaci filtr typu polozky i pristupne razeni v nastaveni datove sady, ale rychle textove hledani zustava jen docasne
- `Audit dat` a `Globalni hledani` v Avalonia vetvi si pamatuji posledni pristupne razeni v nastaveni datove sady, ale rychle textove hledani zustava jen docasne
- dashboard v Avalonia vetvi ma sdilene `Obnovit` / `Ctrl+R`, ktere prepocita auditni vyrez, naklady a nejblizsi terminy bez ztraty vyberu
- dashboard v Avalonia vetvi umi primo prepnout `show_dashboard_on_launch`, stejne jako AHK dashboard a dialog nastaveni
- dashboard v Avalonia vetvi ma primy vstup do historie vozidla, nakladu vozidla, flotiloveho souhrnu nakladu a dokonceni vybraneho servisniho terminu pres stejny modalni dialog jako plan udrzby
- terminove prehledy v Avalonia vetvi maji sdilene `Obnovit` / `Ctrl+R`, ktere prepocita seznam bez ztraty vyberu
- keyboard-first vrstvu shellu s focus managementem, shortcuty, enter-akcemi na hlavnich seznamech a explicitnim tab stopem hlavniho seznamu vozidel
- Avalonia accessibility checklist v `dotnet/docs/ACCESSIBILITY.md`, ktery drzi stav `accessibility-oriented / pre-conformance`, dokumentuje povolene keyboard/focus vyjimky a napojuje unit testy na oficialni Avalonia model misto nahodnych workaroundu
- accessibility evidence slozku `dotnet/docs/accessibility-evidence/` pro rucni NVDA/Narrator a pozdeji VoiceOver/Orca zaznamy, aby slo pred 2.0 beta/stable dolozit realne screen-reader scenare
- staticke guard testy, ktere hlidaji `AutomationId` a lidske pristupne jmeno u interaktivnich Avalonia prvku vcetne `MenuItem` a kartovych `RadioButton` a blokuji nove rucni `KeyDown` handlery bez dokumentovane vyjimky
- staticke guard testy pro vsechny nadpisy s `AutomationProperties.HeadingLevel`, aby hlavni i sekcni nadpisy mely stabilni `AutomationId` a pristupne jmeno
- staticke guard testy pro dulezite informacni `TextBlock` prvky s `AutomationId`, aby souhrny a diagnosticke detaily mely explicitni `AutomationProperties.Name`
- staticke guard testy pro kopirovatelne `SelectableTextBlock` hodnoty s `AutomationId`, aby cesta nebo jina technicka hodnota nebyla pro ctecku skryta za obecnym popiskem
- staticke guard testy pro podminene vypnuta nastaveni, aby screen-reader uzivatel slysel, ktera volba je znovu aktivuje
- staticke guard testy pro live regiony u stavovych/chybovych/prubehovych textu, jeden hlavni `HeadingLevel=1` nadpis v kazdem samostatnem okne/dialogu a `AccessibilityView=Control` u landmarku
- staticke guard testy pro `AutomationProperties.AcceleratorKey` u menu zkratek a citelna accessibility metadata u progress baru
- staticke guard testy pro `AutomationProperties.ItemType` u seznamovych polozek s `AccessibleLabel` a konzervativni `AutomationProperties.ItemStatus` jen pro skutecny stav, prioritu nebo dostupnost
- staticke guard testy pro `AutomationProperties.IsRequiredForForm` u editorovych poli, ktera runtime validace opravdu vyzaduje
- staticke guard testy pro `PlaceholderText` v textovych polich, aby priklady hodnot a filtracni napovedy byly vystavene i pres `AutomationProperties.HelpText`
- staticke guard testy pro destruktivni nebo datove nahrazujici akce, aby mazani a obnova dat mely `AutomationProperties.HelpText` s dopadem akce pro ctecky
- globalni accessibility styl pro `ComboBox`, ktery rozbalovacim seznamum dava jednotnou napovedu pro otevreni sipkami a vyber hodnoty
- sdilenou top-level keyboard guard vrstvu, ktera chrani standardni editaci `TextBox`, otevirani `ComboBox` sipkami a focus po ulozeni vozidla proti regresim z control templatu nebo modalnich oken
- docasny `TextBox` live-region fallback pro NVDA, ktery pri kurzorove navigaci oznamuje nazev pole, pozici a okolni znak jako testovanou vyjimku do doby, nez bude nativni UIA textova podpora Avalonie dostatecna
- desktopovy shell, ktery uz ukazuje vozidla, detail vybraneho vozidla, historii, tankovani, doklady, pripominky, plan udrzby, audit, naklady a casovou osu z realnych dat po legacy migraci nebo primo ze SQLite
- detail vozidla v Avalonia vetvi ukazuje stav, stitky, posledni historicke zaznamy, posledni znamy tachometr a samostatne stavove souhrny historie, tankovani, pripominek, dokladu a udrzby, aby se priblizil kontrolnimu detailu z AHK verze
- detail vozidla v Avalonia vetvi umi z pristupneho bloku `Souvisejici evidence` prepnout na historii, tankovani, pripominky, udrzbu, doklady, casovou osu, servisni knizku nebo naklady vybraneho vozidla
- editory vozidla, historie, tankovani, pripominek, udrzby a dokladu jsou samostatne modalni dialogy; pridani/uprava se uz nemicha s ctecim detailem workspace, dialogy maji vlastni prvni fokus, `Ctrl+S`, `Esc`/`Zrusit`, live status, validacni fokus na chybne pole a navrat fokusu podle toho, odkud byly otevrene
- hlavni seznam vozidel v Avalonia shellu si pamatuje posledni rozbalovaci filtr kategorie a stavovy filtr v nastaveni datove sady, ale rychle textove hledani zustava jen docasne
- globalni hledani v Avalonia vetvi prohledava rozsirena metadata vozidla, stavove texty z casove osy a u navazujicich evidenci bere v uvahu i nazev vozidla, SPZ a znacku/model stejne jako AHK implementace
- editacni workflow pro pripominky, doklady, historii, tankovani a plan udrzby, vcetne importu spravovanych priloh, AHK-kompatibilni validace/normalizace datumu, tachometru, castek, intervalu, typu paliva a platnosti dokladu i rozbalovacich hodnot pro typ dokladu a opakovani pripominky v beznych editorech i v balicku pro vozidlo
- tankovani v .NET vetvi uz zapisuje `# Vehimap fuel v2` s detailem paliva a mistem tankovani; parser zustava kompatibilni se starsim `fuel v1`
- tankovani ma samostatnou odvozenou analyzu bez zmeny `fuel.tsv`: spotreba se pocita jen z pouzitelnych useku mezi plnymi nadrzemi, UI ukazuje cenu za litr, mista/paliva a konzervativni upozorneni s moznosti skocit na souvisejici tankovani
- persist chyb evidencnich editoru je osetreny pres sdileny helper: editor zustane otevreny, chyba se precte ve workspace statusu i shellu a fokus se vrati na editor nebo seznam
- evidencni persist helper umi rollback session datasetu na snapshot pred mutaci; fyzicke mazani managed priloh se provadi az po uspesnem zapsani datove sady
- prubezne ukladane preference shellu, filtru, razeni, casove osy, terminovych prehledu a nakladoveho obdobi pouzivaji sdilenou serializovanou persist frontu se snapshot rollbackem, aby selhani zapisu nastaveni neprosaklo do pozdejsiho ulozeni dat
- session-level zapisy nastaveni pro dialog Nastaveni, historii dennich desktopovych oznameni a metadata posledni automaticke zalohy maji vlastni snapshot rollback v `DesktopSessionController`, takze selhani zapisu nenecha v pameti nepravdivy ulozeny stav
- pristupne razeni evidencnich workspace `Historie`, `Tankovani`, `Pripominky`, `Plan udrzby` a `Doklady` pres ovladace `Radit` a `Sestupne`; posledni volba se uklada do datove sady
- dokladove akce pro otevreni souboru, otevreni slozky a zkopirovani vyresene cesty prilohy pres `Ctrl+Shift+C`, vcetne citelneho stavoveho vysledku pri uspechu i selhani
- posun opakovanych pripominek na dalsi termin v Avalonia workspace, vcetne zkratky `Ctrl+Shift+N`
- oznaceni servisniho planu jako splneneho v Avalonia workspace, vcetne zkratky `Ctrl+L`, potvrzeni data a tachometru a volitelneho zapisu stejne udalosti do historie vozidla
- doplneni doporucenych servisnich sablon primo z Avalonia planu udrzby pres sdileny vyberovy dialog a zkratku `Ctrl+Shift+N`
- vyber bezne servisni sablony v Avalonia editoru udrzby, ktery predvyplni nazev, intervaly a poznamku
- pristupne polozky a klavesove ovladani dialogu `Balicek pro vozidlo` / `Doporucene servisni sablony`: ctecky dostavaji lidsky popis polozky, mezernik prepina vybranou polozku, `Ctrl+S` potvrzuje, `Ctrl+A` vybira vse, `Ctrl+Shift+A` vyber maze a `Escape` zavira dialog
- editacni workflow pro zakladni udaje vozidla, vcetne AHK-kompatibilne normalizovanych stitku, kategorie, SPZ, terminu TK/ZK, stavu vozidla a celeho servisniho profilu
- samostatna desktopova okna pro `Historii`, `Tankovani`, `Pripominky`, `Udrzbu`, `Doklady`, `Detail vozidla`, `Audit` a `Dashboard`
- app-level dialogy `Nastaveni`, `O programu` a `Zkontrolovat aktualizace`
- app-level controller hlasi zruseni a selhani nastaveni, exportu/importu zalohy, otevreni release poznamek, kontroly aktualizaci a spusteni updateru pres stavovy text nebo aktualizacni dialog misto padu shellu
- typed vrstvu nad podporovanymi hodnotami nastaveni, ktera umi menit reminder thresholdy a `show_dashboard_on_launch`
- centralizovane build info z root `src/VERSION`, ktere desktop vetvi dava stejnou semver a file version jako AHK release tok
- kompatibilni parser `update/latest.ini` a Windows pripravu automaticke instalace pres `Vehimap.Updater`
- modalni workflow pro `Export dat` a `Obnovit data`, ktere ve 2.0 pouziva SQLite `.vehimapbak` a starsi AHK/C# 1.x zalohy umi importovat pres legacy parser
- pred obnovou zalohy vytvari ochrannou kopii aktualni SQLite databaze, pripadnych legacy TSV/INI souboru i spravovanych priloh v `data/import-backups/<cas>` a shell po importu hlasi konkretni cestu i pocet obnovenych priloh
- po uspesne obnove zalohy `DesktopSessionController` synchronizuje in-memory dataset, meta lookupy, audit a podporovane nastaveni; selhani restore ponecha predchozi session stav beze zmen
- hlavni Avalonia shell po importu zalohy obnovuje projekce, seznamy, audit, dashboard, naklady a rychle akce z aktualni session, ne pres druhy okamzity reload legacy store
- start shellu, rucni reload a import zalohy pouzivaji jednu sdilenou refresh cestu nad aktualni session, aby se hlavni projekce a action-state stavy nerozchazely mezi workflow
- Appium harness a zivy Windows UI smoke nad publish buildem
- rozsireny Appium smoke pro samostatna workflow okna a hlavni app-level dialogy vcetne nastaveni, o programu a kontroly aktualizaci
- sjednocena korenova accessibility metadata hlavniho shellu, workspace oken a app-level dialogu pro stabilnejsi screen-reader diagnostiku a UI automatizaci
- sdileny lifecycle samostatnych workspace oken pro prvni fokus po otevreni a potvrzeni zavreni rozpracovanych editoru
- samostatna workspace okna lze zavrit klavesou `Escape`; stejne jako zaviraci tlacitko tato cesta respektuje potvrzeni rozpracovanych editoru
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
- sdilene editacni workspace zkratky pro historii, tankovani, pripominky, udrzbu a doklady; `Ctrl+N` a `Ctrl+U` / `F2` funguji stejne v karte i samostatnem okne a oteviraji stejny editorovy dialog, `Ctrl+S` patri do aktivniho dialogu a dokladove `Ctrl+O` / `Ctrl+Shift+O` zustavaji pro otevreni prilohy nebo slozky z prehledu
- tlacitko `V okne` zustava pro prehlednejsi modalni praci se seznamem, ale editace se z karty i ze samostatneho workspace okna presouva do stejneho dialogoveho editoru kvuli jednoznacnemu kontextu `Novy` vs. `Upravit`
- dialogove editory sdileji jeden lifecycle helper pro prvni fokus, `Ctrl+S`, `Esc`, validacni fokus a navrat po zavreni; `Shift+Tab` se chova specialne jen z prvniho logickeho pole na `Zrusit`, z dalsich poli jde standardne na predchozi prvek
- editacni panely evidenci maji vlastni svisly scroll a prehledy `Dashboard` a `Naklady` maji celostrankovy scroll, aby vetsi systemove pismo nebo mensi viewport neschovaly spodni obsah
- plny auditni workspace s vlastnim hledanim, vsemi auditnimi polozkami a oddelenym dashboardovym top vyrezem
- nastaveni `run_at_startup`, `hide_on_launch` a automatickych zaloh vcetne rucni akce `Zalohovat ihned`
- settings UX pro automaticke zalohy: interval a pocet ponechanych zaloh jsou aktivni jen pri zapnutych pravidelnych zalohach a vypnuta sekce neblokuje ulozeni ostatnich voleb
- klavesove ovladani dialogu `Nastaveni`: `Ctrl+S`, `Ctrl+B` a `Esc`
- sjednocene klavesove ovladani app-level dialogu: `O programu` ma bezny souhrn aplikace s autorem `by Vlcek apps` a verzi vcetne kanalu, technickou diagnostiku schovanou pod samostatnym tlacitkem, `Ctrl+O` pro release poznamky, `Ctrl+Shift+C` pro zkopirovani diagnostiky a pristupny stav vysledku kopirovani; kontrola aktualizaci, potvrzeni, upozorneni a tray akce maji `Esc` pro bezpecne zavreni
- app-level dialogy `Nastaveni` a `Kontrola aktualizaci` jsou resizable a hlavni obsah maji ve scrollovatelne oblasti; `O programu` zustava kratky bezny souhrn a dlouha diagnostika je ve vlastni scrollovatelne casti, aby delsi cesty nebo vetsi systemove pismo neschovaly akcni tlacitka
- potvrzovaci dialogy, dokonceni udrzby, `Balicek pro vozidlo` a pristupne tray akcni okno maji explicitni resize nebo scroll regiony, aby dlouhy text a vetsi systemove pismo neschovaly primarni akce
- testovane generovani autostart zaznamu pro Linux `.desktop` a macOS LaunchAgent, vcetne cest s mezerami, uvozovkami a XML znaky
- multiplatformni publish matrix pro `.NET` desktop release
- publikovany release workflow pro tagy `dotnet-v<verze>` s verzovanymi balicky a checksumy
- runtime-specific desktop update manifesty `update/latest-dotnet-<rid>.ini`
- Windows release artefakt jako per-user Inno Setup instalator misto verejneho portable ZIPu
- Windows update tok pro Inno Setup installer: aplikace stahne a overi setup EXE v samostatnem progress dialogu se zrusenim, spusti ho interaktivne, pozada runtime o skutecne ukonceni aplikace misto schovani do tray a installer na konci nabidne spusteni nove verze
- Windows Inno update rezim prevezme jazyk predchozi instalace, stale vyzada licencni souhlas, ale preskoci volbu slozky, zkratky/zastupce i pripravenostni stranku; po licenci tak update pokracuje rovnou instalaci a na konci ponecha volbu spustit aplikaci.
- oddelene desktop kanaly `stable`, `beta` a `nightly` s vlastnim nazvem aplikace, update manifestem a systemovou datovou slozkou
- rolling nightly release se vytvari automaticky kazdou noc nebo rucnim workflow dispatch spustenim kanalu `nightly` a pouziva unikatni prerelease verzi `<verze>-nightly.<run>.<attempt>`, aby se nightly instalace dokazaly aktualizovat mezi jednotlivymi buildy
- legacy aliasy `update/latest-dotnet-preview-<rid>.ini`, ktere starym preview buildum umozni prejit na prvni stabilni desktop release
- lokalni release readiness skript `dotnet/build/Test-DotnetReleaseReadiness.ps1`, ktery postavi, otestuje, publikuje, zabali a overi manifest pro vybrany RID pred tagovanim
- release tag skript `dotnet/build/New-DotnetDesktopReleaseTag.ps1`, ktery vynuti cisty `main`, shodu s `origin/main`, neexistujici release tag, promotion gate pro `beta`/`stable` a readiness branu; push tagu na GitHub je explicitni volba `-Push`
- release promotion gate `dotnet/build/Test-DotnetReleasePromotion.ps1`, ktery hlida proces `nightly -> beta -> stable`, pred beta vyzaduje publikovany nightly tag i manifest, pred stable publikovany beta tag i manifest a pri uspesne kontrole vypise doporuceny kanalovy readiness wrapper i tagovaci prikaz
- post-release skript `dotnet/build/Test-DotnetPublishedRelease.ps1`, ktery po dobehnuti GitHub Actions overi manifest zvoleneho kanalu, release notes, asset, SHA-256 a velikost; kanalove wrappery `Test-DotnetPublishedNightly.ps1`, `Test-DotnetPublishedBeta.ps1` a `Test-DotnetPublishedStable.ps1` predavaji spravny kanal bez rucniho prepinani, stable navic hlida preview alias a AHK retirement gate
- release workflow po vygenerovani manifestu spousti `Test-DotnetPublishedRelease.ps1 -RuntimeIdentifier win-x64 -Channel <kanal> -SkipNetwork` jeste pred commitem do `update/`, aby nightly/stable manifest nesel publikovat s chybnym kanalem, assetem, hashem nebo velikosti
- publish workflow pred uploadem Windows artefaktu spousti `Test-DotnetInstallerSmoke.ps1`, aby se chybny Inno setup, checksum nebo JSON metadata nedostaly ani do GitHub release artefaktu
- release train status `dotnet/build/Get-DotnetReleaseTrainStatus.ps1`, ktery bez tagovani nebo publikovani shrne lokalni artefakty `nightly`, `beta`, `stable`, manifesty, remote tagy a dalsi doporuceny krok
- Windows hardening wrapper `dotnet/build/Test-DotnetWindowsHardening.ps1`, ktery pred beta kanalem spoji release train status, cele `dotnet test`, nightly readiness, volitelnou kontrolu publikovane nightly a informativni AHK retirement status; plny lokalni install smoke blokuje bez explicitniho `-AllowLocalInstallSmoke`, aby neprepsal uninstall registr existujici instalace stejneho kanalu
- AHK retirement readiness report `dotnet/build/Get-AhkRetirementReadiness.ps1`, ktery po stable release kontroluje stabilni manifest, release workflow, preview alias pro stare buildy a to, ze AHK-only artefakty zustavaji odstranene
- migracni parity report `dotnet/build/Get-DotnetMigrationParity.ps1`, ktery mapuje historicke `src/lib/*.ahk` moduly na konkretni .NET evidence soubory; Windows hardening i retirement gate ho spousti s `-FailOnBlockers`, aby po odstraneni AHK zustala dohledatelna parita modulu
- AHK retirement readiness report pouziva stejnou kanalovou strukturu lokalnich artefaktu jako tester, tedy `dotnet/artifacts/stable/<rid>/app/Vehimap.Desktop.exe` pro stable build misto historickeho `desktop-release`, a blokuje navrat `src/Vehimap.ahk`, `src/lib`, `src/tests` nebo generovanych AHK HTML vystupu
- channel-aware release readiness pro `stable`, `beta` i `nightly`; wrappery `Test-DotnetNightlyReadiness.ps1`, `Test-DotnetBetaReadiness.ps1` a `Test-DotnetStableReadiness.ps1` umi pred rucnim vydanim lokalne overit Inno instalator, metadata, SHA-256, velikost, odpovidajici update manifest a Windows setup EXE pres installer smoke
- lokalni release/readiness vystupy podle kanalu v `dotnet/artifacts/<stable|beta|nightly>/<rid>/app` pro spustitelnou aplikaci a `dotnet/artifacts/<stable|beta|nightly>/<rid>/release` pro instalator, metadata, checksum a manifest, aby tester nemusel vybirat mezi historickymi technickymi artefakty
- installer smoke `dotnet/build/Test-DotnetInstallerSmoke.ps1`, ktery ve vychozim rezimu overi Windows setup EXE, `.sha256` a JSON metadata a v CI nebo s explicitnim lokalnim potvrzenim provede izolovanou tichou instalaci, portable launch smoke a odinstalaci
- Windows packaging ma opt-in podpis Inno setupu pres `VEHIMAP_INNO_SIGNTOOL_COMMAND`; pokud je nastaveny validni sign tool command s placeholderem `$f`, baleni doplni `SignTool`, `SignedUninstaller` a retry direktivy bez potreby pouzivat nepristupny Inno Setup GUI dialog
- stable vetev release workflow spousti AHK retirement gate neprimo pres `Test-DotnetPublishedRelease.ps1`, aby prvni stabilni Windows kanal nesel publikovat s nefunkcnim update odkazem
- detailni stav automaticke instalace v dialogu kontroly aktualizaci, vcetne duvodu rucniho rezimu
- asset URL a SHA-256 hash v dialogu kontroly aktualizaci pro overitelnou rucni instalaci
- kopirovani detailu kontroly aktualizaci do schranky tlacitkem nebo `Ctrl+Shift+C` vcetne pristupneho stavoveho textu vysledku, aby sla rucni instalace overit bez opisovani URL a hashe
- odolnejsi kontrolu aktualizaci: poskozeny lokalni desktop manifest se preskoci a sluzba zkusi vzdaleny manifest
- odolnejsi aktualizacni orchestrace: chyba site/manifestu, otevreni externiho odkazu nebo spusteni updater helperu zustane uzivatelsky citelna a nespadne mimo shell
- regresni testy packaging skriptu a generatoru manifestu, vcetne kontroly, ze `.sha256`, metadata a fyzicky balicek souhlasi
- Windows CI Appium smoke nad publish buildem desktop release
- ulozeni `Tiskoveho prehledu` vozidel jako HTML souboru pres sdilenou exportni sluzbu a otevreni ulozeneho souboru
- rychle akce v Avalonia menu pro nejblizsi TK, zelenou kartu, vlastni pripominku, servisni udrzbu a doklad i pro filtrovanou kontrolu techto terminu; kontrola zelenych karet umi otevrit i vozidla s chybejici ZK
- primy vstup `Casova osa vozidla` v menu `Vozidlo`, aby C# shell zachoval stejnou informacni architekturu jako AHK menu
- primy vstup `Export terminu do kalendare (.ics)` v menu `Prehledy`, napojeny na sdileny kalendarovy export se stavovou hlaskou pro uspech, zruseni i selhani exportu
- nativni `InputGesture` popisky u hlavnich Avalonia menu polozek, aby menu ukazovalo stejne klavesove zkratky jako shell
- hlavni menu `Rychle akce` pouziva stejne dostupnostni podminky jako pristupne tray akcni okno, takze nejblizsi polozky ani filtrovane kontroly nejsou aktivni bez realneho cile nebo behem rozpracovane editace
- hlavni menu `Rychle akce` ma stejnou akci pro otevreni aktualniho upozorneni jako tray akcni okno a sdili s nim stejny background snapshot i blokovani pri rozpracovane editaci
- Appium smoke nad publish buildem uz aktualni upozorneni z menu `Rychle akce` nejen vidi, ale i aktivuje a overuje, ze shell otevre odpovidajici workspace
- CI Appium smoke nad publish buildem overuje i otevreni a zavreni pristupnych tray akci z menu `Aplikace`, aby byl klavesnicovy fallback k nativni tray oblasti soucasti bezne Windows kontroly
- nativni Windows tray menu v Avalonia zustava jen kratka convenience nabidka `Zobrazit Vehimap`, `Otevrit dashboard`, `Ukoncit Vehimap`; polozka `Akce Vehimapu` byla odstranena, protoze problem pro ctecky vznikal uz pri otevreni nativniho menu, a oficialni pristupna cesta je ted menu `Aplikace -> Akce na liste` nebo `Ctrl+Shift+Y`
- CI Appium smoke nad publish buildem uklada podporovane volby automatickych zaloh v dialogu `Nastaveni` a overuje, ze tlacitko `Zalohovat ihned` vytvori `.vehimapbak` v izolovane datove slozce
- CI Appium smoke nad publish buildem otevira a zavira app-level dialogy `Nastaveni`, `O programu` a `Zkontrolovat aktualizace` z menu `Aplikace`
- CI Appium smoke nad publish buildem overuje ulozeni volby Dashboardu pri startu z dialogu `Nastaveni`, propsani do Dashboardu i zustani dialogu otevreneho s citelnou validacni chybou pri neplatne hodnote
- CI Appium smoke nad publish buildem pri zalozeni noveho vozidla overuje i automaticke otevreni `Balicku pro vozidlo` se servisnimi plany, doklady a pripominkami
- CI Appium smoke nad publish buildem overuje i rucni otevreni `Balicku pro vozidlo` z menu `Vozidlo`
- CI Appium smoke nad publish buildem overuje i otevreni doporucenych servisnich sablon z pracovniho okna `Plan udrzby`
- CI Appium smoke nad publish buildem overuje i otevreni potvrzovaciho dialogu `Splneno` z pracovniho okna `Plan udrzby` i dashboardove akce `Dokoncit servis`
- CI Appium smoke nad publish buildem overuje prvni fokus po startu na seznamu vozidel, dostupnost seznamu pres `Tab` z filtru, navrat `Shift+Tab` z vybrane pracovni karty zpet na seznam, vyvolani hlavniho menu klavesou `F10`, zavreni menu druhym `F10` zpet na puvodni fokus, vynechani menu pri beznem `Tab` / `Shift+Tab` a otevreni aktualniho upozorneni z menu `Rychle akce`
- CI Appium smoke nad publish buildem overuje otevreni a zavreni vsech samostatnych pracovnich oken i zavreni workspace klavesou `Escape`
- CI Appium smoke nad publish buildem overuje navigaci z globalniho hledani, casove osy, blizicich se terminu, propadlych terminu a nakladoveho workspace na spravne navazujici workflow
- CI Appium smoke nad publish buildem overuje zakladni ulozeni editoru pripominek, dokladu, historie, tankovani a udrzby v samostatnych oknech
- CI Appium smoke nad publish buildem overuje `Shift+Tab` z nazvu vozidla na `Zrusit` a potvrzeni pri zavirani rozpracovane editace bez ztraty fokusu
- Appium smoke nad publish buildem overuje zkopirovani diagnostiky z dialogu `O programu` vcetne pristupneho stavoveho textu a obsahu systemove schranky
- Appium smoke nad publish buildem overuje zkopirovani detailu z dialogu kontroly aktualizaci vcetne pristupneho stavoveho textu a obsahu systemove schranky
- Appium smoke nad publish buildem overuje zkopirovani vyresene absolutni cesty spravovane prilohy dokladu vcetne pristupneho stavoveho textu a obsahu systemove schranky
- Appium smoke nad publish buildem overuje otevreni pristupnych `Akci Vehimapu na liste` z menu `Aplikace`, prvni fokus i aktivaci aktualniho upozorneni do spravneho workspace
- otevreni aktualni datove slozky z menu `Soubor` i z pristupneho tray akcniho okna pres sdileny multiplatformni file launcher; slozky pouzivaji samostatnou vetev s `explorer.exe` na Windows, `open` na macOS a `xdg-open` na Linuxu
- okamzita automaticka zaloha a otevreni slozky `data/auto-backups` jsou dostupne z menu `Soubor` i z pristupneho tray akcniho okna a sdileji stejne blokovani datovych akci pri rozpracovane editaci jako export/import zalohy
- ukonceni aplikace z menu `Soubor` polozkou `Konec` stejnym handlerem jako v menu `Aplikace`, aby F10 cesta zustala blizka AHK i bez ztraty samostatneho app menu
- pristupne tray akcni okno dostupne z nativniho tray i z menu `Aplikace`, s aktualnim stavem pozadi ze stejneho snapshotu jako tooltip/oznameni a s primym otevrenim hlavniho okna, dashboardu, blizicich se terminu, propadlych terminu, nejblizsi TK/ZK/pripominky/servisu/dokladu, filtrovanych kontrol, tiskoveho prehledu, exportu/importu zalohy, okamzite automaticke zalohy, otevreni slozky automatickych zaloh, exportu kalendare, znovunacteni dat, otevreni datove slozky, nastaveni, O programu, kontroly aktualizaci a ukonceni aplikace; dostupnost tlacitek se sklada z aktualnich dat shellu, aby nejblizsi polozky ani datove akce nebyly aktivni bez cile nebo behem rozpracovane editace
- app-level akce `Podekovat autorovi` dostupna z menu `Aplikace`, dialogu `O programu` a pristupneho tray okna; otevirani URL jde pres sdileny externi launcher, ne pres specialni platformni hack
- app-level akce `Nahlasit zpetnou vazbu` dostupna z menu `Aplikace` i pristupneho tray okna; otvira predvyplneny GitHub issue s verzi, kanalem, platformou, rezimem dat a zakladnimi pocty, ale bez soukromych cest nebo nazvu vozidel
- pristupne tray akcni okno umi z aktualniho stavu pozadi rovnou otevrit nejdulezitejsi termin k reseni, pripadne prvni auditni polozku, kdyz zadny termin prave nevyzaduje pozornost
- background snapshot pro tray tooltip a oznameni preferuje akutni terminy pred obecnym auditem dat a audit pouziva jako fallback, pokud zadny termin prave nevyzaduje pozornost
- background runtime se po uspesnem ulozeni evidence, importu zalohy, zmene nastaveni, rucnim reloadu nebo okamzite automaticke zaloze synchronizuje ze shell snapshotu hned a bez druheho legacy reloadu
- platformne oddelene desktopove oznameni: Windows balonkova vetev a ne-Windows inline fallback jsou testovane bez zavislosti na aktualnim OS runneru
- denni historie desktopovych oznameni v nastaveni datove sady, vcetne resetu po zmene reminder nastaveni nebo po obnoveni zalohy, aby .NET vetev neoznamovala stejny akutni termin porad dokola
- Windows resume hook pro background runtime: po probuzeni systemu se po 1500 ms provede stejna kontrola terminu, tray tooltipu a automatickych zaloh jako v AHK; pro macOS/Linux je vrstva pripravena jako no-op, dokud se platformy nebudou stabilizovat
- regresni kontrola desktop UI zdroju proti typickym mojibake znakum, aby ctecky obrazovky nedostavaly poskozenou UTF-8 diakritiku
- CI Appium smoke nad publish buildem kontroluje prvni fokus po startu na seznamu vozidel, dostupnost seznamu pres `Tab` z filtru, navrat `Shift+Tab` z vybrane pracovni karty zpet na seznam, app-level menu `Soubor`, menu `Rychle akce`, dostupnost zapnutych/vypnutych akci, vyvolani hlavniho menu pres `F10`, zavreni menu druhym `F10` zpet na puvodni fokus, vynechani menu pri beznem `Tab` / `Shift+Tab`, otevreni aktualniho upozorneni do spravneho workspace, otevreni/zavreni app-level dialogu a pristupnych tray akci z menu `Aplikace`, samostatna pracovni okna vcetne `Escape` zavreni, navigaci z globalniho hledani, casove osy, terminovych prehledu a nakladu, zakladni ulozeni editoru evidenci v samostatnych oknech, focus regresi `Shift+Tab` v editoru vozidla, ochranu rozpracovane editace, ulozeni automatickych zaloh z `Nastaveni` vcetne okamziteho vytvoreni `.vehimapbak`, ulozeni a validaci volby Dashboardu pri startu, post-create i rucni `Balicek pro vozidlo`, doporucene servisni sablony a potvrzeni `Splneno` z udrzby i Dashboardu. Rozsirena sada v izolovane portable kopii navic overuje kopirovani diagnostiky z `O programu`, kopirovani detailu kontroly aktualizaci, kopirovani vyresene cesty spravovane dokladove prilohy a dalsi realne vytvoreni okamzite automaticke zalohy

## Release a branch model

- Dlouhodoby vyvoj zustava na jedine vetvi `main`; kanaly `nightly`, `beta` a `stable` se neoddeluji trvalymi vetvemi, ale tagy, manifesty, instalatory, AppId a vlastnimi datovymi slozkami.
- `nightly` je rolling build z `main`, `beta` je overeny release candidate z konkretniho commitu na `main` a `stable` je povysena beta po testerske vlne bez P0/P1 blockeru.
- Po beta tagu plati release freeze: do stable patri jen P0/P1 opravy, dokumentace, release tooling a bezpecne overovaci opravy; nove funkce se odkladaji az po stable.
- Pokud beta najde zasadni blocker, oprava jde do `main`, zvysi se `src/VERSION` a vyda se nova beta s novym tagem. Existujici beta tag se neprepisuje.
- Kratkodobou vetev `release/<verze>` zalozit jen vyjimecne, pokud by bylo potreba udrzovat starsi beta/stable kandidat a soucasne rozjet dalsi velke nightly zmeny.

## Co je dalsi na rade

1. Udrzet Windows stable kanal jako baseline: po kazde release/tooling zmene spustit `Test-DotnetPublishedStable.ps1 -RuntimeIdentifier win-x64 -SkipNetwork` a `Get-AhkRetirementReadiness.ps1 -RuntimeIdentifier win-x64 -FailOnBlockers`, aby stable manifest, preview alias i odstraneni AHK-only artefaktu zustaly v zelenem stavu.
2. Radu 2.0 zatim drzet jako dlouhou nightly etapu, ne jako beta kandidata. Dalsi bezny vyvoj delat pres `nightly` na `main` a lokalne testovat `dotnet/artifacts/nightly/win-x64/app/Vehimap.Desktop.exe`.
3. Pred vetsimi storage nebo migracnimi zmenami spustit `Test-DotnetStorageNightlyGate.ps1 -RuntimeIdentifier win-x64`; gate pouziva anonymizovany legacy fixture balicek 1.0.2 a overuje migraci do SQLite, SQLite health check vcetne poskozene databaze, odklizeni zivych TSV/INI, SQLite-only runtime zapis, nove SQLite `.vehimapbak`, import stare `.vehimapbak`, `*.vehimapvehicle` balicky a source guard proti navratu legacy runtime zapisu.
4. Pred vetsim nightly posunem spustit `Test-DotnetWindowsHardening.ps1 -RuntimeIdentifier win-x64` a po GitHub Actions overit publikovanou nightly pres `Test-DotnetPublishedNightly.ps1 -RuntimeIdentifier win-x64`.
5. `Vehimap.Storage.Legacy` ponechat jako podporovanou kompatibilitni vrstvu po celou radu 2.x, ale nevracet ji jako runtime zapisovy format.
6. Lokalizacni zaklad pred Androidem drzet jako sdilenou infrastrukturu: Windows instalator predava vybrany jazyk pres jednorazovy `installer-preferences.json`, aplikace doplni chybejici locale/jednotkove volby do SQLite a aktivni runtime uz nepouziva `settings.ini`.
7. Po stabilizaci Windows 2.0 storage a i18n zakladu zacit Android vetvi jako dalsi platformu; nejdrive jen sdilena domena, SQLite storage a read-only shell nad testovacimi daty.
8. Po Android zakladu stabilizovat macOS desktop, hlavne VoiceOver, notarizaci, app bundle a rucni update tok.
9. Linux brat jako posledni platformu; az po macOS doresit distribuci, X11/Wayland pristupnost a Orca smoke.
