param(
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release",
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $dotnetRoot
$versionPath = Join-Path $repositoryRoot "src\VERSION"
if (-not (Test-Path -LiteralPath $versionPath -PathType Leaf)) {
    throw "Soubor verze '$versionPath' neexistuje."
}

$version = (Get-Content -LiteralPath $versionPath | Select-Object -First 1).Trim()
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "Soubor verze '$versionPath' je prazdny."
}

$releaseTag = "dotnet-v$version"
$readinessRoot = Join-Path $dotnetRoot "artifacts\release-readiness\$RuntimeIdentifier"
$publishDirectory = Join-Path $readinessRoot "publish"
$releaseDirectory = Join-Path $readinessRoot "release"
$manifestPath = Join-Path $readinessRoot "latest-dotnet-$RuntimeIdentifier.ini"

Write-Host "Vehimap .NET desktop release readiness"
Write-Host "Version: $version"
Write-Host "Runtime: $RuntimeIdentifier"
Write-Host "Configuration: $Configuration"

Push-Location $dotnetRoot
try {
    dotnet build Vehimap.sln --configuration $Configuration -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    if (-not $SkipTests) {
        dotnet test tests\Vehimap.Tests.Unit\Vehimap.Tests.Unit.csproj --no-build --configuration $Configuration -p:UseSharedCompilation=false
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }

        dotnet test tests\Vehimap.Tests.LegacyCompatibility\Vehimap.Tests.LegacyCompatibility.csproj --no-build --configuration $Configuration -p:UseSharedCompilation=false
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }

        dotnet test tests\Vehimap.Tests.UI\Vehimap.Tests.UI.csproj --no-build --configuration $Configuration -p:UseSharedCompilation=false
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
    }

    if (Test-Path -LiteralPath $readinessRoot) {
        Remove-Item -LiteralPath $readinessRoot -Recurse -Force
    }

    dotnet publish src\Vehimap.Desktop\Vehimap.Desktop.csproj -c $Configuration -r $RuntimeIdentifier --self-contained true -p:UseSharedCompilation=false -o $publishDirectory
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    & (Join-Path $PSScriptRoot "Package-DesktopRelease.ps1") `
        -PublishDirectory $publishDirectory `
        -RuntimeIdentifier $RuntimeIdentifier `
        -Version $version `
        -OutputDirectory $releaseDirectory

    $metadata = Get-ChildItem -LiteralPath $releaseDirectory -Filter "*.json" | Sort-Object Name | Select-Object -First 1
    if ($null -eq $metadata) {
        throw "Release metadata nebyla vytvorena."
    }

    & (Join-Path $PSScriptRoot "Write-DotnetUpdateManifest.ps1") `
        -PackageMetadataPath $metadata.FullName `
        -ArtifactsDirectory $releaseDirectory `
        -ReleaseTag $releaseTag `
        -OutputPath $manifestPath

    $packagePrefix = "vehimap-desktop-$version-$RuntimeIdentifier"
    $package = Get-ChildItem -LiteralPath $releaseDirectory -File |
        Where-Object { $_.Name -like "$packagePrefix.*" -and $_.Extension -ne ".json" -and $_.Extension -ne ".sha256" } |
        Sort-Object Name |
        Select-Object -First 1
    if ($null -eq $package) {
        throw "Release balicek '$packagePrefix' nebyl vytvoren."
    }

    $checksumPath = "$($package.FullName).sha256"
    if (-not (Test-Path -LiteralPath $checksumPath -PathType Leaf)) {
        throw "Checksum '$checksumPath' nebyl vytvoren."
    }

    $manifestContent = Get-Content -Raw -LiteralPath $manifestPath
    $manifestValues = @{}
    foreach ($line in (Get-Content -LiteralPath $manifestPath)) {
        if ($line -match "^\s*([^=]+?)\s*=(.*)$") {
            $manifestValues[$matches[1].Trim()] = $matches[2].Trim()
        }
    }

    if (-not $manifestValues.ContainsKey("version") -or $manifestValues["version"] -ne $version) {
        throw "Update manifest neobsahuje ocekavanou verzi '$version'."
    }

    $assetUrl = if ($manifestValues.ContainsKey("asset_url")) { $manifestValues["asset_url"] } else { "" }
    $expectedAssetPattern = "*/releases/download/$releaseTag/$($package.Name)"
    if ([string]::IsNullOrWhiteSpace($assetUrl) -or $assetUrl -notlike $expectedAssetPattern) {
        throw "Update manifest neukazuje na release tag '$releaseTag' a balicek '$($package.Name)'."
    }

    if ($manifestContent -match "preview") {
        throw "Stable update manifest nesmi obsahovat preview oznaceni."
    }

    Write-Host "Release readiness OK"
    Write-Host "Package: $($package.FullName)"
    Write-Host "Manifest: $manifestPath"
}
finally {
    Pop-Location
}
