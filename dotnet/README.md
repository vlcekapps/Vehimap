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

## Lokální build

Na tomto stroji zatím není nainstalovaný `.NET SDK`, pouze runtime. Řešení je proto připravené ručně, ale lokální `dotnet restore/build/test` bude fungovat až po doinstalování SDK 10.

Po instalaci SDK:

```powershell
cd dotnet
dotnet restore
dotnet test
dotnet build
```
