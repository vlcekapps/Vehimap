# Changelog
Všechny významné změny ve Vehimapu budou zapisovány sem.
Formát vychází z [Keep a Changelog](https://keepachangelog.com/cs/1.1.0/)
a projekt používá [Semantic Versioning](https://semver.org/lang/cs/).

## [Unreleased]

### Přidáno
- nový přehled `Audit dat`, který sjednocuje chybějící povinné údaje, neplatné rozsahy, problematické doklady, podezřelé tachometry, nákladové nesrovnalosti i servisní plány bez použitelného odometru
- explicitní servisní profil vozidla s poli `Pohon`, `Klimatizace`, `Rozvody` a `Převodovka`, který slouží jako základ pro doporučené servisní šablony
- výběrový dialog doporučených servisních šablon, ve kterém lze návrhy před přidáním odškrtnout nebo upravit
- automatická nabídka doporučených servisních šablon hned po založení nového vozidla
- nový `Balíček pro vozidlo`, který umí v jednom kroku nabídnout doporučené servisní plány, placeholdery dokladů i obecné připomínky podle kategorie i servisního profilu vozidla
- akce v dashboardu pro rychlé otevření historie vozidla a okamžité označení servisního úkonu jako splněného
- samostatný přehled `Náklady napříč vozidly` pro porovnání vozidel v jednom období
- výpočet `Ujeto km`, `Ceny / km` a srovnání proti předchozímu stejně dlouhému období v nákladech vozidla i v přehledu `Náklady napříč vozidly`
- nová `Časová osa vozidla`, která spojuje historii, tankování, připomínky, expirace dokladů, TK, ZK i servisní úkoly s konkrétním datem
- ruční export budoucích termínů do kalendářového souboru `.ics`
- spravované přílohy dokladů s volbou mezi `Externí cestou` a `Spravovanou kopií`
- interní složka `data/attachments/<id vozidla>/` pro portable uložení spravovaných příloh
- rozšířený export a import `.vehimapbak`, který umí přenést i spravované přílohy dokladů
- v Avalonia větvi samostatná okna pro `Časovou osu`, `Globální hledání`, `Náklady napříč vozidly`, `Blížící se termíny` a `Propadlé termíny`, postavená nad stejnými workspace views jako hlavní karty
- C# desktop větev umí odstranit vybrané vozidlo včetně potvrzení, navázaných evidencí a spravovaných příloh
- v Avalonia větvi lze z menu vybraného vozidla otevřít `Náklady a souhrny` s detailem rozpadu nákladů
- nákladový workspace v Avalonia větvi umí exportovat flotilový souhrn TSV, detail vybraného vozidla TSV a HTML sestavu
- připomínkový workspace v Avalonia větvi umí tlačítkem `Další termín` nebo zkratkou `Ctrl+Shift+N` posunout opakovanou připomínku podle nastaveného intervalu
- servisní workspace v Avalonia větvi otevírá tlačítkem `Splněno` nebo zkratkou `Ctrl+L` potvrzovací dialog pro datum a tachometr splněného úkonu a umí stejnou událost volitelně zapsat i do historie vozidla
- servisní workspace v Avalonia větvi umí tlačítkem `Doporučené` nebo zkratkou `Ctrl+Shift+N` otevřít výběr chybějících servisních šablon podle kategorie a servisního profilu vozidla
- editor plánu údržby v Avalonia větvi má výběr běžné servisní šablony, který předvyplní název úkonu, intervaly a poznámku

### Změněno
- pole `Typ` u vozidla bylo nahrazeno praktičtější `Poznámkou k vozidlu`
- přehledy termínů a dashboard lépe zvýrazňují problémové stavy, datové nedostatky a servisní úkoly
- Vehimap je interně rozdělený do menších modulů `#Include`, takže se aplikace lépe udržuje a rozvíjí
- evidence `Pojištění a doklady` nově pracuje s vyřešenou cestou k příloze napříč dialogy, auditem, hledáním, časovou osou i nákladovými přehledy
- formát `records.tsv` byl posunut na `# Vehimap records v2` a backup na `# Vehimap backup v6`
- hlavní pracovní okna jsou nově zvětšitelná a důležité seznamy se při změně velikosti roztahují do šířky i výšky
- `Audit dat` dostal plné klávesové ovládání ve stylu ostatních přehledů a detail vozidla nově po otevření fokusuje primární akci úpravy
- formulář dokladu v režimu `Spravovaná kopie` už nevystavuje interní relativní cestu jako běžně editovatelné pole a přehled dokladů nově jasně zobrazuje i režim přílohy
- Avalonia shell už se v titulku a přístupném názvu neprezentuje jako preview a dostal samostatnou nabídku `Přehledy` pro otevření hlavních přehledových oken
- klávesové zkratky přehledů v Avalonia shellu jsou sjednocené s hlavní dokumentací: `Ctrl+D` dashboard, `Ctrl+T` blížící se termíny a `Ctrl+Shift+T` propadlé termíny
- C# desktop větev používá Avalonia `12.0.4` místo původní release-candidate verze a NuGet restore už nehlásí zranitelný `Tmds.DBus.Protocol`
- preview release tooling C# větve zapisuje do package metadat i SHA-256 a velikost balíčku a generátor update manifestu ověřuje shodu metadat, `.sha256` souboru i fyzického artefaktu
- dostupnost akce `Minimalizovat na lištu` v Avalonia shellu je nově řízená viewmodelem podle podpory tray a nastavení automatických záloh vypíná intervalová pole, pokud nejsou pravidelné zálohy zapnuté
- dialog `Nastavení` v Avalonia větvi má vlastní klávesové ovládání `Ctrl+S`, `Ctrl+B` a `Esc` i přístupný help text s těmito zkratkami
- app-level dialogy v Avalonia větvi mají sjednocené bezpečné zavření klávesou `Esc`; `O programu` navíc nabízí `Ctrl+O` pro otevření release poznámek a všechny tyto dialogy popisují zkratky v přístupném help textu
- `Tiskový přehled` v Avalonia větvi se nově ukládá přes standardní exportní dialog jako HTML soubor a po uložení se otevře, místo aby vznikal jen jako dočasný soubor
- nabídka `Rychlé akce` v Avalonia větvi nově kromě TK a zelených karet umí otevřít nejbližší vlastní připomínku, servisní úkon nebo doklad a filtrovaně zkontrolovat připomínky, údržbu i doklady v přehledu termínů
- přístupné okno `Akce Vehimapu na liště` v Avalonia větvi umí kromě hlavního okna, dashboardu, přehledů a ukončení aplikace otevřít i nejbližší TK, ZK, připomínku, servisní úkon nebo doklad, spustit filtrované kontroly těchto oblastí a vyvolat tiskový přehled, export/import zálohy, export kalendáře, znovunačtení dat, nastavení, `O programu` nebo kontrolu aktualizací

### Opraveno
- zpracování servisních doporučení, záloh a meta dat vozidel tak, aby správně fungoval nový servisní profil i smoke testy
- výpočet ujetých kilometrů v nákladových souhrnech už nepřidává jednotku `km` dvakrát
- spravované přílohy se po přepnutí zpět na externí cestu korektně uklidí a smoke testy nově hlídají i round-trip přes zálohu
- akční tlačítka v `Plánu údržby`, `Auditu dat`, `Časové ose`, přehledech a dokladech teď důsledně respektují skutečný výběr v seznamu
- horní menu v Avalonia shellu jde vyvolat klávesou `F10` i samostatným `Alt`, ale zůstává mimo běžné pořadí `Tab` / `Shift+Tab`
- sdílené Avalonia workspace pro časovou osu, globální hledání a termínové přehledy mají vlastní `Ctrl+F`, `Ctrl+O` a `Ctrl+P` zkratky, takže stejné ovládání funguje v kartě i samostatném okně
- `Ctrl+F`, `Ctrl+O` a `Ctrl+P` v Avalonia shellu se v přehledových kartách routují kontextově a už nepřebíjejí aktivní hledání nebo otevření vybrané položky globální akcí hlavního okna
- `Ctrl+N`, `Ctrl+U` / `F2`, `Ctrl+S` a dokladové `Ctrl+O` / `Ctrl+Shift+O` v Avalonia shellu se nově routují podle aktivní evidence, takže stejné editace fungují v hlavních kartách i samostatných workspace oknech
- `Audit dat` v Avalonia větvi má vlastní hledání, plný seznam auditních položek mimo dashboardový výřez a zkratky `Ctrl+F`, `Ctrl+O`, `Ctrl+P`, `Ctrl+U` / `F2`
- `Náklady napříč vozidly` v Avalonia větvi mají vlastní akce a zkratky: `Ctrl+P` přesune fokus na rozpad nákladů, `Ctrl+O` nebo `Enter` otevře vozidlo a `Ctrl+U` / `F2` otevře editor vozidla
- otevírání souborů a složek v C# větvi nově používá explicitní platformní strategii: shell execute na Windows, `open` na macOS a `xdg-open` na Linuxu
- Linux autostart záznam v C# větvi už se v popisu neoznačuje jako `preview`
- samostatná Avalonia workspace okna mají sjednocené zavírací `AutomationId` a UI smoke test nově ověřuje jejich otevření i zavření napříč hlavními workflow
- Avalonia okna mají sjednocené kořenové přístupné názvy a `AutomationId`, aby šla spolehlivěji testovat a diagnostikovat pomocí UI automatizace

## [1.0.2] - 2026-03-27

### Opraveno
- kontrola aktualizací ve zkompilované portable aplikaci teď správně načítá veřejný release manifest
- doplněny smoke testy pro kontrolu aktualizací a načítání update manifestu

## [1.0.1] - 2026-03-27

### Přidáno
- první veřejné vydání aplikace Vehimap
- evidence vozidel, historie událostí, kilometrů a tankování, pojištění a dokladů i vlastních připomínek
- dashboard, přehled blížících se a propadlých termínů, globální hledání a rychlé filtrování v jednotlivých evidencích
- exporty, import dat, automatické zálohy, klávesové zkratky a další úpravy přístupnosti
- ruční kontrola aktualizací a portable aktualizace aplikace z GitHub release
