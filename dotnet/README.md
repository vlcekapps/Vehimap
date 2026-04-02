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
- vyhledavat napric vozidly, historii, tankovanim, doklady, pripominkami a planem udrzby v nove karte `Hledani`
- vyexportovat budouci terminy do `.ics` primo z nove C# vetve
- otevrit z casove osy souvisejici historii, doklad, pripominku nebo servisni plan primo na odpovidajici karte shellu
- pouzit prvni dashboard nad auditem, naklady a nejblizsimi terminy napric vozidly
- ovladat shell vice klavesnici: `Ctrl+R` pro znovunacteni, `Ctrl+E` pro export kalendare, `Ctrl+F` pro fokus do hledani v casove ose, `Ctrl+Shift+F` pro globalni hledani a `Enter` pro otevreni vybranych polozek v casove ose, auditu, nakladech, dashboardu i ve vysledcich hledani
- primo vytvaret, upravovat a mazat `pripominky`
- primo vytvaret, upravovat a mazat `doklady`, vcetne volby `Spravovana kopie` vs `Externi cesta` a importu souboru do spravovanych priloh
- primo vytvaret, upravovat a mazat `historii`
- primo vytvaret, upravovat a mazat `tankovani`
- primo vytvaret, upravovat a mazat `plan udrzby`
- primo vytvaret a upravovat `vozidla`, vcetne zakladniho servisniho profilu a stavu vozidla
- drzet keyboard-first a11y i v hlavnim shellu, vcetne vlastni focusovatelne listy karet a konzistentniho focusu pro ctecky obrazovky
- otevrit modalni `Nastaveni`, `O programu` a `Zkontrolovat aktualizace` primo z desktop shellu
- cist a zapisovat podporovane reminder volby do stejneho `settings.ini` jako AHK verze a respektovat `show_dashboard_on_launch`
- reportovat stejnou verzi jako root `src/VERSION`, vcetne file version pro desktop buildy
- kontrolovat `update/latest.ini` kompatibilne s AHK vetvi a na Windows pripravit automatickou instalaci pres `Vehimap.Updater`
- otevrit modalni export a obnovu dat a pracovat se stejnym `.vehimapbak` formatem jako AHK vetev

## Lokalni build

```powershell
cd dotnet
dotnet restore
dotnet test
dotnet build
dotnet publish .\src\Vehimap.Desktop\Vehimap.Desktop.csproj -c Release -o .\artifacts\desktop-preview
```
