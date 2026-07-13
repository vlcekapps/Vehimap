# SPDX-License-Identifier: GPL-3.0-or-later
[CmdletBinding()]
param(
    [string[]]$RuntimeIdentifiers = @("win-x64", "linux-x64", "osx-x64", "osx-arm64"),
    [string]$Configuration = "Release",
    [string]$EffectiveVersion,
    [string]$AndroidSdkDirectory,
    [string]$JavaSdkDirectory,
    [switch]$SkipTests,
    [switch]$SkipAndroid
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $dotnetRoot
$baseVersion = (Get-Content -LiteralPath (Join-Path $repositoryRoot "src\VERSION") | Select-Object -First 1).Trim()
if ([string]::IsNullOrWhiteSpace($EffectiveVersion)) {
    $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmmss", [System.Globalization.CultureInfo]::InvariantCulture)
    $EffectiveVersion = "$baseVersion-nightly.local.$timestamp"
}

$desktopArguments = @{
    RuntimeIdentifiers = $RuntimeIdentifiers
    Configuration = $Configuration
    EffectiveVersion = $EffectiveVersion
}
if ($SkipTests) { $desktopArguments["SkipTests"] = $true }

& (Join-Path $PSScriptRoot "Test-DotnetNightlyMatrix.ps1") @desktopArguments
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not $SkipAndroid) {
    $androidArguments = @{
        Configuration = $Configuration
        EffectiveVersion = $EffectiveVersion
        SkipTests = $true
    }
    if (-not [string]::IsNullOrWhiteSpace($AndroidSdkDirectory)) {
        $androidArguments["AndroidSdkDirectory"] = $AndroidSdkDirectory
    }
    if (-not [string]::IsNullOrWhiteSpace($JavaSdkDirectory)) {
        $androidArguments["JavaSdkDirectory"] = $JavaSdkDirectory
    }

    & (Join-Path $PSScriptRoot "Test-DotnetAndroidNightlyReadiness.ps1") @androidArguments
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host ""
Write-Host "Vehimap local nightly matrix OK"
Write-Host "Version: $EffectiveVersion"
Write-Host "Desktop roots: $(Join-Path $dotnetRoot 'artifacts\nightly\<runtime>')"
if (-not $SkipAndroid) {
    Write-Host "Android APK: $(Join-Path $dotnetRoot 'artifacts\nightly\android\app\Vehimap.apk')"
}
