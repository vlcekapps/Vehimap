param(
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release",
    [string]$EffectiveVersion,
    [switch]$InstallSmoke,
    [switch]$AllowLocalInstallSmoke,
    [int]$InstallerSmokeLaunchSeconds = 8,
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$arguments = @{
    RuntimeIdentifier = $RuntimeIdentifier
    Configuration = $Configuration
    Channel = "nightly"
}

if (-not [string]::IsNullOrWhiteSpace($EffectiveVersion)) {
    $arguments["EffectiveVersion"] = $EffectiveVersion
}

if ($SkipTests) {
    $arguments["SkipTests"] = $true
}

if ($InstallSmoke) {
    $arguments["InstallSmoke"] = $true
    $arguments["InstallerSmokeLaunchSeconds"] = $InstallerSmokeLaunchSeconds
    if ($AllowLocalInstallSmoke) {
        $arguments["AllowLocalInstallSmoke"] = $true
    }
}

& (Join-Path $PSScriptRoot "Test-DotnetReleaseReadiness.ps1") @arguments
