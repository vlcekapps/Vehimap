# SPDX-License-Identifier: GPL-3.0-or-later
param(
    [string]$RuntimeIdentifier = "win-x64",
    [string]$RepositoryFullName = "vlcekapps/Vehimap",
    [switch]$SkipNetwork
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$arguments = @{
    RuntimeIdentifier = $RuntimeIdentifier
    Channel = "nightly"
    RepositoryFullName = $RepositoryFullName
}

if ($SkipNetwork) {
    $arguments["SkipNetwork"] = $true
}

& (Join-Path $PSScriptRoot "Test-DotnetPublishedRelease.ps1") @arguments
