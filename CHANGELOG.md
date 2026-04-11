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

### Opraveno
- zpracování servisních doporučení, záloh a meta dat vozidel tak, aby správně fungoval nový servisní profil i smoke testy
- výpočet ujetých kilometrů v nákladových souhrnech už nepřidává jednotku `km` dvakrát
- spravované přílohy se po přepnutí zpět na externí cestu korektně uklidí a smoke testy nově hlídají i round-trip přes zálohu
- akční tlačítka v `Plánu údržby`, `Auditu dat`, `Časové ose`, přehledech a dokladech teď důsledně respektují skutečný výběr v seznamu
- horní menu v Avalonia shellu jde vyvolat klávesou `F10` i samostatným `Alt`, ale zůstává mimo běžné pořadí `Tab` / `Shift+Tab`
- otevírání souborů a složek v C# větvi nově používá explicitní platformní strategii: shell execute na Windows, `open` na macOS a `xdg-open` na Linuxu
- Linux autostart záznam v C# větvi už se v popisu neoznačuje jako `preview`

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
