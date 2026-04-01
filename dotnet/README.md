# Vehimap .NET rewrite

Tato složka obsahuje novou C# codebase pro multiplatformní desktopový Vehimap.

Aktuální záměr:

- zachovat současný AHK Vehimap funkční beze změn
- vedle něj vybudovat kompatibilní `.NET + Avalonia` aplikaci
- první priorita je přímé čtení dnešních `TSV`, `INI`, `.vehimapbak` a `data/attachments`

## Struktura

- `src/Vehimap.Domain` - čisté doménové modely
- `src/Vehimap.Application` - use cases a interní rozhraní
- `src/Vehimap.Storage.Legacy` - kompatibilita se současnými AHK daty
- `src/Vehimap.Platform` - platform-specific adaptéry
- `src/Vehimap.Desktop` - Avalonia desktop shell
- `src/Vehimap.Updater` - separátní helper pro update
- `tests/*` - unit, compat a UI testy

## Aktuální stav

Tato větev už není jen scaffold. Aktuálně umí:

- postavit celou solution přes `dotnet build`
- spustit unit a kompatibilitní testy přes `dotnet test`
- vygenerovat desktopový preview build přes `dotnet publish`
- přímo číst a zapisovat dnešní Vehimap data (`TSV`, `INI`, `.vehimapbak`, managed attachments)
- skládat první sdílené C# use-cases pro audit a nákladové souhrny
- zobrazit v Avalonia shellu seznam vozidel, auditní frontu a souhrn nákladů

## Lokální build

```powershell
cd dotnet
dotnet restore
dotnet test
dotnet build
dotnet publish .\src\Vehimap.Desktop\Vehimap.Desktop.csproj -c Release -o .\artifacts\desktop-preview
```
