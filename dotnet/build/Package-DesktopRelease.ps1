param(
    [Parameter(Mandatory = $true)]
    [string]$PublishDirectory,

    [Parameter(Mandatory = $true)]
    [string]$RuntimeIdentifier,

    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Copy-ReleasePayload {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDirectory,

        [Parameter(Mandatory = $true)]
        [string]$DestinationDirectory
    )

    New-Item -ItemType Directory -Path $DestinationDirectory -Force | Out-Null
    $sourceRoot = [System.IO.Path]::GetFullPath($SourceDirectory)
    if (-not $sourceRoot.EndsWith([System.IO.Path]::DirectorySeparatorChar) -and -not $sourceRoot.EndsWith([System.IO.Path]::AltDirectorySeparatorChar)) {
        $sourceRoot += [System.IO.Path]::DirectorySeparatorChar
    }

    Get-ChildItem -Path $SourceDirectory -Recurse -File | ForEach-Object {
        if ($_.Extension -ieq ".pdb") {
            return
        }

        $relativePath = $_.FullName.Substring($sourceRoot.Length)
        $targetPath = Join-Path $DestinationDirectory $relativePath
        $targetDirectory = Split-Path -Path $targetPath -Parent
        New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $targetPath -Force
    }
}

function New-MacAppBundle {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDirectory,

        [Parameter(Mandatory = $true)]
        [string]$DestinationRoot,

        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    $appBundle = Join-Path $DestinationRoot "Vehimap.app"
    $contentsDirectory = Join-Path $appBundle "Contents"
    $macOsDirectory = Join-Path $contentsDirectory "MacOS"
    $resourcesDirectory = Join-Path $contentsDirectory "Resources"

    New-Item -ItemType Directory -Path $macOsDirectory -Force | Out-Null
    New-Item -ItemType Directory -Path $resourcesDirectory -Force | Out-Null

    Copy-ReleasePayload -SourceDirectory $SourceDirectory -DestinationDirectory $macOsDirectory

    $infoPlistPath = Join-Path $contentsDirectory "Info.plist"
    $infoPlist = @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "https://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key>
  <string>Vehimap</string>
  <key>CFBundleDisplayName</key>
  <string>Vehimap Desktop Preview</string>
  <key>CFBundleIdentifier</key>
  <string>com.vlcekapps.vehimap.desktop.preview</string>
  <key>CFBundleVersion</key>
  <string>$Version</string>
  <key>CFBundleShortVersionString</key>
  <string>$Version</string>
  <key>CFBundleExecutable</key>
  <string>Vehimap.Desktop</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>LSMinimumSystemVersion</key>
  <string>12.0</string>
</dict>
</plist>
"@
    Set-Content -Path $infoPlistPath -Value $infoPlist -Encoding UTF8
    return $appBundle
}

function New-ZipArchive {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDirectory,

        [Parameter(Mandatory = $true)]
        [string]$ArchivePath
    )

    if (Test-Path -LiteralPath $ArchivePath) {
        Remove-Item -LiteralPath $ArchivePath -Force
    }

    [System.IO.Compression.ZipFile]::CreateFromDirectory($SourceDirectory, $ArchivePath)
}

function New-TarGzArchive {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceParentDirectory,

        [Parameter(Mandatory = $true)]
        [string]$SourceName,

        [Parameter(Mandatory = $true)]
        [string]$ArchivePath
    )

    if (Test-Path -LiteralPath $ArchivePath) {
        Remove-Item -LiteralPath $ArchivePath -Force
    }

    $process = Start-Process -FilePath "tar" `
        -ArgumentList @("-czf", $ArchivePath, $SourceName) `
        -WorkingDirectory $SourceParentDirectory `
        -NoNewWindow `
        -Wait `
        -PassThru

    if ($process.ExitCode -ne 0) {
        throw "Vytvoření tar.gz archivu selhalo s kódem $($process.ExitCode)."
    }
}

function Write-ChecksumFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ArtifactPath
    )

    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $ArtifactPath).Hash.ToLowerInvariant()
    $checksumPath = "$ArtifactPath.sha256"
    $artifactName = Split-Path -Path $ArtifactPath -Leaf
    Set-Content -Path $checksumPath -Value "$hash  $artifactName" -Encoding ascii
    return $checksumPath
}

$resolvedPublishDirectory = (Resolve-Path -LiteralPath $PublishDirectory).Path
if (-not (Test-Path -LiteralPath $resolvedPublishDirectory -PathType Container)) {
    throw "Publikační složka '$PublishDirectory' neexistuje."
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$resolvedOutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path

$packageBaseName = "vehimap-desktop-preview-$Version-$RuntimeIdentifier"
$stagingRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("vehimap-release-" + [guid]::NewGuid().ToString("N"))
$stagingDirectory = Join-Path $stagingRoot $packageBaseName

New-Item -ItemType Directory -Path $stagingDirectory -Force | Out-Null

try {
    if ($RuntimeIdentifier -like "osx-*") {
        $appBundle = New-MacAppBundle -SourceDirectory $resolvedPublishDirectory -DestinationRoot $stagingDirectory -Version $Version
        $archivePath = Join-Path $resolvedOutputDirectory "$packageBaseName.zip"
        New-ZipArchive -SourceDirectory $stagingDirectory -ArchivePath $archivePath
    }
    elseif ($RuntimeIdentifier -like "linux-*") {
        $payloadDirectory = $stagingDirectory
        Copy-ReleasePayload -SourceDirectory $resolvedPublishDirectory -DestinationDirectory $payloadDirectory
        $archivePath = Join-Path $resolvedOutputDirectory "$packageBaseName.tar.gz"
        New-TarGzArchive -SourceParentDirectory $stagingRoot -SourceName $packageBaseName -ArchivePath $archivePath
    }
    else {
        $payloadDirectory = $stagingDirectory
        Copy-ReleasePayload -SourceDirectory $resolvedPublishDirectory -DestinationDirectory $payloadDirectory
        $archivePath = Join-Path $resolvedOutputDirectory "$packageBaseName.zip"
        New-ZipArchive -SourceDirectory $stagingDirectory -ArchivePath $archivePath
    }

    $checksumPath = Write-ChecksumFile -ArtifactPath $archivePath

    $manifestPath = Join-Path $resolvedOutputDirectory "$packageBaseName.json"
    $artifactName = Split-Path -Path $archivePath -Leaf
    $manifest = [ordered]@{
        version = $Version
        runtimeIdentifier = $RuntimeIdentifier
        packageFile = $artifactName
        checksumFile = (Split-Path -Path $checksumPath -Leaf)
        createdUtc = [DateTime]::UtcNow.ToString("o")
    } | ConvertTo-Json -Depth 3
    Set-Content -Path $manifestPath -Value $manifest -Encoding UTF8

    Write-Host "Created package: $archivePath"
    Write-Host "Checksum file: $checksumPath"
    Write-Host "Manifest file: $manifestPath"
}
finally {
    if (Test-Path -LiteralPath $stagingRoot) {
        Remove-Item -LiteralPath $stagingRoot -Recurse -Force
    }
}
