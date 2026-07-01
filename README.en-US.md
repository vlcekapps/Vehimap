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

The currently supported public version is the Windows desktop application.

- Windows 10 or Windows 11
- 64-bit system
- standard user account; administrator rights are not required for the default installation

Other platforms are part of the long-term plan, but regular users should currently use the Windows version.

## Installation

1. Open the [Releases](https://github.com/vlcekapps/Vehimap/releases) page.
2. Download the Windows installer.
3. Run the installer and complete the installation.
4. Open Vehimap from the Start menu or the desktop shortcut.

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
- [Migration plan](dotnet/docs/MIGRATION.md)
- [Accessibility](dotnet/docs/ACCESSIBILITY.md)
- [Localization](dotnet/docs/I18N.md)
- [Release process](RELEASE.md)
