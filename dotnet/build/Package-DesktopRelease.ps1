param(
    [Parameter(Mandatory = $true)]
    [string]$PublishDirectory,

    [Parameter(Mandatory = $true)]
    [string]$RuntimeIdentifier,

    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [string]$Channel = "stable"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Normalize-ReleaseChannel {
    param([string]$Channel)

    if ([string]::IsNullOrWhiteSpace($Channel)) {
        return "stable"
    }

    switch ($Channel.Trim().ToLowerInvariant()) {
        "beta" { return "beta" }
        "nightly" { return "nightly" }
        default { return "stable" }
    }
}

function Get-ChannelAppName {
    param([string]$Channel)

    switch (Normalize-ReleaseChannel -Channel $Channel) {
        "beta" { return "Vehimap Beta" }
        "nightly" { return "Vehimap Nightly" }
        default { return "Vehimap" }
    }
}

function Get-ChannelAppId {
    param([string]$Channel)

    switch (Normalize-ReleaseChannel -Channel $Channel) {
        "beta" { return "{{D6BA4F44-3961-4EE0-9645-0C64B00F1D95}" }
        "nightly" { return "{{F62CE01E-1CB2-4E09-A52D-2865B1F02078}" }
        default { return "{{C11E3BB4-7B0A-4D4E-91F3-FBC2F3F50D8A}" }
    }
}

function Resolve-InnoCompiler {
    $envPath = $env:INNO_SETUP_COMPILER
    if (-not [string]::IsNullOrWhiteSpace($envPath) -and (Test-Path -LiteralPath $envPath -PathType Leaf)) {
        return (Resolve-Path -LiteralPath $envPath).Path
    }

    $command = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace($command.Source)) {
        return $command.Source
    }

    foreach ($candidate in @(
        "C:\Program Files\Inno Setup 7\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 7\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    )) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return $candidate
        }
    }

    throw "Inno Setup compiler ISCC.exe nebyl nalezen. Nastavte INNO_SETUP_COMPILER nebo nainstalujte Inno Setup 7."
}

function Escape-InnoTemplateValue {
    param([string]$Value)

    return $Value.Replace('"', '""')
}

function New-InnoSetupInstaller {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDirectory,

        [Parameter(Mandatory = $true)]
        [string]$OutputDirectory,

        [Parameter(Mandatory = $true)]
        [string]$OutputBaseName,

        [Parameter(Mandatory = $true)]
        [string]$Version,

        [Parameter(Mandatory = $true)]
        [string]$Channel
    )

    $repositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    $templatePath = Join-Path $repositoryRoot "dotnet\installer\windows\Vehimap.iss.in"
    if (-not (Test-Path -LiteralPath $templatePath -PathType Leaf)) {
        throw "Inno Setup template '$templatePath' neexistuje."
    }

    $isccPath = Resolve-InnoCompiler
    $appName = Get-ChannelAppName -Channel $Channel
    $appId = Get-ChannelAppId -Channel $Channel
    $generatedScriptPath = Join-Path ([System.IO.Path]::GetTempPath()) ("vehimap-installer-" + [guid]::NewGuid().ToString("N") + ".iss")
    $template = Get-Content -Raw -LiteralPath $templatePath
    $script = $template.Replace("{{APP_ID}}", $appId)
    $script = $script.Replace("{{APP_NAME}}", $appName)
    $script = $script.Replace("{{APP_VERSION}}", $Version)
    $script = $script.Replace("{{INSTALL_FOLDER}}", $appName)
    $script = $script.Replace("{{OUTPUT_DIR}}", (Escape-InnoTemplateValue -Value $OutputDirectory))
    $script = $script.Replace("{{OUTPUT_BASE_FILENAME}}", $OutputBaseName)
    $script = $script.Replace("{{SOURCE_DIR}}", (Escape-InnoTemplateValue -Value $SourceDirectory))

    Set-Content -LiteralPath $generatedScriptPath -Value $script -Encoding UTF8
    try {
        $process = Start-Process -FilePath $isccPath `
            -ArgumentList @("/Qp", $generatedScriptPath) `
            -NoNewWindow `
            -Wait `
            -PassThru

        if ($process.ExitCode -ne 0) {
            throw "Inno Setup compiler selhal s kodem $($process.ExitCode)."
        }
    }
    finally {
        if (Test-Path -LiteralPath $generatedScriptPath -PathType Leaf) {
            Remove-Item -LiteralPath $generatedScriptPath -Force
        }
    }

    return Join-Path $OutputDirectory "$OutputBaseName.exe"
}

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
  <string>Vehimap</string>
  <key>CFBundleIdentifier</key>
  <string>com.vlcekapps.vehimap.desktop</string>
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
    return [pscustomobject]@{
        Path = $checksumPath
        Sha256 = $hash
    }
}

$resolvedPublishDirectory = (Resolve-Path -LiteralPath $PublishDirectory).Path
if (-not (Test-Path -LiteralPath $resolvedPublishDirectory -PathType Container)) {
    throw "Publikační složka '$PublishDirectory' neexistuje."
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$resolvedOutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path

$normalizedChannel = Normalize-ReleaseChannel -Channel $Channel
$assetKind = if ($RuntimeIdentifier -like "win-*") { "installer" } else { "archive" }
$packageBaseName = if ($RuntimeIdentifier -like "win-*") {
    "vehimap-desktop-$normalizedChannel-$Version-$RuntimeIdentifier-setup"
}
else {
    "vehimap-desktop-$Version-$RuntimeIdentifier"
}
$stagingRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("vehimap-release-" + [guid]::NewGuid().ToString("N"))
$stagingDirectory = Join-Path $stagingRoot $packageBaseName

New-Item -ItemType Directory -Path $stagingDirectory -Force | Out-Null

try {
    if ($RuntimeIdentifier -like "win-*") {
        Copy-ReleasePayload -SourceDirectory $resolvedPublishDirectory -DestinationDirectory $stagingDirectory
        $archivePath = New-InnoSetupInstaller `
            -SourceDirectory $stagingDirectory `
            -OutputDirectory $resolvedOutputDirectory `
            -OutputBaseName $packageBaseName `
            -Version $Version `
            -Channel $normalizedChannel
    }
    elseif ($RuntimeIdentifier -like "osx-*") {
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

    $checksum = Write-ChecksumFile -ArtifactPath $archivePath

    $manifestPath = Join-Path $resolvedOutputDirectory "$packageBaseName.json"
    $artifactInfo = Get-Item -LiteralPath $archivePath
    $artifactName = Split-Path -Path $archivePath -Leaf
    $manifest = [ordered]@{
        version = $Version
        runtimeIdentifier = $RuntimeIdentifier
        channel = $normalizedChannel
        assetKind = $assetKind
        packageFile = $artifactName
        checksumFile = (Split-Path -Path $checksum.Path -Leaf)
        sha256 = $checksum.Sha256
        packageSize = $artifactInfo.Length
        createdUtc = [DateTime]::UtcNow.ToString("o")
    } | ConvertTo-Json -Depth 3
    Set-Content -Path $manifestPath -Value $manifest -Encoding UTF8

    Write-Host "Created package: $archivePath"
    Write-Host "Checksum file: $($checksum.Path)"
    Write-Host "Manifest file: $manifestPath"
}
finally {
    if (Test-Path -LiteralPath $stagingRoot) {
        Remove-Item -LiteralPath $stagingRoot -Recurse -Force
    }
}
