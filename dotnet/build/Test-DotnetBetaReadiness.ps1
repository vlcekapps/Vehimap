param(
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release",
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
    Channel = "beta"
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
