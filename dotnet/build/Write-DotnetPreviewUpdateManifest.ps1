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
Write-Host "Wrote preview update manifest: $OutputPath"
