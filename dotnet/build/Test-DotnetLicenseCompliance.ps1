# SPDX-License-Identifier: GPL-3.0-or-later
param(
    [string]$PublishDirectory = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$spdx = "SPDX-License-Identifier: GPL-3.0-or-later"

function Add-Failure {
    param(
        [System.Collections.Generic.List[string]]$Failures,
        [string]$Message
    )

    $Failures.Add($Message) | Out-Null
}

function Test-RequiredFiles {
    param([System.Collections.Generic.List[string]]$Failures)

    $requiredFiles = @(
        "LICENSE",
        "COPYING",
        "COPYRIGHT-NOTICE.txt",
        "THIRD-PARTY-NOTICES.md",
        "LICENSES\GPL-3.0.txt",
        "LICENSES\MIT.txt",
        "LICENSES\Apache-2.0.txt",
        "LICENSES\ANGLE-BSD-3-Clause.txt",
        "LICENSES\SQLite-Public-Domain.txt",
        "docs\licensing\README-LICENSING.md",
        "docs\licensing\RELEASE-PACKAGING-CHECKLIST.md",
        "docs\licensing\development-dependencies.md",
        "docs\licensing\dotnet-list-package-include-transitive.txt",
        "docs\licensing\metadata\direct-nuget-dependencies.csv",
        "docs\licensing\metadata\license-metadata.json",
        "docs\licensing\metadata\runtime-nuget-dependencies-win-x64.csv"
    )

    foreach ($relativePath in $requiredFiles) {
        if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $relativePath) -PathType Leaf)) {
            Add-Failure $Failures "Chybi povinny licencni soubor: $relativePath"
        }
    }
}

function Test-LicenseTexts {
    param([System.Collections.Generic.List[string]]$Failures)

    $license = Get-Content -Raw -LiteralPath (Join-Path $repoRoot "LICENSE")
    if ($license -notmatch "GNU GENERAL PUBLIC LICENSE" -or $license -notmatch "Version 3") {
        Add-Failure $Failures "Root LICENSE nevypada jako text GNU GPL v3."
    }

    $copyright = Get-Content -Raw -LiteralPath (Join-Path $repoRoot "COPYRIGHT-NOTICE.txt")
    foreach ($expected in @("Pavel Vlček", "GPL-3.0-or-later", "either version 3 of the License, or \(at your option\) any later\s+version")) {
        if ($copyright -notmatch $expected) {
            Add-Failure $Failures "COPYRIGHT-NOTICE.txt neobsahuje: $expected"
        }
    }

    foreach ($relativePath in @("README.md", "dotnet\README.md", "RELEASE.md")) {
        $content = Get-Content -Raw -LiteralPath (Join-Path $repoRoot $relativePath)
        if ($content -notmatch "GPL-3.0-or-later") {
            Add-Failure $Failures "$relativePath neobsahuje GPL-3.0-or-later."
        }

        if ($content -notmatch "THIRD-PARTY-NOTICES.md") {
            Add-Failure $Failures "$relativePath neobsahuje odkaz na THIRD-PARTY-NOTICES.md."
        }
    }
}

function Test-ThirdPartyNotices {
    param([System.Collections.Generic.List[string]]$Failures)

    $notices = Get-Content -Raw -LiteralPath (Join-Path $repoRoot "THIRD-PARTY-NOTICES.md")
    $requiredNoticeTokens = @(
        ".NET runtime",
        "Avalonia.Angle.Windows.Natives",
        "CommunityToolkit.Mvvm",
        "Microsoft.Data.Sqlite.Core",
        "Microsoft.Win32.SystemEvents",
        "HarfBuzzSharp.NativeAssets.Win32",
        "SkiaSharp.NativeAssets.Win32",
        "SourceGear.sqlite3",
        "SQLitePCLRaw.provider.e_sqlite3",
        "Tmds.DBus.Protocol"
    )

    foreach ($token in $requiredNoticeTokens) {
        if ($notices -notmatch [regex]::Escape($token)) {
            Add-Failure $Failures "THIRD-PARTY-NOTICES.md neobsahuje runtime zavislost: $token"
        }
    }
}

function Test-SpdxHeaders {
    param([System.Collections.Generic.List[string]]$Failures)

    $trackedFiles = git -C $repoRoot ls-files --cached --others --exclude-standard
    if ($LASTEXITCODE -ne 0) {
        throw "Nelze nacist tracked soubory pres git ls-files."
    }

    $sourcePattern = '\.(cs|csproj|props|targets|axaml|resx|ps1|ya?ml|iss\.in|manifest)$'
    foreach ($relativePath in $trackedFiles) {
        if ($relativePath -notmatch $sourcePattern) {
            continue
        }

        if ($relativePath -match '(^|/)dotnet/artifacts/' -or $relativePath -match '(^|/)bin/' -or $relativePath -match '(^|/)obj/') {
            continue
        }

        $fullPath = Join-Path $repoRoot ($relativePath -replace '/', [System.IO.Path]::DirectorySeparatorChar)
        $firstLines = (Get-Content -LiteralPath $fullPath -TotalCount 5) -join "`n"
        if ($firstLines -notmatch [regex]::Escape($spdx)) {
            Add-Failure $Failures "Soubor nema SPDX hlavicku: $relativePath"
        }
    }
}

function Test-PublishPayload {
    param(
        [System.Collections.Generic.List[string]]$Failures,
        [string]$Directory
    )

    if ([string]::IsNullOrWhiteSpace($Directory)) {
        return
    }

    if (-not (Test-Path -LiteralPath $Directory -PathType Container)) {
        Add-Failure $Failures "Publish slozka neexistuje: $Directory"
        return
    }

    $requiredFiles = @(
        "LICENSE",
        "COPYING",
        "COPYRIGHT-NOTICE.txt",
        "THIRD-PARTY-NOTICES.md",
        "LICENSES\GPL-3.0.txt",
        "LICENSES\MIT.txt",
        "LICENSES\Apache-2.0.txt",
        "LICENSES\ANGLE-BSD-3-Clause.txt",
        "LICENSES\SQLite-Public-Domain.txt"
    )

    foreach ($relativePath in $requiredFiles) {
        if (-not (Test-Path -LiteralPath (Join-Path $Directory $relativePath) -PathType Leaf)) {
            Add-Failure $Failures "Publish payload neobsahuje: $relativePath"
        }
    }
}

$failures = [System.Collections.Generic.List[string]]::new()
Test-RequiredFiles -Failures $failures
Test-LicenseTexts -Failures $failures
Test-ThirdPartyNotices -Failures $failures
Test-SpdxHeaders -Failures $failures
Test-PublishPayload -Failures $failures -Directory $PublishDirectory

if ($failures.Count -gt 0) {
    $message = "License compliance gate failed:" + [Environment]::NewLine + ($failures | ForEach-Object { "- $_" } | Out-String)
    throw $message
}

Write-Host "Vehimap license compliance gate OK"
