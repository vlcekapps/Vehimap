param(
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$FailOnBlockers
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $dotnetRoot
$versionPath = Join-Path $repositoryRoot "src\VERSION"
$workflowPath = Join-Path $repositoryRoot ".github\workflows\dotnet-desktop.yml"
$stableManifestPath = Join-Path $repositoryRoot "update\latest-dotnet-$RuntimeIdentifier.ini"
$legacyPreviewManifestPath = Join-Path $repositoryRoot "update\latest-dotnet-preview-$RuntimeIdentifier.ini"
$releaseReadinessScript = Join-Path $PSScriptRoot "Test-DotnetReleaseReadiness.ps1"
$desktopExePath = Join-Path $dotnetRoot "artifacts\stable\$RuntimeIdentifier\app\Vehimap.Desktop.exe"

$passed = New-Object System.Collections.Generic.List[string]
$warnings = New-Object System.Collections.Generic.List[string]
$blockers = New-Object System.Collections.Generic.List[string]

function Add-Pass {
    param([string]$Message)
    $passed.Add($Message) | Out-Null
}

function Add-Warning {
    param([string]$Message)
    $warnings.Add($Message) | Out-Null
}

function Add-Blocker {
    param([string]$Message)
    $blockers.Add($Message) | Out-Null
}

function Read-KeyValueManifest {
    param([string]$Path)

    $values = @{}
    foreach ($line in (Get-Content -LiteralPath $Path)) {
        if ($line -match "^\s*([^=]+?)\s*=(.*)$") {
            $values[$matches[1].Trim()] = $matches[2].Trim()
        }
    }

    return $values
}

if (-not (Test-Path -LiteralPath $versionPath -PathType Leaf)) {
    Add-Blocker "Chybi src/VERSION; release verzi nejde urcit."
    $version = ""
}
else {
    $version = (Get-Content -LiteralPath $versionPath | Select-Object -First 1).Trim()
    if ([string]::IsNullOrWhiteSpace($version)) {
        Add-Blocker "src/VERSION je prazdny."
    }
    else {
        Add-Pass "Release verze je $version."
    }
}

if (Test-Path -LiteralPath $releaseReadinessScript -PathType Leaf) {
    Add-Pass "Existuje lokalni release gate Test-DotnetReleaseReadiness.ps1."
}
else {
    Add-Blocker "Chybi Test-DotnetReleaseReadiness.ps1; pred odstranenim AHK neni lokalni release gate."
}

if (-not (Test-Path -LiteralPath $workflowPath -PathType Leaf)) {
    Add-Blocker "Chybi .github/workflows/dotnet-desktop.yml."
}
else {
    $workflow = Get-Content -Raw -LiteralPath $workflowPath
    if ($workflow.Contains('"dotnet-v*"')) {
        Add-Pass "GitHub Actions pouziva stabilni tagy dotnet-v<verze>."
    }
    else {
        Add-Blocker "GitHub Actions nepouziva stabilni tagy dotnet-v<verze>."
    }

    if ($workflow -notmatch "--draft") {
        Add-Pass "GitHub Actions vytvari publikovany release, ne draft."
    }
    else {
        Add-Blocker "GitHub Actions stale vytvari draft release."
    }

    if ($workflow.Contains("Write-DotnetUpdateManifest.ps1")) {
        Add-Pass "GitHub Actions generuje stabilni desktop update manifesty."
    }
    else {
        Add-Blocker "GitHub Actions negeneruje stabilni desktop update manifesty."
    }

    if ($workflow.Contains("latest-dotnet-preview-")) {
        Add-Pass "Workflow drzi prechodovy preview manifest alias pro uzivatele starsich preview buildu."
    }
    else {
        Add-Warning "Workflow nema prechodovy latest-dotnet-preview alias; starsi preview buildy se nemusi dostat na stable release."
    }
}

if (-not (Test-Path -LiteralPath $stableManifestPath -PathType Leaf)) {
    Add-Blocker "Chybi stabilni manifest update\latest-dotnet-$RuntimeIdentifier.ini. Nejdrive vydejte dotnet-v$version release a nechte workflow manifest zapsat."
}
else {
    $manifest = Read-KeyValueManifest -Path $stableManifestPath
    $expectedAssetPart = "/releases/download/dotnet-v$version/vehimap-desktop-stable-$version-$RuntimeIdentifier-setup.exe"

    if ($manifest.ContainsKey("version") -and $manifest["version"] -eq $version) {
        Add-Pass "Stabilni manifest ma verzi $version."
    }
    else {
        Add-Blocker "Stabilni manifest nema ocekavanou verzi $version."
    }

    if ($manifest.ContainsKey("asset_url") -and $manifest["asset_url"].Contains($expectedAssetPart)) {
        Add-Pass "Stabilni manifest ukazuje na dotnet-v$version Inno Setup asset pro $RuntimeIdentifier."
    }
    else {
        Add-Blocker "Stabilni manifest neukazuje na ocekavany dotnet-v$version asset pro $RuntimeIdentifier."
    }

    if ($manifest.ContainsKey("asset_sha256") -and $manifest["asset_sha256"] -match "^[0-9a-fA-F]{64}$") {
        Add-Pass "Stabilni manifest obsahuje platny SHA-256 hash."
    }
    else {
        Add-Blocker "Stabilni manifest neobsahuje platny SHA-256 hash."
    }

    if ($manifest.ContainsKey("asset_kind") -and $manifest["asset_kind"] -eq "installer") {
        Add-Pass "Stabilni Windows manifest pouziva installer asset."
    }
    else {
        Add-Blocker "Stabilni Windows manifest nepouziva installer asset."
    }

    if ($manifest.ContainsKey("channel") -and $manifest["channel"] -eq "stable") {
        Add-Pass "Stabilni manifest je ve stable kanalu."
    }
    else {
        Add-Blocker "Stabilni manifest nema channel=stable."
    }

    $assetSize = 0L
    if ($manifest.ContainsKey("asset_size") -and [long]::TryParse($manifest["asset_size"], [ref]$assetSize) -and $assetSize -gt 0) {
        Add-Pass "Stabilni manifest obsahuje velikost assetu."
    }
    else {
        Add-Blocker "Stabilni manifest neobsahuje platnou velikost assetu."
    }

    if ((Get-Content -Raw -LiteralPath $stableManifestPath) -notmatch "preview") {
        Add-Pass "Stabilni manifest neobsahuje preview oznaceni."
    }
    else {
        Add-Blocker "Stabilni manifest stale obsahuje preview oznaceni."
    }
}

if (-not (Test-Path -LiteralPath $legacyPreviewManifestPath -PathType Leaf)) {
    Add-Warning "Chybi prechodovy preview manifest update\latest-dotnet-preview-$RuntimeIdentifier.ini."
}
elseif (Test-Path -LiteralPath $stableManifestPath -PathType Leaf) {
    $stableContent = Get-Content -Raw -LiteralPath $stableManifestPath
    $legacyContent = Get-Content -Raw -LiteralPath $legacyPreviewManifestPath
    if ($stableContent -eq $legacyContent) {
        Add-Pass "Prechodovy preview manifest odpovida stabilnimu manifestu."
    }
    else {
        Add-Blocker "Prechodovy preview manifest neodpovida stabilnimu manifestu; preview uzivatele by nedostali stable update."
    }
}
else {
    Add-Warning "Prechodovy preview manifest existuje, ale stabilni manifest jeste ne; stav se vyhodnoti po prvnim stable release."
}

if (Test-Path -LiteralPath $desktopExePath -PathType Leaf) {
    Add-Pass "Lokalni stable desktop build existuje: $desktopExePath."
}
else {
    Add-Warning "Lokalni stable desktop build zatim neexistuje. Spustte Test-DotnetReleaseReadiness.ps1 -RuntimeIdentifier $RuntimeIdentifier -Channel stable."
}

foreach ($relativePath in @("src\Vehimap.ahk", "src\lib", "src\tests")) {
    $path = Join-Path $repositoryRoot $relativePath
    if (Test-Path -LiteralPath $path) {
        Add-Warning "AHK artefakt stale existuje: $relativePath. To je v poradku pred finalnim retirement commitem, ale ne po nem."
    }
}

Write-Host "Vehimap AHK retirement readiness"
Write-Host "Runtime: $RuntimeIdentifier"
Write-Host "Version: $version"
Write-Host ""

Write-Host "Splneno:"
foreach ($item in $passed) {
    Write-Host "  [OK] $item"
}

if ($warnings.Count -gt 0) {
    Write-Host ""
    Write-Host "Upozorneni:"
    foreach ($item in $warnings) {
        Write-Host "  [WARN] $item"
    }
}

if ($blockers.Count -gt 0) {
    Write-Host ""
    Write-Host "Blockery:"
    foreach ($item in $blockers) {
        Write-Host "  [BLOCK] $item"
    }
}

Write-Host ""
if ($blockers.Count -eq 0) {
    Write-Host "Vysledek: AHK retirement gate je pruchozi pro $RuntimeIdentifier."
}
else {
    Write-Host "Vysledek: AHK zatim nemazat. Nejdrive odstrante blockery vyse."
}

if ($FailOnBlockers -and $blockers.Count -gt 0) {
    exit 1
}
