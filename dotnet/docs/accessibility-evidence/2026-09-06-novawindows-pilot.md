# NovaWindows Driver Pilot: 2026-09-06

## Scope And Environment

- Purpose: evaluate a maintained alternative to WinAppDriver without changing Vehimap UI or CI.
- Windows 11, .NET SDK 10.0.303 / runtime 10.0.11, desktop Avalonia 12.1.2.
- Appium 3.7.0; NovaWindows 1.4.5 installed using the official npm distribution.
- Existing Appium Windows driver 5.1.9 retained; no WinAppDriver MSI was installed.
- Target: local nightly `2.0.0-nightly.local.20260906070818` at
  `dotnet/artifacts/nightly/win-x64/app/Vehimap.exe`.
- Appium listened on `127.0.0.1:4725`, without relaxed security.
- `VEHIMAP_UI_REQUIRE_APPIUM=1` prevented silent test omission.
- Each attempt prepared its own synthetic fixture under `%TEMP%/vehimap-appium/<guid>/`.
  No installed-channel AppData or real vehicle data was used.

## Observations

1. Installation of NovaWindows 1.4.5 succeeded; Appium loaded it and `/status` reported ready.
2. The existing `Main_shell_exposes_visible_startup_controls_when_appium_is_available`
   test failed with a 90-second session-creation timeout. The log selected NovaWindows,
   created an internal session ID, then stopped during PowerShell initialization.
   Vehimap had not launched; startup focus and controls were not exercised.
3. A second attempt omitted optional `appWorkingDir` and `deviceName`. It failed at
   the same point after 90 seconds. Removing the working directory was not a fix;
   the shared harness retains it.
4. A harmless Node/Windows PowerShell transport probe reproduced a concrete blocker:
   after `$OutputEncoding = [Console]::OutputEncoding = [Text.Encoding]::UTF8`,
   `Write-Output ([char]0xF2EE)` emitted byte `3f` (`?`), but
   `[Console]::WriteLine([char]0xF2EE)` emitted UTF-8 `ef8bae`.
   Both encoding properties reported UTF-8. The driver waits for that marker in
   `build/lib/commands/powershell.js`; it has no bounded wait on that output listener.
5. The probe behaved the same with `-NoProfile`, with an explicit parent-process
   UTF-8 output encoding, and from a pseudo-terminal. This isolates the observed
   failure from Vehimap, but does not establish every cause of the host encoding behavior.

## Result And Boundaries

**Installation passed; live UI pilot failed before application launch.**
Menu, editor save/cancel, keyboard navigation and screen-reader behavior have no new
passing evidence from NovaWindows in this run. Previously reported manual desktop
checks remain separate evidence, not substitutes for this failed automated test.

No third-party files, PowerShell profiles, system locale, Developer Mode, security
settings, or product code were patched. The CI driver stays unchanged. NovaWindows
remains installed as an opt-in experiment, not a validated replacement. Locally started
Appium/PowerShell processes are stopped after the pilot.

Raw logs and failed TRX results are retained locally under
`dotnet/artifacts/novawindows-pilot-20260906/` (not included in releases or source commits).
Driver-selection regression tests are separate from live UI acceptance.

Repository verification: `dotnet test dotnet/Vehimap.sln --configuration Release`
passed 669 unit tests and 30 compatibility tests. The UI project reported 81 passing
methods: seven are real driver-configuration checks, while the 74 live cases returned
early with Appium unavailable in this separate default-configuration run. That total
must not be cited as 81 successful UI interactions. Both explicitly required
NovaWindows startup runs above failed.

Cleanup of the two failed synthetic launch directories was rejected by the tool's
execution policy; those disposable copies may remain under `%TEMP%/vehimap-appium/`.
The blocked deletion was not retried through another mechanism.

## Follow-up

The subsequent [WinAppDriver / NovaWindows 2 preview comparison](2026-09-06-windows-driver-comparison.md)
is recorded separately. It does not replace or turn the failed 1.4.5 attempts above
into passing evidence.

Prepare a minimal upstream report of the completion-marker transport problem before
adopting a backend workaround. Do not infer that a different issue about PowerShell
process exit is the same bug. No upstream issue has been submitted by this pilot.
Once resolved upstream or by a documented supported launch configuration, rerun
startup, menu, editor cancel/save and focus tests with required Appium availability,
then decide whether CI should switch.

The user also identified Appium Windows Driver **6.1.1**. Its versioned README still
describes a proxy to Microsoft's WinAppDriver; upgrading that wrapper does not replace
the old server. Separately, NovaWindows **2.0.0-preview.2** is published under npm's
`develop` tag and uses a native C# UIA3 backend rather than the PowerShell transport.
It is a reasonable next isolated pilot, but was not installed or tested in this run.

Sources:
- [Appium Windows driver maintenance warning](https://github.com/appium/appium-windows-driver#readme)
- [NovaWindows installation and capabilities](https://github.com/AutomateThePlanet/appium-novawindows-driver#readme)
- [Appium Windows driver 6.1.1](https://github.com/appium/appium-windows-driver/blob/v6.1.1/README.md)
- [NovaWindows 2.0 preview backend](https://github.com/AutomateThePlanet/appium-novawindows-driver/blob/v2.0.0-preview.2/README.md)
