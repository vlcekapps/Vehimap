# SPDX-License-Identifier: GPL-3.0-or-later
param(
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release",
    [ValidateSet("stable", "beta", "nightly")]
    [string]$Channel = "stable",
    [string]$EffectiveVersion,
    [switch]$InstallSmoke,
    [switch]$AllowLocalInstallSmoke,
    [int]$InstallerSmokeLaunchSeconds = 8,
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

$channelName = $Channel.ToLowerInvariant()
if ([string]::IsNullOrWhiteSpace($EffectiveVersion)) {
    if ($channelName -eq "nightly") {
        $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmmss", [System.Globalization.CultureInfo]::InvariantCulture)
        $effectiveVersion = "$version-nightly.local.$timestamp"
    }
    else {
        $effectiveVersion = $version
    }
}
else {
    $effectiveVersion = $EffectiveVersion.Trim()
    if ([string]::IsNullOrWhiteSpace($effectiveVersion)) {
        throw "EffectiveVersion nesmi byt prazdna hodnota."
    }
}

$releaseTag = switch ($channelName) {
    "beta" { "dotnet-beta-v$version" }
    "nightly" { "dotnet-nightly" }
    default { "dotnet-v$version" }
}
$manifestFileName = if ($channelName -eq "stable") {
    "latest-dotnet-$RuntimeIdentifier.ini"
}
else {
    "latest-dotnet-$channelName-$RuntimeIdentifier.ini"
}
$readinessRoot = Join-Path $dotnetRoot "artifacts\$channelName\$RuntimeIdentifier"
$publishDirectory = Join-Path $readinessRoot "app"
$releaseDirectory = Join-Path $readinessRoot "release"
$manifestPath = Join-Path $readinessRoot $manifestFileName
$installerSmokeScript = Join-Path $PSScriptRoot "Test-DotnetInstallerSmoke.ps1"

Write-Host "Vehimap .NET desktop release readiness"
Write-Host "Base version: $version"
Write-Host "Effective version: $effectiveVersion"
Write-Host "Runtime: $RuntimeIdentifier"
Write-Host "Configuration: $Configuration"
Write-Host "Channel: $channelName"
Write-Host "Release tag: $releaseTag"

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

    dotnet publish src\Vehimap.Desktop\Vehimap.Desktop.csproj -c $Configuration -r $RuntimeIdentifier --self-contained true -p:UseSharedCompilation=false "-p:VehimapReleaseChannel=$channelName" "-p:VehimapVersion=$effectiveVersion" -o $publishDirectory
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    & (Join-Path $PSScriptRoot "Package-DesktopRelease.ps1") `
        -PublishDirectory $publishDirectory `
        -RuntimeIdentifier $RuntimeIdentifier `
        -Version $effectiveVersion `
        -OutputDirectory $releaseDirectory `
        -Channel $channelName

    $expectedPackageBaseName = if ($RuntimeIdentifier -like "win-*") {
        "vehimap-desktop-$channelName-$effectiveVersion-$RuntimeIdentifier-setup"
    }
    else {
        "vehimap-desktop-$channelName-$effectiveVersion-$RuntimeIdentifier"
    }
    $metadataPath = Join-Path $releaseDirectory "$expectedPackageBaseName.json"
    if (-not (Test-Path -LiteralPath $metadataPath -PathType Leaf)) {
        throw "Release metadata '$metadataPath' nebyla vytvorena."
    }
    $metadata = Get-Item -LiteralPath $metadataPath

    & (Join-Path $PSScriptRoot "Write-DotnetUpdateManifest.ps1") `
        -PackageMetadataPath $metadata.FullName `
        -ArtifactsDirectory $releaseDirectory `
        -ReleaseTag $releaseTag `
        -OutputPath $manifestPath

    $metadataContent = Get-Content -Raw -LiteralPath $metadata.FullName | ConvertFrom-Json
    $package = Get-Item -LiteralPath (Join-Path $releaseDirectory $metadataContent.packageFile)
    if ($null -eq $package) {
        throw "Release balicek '$($metadataContent.packageFile)' nebyl vytvoren."
    }

    if ($RuntimeIdentifier -like "win-*" -and $metadataContent.assetKind -ne "installer") {
        throw "Windows release musi byt Inno Setup installer, ne portable archiv."
    }

    if ($RuntimeIdentifier -like "win-*" -and $package.Extension -ne ".exe") {
        throw "Windows release musi mit priponu .exe."
    }

    $checksumPath = "$($package.FullName).sha256"
    if (-not (Test-Path -LiteralPath $checksumPath -PathType Leaf)) {
        throw "Checksum '$checksumPath' nebyl vytvoren."
    }

    if ($RuntimeIdentifier -like "win-*") {
        if (-not (Test-Path -LiteralPath $installerSmokeScript -PathType Leaf)) {
            throw "Chybi installer smoke skript '$installerSmokeScript'."
        }

        $installerSmokeArguments = @{
            InstallerPath = $package.FullName
            PackageMetadataPath = $metadata.FullName
        }

        if ($InstallSmoke) {
            $installerSmokeArguments["Install"] = $true
            $installerSmokeArguments["LaunchSeconds"] = $InstallerSmokeLaunchSeconds
            if ($AllowLocalInstallSmoke) {
                $installerSmokeArguments["AllowLocalInstall"] = $true
            }
        }

        & $installerSmokeScript @installerSmokeArguments
    }

    $manifestContent = Get-Content -Raw -LiteralPath $manifestPath
    $manifestValues = @{}
    foreach ($line in (Get-Content -LiteralPath $manifestPath)) {
        if ($line -match "^\s*([^=]+?)\s*=(.*)$") {
            $manifestValues[$matches[1].Trim()] = $matches[2].Trim()
        }
    }

    if (-not $manifestValues.ContainsKey("version") -or $manifestValues["version"] -ne $effectiveVersion) {
        throw "Update manifest neobsahuje ocekavanou verzi '$effectiveVersion'."
    }

    if (-not $manifestValues.ContainsKey("channel") -or $manifestValues["channel"] -ne $channelName) {
        throw "Update manifest neobsahuje ocekavany kanal '$channelName'."
    }

    $expectedAssetKind = if ($RuntimeIdentifier -like "win-*") { "installer" } else { "archive" }
    if (-not $manifestValues.ContainsKey("asset_kind") -or $manifestValues["asset_kind"] -ne $expectedAssetKind) {
        throw "Update manifest neobsahuje ocekavany typ assetu '$expectedAssetKind'."
    }

    $assetUrl = if ($manifestValues.ContainsKey("asset_url")) { $manifestValues["asset_url"] } else { "" }
    $expectedAssetPattern = "*/releases/download/$releaseTag/$($package.Name)"
    if ([string]::IsNullOrWhiteSpace($assetUrl) -or $assetUrl -notlike $expectedAssetPattern) {
        throw "Update manifest neukazuje na release tag '$releaseTag' a balicek '$($package.Name)'."
    }

    $assetSha256 = if ($manifestValues.ContainsKey("asset_sha256")) { $manifestValues["asset_sha256"] } else { "" }
    if ($assetSha256 -notmatch "^[a-fA-F0-9]{64}$") {
        throw "Update manifest neobsahuje platny SHA-256 hash assetu."
    }

    $assetSize = 0L
    if (-not $manifestValues.ContainsKey("asset_size") -or -not [long]::TryParse($manifestValues["asset_size"], [ref]$assetSize) -or $assetSize -ne $package.Length) {
        throw "Update manifest neobsahuje ocekavanou velikost assetu '$($package.Length)'."
    }

    if ($channelName -eq "stable" -and $manifestContent -match "preview") {
        throw "Stable update manifest nesmi obsahovat preview oznaceni."
    }

    Write-Host "Release readiness OK"
    Write-Host "Artifact root: $readinessRoot"
    Write-Host "App: $(Join-Path $publishDirectory 'Vehimap.Desktop.exe')"
    Write-Host "Package: $($package.FullName)"
    Write-Host "Manifest: $manifestPath"
}
finally {
    Pop-Location
}
