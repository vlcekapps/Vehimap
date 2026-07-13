# SPDX-License-Identifier: GPL-3.0-or-later
[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$EffectiveVersion,
    [int]$AndroidVersionCode,
    [string]$AndroidSdkDirectory,
    [string]$JavaSdkDirectory,
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $dotnetRoot
$versionPath = Join-Path $repositoryRoot "src\VERSION"
$projectPath = Join-Path $dotnetRoot "src\Vehimap.Android\Vehimap.Android.csproj"
$packageId = "cz.vlcekapps.vehimap.nightly"
$minimumApi = 31
$targetApi = 36
$runningOnWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [System.Runtime.InteropServices.OSPlatform]::Windows)
$runningOnMacOS = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [System.Runtime.InteropServices.OSPlatform]::OSX)

function Resolve-ExistingDirectory {
    param(
        [string]$RequestedPath,
        [string[]]$Candidates,
        [string]$Description
    )

    foreach ($candidate in @($RequestedPath) + $Candidates) {
        if (-not [string]::IsNullOrWhiteSpace($candidate) -and
            (Test-Path -LiteralPath $candidate -PathType Container)) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw "$Description was not found. Pass its path explicitly or configure the standard environment variable."
}

function Assert-SafeArtifactDirectory {
    param([Parameter(Mandatory = $true)][string]$Path)

    $allowedRoot = [System.IO.Path]::GetFullPath((Join-Path $dotnetRoot "artifacts\nightly"))
    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $allowedPrefix = $allowedRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith($allowedPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean Android artifacts outside '$allowedRoot': '$fullPath'."
    }
}

if (-not (Test-Path -LiteralPath $versionPath -PathType Leaf)) {
    throw "Version file '$versionPath' does not exist."
}

$baseVersion = (Get-Content -LiteralPath $versionPath | Select-Object -First 1).Trim()
if ([string]::IsNullOrWhiteSpace($EffectiveVersion)) {
    $timestamp = [DateTime]::UtcNow.ToString("yyyyMMddHHmmss", [System.Globalization.CultureInfo]::InvariantCulture)
    $EffectiveVersion = "$baseVersion-nightly.local.$timestamp"
}

if ($AndroidVersionCode -le 0) {
    $epoch = [DateTime]::SpecifyKind([DateTime]::ParseExact(
        "202001010000",
        "yyyyMMddHHmm",
        [System.Globalization.CultureInfo]::InvariantCulture), [DateTimeKind]::Utc)
    $AndroidVersionCode = [int][Math]::Floor(([DateTime]::UtcNow - $epoch).TotalMinutes)
}

$workloadOutput = @(& dotnet workload list 2>&1)
if ($LASTEXITCODE -ne 0 -or -not ($workloadOutput | Where-Object { $_ -match '^android\s' })) {
    throw "The .NET Android workload is missing. Run 'dotnet workload install android'."
}

$sdkCandidates = [System.Collections.Generic.List[string]]::new()
if (-not [string]::IsNullOrWhiteSpace($env:ANDROID_HOME)) { $sdkCandidates.Add($env:ANDROID_HOME) }
if (-not [string]::IsNullOrWhiteSpace($env:ANDROID_SDK_ROOT)) { $sdkCandidates.Add($env:ANDROID_SDK_ROOT) }
if (-not [string]::IsNullOrWhiteSpace($env:LOCALAPPDATA)) { $sdkCandidates.Add((Join-Path $env:LOCALAPPDATA "Android\Sdk")) }
$resolvedAndroidSdk = Resolve-ExistingDirectory $AndroidSdkDirectory $sdkCandidates "Android SDK"

$androidJar = Join-Path $resolvedAndroidSdk "platforms\android-$targetApi\android.jar"
if (-not (Test-Path -LiteralPath $androidJar -PathType Leaf)) {
    throw "Android SDK platform $targetApi is missing from '$resolvedAndroidSdk'."
}

$javaCandidates = [System.Collections.Generic.List[string]]::new()
if (-not [string]::IsNullOrWhiteSpace($env:JAVA_HOME)) { $javaCandidates.Add($env:JAVA_HOME) }
if ($runningOnWindows) {
    $javaCandidates.Add("C:\Program Files\Android\Android Studio\jbr")
    $javaCandidates.Add("C:\nvgt\android-tools\java17")
}
elseif ($runningOnMacOS) {
    $javaCandidates.Add("/Applications/Android Studio.app/Contents/jbr/Contents/Home")
}
else {
    $javaCandidates.Add((Join-Path $HOME "android-studio\jbr"))
}
$resolvedJavaSdk = Resolve-ExistingDirectory $JavaSdkDirectory $javaCandidates "JDK"
$javaBinaryName = if ($runningOnWindows) { "bin\java.exe" } else { "bin/java" }
$javaExecutable = Join-Path $resolvedJavaSdk $javaBinaryName
if (-not (Test-Path -LiteralPath $javaExecutable -PathType Leaf)) {
    throw "Java executable was not found in '$resolvedJavaSdk'."
}

if (-not $SkipTests) {
    & dotnet test (Join-Path $dotnetRoot "Vehimap.sln") --configuration $Configuration -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host "Vehimap Android nightly readiness"
Write-Host "Version: $EffectiveVersion ($AndroidVersionCode)"
Write-Host "Android SDK: $resolvedAndroidSdk"
Write-Host "JDK: $resolvedJavaSdk"

$buildArguments = @(
    "build",
    $projectPath,
    "--configuration", $Configuration,
    "--framework", "net10.0-android",
    "-p:UseSharedCompilation=false",
    "-p:VehimapReleaseChannel=nightly",
    "-p:VehimapVersion=$EffectiveVersion",
    "-p:VehimapAndroidVersionCode=$AndroidVersionCode",
    "-p:AndroidSdkDirectory=$resolvedAndroidSdk",
    "-p:JavaSdkDirectory=$resolvedJavaSdk"
)
& dotnet @buildArguments
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$androidOutput = Join-Path $dotnetRoot "src\Vehimap.Android\bin\$Configuration\net10.0-android"
$signedApk = Get-ChildItem -LiteralPath $androidOutput -Filter "*-Signed.apk" -File |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1
if ($null -eq $signedApk) {
    throw "The signed Android APK was not created in '$androidOutput'."
}

$artifactRoot = Join-Path $dotnetRoot "artifacts\nightly\android"
$appDirectory = Join-Path $artifactRoot "app"
$releaseDirectory = Join-Path $artifactRoot "release"
foreach ($directory in @($appDirectory, $releaseDirectory)) {
    Assert-SafeArtifactDirectory $directory
    if (Test-Path -LiteralPath $directory) {
        Remove-Item -LiteralPath $directory -Recurse -Force
    }
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
}

$appApkPath = Join-Path $appDirectory "Vehimap.apk"
$releaseFileName = "vehimap-android-nightly-$EffectiveVersion.apk"
$releaseApkPath = Join-Path $releaseDirectory $releaseFileName
Copy-Item -LiteralPath $signedApk.FullName -Destination $appApkPath -Force
Copy-Item -LiteralPath $signedApk.FullName -Destination $releaseApkPath -Force

$buildTools = Get-ChildItem -LiteralPath (Join-Path $resolvedAndroidSdk "build-tools") -Directory |
    Sort-Object Name -Descending
$aapt2 = $null
foreach ($buildTool in $buildTools) {
    $aapt2FileName = if ($runningOnWindows) { "aapt2.exe" } else { "aapt2" }
    $candidate = Join-Path $buildTool.FullName $aapt2FileName
    if (Test-Path -LiteralPath $candidate -PathType Leaf) {
        $aapt2 = $candidate
        break
    }
}
if ([string]::IsNullOrWhiteSpace($aapt2)) {
    throw "aapt2 was not found under '$resolvedAndroidSdk\build-tools'."
}

$badging = (& $aapt2 dump badging $releaseApkPath 2>&1) -join "`n"
if ($LASTEXITCODE -ne 0) { throw "aapt2 could not inspect '$releaseApkPath'." }
if ($badging -notmatch "package: name='$([regex]::Escape($packageId))'") {
    throw "APK package id is not '$packageId'."
}
if ($badging -notmatch "versionCode='$AndroidVersionCode'" -or
    $badging -notmatch "versionName='$([regex]::Escape($EffectiveVersion))'") {
    throw "APK version metadata does not match '$EffectiveVersion' ($AndroidVersionCode)."
}
if ($badging -notmatch "sdkVersion:'$minimumApi'" -or $badging -notmatch "targetSdkVersion:'$targetApi'") {
    throw "APK API metadata does not match min $minimumApi / target $targetApi."
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($releaseApkPath)
try {
    $entryNames = @($archive.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
    foreach ($requiredEntry in @(
        "assets/legal/LICENSE",
        "assets/legal/THIRD-PARTY-NOTICES.md"
    )) {
        if ($requiredEntry -notin $entryNames) {
            throw "APK is missing required legal asset '$requiredEntry'."
        }
    }

    foreach ($abi in @("arm64-v8a", "x86_64")) {
        if (-not ($entryNames | Where-Object { $_ -like "lib/$abi/*.so" })) {
            throw "APK is missing native libraries for '$abi'."
        }
    }
}
finally {
    $archive.Dispose()
}

$package = Get-Item -LiteralPath $releaseApkPath
$hash = (Get-FileHash -LiteralPath $releaseApkPath -Algorithm SHA256).Hash.ToLowerInvariant()
$checksumPath = "$releaseApkPath.sha256"
Set-Content -LiteralPath $checksumPath -Value "$hash  $releaseFileName" -Encoding utf8
$metadataPath = Join-Path $releaseDirectory "vehimap-android-nightly-$EffectiveVersion.json"
[ordered]@{
    version = $EffectiveVersion
    versionCode = $AndroidVersionCode
    channel = "nightly"
    platform = "android"
    packageId = $packageId
    minimumApi = $minimumApi
    targetApi = $targetApi
    packageFile = $releaseFileName
    sha256 = $hash
    size = $package.Length
    architectures = @("arm64-v8a", "x86_64")
    signing = "local-development"
} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $metadataPath -Encoding utf8

Write-Host "Android nightly readiness OK"
Write-Host "APK: $appApkPath"
Write-Host "Versioned APK: $releaseApkPath"
Write-Host "Metadata: $metadataPath"
