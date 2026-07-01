# Third-party notices for Vehimap

Generated: **2026-07-01**

Project license: **GNU GPL v3 or later** (`GPL-3.0-or-later`).

Copyright (C) 2026 Pavel Vlček.

This file lists third-party components used by the .NET desktop application and the Windows `win-x64` self-contained release. It separates runtime/shipped components from development-only test tooling. Before publishing a new release, run `dotnet list dotnet\Vehimap.sln package --include-transitive` and verify the publish output for the target runtime identifier.

## Runtime and shipped components

| Component | Version observed | License | Notes |
|---|---:|---|---|
| .NET runtime and framework assemblies | 10.0.x release toolchain | MIT | Self-contained desktop releases include Microsoft .NET runtime/framework components. |
| Avalonia | 12.0.4 | MIT | Avalonia UI framework, including desktop UI assemblies. Copyright 2013-2026 The AvaloniaUI Project. |
| Avalonia.Desktop | 12.0.4 | MIT | Avalonia desktop host package. |
| Avalonia.Themes.Fluent | 12.0.4 | MIT | Avalonia Fluent theme package. |
| Avalonia.FreeDesktop | 12.0.4 | MIT | Transitive Avalonia package used for desktop platform support. |
| Avalonia.FreeDesktop.AtSpi | 12.0.4 | MIT | Transitive Avalonia accessibility/platform package. |
| Avalonia.HarfBuzz | 12.0.4 | MIT | Transitive Avalonia text shaping integration. |
| Avalonia.Native | 12.0.4 | MIT | Transitive Avalonia native platform integration. |
| Avalonia.Remote.Protocol | 12.0.4 | MIT | Transitive Avalonia protocol package. |
| Avalonia.Skia | 12.0.4 | MIT | Transitive Avalonia rendering package. |
| Avalonia.Win32 | 12.0.4 | MIT | Windows platform backend included in Windows builds. |
| Avalonia.X11 | 12.0.4 | MIT | Transitive Avalonia Linux/X11 package; included by restore graph and relevant to non-Windows builds. |
| Avalonia.Angle.Windows.Natives | 2.1.27548.20260419 | BSD-style | ANGLE Windows native assets. See `LICENSES/ANGLE-BSD-3-Clause.txt`. |
| CommunityToolkit.Mvvm | 8.4.2 | MIT | MVVM helpers from .NET Community Toolkit. |
| HarfBuzzSharp | 8.3.1.3 | MIT | Text shaping bindings. |
| HarfBuzzSharp.NativeAssets.Win32 | 8.3.1.3 | MIT | Windows native HarfBuzz assets used by Windows builds. |
| MicroCom.Runtime | 0.11.4 | MIT | Transitive COM interop runtime used by Avalonia. |
| Microsoft.Data.Sqlite | 10.0.0 | MIT | SQLite ADO.NET provider. |
| Microsoft.Data.Sqlite.Core | 10.0.0 | MIT | Transitive core SQLite provider package. |
| Microsoft.Win32.SystemEvents | 10.0.0 | MIT | Windows system event integration. |
| SkiaSharp | 3.119.4 | MIT | Rendering library bindings. |
| SkiaSharp.NativeAssets.Win32 | 3.119.4 | MIT | Windows native SkiaSharp assets used by Windows builds. |
| SourceGear.sqlite3 | 3.50.4.5 | Public domain notice | Native SQLite build used by SQLitePCLRaw. See `LICENSES/SQLite-Public-Domain.txt`. |
| SQLitePCLRaw.bundle_e_sqlite3 | 3.0.3 | Apache-2.0 | SQLitePCLRaw bundle package. |
| SQLitePCLRaw.config.e_sqlite3 | 3.0.3 | Apache-2.0 | SQLitePCLRaw configuration package. |
| SQLitePCLRaw.core | 3.0.3 | Apache-2.0 | SQLitePCLRaw core package. |
| SQLitePCLRaw.provider.e_sqlite3 | 3.0.3 | Apache-2.0 | SQLitePCLRaw provider package. |
| Tmds.DBus.Protocol | 0.92.0 | MIT | Transitive DBus protocol package used by Avalonia platform support. |

## Development and test-only components

Development/test packages are documented separately in `docs/licensing/development-dependencies.md`. They are used by test projects or local tooling and are not part of normal end-user runtime distribution unless a test artifact is explicitly shipped.

## License texts included

- Project license: `LICENSE`, `COPYING`, and `LICENSES/GPL-3.0.txt`
- MIT terms: `LICENSES/MIT.txt`
- Apache License 2.0 terms: `LICENSES/Apache-2.0.txt`
- ANGLE BSD-style terms: `LICENSES/ANGLE-BSD-3-Clause.txt`
- SQLite public-domain notice: `LICENSES/SQLite-Public-Domain.txt`

## Release checklist

Before publishing a binary package:

1. Run `dotnet restore dotnet\Vehimap.sln` in a clean checkout.
2. Run `dotnet list dotnet\Vehimap.sln package --include-transitive`.
3. Publish the target runtime and inspect the publish output and `.deps.json`.
4. Update this file for packages, runtime packs, native assets, icons, fonts, sample data or documentation that are actually distributed.
5. Include `LICENSE`, `COPYING`, `COPYRIGHT-NOTICE.txt`, `THIRD-PARTY-NOTICES.md`, and `LICENSES/` in every binary release artifact.

This file is a practical compliance aid, not a legal audit.
