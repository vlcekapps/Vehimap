param(
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$compatibilityProject = Join-Path $dotnetRoot "tests\Vehimap.Tests.LegacyCompatibility\Vehimap.Tests.LegacyCompatibility.csproj"

Write-Host "Vehimap SQLite 2.0 storage nightly gate"
Write-Host "Runtime: $RuntimeIdentifier"
Write-Host "Configuration: $Configuration"
Write-Host "Project: $compatibilityProject"
Write-Host ""
Write-Host "Overuji migraci legacy fixture dat, SQLite backup, import stare zalohy a balicek vozidla."

Push-Location $dotnetRoot
try {
    dotnet test $compatibilityProject --configuration $Configuration --filter "FullyQualifiedName~Vehimap.Tests.LegacyCompatibility.SqliteStorageCompatibilityTests" -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "SQLite 2.0 storage nightly gate OK"
