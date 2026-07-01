# SPDX-License-Identifier: GPL-3.0-or-later
param(
    [Parameter(Mandatory = $true)]
    [string]$PackageMetadataPath,

    [Parameter(Mandatory = $true)]
    [string]$ArtifactsDirectory,

    [Parameter(Mandatory = $true)]
    [string]$ReleaseTag,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [string]$RepositoryFullName = "vlcekapps/Vehimap"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $PackageMetadataPath -PathType Leaf)) {
    throw "Metadata package '$PackageMetadataPath' neexistuji."
}

if (-not (Test-Path -LiteralPath $ArtifactsDirectory -PathType Container)) {
    throw "Slozka s artefakty '$ArtifactsDirectory' neexistuje."
}

$metadata = Get-Content -Raw -LiteralPath $PackageMetadataPath | ConvertFrom-Json

if ([string]::IsNullOrWhiteSpace($metadata.packageFile)) {
    throw "Metadata neobsahuji packageFile."
}

if ([string]::IsNullOrWhiteSpace($metadata.checksumFile)) {
    throw "Metadata neobsahuji checksumFile."
}

if ([string]::IsNullOrWhiteSpace($metadata.version)) {
    throw "Metadata neobsahuji version."
}

$assetKind = "archive"
$metadataAssetKind = $metadata.PSObject.Properties["assetKind"]
if ($null -ne $metadataAssetKind -and -not [string]::IsNullOrWhiteSpace([string]$metadataAssetKind.Value)) {
    $assetKind = ([string]$metadataAssetKind.Value).Trim().ToLowerInvariant()
}

if ($assetKind -notin @("archive", "installer")) {
    throw "Metadata obsahuji nepodporovany assetKind '$assetKind'."
}

$channel = "stable"
$metadataChannel = $metadata.PSObject.Properties["channel"]
if ($null -ne $metadataChannel -and -not [string]::IsNullOrWhiteSpace([string]$metadataChannel.Value)) {
    $channel = ([string]$metadataChannel.Value).Trim().ToLowerInvariant()
}

if ($channel -notin @("stable", "beta", "nightly")) {
    throw "Metadata obsahuji nepodporovany channel '$channel'."
}

$packagePath = Join-Path $ArtifactsDirectory $metadata.packageFile
$checksumPath = Join-Path $ArtifactsDirectory $metadata.checksumFile

if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf)) {
    throw "Balicek '$packagePath' neexistuje."
}

if (-not (Test-Path -LiteralPath $checksumPath -PathType Leaf)) {
    throw "Checksum '$checksumPath' neexistuje."
}

$checksumLine = (Get-Content -LiteralPath $checksumPath | Select-Object -First 1).Trim()
if ([string]::IsNullOrWhiteSpace($checksumLine)) {
    throw "Checksum soubor '$checksumPath' je prazdny."
}

$checksum = ($checksumLine -split "\s+")[0].Trim().ToLowerInvariant()
if ($checksum.Length -ne 64) {
    throw "Checksum '$checksumPath' neobsahuje platny SHA-256 hash."
}

$packageInfo = Get-Item -LiteralPath $packagePath
$actualChecksum = (Get-FileHash -Algorithm SHA256 -LiteralPath $packagePath).Hash.ToLowerInvariant()
if ($actualChecksum -ne $checksum) {
    throw "Checksum '$checksumPath' neodpovida skutecnemu SHA-256 hashi balicku '$packagePath'."
}

$metadataSha256 = $metadata.PSObject.Properties["sha256"]
if ($null -ne $metadataSha256 -and -not [string]::IsNullOrWhiteSpace([string]$metadataSha256.Value)) {
    $metadataChecksum = ([string]$metadataSha256.Value).Trim().ToLowerInvariant()
    if ($metadataChecksum -ne $checksum) {
        throw "Metadata '$PackageMetadataPath' obsahuji jiny SHA-256 hash nez checksum soubor."
    }
}

$metadataPackageSize = $metadata.PSObject.Properties["packageSize"]
if ($null -ne $metadataPackageSize -and -not [string]::IsNullOrWhiteSpace([string]$metadataPackageSize.Value)) {
    $expectedSize = [long]$metadataPackageSize.Value
    if ($expectedSize -ne $packageInfo.Length) {
        throw "Metadata '$PackageMetadataPath' obsahuji jinou velikost balicku nez fyzicky soubor."
    }
}

$publishedAt = if ([string]::IsNullOrWhiteSpace($metadata.createdUtc)) {
    [DateTime]::UtcNow.ToString("yyyy-MM-dd")
}
else {
    try {
        [DateTime]::Parse($metadata.createdUtc, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind).ToString("yyyy-MM-dd")
    }
    catch {
        [DateTime]::UtcNow.ToString("yyyy-MM-dd")
    }
}

$releaseAssetUrl = "https://github.com/$RepositoryFullName/releases/download/$ReleaseTag/$($metadata.packageFile)"
$notesUrl = "https://github.com/$RepositoryFullName/releases/tag/$ReleaseTag"

$manifestLines = @(
    "[release]"
    "version=$($metadata.version)"
    "published_at=$publishedAt"
    "channel=$channel"
    "asset_kind=$assetKind"
    "asset_url=$releaseAssetUrl"
    "asset_sha256=$checksum"
    "asset_size=$($packageInfo.Length)"
    "notes_url=$notesUrl"
)

$outputDirectory = Split-Path -Path $OutputPath -Parent
if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

Set-Content -LiteralPath $OutputPath -Value ($manifestLines -join [Environment]::NewLine) -Encoding UTF8
Write-Host "Wrote desktop update manifest: $OutputPath"
