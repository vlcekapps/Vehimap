# SPDX-License-Identifier: GPL-3.0-or-later
param(
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$compatibilityProject = Join-Path $dotnetRoot "tests\Vehimap.Tests.LegacyCompatibility\Vehimap.Tests.LegacyCompatibility.csproj"
$unitProject = Join-Path $dotnetRoot "tests\Vehimap.Tests.Unit\Vehimap.Tests.Unit.csproj"

Write-Host "Vehimap SQLite 2.0 storage nightly gate"
Write-Host "Runtime: $RuntimeIdentifier"
Write-Host "Configuration: $Configuration"
Write-Host "Project: $compatibilityProject"
Write-Host ""
Write-Host "Overuji migraci legacy fixture dat, health check SQLite databaze, SQLite-only runtime zapis, SQLite backup, import stare zalohy a balicek vozidla."
Write-Host "Includes restore journal recovery after process termination and interrupted rollback."
Write-Host "Includes archive limits and package copy/commit rollback with shell lifecycle guards."

Push-Location $dotnetRoot
try {
    dotnet test $compatibilityProject --configuration $Configuration --filter "FullyQualifiedName~Vehimap.Tests.LegacyCompatibility.SqliteStorageCompatibilityTests" -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    dotnet test $unitProject --configuration $Configuration --filter "FullyQualifiedName~Vehimap.Tests.Unit.RuntimeStorageWriteGuardTests|FullyQualifiedName~Vehimap.Tests.Unit.DataArchiveSafetyTests|FullyQualifiedName~Package_import_publishes_only_committed_data" -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "SQLite 2.0 storage nightly gate OK"
