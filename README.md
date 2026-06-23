# Vehimap

Komplexní řešení pro evidenci vašich vozidel.

## Co umí

- rozdělení vozidel do kategorií `Osobní vozidla`, `Motocykly`, `Nákladní vozidla`, `Autobusy`, `Ostatní`
- přidání, úpravu a odstranění vozidla
- vyhledávání v hlavním seznamu podle názvu, poznámky, značky nebo SPZ
- globální hledání napříč vozidly, historií událostí, kilometry a tankováním, pojištěním a doklady, vlastními připomínkami i plány údržby
- filtr hlavního seznamu na všechna vozidla, vozidla s blížícím se termínem, vozidla po termínu a vozidla bez vyplněné zelené karty
- volitelné skrytí archivovaných a odstavených vozidel v hlavním seznamu, aniž by zmizela z dat a přehledů
- v C# Avalonia větvi se poslední zvolená kategorie a stavový filtr hlavního seznamu ukládají do `settings.ini`; textové hledání zůstává jen dočasné, aby po startu neschovalo očekávaná vozidla
- detail vozidla se souhrnem údajů, stavem platností, posledními událostmi z historie a souhrnem tankování i dokladů
- v C# Avalonia větvi detail vozidla ukazuje i poslední historické záznamy, poslední známý tachometr a samostatné stavové souhrny historie, tankování, připomínek, dokladů a údržby, takže odpovídá rychlé kontrolní ploše z AHK verze
- v C# Avalonia větvi má detail vozidla přístupný blok `Související evidence`, ze kterého lze rovnou přejít do historie, tankování, připomínek, údržby, dokladů, časové osy nebo nákladů vybraného vozidla
- historii událostí pro každé vozidlo, včetně přidání, úpravy a odstranění servisních nebo jiných záznamů
- samostatnou evidenci `Kilometry a tankování` pro každé vozidlo, včetně přidání, úpravy a odstranění záznamů
- samostatný `Plán údržby` pro každé vozidlo, včetně šablon běžných servisních úkonů, doporučených balíků podle kategorie a explicitního servisního profilu vozidla, intervalu podle kilometrů nebo měsíců, evidence posledního servisu a rychlého označení splnění s volitelným zapsáním do historie
- samostatný `Balíček pro vozidlo`, který umí v jednom kroku nabídnout doporučené servisní plány, placeholdery dokladů i obecné připomínky podle kategorie a servisního profilu
- rychlé hledání a přístupné řazení v historii událostí, kilometrech a tankování, pojištění a dokladech, vlastních připomínkách i plánu údržby, včetně zapamatování posledního řazení
- samostatnou evidenci `Pojištění a doklady` pro každé vozidlo, včetně přidání, úpravy a odstranění záznamů, volby mezi `Externí cestou` a `Spravovanou kopií`, přesunu přílohy do interní složky aplikace, znovu propojení chybějícího souboru, otevření navázaného souboru i jeho složky, kopírování cesty a zobrazení stavu dostupnosti přílohy; v C# Avalonia větvi se úspěch i selhání těchto akcí hlásí ve stavovém textu dokladů i hlavního shellu
- `Náklady a souhrny` pro každé vozidlo s přehledem podle roku i s výběrem vlastního období po měsících v rámci zvoleného roku, včetně `Ujeto km`, `Ceny / km` a srovnání s předchozím stejně dlouhým obdobím
- samostatný přehled `Náklady napříč vozidly` pro vybrané období s porovnáním vozidel, rozpadnutím částek podle skupin, `Ujeto km`, `Cenou / km`, upozorněním na aktivní vozidla bez číselného nákladu a se stavy k řešení přímo u jednotlivých vozidel
- v C# Avalonia větvi lze v přehledu `Náklady napříč vozidly` zvolit období přes předvolby nebo vlastní datumový rozsah; poslední volba se ukládá do `settings.ini` a používá se i v dashboardu a exportech
- export nákladových přehledů do `TSV souhrnu`, `TSV detailu` a `HTML sestavy`; v C# Avalonia větvi se úspěch, zrušení i chyba exportu hlásí ve stavovém textu nákladů i hlavního shellu a uložená HTML sestava jasně oznámí i případné selhání otevření v prohlížeči
- vlastní připomínky i s volbou opakování `Neopakovat`, `Každý rok`, `Každé 2 roky` a `Každých 5 let`
- upozornění na blížící se nebo propadlou technickou kontrolu
- upozornění na blížící se nebo propadlou zelenou kartu
- samostatný `Dashboard` s rychlým souhrnem vozidel, termínů, nákladů v aktuálním roce, pořadím nejdražších vozidel, upozorněním na aktivní vozidla bez číselného nákladu, rychlými highlighty nejpalčivějších problémových stavů a akčním seznamem nejbližších termínů, blížících se servisních úkonů, chybějících zelených karet, vozidel bez SPZ nebo příští TK i dokladů s nedostupnou nebo prázdnou cestou
- samostatný dialog s přehledem všech blížících se a propadlých termínů, servisních úkonů a připomínek, filtrem, rychlým hledáním podle názvu, SPZ nebo typu položky, ruční obnovou, řazením podle sloupců, volitelným zobrazením datových nedostatků, přímým otevřením řešené položky, otevřením vozidla i editací vozidla a zapamatováním posledního nastavení přehledu
- samostatný dialog `Propadlé termíny` se seznamem všech už propadlých `TK`, `ZK` a servisních úkonů, rychlým hledáním a přímým otevřením řešené položky, otevřením vozidla nebo editací vozidla
- `Tiskový přehled` všech vozidel ve formátu HTML; v C# Avalonia větvi se nejprve uloží jako běžný soubor a po uložení se otevře v prohlížeči pro tisk přes `Ctrl+P`
- `Export dat` do jednoho záložního souboru `.vehimapbak` včetně plánů údržby a spravovaných příloh dokladů; v C# Avalonia větvi výsledná hláška uvádí i počet zahrnutých a přeskočených chybějících spravovaných příloh
- `Import dat` z dříve vytvořené zálohy včetně automatické zálohy původních souborů před přepsáním, obnovení plánů údržby i návratu spravovaných příloh dokladů
- v C# Avalonia větvi se po úspěšné obnově zálohy okamžitě synchronizuje i běžící session stav aplikace, tedy data, audit, meta údaje a podporovaná nastavení; pokud samotné obnovení selže, původní běžící data zůstanou zachovaná a chyba se oznámí stavovou hláškou
- Avalonia shell po obnovení zálohy promítá do seznamů a přehledů přímo obnovený session stav, takže výběr vozidla, audit, dashboard, náklady i rychlé akce odpovídají nové záloze bez dalšího ručního reloadu
- v C# Avalonia větvi používá start aplikace, ruční znovunačtení dat i obnova zálohy stejnou refresh cestu shellu, takže seznam vozidel, dashboard, audit, přehledy, náklady a rychlé akce zůstávají konzistentní napříč těmito vstupy
- v C# Avalonia větvi se při poškozeném legacy `TSV`, `INI` nebo `.vehimapbak` souboru zobrazí konkrétní cesta a detail parseru, aby šlo chybu opravit bez hádání, která evidence nebo záloha selhala
- v C# Avalonia větvi app-level akce z menu a tray okna jako nastavení, export/import zálohy, `O programu` a kontrola aktualizací hlásí zrušení i chyby ve stavovém textu shellu nebo v aktualizačním dialogu místo pádu aplikace
- hlavní pracovní okna jako `Dashboard`, `Přehled termínů`, `Propadlé termíny`, `Audit dat`, `Časová osa vozidla`, `Pojištění a doklady`, `Plán údržby`, detail vozidla i hlavní seznam lze zvětšit, takže se seznamy natáhnou do šířky i výšky
- v C# desktopové větvi lze přehledová workflow `Časová osa`, `Globální hledání`, `Audit dat`, `Náklady`, `Dashboard`, `Blížící se termíny` a `Propadlé termíny` otevřít jako kartu v hlavním shellu i jako samostatné okno se stejnou přístupnou pracovní plochou
- v C# desktopové větvi mají samostatná workspace okna sjednocené otevření prvního logického prvku a stejnou ochranu před zavřením rozpracovaného editoru jako hlavní shell
- v C# desktopové větvi lze samostatná pracovní okna zavřít klávesou `Escape`; při rozpracované úpravě se použije stejné potvrzení zahození změn jako při zavření tlačítkem
- v C# desktopové větvi patří názvy samostatných workspace oken jejich workspace viewmodelům; hlavní shell je už nevystavuje jako veřejné root aliasy
- v C# desktopové větvi zůstávají action-state podmínky evidenčních editorů, dokladových příloh a exportů nákladů interní součástí příkazů a workspace vrstvy, ne veřejným root API shellu
- v C# desktopové větvi jde otevření samostatných workspace oken přes jeden sdílený tok, takže potvrzení rozpracované editace, přepnutí karty a návrat fokusu zůstávají stejné napříč evidencemi i přehledy
- v C# desktopové větvi už `Časová osa` a `Globální hledání` drží svůj vyhledávací text, výběr a stavové texty přímo ve vlastních workspace viewmodelech, ne přes duplicitní root aliasy
- v C# desktopové větvi vlastní `Časová osa` a `Globální hledání` také své načtené kolekce položek a výsledků, takže hlavní viewmodel je pouze obnovuje při změně dat nebo hledání
- v C# desktopové větvi stejný princip platí i pro termínové přehledy `Blížící se termíny` a `Propadlé termíny`, takže jejich filtr, hledání, volby a výběr patří child workspace viewmodelům
- v C# desktopové větvi vlastní termínové přehledy `Blížící se termíny` a `Propadlé termíny` také načtené kolekce položek; hlavní viewmodel je pouze obnovuje při změně dat, filtrů nebo hledání
- v C# desktopové větvi už ani dashboardové souhrny auditu, nákladů a nejbližších termínů nejsou znovu vystavené přes root aliasy; čtou se ze sdílených workspace stavů auditu, nákladů a dashboardu
- v C# desktopové větvi vlastní `AuditWorkspace` plný seznam auditních položek, `DashboardWorkspace` vlastní dashboardový auditní výřez a nejbližší termíny a `CostWorkspace` vlastní flotilový seznam nákladů
- v C# desktopové větvi vlastní příslušné workspace viewmodely také své volby filtrů a předvoleb pro náklady, termínové přehledy, údržbu a režimy dokladových příloh
- v C# desktopové větvi patří i stav exportů nákladového přehledu přímo `CostWorkspace`, takže hlavní viewmodel jen spouští exportní příkazy
- v C# desktopové větvi vlastní souhrnné texty evidencí `Historie`, `Tankování`, `Připomínky`, `Údržba` a `Doklady` přímo jejich workspace viewmodely místo root aliasů
- v C# desktopové větvi vlastní evidence workspaces `Historie`, `Tankování`, `Připomínky`, `Údržba` a `Doklady` také načtené kolekce záznamů; hlavní viewmodel je pouze plní při změně vozidla nebo dat
- v C# desktopové větvi vlastní `HistoryWorkspace`, `FuelWorkspace`, `ReminderWorkspace`, `MaintenanceWorkspace` a `RecordWorkspace` i výběr položky, detailní texty a stav svých editorů; root už je nevystavuje jako veřejné aliasy
- v C# desktopové větvi vlastní souhrnné texty detailu vozidla přímo `VehicleDetailWorkspace`, zatímco hlavní viewmodel je jen přepočítává při změně výběru vozidla
- v C# desktopové větvi vlastní `VehicleDetailWorkspace` i formulářové hodnoty editoru vozidla, takže hlavní viewmodel dál řídí workflow, ale nedrží lokální stav jednotlivých polí
- v C# desktopové větvi vlastní `VehicleDetailWorkspace` také režim editace a nadpis detailního panelu, takže viditelný stav editoru patří stejné pracovní ploše jako jeho pole
- C# desktopová větev má preview release balíčky pro Windows, Linux a macOS s ověřovanými `.sha256` soubory, metadata JSON a runtime-specific update manifesty; pokud je lokální preview manifest poškozený nebo kontrola aktualizací narazí na chybu sítě, manifestu, otevření odkazu či spuštění updateru, shell zůstane běžet a v dialogu nebo stavovém textu vysvětlí další bezpečný krok
- pravidelné automatické zálohy do `data/auto-backups` se samostatným intervalem ve dnech a omezením počtu ponechaných souborů
- samostatné nastavení počtu dnů pro upozornění na `TK`, `ZK` i servisní plány a kilometrového limitu pro blížící se údržbu
- volby `Spustit po startu počítače`, `Automaticky skrýt na lištu` a `Zobrazovat dashboard při startu`
- v C# Avalonia větvi jsou generátory autostartu připravené i pro budoucí macOS/Linux stabilizaci; Linux `.desktop` a macOS LaunchAgent výstupy mají regresní testy na escapování cest a argumentů
- horní menu; v AHK aplikaci `Soubor`, `Vozidlo`, `Přehled`, `Nástroje` a `Nápověda`, v C# Avalonia větvi `Soubor`, `Vozidlo`, `Přehledy`, `Rychlé akce` a `Aplikace`
- v C# Avalonia větvi lze z hlavního menu i přístupného tray okna otevřít aktuální datovou složku i složku automatických záloh; složky se otevírají oddělenou multiplatformní cestou (`explorer.exe` na Windows, `open` na macOS, `xdg-open` na Linuxu), což pomáhá při kontrole portable dat, záloh a spravovaných příloh
- přístupné tray okno pro rychlé zobrazení hlavního okna, otevření aktuálního upozornění z pozadí, dashboardu, blížících se termínů, propadlých termínů, nejbližší TK/ZK/připomínky/servisu/dokladu, filtrovaných kontrol, tiskového přehledu, ručního exportu/importu zálohy, okamžité automatické zálohy, exportu kalendáře, znovunačtení dat, otevření datové složky nebo složky automatických záloh, nastavení, dialogu `O programu`, kontroly aktualizací a ukončení aplikace; rychlé akce bez aktuálního cíle nebo akce blokované rozpracovanou editací jsou vypnuté v tray okně i v hlavním menu `Rychlé akce`
- v C# Avalonia větvi mají stavové, souhrnné a detailní texty hlavního shellu i workspace obrazovek vlastní přístupný název a stabilní `AutomationId`, aby je šlo spolehlivě číst čtečkou obrazovky i ověřovat UI testy
- v C# Avalonia větvi selhání zápisu při uložení vozidla, historie, tankování, připomínky, dokladu, údržby nebo balíčku nezavře rozpracovaný editor; chyba se zapíše do stavového textu editoru i hlavního shellu a fokus se vrátí na smysluplný prvek
- v C# Avalonia větvi se při selhání zápisu vrací i interní pracovní data před neúspěšnou změnu; mazání spravovaných příloh a složek vozidla proběhne až po úspěšném uložení dat, aby chybový stav nerozbil existující doklady
- v C# Avalonia větvi má stejnou ochranu i průběžné ukládání filtrů, řazení, časové osy, přehledů termínů a období nákladů; pokud `settings.ini` nejde zapsat, aplikace chybu oznámí a neuložená preference se nepřimíchá do pozdějšího uložení jiné evidence
- v C# Avalonia větvi se při chybě zápisu `settings.ini` vrací i session-level hodnoty z dialogu `Nastavení`, historie denních oznámení a metadata poslední automatické zálohy; aplikace tak neukazuje nepravdivý stav jen proto, že export zálohy nebo změna nastavení proběhly napůl
- automatickou kontrolu termínů, připomínek a servisních plánů každých 15 minut a znovu po probuzení počítače ze spánku; v C# Avalonia větvi se probuzení zatím napojuje přes Windows systémovou událost a akutní termíny v oznámeních a tooltipu lišty mají přednost před obecnými auditními nedostatky
- v C# Avalonia větvi jsou desktopová oznámení připravená na více platforem: Windows používá systémové balónkové oznámení a ostatní platformy přístupné inline okno; obě větve jsou testované nezávisle na aktuálním OS testovacího runneru
- v C# Avalonia větvi se poslední denní desktopové oznámení ukládá do `settings.ini`, takže stejný akutní termín neotravuje opakovaně během jednoho dne; změna nastavení upozornění nebo obnova zálohy tuto historii bezpečně vynuluje
- v C# Avalonia větvi se po úspěšném uložení evidence, obnovení zálohy, změně nastavení nebo okamžité automatické záloze hned přepočítá tray tooltip a stav oznámení, takže pozadí nečeká až na další patnáctiminutový interval

## Jak se používá

Ve většině případů stačí spustit `Vehimap.exe`.

V hlavním okně:

- záložky nahoře dál rozdělují vozidla podle kategorií
- pole `Hledat název, značku, SPZ, poznámku nebo štítek` filtruje jen aktuálně otevřenou kategorii
- pole `Filtr seznamu` rychle zobrazí jen vozidla, která právě vyžadují pozornost
- zaškrtávátko `Skrýt archivovaná a odstavená vozidla` schová neaktivní vozidla jen z hlavního seznamu a svou volbu si pamatuje i po dalším spuštění
- v C# Avalonia větvi se po dalším spuštění obnoví i poslední rozbalovací filtr kategorie a stavový filtr; rychlé textové hledání se záměrně neobnovuje
- v C# Avalonia větvi tlačítko `Vymazat filtry` obnoví celý seznam vozidel, ohlásí změnu ve stavovém textu a vrátí fokus do hledání
- v C# Avalonia větvi se během rozpracované editace zamkne seznam vozidel, jeho filtry, přepínání pracovních karet i otevírání jiných workspace oken; nejdřív je potřeba editor uložit nebo zrušit, aby nešlo omylem odejít z rozpracované práce
- tlačítka `Detail vozidla` a `Historie událostí` pracují s právě vybraným vozidlem, další evidence včetně `Plánu údržby` otevřete i z menu `Vozidlo`
- v C# Avalonia větvi lze stejné navazující evidence otevřít přímo z detailu vozidla v bloku `Související evidence`; pokud je detail otevřený jako samostatné okno, po přechodu do evidence se okno zavře
- položky `Dashboard` a `Globální hledání` v menu `Přehled` nebo `Přehledy` otevřou rychlý souhrn termínů, servisních úkonů, nákladů, problémových stavů a stavu evidencí nebo vyhledání napříč všemi evidencemi
- položka `Náklady napříč vozidly` v menu `Přehled` nebo `Přehledy` otevře porovnání nákladů za zvolené období mezi všemi vozidly a umožní z přehledu rovnou přejít na detail nákladů, detail vozidla nebo editaci
- v AHK menu `Nápověda` a v C# Avalonia menu `Aplikace` najdete `O programu` s přehledem verze, cesty k aplikaci a datové složky; v C# větvi lze tyto informace zkopírovat do schránky pro podporu a vedle toho spustit samostatnou ruční kontrolu aktualizací
- v dashboardu souhrn vozidel vypisuje i nejpalčivější problémové stavy podle priority a nákladový souhrn ukazuje nejdražší vozidla i aktivní vozidla bez číselného nákladu v aktuálním roce
- v dashboardu, `Přehledu termínů` i `Propadlých termínech` se vedle `TK`, `ZK` a vlastních připomínek zobrazují i blížící se nebo propadlé servisní úkony
- v dashboardu se v seznamu zobrazují nejen nejbližší termíny, ale i datové nedostatky jako chybějící SPZ, chybějící příští TK nebo problémové dokladové přílohy
- v dashboardu přibyly i akce `Souhrn nákladů` a `Náklady vozidla`, takže jde rovnou otevřít globální přehled nákladů nebo náklady vybraného vozidla z právě řešeného problému
- v dashboardu lze nově z vybraného servisního úkonu rovnou otevřít historii vozidla nebo servis přímo označit jako splněný bez ručního hledání správného plánu
- v `Plánu údržby` lze tlačítkem `Doporučené šablony` nebo zkratkou `Ctrl+Shift+N` otevřít výběr jen chybějících servisních plánů podle kategorie a servisního profilu vozidla a před přidáním je ještě upravit nebo odškrtnout
- po založení nového vozidla může Vehimap nabídnout rovnou celý `Balíček pro vozidlo`, tedy doporučené servisní plány, placeholdery dokladů i obecné připomínky
- v C# Avalonia větvi balíček používá pro typ dokladu a opakování připomínky stejné rozbalovací seznamy jako běžné editory, takže není potřeba ručně opisovat interní hodnoty
- `Časová osa vozidla` spojuje do jednoho seznamu historii, tankování, připomínky, expirace dokladů, technickou kontrolu, zelenou kartu i servisní úkoly s konkrétním datem
- v C# Avalonia větvi si `Časová osa vozidla` pamatuje poslední filtr `Vše`, `Budoucí` nebo `Minulé`; rychlé textové hledání v ose zůstává jen dočasné
- v `Přehledu` lze nově ručně exportovat budoucí termíny do kalendářového souboru `.ics`, včetně TK, ZK, připomínek, expirací dokladů a servisních úkolů s datem; C# Avalonia větev navíc skládá dlouhé iCalendar řádky podle pravidel `.ics`, aby se neztrácely delší české popisy, a výsledek nebo chybu exportu hlásí i v hlavním stavovém textu shellu
- v `Přehledu termínů` lze pod hledáním zapnout i datové nedostatky, takže se vedle termínů zobrazí i chybějící SPZ, příští TK nebo problémové dokladové přílohy
- v C# Avalonia větvi lze v `Blížících se termínech` zapnout i vozidla bez zelené karty a datové nedostatky z auditu; obě volby i rozbalovací filtr typu položky se ukládají do `settings.ini`, otevření datového nedostatku skočí rovnou na správnou evidenci a rychlá akce `Zkontrolovat ZK` umí přehled otevřít i tehdy, když je problémem jen chybějící zelená karta
- v C# Avalonia větvi si `Blížící se termíny` i `Propadlé termíny` pamatují poslední rozbalovací filtr, sloupec řazení a směr řazení, ale rychlé textové hledání zůstává jen dočasné
- `Audit dat` teď funguje stejně klávesnicově jako ostatní přehledy: jde z něj rovnou otevřít řešenou položku, detail vozidla nebo přejít do úpravy
- v dashboardu je i zaškrtávátko `Zobrazovat dashboard při startu`, které mění stejnou volbu jako dialog `Nastavení` a uloží ji ihned do `settings.ini`

## Klávesové zkratky v hlavním okně:

- `F10` nebo samostatná klávesa `Alt`: otevře horní menu bez zařazení menu do běžného pořadí `Tab` / `Shift+Tab`
- v C# Avalonia větvi horní menu zobrazuje hlavní dostupné klávesové zkratky přímo u položek, takže stejnou akci lze najít přes menu i použít zkratkou
- `Ctrl+N`: přidat vozidlo
- `Ctrl+U` nebo `F2`: upravit vybrané vozidlo
- `Ctrl+F`: přesunout fokus do hledání
- `Ctrl+Shift+F`: otevřít globální hledání
- `Ctrl+D`: otevřít dashboard
- `Ctrl+T`: otevřít přehled termínů
- `Ctrl+Shift+T`: otevřít propadlé termíny
- `Ctrl+O`: zobrazit detail vybraného vozidla
- `Ctrl+H`: otevřít historii vybraného vozidla
- `Ctrl+K`: otevřít kilometry a tankování vybraného vozidla
- `Ctrl+M`: otevřít plán údržby vybraného vozidla
- `Ctrl+P`: otevřít pojištění a doklady vybraného vozidla
- `Ctrl+R`: otevřít vlastní připomínky vybraného vozidla
- v hlavním seznamu klávesa `Enter` otevře detail právě vybraného vozidla
- v C# Avalonia větvi jsou `Ctrl+F`, `Ctrl+O` a `Ctrl+P` v přehledových kartách kontextové: nejdřív obslouží aktivní hledání, výsledek nebo termín a teprve mimo tyto pracovní plochy se použije globální akce hlavního okna
- v C# Avalonia větvi jsou v kartách evidencí kontextové i `Ctrl+N`, `Ctrl+U` / `F2` a `Ctrl+S`: `Ctrl+N` a `Ctrl+U` / `F2` z hlavní karty otevřou příslušné modální workspace okno s viditelným editorem, `Ctrl+S` ukládá aktivní editor a mimo evidenci zůstávají `Ctrl+N` a `Ctrl+U` / `F2` globálními akcemi vozidla
- v C# Avalonia větvi mají evidence `Historie`, `Kilometry a tankování`, `Pojištění a doklady`, `Vlastní připomínky` a `Plán údržby` vlastní rychlé hledání; `Ctrl+F` přesune fokus do filtru, tlačítko `Vymazat` filtr smaže a vrátí fokus do hledání, seznam zachová výběr podle položky a při prázdném výsledku vypne akce nad výběrem
- v C# Avalonia větvi mají evidence `Historie`, `Kilometry a tankování`, `Pojištění a doklady`, `Vlastní připomínky` a `Plán údržby` přístupné ovladače `Řadit` a `Sestupně`; zvolený sloupec i směr řazení se ukládají do `settings.ini`
- v C# Avalonia větvi mají `Globální hledání` a `Audit dat` stejné přístupné ovladače `Řadit` a `Sestupně`; poslední volba se ukládá do `settings.ini`, zatímco samotný hledaný text zůstává dočasný
- v C# Avalonia větvi `Globální hledání` prohledává i štítky, stav, servisní profil a stavové texty z časové osy; u historie, tankování, dokladů, připomínek a údržby bere v úvahu také název vozidla, SPZ a značku/model stejně jako AHK verze

## Klávesové zkratky v dashboardu a přehledech:

- `Dashboard`: `Ctrl+R` obnoví seznam, `Ctrl+F` otevře globální hledání, `Ctrl+T` otevře přehled termínů, `Ctrl+Shift+T` otevře propadlé termíny, `Ctrl+P` otevře vybraný nejbližší termín, `Ctrl+H` otevře historii vozidla, `Ctrl+L` dokončí vybraný servisní termín, `Ctrl+O` zobrazí vozidlo vybrané v dashboardu a `Ctrl+U` nebo `F2` ho upraví
- `Globální hledání`: `Ctrl+F` přesune fokus do hledání, tlačítko `Vymazat` smaže dotaz a vrátí fokus do hledání, `Řadit` / `Sestupně` přeskupí výsledky a uloží volbu, `Ctrl+R` obnoví výsledky bez ztráty výběru a `Ctrl+O`, `Ctrl+P` nebo `Enter` na seznamu otevře vybraný výsledek
- `Časová osa`: `Ctrl+F` přesune fokus do hledání, tlačítko `Vymazat` smaže dotaz a vrátí fokus do hledání, `Ctrl+R` obnoví časovou osu bez ztráty výběru a `Ctrl+P` nebo `Enter` otevře vybranou položku
- `Náklady napříč vozidly`: nahoře lze zvolit předvolbu období nebo vlastní datumový rozsah a tlačítkem `Přepočítat` ho použít; `Ctrl+F` přesune fokus do hledání vozidel, tlačítko `Vymazat` smaže dotaz a vrátí fokus do hledání, tlačítko `Obnovit` nebo `Ctrl+R` obnoví aktuální období, `Ctrl+P` přesune fokus na rozpad nákladů vybraného vozidla, `Enter` na seznamu nebo `Ctrl+O` zobrazí detail vozidla a `Ctrl+U` nebo `F2` upraví vybrané vozidlo
- `Přehled termínů`: `Ctrl+F` přesune fokus do hledání, tlačítko `Vymazat` smaže dotaz a vrátí fokus do hledání, `Ctrl+R` obnoví seznam, `Ctrl+P` otevře vybranou položku, `Ctrl+O` zobrazí vybrané vozidlo, `Ctrl+U` nebo `F2` upraví vybrané vozidlo a `Ctrl+Shift+T` přepne do propadlých termínů
- `Propadlé termíny`: `Ctrl+F` přesune fokus do hledání, tlačítko `Vymazat` smaže dotaz a vrátí fokus do hledání, `Ctrl+R` obnoví seznam, `Ctrl+P` otevře vybranou položku, `Ctrl+O` zobrazí vybrané vozidlo, `Ctrl+U` nebo `F2` upraví vybrané vozidlo a `Ctrl+T` přepne zpět do přehledu termínů
- v C# Avalonia větvi má `Dashboard` také horní tlačítka `Obnovit`, `Hledat`, `Souhrn nákladů`, `Blížící se`, `Propadlé`, `Zobrazit vozidlo`, `Historie vozidla`, `Náklady vozidla`, `Dokončit servis` a `Upravit vozidlo`; `Obnovit` stejně jako `Ctrl+R` přepočítá audit, náklady a nejbližší termíny bez ztráty aktuálního výběru
- v C# Avalonia větvi mají `Blížící se termíny` i `Propadlé termíny` také tlačítko `Obnovit` a přístupné ovladače `Řadit` / `Sestupně`; zachovají výběr a po obnově vrací fokus na seznam, pokud obsahuje položky
- `Audit dat`: `Ctrl+F` přesune fokus do hledání, `Ctrl+R` obnoví auditní seznam bez ztráty výběru, `Ctrl+P` nebo `Enter` na seznamu otevře řešenou položku, `Ctrl+O` zobrazí detail vozidla a `Ctrl+U` nebo `F2` otevře nejbližší relevantní úpravu
- v C# Avalonia větvi má `Audit dat` vlastní hledání nad všemi auditními položkami; tlačítko `Vymazat` smaže dotaz a vrátí fokus do hledání, `Řadit` / `Sestupně` přeskupí seznam a uloží volbu a dashboard zůstává jen stručným výřezem nejdůležitějších problémů
- v `Dashboardu`, `Přehledu termínů` i `Propadlých termínech` klávesa `Enter` otevře právě vybranou položku; stejné chování má i dvojklik na seznamu

## Klávesové zkratky v detailu a evidencích:

- `Detail vozidla`: `Ctrl+U` nebo `F2` upraví vozidlo, `Ctrl+H` otevře historii, `Ctrl+R` připomínky, `Ctrl+K` kilometry a tankování, `Ctrl+M` plán údržby a `Ctrl+P` pojištění a doklady; v C# Avalonia větvi jsou hlavní navazující evidence dostupné i jako pojmenovaná tlačítka v bloku `Související evidence`
- `Historie událostí`, `Kilometry a tankování`, `Pojištění a doklady` i `Vlastní připomínky`: `Ctrl+F` přesune fokus do rychlého hledání, `Ctrl+N` přidá záznam, `Ctrl+U` nebo `F2` upraví vybraný záznam a `Ctrl+D` otevře detail vozidla
- `Plán údržby`: `Ctrl+F` přesune fokus do hledání, `Ctrl+N` přidá úkon, `Ctrl+Shift+N` nabídne doporučené šablony, `Ctrl+U` nebo `F2` upraví vybraný úkon, `Ctrl+L` jej označí jako splněný a `Ctrl+D` otevře detail vozidla
- v C# Avalonia větvi má editor údržby rozbalovací `Šablonu úkonu`, která při ručním přidání rychle předvyplní název, intervaly a poznámku běžného servisu
- v C# Avalonia větvi je výběr doporučených servisních šablon dostupný i tlačítkem `Doporučené` ve sdíleném workspace údržby a funguje stejně v hlavní kartě i samostatném okně
- v C# Avalonia větvi je označení servisu jako splněného dostupné i tlačítkem `Splněno`; otevře potvrzovací dialog s datem, tachometrem a volitelným zápisem stejné události do historie vozidla
- `Výběr doporučených šablon` a `Balíček pro vozidlo`: `Ctrl+S` přidá vybrané položky, `Ctrl+A` vybere vše, `Ctrl+Shift+A` výběr vymaže, `Escape` dialog zavře a mezerník v seznamu přepne, zda se právě vybraná položka přidá
- v těchto čtyřech seznamech `Enter` upraví vybraný záznam a `Delete` jej odstraní
- v `Plánu údržby` klávesa `Enter` upraví vybraný úkon a `Delete` jej odstraní
- v AHK aplikaci kliknutí na hlavičku sloupce v evidencích přepíná řazení podle vybraného sloupce; v C# Avalonia větvi se stejná volba provádí přes přístupné ovladače `Řadit` a `Sestupně`, aby byla dobře čitelná i pro screen readery
- v `Pojištění a dokladech` navíc `Ctrl+O` otevře soubor u vybraného záznamu, `Ctrl+Shift+O` jeho složku a `Ctrl+Shift+C` zkopíruje uloženou cestu
- v C# Avalonia větvi je stejná dokladová akce dostupná i tlačítkem `Kopírovat cestu`; kopíruje vyřešenou cestu, tedy použitelnou absolutní cestu ke spravované i externí příloze, a případný problém se schránkou nebo otevřením souboru oznámí stavovou hláškou místo tichého selhání
- ve `Vlastních připomínkách` navíc `Ctrl+Shift+N` posune vybranou opakovanou připomínku na další termín
- v C# Avalonia větvi je stejná akce dostupná i tlačítkem `Další termín` ve sdíleném workspace připomínek a funguje stejně v hlavní kartě i samostatném okně
- v `Nákladech a souhrnech` `Ctrl+R` obnoví vybrané období a `Ctrl+D` otevře detail vozidla

## Klávesové zkratky ve formulářích a nastavení:

- `Ctrl+S`: uloží aktuální formulář nebo nastavení; v C# Avalonia větvi při chybě zápisu zůstane formulář otevřený a čtečka dostane chybový stav v editoru i shellu
- v `Nastavení` navíc `Ctrl+B`: vytvoří zálohu ihned
- v `Nastavení` klávesa `Esc` zavře dialog bez uložení
- v `O programu` zkratka `Ctrl+O` otevře release poznámky, `Ctrl+Shift+C` zkopíruje informace o aplikaci do schránky a `Esc` dialog zavře
- v aplikačních dialozích jako `Zkontrolovat aktualizace`, potvrzení akce, upozornění nebo `Akce Vehimapu na liště` klávesa `Esc` zavře dialog bez provedení nové akce; v kontrole aktualizací `Ctrl+Shift+C` zkopíruje detail výsledku do schránky a tlačítka se dál aktivují standardně klávesou `Enter` nebo mezerníkem
- v samostatných pracovních oknech detailu, evidencí a přehledů klávesa `Esc` zavře okno; pokud je uvnitř rozpracovaný editor, Vehimap se nejdřív zeptá na zahození změn
- v C# Avalonia větvi jsou dialogy `Nastavení`, `O programu` a `Zkontrolovat aktualizace` zvětšitelné a mají scrollovatelný hlavní obsah, takže dlouhé cesty nebo větší systémové písmo neschovají tlačítka `Uložit`, `Zavřít` nebo hlavní aktualizační akci
- v C# Avalonia větvi stejnou ochranu dostaly i potvrzovací dialogy, dokončení údržby, `Balíček pro vozidlo` a přístupné tray okno: dlouhý text nebo větší písmo rolují obsah, zatímco hlavní akce zůstávají dosažitelné

## Používání detailněji: 

Ve formuláři pro vozidlo:

- `Vlastní pojmenování`, `Kategorie`, `Značka / model` a `Příští TK` jsou povinné
- `Poznámka k vozidlu`, `Štítky`, `SPZ`, `Rok výroby`, `Výkon`, `Poslední TK`, `Zelená karta od`, `Zelená karta do`, `Pohon`, `Klimatizace`, `Rozvody` i `Převodovka` jsou volitelné
- štítky lze oddělovat čárkou nebo středníkem; při uložení se prázdné položky a duplicity sjednotí stejně jako v původní AHK aplikaci
- datum technické i zelené karty se zadává jako `MM/RRRR`, například `04/2026`; C# Avalonia editor přijme i `4.2026` nebo `4-2026` a při uložení je sjednotí na `04/2026`
- C# Avalonia editor při uložení sjednocuje zkrácené kategorie, převede SPZ na velká písmena a při neplatném datu nebo roku vrátí fokus na pole k opravě
- C# Avalonia editor při uložení normalizuje také stav vozidla a servisní profil podle AHK rozbalovacích hodnot; starší neznámé hodnoty se v editoru zobrazí jako prázdná volba

V historii událostí:

- `Datum události` a `Název události` jsou povinné
- datum události se zadává jako `DD.MM.RRRR`, například `26.03.2026`
- v C# Avalonia větvi se při uložení datum sjednotí na `DD.MM.RRRR`, tachometr musí být celé číslo a cena musí být číselná částka; při chybě se fokus vrátí na konkrétní pole
- `Stav tachometru`, `Cena nebo částka` a `Poznámka` jsou volitelné

V evidenci kilometrů a tankování:

- `Datum záznamu` a `Stav tachometru` jsou povinné
- datum záznamu se zadává jako `DD.MM.RRRR`, například `26.03.2026`
- v C# Avalonia větvi se při uložení datum, tachometr, litry a cena normalizují; pokud je vyplněná cena tankování, musí být vyplněné i litry
- v C# Avalonia větvi je `Typ paliva` rozbalovací seznam se stejnými hodnotami jako v AHK aplikaci; starší uložené volné texty zůstávají v seznamech čitelné
- `Natankováno litrů`, `Cena celkem v Kč`, `Typ paliva`, `Plná nádrž` a `Poznámka` jsou volitelné

V plánu údržby:

- `Název úkonu` je povinný
- alespoň jeden interval `po kilometrech` nebo `po měsících` musí být vyplněný
- v C# Avalonia větvi musí mít kilometrový interval také tachometr posledního servisu a měsíční interval datum posledního servisu; neplatná čísla nebo datum vrací fokus na příslušné pole
- `Poslední servis dne`, `Stav tachometru při posledním servisu`, `Poznámka` a volba aktivního plánu jsou volitelné
- nahoře lze zvolit šablonu běžného servisního úkonu, která předvyplní název, intervaly i doporučenou poznámku
- tlačítko `Doporučené šablony` otevře výběrový dialog, ve kterém lze doporučené plány podle kategorie a servisního profilu vozidla před přidáním odškrtnout nebo doladit
- po uložení nového vozidla může Vehimap stejný výběr doporučených šablon nabídnout automaticky hned v navazujícím kroku
- tlačítko `Označit splněno` uloží nové datum a tachometr posledního servisu a volitelně zapíše stejnou událost i do historie vozidla

V evidenci pojištění a dokladů:

- `Druh záznamu` a `Název záznamu` jsou povinné
- v C# Avalonia větvi je `Druh záznamu` rozbalovací seznam se stejnými hodnotami jako v AHK aplikaci
- `Platné od` a `Platné do` se zadávají jako `MM/RRRR`, například `04/2026`
- v C# Avalonia větvi se platnost při uložení normalizuje a hlídá se, aby `Platné od` nebylo později než `Platné do`; cena dokladu musí být číselná částka
- `Poskytovatel / vydavatel`, `Cena / částka`, `Režim přílohy`, `Příloha` a `Poznámka` jsou volitelné
- `Spravovaná kopie` uloží vybraný soubor relativně do `data/attachments/<id vozidla>/`, takže portable přesun celé aplikace přílohu nerozbije; uloženou interní cestu pak Vehimap ukazuje jen jako spravovanou hodnotu, ne jako běžně editovatelný technický vstup
- `Externí cesta` ponechá doklad napojený na původní soubor mimo aplikaci bez automatického kopírování
- tlačítko `Přesunout do příloh` umí převést existující externí cestu na spravovanou kopii a `Znovu propojit` opraví chybějící soubor v obou režimech
- stav přílohy zobrazuje režim, uloženou cestu, skutečně vyřešenou cestu i dostupnost a seznam navíc rozlišuje `Soubor`, `Složka`, `Chybí soubor`, `Chybí složka` nebo `Bez cesty`, takže hned poznáte nedostupnou přílohu
- otevření přílohy, otevření její složky i kopírování cesty v C# Avalonia větvi zapisuje čitelný výsledek do stavového textu, takže je jasné, zda akce proběhla, nebo proč selhala

V `Náklady a souhrny`:

- nahoře lze vybrat rok, předvolbu období a vlastní rozsah měsíců od 1 do 12
- předvolby pokrývají 1, 2, 3, 6, 9 a 12 měsíců, ale můžete si nastavit i vlastní rozsah v rámci roku
- souhrn vybraného období nově dopočítává i `Ujeto km`, `Cenu / km` a srovnání proti předchozímu stejně dlouhému období
- pod tím zůstává i dlouhodobý přehled podle jednotlivých let
- tlačítka `TSV souhrn`, `TSV detail` a `HTML sestava` umí vyexportovat vybrané období i dlouhodobý přehled do souboru pro další práci, tisk nebo archivaci

V `Časové ose vozidla`:

- nahoře lze přepnout mezi `Vše`, `Budoucí` a `Minulé` a průběžně vyhledávat podle druhu, položky, detailu nebo stavu
- nahoře se zobrazují nejbližší budoucí termíny a pod nimi nejnovější minulost
- tlačítko `Otevřít položku` nebo klávesa `Enter` otevře správnou evidenci přímo na vybrané položce
- tlačítko `Detail vozidla` otevře detail právě sledovaného vozidla bez návratu do hlavního seznamu

Ve vlastních připomínkách:

- `Název připomínky`, `Termín` a `Upozornit dnů předem` jsou povinné
- `Opakování` může být `Neopakovat`, `Každý rok`, `Každé 2 roky` nebo `Každých 5 let`
- v C# Avalonia větvi je `Opakování` rozbalovací seznam, takže není potřeba ručně opisovat text hodnoty
- v C# Avalonia větvi nová připomínka předvyplní předstih `30` dnů a při uložení normalizuje termín na `DD.MM.RRRR`
- tlačítko `Posunout na další` přesune opakovanou připomínku na další termín bez nutnosti ručního přepisu data

V nastavení:

- volba `Pravidelně vytvářet automatické zálohy` zapne interní zálohování celé aplikace bez dotazu na název souboru
- pole `Interval automatické zálohy ve dnech` určuje, po kolika dnech má vzniknout nová automatická záloha
- pole `Ponechat posledních automatických záloh` určuje, kolik nejnovějších souborů si Vehimap ponechá a starší automaticky smaže
- pokud je volba pravidelných automatických záloh vypnutá, pole pro interval a počet ponechaných záloh jsou v C# dialogu Nastavení neaktivní a neblokují uložení ostatních voleb
- tlačítko `Zálohovat ihned` vytvoří novou zálohu okamžitě do stejné složky bez čekání na další automatický interval
- v C# Avalonia větvi lze stejnou okamžitou automatickou zálohu spustit i z menu `Soubor` nebo z přístupného tray okna a vedle toho přímo otevřít složku `data/auto-backups`
- pole `Upozornit na údržbu dnů předem` určuje, jak brzy má Vehimap začít hlásit blížící se servis podle data
- pole `Upozornit na údržbu kilometrů předem` určuje, jak brzy má Vehimap začít hlásit blížící se servis podle tachometru
- automatické zálohy se ukládají do složky `data/auto-backups`

Pro upozornění aplikace používá pole `Příští TK`, `Zelená karta do` a u servisních úkonů intervaly uložené v `Plánu údržby`. Doporučené servisní šablony navíc vycházejí z kategorie a servisního profilu vozidla, tedy z pohonu, klimatizace, rozvodů a převodovky, které si můžete sami upravit.

V horním menu najdete tyto části:

- `Soubor`: tiskový přehled, export a import zálohy, export budoucích termínů do kalendáře, znovunačtení dat, v C# Avalonia větvi také okamžitá automatická záloha, otevření datové složky a otevření složky automatických záloh a v obou větvích ukončení aplikace
- `Vozidlo`: práce s vybraným vozidlem včetně detailu, historie, kilometrů a tankování, plánu údržby, `Časové osy vozidla`, `Balíčku pro vozidlo` a pojištění a dokladů
- `Přehled` v AHK nebo `Přehledy` v C# Avalonia větvi: `Dashboard`, `Náklady napříč vozidly`, `Globální hledání`, `Časová osa vozidla`, blížící se a propadlé termíny, `Audit dat` a export termínů do kalendáře `.ics`
- `Rychlé akce` v C# Avalonia větvi: aktuální upozornění z pozadí, nejbližší TK, ZK, připomínka, servis nebo doklad a filtrovaná kontrola těchto termínů v přehledech; kontrola ZK zahrne i vozidla bez vyplněné zelené karty a položky bez aktuálního cíle jsou v menu vypnuté
- `Nástroje` v AHK aplikaci: `Nastavení`, `Skrýt do lišty`
- `Nápověda` v AHK aplikaci nebo `Aplikace` v C# Avalonia větvi: `Nastavení`, minimalizace na lištu, `O programu`, kontrola aktualizací a ukončení aplikace

## Ukládání dat

- vozidla se ukládají do souboru `data/vehicles.tsv`
- historie událostí se ukládá do souboru `data/history.tsv`
- kilometry a tankování se ukládají do souboru `data/fuel.tsv`
- pojištění a doklady se ukládají do souboru `data/records.tsv`
- spravované přílohy dokladů se ukládají do složky `data/attachments/<id vozidla>/`
- plány údržby se ukládají do souboru `data/maintenance.tsv`
- nastavení upozornění a chování aplikace se ukládá do souboru `data/settings.ini`
- automatické zálohy se ukládají do složky `data/auto-backups`
- při importu se původní soubory před přepsáním automaticky odloží do `data/import-backups`
- v C# Avalonia větvi se při obnově zálohy do `data/import-backups/<čas>` odkládají i spravované přílohy; stavová hláška po importu ukáže konkrétní složku s původními daty i počet obnovených příloh
- pokud je vybraný `.vehimapbak` soubor nedostupný, poškozený nebo obsahuje neplatná data příloh, C# Avalonia větev nepřeruší shell výjimkou, ale oznámí cestu k záloze a čitelný detail chyby ve stavovém textu
- pokud uživatel zruší výběr souboru pro export nebo import zálohy v C# Avalonia větvi, stavový text to výslovně oznámí, aby nezůstala čtená stará hláška z předchozí akce
- oba soubory jsou ve složce `data` vedle aplikace
- Vehimap zapisuje vozidla ve formátu `# Vehimap data v4`
- historie používá hlavičku `# Vehimap history v1`
- kilometry a tankování používají hlavičku `# Vehimap fuel v1`
- pojištění a doklady používají hlavičku `# Vehimap records v2`
- plány údržby používají hlavičku `# Vehimap maintenance v1`
- export vytváří jeden soubor se zálohou ve formátu `.vehimapbak`, který nově zahrnuje i spravované přílohy dokladů; pokud některá spravovaná příloha na disku chybí, export pokračuje a v C# Avalonia větvi to uvede ve výsledkové hlášce

## Poznámka k oznamovací oblasti

Zavření hlavního okna aplikaci neukončí. Vehimap se schová do oznamovací oblasti a dál hlídá technické kontroly, zelené karty, vlastní připomínky i plány údržby. Kontrola běží průběžně na pozadí každých 15 minut a znovu se vyvolá i po probuzení počítače ze spánku; v C# Avalonia větvi je toto explicitní napojení zatím Windows-only a na ostatních platformách zůstávají bezpečným fallbackem pravidelné intervaly. Stejným způsobem se na pozadí jednou za hodinu ověřuje i potřeba automatické zálohy. Pokud je vše v pořádku, tooltip tray ikony zůstává jen `Vehimap`; pokud ne, zobrazí souhrn propadlých a brzy končících `TK`, `ZK`, připomínek i servisních úkonů. V C# Avalonia větvi se jako první oznámení vybírá nejbližší termín k řešení; audit dat se použije jako fallback, když žádný termín právě nevyžaduje pozornost. Poslední zobrazené desktopové oznámení se pamatuje v `settings.ini` po dnech, aby se stejný akutní termín neoznamoval opakovaně při každé kontrole. Po úspěšném uložení dat, importu zálohy nebo změně nastavení se tento tray/background snapshot obnoví hned, bez čekání na další plánovanou kontrolu.

V C# Avalonia větvi nativní menu lišty otevírá vlastní přístupné okno `Akce Vehimapu na liště`. Hned nahoře ukazuje aktuální stav pozadí ze stejného snapshotu jako tooltip a desktopové oznámení, takže jej lze přečíst i tehdy, když čtečka obrazovky neumí spolehlivě oznámit nativní tray tooltip. Tlačítko `Otevřít aktuální upozornění` z tohoto stavu skočí přímo na nejdůležitější termín k řešení, případně na první auditní položku, pokud žádný termín právě pozornost nevyžaduje. Z okna lze klávesnicí a čtečkou obrazovky zobrazit hlavní okno, otevřít `Dashboard`, přejít rovnou do `Blížících se termínů` nebo `Propadlých termínů`, otevřít nejbližší TK, ZK, připomínku, servisní úkon nebo doklad, spustit filtrovanou kontrolu těchto oblastí, uložit tiskový přehled, exportovat nebo obnovit zálohu, vytvořit okamžitou automatickou zálohu, exportovat budoucí termíny do kalendáře, znovu načíst data, otevřít datovou složku nebo složku automatických záloh, otevřít nastavení, zobrazit `O programu`, zkontrolovat aktualizace, případně aplikaci ukončit. Tlačítka pro nejbližší položky a filtrované kontroly jsou aktivní jen tehdy, když existuje odpovídající termín k řešení; navigační a datové akce se při rozpracované editaci vypnou, aby nedošlo ke ztrátě neuložených změn.

## Vývojové ověření C# větve

C# Avalonia větev má vedle unit testů také Appium smoke testy nad izolovanou kopií publish buildu. CI smoke kontroluje dostupnost menu `Soubor` a `Rychlé akce`, jejich aktuální aktivní/neaktivní stavy a hlavní app-level vstupy. Rozšířená Appium sada navíc aktivuje `Aktuální upozornění` v menu `Rychlé akce`, ověřuje automatické otevření `Balíčku pro vozidlo` po uložení nového vozidla, jeho ruční spuštění z menu `Vozidlo`, otevření doporučených servisních šablon, potvrzovací dialog `Splněno` z `Plánu údržby` i dashboardové akce `Dokončit servis`, uložení volby Dashboardu při startu v `Nastavení` včetně validační chyby a spouští okamžitou automatickou zálohu v dočasné datové složce, aby se ověřilo, že důležité akce opravdu navigují, ukládají nastavení nebo vytvoří `.vehimapbak`, aniž by sahaly do uživatelských dat.
