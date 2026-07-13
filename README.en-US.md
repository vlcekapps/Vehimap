# Vehimap

Vehimap is an application for clear vehicle record keeping. It helps you track roadworthiness inspections, insurance validity, service, documents, fuel, costs, reminders, and other important information about a car, bus, motorcycle, or another vehicle.

The application is also built with accessibility for disabled users in mind. It can be operated with a keyboard and is continuously tested with screen readers, especially NVDA on Windows.

## Contents

- [Česká verze](README.md)
- [Who Vehimap Is For](#who-vehimap-is-for)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Key Features](#key-features)
- [Data And Privacy](#data-and-privacy)
- [License](#license)
- [Support And Feedback](#support-and-feedback)
- [Developer Documentation](#developer-documentation)

## Who Vehimap Is For

Vehimap is useful when you want to keep the following in one place:

- your vehicle list,
- inspection and insurance dates,
- service history and maintenance plans,
- documents and attachments,
- fuel, consumption, and costs,
- reminders and an audit of missing or suspicious data.

Your data is stored locally on your computer. Vehimap is not a cloud service and does not send your vehicle records anywhere unless you explicitly decide to do so.

## System Requirements

Published desktop packages are self-contained and include the required .NET runtime. Regular users do not need to install the .NET SDK, .NET Runtime, Avalonia, or PowerShell.

### Windows

- Windows 10 22H2 x64 or Windows 11 x64
- standard user account; administrator rights are not required for the default per-user installation
- Windows is the primary supported and accessibility-validated platform

### macOS

- macOS 14 Sonoma or later
- the `osx-arm64` package for Apple Silicon, such as M1 through M4, or `osx-x64` for an Intel Mac
- no separate .NET installation is required

macOS packages currently pass build and structural smoke checks but still await complete native validation, Apple signing, and notarization. Treat them as test packages until that work is complete; macOS may require the first launch to be explicitly allowed in Privacy & Security settings.

### Linux

- 64-bit x86 system (`linux-x64`) with `glibc` 2.17 or later
- an X11 or XWayland graphical session; a native Wayland backend is not currently a supported Vehimap path
- Ubuntu 25.x, Fedora 43, and Debian 13 are Avalonia 12 Tier 1; older supported releases are Tier 2 and Arch Linux is Tier 3

On Ubuntu or Debian, install the desktop libraries with:

```bash
sudo apt update
sudo apt install libx11-6 libice6 libsm6 libfontconfig1 xdg-utils
```

On Fedora:

```bash
sudo dnf install libX11 libICE libSM fontconfig xdg-utils
```

On Arch Linux:

```bash
sudo pacman -S --needed libx11 libice libsm fontconfig xdg-utils
```

A Wayland-only installation may also need XWayland: package `xwayland` on Ubuntu/Debian, `xorg-x11-server-Xwayland` on Fedora, or `xorg-xwayland` on Arch. Minimal distributions must also provide the standard native .NET dependencies, particularly ICU, OpenSSL, `libstdc++`, zlib, Kerberos, certificates, and timezone data. Full desktop installations normally include them already.

The current platform tiers and native libraries come from the [official Avalonia Supported Platforms documentation](https://docs.avaloniaui.net/docs/supported-platforms). ICU, OpenSSL, and other base dependencies for minimal Linux installations are maintained in the [.NET Linux installation documentation](https://learn.microsoft.com/dotnet/core/install/linux).

### Android

- Android 12 (API 31) or later
- an ARM64 phone or tablet; the local development APK also carries x86-64 libraries for an emulator
- no separate .NET or Avalonia installation is required

Android is currently an experimental local nightly for development and testing. It is read-only and currently exposes the vehicle list and vehicle details in its own app data set; a publicly signed Android package is not available yet.

## Installation

### Windows

1. Open the [Releases](https://github.com/vlcekapps/Vehimap/releases) page.
2. Download the `win-x64-setup.exe` installer.
3. Run the installer and complete the installation.
4. Open Vehimap from the Start menu or the desktop shortcut.

### macOS

1. Download the `osx-arm64` or `osx-x64` ZIP for your processor.
2. Extract `Vehimap.app` and move it to Applications.
3. If Gatekeeper blocks the test package, explicitly allow it in Privacy & Security settings.

### Linux

1. Install the system libraries listed above and download the `linux-x64.tar.gz` archive.
2. Extract it, open the resulting directory, and run the `Vehimap` file.
3. If the executable bit was not preserved, run `chmod +x Vehimap` followed by `./Vehimap`.

### Android

At this stage the Android APK is installed locally through developer tools and USB debugging only. Regular users should wait for the first publicly signed mobile release; contributors can follow [dotnet/docs/DEVELOPMENT.md](dotnet/docs/DEVELOPMENT.md).

For everyday use, choose a stable release. Nightly builds are intended for braver testers and may contain work in progress.

## Quick Start

1. After launch, choose `Vehicle` -> `Add Vehicle`.
2. Fill in the name, category, license plate, and any other details you want to track.
3. Add the next roadworthiness inspection and insurance end date to get basic deadline tracking.
4. Use the vehicle tabs to add history, fuel records, documents, reminders, and maintenance.
5. Use the overview screens, dashboard, data audit, and smart advisor to find important or missing information.
6. Back up your data regularly, especially before major changes or updates.

## Key Features

### Vehicle Records

Vehimap can manage multiple vehicles at once. Each vehicle can have basic details, status, notes, dates, history, and related records.

### Reminders And Deadlines

The application tracks important deadlines such as roadworthiness inspections, insurance validity, reminders, and service maintenance. Deadlines can be shown in overviews and exported to a calendar.

### Documents And Attachments

You can attach documents and files to a vehicle. Vehimap supports both external file paths and managed attachments stored in the application's data folder.

### Service And Maintenance

Vehimap includes maintenance plans, service history, and a service book. This is useful for everyday vehicles, vintage vehicles, and company or work vehicles.

### Fuel And Costs

Vehimap records fuel entries, fuel location, fuel details, total price, and odometer values. It can calculate consumption, price per liter, and warn about suspicious records.

### Data Audit And Smart Advisor

The data audit finds missing or suspicious information. The smart advisor builds recommendations from existing data and helps you decide what to handle first.

### Backup And Restore

Data can be exported to a backup and restored later. Newer Vehimap versions use a local database and can safely migrate older data when upgrading.

### Accessibility

Vehimap is designed as a keyboard-first application. Important screens provide keyboard control, screen-reader-friendly labels, and separate dialogs for editing records.

Vehimap 2.0 also has an ACR-ready evidence draft in progress for future customer accessibility review. This is not a formal conformance statement yet; that requires completed manual assistive-technology validation.

## Data And Privacy

Vehimap stores data locally in the data folder of the selected installation channel. The application does not use cloud synchronization, and your vehicle records stay on your device.

When upgrading from an older version, the original data is backed up automatically and migrated to the new data set. After a verified migration, the original files are moved into a migration backup so normal work can continue with the new format.

## License

Vehimap is free software licensed under `GPL-3.0-or-later`.

Copyright: Pavel Vlček

Release packages also include information about third-party libraries in `THIRD-PARTY-NOTICES.md`.

## Support And Feedback

You can report bugs, suggestions, and feedback through [GitHub Issues](https://github.com/vlcekapps/Vehimap/issues).

If you want to thank the author, the application includes a `Thank the author` item that opens a voluntary support page.

## Developer Documentation

This file is intended for regular users. Technical information for development, builds, tests, data migration, accessibility, and localization is available in separate documentation:

- [Developer README](dotnet/README.md)
- [Development environment requirements](dotnet/docs/DEVELOPMENT.md)
- [Contributing guide](CONTRIBUTING.md)
- [Migration plan](dotnet/docs/MIGRATION.md)
- [Accessibility](dotnet/docs/ACCESSIBILITY.md)
- [Localization](dotnet/docs/I18N.md)
- [Release process](RELEASE.md)
