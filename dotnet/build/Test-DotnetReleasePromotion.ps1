param(
    [ValidateSet("beta", "stable")]
    [string]$TargetChannel = "beta",

    [string]$RuntimeIdentifier = "win-x64",

    [switch]$FailOnBlockers,

    [switch]$SkipFetch
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $dotnetRoot
$versionPath = Join-Path $repositoryRoot "src\VERSION"
$tagScript = Join-Path $PSScriptRoot "New-DotnetDesktopReleaseTag.ps1"
$readinessScript = Join-Path $PSScriptRoot "Test-DotnetReleaseReadiness.ps1"
$publishedReleaseScript = Join-Path $PSScriptRoot "Test-DotnetPublishedRelease.ps1"
$readinessWrapperScriptName = if ($TargetChannel -eq "stable") { "Test-DotnetStableReadiness.ps1" } else { "Test-DotnetBetaReadiness.ps1" }
$readinessWrapperScript = Join-Path $PSScriptRoot $readinessWrapperScriptName

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

function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    Push-Location $repositoryRoot
    try {
        $previousErrorActionPreference = $ErrorActionPreference
        try {
            $ErrorActionPreference = "Continue"
            $output = & git @Arguments 2>&1
            $exitCode = $LASTEXITCODE
        }
        finally {
            $ErrorActionPreference = $previousErrorActionPreference
        }

        if ($exitCode -ne 0) {
            $message = ($output | Out-String).Trim()
            if ([string]::IsNullOrWhiteSpace($message)) {
                $message = "git $($Arguments -join ' ') selhal s kodem $exitCode."
            }

            throw $message
        }

        return $output
    }
    finally {
        Pop-Location
    }
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

$sourceChannel = if ($TargetChannel -eq "stable") { "beta" } else { "nightly" }
$targetTag = if ($TargetChannel -eq "stable") { "dotnet-v$version" } else { "dotnet-beta-v$version" }
$sourceTag = if ($sourceChannel -eq "beta") { "dotnet-beta-v$version" } else { "dotnet-nightly" }
$sourceManifestFileName = if ($sourceChannel -eq "nightly") { "latest-dotnet-nightly-$RuntimeIdentifier.ini" } else { "latest-dotnet-beta-$RuntimeIdentifier.ini" }
$sourceManifestPath = Join-Path $repositoryRoot "update\$sourceManifestFileName"

if (-not (Test-Path -LiteralPath $tagScript -PathType Leaf)) {
    Add-Blocker "Chybi tag skript New-DotnetDesktopReleaseTag.ps1."
}
else {
    Add-Pass "Existuje tag skript pro vytvoreni $TargetChannel releasu."
}

if (-not (Test-Path -LiteralPath $readinessScript -PathType Leaf)) {
    Add-Blocker "Chybi release readiness skript Test-DotnetReleaseReadiness.ps1."
}
else {
    Add-Pass "Existuje release readiness gate pro $TargetChannel kanal."
}

if (-not (Test-Path -LiteralPath $publishedReleaseScript -PathType Leaf)) {
    Add-Blocker "Chybi post-release verifier Test-DotnetPublishedRelease.ps1."
}
else {
    Add-Pass "Existuje post-release verifier pro publikovane manifesty."
}

if (-not (Test-Path -LiteralPath $readinessWrapperScript -PathType Leaf)) {
    Add-Blocker "Chybi kanalovy readiness wrapper $readinessWrapperScriptName."
}
else {
    Add-Pass "Existuje kanalovy readiness wrapper $readinessWrapperScriptName."
}

if (-not (Test-Path -LiteralPath $sourceManifestPath -PathType Leaf)) {
    Add-Blocker "Chybi publikovany manifest zdrojoveho kanalu update\$sourceManifestFileName."
}
else {
    $sourceManifest = Read-KeyValueManifest -Path $sourceManifestPath
    $sourceVersion = if ($sourceManifest.ContainsKey("version")) { $sourceManifest["version"] } else { "" }
    $expectedAssetKind = if ($RuntimeIdentifier -like "win-*") { "installer" } else { "archive" }

    if ($sourceChannel -eq "nightly") {
        $expectedNightlyVersionRegex = "^" + [regex]::Escape($version) + "-nightly\.\d+\.\d+$"
        if ($sourceVersion -match $expectedNightlyVersionRegex) {
            Add-Pass "Zdrojovy nightly manifest ma publikovanou prerelease verzi $sourceVersion."
        }
        else {
            Add-Blocker "Zdrojovy nightly manifest nema ocekavanou prerelease verzi podle $version-nightly.<run>.<attempt>."
        }
    }
    elseif ($sourceVersion -eq $version) {
        Add-Pass "Zdrojovy beta manifest ma verzi $version."
    }
    else {
        Add-Blocker "Zdrojovy beta manifest nema ocekavanou verzi $version."
    }

    if ($sourceManifest.ContainsKey("channel") -and $sourceManifest["channel"] -eq $sourceChannel) {
        Add-Pass "Zdrojovy manifest je v kanalu $sourceChannel."
    }
    else {
        Add-Blocker "Zdrojovy manifest nema channel=$sourceChannel."
    }

    if ($sourceManifest.ContainsKey("asset_kind") -and $sourceManifest["asset_kind"] -eq $expectedAssetKind) {
        Add-Pass "Zdrojovy manifest pouziva $expectedAssetKind asset."
    }
    else {
        Add-Blocker "Zdrojovy manifest nepouziva $expectedAssetKind asset."
    }

    $expectedAssetPart = "releases/download/$sourceTag/"
    if ($sourceManifest.ContainsKey("asset_url") -and $sourceManifest["asset_url"].Contains($expectedAssetPart)) {
        Add-Pass "Zdrojovy manifest ukazuje na release asset $sourceTag."
    }
    else {
        Add-Blocker "Zdrojovy manifest neukazuje na ocekavany release asset $sourceTag."
    }

    if ($sourceManifest.ContainsKey("notes_url") -and $sourceManifest["notes_url"].EndsWith("/releases/tag/$sourceTag")) {
        Add-Pass "Zdrojovy manifest ukazuje na release notes $sourceTag."
    }
    else {
        Add-Blocker "Zdrojovy manifest nema ocekavanou notes_url pro $sourceTag."
    }

    if ($sourceManifest.ContainsKey("asset_sha256") -and $sourceManifest["asset_sha256"] -match "^[0-9a-fA-F]{64}$") {
        Add-Pass "Zdrojovy manifest obsahuje platny SHA-256 hash."
    }
    else {
        Add-Blocker "Zdrojovy manifest neobsahuje platny SHA-256 hash."
    }

    $sourceAssetSize = 0L
    if ($sourceManifest.ContainsKey("asset_size") -and [long]::TryParse($sourceManifest["asset_size"], [ref]$sourceAssetSize) -and $sourceAssetSize -gt 0) {
        Add-Pass "Zdrojovy manifest obsahuje velikost assetu."
    }
    else {
        Add-Blocker "Zdrojovy manifest neobsahuje platnou velikost assetu."
    }
}

try {
    $branch = (Invoke-Git rev-parse --abbrev-ref HEAD | Select-Object -First 1).Trim()
    if ($branch -eq "main") {
        Add-Pass "Aktualni vetev je main."
    }
    else {
        Add-Blocker "Promotion lze provest jen z vetve main. Aktualni vetev: $branch."
    }

    $status = (Invoke-Git status --porcelain | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($status)) {
        Add-Pass "Pracovni strom je cisty."
    }
    else {
        Add-Blocker "Pracovni strom neni cisty. Pred promotion commitnete nebo odlozte zmeny."
    }

    if (-not $SkipFetch) {
        Invoke-Git fetch origin main --tags | Out-Null
        Add-Pass "Origin main a tagy byly nacteny."
    }
    else {
        Add-Warning "Fetch byl preskocen; kontrola origin/main a remote tagu muze byt zastarala."
    }

    $head = (Invoke-Git rev-parse HEAD | Select-Object -First 1).Trim()
    $originMain = (Invoke-Git rev-parse origin/main | Select-Object -First 1).Trim()
    if ($head -eq $originMain) {
        Add-Pass "Lokalni main odpovida origin/main."
    }
    else {
        Add-Blocker "Lokalni main neni shodny s origin/main."
    }

    $sourceRemoteTag = (Invoke-Git ls-remote --tags origin "refs/tags/$sourceTag" | Out-String).Trim()
    if (-not [string]::IsNullOrWhiteSpace($sourceRemoteTag)) {
        Add-Pass "Zdrojovy tag $sourceTag existuje na origin."
    }
    elseif ($TargetChannel -eq "beta") {
        Add-Blocker "Pred beta releasem musi existovat rolling nightly tag $sourceTag na origin."
    }
    else {
        Add-Blocker "Pred stable releasem musi existovat beta tag $sourceTag na origin."
    }

    $targetLocalTag = (Invoke-Git tag --list $targetTag | Out-String).Trim()
    $targetRemoteTag = (Invoke-Git ls-remote --tags origin "refs/tags/$targetTag" | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($targetLocalTag) -and [string]::IsNullOrWhiteSpace($targetRemoteTag)) {
        Add-Pass "Cilovy tag $targetTag zatim neexistuje."
    }
    else {
        Add-Blocker "Cilovy tag $targetTag uz existuje lokalne nebo na origin."
    }
}
catch {
    Add-Blocker $_.Exception.Message
}

Write-Host "Vehimap .NET release promotion gate"
Write-Host "Version: $version"
Write-Host "Source channel: $sourceChannel"
Write-Host "Target channel: $TargetChannel"
Write-Host "Runtime: $RuntimeIdentifier"
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
    Write-Host "Vysledek: promotion gate je pruchozi pro $TargetChannel."
    Write-Host "Doporucena lokalni kontrola:"
    Write-Host "  powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\$readinessWrapperScriptName -RuntimeIdentifier $RuntimeIdentifier"
    Write-Host "Doporuceny prikaz:"
    Write-Host "  powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\New-DotnetDesktopReleaseTag.ps1 -RuntimeIdentifier $RuntimeIdentifier -Channel $TargetChannel -Push"
}
else {
    Write-Host "Vysledek: $TargetChannel zatim nevydavat. Nejdrive odstrante blockery vyse."
}

if ($FailOnBlockers -and $blockers.Count -gt 0) {
    exit 1
}
