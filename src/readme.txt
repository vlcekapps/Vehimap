# Vehimap

První verze jednoduché přístupné evidence vozidel v AutoHotkey v2.

## Co umí

- rozdělení vozidel do kategorií `Osobní vozidla`, `Motocykly`, `Nákladní vozidla`, `Autobusy`, `Ostatní`
- přidání, úpravu a odstranění vozidla
- vyhledávání v hlavním seznamu podle názvu, poznámky, značky nebo SPZ
- globální hledání napříč vozidly, historií událostí, kilometry a tankováním, pojištěním a doklady, vlastními připomínkami i plány údržby
- filtr hlavního seznamu na všechna vozidla, vozidla s blížícím se termínem, vozidla po termínu a vozidla bez vyplněné zelené karty
- volitelné skrytí archivovaných a odstavených vozidel v hlavním seznamu, aniž by zmizela z dat a přehledů
- detail vozidla se souhrnem údajů, stavem platností, posledními událostmi z historie a souhrnem tankování i dokladů
- historii událostí pro každé vozidlo, včetně přidání, úpravy a odstranění servisních nebo jiných záznamů
- samostatnou evidenci `Kilometry a tankování` pro každé vozidlo, včetně přidání, úpravy a odstranění záznamů
- samostatný `Plán údržby` pro každé vozidlo, včetně šablon běžných servisních úkonů, intervalu podle kilometrů nebo měsíců, evidence posledního servisu a rychlého označení splnění s volitelným zapsáním do historie
- rychlé hledání a řazení sloupců v historii událostí, kilometrech a tankování, pojištění a dokladech i vlastních připomínkách, včetně zapamatování posledního řazení
- samostatnou evidenci `Pojištění a doklady` pro každé vozidlo, včetně přidání, úpravy a odstranění záznamů, otevření navázaného souboru i otevření jeho složky, kopírování cesty a zobrazení stavu dostupnosti přílohy
- `Náklady a souhrny` pro každé vozidlo s přehledem podle roku i s výběrem vlastního období po měsících v rámci zvoleného roku
- samostatný přehled `Náklady napříč vozidly` pro vybrané období s porovnáním vozidel, rozpadnutím částek podle skupin, upozorněním na aktivní vozidla bez číselného nákladu a se stavy k řešení přímo u jednotlivých vozidel
- export nákladových přehledů do `TSV souhrnu`, `TSV detailu` a `HTML sestavy`
- vlastní připomínky i s volbou opakování `Neopakovat`, `Každý rok`, `Každé 2 roky` a `Každých 5 let`
- upozornění na blížící se nebo propadlou technickou kontrolu
- upozornění na blížící se nebo propadlou zelenou kartu
- samostatný `Dashboard` s rychlým souhrnem vozidel, termínů, nákladů v aktuálním roce, pořadím nejdražších vozidel, upozorněním na aktivní vozidla bez číselného nákladu, rychlými highlighty nejpalčivějších problémových stavů a akčním seznamem nejbližších termínů, blížících se servisních úkonů, chybějících zelených karet, vozidel bez SPZ nebo příští TK i dokladů s nedostupnou nebo prázdnou cestou
- samostatný dialog s přehledem všech blížících se a propadlých termínů, servisních úkonů a připomínek, filtrem, rychlým hledáním podle názvu, SPZ nebo typu položky, ruční obnovou, řazením podle sloupců, volitelným zobrazením datových nedostatků, přímým otevřením řešené položky, otevřením vozidla i editací vozidla a zapamatováním posledního nastavení přehledu
- samostatný dialog `Propadlé termíny` se seznamem všech už propadlých `TK`, `ZK` a servisních úkonů, rychlým hledáním a přímým otevřením řešené položky, otevřením vozidla nebo editací vozidla
- `Tiskový přehled` všech vozidel ve formátu HTML, který se otevře v prohlížeči a dá se vytisknout běžným `Ctrl+P`
- `Export dat` do jednoho záložního souboru `.vehimapbak` včetně plánů údržby
- `Import dat` z dříve vytvořené zálohy včetně automatické zálohy původních souborů před přepsáním a obnovení plánů údržby
- pravidelné automatické zálohy do `data/auto-backups` se samostatným intervalem ve dnech a omezením počtu ponechaných souborů
- samostatné nastavení počtu dnů pro upozornění na `TK`, `ZK` i servisní plány a kilometrového limitu pro blížící se údržbu
- volby `Spustit po startu počítače`, `Automaticky skrýt na lištu` a `Zobrazovat dashboard při startu`
- horní menu `Soubor`, `Vozidlo`, `Přehled`, `Nástroje` a `Nápověda`
- tray menu pro rychlé otevření nejbližších termínů i dalších funkcí aplikace
- automatickou kontrolu termínů, připomínek a servisních plánů každých 15 minut a znovu po probuzení počítače ze spánku

## Jak se používá

Ve většině případů stačí spustit `Vehimap.exe`.

V hlavním okně:

- záložky nahoře dál rozdělují vozidla podle kategorií
- pole `Hledat název, značku, SPZ, poznámku nebo štítek` filtruje jen aktuálně otevřenou kategorii
- pole `Filtr seznamu` rychle zobrazí jen vozidla, která právě vyžadují pozornost
- zaškrtávátko `Skrýt archivovaná a odstavená vozidla` schová neaktivní vozidla jen z hlavního seznamu a svou volbu si pamatuje i po dalším spuštění
- tlačítka `Detail vozidla` a `Historie událostí` pracují s právě vybraným vozidlem, další evidence včetně `Plánu údržby` otevřete i z menu `Vozidlo`
- položky `Dashboard` a `Globální hledání` v menu `Přehled` otevřou rychlý souhrn termínů, servisních úkonů, nákladů, problémových stavů a stavu evidencí nebo vyhledání napříč všemi evidencemi
- položka `Náklady napříč vozidly` v menu `Přehled` otevře porovnání nákladů za zvolené období mezi všemi vozidly a umožní z přehledu rovnou přejít na detail nákladů, detail vozidla nebo editaci
- v menu `Nápověda` najdete `O programu` s přehledem verze, cesty k aplikaci a datové složky i samostatnou ruční kontrolu aktualizací
- v dashboardu souhrn vozidel vypisuje i nejpalčivější problémové stavy podle priority a nákladový souhrn ukazuje nejdražší vozidla i aktivní vozidla bez číselného nákladu v aktuálním roce
- v dashboardu, `Přehledu termínů` i `Propadlých termínech` se vedle `TK`, `ZK` a vlastních připomínek zobrazují i blížící se nebo propadlé servisní úkony
- v dashboardu se v seznamu zobrazují nejen nejbližší termíny, ale i datové nedostatky jako chybějící SPZ, chybějící příští TK nebo problémové dokladové přílohy
- v dashboardu přibyly i akce `Souhrn nákladů` a `Náklady vozidla`, takže jde rovnou otevřít globální přehled nákladů nebo náklady vybraného vozidla z právě řešeného problému
- v dashboardu lze nově z vybraného servisního úkonu rovnou otevřít historii vozidla nebo servis přímo označit jako splněný bez ručního hledání správného plánu
- v `Přehledu termínů` lze pod hledáním zapnout i datové nedostatky, takže se vedle termínů zobrazí i chybějící SPZ, příští TK nebo problémové dokladové přílohy
- v dashboardu je i zaškrtávátko `Zobrazovat dashboard při startu`, které změnu uloží ihned
Klávesové zkratky v hlavním okně:

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

Klávesové zkratky v dashboardu a přehledech:

- `Dashboard`: `Ctrl+R` obnoví seznam, `Ctrl+F` otevře globální hledání, `Ctrl+T` otevře přehled termínů, `Ctrl+Shift+T` otevře propadlé termíny, `Ctrl+P` otevře řešitelnou položku, `Ctrl+H` otevře historii vybraného vozidla, `Ctrl+L` označí vybraný servisní úkon jako splněný, `Ctrl+O` zobrazí vybrané vozidlo a `Ctrl+U` nebo `F2` upraví vybrané vozidlo
- `Globální hledání`: `Ctrl+F` přesune fokus do hledání, `Ctrl+O` nebo `Enter` na seznamu otevře vybraný výsledek
- `Náklady napříč vozidly`: `Ctrl+R` obnoví období, `Ctrl+P` nebo `Enter` na seznamu otevře náklady vybraného vozidla, `Ctrl+O` zobrazí detail vozidla a `Ctrl+U` nebo `F2` upraví vybrané vozidlo
- `Přehled termínů`: `Ctrl+F` přesune fokus do hledání, `Ctrl+R` obnoví seznam, `Ctrl+P` otevře vybranou položku, `Ctrl+O` zobrazí vybrané vozidlo, `Ctrl+U` nebo `F2` upraví vybrané vozidlo a `Ctrl+Shift+T` přepne do propadlých termínů
- `Propadlé termíny`: `Ctrl+F` přesune fokus do hledání, `Ctrl+R` obnoví seznam, `Ctrl+P` otevře vybranou položku, `Ctrl+O` zobrazí vybrané vozidlo, `Ctrl+U` nebo `F2` upraví vybrané vozidlo a `Ctrl+T` přepne zpět do přehledu termínů
- v `Dashboardu`, `Přehledu termínů` i `Propadlých termínech` klávesa `Enter` otevře právě vybranou položku; stejné chování má i dvojklik na seznamu

Klávesové zkratky v detailu a evidencích:

- `Detail vozidla`: `Ctrl+U` nebo `F2` upraví vozidlo, `Ctrl+H` otevře historii, `Ctrl+R` připomínky, `Ctrl+K` kilometry a tankování, `Ctrl+M` plán údržby a `Ctrl+P` pojištění a doklady
- `Historie událostí`, `Kilometry a tankování`, `Pojištění a doklady` i `Vlastní připomínky`: `Ctrl+F` přesune fokus do rychlého hledání, `Ctrl+N` přidá záznam, `Ctrl+U` nebo `F2` upraví vybraný záznam a `Ctrl+D` otevře detail vozidla
- `Plán údržby`: `Ctrl+F` přesune fokus do hledání, `Ctrl+N` přidá úkon, `Ctrl+U` nebo `F2` upraví vybraný úkon, `Ctrl+L` jej označí jako splněný a `Ctrl+D` otevře detail vozidla
- v těchto čtyřech seznamech `Enter` upraví vybraný záznam a `Delete` jej odstraní
- v `Plánu údržby` klávesa `Enter` upraví vybraný úkon a `Delete` jej odstraní
- kliknutí na hlavičku sloupce v těchto čtyřech seznamech přepíná řazení podle vybraného sloupce a Vehimap si poslední volbu pamatuje i po dalším otevření
- v `Pojištění a dokladech` navíc `Ctrl+O` otevře soubor u vybraného záznamu, `Ctrl+Shift+O` jeho složku a `Ctrl+Shift+C` zkopíruje uloženou cestu
- ve `Vlastních připomínkách` navíc `Ctrl+Shift+N` posune vybranou opakovanou připomínku na další termín
- v `Nákladech a souhrnech` `Ctrl+R` obnoví vybrané období a `Ctrl+D` otevře detail vozidla

Klávesové zkratky ve formulářích a nastavení:

- `Ctrl+S`: uloží aktuální formulář nebo nastavení
- v `Nastavení` navíc `Ctrl+B`: vytvoří zálohu ihned

Ve formuláři pro vozidlo:

- `Vlastní pojmenování`, `Kategorie`, `Značka / model` a `Příští TK` jsou povinné
- `Poznámka k vozidlu`, `SPZ`, `Rok výroby`, `Výkon`, `Poslední TK`, `Zelená karta od` a `Zelená karta do` jsou volitelné
- datum technické i zelené karty se zadává jako `MM/RRRR`, například `04/2026`

V historii událostí:

- `Datum události` a `Název události` jsou povinné
- datum události se zadává jako `DD.MM.RRRR`, například `26.03.2026`
- `Stav tachometru`, `Cena nebo částka` a `Poznámka` jsou volitelné

V evidenci kilometrů a tankování:

- `Datum záznamu` a `Stav tachometru` jsou povinné
- datum záznamu se zadává jako `DD.MM.RRRR`, například `26.03.2026`
- `Natankováno litrů`, `Cena celkem v Kč`, `Typ paliva`, `Plná nádrž` a `Poznámka` jsou volitelné

V plánu údržby:

- `Název úkonu` je povinný
- alespoň jeden interval `po kilometrech` nebo `po měsících` musí být vyplněný
- `Poslední servis dne`, `Stav tachometru při posledním servisu`, `Poznámka` a volba aktivního plánu jsou volitelné
- nahoře lze zvolit šablonu běžného servisního úkonu, která předvyplní název, intervaly i doporučenou poznámku
- tlačítko `Označit splněno` uloží nové datum a tachometr posledního servisu a volitelně zapíše stejnou událost i do historie vozidla

V evidenci pojištění a dokladů:

- `Druh záznamu` a `Název záznamu` jsou povinné
- `Platné od` a `Platné do` se zadávají jako `MM/RRRR`, například `04/2026`
- `Poskytovatel / vydavatel`, `Cena / částka`, `Soubor nebo cesta` a `Poznámka` jsou volitelné
- seznam zobrazuje i stav cesty `Soubor`, `Složka`, `Chybí soubor`, `Chybí složka` nebo `Bez cesty`, takže hned poznáte nedostupnou přílohu

V `Náklady a souhrny`:

- nahoře lze vybrat rok, předvolbu období a vlastní rozsah měsíců od 1 do 12
- předvolby pokrývají 1, 2, 3, 6, 9 a 12 měsíců, ale můžete si nastavit i vlastní rozsah v rámci roku
- pod tím zůstává i dlouhodobý přehled podle jednotlivých let
- tlačítka `TSV souhrn`, `TSV detail` a `HTML sestava` umí vyexportovat vybrané období i dlouhodobý přehled do souboru pro další práci, tisk nebo archivaci

Ve vlastních připomínkách:

- `Název připomínky`, `Termín` a `Upozornit dnů předem` jsou povinné
- `Opakování` může být `Neopakovat`, `Každý rok`, `Každé 2 roky` nebo `Každých 5 let`
- tlačítko `Posunout na další` přesune opakovanou připomínku na další termín bez nutnosti ručního přepisu data

V nastavení:

- volba `Pravidelně vytvářet automatické zálohy` zapne interní zálohování celé aplikace bez dotazu na název souboru
- pole `Interval automatické zálohy ve dnech` určuje, po kolika dnech má vzniknout nová automatická záloha
- pole `Ponechat posledních automatických záloh` určuje, kolik nejnovějších souborů si Vehimap ponechá a starší automaticky smaže
- tlačítko `Zálohovat ihned` vytvoří novou zálohu okamžitě do stejné složky bez čekání na další automatický interval
- pole `Upozornit na údržbu dnů předem` určuje, jak brzy má Vehimap začít hlásit blížící se servis podle data
- pole `Upozornit na údržbu kilometrů předem` určuje, jak brzy má Vehimap začít hlásit blížící se servis podle tachometru
- automatické zálohy se ukládají do složky `data/auto-backups`

Pro upozornění aplikace používá pole `Příští TK`, `Zelená karta do` a u servisních úkonů intervaly uložené v `Plánu údržby`. Je to spolehlivější než automaticky dopočítávat vše jen podle druhu vozidla, protože se mohou lišit podle zvláštního režimu vozidla i servisního plánu.

V horním menu najdete tyto části:

- `Soubor`: `Tiskový přehled`, `Export dat`, `Import dat`, `Konec`
- `Vozidlo`: práce s vybraným vozidlem včetně detailu, historie, kilometrů a tankování, plánu údržby a pojištění a dokladů
- `Přehled`: `Dashboard`, `Náklady napříč vozidly`, `Globální hledání`, `Přehled termínů`, `Propadlé termíny`
- `Nástroje`: `Nastavení`, `Skrýt do lišty`
- `Nápověda`: `O programu`, `Zkontrolovat aktualizace`

## Ukládání dat

- vozidla se ukládají do souboru `data/vehicles.tsv`
- historie událostí se ukládá do souboru `data/history.tsv`
- kilometry a tankování se ukládají do souboru `data/fuel.tsv`
- pojištění a doklady se ukládají do souboru `data/records.tsv`
- plány údržby se ukládají do souboru `data/maintenance.tsv`
- nastavení upozornění a chování aplikace se ukládá do souboru `data/settings.ini`
- automatické zálohy se ukládají do složky `data/auto-backups`
- při importu se původní soubory před přepsáním automaticky odloží do `data/import-backups`
- oba soubory jsou ve složce `data` vedle aplikace
- Vehimap zapisuje vozidla ve formátu `# Vehimap data v4`
- historie používá hlavičku `# Vehimap history v1`
- kilometry a tankování používají hlavičku `# Vehimap fuel v1`
- pojištění a doklady používají hlavičku `# Vehimap records v1`
- plány údržby používají hlavičku `# Vehimap maintenance v1`
- export vytváří jeden soubor se zálohou ve formátu `.vehimapbak`

## Poznámka k oznamovací oblasti

Zavření hlavního okna aplikaci neukončí. Vehimap se schová do oznamovací oblasti a dál hlídá technické kontroly, zelené karty, vlastní připomínky i plány údržby. Kontrola běží průběžně na pozadí každých 15 minut a znovu se vyvolá i po probuzení počítače ze spánku. Stejným způsobem se na pozadí jednou za hodinu ověřuje i potřeba automatické zálohy. Pokud je vše v pořádku, tooltip tray ikony zůstává jen `Vehimap`; pokud ne, zobrazí souhrn propadlých a brzy končících `TK`, `ZK`, připomínek i servisních úkonů.


