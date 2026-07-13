# SPDX-License-Identifier: GPL-3.0-or-later
param(
    [Parameter(Mandatory = $true)]
    [string]$ArchivePath,

    [Parameter(Mandatory = $true)]
    [string]$PackageMetadataPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-NormalizedArchiveEntry {
    param([string]$Value)

    return $Value.Replace('\', '/').TrimStart([char[]]@('.', '/'))
}

function Assert-ArchiveEntry {
    param(
        [string[]]$Entries,
        [string]$ExpectedEntry
    )

    $normalizedExpected = Get-NormalizedArchiveEntry -Value $ExpectedEntry
    if ($Entries -notcontains $normalizedExpected) {
        throw "Archiv neobsahuje povinnou polozku '$normalizedExpected'."
    }
}

$resolvedArchivePath = (Resolve-Path -LiteralPath $ArchivePath).Path
$resolvedMetadataPath = (Resolve-Path -LiteralPath $PackageMetadataPath).Path
$metadata = Get-Content -Raw -LiteralPath $resolvedMetadataPath | ConvertFrom-Json
$archive = Get-Item -LiteralPath $resolvedArchivePath

if ($metadata.assetKind -ne "archive") {
    throw "Metadata nepopisuji archiv: assetKind=$($metadata.assetKind)."
}

if ($metadata.packageFile -ne $archive.Name) {
    throw "Metadata ocekavaji balicek '$($metadata.packageFile)', ale byl predan '$($archive.Name)'."
}

if ([long]$metadata.packageSize -ne $archive.Length) {
    throw "Velikost archivu neodpovida package metadata."
}

$actualSha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $resolvedArchivePath).Hash.ToLowerInvariant()
if ($actualSha256 -ne ([string]$metadata.sha256).ToLowerInvariant()) {
    throw "SHA-256 archivu neodpovida package metadata."
}

$checksumPath = "$resolvedArchivePath.sha256"
if (-not (Test-Path -LiteralPath $checksumPath -PathType Leaf)) {
    throw "Chybi checksum soubor '$checksumPath'."
}

$checksumLine = (Get-Content -LiteralPath $checksumPath | Select-Object -First 1).Trim()
if ($checksumLine -notmatch "^$actualSha256\s+$([regex]::Escape($archive.Name))$") {
    throw "Checksum soubor neobsahuje ocekavany hash a nazev archivu."
}

$runtimeIdentifier = [string]$metadata.runtimeIdentifier
$isWindowsHost = [System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT
$entries = @()
if ($runtimeIdentifier -like "linux-*") {
    if ($archive.Name -notlike "*.tar.gz") {
        throw "Linux release musi byt tar.gz archiv."
    }

    $tarOutput = & tar -tzf $resolvedArchivePath
    if ($LASTEXITCODE -ne 0) {
        throw "Linux tar.gz archiv nelze precist."
    }

    $entries = @($tarOutput | ForEach-Object { Get-NormalizedArchiveEntry -Value $_ })
    $rootName = $archive.Name.Substring(0, $archive.Name.Length - ".tar.gz".Length)
    Assert-ArchiveEntry -Entries $entries -ExpectedEntry "$rootName/Vehimap"
    Assert-ArchiveEntry -Entries $entries -ExpectedEntry "$rootName/LICENSE"
    Assert-ArchiveEntry -Entries $entries -ExpectedEntry "$rootName/THIRD-PARTY-NOTICES.md"

    if (-not $isWindowsHost) {
        $desktopEntry = "$rootName/Vehimap"
        $verboseEntry = & tar -tvzf $resolvedArchivePath | Where-Object { $_ -like "*$desktopEntry" } | Select-Object -First 1
        if ([string]::IsNullOrWhiteSpace($verboseEntry) -or $verboseEntry -notmatch "^...x") {
            throw "Linux archiv neuchoval executable bit souboru '$desktopEntry'."
        }
    }
}
elseif ($runtimeIdentifier -like "osx-*") {
    if ($archive.Extension -ne ".zip") {
        throw "macOS release musi byt ZIP archiv."
    }

    $zip = [System.IO.Compression.ZipFile]::OpenRead($resolvedArchivePath)
    try {
        $entries = @($zip.Entries | ForEach-Object { Get-NormalizedArchiveEntry -Value $_.FullName })
        $desktopZipEntry = $zip.Entries |
            Where-Object { (Get-NormalizedArchiveEntry -Value $_.FullName) -eq "Vehimap.app/Contents/MacOS/Vehimap" } |
            Select-Object -First 1
        $desktopExternalAttributes = if ($null -eq $desktopZipEntry) { 0 } else { [int]$desktopZipEntry.ExternalAttributes }
    }
    finally {
        $zip.Dispose()
    }

    Assert-ArchiveEntry -Entries $entries -ExpectedEntry "Vehimap.app/Contents/MacOS/Vehimap"
    Assert-ArchiveEntry -Entries $entries -ExpectedEntry "Vehimap.app/Contents/Info.plist"
    Assert-ArchiveEntry -Entries $entries -ExpectedEntry "Vehimap.app/Contents/MacOS/LICENSE"
    Assert-ArchiveEntry -Entries $entries -ExpectedEntry "Vehimap.app/Contents/MacOS/THIRD-PARTY-NOTICES.md"

    if (-not $isWindowsHost) {
        $unsignedAttributes = [BitConverter]::ToUInt32([BitConverter]::GetBytes($desktopExternalAttributes), 0)
        $unixMode = ($unsignedAttributes -shr 16) -band 0xFFFF
        if (($unixMode -band 0x40) -eq 0) {
            throw "macOS archiv neuchoval executable bit hlavniho souboru aplikace."
        }
    }
}
else {
    throw "Archive smoke nepodporuje runtime '$runtimeIdentifier'."
}

Write-Host "Archive smoke OK: $runtimeIdentifier"
Write-Host "Package: $resolvedArchivePath"
Write-Host "Entries: $($entries.Count)"
