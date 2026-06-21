# Vehimap .NET rewrite

Tato slozka obsahuje novou C# codebase pro multiplatformni desktopovy Vehimap.

Aktualni zamer:

- zachovat soucasny AHK Vehimap funkcni beze zmen
- vedle nej vybudovat kompatibilni `.NET + Avalonia` aplikaci
- prvni priorita je prime cteni dnesnich `TSV`, `INI`, `.vehimapbak` a `data/attachments`

## Struktura

- `src/Vehimap.Domain` - ciste domenove modely
- `src/Vehimap.Application` - use cases a interni rozhrani
- `src/Vehimap.Storage.Legacy` - kompatibilita se soucasnymi AHK daty
- `src/Vehimap.Platform` - platform-specific adaptery
- `src/Vehimap.Desktop` - Avalonia desktop shell
- `src/Vehimap.Updater` - separatni helper pro update
- `tests/*` - unit, compat a UI testy

## Aktualni stav

Tato vetev uz neni jen scaffold. Aktualne umi:

- postavit celou solution pres `dotnet build`
- spustit unit a kompatibilitni testy pres `dotnet test`
- vygenerovat desktopovy preview build pres `dotnet publish`
- primo cist a zapisovat dnesni Vehimap data (`TSV`, `INI`, `.vehimapbak`, managed attachments)
- skladat sdilene C# use-cases pro audit, nakladove souhrny, cenu za kilometr, casovou osu vozidla a ICS export
- zobrazit v Avalonia shellu seznam vozidel, detail vybraneho vozidla, historii, tankovani, doklady, pripominky, plan udrzby, auditni frontu, naklady a casovou osu z realnych legacy dat
- v detailu vozidla zobrazit stav, stitky, posledni historicke zaznamy, posledni znamy tachometr a samostatne stavove souhrny historie, tankovani, pripominek, dokladu a udrzby stejne jako rychla kontrolni plocha v AHK verzi
- prejit z detailu vozidla pres pristupny blok `Souvisejici evidence` rovnou do historie, tankovani, pripominek, udrzby, dokladu, casove osy nebo nakladu vybraneho vozidla
- vyhledavat napric vozidly, historii, tankovanim, doklady, pripominkami a planem udrzby v nove karte `Hledani`, vcetne stitku, stavu, servisniho profilu, timeline statusu a identity vozidla u souvisejicich evidenci
- zobrazit flotilovy `Prehled terminu` a `Propadle terminy` nad stejnymi daty jako AHK verze a z obou pohledu skocit na spravne vozidlo nebo evidenci
- v `Blizicich se terminech` volitelne zobrazit i vozidla bez zelene karty a datove nedostatky z auditu; volby se ukladaji do legacy `settings.ini`
- v terminovych prehledech pouzit `Obnovit` nebo `Ctrl+R`; refresh zachova vyber a fokus vrati na seznam, pokud je v nem polozka
- radit terminove prehledy `Blizici se terminy` a `Propadle terminy` pres pristupne ovladace `Radit` a `Sestupne`; posledni sloupec i smer razeni se ukladaji do legacy `settings.ini`
- radit `Audit` a `Globalni hledani` pres pristupne ovladace `Radit` a `Sestupne`; posledni sloupec i smer razeni se ukladaji do legacy `settings.ini`, hledany text zustava jen docasny
- vyexportovat budouci terminy do `.ics` primo z nove C# vetve vcetne foldingu dlouhych iCalendar radku pro delsi popisy
- otevrit z casove osy souvisejici historii, doklad, pripominku nebo servisni plan primo na odpovidajici karte shellu; `Obnovit` nebo `Ctrl+R` prepocita casovou osu bez ztraty vyberu a vrati fokus na seznam
- pouzit prvni dashboard nad auditem, naklady a nejblizsimi terminy napric vozidly
- v dashboardu pouzit `Obnovit` nebo `Ctrl+R`; refresh prepocita auditni vyrez, naklady i nejblizsi terminy a zachova aktualni vyber
- v dashboardu pouzit horni akce `Hledat`, `Blizici se`, `Propadle`, `Zobrazit vozidlo` a `Upravit vozidlo`; stejne akce maji zkratky `Ctrl+F`, `Ctrl+T`, `Ctrl+Shift+T`, `Ctrl+O`, `Ctrl+P` a `Ctrl+U` / `F2`
- v dashboardu prepnout `Zobrazovat dashboard pri startu`; zmena se uklada do stejneho `settings.ini` jako dialog `Nastaveni`
- pouzit plny auditni workspace se samostatnym hledanim, tlacitky `Vymazat` a `Obnovit`, zkratkami `Ctrl+F`, `Ctrl+R`, `Ctrl+O`, `Ctrl+P`, `Ctrl+U` / `F2` a oddelenym dashboardovym top vyrezem
- pouzit nakladovy workspace s volbou predvolby obdobi nebo vlastniho datumoveho rozsahu, rychlym hledanim vozidel, tlacitky `Vymazat` a `Obnovit` a zkratkami `Ctrl+F` pro hledani, `Ctrl+R` pro obnovu prehledu, `Ctrl+P` pro precteni rozpadu nakladu, `Ctrl+O` nebo `Enter` pro otevreni vozidla a `Ctrl+U` / `F2` pro upravu vozidla
- ovladat shell vice klavesnici: `F5` pro znovunacteni, `Ctrl+E` pro export kalendare, `Ctrl+D` pro dashboard, `Ctrl+T` pro blizici se terminy, `Ctrl+Shift+T` pro propadle terminy, kontextove `Ctrl+F` pro hledani v aktivni pracovni plose, `Ctrl+Shift+F` pro globalni hledani a `Enter` pro otevreni vybranych polozek v casove ose, auditu, nakladech, dashboardu i ve vysledcich hledani
- primo vytvaret, upravovat a mazat `pripominky`
- primo vytvaret, upravovat a mazat `doklady`, vcetne volby `Spravovana kopie` vs `Externi cesta` a importu souboru do spravovanych priloh
- v editorech dokladu a pripominek pouzivat AHK-kompatibilni rozbalovaci hodnoty pro typ dokladu a opakovani pripominky
- v dialogu `Balicek pro vozidlo` pouzivat stejne rozbalovaci hodnoty a normalizaci pro typ dokladu a opakovani pripominky jako v beznych editorech
- u dokladu otevrit prilozeny soubor, otevrit jeho slozku a zkopirovat vyresenou cestu pres `Ctrl+Shift+C` nebo tlacitko `Kopirovat cestu`
- primo vytvaret, upravovat a mazat `historii`
- primo vytvaret, upravovat a mazat `tankovani`
- v editoru tankovani pouzivat AHK-kompatibilni rozbalovaci hodnoty pro typ paliva
- pri ukladani evidencnich editoru validovat a normalizovat datumy, tachometr, litry, castky, intervaly udrzby, terminy pripominek a platnost dokladu podle legacy AHK pravidel s navratem fokusu na chybne pole
- posunout opakovanou `pripominku` na dalsi termin tlacitkem `Dalsi termin` nebo zkratkou `Ctrl+Shift+N`
- primo vytvaret, upravovat a mazat `plan udrzby`
- v editoru `planu udrzby` vybrat beznou servisni sablonu, ktera predvyplni nazev ukonu, intervaly a poznamku
- doplnit chybejici doporucene servisni sablony v `Planu udrzby` tlacitkem `Doporucene` nebo zkratkou `Ctrl+Shift+N`; dialog pouziva stejny katalog jako balicek pro vozidlo, ale prida jen servisni plany
- oznacit vybrany servisni plan jako splneny tlacitkem `Splneno` nebo zkratkou `Ctrl+L`; akce otevre potvrzovaci dialog s datem, tachometrem a volitelnym zapisem stejne udalosti do historie vozidla
- primo vytvaret, upravovat a mazat `vozidla`, vcetne stitku, stavu vozidla, zakladniho servisniho profilu, AHK-kompatibilni normalizace stitku, kategorie, SPZ, terminu TK/ZK a rozbalovacich hodnot servisniho profilu, potvrzeni kaskadoveho odstraneni evidenci a uklidu spravovanych priloh
- otevrit `Naklady a souhrny` primo z vybraneho vozidla, zobrazit rozpad palivo / historie / doklady a exportovat souhrn TSV, detail TSV i HTML sestavu
- ulozit `Tiskovy prehled` vozidel jako HTML soubor pres standardni exportni dialog a po ulozeni ho otevrit pro tisk nebo archivaci
- otevrit `Historii`, `Tankovani`, `Připominky`, `Údrzbu` i `Doklady` v samostatnych desktopovych oknech nad stejnou editační logikou jako hlavni shell
- otevrit `Detail vozidla`, `Audit` a `Dashboard` i v samostatnych desktopovych oknech nad stejnym viewmodelem jako hlavni shell
- sdilet lifecycle samostatnych workspace oken: prvni fokus po otevreni i potvrzeni zavreni rozpracovaneho editoru jde pres jeden helper
- drzet nazvy samostatnych workspace oken ve workspace viewmodelech misto verejnych root aliasu v `MainWindowViewModel`
- drzet action-state podminky evidencnich editoru, dokladovych priloh a exportu nakladu jako interni soucast prikazu a workspace vrstvy misto verejneho root API shellu
- sdilet i samotne otevirani workspace oken z hlavniho shellu: potvrzeni rozpracovane editace, prepnuti karty, modalni okno a navrat fokusu maji jeden helper
- drzet stav `Casove osy` a `Globalniho hledani` primo v jejich workspace viewmodelech misto duplicitnich root proxy vlastnosti v `MainWindowViewModel`
- drzet nactene kolekce polozek `Casove osy` a vysledku `Globalniho hledani` primo v jejich workspace viewmodelech misto verejnych root vlastnosti
- drzet stav terminovych prehledu `Blizici se terminy` a `Propadle terminy` primo v jejich workspace viewmodelech misto duplicitnich root proxy vlastnosti v `MainWindowViewModel`
- drzet nactene kolekce polozek terminovych prehledu `Blizici se terminy` a `Propadle terminy` primo v jejich workspace viewmodelech misto verejnych root vlastnosti
- drzet dashboardove souhrny auditu, nakladu a nejblizsich terminu ve sdilenych workspace stavech auditu, nakladu a dashboardu misto root proxy vlastnosti
- drzet plny auditni seznam v `AuditWorkspace`, dashboardovy auditni vyrez a nejblizsi terminy v `DashboardWorkspace` a flotilovy seznam nakladu v `CostWorkspace` misto verejnych root kolekci
- drzet volby nakladoveho obdobi, terminovych filtru, sablon udrzby a rezimu dokladovych priloh primo v odpovidajicich workspace viewmodelech misto root proxy vlastnosti
- drzet stav exportu nakladoveho prehledu primo v `CostWorkspace`, zatimco root `MainWindowViewModel` zustava jen orchestratorem exportnich prikazu
- drzet souhrnne texty evidenci `Historie`, `Tankovani`, `Pripominky`, `Udrzba` a `Doklady` primo v jejich workspace viewmodelech misto root proxy vlastnosti
- drzet nactene kolekce zaznamu evidenci `Historie`, `Tankovani`, `Pripominky`, `Udrzba` a `Doklady` primo v jejich workspace viewmodelech misto verejnych root vlastnosti
- drzet vyber polozky, detailni text a stav editoru `Historie`, `Tankovani`, `Pripominky`, `Udrzba` a `Doklady` primo v jejich workspace viewmodelech misto verejnych root aliasu
- drzet souhrnne texty detailu vozidla primo ve `VehicleDetailWorkspace`, zatimco root je jen prepocitava pri zmene vyberu vozidla
- drzet formularove hodnoty editoru vozidla primo ve `VehicleDetailWorkspace`, zatimco root zustava zodpovedny za prikazy, validaci a ulozeni
- drzet rezim editace vozidla, viditelnost detailu a nadpis detailniho panelu primo ve `VehicleDetailWorkspace`, zatimco root jen synchronizuje shellove prikazy
- drzet keyboard-first a11y i v hlavnim shellu, vcetne vlastni focusovatelne listy karet a konzistentniho focusu pro ctecky obrazovky
- vymazat filtry hlavniho seznamu vozidel tlacitkem `Vymazat filtry`; obnovi cely seznam, zapise stavovou hlasku a vrati fokus do hledani vozidel
- zamknout hlavni seznam vozidel, jeho filtry, prepinani pracovnich karet a otevirani jinych workspace oken po dobu aktivni editace, aby se nedalo omylem odejit z rozpracovane prace pred ulozenim nebo zrusenim editoru
- pouzivat stejne workspace zkratky v kartach i samostatnych oknech: `Ctrl+F` pro hledani, `Ctrl+R` pro obnovu prehledovych vysledku, `Ctrl+O` pro vozidlo/vysledek a `Ctrl+P` pro otevreni resene polozky v casove ose, hledani i terminovych prehledech; v nakladech `Ctrl+P` presune fokus na rozpad vybraneho vozidla a v hlavnim shellu maji kontextove zkratky prednost pred globalnim otevrenim vozidla nebo dokladu
- filtrovat evidencni seznamy `Historie`, `Tankovani`, `Pripominky`, `Plan udrzby` a `Doklady` vlastnim rychlym hledanim; tlacitko `Vymazat` filtr smaze, vrati fokus do hledani, filtr zachova vyber podle ID a pri prazdnem vysledku vypne akce nad vybranou polozkou
- radit evidencni seznamy `Historie`, `Tankovani`, `Pripominky`, `Plan udrzby` a `Doklady` pres pristupne ovladace `Radit` a `Sestupne`; posledni sloupec i smer razeni se ukladaji do legacy `settings.ini`
- mazat rychle hledani stejnym tlacitkem `Vymazat` i v prehledovych workspacech `Casova osa`, `Globalni hledani`, `Audit`, `Naklady`, `Blizici se terminy` a `Propadle terminy`; po smazani se fokus vrati do prislusneho hledani
- pouzivat kontextove editacni zkratky v evidencnich workspacech: `Ctrl+N` pro novou polozku, `Ctrl+U` nebo `F2` pro upravu vybrane polozky, `Ctrl+S` pro ulozeni aktivniho editoru a v dokladech `Ctrl+O` / `Ctrl+Shift+O` pro otevreni prilohy nebo slozky
- otevrit modalni `Nastaveni`, `O programu` a `Zkontrolovat aktualizace` primo z desktop shellu
- ridit dostupnost `Minimalizovat na listu` z viewmodelu podle podpory tray a v nastaveni zneaktivnit intervalova pole, pokud nejsou zapnute pravidelne automaticke zalohy
- ovladat `Nastaveni` klavesnici: `Ctrl+S` ulozi, `Ctrl+B` ulozi a vytvori zalohu ihned, `Esc` zavre dialog bez ulozeni
- ovladat app-level dialogy konzistentne klavesnici: `O programu` ma `Ctrl+O` pro release poznamky, `Ctrl+Shift+C` pro zkopirovani diagnostiky a `Esc` pro zavreni, potvrzovaci dialogy, kontrola aktualizaci, upozorneni i tray akce maji `Esc` pro bezpecne zavreni bez nove akce
- pouzivat `Rychle akce` pro nejblizsi TK, zelenou kartu, vlastni pripominku, servisni udrzbu a doklady i pro filtrovanou kontrolu techto terminu v prehledech; `Zkontrolovat ZK` otevira i vozidla bez vyplnene zelene karty
- otevrit z pristupneho tray okna hlavni okno, dashboard, blizici se terminy, propadle terminy, nejblizsi TK/ZK/pripominku/servis/doklad, filtrovanou kontrolu techto oblasti, tiskovy prehled, export/import zalohy, export kalendare, znovunacteni dat, nastaveni, O programu, kontrolu aktualizaci nebo ukoncit aplikaci bez nativniho focus problemu tray menu
- v background snapshotu pro tray tooltip a oznameni uprednostnit akutni terminy pred obecnym auditem dat, aby propadle nebo blizici se TK/ZK/pripominky/servis/doklady nezapadly za mene nalehavym datovym nedostatkem
- hlidat desktop UI zdroje proti typickym mojibake znakum, aby se poskozena UTF-8 diakritika nevratila do pristupnych nazvu ani textu pro ctecky obrazovky
- vystavovat souhrnne, stavove a detailni texty v hlavnim shellu i workspacech pres explicitni pristupne nazvy a stabilni `AutomationId`, aby je mohly cist screen readery a kontrolovat UI testy
- cist a zapisovat podporovane reminder volby do stejneho `settings.ini` jako AHK verze a respektovat `show_dashboard_on_launch`
- reportovat stejnou verzi jako root `src/VERSION`, vcetne file version pro desktop buildy
- kontrolovat `update/latest.ini` kompatibilne s AHK vetvi a na Windows pripravit automatickou instalaci pres `Vehimap.Updater`
- v dialogu kontroly aktualizaci ukazat duvod, proc automaticka instalace neni dostupna, napriklad chybejici updater, nepublikovany build, nepodporovanou platformu nebo nekompletni metadata manifestu
- v dialogu kontroly aktualizaci zobrazit asset URL a SHA-256 hash, aby rucni instalace sla overit bez dohledavani v manifestu
- v dialogu kontroly aktualizaci zkopirovat cely vysledek vcetne verzi, asset URL a SHA-256 do schranky tlacitkem nebo `Ctrl+Shift+C`
- pri poskozenem lokalnim preview manifestu preskocit lokalni override a zkusit bezny vzdaleny manifest, aby testovaci soubor v `update/` nezablokoval kontrolu aktualizaci
- otevrit modalni export a obnovu dat a pracovat se stejnym `.vehimapbak` formatem jako AHK vetev
- generovat verzovane release balicky pro `win-x64`, `linux-x64`, `osx-x64` a `osx-arm64`
- pripravit draft release pro `.NET` preview tag `dotnet-preview-v<verze>` pres GitHub Actions
- publikovat runtime-specific preview manifesty `update/latest-dotnet-preview-<rid>.ini`
- spoustet Windows Appium smoke i v CI nad publish buildem desktop preview

## Lokalni build

```powershell
cd dotnet
dotnet restore
dotnet test
dotnet build
dotnet publish .\src\Vehimap.Desktop\Vehimap.Desktop.csproj -c Release -o .\artifacts\desktop-preview
```

## Release balicky

Lokalni packaging publish vystupu:

```powershell
cd dotnet
dotnet publish .\src\Vehimap.Desktop\Vehimap.Desktop.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\win-x64\desktop
.\build\Package-DesktopRelease.ps1 -PublishDirectory .\artifacts\win-x64\desktop -RuntimeIdentifier win-x64 -Version (Get-Content ..\src\VERSION).Trim() -OutputDirectory .\artifacts\win-x64\release
```

Balickovaci skript vynechava `.pdb`, vytvori archiv pro dany RID, prida `.sha256` a JSON metadata s `runtimeIdentifier`, nazvem balicku, hashem a velikosti. Generator preview update manifestu z techto metadat znovu overuje fyzicky hash i velikost balicku, aby manifest nemohl ukazovat na poskozeny nebo zamereny artefakt. Kontrola aktualizaci umi lokalni preview manifest pouzit jako override, ale pokud je lokalni soubor poskozeny, pokracuje na vzdaleny manifest. Dialog kontroly aktualizaci navic ukazuje, proc je update jen rucni, pokud automaticka instalace neni dostupna, vypise asset URL i SHA-256 pro overeni stazeneho balicku a umi tyto detaily zkopirovat do schranky tlacitkem nebo `Ctrl+Shift+C`.

CI workflow `.github/workflows/dotnet-desktop.yml` umi:

- otestovat `.NET` vetev na Windows
- publikovat self-contained desktop buildy pro Windows, Linux a macOS
- zabalit je do verzovanych artefaktu:
  - `vehimap-desktop-preview-<verze>-win-x64.zip`
  - `vehimap-desktop-preview-<verze>-linux-x64.tar.gz`
  - `vehimap-desktop-preview-<verze>-osx-x64.zip`
  - `vehimap-desktop-preview-<verze>-osx-arm64.zip`
- pridat ke kazdemu balicku `.sha256` a `.json` metadata
- pri pushi tagu `dotnet-preview-v<verze>` vytvorit draft GitHub release
- po draft release zapsat runtime-specific preview manifesty do `update/`
- na Windows runneru spustit Appium smoke nad publish buildem desktop preview
