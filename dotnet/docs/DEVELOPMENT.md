# Vehimap Development Environment

This document describes the supported developer setup for Vehimap desktop and the experimental Android nightly. It is intended for contributors using a normal editor and command line; no AI service or proprietary builder is required.

## Supported Development Scope

The current solution contains the .NET 10 desktop application and targets these release runtime identifiers:

| Platform | Runtime identifier | Current output |
| --- | --- | --- |
| Windows x64 | `win-x64` | Inno Setup per-user installer |
| Linux x64 | `linux-x64` | self-contained `tar.gz` directory |
| macOS Intel | `osx-x64` | self-contained zipped `.app` |
| macOS Apple Silicon | `osx-arm64` | self-contained zipped `.app` |
| Android ARM64/x64 | `net10.0-android` | locally development-signed APK |

The Android project is deliberately kept in `Vehimap.Android.sln`, separate from the normal desktop `Vehimap.sln`. Desktop contributors therefore do not need the Android workload. iOS does not have a host project yet.

## Common Requirements

Install these tools on every development platform:

- Git.
- .NET 10 SDK. `dotnet/global.json` requests SDK `10.0.100` and allows a newer .NET 10 feature band.
- PowerShell 7 (`pwsh`) for repository build, packaging, readiness, and compliance scripts.
- An editor with C# support. VS Code with C# Dev Kit, JetBrains Rider, Visual Studio 2026 18.0 or later on Windows, or another editor is acceptable.
- Internet access for the first `dotnet restore` so NuGet dependencies can be downloaded.

Avalonia does not require a separate installation. `dotnet restore` downloads Avalonia 12 and all other NuGet packages declared by the solution.

Verify the common environment from the repository root:

```text
pwsh ./dotnet/build/Test-DotnetDeveloperEnvironment.ps1
cd dotnet
dotnet restore Vehimap.sln
dotnet build Vehimap.sln --configuration Release
dotnet test Vehimap.sln --configuration Release
```

The application is self-contained only after `dotnet publish`. Developers still need the .NET 10 SDK to build and test source code.

## Windows Development

### Required

- Windows 10 22H2 x64 or Windows 11 x64.
- Git, .NET 10 SDK, and PowerShell 7.

The basic tools can be installed with WinGet:

```powershell
winget install Git.Git
winget install Microsoft.DotNet.SDK.10
winget install Microsoft.PowerShell
```

No Visual Studio workload is required for CLI builds. If Visual Studio is used, .NET 10 requires Visual Studio 2026 version 18.0 or later. VS Code or Rider can use the same repository and CLI commands.

### Windows Installer Packaging

Install Inno Setup 7 to create or fully validate Windows setup packages. The packaging script locates `ISCC.exe` through `INNO_SETUP_COMPILER`, `PATH`, or the default Inno Setup 6/7 install directories.

Run the optional release-tools check with:

```powershell
pwsh ./dotnet/build/Test-DotnetDeveloperEnvironment.ps1 -IncludeReleaseTools
```

### Windows Appium UI Tests

The normal test suite can run without Appium; live desktop UI tests skip when the server is unavailable. To run the same UI smoke path as CI, install:

- Node.js 20 or later and npm.
- Appium and its Windows driver: `npm install -g appium`, then `appium driver install windows`.
- Microsoft WinAppDriver 1.2.1.
- Windows Developer Mode if required by WinAppDriver on the machine.

Then publish the application, start Appium on port 4723, and set the test variables:

```powershell
dotnet publish ./src/Vehimap.Desktop/Vehimap.Desktop.csproj -c Release -r win-x64 --self-contained true -o ./artifacts/desktop-release
appium --address 127.0.0.1 --port 4723
$env:VEHIMAP_APPIUM_SERVER_URL = "http://127.0.0.1:4723/"
$env:VEHIMAP_UI_APP = (Resolve-Path ./artifacts/desktop-release/Vehimap.exe)
$env:VEHIMAP_UI_REQUIRE_APPIUM = "1"
dotnet test ./tests/Vehimap.Tests.UI/Vehimap.Tests.UI.csproj -c Release
```

Use NVDA and Narrator for the documented manual accessibility matrix. Appium does not replace screen-reader testing.

## Linux Development

Vehimap currently publishes `linux-x64`. Avalonia 12 targets X11 directly; Wayland users need XWayland until the native Wayland backend becomes a supported path. Skia requires `glibc` 2.17 or later.

### Ubuntu And Debian

Install .NET 10 according to the current Microsoft instructions for the exact distribution release. On currently supported Ubuntu releases, the package is normally `dotnet-sdk-10.0`. Then install the desktop development dependencies:

```bash
sudo apt update
sudo apt install git dotnet-sdk-10.0 libx11-6 libice6 libsm6 libfontconfig1 xdg-utils tar
```

Install PowerShell 7 using the official Microsoft package instructions. A Wayland-only installation may also need `xwayland`.

### Fedora

```bash
sudo dnf install git dotnet-sdk-10.0 libX11 libICE libSM fontconfig xdg-utils tar
```

Install PowerShell 7 from the supported Microsoft package or repository. A Wayland-only installation may also need `xorg-x11-server-Xwayland`.

### Arch Linux

Arch is Avalonia Tier 3 and is not part of the official Vehimap acceptance matrix. It is still useful for community testing, but distribution-specific failures must be reproduced on a supported Tier 1 or Tier 2 distribution when possible.

```bash
sudo pacman -S --needed git dotnet-sdk libx11 libice libsm fontconfig xdg-utils tar
```

Install PowerShell 7 using a trusted Arch package source and verify that `dotnet --list-sdks` contains a .NET 10 SDK. A Wayland-only installation may also need `xorg-xwayland`.

### Linux Testing Notes

- Run `pwsh ./build/Test-DotnetDeveloperEnvironment.ps1` before the first build.
- Use `dotnet test Vehimap.sln -c Release` for unit, compatibility, static UI, localization, and accessibility contracts.
- Windows Appium tests are not currently a Linux UI automation path.
- Test the actual `linux-x64` application on native Linux; a Windows cross-publish only verifies package structure.
- For accessibility testing, install Orca and optionally Accerciser. Avalonia exposes Linux accessibility through AT-SPI2 when a D-Bus session and accessibility service are available.

## macOS Development

Use macOS 14 Sonoma or later and install the .NET 10 SDK matching the Mac architecture. Install Git and PowerShell 7; Xcode Command Line Tools provide Git if a separate Git package is not used.

Avalonia desktop uses its own native macOS backend and does not require the .NET macOS or Mac Catalyst workload. Basic desktop builds can also be cross-published, but native execution, VoiceOver validation, signing, and notarization require a Mac.

Build and test on a Mac with:

```text
pwsh ./build/Test-DotnetDeveloperEnvironment.ps1
cd dotnet
dotnet restore Vehimap.sln
dotnet test Vehimap.sln --configuration Release
dotnet publish ./src/Vehimap.Desktop/Vehimap.Desktop.csproj -c Release -r osx-arm64 --self-contained true -o ./artifacts/osx-arm64/desktop
```

Use `osx-x64` instead of `osx-arm64` on an Intel Mac. Xcode is optional for basic desktop compilation but required for signing/notarization and future iOS work. VoiceOver is built into macOS and remains a required manual accessibility test.

## Release And Test Tool Matrix

| Capability | Additional tools |
| --- | --- |
| Build and unit/static tests | common requirements only |
| Windows installer | Inno Setup 7 |
| Linux archive validation | `tar` |
| macOS native validation | physical/virtual Mac matching the target architecture |
| macOS signing and notarization | Xcode command line tools, Apple signing identity, notarization credentials |
| Windows live UI automation | Node.js 20+, Appium Windows driver, WinAppDriver 1.2.1 |
| Windows accessibility | NVDA and Narrator |
| Linux accessibility | Orca; Accerciser is recommended for tree inspection |
| macOS accessibility | VoiceOver |
| Android build | .NET Android workload, Android SDK API 36, JDK 21 |
| Android device test | Android 12/API 31 or newer device or emulator, `adb` |

## Android Development

Vehimap now has an experimental read-only Android nightly shell. It reads its own app-private SQLite data set and shares domain, storage, localization and known-value projection code with desktop Vehimap. It is a separate application, not a desktop RID and not yet a public Android release.

Android accessibility must be tested with TalkBack on a physical device. The current
Avalonia 12.0.4 backend exposes names and actions but has a documented standard-role
announcement limitation; see `docs/ACCESSIBILITY.md`. Every Avalonia package upgrade
must repeat the button, selectable-card/list and tab-role smoke and update the verified
framework version before the upgrade is accepted.

The current baseline is:

- official Microsoft .NET 10 SDK;
- Android workload installed with `dotnet workload install android`;
- Android Studio or Android command-line tools with SDK platform API 36, build tools and platform tools;
- JDK 21; Android Studio's bundled JetBrains Runtime is supported;
- Android 12/API 31 or newer device or emulator;
- USB debugging and an accepted computer RSA key for physical-device installation.

On Windows the readiness script automatically checks common locations such as `%LOCALAPPDATA%\Android\Sdk` and `C:\Program Files\Android\Android Studio\jbr`. Explicit paths can also be passed to the scripts.

Verify the Android environment and build the APK from the repository root:

```powershell
pwsh ./dotnet/build/Test-DotnetDeveloperEnvironment.ps1 -IncludeAndroidTools
pwsh ./dotnet/build/Test-DotnetAndroidNightlyReadiness.ps1
```

The easy-to-find APK is written to `dotnet/artifacts/nightly/android/app/Vehimap.apk`. It uses the separate package id `cz.vlcekapps.vehimap.nightly`, includes ARM64 and x86-64 native libraries and is signed with a local development key. It is suitable for local testing, not store distribution.

Install and launch it on an authorized connected device:

```powershell
$adb = "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe"
& $adb devices -l
& $adb install -r ./dotnet/artifacts/nightly/android/app/Vehimap.apk
& $adb shell monkey -p cz.vlcekapps.vehimap.nightly -c android.intent.category.LAUNCHER 1
```

Build every current local nightly target with one shared version:

```powershell
pwsh ./dotnet/build/Build-DotnetLocalNightlies.ps1
```

This creates desktop packages for `win-x64`, `linux-x64`, `osx-x64`, `osx-arm64` and the Android APK. Windows can cross-publish the Linux and macOS packages, but native execution, accessibility and platform integration still require their respective operating systems.

On Linux, distribution-provided .NET packages may not include mobile workload support. Use the official Microsoft SDK if `dotnet workload install android` fails.

## Future iOS Development

iOS tooling is not required until an iOS project is added. Running, testing, signing, and shipping iOS requires:

- a supported Mac with current Xcode;
- .NET 10 SDK and `dotnet workload install ios`;
- an iOS simulator or physical device;
- Apple signing and provisioning for device or store distribution.

Source may be compiled in limited scenarios elsewhere, but final iOS testing and release work cannot be completed without macOS and Xcode.

## Authoritative References

- [Avalonia supported platforms](https://docs.avaloniaui.net/docs/supported-platforms)
- [Avalonia desktop Linux guide](https://docs.avaloniaui.net/docs/platform-specific-guides/linux)
- [Avalonia Android setup](https://docs.avaloniaui.net/docs/platform-specific-guides/android)
- [Avalonia iOS setup](https://docs.avaloniaui.net/docs/platform-specific-guides/ios)
- [.NET installation documentation](https://learn.microsoft.com/dotnet/core/install/)
- [.NET 10 supported operating systems](https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md)
- [PowerShell installation documentation](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)

Platform support changes over time. Update this document together with Avalonia, .NET, target framework, runtime identifier, or packaging changes.
