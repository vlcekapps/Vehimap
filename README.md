# Vehimap

Vehimap je aplikace pro přehlednou evidenci vozidel. Pomáhá hlídat technickou kontrolu, zelenou kartu, servis, doklady, tankování, náklady, připomínky a další důležité věci kolem auta, autobusu, motorky nebo jiného vozidla.

Aplikace vzniká i s důrazem na přístupnost pro handicapované uživatele. Dá se ovládat klávesnicí a průběžně ji testujeme se čtečkami obrazovky, zejména s NVDA na Windows.

## Obsah

- [English version](README.en-US.md)
- [Pro koho je Vehimap](#pro-koho-je-vehimap)
- [Systémové požadavky](#systémové-požadavky)
- [Instalace](#instalace)
- [Rychlý start](#rychlý-start)
- [Nejdůležitější funkce](#nejdůležitější-funkce)
- [Data a soukromí](#data-a-soukromí)
- [Licence](#licence)
- [Podpora a zpětná vazba](#podpora-a-zpětná-vazba)
- [Dokumentace pro vývojáře](#dokumentace-pro-vývojáře)

## Pro Koho Je Vehimap

Vehimap se hodí, pokud chcete mít na jednom místě:

- seznam vlastních vozidel,
- termíny technické kontroly a pojištění,
- servisní historii a plán údržby,
- doklady a přílohy,
- tankování, spotřebu a náklady,
- připomínky a audit chybějících nebo podezřelých údajů.

Data jsou uložená lokálně ve vašem počítači. Vehimap není cloudová služba a bez vašeho rozhodnutí nikam neposílá evidenci vozidel.

## Systémové Požadavky

Aktuální veřejně podporovaná verze je desktopová aplikace pro Windows.

- Windows 10 nebo Windows 11
- 64bitový systém
- běžný uživatelský účet, administrátorská práva nejsou pro standardní instalaci potřeba

Další platformy jsou součástí dlouhodobého plánu, ale běžným uživatelům zatím doporučujeme Windows verzi.

## Instalace

1. Otevřete stránku [Releases](https://github.com/vlcekapps/Vehimap/releases).
2. Stáhněte instalační soubor pro Windows.
3. Spusťte instalátor a dokončete instalaci.
4. Vehimap otevřete ze Start menu nebo ze zástupce na ploše.

Pro běžné používání vybírejte stabilní vydání. Nightly verze jsou určené pro odvážnější testery a mohou obsahovat rozpracované změny.

## Rychlý Start

1. Po spuštění zvolte `Vozidlo` -> `Přidat vozidlo`.
2. Vyplňte název, kategorii, SPZ a další údaje, které chcete evidovat.
3. Doplněním příští technické kontroly a konce zelené karty získáte základní hlídání termínů.
4. V kartách vozidla postupně přidávejte historii, tankování, doklady, připomínky a údržbu.
5. V přehledech používejte dashboard, audit dat a chytrého poradce, které upozorní na důležité nebo chybějící informace.
6. Pravidelně používejte zálohu dat, zejména před většími změnami nebo aktualizacemi.

## Nejdůležitější Funkce

### Evidence Vozidel

Vehimap umí vést více vozidel najednou. U každého vozidla eviduje základní údaje, stav, poznámky, termíny, historii a související záznamy.

### Připomínky A Termíny

Aplikace hlídá důležité termíny, například technickou kontrolu, zelenou kartu, připomínky a servisní údržbu. Termíny lze zobrazit v přehledech a exportovat do kalendáře.

### Doklady A Přílohy

K vozidlu můžete připojit doklady a soubory. Vehimap podporuje externí cesty i spravované přílohy uložené přímo v datové složce aplikace.

### Servis A Údržba

K dispozici je plán údržby, servisní historie a servisní knížka. Užitečné je to pro běžný provoz, veterány i firemní nebo pracovní vozidla.

### Tankování A Náklady

Vehimap eviduje tankování, místo tankování, detail paliva, cenu a tachometr. Umí dopočítat spotřebu, cenu za litr a upozornit na podezřelé záznamy.

### Audit Dat A Chytrý Poradce

Audit dat hledá chybějící nebo podezřelé údaje. Chytrý poradce z existujících dat sestaví doporučení, čemu se věnovat jako první.

### Zálohování A Obnova

Data lze exportovat do zálohy a později obnovit. Novější verze Vehimapu používají lokální databázi a starší data umí při přechodu bezpečně převést.

### Přístupnost

Vehimap je navržený jako keyboard-first aplikace. Důležité obrazovky mají klávesové ovládání, popsané prvky pro čtečky obrazovky a samostatné dialogy pro editaci záznamů.

## Data A Soukromí

Vehimap ukládá data lokálně do datové složky vybraného instalačního kanálu. Aplikace nepoužívá cloudovou synchronizaci a evidence vozidel zůstává na vašem zařízení.

Při přechodu ze starší verze se původní data automaticky zálohují a převedou do nové datové sady. Původní soubory se po ověřené migraci odloží do migrační zálohy, aby běžná práce pokračovala už nad novým formátem.

## Licence

Vehimap je svobodný software pod licencí `GPL-3.0-or-later`.

Copyright: Pavel Vlček

Součástí vydání jsou také informace o použitých knihovnách v souboru `THIRD-PARTY-NOTICES.md`.

## Podpora A Zpětná Vazba

Chyby, návrhy a připomínky můžete hlásit přes [GitHub Issues](https://github.com/vlcekapps/Vehimap/issues).

Pokud chcete autorovi poděkovat, v aplikaci je položka `Poděkovat autorovi`, která otevře stránku s dobrovolnou podporou.

## Dokumentace Pro Vývojáře

Tento soubor je určený běžným uživatelům. Technické informace pro vývoj, build, testy, migraci dat, přístupnost a lokalizaci jsou v samostatné dokumentaci:

- [Vývojářské README](dotnet/README.md)
- [Migrační plán](dotnet/docs/MIGRATION.md)
- [Přístupnost](dotnet/docs/ACCESSIBILITY.md)
- [Lokalizace](dotnet/docs/I18N.md)
- [Release proces](RELEASE.md)
