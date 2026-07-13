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

Vydané desktopové balíčky jsou `self-contained`: obsahují potřebný .NET runtime. Běžný uživatel proto neinstaluje .NET SDK, .NET Runtime, Avalonia ani PowerShell.

### Windows

- Windows 10 22H2 x64 nebo Windows 11 x64
- běžný uživatelský účet; administrátorská práva nejsou pro výchozí per-user instalaci potřeba
- Windows je hlavní podporovaná a přístupnostně ověřovaná platforma

### macOS

- macOS 14 Sonoma nebo novější
- balíček `osx-arm64` pro Apple Silicon, například M1 až M4, nebo `osx-x64` pro Intel Mac
- žádná samostatná instalace .NET není potřeba

macOS balíčky zatím procházejí sestavením a strukturálním smoke testem, ale čekají na plné nativní ověření, Apple podpis a notarizaci. Do té doby je považujte za testovací balíčky; macOS může jejich první spuštění vyžadovat ručně povolit v nastavení Soukromí a zabezpečení.

### Linux

- 64bitový x86 systém (`linux-x64`), `glibc` 2.17 nebo novější
- grafická relace X11 nebo XWayland; nativní Wayland backend zatím není podporovanou cestou Vehimapu
- Ubuntu 25.x, Fedora 43 a Debian 13 jsou v Avalonia 12 Tier 1; starší podporované řady jsou Tier 2 a Arch Linux je Tier 3

Na Ubuntu nebo Debianu nainstalujte desktopové knihovny:

```bash
sudo apt update
sudo apt install libx11-6 libice6 libsm6 libfontconfig1 xdg-utils
```

Na Fedoře:

```bash
sudo dnf install libX11 libICE libSM fontconfig xdg-utils
```

Na Arch Linuxu:

```bash
sudo pacman -S --needed libx11 libice libsm fontconfig xdg-utils
```

Na čistě Wayland instalaci může být navíc potřeba XWayland: balíček `xwayland` na Ubuntu/Debianu, `xorg-x11-server-Xwayland` na Fedoře nebo `xorg-xwayland` na Archu. Minimální instalace distribuce musí také obsahovat běžné nativní závislosti .NET, zejména ICU, OpenSSL, `libstdc++`, zlib, Kerberos, certifikáty a časová data. Běžné plné desktopové instalace je zpravidla už obsahují.

Aktuální platformní úrovně a nativní knihovny vycházejí z [oficiální dokumentace Avalonia Supported Platforms](https://docs.avaloniaui.net/docs/supported-platforms). Požadavky minimálních linuxových instalací na ICU, OpenSSL a další základní knihovny udržuje [.NET Linux installation documentation](https://learn.microsoft.com/dotnet/core/install/linux).

### Android

- Android 12 (API 31) nebo novější
- telefon nebo tablet s ARM64; lokální vývojový APK obsahuje také x86-64 knihovny pro emulátor
- žádná samostatná instalace .NET ani Avalonie není potřeba

Android je nyní experimentální lokální nightly pro vývoj a testování. Nabízí zatím pouze čtení seznamu a detailu vozidel ve vlastní datové sadě aplikace; veřejný podepsaný Android balíček ještě nevydáváme.

## Instalace

### Windows

1. Otevřete stránku [Releases](https://github.com/vlcekapps/Vehimap/releases).
2. Stáhněte instalační soubor `win-x64-setup.exe`.
3. Spusťte instalátor a dokončete instalaci.
4. Vehimap otevřete ze Start menu nebo ze zástupce na ploše.

### macOS

1. Stáhněte ZIP pro `osx-arm64` nebo `osx-x64` podle procesoru.
2. Rozbalte `Vehimap.app` a přesuňte jej do složky Aplikace.
3. Pokud Gatekeeper testovací balíček zablokuje, povolte jeho spuštění v nastavení Soukromí a zabezpečení.

### Linux

1. Nainstalujte výše uvedené systémové knihovny a stáhněte archiv `linux-x64.tar.gz`.
2. Archiv rozbalte, otevřete jeho složku a spusťte soubor `Vehimap`.
3. Pokud archiv nezachoval spustitelný příznak, použijte `chmod +x Vehimap` a poté `./Vehimap`.

### Android

Android APK se v této fázi instaluje pouze lokálně přes vývojářské nástroje a USB ladění. Běžní uživatelé mají počkat na první veřejně podepsané mobilní vydání; podrobný postup pro vývojáře je v [dotnet/docs/DEVELOPMENT.md](dotnet/docs/DEVELOPMENT.md).

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

Pro Vehimap 2.0 vzniká také ACR-ready evidenční draft pro budoucí zákaznické posouzení přístupnosti. Nejde zatím o formální prohlášení o shodě; to bude možné až po dokončení ručních testů s asistivními technologiemi.

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
- [Požadavky na vývojové prostředí](dotnet/docs/DEVELOPMENT.md)
- [Jak přispívat](CONTRIBUTING.md)
- [Migrační plán](dotnet/docs/MIGRATION.md)
- [Přístupnost](dotnet/docs/ACCESSIBILITY.md)
- [Lokalizace](dotnet/docs/I18N.md)
- [Release proces](RELEASE.md)
