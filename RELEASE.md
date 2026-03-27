# Release

Tento soubor je jen pro release a vyvoj. Uzivatelske informace zustavaji v `README.md`.

## Co release skript dela

Skript `build/release.ps1`:

- nacte aktualni verzi ze souboru `src/VERSION`
- zvysi verzi nebo pouzije verzi predanou parametrem
- vygeneruje `src/GeneratedBuildInfo.ahk` s embedded verzi pro runtime i EXE metadata
- aktualizuje `CHANGELOG.md` ze sekce `## [Unreleased]`
- zkompiluje `src/Vehimap.ahk` do `dist/vehimap.exe`
- vytvori asset `dist/vehimap-VERZE.zip`
- do zipu prida jen `readme.txt` a `vehimap.exe`
- nevklada do assetu zadny `.ahk`
- vytvori release commit a tag
- pokud neni pouzite `-SkipPush`, pushne `main`, pushne tag a vytvori nebo upravi GitHub release

## Pozadavky

- cisty git working tree
- dostupny `git`
- dostupny `Ahk2Exe.exe`
- `src/GeneratedBuildInfo.ahk` se pri release prepise automaticky podle `src/VERSION`
- pro publikaci release bez `-SkipPush` i dostupny `gh`

Aktualni kompilator je nastaven na:

- `C:\Users\vlcek\AppData\Local\Programs\AutoHotkey\Compiler\Ahk2Exe.exe`

## Bezny postup

Patch release bez publikace:

```powershell
.\build\release.ps1 -SkipPush
```

Vlastni verze bez publikace:

```powershell
.\build\release.ps1 -Version 1.0.1 -SkipPush
```

Minor release rovnou s publikaci:

```powershell
.\build\release.ps1 -Bump minor
```

Prerelease:

```powershell
.\build\release.ps1 -Bump minor -PrereleaseLabel beta
```

## Vystupy

Po uspesnem buildu vzniknou v `dist`:

- `vehimap.exe`
- `vehimap-VERZE.zip`

Zip obsahuje jen:

- `readme.txt`
- `vehimap.exe`

## Poznamky

- Root `README.md` ma zustat uzivatelsky a nema obsahovat release nebo vyvojarske pokyny.
- `CHANGELOG.md` ma drzet sekci `## [Unreleased]`, ze ktere se pri release vytvori nova verze s datem.
- `src/VERSION` je kanonicka semver verze aplikace pro release a update checker, zatimco EXE metadata pouzivaji odvozenou Windows file version, napr. `1.0.0` -> `1.0.0.0`.
