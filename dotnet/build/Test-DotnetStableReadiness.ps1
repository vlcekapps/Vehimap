param(
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release",
    [switch]$InstallSmoke,
    [int]$InstallerSmokeLaunchSeconds = 8,
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$arguments = @{
    RuntimeIdentifier = $RuntimeIdentifier
    Configuration = $Configuration
    Channel = "stable"
}

if ($SkipTests) {
    $arguments["SkipTests"] = $true
}

if ($InstallSmoke) {
    $arguments["InstallSmoke"] = $true
    $arguments["InstallerSmokeLaunchSeconds"] = $InstallerSmokeLaunchSeconds
}

& (Join-Path $PSScriptRoot "Test-DotnetReleaseReadiness.ps1") @arguments
