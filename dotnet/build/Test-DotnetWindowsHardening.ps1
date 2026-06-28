param(
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release",
    [int]$InstallerSmokeLaunchSeconds = 8,
    [switch]$SkipTests,
    [switch]$SkipInstallSmoke,
    [switch]$SkipFetch,
    [switch]$VerifyPublishedNightly,
    [switch]$SkipRetirementStatus
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($RuntimeIdentifier -notlike "win-*") {
    throw "Windows hardening gate je urcena jen pro Windows runtime. Zadano: $RuntimeIdentifier."
}

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $dotnetRoot
$solutionPath = Join-Path $dotnetRoot "Vehimap.sln"

function Invoke-HardeningStep {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    Write-Host ""
    Write-Host "== $Name =="
    $global:LASTEXITCODE = 0
    & $Action
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Write-Host "Vehimap Windows hardening gate"
Write-Host "Runtime: $RuntimeIdentifier"
Write-Host "Configuration: $Configuration"
Write-Host "Tests: $(-not $SkipTests)"
Write-Host "Install smoke: $(-not $SkipInstallSmoke)"
Write-Host "Published nightly verification: $VerifyPublishedNightly"

$releaseTrainArguments = @{
    RuntimeIdentifier = $RuntimeIdentifier
}
if ($SkipFetch) {
    $releaseTrainArguments["SkipFetch"] = $true
}

Invoke-HardeningStep "Release train status" {
    & (Join-Path $PSScriptRoot "Get-DotnetReleaseTrainStatus.ps1") @releaseTrainArguments
}

if (-not $SkipTests) {
    Invoke-HardeningStep "dotnet test" {
        Push-Location $dotnetRoot
        try {
            dotnet test $solutionPath --configuration $Configuration -p:UseSharedCompilation=false
        }
        finally {
            Pop-Location
        }
    }
}

$nightlyReadinessArguments = @{
    RuntimeIdentifier = $RuntimeIdentifier
    Configuration = $Configuration
}
$nightlyReadinessArguments["SkipTests"] = $true
if (-not $SkipInstallSmoke) {
    $nightlyReadinessArguments["InstallSmoke"] = $true
    $nightlyReadinessArguments["InstallerSmokeLaunchSeconds"] = $InstallerSmokeLaunchSeconds
}

Invoke-HardeningStep "Nightly readiness" {
    & (Join-Path $PSScriptRoot "Test-DotnetNightlyReadiness.ps1") @nightlyReadinessArguments
}

if ($VerifyPublishedNightly) {
    Invoke-HardeningStep "Published nightly verification" {
        & (Join-Path $PSScriptRoot "Test-DotnetPublishedNightly.ps1") -RuntimeIdentifier $RuntimeIdentifier
    }
}
else {
    Write-Host ""
    Write-Host "Published nightly verification skipped. Po dobehu GitHub Actions spustte:"
    Write-Host "  powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetPublishedNightly.ps1 -RuntimeIdentifier $RuntimeIdentifier"
}

if (-not $SkipRetirementStatus) {
    Invoke-HardeningStep "AHK retirement status" {
        & (Join-Path $PSScriptRoot "Get-AhkRetirementReadiness.ps1") -RuntimeIdentifier $RuntimeIdentifier
    }
}

$nightlyApp = Join-Path $dotnetRoot "artifacts\nightly\$RuntimeIdentifier\app\Vehimap.Desktop.exe"
$nightlyRelease = Join-Path $dotnetRoot "artifacts\nightly\$RuntimeIdentifier\release"

Write-Host ""
Write-Host "Windows hardening gate OK"
Write-Host "Manual NVDA smoke:"
Write-Host "  $nightlyApp"
Write-Host "Nightly installer directory:"
Write-Host "  $nightlyRelease"
Write-Host "Dalsi krok po testerske vlne bez P0/P1 blockeru:"
Write-Host "  powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File .\build\Test-DotnetReleasePromotion.ps1 -TargetChannel beta -RuntimeIdentifier $RuntimeIdentifier -FailOnBlockers"
