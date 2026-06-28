param(
    [string]$RuntimeIdentifier = "win-x64",
    [ValidateSet("stable", "beta", "nightly")]
    [string]$Channel = "stable",
    [string]$RepositoryFullName = "vlcekapps/Vehimap",
    [switch]$SkipNetwork,
    [switch]$SkipRetirementGate
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $dotnetRoot
$versionPath = Join-Path $repositoryRoot "src\VERSION"
$channelName = $Channel.Trim().ToLowerInvariant()
$manifestPath = if ($channelName -eq "stable") {
    Join-Path $repositoryRoot "update\latest-dotnet-$RuntimeIdentifier.ini"
}
else {
    Join-Path $repositoryRoot "update\latest-dotnet-$channelName-$RuntimeIdentifier.ini"
}
$legacyPreviewManifestPath = Join-Path $repositoryRoot "update\latest-dotnet-preview-$RuntimeIdentifier.ini"
$retirementReadinessScript = Join-Path $PSScriptRoot "Get-AhkRetirementReadiness.ps1"

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

function Get-ChannelDisplayName {
    param([string]$Channel)

    switch ($Channel) {
        "beta" { return "Beta" }
        "nightly" { return "Nightly" }
        default { return "Stabilni" }
    }
}

function Get-ReleaseTag {
    param(
        [string]$Channel,
        [string]$Version
    )

    switch ($Channel) {
        "beta" { return "dotnet-beta-v$Version" }
        "nightly" { return "dotnet-nightly" }
        default { return "dotnet-v$Version" }
    }
}

function Get-ExpectedPackageFileName {
    param(
        [string]$Channel,
        [string]$Version,
        [string]$RuntimeIdentifier
    )

    if ($RuntimeIdentifier -like "win-*") {
        return "vehimap-desktop-$Channel-$Version-$RuntimeIdentifier-setup.exe"
    }

    if ($RuntimeIdentifier -like "linux-*") {
        return "vehimap-desktop-$Version-$RuntimeIdentifier.tar.gz"
    }

    return "vehimap-desktop-$Version-$RuntimeIdentifier.zip"
}

function Invoke-RemoteHeadCheck {
    param(
        [string]$Name,
        [string]$Url,
        [nullable[long]]$ExpectedSize
    )

    $expectedSizeValue = 0L
    $hasExpectedSize = $false
    if ($null -ne $ExpectedSize) {
        $expectedSizeValue = [long]$ExpectedSize
        $hasExpectedSize = $expectedSizeValue -gt 0
    }

    if ([string]::IsNullOrWhiteSpace($Url)) {
        Add-Blocker "$Name nema URL."
        return
    }

    try {
        $response = Invoke-WebRequest -Uri $Url -Method Head -MaximumRedirection 5 -TimeoutSec 30 -UseBasicParsing
        if ($response.StatusCode -lt 200 -or $response.StatusCode -ge 400) {
            Add-Blocker "$Name neni dostupny. HTTP stav: $($response.StatusCode)."
            return
        }

        Add-Pass "$Name je dostupny pres HTTP HEAD."

        $contentLengthHeader = $response.Headers["Content-Length"]
        if ($null -ne $contentLengthHeader -and -not [string]::IsNullOrWhiteSpace([string]$contentLengthHeader)) {
            $contentLength = 0L
            if ([long]::TryParse([string]$contentLengthHeader, [ref]$contentLength)) {
                if ($hasExpectedSize -and $contentLength -ne $expectedSizeValue) {
                    Add-Blocker "$Name ma jinou vzdalenou velikost ($contentLength) nez manifest ($expectedSizeValue)."
                }
                else {
                    Add-Pass "$Name vzdalenou velikosti odpovida manifestu."
                }
            }
        }
        else {
            Add-Warning "$Name neposkytl Content-Length; dostupnost je overena, velikost zustava podle manifestu."
        }
    }
    catch {
        Add-Blocker "$Name se nepodarilo overit: $($_.Exception.Message)"
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

$channelDisplayName = Get-ChannelDisplayName -Channel $channelName
$releaseTag = Get-ReleaseTag -Channel $channelName -Version $version
$manifestAssetSize = $null
$assetUrl = ""
$notesUrl = ""
$manifestVersion = ""
$expectedPackageFileName = ""

if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    Add-Blocker "Chybi $channelName manifest '$manifestPath'. Nejdrive pockejte na dobeh release workflow."
}
else {
    $manifest = Read-KeyValueManifest -Path $manifestPath

    if ($manifest.ContainsKey("version")) {
        $manifestVersion = $manifest["version"]
    }

    if ($channelName -eq "nightly") {
        $expectedVersionRegex = "^" + [regex]::Escape($version) + "-nightly\.\d+\.\d+$"
        if ($manifestVersion -match $expectedVersionRegex) {
            Add-Pass "Nightly manifest ma publikovanou prerelease verzi $manifestVersion."
        }
        else {
            Add-Blocker "Nightly manifest nema ocekavanou prerelease verzi podle $version-nightly.<run>.<attempt>."
        }
    }
    elseif ($manifestVersion -eq $version) {
        Add-Pass "$channelDisplayName manifest ma verzi $version."
    }
    else {
        Add-Blocker "$channelDisplayName manifest nema ocekavanou verzi $version."
    }

    $effectiveVersion = if ([string]::IsNullOrWhiteSpace($manifestVersion)) { $version } else { $manifestVersion }
    $expectedPackageFileName = Get-ExpectedPackageFileName -Channel $channelName -Version $effectiveVersion -RuntimeIdentifier $RuntimeIdentifier

    if ($manifest.ContainsKey("asset_url")) {
        $assetUrl = $manifest["asset_url"]
    }

    $expectedAssetPart = "/$RepositoryFullName/releases/download/$releaseTag/$expectedPackageFileName"
    if (-not [string]::IsNullOrWhiteSpace($assetUrl) -and $assetUrl.Contains($expectedAssetPart)) {
        Add-Pass "$channelDisplayName manifest ukazuje na release asset $releaseTag pro $RuntimeIdentifier."
    }
    else {
        Add-Blocker "$channelDisplayName manifest neukazuje na ocekavany release asset $releaseTag pro $RuntimeIdentifier."
    }

    if ($manifest.ContainsKey("notes_url")) {
        $notesUrl = $manifest["notes_url"]
    }

    $expectedNotesUrl = "https://github.com/$RepositoryFullName/releases/tag/$releaseTag"
    if ($notesUrl -eq $expectedNotesUrl) {
        Add-Pass "$channelDisplayName manifest ukazuje na release notes pro $releaseTag."
    }
    else {
        Add-Blocker "$channelDisplayName manifest nema ocekavanou notes_url pro $releaseTag."
    }

    if ($manifest.ContainsKey("asset_sha256") -and $manifest["asset_sha256"] -match "^[0-9a-fA-F]{64}$") {
        Add-Pass "$channelDisplayName manifest obsahuje platny SHA-256 hash."
    }
    else {
        Add-Blocker "$channelDisplayName manifest neobsahuje platny SHA-256 hash."
    }

    $expectedAssetKind = if ($RuntimeIdentifier -like "win-*") { "installer" } else { "archive" }
    if ($manifest.ContainsKey("asset_kind") -and $manifest["asset_kind"] -eq $expectedAssetKind) {
        Add-Pass "$channelDisplayName manifest pouziva $expectedAssetKind asset."
    }
    else {
        Add-Blocker "$channelDisplayName manifest nepouziva $expectedAssetKind asset."
    }

    if ($manifest.ContainsKey("channel") -and $manifest["channel"] -eq $channelName) {
        Add-Pass "$channelDisplayName manifest je v kanalu $channelName."
    }
    else {
        Add-Blocker "$channelDisplayName manifest nema channel=$channelName."
    }

    $assetSize = 0L
    if ($manifest.ContainsKey("asset_size") -and [long]::TryParse($manifest["asset_size"], [ref]$assetSize) -and $assetSize -gt 0) {
        $manifestAssetSize = $assetSize
        Add-Pass "$channelDisplayName manifest obsahuje velikost assetu."
    }
    else {
        Add-Blocker "$channelDisplayName manifest neobsahuje platnou velikost assetu."
    }
}

if ($channelName -eq "stable") {
    if (-not (Test-Path -LiteralPath $legacyPreviewManifestPath -PathType Leaf)) {
        Add-Warning "Chybi prechodovy preview manifest update\latest-dotnet-preview-$RuntimeIdentifier.ini."
    }
    elseif (Test-Path -LiteralPath $manifestPath -PathType Leaf) {
        $stableContent = Get-Content -Raw -LiteralPath $manifestPath
        $legacyContent = Get-Content -Raw -LiteralPath $legacyPreviewManifestPath
        if ($stableContent -eq $legacyContent) {
            Add-Pass "Prechodovy preview manifest odpovida stabilnimu manifestu."
        }
        else {
            Add-Blocker "Prechodovy preview manifest neodpovida stabilnimu manifestu."
        }
    }
}
else {
    Add-Warning "Preview alias a AHK retirement gate se overuji jen pro stable kanal."
}

if ($SkipNetwork) {
    Add-Warning "Sitova kontrola release assetu a release notes byla preskocena."
}
else {
    Invoke-RemoteHeadCheck -Name "Release asset" -Url $assetUrl -ExpectedSize $manifestAssetSize
    Invoke-RemoteHeadCheck -Name "Release notes" -Url $notesUrl -ExpectedSize $null
}

if ($channelName -ne "stable") {
    Add-Warning "AHK retirement gate se spousti jen pro stable kanal."
}
elseif ($SkipRetirementGate) {
    Add-Warning "AHK retirement gate byl preskocen."
}
elseif (-not (Test-Path -LiteralPath $retirementReadinessScript -PathType Leaf)) {
    Add-Blocker "Chybi Get-AhkRetirementReadiness.ps1."
}
else {
    $currentPowerShell = (Get-Process -Id $PID).Path
    if ([string]::IsNullOrWhiteSpace($currentPowerShell)) {
        $currentPowerShell = if ($PSVersionTable.PSEdition -eq "Core") { "pwsh" } else { "powershell" }
    }

    $retirementProcess = Start-Process `
        -FilePath $currentPowerShell `
        -ArgumentList @("-NoLogo", "-NoProfile", "-File", $retirementReadinessScript, "-RuntimeIdentifier", $RuntimeIdentifier, "-FailOnBlockers") `
        -NoNewWindow `
        -Wait `
        -PassThru

    if ($retirementProcess.ExitCode -eq 0) {
        Add-Pass "AHK retirement gate je pruchozi pro $RuntimeIdentifier."
    }
    else {
        Add-Blocker "AHK retirement gate selhal pro $RuntimeIdentifier."
    }
}

Write-Host "Vehimap published .NET release verification"
Write-Host "Runtime: $RuntimeIdentifier"
Write-Host "Channel: $channelName"
Write-Host "Base version: $version"
if (-not [string]::IsNullOrWhiteSpace($manifestVersion)) {
    Write-Host "Published version: $manifestVersion"
}
Write-Host "Tag: $releaseTag"
if (-not [string]::IsNullOrWhiteSpace($expectedPackageFileName)) {
    Write-Host "Expected package: $expectedPackageFileName"
}
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
    Write-Host "Vysledek: publikovany .NET desktop release je overeny pro $RuntimeIdentifier."
}
else {
    Write-Host "Vysledek: release zatim neberte jako hotovy. Nejdrive odstrante blockery vyse."
    exit 1
}
