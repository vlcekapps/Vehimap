# SPDX-License-Identifier: GPL-3.0-or-later
param(
    [string[]]$RuntimeIdentifiers = @("win-x64", "linux-x64", "osx-x64", "osx-arm64"),
    [string]$Configuration = "Release",
    [string]$EffectiveVersion,
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $dotnetRoot
$versionPath = Join-Path $repositoryRoot "src\VERSION"
$supportedRuntimeIdentifiers = @("win-x64", "linux-x64", "osx-x64", "osx-arm64")

if (-not (Test-Path -LiteralPath $versionPath -PathType Leaf)) {
    throw "Soubor verze '$versionPath' neexistuje."
}

$baseVersion = (Get-Content -LiteralPath $versionPath | Select-Object -First 1).Trim()
if ([string]::IsNullOrWhiteSpace($EffectiveVersion)) {
    $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmmss", [System.Globalization.CultureInfo]::InvariantCulture)
    $EffectiveVersion = "$baseVersion-nightly.local.$timestamp"
}

$requestedRuntimeIdentifiers = @($RuntimeIdentifiers | Select-Object -Unique)
if ($requestedRuntimeIdentifiers.Count -eq 0) {
    throw "Zadejte alespon jeden runtime identifier."
}

foreach ($runtimeIdentifier in $requestedRuntimeIdentifiers) {
    if ($runtimeIdentifier -notin $supportedRuntimeIdentifiers) {
        throw "Nepodporovany runtime '$runtimeIdentifier'. Podporovane hodnoty: $($supportedRuntimeIdentifiers -join ', ')."
    }
}

Write-Host "Vehimap desktop nightly matrix"
Write-Host "Version: $EffectiveVersion"
Write-Host "Runtimes: $($requestedRuntimeIdentifiers -join ', ')"

$testsCompleted = $SkipTests.IsPresent
foreach ($runtimeIdentifier in $requestedRuntimeIdentifiers) {
    Write-Host ""
    Write-Host "=== $runtimeIdentifier ==="

    $arguments = @{
        RuntimeIdentifier = $runtimeIdentifier
        Configuration = $Configuration
        EffectiveVersion = $EffectiveVersion
    }

    if ($testsCompleted) {
        $arguments["SkipTests"] = $true
        $arguments["SkipSolutionBuild"] = $true
    }

    & (Join-Path $PSScriptRoot "Test-DotnetNightlyReadiness.ps1") @arguments
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    $testsCompleted = $true
}

Write-Host ""
Write-Host "Nightly matrix OK"
foreach ($runtimeIdentifier in $requestedRuntimeIdentifiers) {
    $appFileName = if ($runtimeIdentifier -like "win-*") { "Vehimap.Desktop.exe" } else { "Vehimap.Desktop" }
    $artifactRoot = Join-Path $dotnetRoot "artifacts\nightly\$runtimeIdentifier"
    $appPath = Join-Path $artifactRoot "app\$appFileName"
    $releasePath = Join-Path $artifactRoot "release"
    Write-Host "$runtimeIdentifier app: $appPath"
    Write-Host "$runtimeIdentifier package: $releasePath"
}
