# Vehimap .NET rewrite

Tato slozka obsahuje novou C# codebase pro multiplatformni desktopovy Vehimap.

Aktualni zamer:

- brat `.NET + Avalonia` jako primarni desktopovou vetev Vehimapu
- drzet C# Avalonia jako jedinou aktivni aplikaci po finalnim AHK retirement commitu
- pouzivat od rady 2.0 primarne SQLite databazi `data/vehimap.db`
- zachovat legacy kompatibilitu dat jako jednorazovou migraci/import bez navratu AHK-only runtime, knihoven nebo testu
- cist legacy `TSV`, `INI`, starsi `.vehimapbak` a `data/attachments` pres `Vehimap.Storage.Legacy` jen pro migraci z 1.0.2 a import starsich zaloh; po uspesne migraci zive TSV/INI soubory z `data/` odlozit mimo runtime koren
- release priorita je nejdrive stabilni Windows desktop pres Inno Setup instalator, potom Android, nasledne macOS a nakonec Linux
- rada 2.0 zustava po zavedeni SQLite v delsi nightly stabilizaci; beta/stable se neplanuji, dokud storage gate a testerska vlna nebudou bez blockeru

## Struktura

- `src/Vehimap.Domain` - ciste domenove modely
- `src/Vehimap.Application` - use cases a interni rozhrani
- `src/Vehimap.Storage.Legacy` - kompatibilita a migrace soucasnych AHK/1.0.2 dat
- `src/Vehimap.Storage.Sqlite` - primarni runtime storage Vehimapu 2.0
- `src/Vehimap.Platform` - platform-specific adaptery
- `src/Vehimap.Desktop` - Avalonia desktop shell
- `src/Vehimap.Updater` - separatni helper pro update
- `tests/*` - unit, compat a UI testy

## Aktualni stav

Tato vetev uz neni jen scaffold. Aktualne umi:

- postavit celou solution pres `dotnet build`
- spustit unit a kompatibilitni testy pres `dotnet test`
- vygenerovat desktopovy release build pres `dotnet publish`
- primarne cist a zapisovat datovou sadu 2.0 v SQLite `data/vehimap.db`
- pri prvnim startu nad daty 1.0.2 automaticky vytvorit predmigracni kopii v `data/migration-backups/<cas>`, nacist legacy `TSV/INI`, zapsat data do SQLite v jedne transakcni storage ceste a po overenem nacteni `vehimap.db` presunout zive TSV/INI do `data/migration-backups/<cas>/removed-from-data-root/`
- ulozit cestu k predmigracni kopii do nastaveni datove sady a otevrit ji z menu `Soubor`, pokud tato slozka existuje
- pri startu s uz existujicim `vehimap.db` uklidit pripadne zbytky TSV/INI po prvni SQLite nightly bez opakovaneho importu; `data/attachments` zustava aktivni slozka spravovanych priloh
- overit SQLite 2.0 storage cestu nad anonymizovanym legacy fixture balickem prikazem `dotnet\build\Test-DotnetStorageNightlyGate.ps1 -RuntimeIdentifier win-x64`
- hlidat SQLite-only runtime zapis stejnou storage nightly branou: po migraci bezny save meni `vehimap.db`, nevytvari zive `TSV/INI` soubory a source guard blokuje navrat legacy backup/storage konstrukci do desktop runtime vrstev
- zkontrolovat zdravi datove sady 2.0 rucne z menu `Soubor -> Zkontrolovat datovou sadu 2.0`; kontrola overi `vehimap.db`, `PRAGMA quick_check`, schema, zapisovatelnost datove slozky, aktivni `attachments` a pripadne zbytky legacy `TSV/INI`, pricemz poskozenou databazi sama nemaze ani neopravuje
- importovat starsi textove `.vehimapbak` zalohy pres legacy parser a obnovit je uz do SQLite runtime storage
- exportovat nove `.vehimapbak` zalohy jako SQLite backup s `vehimap.db` a referenced spravovanymi prilohami
- exportovat a importovat jedno vozidlo jako `*.vehimapvehicle` balicek vcetne historie, tankovani, dokladu, pripominek, udrzby a relevantnich spravovanych priloh
- pri poskozenem legacy `TSV` nebo `INI` souboru ukazat konkretni nazev souboru, plnou cestu a parser detail v chybovem stavu shellu
- pri nedostupnem nebo poskozenem `.vehimapbak` souboru ukazat cestu k zaloze a parser/I/O detail ve stavovem textu misto neobslouzene vyjimky v shellu
- pri zruseni nebo selhani app-level akci z menu/tray, jako jsou nastaveni, export/import zalohy, O programu a kontrola aktualizaci, zapsat citelnou stavovou hlasku misto neobslouzene vyjimky
- otevrit stranku `Podekovat autorovi` z menu `Aplikace`, dialogu `O programu` i pristupneho tray okna pres stejny externi launcher jako release poznamky nebo update odkazy
- otevrit predvyplneny GitHub issue `Nahlasit zpetnou vazbu` z menu `Aplikace` i pristupneho tray okna; sablona doplni verzi, kanal, platformu a zakladni pocty, ale zamerne neobsahuje datovou slozku ani nazvy vozidel
- pri exportu `.vehimapbak` hlasit pocet zahrnutych spravovanych priloh a pocet chybejicich spravovanych priloh, ktere byly bezpecne preskoceny stejne jako v AHK vetvi
- skladat sdilene C# use-cases pro audit, nakladove souhrny, cenu za kilometr, casovou osu vozidla a ICS export
- otevrit pristupnou `Servisni knizku` vybraneho vozidla z menu `Vozidlo` nebo detailu vozidla; sklada historii, servisni plany a servisni doklady ze soucasnych dat bez migrace, umi prejit na souvisejici evidenci a exportovat HTML pro tisk nebo archivaci
- otevrit offline `Chytry poradce` z menu `Prehledy`, karty shellu nebo samostatneho okna; pravidla jsou deterministicka, nepouzivaji AI ani externi API a skladaji doporuceni z auditu, terminu, udrzby, tankovaci analyzy, dokladovych priloh a zakladnich nakladovych signalu
- zobrazit v Avalonia shellu seznam vozidel, detail vybraneho vozidla, historii, tankovani, doklady, pripominky, plan udrzby, auditni frontu, naklady a casovou osu z realnych dat po legacy migraci nebo primo ze SQLite
- v detailu vozidla zobrazit stav, stitky, posledni historicke zaznamy, posledni znamy tachometr a samostatne stavove souhrny historie, tankovani, pripominek, dokladu a udrzby stejne jako rychla kontrolni plocha v AHK verzi
- prejit z detailu vozidla pres pristupny blok `Souvisejici evidence` rovnou do historie, tankovani, pripominek, udrzby, dokladu, casove osy, servisni knizky nebo nakladu vybraneho vozidla
- vyhledavat napric vozidly, historii, tankovanim, doklady, pripominkami a planem udrzby v nove karte `Hledani`, vcetne stitku, stavu, servisniho profilu, timeline statusu a identity vozidla u souvisejicich evidenci
- zobrazit flotilovy `Prehled terminu` a `Propadle terminy` nad stejnymi daty jako AHK verze a z obou pohledu skocit na spravne vozidlo nebo evidenci
- v `Blizicich se terminech` volitelne zobrazit i vozidla bez zelene karty a datove nedostatky z auditu; volby se ukladaji do nastaveni datove sady
- v terminovych prehledech pouzit `Obnovit` nebo `Ctrl+R`; refresh zachova vyber a fokus vrati na seznam, pokud je v nem polozka
- radit terminove prehledy `Blizici se terminy` a `Propadle terminy` pres pristupne ovladace `Radit` a `Sestupne`; posledni sloupec i smer razeni se ukladaji do nastaveni datove sady
- radit `Audit` a `Globalni hledani` pres pristupne ovladace `Radit` a `Sestupne`; posledni sloupec i smer razeni se ukladaji do nastaveni datove sady, hledany text zustava jen docasny
- vyexportovat budouci terminy do `.ics` primo z nove C# vetve vcetne foldingu dlouhych iCalendar radku pro delsi popisy
- hlasit uspech, zruseni i selhani exportu `.ics` ve stavovem textu casove osy i hlavniho shellu, aby menu a tray akce neskoncily neobslouzenou vyjimkou
- otevrit z casove osy souvisejici historii, doklad, pripominku nebo servisni plan primo na odpovidajici karte shellu; `Obnovit` nebo `Ctrl+R` prepocita casovou osu bez ztraty vyberu a vrati fokus na seznam
- pouzit prvni dashboard nad auditem, naklady a nejblizsimi terminy napric vozidly
- v dashboardu pouzit `Obnovit` nebo `Ctrl+R`; refresh prepocita auditni vyrez, naklady i nejblizsi terminy a zachova aktualni vyber
- v dashboardu pouzit horni akce `Hledat`, `Souhrn nakladu`, `Blizici se`, `Propadle`, `Zobrazit vozidlo`, `Historie vozidla`, `Naklady vozidla`, `Dokoncit servis` a `Upravit vozidlo`; stejne akce maji zkratky `Ctrl+F`, `Ctrl+T`, `Ctrl+Shift+T`, `Ctrl+O`, `Ctrl+P`, `Ctrl+H`, `Ctrl+L` a `Ctrl+U` / `F2`
- v dashboardu prepnout `Zobrazovat dashboard pri startu`; zmena se uklada do stejneho nastaveni datove sady jako dialog `Nastaveni`
- pouzit plny auditni workspace se samostatnym hledanim, tlacitky `Vymazat` a `Obnovit`, zkratkami `Ctrl+F`, `Ctrl+R`, `Ctrl+O`, `Ctrl+P`, `Ctrl+U` / `F2` a oddelenym dashboardovym top vyrezem
- pouzit workspace `Chytry poradce` se souhrnem doporuceni, filtry podle priority, kategorie, vozidla a textu, zkratkami `Ctrl+F`, `Ctrl+R`, `Enter` / `Ctrl+O` a navigaci na souvisejici vozidlo nebo evidenci bez rucniho hledani
- pouzit nakladovy workspace s volbou predvolby obdobi nebo vlastniho datumoveho rozsahu, rychlym hledanim vozidel, tlacitky `Vymazat` a `Obnovit` a zkratkami `Ctrl+F` pro hledani, `Ctrl+R` pro obnovu prehledu, `Ctrl+P` pro precteni rozpadu nakladu, `Ctrl+O` nebo `Enter` pro otevreni vozidla a `Ctrl+U` / `F2` pro upravu vozidla
- ovladat shell vice klavesnici: `F5` pro znovunacteni, `Ctrl+E` pro export kalendare, `Ctrl+D` pro dashboard, `Ctrl+T` pro blizici se terminy, `Ctrl+Shift+T` pro propadle terminy, kontextove `Ctrl+F` pro hledani v aktivni pracovni plose, `Ctrl+Shift+F` pro globalni hledani a `Enter` pro otevreni vybranych polozek v casove ose, auditu, nakladech, dashboardu i ve vysledcich hledani
- primo vytvaret, upravovat a mazat `pripominky`
- primo vytvaret, upravovat a mazat `doklady`, vcetne volby `Spravovana kopie` vs `Externi cesta` a importu souboru do spravovanych priloh
- v editorech dokladu a pripominek pouzivat AHK-kompatibilni rozbalovaci hodnoty pro typ dokladu a opakovani pripominky
- v dialogu `Balicek pro vozidlo` pouzivat stejne rozbalovaci hodnoty a normalizaci pro typ dokladu a opakovani pripominky jako v beznych editorech
- u dokladu otevrit prilozeny soubor, otevrit jeho slozku a zkopirovat vyresenou cestu pres `Ctrl+Shift+C` nebo tlacitko `Kopirovat cestu`; uspech i chyba techto akci se propisuje do stavoveho textu dokladu i hlavniho shellu
- primo vytvaret, upravovat a mazat `historii`
- primo vytvaret, upravovat a mazat `tankovani`
- v editoru tankovani pouzivat AHK-kompatibilni rozbalovaci hodnoty pro typ paliva
- v editoru tankovani evidovat i detail paliva, napr. Natural 95/98 nebo komercni produkt, a misto tankovani; nove zapisy jdou do `# Vehimap fuel v2`, zatimco `fuel v1` se stale nacita jako kompatibilni legacy format
- v karte i samostatnem okne `Tankovani` zobrazit pristupnou analyzu bez zmeny datoveho formatu: spotrebu mezi plnymi nadrzemi, prumernou cenu za litr, souhrny podle mista/paliva a opatrna upozorneni na neciselne hodnoty, klesajici tachometr nebo vyrazne odchylky
- pri ukladani evidencnich editoru validovat a normalizovat datumy, tachometr, litry, castky, intervaly udrzby, terminy pripominek a platnost dokladu podle legacy AHK pravidel s navratem fokusu na chybne pole
- v editorech chranit standardni textovou navigaci sipkami, Home/End, Backspace/Delete a beznymi Ctrl zkratkami, rozbalit ComboBox sipkou nahoru/dolu bez nutnosti `Alt+Down` a po ulozeni vozidla vratit fokus na prvni logickou akci detailu `Upravit vozidlo`
- pri selhani zapisu evidencnich editoru ponechat rozpracovany editor otevreny, zapsat chybu do stavoveho textu editoru i shellu a vratit fokus na smysluplny prvek
- pri selhani zapisu vratit session dataset na snapshot pred zmenou a mazat managed prilohy nebo slozku priloh vozidla az po uspesnem persistu
- posunout opakovanou `pripominku` na dalsi termin tlacitkem `Dalsi termin` nebo zkratkou `Ctrl+Shift+N`
- primo vytvaret, upravovat a mazat `plan udrzby`
- v editoru `planu udrzby` vybrat beznou servisni sablonu, ktera predvyplni nazev ukonu, intervaly a poznamku
- v editoru `planu udrzby` pouzit rucni sablonu `Pravidelny servis` pro jeden souhrnny ukon zahrnujici motorovy olej, olejovy filtr a bezne filtry
- v editoru `planu udrzby` vybirat rozsirene rucni sablony clenene podle oblasti `Motor`, `Podvozek`, `Vyfukove potrubi` a `Elektronika`
- doplnit chybejici doporucene servisni sablony v `Planu udrzby` tlacitkem `Doporucene` nebo zkratkou `Ctrl+Shift+N`; dialog pouziva stejny katalog jako balicek pro vozidlo, ale prida jen servisni plany
- oznacit vybrany servisni plan jako splneny tlacitkem `Splneno` nebo zkratkou `Ctrl+L`; akce otevre potvrzovaci dialog s datem, tachometrem a volitelnym zapisem stejne udalosti do historie vozidla
- primo vytvaret, upravovat a mazat `vozidla`, vcetne stitku, stavu vozidla, zakladniho servisniho profilu, AHK-kompatibilni normalizace stitku, kategorie, SPZ, terminu TK/ZK a rozbalovacich hodnot servisniho profilu, potvrzeni kaskadoveho odstraneni evidenci a uklidu spravovanych priloh
- otevrit `Naklady a souhrny` primo z vybraneho vozidla, zobrazit rozpad palivo / historie / doklady a exportovat souhrn TSV, detail TSV i HTML sestavu vcetne stavove hlasky pro uspech, zruseni, selhani ulozeni i selhani otevreni HTML vystupu
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
- drzet keyboard-first a11y i v hlavnim shellu, vcetne vlastni focusovatelne listy karet, explicitniho tab stopu hlavniho seznamu vozidel a konzistentniho focusu pro ctecky obrazovky
- vymazat filtry hlavniho seznamu vozidel tlacitkem `Vymazat filtry`; obnovi cely seznam, zapise stavovou hlasku a vrati fokus do hledani vozidel
- zamknout hlavni seznam vozidel, jeho filtry, prepinani pracovnich karet a otevirani jinych workspace oken po dobu aktivni editace, aby se nedalo omylem odejit z rozpracovane prace pred ulozenim nebo zrusenim editoru
- pouzivat stejne workspace zkratky v kartach i samostatnych oknech: `Ctrl+F` pro hledani, `Ctrl+R` pro obnovu prehledovych vysledku, `Ctrl+O` pro vozidlo/vysledek a `Ctrl+P` pro otevreni resene polozky v casove ose, hledani i terminovych prehledech; v nakladech `Ctrl+P` presune fokus na rozpad vybraneho vozidla a v hlavnim shellu maji kontextove zkratky prednost pred globalnim otevrenim vozidla nebo dokladu
- filtrovat evidencni seznamy `Historie`, `Tankovani`, `Pripominky`, `Plan udrzby` a `Doklady` vlastnim rychlym hledanim; tlacitko `Vymazat` filtr smaze, vrati fokus do hledani, filtr zachova vyber podle ID a pri prazdnem vysledku vypne akce nad vybranou polozkou
- radit evidencni seznamy `Historie`, `Tankovani`, `Pripominky`, `Plan udrzby` a `Doklady` pres pristupne ovladace `Radit` a `Sestupne`; posledni sloupec i smer razeni se ukladaji do nastaveni datove sady
- mazat rychle hledani stejnym tlacitkem `Vymazat` i v prehledovych workspacech `Casova osa`, `Globalni hledani`, `Audit`, `Naklady`, `Blizici se terminy` a `Propadle terminy`; po smazani se fokus vrati do prislusneho hledani
- pouzivat kontextove editacni zkratky v evidencnich workspacech: `Ctrl+N` pro novou polozku, `Ctrl+U` nebo `F2` pro upravu vybrane polozky, `Ctrl+S` pro ulozeni aktivniho editoru a v dokladech `Ctrl+O` / `Ctrl+Shift+O` pro otevreni prilohy nebo slozky
- v hlavni karte `Historie` a `Tankovani` zobrazit editacni akce primo bez nutnosti nejdriv otevrit samostatne okno; tlacitko `V okne` zustava dostupne pro prehlednejsi modalni praci
- chranit editory evidenci vlastnim svislym scrollem a prehledy `Dashboard` a `Naklady` celostrankovym scrollem, aby vetsi systemove pismo nebo mensi viewport neschovaly spodni obsah
- otevrit modalni `Nastaveni`, `O programu` a `Zkontrolovat aktualizace` primo z desktop shellu
- ridit dostupnost `Minimalizovat na listu` z viewmodelu podle podpory tray a v nastaveni zneaktivnit intervalova pole, pokud nejsou zapnute pravidelne automaticke zalohy
- ovladat `Nastaveni` klavesnici: `Ctrl+S` ulozi, `Ctrl+B` ulozi a vytvori zalohu ihned, `Esc` zavre dialog bez ulozeni
- ovladat app-level dialogy a akce konzistentne klavesnici: `O programu` ukazuje bezny souhrn aplikace, autora `by Vlcek apps` a verzi vcetne kanalu, technickou diagnostiku schovava pod `Zobrazit diagnosticka data`, ma `Ctrl+O` pro release poznamky, `Ctrl+K` pro podekovani autorovi, `Ctrl+Shift+C` pro zkopirovani diagnostiky, pristupny stav vysledku kopirovani a `Esc` pro zavreni; `Nahlasit zpetnou vazbu` otevre bezpecne predvyplneny GitHub issue a potvrzovaci dialogy, kontrola aktualizaci, upozorneni i tray akce maji `Esc` pro bezpecne zavreni bez nove akce
- otevrit pristupne `Akce Vehimapu na liste` i z menu `Aplikace`, aby stejny dialog jako nativni tray slo vyvolat pres F10/Alt bez zavislosti na oznamovaci oblasti
- ukoncit aplikaci z menu `Soubor` pres polozku `Konec` i z menu `Aplikace`, aby F10 tok odpovidal klasickemu desktopovemu rozvrzeni i AHK parite
- generovat a testovat autostart zaznamy pro Linux `.desktop` a macOS LaunchAgent vcetne bezpecneho escapovani cest, argumentu a XML znaku
- otevirat soubory a slozky pres oddelene vetve multiplatformniho launcheru; slozky jdou na Windows pres `explorer.exe`, na macOS pres `open` a na Linuxu pres `xdg-open`
- spustit okamzitou automatickou zalohu a otevrit slozku `data/auto-backups` primo z menu `Soubor` i z pristupneho tray okna; tyto datove akce se vypinaji pri rozpracovane editaci stejne jako export/import zalohy
- pouzivat `Rychle akce` pro nejblizsi TK, zelenou kartu, vlastni pripominku, servisni udrzbu a doklady i pro filtrovanou kontrolu techto terminu v prehledech; `Zkontrolovat ZK` otevira i vozidla bez vyplnene zelene karty a menu polozky bez aktualniho cile jsou neaktivni
- otevrit z pristupneho tray okna hlavni okno, dashboard, blizici se terminy, propadle terminy, nejblizsi TK/ZK/pripominku/servis/doklad, filtrovanou kontrolu techto oblasti, tiskovy prehled, export/import zalohy, okamzitou automatickou zalohu, slozku automatickych zaloh, export kalendare, znovunacteni dat, nastaveni, O programu, kontrolu aktualizaci nebo ukoncit aplikaci bez nativniho focus problemu tray menu; tlacitka bez aktualniho cile nebo blokovana rozpracovanou editaci jsou v dialogu neaktivni
- v background snapshotu pro tray tooltip a oznameni uprednostnit akutni terminy pred obecnym auditem dat, aby propadle nebo blizici se TK/ZK/pripominky/servis/doklady nezapadly za mene nalehavym datovym nedostatkem
- testovat desktopova oznameni bez vazby na OS runneru: Windows vetev pouziva balonkove oznameni a ne-Windows vetev pristupne inline okno
- pamatovat posledni desktopove oznameni v nastaveni datove sady po dnech, aby se stejny akutni termin neoznamoval opakovane pri kazde background kontrole; zmena reminder nastaveni a obnoveni zalohy historii resetuji
- po probuzeni Windows zachytit systemovy resume signal a po stejne kratke prodleve jako AHK znovu spustit background kontrolu terminu, tooltipu a automatickych zaloh; na ostatnich platformach zatim zustava bezpecny no-op fallback
- hlidat desktop UI zdroje proti typickym mojibake znakum, aby se poskozena UTF-8 diakritika nevratila do pristupnych nazvu ani textu pro ctecky obrazovky
- vest `dotnet/docs/ACCESSIBILITY.md` jako Avalonia accessibility checklist s jasnym stavem `accessibility-oriented / pre-conformance`, dokumentovanymi keyboard/focus vyjimkami a odkazy na oficialni Avalonia pravidla
- ukladat rucni screen-reader dukazy do `dotnet/docs/accessibility-evidence/`, aby pozdejsi ACR/VPAT audit nebyl rekonstruovan az z pameti
- staticky hlidat, ze interaktivni Avalonia prvky maji stabilni `AutomationId` a lidske pristupne jmeno a ze nove rucni `KeyDown` handlery nevzniknou bez vedome dokumentovane vyjimky
- staticky hlidat live regiony u stavovych, chybovych a prubehovych textu, jeden hlavni nadpis kazdeho samostatneho okna/dialogu a `AccessibilityView=Control` u landmarku podle oficialniho Avalonia accessibility modelu
- staticky hlidat, ze menu zkratky viditelne pres `InputGesture` jsou vystavene i jako `AutomationProperties.AcceleratorKey` a ze progress bary maji citelny nazev, ID i napovedu
- staticky hlidat, ze seznamove polozky s `AccessibleLabel` maji `AutomationProperties.ItemType` a ze `AutomationProperties.ItemStatus` pouzivaji jen skutecne stavove, prioritni nebo dostupnostni vlastnosti
- staticky hlidat, ze runtime povinna editorova pole vystavuji `AutomationProperties.IsRequiredForForm` a volitelna pole se za povinna nevydavaji
- vystavovat souhrnne, stavove a detailni texty v hlavnim shellu i workspacech pres explicitni pristupne nazvy a stabilni `AutomationId`, aby je mohly cist screen readery a kontrolovat UI testy
- cist a zapisovat podporovane reminder volby do nastaveni datove sady a respektovat `show_dashboard_on_launch`
- reportovat stejnou verzi jako root `src/VERSION`, vcetne file version pro desktop buildy
- kontrolovat `update/latest.ini` kompatibilne s AHK vetvi a na Windows pripravit automatickou instalaci pres Inno Setup installer; `Vehimap.Updater` zustava fallback pro starsi archivni/portable balicky
- v dialogu kontroly aktualizaci ukazat duvod, proc automaticka instalace neni dostupna, napriklad chybejici updater, nepublikovany build, nepodporovanou platformu nebo nekompletni metadata manifestu
- v dialogu kontroly aktualizaci zobrazit asset URL a SHA-256 hash, aby rucni instalace sla overit bez dohledavani v manifestu
- v dialogu kontroly aktualizaci zkopirovat cely vysledek vcetne verzi, asset URL a SHA-256 do schranky tlacitkem nebo `Ctrl+Shift+C`; vysledek kopirovani se oznami pristupnym stavovym textem
- pri automaticke instalaci ukazat samostatny dialog `Stahovani aktualizace` s progressbarem, stavem overovani a tlacitkem `Zrusit`; po spusteni Inno Setup instalatoru shell pozada runtime o skutecne ukonceni aplikace, aby Vehimap nezustal schovany v tray a neblokoval update
- pri poskozenem lokalnim desktop manifestu preskocit lokalni override a zkusit bezny vzdaleny manifest, aby testovaci soubor v `update/` nezablokoval kontrolu aktualizaci
- pri selhani kontroly aktualizaci, otevreni release poznamek, otevreni assetu nebo spusteni updater helperu udrzet shell bezici a ukazat chybu ve stavovem textu nebo aktualizacnim dialogu
- otevrit modalni export a obnovu dat a pracovat s novym SQLite `.vehimapbak` formatem, zatimco starsi AHK/C# 1.x zalohy zustavaji importovatelne pres legacy parser
- pri obnoveni zalohy pred prepsanim aktualnich dat vytvorit ochrannou kopii SQLite databaze, pripadnych puvodnich TSV/INI souboru i spravovanych priloh v `data/import-backups/<cas>` a po importu ukazat cestu k teto kopii i pocet obnovenych priloh
- pri selhani obnovy `.vehimapbak` zobrazit cestu k zaloze a konkretni parser detail vcetne chybne radky priloh, pokud je problem v attachment sekci
- generovat verzovane release balicky pro `win-x64`, `linux-x64`, `osx-x64` a `osx-arm64`; Windows verejny artefakt je Inno Setup instalator, macOS/Linux zatim zustavaji archivni balicky
- rozlisovat desktop kanaly `stable`, `beta` a `nightly`; na Windows maji vlastni nazev aplikace, update manifest i systemovou datovou slozku
- pripravit publikovany release pro `.NET` desktop tag `dotnet-v<verze>` pres GitHub Actions
- publikovat runtime-specific desktop manifesty `update/latest-dotnet-<rid>.ini`
- udrzet prechodovy alias `update/latest-dotnet-preview-<rid>.ini`, aby se uz vydane preview buildy dokazaly aktualizovat na prvni stabilni desktop release
- spoustet Windows Appium smoke i v CI nad publish buildem desktop release; CI smoke overuje prvni fokus po startu na seznamu vozidel, dostupnost seznamu pres `Tab` z filtru, navrat `Shift+Tab` z vybrane pracovni karty zpet na seznam, app-level menu `Soubor`, menu `Vozidlo`, menu `Rychle akce`, jejich aktualni action-state, vyvolani hlavniho menu pres `F10` bez automatickeho rozbaleni, zavreni menu druhym `F10` zpet na puvodni fokus, rozbaleni nabidky az sipkou dolu, vynechani menu pri beznem `Tab` / `Shift+Tab`, otevreni aktualniho upozorneni z menu `Rychle akce`, otevreni a zavreni app-level dialogu `Nastaveni`, `O programu` vcetne tlacitka `Podekovat autorovi` a `Zkontrolovat aktualizace`, otevreni a zavreni pristupnych tray akci z menu `Aplikace`, otevreni vsech samostatnych pracovnich oken, zavreni workspace pres `Escape`, navigaci z globalniho hledani, casove osy, terminovych prehledu a nakladu, zakladni ulozeni editoru pripominek, dokladu, historie, tankovani a udrzby v samostatnych oknech, navrat `Shift+Tab` z nazvu vozidla na `Zrusit` a potvrzeni pri zavirani rozpracovane editace, ulozeni voleb automatickych zaloh z dialogu `Nastaveni` vcetne okamziteho vytvoreni `.vehimapbak`, ulozeni volby Dashboardu pri startu vcetne validacni chyby, post-create i rucni `Balicek pro vozidlo`, doporucene servisni sablony a potvrzeni `Splneno` z `Planu udrzby` i Dashboardu, zatimco rozsirena sada nad izolovanou portable kopii testuje kopirovani diagnostiky z `O programu`, detailu kontroly aktualizaci i vyresene cesty spravovane prilohy dokladu do systemove schranky a dalsi realne vytvoreni okamzite automaticke zalohy
- kontrolovat historickou migracni paritu AHK modulu pres `dotnet\build\Get-DotnetMigrationParity.ps1`; Windows hardening i AHK retirement gate tim overuji, ze kazdy odstraneny legacy modul ma pojmenovanou .NET evidence vrstvu

## Lokalni build

```powershell
cd dotnet
dotnet restore
dotnet test
dotnet build
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetNightlyReadiness.ps1 -RuntimeIdentifier win-x64 -SkipTests
```

## Release balicky

Lokalni packaging publish vystupu. Windows RID vytvori Inno Setup instalator, ostatni platformy archiv:

```powershell
cd dotnet
dotnet publish .\src\Vehimap.Desktop\Vehimap.Desktop.csproj -c Release -r win-x64 --self-contained true -p:VehimapReleaseChannel=stable -o .\artifacts\stable\win-x64\app
.\build\Package-DesktopRelease.ps1 -PublishDirectory .\artifacts\stable\win-x64\app -RuntimeIdentifier win-x64 -Version (Get-Content ..\src\VERSION).Trim() -OutputDirectory .\artifacts\stable\win-x64\release -Channel stable
```

Balickovaci skript vynechava `.pdb`, pro Windows vytvori per-user Inno Setup instalator a pro ostatni RID archiv, prida `.sha256` a JSON metadata s `runtimeIdentifier`, `channel`, `assetKind`, nazvem balicku, hashem a velikosti. Korenny `favicon.ico` je zdroj ikon pro desktopove EXE, instalator, odinstalaci i zastupce v nabidce Start a na plose. Windows instalator pri update zustava interaktivni a na konci nabidne checkbox `Spustit Vehimap` / `Spustit Vehimap Beta` / `Spustit Vehimap Nightly`; aplikace pred nim stahuje a overuje setup EXE v samostatnem progress dialogu se zrusenim a po spusteni installeru pozada runtime o skutecne ukonceni aplikace misto schovani do tray. Generator desktop update manifestu z techto metadat znovu overuje fyzicky hash i velikost balicku, aby manifest nemohl ukazovat na poskozeny nebo zamereny artefakt. Kontrola aktualizaci umi lokalni manifest pouzit jako override, ale pokud je lokalni soubor poskozeny, pokracuje na vzdaleny manifest. Dialog kontroly aktualizaci navic ukazuje, proc je update jen rucni, pokud automaticka instalace neni dostupna, vypise asset URL i SHA-256 pro overeni stazeneho balicku a umi tyto detaily zkopirovat do schranky tlacitkem nebo `Ctrl+Shift+C`.

Lokalni testovaci vystupy jsou rozdelene podle kanalu, aby tester nemusel hledat mezi technickymi artefakty:

- aktualni nightly aplikace: `dotnet\artifacts\nightly\win-x64\app\Vehimap.Desktop.exe`
- aktualni nightly instalator: `dotnet\artifacts\nightly\win-x64\release\vehimap-desktop-nightly-<verze>-win-x64-setup.exe`
- budouci beta aplikace a instalator: `dotnet\artifacts\beta\win-x64\app` a `dotnet\artifacts\beta\win-x64\release`
- budouci stable aplikace a instalator: `dotnet\artifacts\stable\win-x64\app` a `dotnet\artifacts\stable\win-x64\release`

Stare lokalni slozky jako `desktop-preview`, `desktop-release`, `release-readiness`, `win-x64` nebo `nightly-version-check` jsou historicke/technicke vystupy ze starsich skriptu a pri beznem testovani je lze ignorovat.

CI workflow `.github/workflows/dotnet-desktop.yml` umi:

- otestovat `.NET` vetev na Windows
- publikovat self-contained desktop buildy pro Windows, Linux a macOS
- zabalit je do verzovanych artefaktu:
  - `vehimap-desktop-stable-<verze>-win-x64-setup.exe`
  - `vehimap-desktop-beta-<verze>-win-x64-setup.exe`
  - `vehimap-desktop-nightly-<verze>-win-x64-setup.exe`
  - `vehimap-desktop-<verze>-linux-x64.tar.gz`
  - `vehimap-desktop-<verze>-osx-x64.zip`
  - `vehimap-desktop-<verze>-osx-arm64.zip`
- pridat ke kazdemu balicku `.sha256` a `.json` metadata
- pri pushi tagu `dotnet-v<verze>` vytvorit publikovany GitHub release
- pri pushi tagu `dotnet-beta-v<verze>` vytvorit prerelease pro beta kanal
- pri nocnim schedule, rucnim workflow dispatchi s kanalem `nightly` nebo tagu `dotnet-nightly` vytvorit/nahradit rolling nightly prerelease
- pro nightly pouzit unikatni efektivni verzi `<verze-ze-src/VERSION>-nightly.<run>.<attempt>`, aby updater poznal kazdy novy nocni instalator jako skutecne novejsi build
- po release zapsat runtime-specific desktop manifesty do `update/`
- zapsat i prechodove `latest-dotnet-preview-<rid>.ini` aliasy pro uz vydane preview buildy
- pred commitem vygenerovanych manifestu overit Windows manifest pres `Test-DotnetPublishedRelease.ps1 -RuntimeIdentifier win-x64 -Channel <kanal> -SkipNetwork`; stable pri tom hlida i AHK retirement gate, nightly/beta overuji kanalovy manifest bez stable-only kroku
- pred uploadem Windows release artefaktu spustit `Test-DotnetInstallerSmoke.ps1`, aby se setup EXE, `.sha256` a JSON metadata overily uz v publish jobu
- na Windows runneru spustit Appium smoke nad publish buildem desktop release vcetne kontroly app-level menu a dostupnosti rychlych akci

Pred vytvorenim tagu lze lokalne spustit stejnou release readiness branu:

```powershell
cd dotnet
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetReleaseReadiness.ps1 -RuntimeIdentifier win-x64
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetWindowsHardening.ps1 -RuntimeIdentifier win-x64
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetNightlyReadiness.ps1 -RuntimeIdentifier win-x64
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetBetaReadiness.ps1 -RuntimeIdentifier win-x64
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetStableReadiness.ps1 -RuntimeIdentifier win-x64
```

Skript release readiness postavi solution, spusti unit/compat/UI kontrakty, publikuje self-contained desktop build, vytvori release balicek, `.sha256`, JSON metadata a overi odpovidajici update manifest. Vystup uklada do `dotnet\artifacts\<kanal>\<rid>\app` pro spustitelnou aplikaci a do `dotnet\artifacts\<kanal>\<rid>\release` pro instalator, metadata a checksum. Wrappery `Test-DotnetNightlyReadiness.ps1`, `Test-DotnetBetaReadiness.ps1` a `Test-DotnetStableReadiness.ps1` nastavují kanál bez ručního předávání `-Channel`. Pro `stable` se kontroluje `latest-dotnet-win-x64.ini` bez preview odkazu, pro `nightly` se vytvori lokalni prerelease verzi `src/VERSION-nightly.local.<utc>` a kontroluje `latest-dotnet-nightly-win-x64.ini` proti rolling tagu `dotnet-nightly`.

`Test-DotnetWindowsHardening.ps1` je pred-beta wrapper pro Windows stabilizaci: nejdrive vypise release train status, potom spusti cele `dotnet test`, nasledne nightly readiness a nakonec overi AHK retirement status. Plny instalacni smoke realneho Inno setupu se spousti automaticky jen v GitHub Actions; lokalne se kvuli ochrane existujici instalace stejneho kanalu drzi bezpecne overeni metadat/checksumu, pokud vedome nepridate `-AllowLocalInstallSmoke`. Po dobehu GitHub Actions lze pridat `-VerifyPublishedNightly`, aby stejny skript overil i publikovany nightly manifest a asset.

Pred samotnym hardeningem lze samostatne spustit i migracni parity kontrolu:

```powershell
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Get-DotnetMigrationParity.ps1 -FailOnBlockers
```

Kontrola nic nemeni. Udrzuje historickou mapu odstranene AHK vetve na konkretni .NET evidence soubory, aby bylo i po retirement commitu jasne, ktery legacy modul nahradila ktera .NET vrstva.

Samotne vytvoreni release tagu je oddelene do bezpecneho skriptu. Ve vychozim rezimu zkontroluje cisty `main`, shodu s `origin/main`, neexistujici tag a spusti release readiness branu; tag na GitHub odesle jen s explicitnim `-Push`.

```powershell
cd dotnet
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\New-DotnetDesktopReleaseTag.ps1 -RuntimeIdentifier win-x64 -DryRun -SkipReadiness
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\New-DotnetDesktopReleaseTag.ps1 -RuntimeIdentifier win-x64 -Channel stable
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\New-DotnetDesktopReleaseTag.ps1 -RuntimeIdentifier win-x64 -Channel stable -Push
```

Prvni prikaz je rychla sucha kontrola bez tagu a bez dlouhe readiness brany. Potom zvolte bud druhy prikaz pro vytvoreni lokalniho anotovaneho tagu `dotnet-v<verze>`, nebo treti prikaz pro vytvoreni tagu a jeho okamzite odeslani na GitHub po uspesne readiness brane.

Pro rychlou lokalni kontrolu nightly pred rucnim workflow dispatch staci:

```powershell
cd dotnet
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetNightlyReadiness.ps1 -RuntimeIdentifier win-x64
```

Wrapper vola stejnou release readiness branu s kanalem `nightly`, takze se overi skutecny nightly nazev instalatoru, metadata, hash, velikost, `asset_kind=installer`, `channel=nightly`, manifest pro rolling release `dotnet-nightly` a u Windows balicku i setup EXE pres installer smoke bez tiche instalace.

Windows instalator lze po vytvoreni balicku overit samostatnym smoke skriptem. Bez `-Install` se kontroluji jen soubory, metadata a SHA-256; s `-Install` skript provede tichou instalaci do izolovane slozky, vytvori vedle EXE portable `data`, kratce spusti aplikaci a zase ji odinstaluje.

```powershell
cd dotnet
$release = Get-ChildItem .\artifacts\nightly\win-x64\release\*.json | Select-Object -First 1
$installer = Join-Path $release.DirectoryName ((Get-Content -Raw $release.FullName | ConvertFrom-Json).packageFile)
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetInstallerSmoke.ps1 -InstallerPath $installer -PackageMetadataPath $release.FullName
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetInstallerSmoke.ps1 -InstallerPath $installer -PackageMetadataPath $release.FullName -Install
```

Stejny plny instalacni smoke lze spustit primo v release readiness brane. Pozor: na lokalnim PC muze realna Inno instalace se stejnym `AppId` prepsat uninstall registr a zastupce existujici nightly/beta/stable instalace. Pouzivejte ho bez dalsiho potvrzeni jen v CI nebo ve VM; lokalne pridejte `-AllowLocalInstallSmoke`, pokud riziko vedome prijimate.

```powershell
cd dotnet
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetNightlyReadiness.ps1 -RuntimeIdentifier win-x64 -InstallSmoke -AllowLocalInstallSmoke
```

Podepisovani Windows instalatoru se nastavuje bez Inno GUI pres promennou prostredi `VEHIMAP_INNO_SIGNTOOL_COMMAND`. Hodnota musi byt Inno sign tool command s placeholderem `$f`, napr. volani `signtool.exe sign ... $f`; pokud neni nastavena, build zustane nepodepsany a dal projde bez certifikatu.

## Promotion tok nightly -> beta -> stable

Bezny vyvoj jde pres tri oddelene kanaly:

- `nightly`: rolling prerelease `dotnet-nightly`, vhodny pro prubezne testovani odvaznych zmen.
- `beta`: verzovany prerelease `dotnet-beta-v<verze>`, povyseny z nightly po zakladnim overeni.
- `stable`: publikovany release `dotnet-v<verze>`, povyseny z beta po realnem testovani.

Branch model je zamerne jednoduchy: dlouhodobe existuje jen `main`. Nightly, beta a stable nejsou oddelene vetve, ale release kanaly nad konkretnimi commity z `main`, oddelene tagy, manifesty, instalatory, AppId a systemovou datovou slozkou. Trvale vetve `beta` a `stable` zatim nezavadet; kratkodobou vetev `release/<verze>` zalozit jen vyjimecne, pokud by bylo potreba udrzovat starsi beta/stable kandidat a soucasne uz rozjet dalsi velke nightly zmeny.

Po vytvoreni beta tagu plati kratky release freeze pro `main`: do stable smi jit jen P0/P1 opravy, dokumentace, release tooling nebo bezpecne overovaci opravy. Nove funkce pockaji az po stable. Pokud beta najde zasadni blocker, oprava jde nejdrive do `main`, potom se zvysi `src/VERSION` a vyda se nova beta s novym tagem; existujici `dotnet-beta-v<verze>` tag se neprepisuje. P2 a drobnosti se odlozi po stable, pokud nejsou vyslovene nutne pro release.

Pred povysenim spustte promotion gate. Nic netaguje ani nemaze, jen zkontroluje stav repozitare a rekne presny dalsi prikaz:

```powershell
cd dotnet
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetReleasePromotion.ps1 -TargetChannel beta -RuntimeIdentifier win-x64 -FailOnBlockers
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetBetaReadiness.ps1 -RuntimeIdentifier win-x64
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\New-DotnetDesktopReleaseTag.ps1 -RuntimeIdentifier win-x64 -Channel beta -Push

powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetReleasePromotion.ps1 -TargetChannel stable -RuntimeIdentifier win-x64 -FailOnBlockers
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetStableReadiness.ps1 -RuntimeIdentifier win-x64
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\New-DotnetDesktopReleaseTag.ps1 -RuntimeIdentifier win-x64 -Channel stable -Push
```

Beta promotion vyzaduje existujici rolling tag `dotnet-nightly` i publikovany `update/latest-dotnet-nightly-win-x64.ini` manifest s ocekavanou nightly prerelease verzi, kanalem, hashem, velikosti a odkazem na release asset. Stable promotion stejne vyzaduje existujici beta tag `dotnet-beta-v<verze>` i publikovany beta manifest a predpoklada testerskou vlnu bez P0/P1 blockeru. Tagovaci skript pro `beta` a `stable` vola promotion gate automaticky pred release readiness branou, takze nejde omylem preskocit proces `nightly -> beta -> stable`. Promotion gate po uspesne kontrole vypise i doporuceny wrapper pro lokalni readiness daneho kanalu, aby se beta/stable pred tagem overovaly stejnou cestou jako nightly.

Po dobehnuti GitHub Actions release workflow lze stabilni i nightly kanal overit jednim post-release skriptem:

```powershell
cd dotnet
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Get-DotnetReleaseTrainStatus.ps1 -RuntimeIdentifier win-x64
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetPublishedNightly.ps1 -RuntimeIdentifier win-x64
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetPublishedBeta.ps1 -RuntimeIdentifier win-x64
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetPublishedStable.ps1 -RuntimeIdentifier win-x64
```

Release train status nic netaguje ani nepublikuje. Jen shrne lokalni artefakty `nightly`, `beta` a `stable`, existenci manifestu v `update/`, dostupnost release skriptu, remote tagy a na konci vypise dalsi doporuceny krok. Kanálové wrappery `Test-DotnetPublishedNightly.ps1`, `Test-DotnetPublishedBeta.ps1` a `Test-DotnetPublishedStable.ps1` jen volaji spolecny `Test-DotnetPublishedRelease.ps1` se spravnym kanalem, aby pri rucni kontrole neslo omylem overovat spatny manifest. Nightly wrapper kontroluje `latest-dotnet-nightly-win-x64.ini`, rolling tag `dotnet-nightly`, prerelease verzi, release notes URL, dostupnost assetu, SHA-256, velikost assetu a `channel=nightly`; AHK retirement gate se u nightly nespousti. Stable wrapper zkontroluje `latest-dotnet-win-x64.ini`, prechodovy `latest-dotnet-preview-win-x64.ini`, release notes URL, dostupnost assetu, SHA-256, velikost assetu a nakonec spusti AHK retirement gate. Pokud je potreba jen offline kontrola po commitu manifestu, pridejte `-SkipNetwork`.

Po finalnim odstraneni AHK vetve zustava retirement report jako ochranna kontrola:

```powershell
cd dotnet
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Get-AhkRetirementReadiness.ps1 -RuntimeIdentifier win-x64 -FailOnBlockers
```

Report nic nemaze. Jen zkontroluje, ze stabilni desktop manifest existuje, ukazuje na `dotnet-v<verze>` release asset, preview alias pro starsi preview buildy miri na stejny obsah, lokalni stable build lezi v `dotnet/artifacts/stable/win-x64/app` a AHK-only artefakty (`src/Vehimap.ahk`, `src/lib`, `src/tests` a navazujici generovane vystupy) zustavaji odstranene. Pred prvnim stable releasem byl ocekavany blocker chybejici `update/latest-dotnet-win-x64.ini`; po stable release je blockerem naopak navrat AHK-only souboru.

Stejna kontrola bezi i v GitHub Actions po vygenerovani desktop manifestu pro prvni stabilni release. Pokud by `latest-dotnet-win-x64.ini` neukazoval na spravny `dotnet-v<verze>` asset nebo by legacy preview alias nemiril na stejny obsah, release workflow skonci chybou jeste pred commitem manifestu.
