# Licenční balíček pro Vehimap

Doporučená hlavní licence projektu: **GNU GPL v3 nebo novější** (`GPL-3.0-or-later`).

Balíček je připravený jako praktický „drop-in“ pro repozitář `github.com/vlcekapps/Vehimap` podle NuGet závislostí nalezených v projektových souborech a podle aktuálního výstupu `dotnet list dotnet/Vehimap.sln package --include-transitive` k datu **2026-07-01**.

## Co zkopírovat do repozitáře

1. `LICENSE` — oficiální text GNU GPL v3. Umístěte do kořene repozitáře.
2. `THIRD-PARTY-NOTICES.md` — přehled runtime, transitive a native third-party závislostí přibalovaných do běžného desktop releasu. Umístěte do kořene repozitáře a přibalujte k release balíčkům.
3. `snippets/README-license-snippet.md` — krátký text do `README.md`.
4. `snippets/SOURCE-FILE-HEADER.cs.txt` — volitelná hlavička do nových `.cs` souborů.
5. `LICENSES/` — texty licencí třetích stran, které můžete přibalit k release balíčkům.

## Důležité k „or later“

Text GPL v souboru `LICENSE` je text GNU GPL verze 3. Varianta **„or later“** se určuje v licenčním oznámení projektu, například větou:

> either version 3 of the License, or (at your option) any later version.

Neměňte text samotné GPL licence. Upravte jen licenční oznámení projektu, copyright holdera a případné hlavičky souborů.

## Copyright holder

Copyright holder tohoto repozitáře:

`Pavel Vlček`

Pokud by se vlastnictví práv v budoucnu změnilo, je potřeba upravit `COPYRIGHT-NOTICE.txt`, dokumentaci a zdrojové hlavičky/metadata. Samotný text GPL licence se nemění.

## Runtime, transitivní a vývojové závislosti

`THIRD-PARTY-NOTICES.md` obsahuje runtime, transitive a native závislosti relevantní pro běžný desktop release. Testovací a vývojové balíčky jsou oddělené v `docs/licensing/development-dependencies.md`, protože nejsou součástí běžného uživatelského releasu.

Před vydáním binárního releasu doporučujeme v čistém checkoutu spustit:

```bash
dotnet restore dotnet/Vehimap.sln
dotnet list dotnet/Vehimap.sln package --include-transitive
```

Poté aktualizujte notice soubory podle skutečně obnovených balíčků a podle cílového publish výstupu. U self-contained buildů se mohou přibalit také runtime/native komponenty, jejichž seznam závisí na cílovém RID a publish nastavení. Aktuální výstup příkazu je uložen v `docs/licensing/dotnet-list-package-include-transitive.txt` a ručně shrnutý runtime seznam pro Windows x64 je v `docs/licensing/metadata/runtime-nuget-dependencies-win-x64.csv`.

## SPDX

Doporučený SPDX identifikátor projektu:

```text
SPDX-License-Identifier: GPL-3.0-or-later
```

## Upozornění

Toto je prakticky připravený licenční balíček, ne právní audit. U release artefaktů vždy ověřte aktuální seznam závislostí, assetů, ikon, fontů a dalších přibalených souborů.
