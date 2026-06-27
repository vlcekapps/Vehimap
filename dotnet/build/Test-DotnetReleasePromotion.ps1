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
        $output = & git @Arguments 2>&1
        $exitCode = $LASTEXITCODE
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
        Add-Warning "Rolling nightly tag $sourceTag zatim na origin neexistuje. Beta lze stale vytvorit po lokalni readiness gate, ale nebude formalne povysena z publikovane nightly."
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
    Write-Host "Doporuceny prikaz:"
    Write-Host "  powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\New-DotnetDesktopReleaseTag.ps1 -RuntimeIdentifier $RuntimeIdentifier -Channel $TargetChannel -Push"
}
else {
    Write-Host "Vysledek: $TargetChannel zatim nevydavat. Nejdrive odstrante blockery vyse."
}

if ($FailOnBlockers -and $blockers.Count -gt 0) {
    exit 1
}
