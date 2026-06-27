param(
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$SkipFetch,
    [switch]$FailOnBlockers
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $dotnetRoot
$versionPath = Join-Path $repositoryRoot "src\VERSION"

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

function Get-ManifestFileName {
    param([string]$Channel)

    if ($Channel -eq "stable") {
        return "latest-dotnet-$RuntimeIdentifier.ini"
    }

    return "latest-dotnet-$Channel-$RuntimeIdentifier.ini"
}

function Get-ReleaseTagName {
    param(
        [string]$Channel,
        [string]$Version
    )

    switch ($Channel) {
        "nightly" { return "dotnet-nightly" }
        "beta" { return "dotnet-beta-v$Version" }
        default { return "dotnet-v$Version" }
    }
}

function Test-RemoteTagExists {
    param([string]$TagName)

    $remoteTag = (Invoke-Git ls-remote --tags origin "refs/tags/$TagName" | Out-String).Trim()
    return -not [string]::IsNullOrWhiteSpace($remoteTag)
}

if (-not (Test-Path -LiteralPath $versionPath -PathType Leaf)) {
    Add-Blocker "Chybi src/VERSION; release train nejde vyhodnotit."
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

foreach ($scriptName in @(
        "Test-DotnetNightlyReadiness.ps1",
        "Test-DotnetBetaReadiness.ps1",
        "Test-DotnetStableReadiness.ps1",
        "Test-DotnetReleasePromotion.ps1",
        "New-DotnetDesktopReleaseTag.ps1",
        "Test-DotnetPublishedNightly.ps1",
        "Test-DotnetPublishedBeta.ps1",
        "Test-DotnetPublishedStable.ps1",
        "Test-DotnetPublishedRelease.ps1",
        "Get-AhkRetirementReadiness.ps1")) {
    $scriptPath = Join-Path $PSScriptRoot $scriptName
    if (Test-Path -LiteralPath $scriptPath -PathType Leaf) {
        Add-Pass "Existuje release skript $scriptName."
    }
    else {
        Add-Blocker "Chybi release skript $scriptName."
    }
}

$channelStates = @{}
foreach ($channel in @("nightly", "beta", "stable")) {
    $artifactRoot = Join-Path $dotnetRoot "artifacts\$channel\$RuntimeIdentifier"
    $appPath = Join-Path $artifactRoot "app\Vehimap.Desktop.exe"
    $releaseDirectory = Join-Path $artifactRoot "release"
    $localManifestPath = Join-Path $artifactRoot (Get-ManifestFileName -Channel $channel)
    $repositoryManifestPath = Join-Path $repositoryRoot ("update\" + (Get-ManifestFileName -Channel $channel))
    $packagePattern = if ($channel -eq "nightly") {
        "vehimap-desktop-nightly-*-$RuntimeIdentifier-setup.exe"
    }
    else {
        "vehimap-desktop-$channel-$version-$RuntimeIdentifier-setup.exe"
    }

    $package = if (Test-Path -LiteralPath $releaseDirectory -PathType Container) {
        Get-ChildItem -LiteralPath $releaseDirectory -Filter $packagePattern | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    }
    else {
        $null
    }

    $metadata = if (Test-Path -LiteralPath $releaseDirectory -PathType Container) {
        Get-ChildItem -LiteralPath $releaseDirectory -Filter "*.json" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    }
    else {
        $null
    }

    if (Test-Path -LiteralPath $appPath -PathType Leaf) {
        Add-Pass "$channel lokalni app existuje: $appPath."
    }
    else {
        Add-Warning "$channel lokalni app zatim chybi: $appPath."
    }

    if ($null -ne $package) {
        Add-Pass "$channel lokalni installer existuje: $($package.FullName)."
    }
    else {
        Add-Warning "$channel lokalni installer zatim chybi v $releaseDirectory."
    }

    if ($null -ne $metadata) {
        Add-Pass "$channel lokalni metadata existuji: $($metadata.FullName)."
    }
    else {
        Add-Warning "$channel lokalni metadata zatim chybi v $releaseDirectory."
    }

    if (Test-Path -LiteralPath $localManifestPath -PathType Leaf) {
        Add-Pass "$channel lokalni manifest existuje: $localManifestPath."
    }
    else {
        Add-Warning "$channel lokalni manifest zatim chybi: $localManifestPath."
    }

    if (Test-Path -LiteralPath $repositoryManifestPath -PathType Leaf) {
        Add-Pass "$channel publikovany manifest v repozitari existuje: $repositoryManifestPath."
    }
    else {
        Add-Warning "$channel publikovany manifest v repozitari zatim chybi: $repositoryManifestPath."
    }

    $channelStates[$channel] = [pscustomobject]@{
        Channel = $channel
        AppExists = Test-Path -LiteralPath $appPath -PathType Leaf
        PackageExists = $null -ne $package
        LocalManifestExists = Test-Path -LiteralPath $localManifestPath -PathType Leaf
        RepositoryManifestExists = Test-Path -LiteralPath $repositoryManifestPath -PathType Leaf
        TagName = Get-ReleaseTagName -Channel $channel -Version $version
        RemoteTagExists = $false
    }
}

if (-not $SkipFetch) {
    try {
        Invoke-Git fetch origin main --tags | Out-Null
        Add-Pass "Origin main a tagy byly nacteny."

        foreach ($channel in @("nightly", "beta", "stable")) {
            $state = $channelStates[$channel]
            $remoteTagExists = Test-RemoteTagExists -TagName $state.TagName
            $state.RemoteTagExists = $remoteTagExists
            if ($remoteTagExists) {
                Add-Pass "$channel tag existuje na origin: $($state.TagName)."
            }
            else {
                Add-Warning "$channel tag zatim na origin neexistuje: $($state.TagName)."
            }
        }
    }
    catch {
        Add-Warning "Remote tagy se nepodarilo overit: $($_.Exception.Message)"
    }
}
else {
    Add-Warning "Fetch a remote tag check byly preskoceny; release train status nemusi znat publikovane tagy."
}

$nextStep = if (-not $channelStates["nightly"].PackageExists -or -not $channelStates["nightly"].LocalManifestExists) {
    "Spustte .\build\Test-DotnetNightlyReadiness.ps1 -RuntimeIdentifier $RuntimeIdentifier."
}
elseif (-not $SkipFetch -and -not $channelStates["nightly"].RemoteTagExists) {
    "Publikujte nebo rucne spustte nightly workflow a po dobehu overte .\build\Test-DotnetPublishedNightly.ps1 -RuntimeIdentifier $RuntimeIdentifier."
}
elseif (-not $channelStates["nightly"].RepositoryManifestExists) {
    "Pockejte na nightly release workflow, commit update\$(Get-ManifestFileName -Channel nightly) a potom spustte .\build\Test-DotnetPublishedNightly.ps1 -RuntimeIdentifier $RuntimeIdentifier."
}
elseif (-not $channelStates["beta"].PackageExists -or -not $channelStates["beta"].LocalManifestExists) {
    "Spustte .\build\Test-DotnetBetaReadiness.ps1 -RuntimeIdentifier $RuntimeIdentifier."
}
elseif (-not $SkipFetch -and -not $channelStates["beta"].RemoteTagExists) {
    "Spustte .\build\Test-DotnetReleasePromotion.ps1 -TargetChannel beta -RuntimeIdentifier $RuntimeIdentifier a po uspesne kontrole vytvorte beta tag."
}
elseif (-not $channelStates["beta"].RepositoryManifestExists) {
    "Pockejte na beta release workflow, commit update\$(Get-ManifestFileName -Channel beta) a potom spustte .\build\Test-DotnetPublishedBeta.ps1 -RuntimeIdentifier $RuntimeIdentifier."
}
elseif (-not $channelStates["stable"].PackageExists -or -not $channelStates["stable"].LocalManifestExists) {
    "Spustte .\build\Test-DotnetStableReadiness.ps1 -RuntimeIdentifier $RuntimeIdentifier."
}
elseif ($SkipFetch) {
    "Spustte .\build\Get-DotnetReleaseTrainStatus.ps1 -RuntimeIdentifier $RuntimeIdentifier bez -SkipFetch, aby bylo mozne presne vyhodnotit remote tagy a publikovane release kroky."
}
elseif (-not $SkipFetch -and -not $channelStates["stable"].RemoteTagExists) {
    "Spustte .\build\Test-DotnetReleasePromotion.ps1 -TargetChannel stable -RuntimeIdentifier $RuntimeIdentifier a po uspesne kontrole vytvorte stable tag."
}
elseif (-not $channelStates["stable"].RepositoryManifestExists) {
    "Pockejte na stable release workflow, commit update/latest-dotnet-$RuntimeIdentifier.ini a potom spustte .\build\Test-DotnetPublishedStable.ps1 -RuntimeIdentifier $RuntimeIdentifier."
}
else {
    "Spustte .\build\Get-AhkRetirementReadiness.ps1 -RuntimeIdentifier $RuntimeIdentifier -FailOnBlockers a pripravte finalni AHK retirement commit."
}

Write-Host "Vehimap .NET release train status"
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
Write-Host "Dalsi doporuceny krok:"
Write-Host "  $nextStep"

if ($FailOnBlockers -and $blockers.Count -gt 0) {
    exit 1
}
