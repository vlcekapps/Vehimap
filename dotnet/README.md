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
- vyexportovat budouci terminy do `.ics` primo z nove C# vetve

## Lokalni build

```powershell
cd dotnet
dotnet restore
dotnet test
dotnet build
dotnet publish .\src\Vehimap.Desktop\Vehimap.Desktop.csproj -c Release -o .\artifacts\desktop-preview
```
