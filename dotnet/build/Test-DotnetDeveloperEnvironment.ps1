# SPDX-License-Identifier: GPL-3.0-or-later
[CmdletBinding()]
param(
    [switch]$IncludeReleaseTools,
    [switch]$IncludeWindowsUiTools
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$failures = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()
$successes = [System.Collections.Generic.List[string]]::new()

function Add-Success {
    param([Parameter(Mandatory = $true)][string]$Message)
    $successes.Add($Message)
}

function Add-Warning {
    param([Parameter(Mandatory = $true)][string]$Message)
    $warnings.Add($Message)
}

function Add-Failure {
    param([Parameter(Mandatory = $true)][string]$Message)
    $failures.Add($Message)
}

function Find-CommandPath {
    param([Parameter(Mandatory = $true)][string]$Name)

    $command = Get-Command $Name -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -eq $command) {
        return $null
    }

    if (-not [string]::IsNullOrWhiteSpace($command.Path)) {
        return $command.Path
    }

    return $command.Source
}

function Test-RequiredCommand {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $path = Find-CommandPath $Name
    if ([string]::IsNullOrWhiteSpace($path)) {
        Add-Failure "$Description was not found in PATH ($Name)."
        return $null
    }

    Add-Success "${Description}: $path"
    return $path
}

function Find-InnoSetupCompiler {
    if (-not [string]::IsNullOrWhiteSpace($env:INNO_SETUP_COMPILER) -and
        (Test-Path -LiteralPath $env:INNO_SETUP_COMPILER -PathType Leaf)) {
        return (Resolve-Path -LiteralPath $env:INNO_SETUP_COMPILER).Path
    }

    $commandPath = Find-CommandPath "ISCC.exe"
    if (-not [string]::IsNullOrWhiteSpace($commandPath)) {
        return $commandPath
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

    return $null
}

function Find-WinAppDriver {
    $commandPath = Find-CommandPath "WinAppDriver.exe"
    if (-not [string]::IsNullOrWhiteSpace($commandPath)) {
        return $commandPath
    }

    $candidates = [System.Collections.Generic.List[string]]::new()
    if (-not [string]::IsNullOrWhiteSpace($env:ProgramFiles)) {
        $candidates.Add((Join-Path $env:ProgramFiles "Windows Application Driver\WinAppDriver.exe"))
    }

    $programFilesX86 = ${env:ProgramFiles(x86)}
    if (-not [string]::IsNullOrWhiteSpace($programFilesX86)) {
        $candidates.Add((Join-Path $programFilesX86 "Windows Application Driver\WinAppDriver.exe"))
    }

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return $candidate
        }
    }

    return $null
}

if ($PSVersionTable.PSEdition -eq "Core" -and $PSVersionTable.PSVersion.Major -ge 7) {
    Add-Success "PowerShell $($PSVersionTable.PSVersion) ($($PSVersionTable.PSEdition))"
}
else {
    Add-Failure "PowerShell 7 or later is required. Run this script with pwsh, not Windows PowerShell."
}

$gitPath = Test-RequiredCommand "git" "Git"
$dotnetPath = Test-RequiredCommand "dotnet" ".NET CLI"

if (-not [string]::IsNullOrWhiteSpace($dotnetPath)) {
    $sdkOutput = @(& $dotnetPath --list-sdks 2>&1)
    if ($LASTEXITCODE -ne 0) {
        Add-Failure "dotnet --list-sdks failed."
    }
    elseif ($sdkOutput | Where-Object { $_ -match '^\s*10\.' }) {
        $net10Sdks = ($sdkOutput | Where-Object { $_ -match '^\s*10\.' }) -join ", "
        Add-Success ".NET 10 SDK: $net10Sdks"
    }
    else {
        Add-Failure ".NET 10 SDK was not found. Installed SDKs: $($sdkOutput -join ', ')"
    }
}

$runningOnWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [System.Runtime.InteropServices.OSPlatform]::Windows)
$runningOnLinux = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [System.Runtime.InteropServices.OSPlatform]::Linux)
$runningOnMacOS = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
    [System.Runtime.InteropServices.OSPlatform]::OSX)

if ($runningOnLinux) {
    $xdgOpenPath = Test-RequiredCommand "xdg-open" "xdg-utils file launcher"

    $ldconfigPath = Find-CommandPath "ldconfig"
    if ([string]::IsNullOrWhiteSpace($ldconfigPath)) {
        foreach ($candidate in @("/sbin/ldconfig", "/usr/sbin/ldconfig")) {
            if (Test-Path -LiteralPath $candidate -PathType Leaf) {
                $ldconfigPath = $candidate
                break
            }
        }
    }

    if ([string]::IsNullOrWhiteSpace($ldconfigPath)) {
        Add-Warning "ldconfig was not found; native Avalonia libraries could not be verified automatically."
    }
    else {
        $libraryCache = (& $ldconfigPath -p 2>$null) -join "`n"
        foreach ($library in @("libX11.so.6", "libICE.so.6", "libSM.so.6", "libfontconfig.so.1")) {
            if ($libraryCache -match [regex]::Escape($library)) {
                Add-Success "Linux native library: $library"
            }
            else {
                Add-Failure "Linux native library is missing: $library"
            }
        }
    }

    if ([string]::IsNullOrWhiteSpace($env:DISPLAY) -and [string]::IsNullOrWhiteSpace($env:WAYLAND_DISPLAY)) {
        Add-Warning "No DISPLAY or WAYLAND_DISPLAY is set. Builds can run, but the desktop application cannot be tested in this session."
    }
}

if ($IncludeReleaseTools) {
    if ($runningOnWindows) {
        $innoSetupCompiler = Find-InnoSetupCompiler
        if ([string]::IsNullOrWhiteSpace($innoSetupCompiler)) {
            Add-Failure "Inno Setup 6 or 7 compiler was not found. Install Inno Setup 7 or set INNO_SETUP_COMPILER."
        }
        else {
            Add-Success "Inno Setup compiler: $innoSetupCompiler"
        }
    }
    elseif ($runningOnLinux) {
        $tarPath = Test-RequiredCommand "tar" "tar archive tool"
    }
    elseif ($runningOnMacOS) {
        $xcodeSelectPath = Test-RequiredCommand "xcode-select" "Xcode command line selector"
        $codesignPath = Test-RequiredCommand "codesign" "Apple code signing tool"
        $xcrunPath = Test-RequiredCommand "xcrun" "Xcode command runner"
    }
}

if ($IncludeWindowsUiTools) {
    if (-not $runningOnWindows) {
        Add-Failure "The current live Appium desktop UI suite is Windows-only."
    }
    else {
        $nodePath = Test-RequiredCommand "node" "Node.js"
        $npmPath = Test-RequiredCommand "npm" "npm"
        $appiumPath = Test-RequiredCommand "appium" "Appium"
        $winAppDriverPath = Find-WinAppDriver
        if ([string]::IsNullOrWhiteSpace($winAppDriverPath)) {
            Add-Failure "Microsoft WinAppDriver 1.2.1 was not found."
        }
        else {
            Add-Success "WinAppDriver: $winAppDriverPath"
        }
    }
}

Write-Host "Vehimap developer environment"
foreach ($message in $successes) {
    Write-Host "[OK] $message"
}

foreach ($message in $warnings) {
    Write-Warning $message
}

foreach ($message in $failures) {
    Write-Host "[FAIL] $message" -ForegroundColor Red
}

if ($failures.Count -gt 0) {
    throw "Vehimap developer environment check failed with $($failures.Count) problem(s)."
}

Write-Host "Vehimap developer environment check passed."
