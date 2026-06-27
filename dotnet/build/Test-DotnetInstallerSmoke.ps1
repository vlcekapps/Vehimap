param(
    [Parameter(Mandatory = $true)]
    [string]$InstallerPath,

    [string]$PackageMetadataPath,

    [string]$InstallRoot,

    [switch]$Install,

    [int]$LaunchSeconds = 8
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-FullPath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Test-ChecksumFile {
    param([string]$Path)

    $checksumPath = "$Path.sha256"
    if (-not (Test-Path -LiteralPath $checksumPath -PathType Leaf)) {
        throw "Checksum soubor '$checksumPath' neexistuje."
    }

    $checksumLine = (Get-Content -LiteralPath $checksumPath | Select-Object -First 1).Trim()
    $expectedHash = ($checksumLine -split "\s+")[0].Trim().ToLowerInvariant()
    if ($expectedHash -notmatch "^[0-9a-f]{64}$") {
        throw "Checksum soubor '$checksumPath' neobsahuje platny SHA-256 hash."
    }

    $actualHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
    if ($actualHash -ne $expectedHash) {
        throw "Checksum '$checksumPath' neodpovida instalatoru '$Path'."
    }

    Write-Host "Checksum OK: $expectedHash"
    return $expectedHash
}

function Test-PackageMetadata {
    param(
        [string]$Path,
        [string]$InstallerFullPath,
        [string]$ExpectedHash
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        Write-Host "Metadata nebyla zadana; kontroluji jen instalator a checksum."
        return
    }

    $metadataFullPath = Resolve-FullPath -Path $Path
    if (-not (Test-Path -LiteralPath $metadataFullPath -PathType Leaf)) {
        throw "Metadata '$metadataFullPath' neexistuji."
    }

    $metadata = Get-Content -Raw -LiteralPath $metadataFullPath | ConvertFrom-Json
    if ($metadata.assetKind -ne "installer") {
        throw "Metadata '$metadataFullPath' nema assetKind=installer."
    }

    if ($metadata.runtimeIdentifier -notlike "win-*") {
        throw "Metadata '$metadataFullPath' nejsou pro Windows runtime."
    }

    if ($metadata.packageFile -ne (Split-Path -Path $InstallerFullPath -Leaf)) {
        throw "Metadata '$metadataFullPath' neukazuji na zadany instalator."
    }

    if ($metadata.sha256 -ne $ExpectedHash) {
        throw "Metadata '$metadataFullPath' obsahuji jiny SHA-256 hash."
    }

    $installerInfo = Get-Item -LiteralPath $InstallerFullPath
    if ([long]$metadata.packageSize -ne $installerInfo.Length) {
        throw "Metadata '$metadataFullPath' obsahuji jinou velikost instalatoru."
    }

    if ($metadata.channel -notin @("stable", "beta", "nightly")) {
        throw "Metadata '$metadataFullPath' obsahuji nepodporovany channel '$($metadata.channel)'."
    }

    Write-Host "Metadata OK: $($metadata.channel) $($metadata.version) $($metadata.runtimeIdentifier)"
}

if (-not [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
    throw "Installer smoke lze spustit jen na Windows."
}

$installerFullPath = Resolve-FullPath -Path $InstallerPath
if (-not (Test-Path -LiteralPath $installerFullPath -PathType Leaf)) {
    throw "Instalator '$installerFullPath' neexistuje."
}

if ([System.IO.Path]::GetExtension($installerFullPath) -ine ".exe") {
    throw "Windows installer smoke ocekava .exe instalator."
}

Write-Host "Vehimap Windows installer smoke"
Write-Host "Installer: $installerFullPath"

$hash = Test-ChecksumFile -Path $installerFullPath
Test-PackageMetadata -Path $PackageMetadataPath -InstallerFullPath $installerFullPath -ExpectedHash $hash

if (-not $Install) {
    Write-Host "Install smoke byl preskocen. Pro tichou izolovanou instalaci pridejte -Install."
    exit 0
}

if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("vehimap-installer-smoke-" + [guid]::NewGuid().ToString("N"))
}

$installFullPath = Resolve-FullPath -Path $InstallRoot
$installLogPath = Join-Path $installFullPath "install.log"
$portableDataPath = Join-Path $installFullPath "data"
$desktopExePath = Join-Path $installFullPath "Vehimap.Desktop.exe"
$uninstallerPath = Join-Path $installFullPath "unins000.exe"

if (Test-Path -LiteralPath $installFullPath) {
    Remove-Item -LiteralPath $installFullPath -Recurse -Force
}

New-Item -ItemType Directory -Path $installFullPath -Force | Out-Null

try {
    $installerArguments = @(
        "/VERYSILENT",
        "/SUPPRESSMSGBOXES",
        "/NORESTART",
        "/NOICONS",
        "/DIR=""$installFullPath""",
        "/LOG=""$installLogPath"""
    )

    Write-Host "Instaluji do izolovane slozky: $installFullPath"
    $installProcess = Start-Process -FilePath $installerFullPath -ArgumentList $installerArguments -Wait -PassThru
    if ($installProcess.ExitCode -ne 0) {
        throw "Instalator skoncil s kodem $($installProcess.ExitCode). Log: $installLogPath"
    }

    if (-not (Test-Path -LiteralPath $desktopExePath -PathType Leaf)) {
        throw "Po instalaci chybi '$desktopExePath'."
    }

    New-Item -ItemType Directory -Path $portableDataPath -Force | Out-Null
    Write-Host "Spoustim nainstalovanou aplikaci v portable rezimu."
    $appProcess = Start-Process -FilePath $desktopExePath -WorkingDirectory $installFullPath -PassThru
    Start-Sleep -Seconds ([Math]::Max(1, $LaunchSeconds))
    if ($appProcess.HasExited) {
        throw "Nainstalovana aplikace skoncila prilis brzy s kodem $($appProcess.ExitCode)."
    }

    Stop-Process -Id $appProcess.Id -Force
    Wait-Process -Id $appProcess.Id -Timeout 10 -ErrorAction SilentlyContinue
    Write-Host "Nainstalovana aplikace se spustila a byla ukoncena."

    if (-not (Test-Path -LiteralPath $uninstallerPath -PathType Leaf)) {
        throw "Po instalaci chybi odinstalator '$uninstallerPath'."
    }

    $uninstallLogPath = Join-Path $installFullPath "uninstall.log"
    $uninstallProcess = Start-Process `
        -FilePath $uninstallerPath `
        -ArgumentList @("/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART", "/LOG=""$uninstallLogPath""") `
        -Wait `
        -PassThru

    if ($uninstallProcess.ExitCode -ne 0) {
        throw "Odinstalator skoncil s kodem $($uninstallProcess.ExitCode). Log: $uninstallLogPath"
    }

    Write-Host "Installer smoke OK."
}
finally {
    if (Test-Path -LiteralPath $installFullPath) {
        Remove-Item -LiteralPath $installFullPath -Recurse -Force -ErrorAction SilentlyContinue
    }
}
